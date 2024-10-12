using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class ControllerBoard : SceneManager {
    #region Variables
    [SerializeField] private GameObject _prefabSquare;
    [SerializeField] private GameObject _prefabPiece;

    private BoardSquareModel[,] _squares;
    private BoardModel _model;

    private BoardState _state = BoardState.Default;
    private BoopVector _selectedSquare = null;
    private List<BoopVector> _alignedSquares = new List<BoopVector>();

    //Accessors
    public BoardModel Model => _model;
    #endregion


    public override void Init(params object[] parameters) {
        _model = new BoardModel();
        _model.Init();
        _model.onBoop += Boop;

        GetComponent<InputManager>().Init();
        GetComponent<UIViewBoard>().Init(_model);

        PlayerIOManager playerIO = GlobalManager.Instance.PlayerIOManager;
        CommonConst commonConst = GlobalManager.Instance.CommonConst;
        playerIO.HandleMessage(commonConst.serverMessageAddPiece, AddPiece, 2);
        playerIO.HandleMessage(commonConst.serverMessageAlignedPieces, AlignedPieces);
        playerIO.HandleMessage(commonConst.serverMessageSelectPieces, SelectPieces, 3);
        playerIO.HandleMessage(commonConst.serverMessageWin, Win, 1);

        BoardSpawn();
    }

    private void BoardSpawn() {
        _squares = new BoardSquareModel[_model.Size, _model.Size];

        Vector3 start = new Vector3(-_model.Size / 2, 0, -_model.Size / 2) + new Vector3(0.5f, 0, 0.5f);

        for (int x = 0; x < _model.Size; x++) {
            for (int y = 0; y < _model.Size; y++) {
                GameObject instantiated = Instantiate(_prefabSquare);
                instantiated.transform.parent = transform;
                instantiated.transform.position = start + new Vector3(x, 0, y);
                BoardSquare square = instantiated.GetComponent<BoardSquare>();
                square.Init(x, y);

                _squares[x, y] = new BoardSquareModel(square, null);
            }
        }
    }


    //Actions
    public void Click(BoopVector v, bool rightClick) {
        if (v == null) {
            Utils.LogError(this, "Click", "v is null");
            return;
        }

        _squares[v.x, v.y].square.Click();

        switch (_state) {
            case BoardState.Placing:
                Place(v, rightClick);
                break;

            case BoardState.Selecting:
                if (rightClick)
                    break;
                Select(v);
                break;
        }
    }

    private void Place(BoopVector v, bool large) {
        int pieceValue = _model.AddPieceOnBoard(v, large, GlobalManager.Instance.currentPlayerValue);

        if (pieceValue == 0)
            return;

        GlobalManager.Instance.PlayerIOManager.SendMessage(
            GlobalManager.Instance.CommonConst.userMessageAddPiece,
            v.ToString(),
            pieceValue
        );

        AddPiece(v, pieceValue);

        _state = BoardState.Waiting;
    }

    private void Select(BoopVector v) {
        if (!_alignedSquares.Contains(v))
            return;

        if (_selectedSquare == null) {
            _selectedSquare = v;
            return;
        }

        if (v.Equals(_selectedSquare))
            return;

        List<BoopVector> selectedSquares = _model.EvaluateAlignmentFromTo(_selectedSquare, v);

        if (selectedSquares != null && selectedSquares.Count == 3) {
            string[] pos = new string[] {
                selectedSquares[0].ToString(),
                selectedSquares[1].ToString(),
                selectedSquares[2].ToString()
            };

            GlobalManager.Instance.PlayerIOManager.SendMessage(
                GlobalManager.Instance.CommonConst.userMessageSelectPieces,
                pos
            );
        }
        else {
            _selectedSquare = null;
            return;
        }

        foreach (BoopVector pos in _alignedSquares) {
            BoardSquareModel sm = _squares[pos.x, pos.y];
            sm.square.SetBaseColor(Color.white);
            if (selectedSquares.Contains(pos)) {
                sm.square.FlashColor(Color.green);
                RemovePiece(pos);
            }
        }

        _state = BoardState.Waiting;
    }


    public void RemovePiece(BoopVector v) {
        if (v == null) {
            Utils.LogError(this, "Remove", "v is null");
            return;
        }

        BoardSquareModel square = _squares[v.x, v.y];

        if (square.piece == null) {
            Utils.LogError(this, "Remove", "square.piece is null");
            return;
        }

        square.piece.Delete(() => { square.piece = null; });
    }

    private void Boop(BoopVector origin, BoopVector destination) {
        if (origin == null || destination == null) {
            Utils.LogError(this, "Boop", "one of the vectors is null");
            return;
        }

        BoardSquareModel originSquare = _squares[origin.x, origin.y];

        if (destination.x >= 0 && destination.x < _model.Size && destination.y >= 0 && destination.y < _model.Size) {
            BoardSquareModel destinationSquare = _squares[destination.x, destination.y];

            Transform t = originSquare.piece.transform;
            t.parent = destinationSquare.square.transform;
            t.transform.DOLocalMove(Vector3.zero, 0.2f);

            destinationSquare.piece = originSquare.piece;
            originSquare.piece = null;
        }
        else {
            BoopVector direction = destination - origin;
            Transform t = originSquare.piece.transform;
            t.parent = null;
            t.DOMove(t.transform.position + new Vector3(direction.x, 0, direction.y), 0.2f);
            RemovePiece(origin);
        }
    }

    private void AddPiece(BoopVector v, int pieceValue) {
        if (v == null) {
            Utils.LogError(this, "AddPiece", "v is null");
            return;
        }

        BoardSquareModel square = _squares[v.x, v.y];

        GameObject instantiated = Instantiate(_prefabPiece, square.square.transform);
        instantiated.transform.localPosition = Vector3.zero;
        BoardPiece piece = instantiated.GetComponent<BoardPiece>();
        piece.Init(pieceValue);

        square.piece = piece;

        _model.Simulate(v, out List<BoopVector>[] alignedPerPlayer);
    }


    //From server notice
    public void AlignedPieces(string[] infos) {
        _state = BoardState.Selecting;

        List<BoopVector> pos = new List<BoopVector>();
        foreach (string info in infos)
            pos.Add(BoopVector.FromString(info));

        foreach (BoopVector p in pos)
            _squares[p.x, p.y].square.SetBaseColor(Color.yellow);

        _alignedSquares = pos;
    }

    public void AddPiece(string[] infos) {
        BoopVector v = BoopVector.FromString(infos[0]);
        int pieceValue = int.Parse(infos[1]);
        _model.AddPieceOnBoard(v, pieceValue);
        AddPiece(v, pieceValue);
    }

    public void SelectPieces(string[] infos) {
        List<BoopVector> selectedSquares = _model.EvaluateAlignmentFromTo(BoopVector.FromString(infos[0]), BoopVector.FromString(infos[2]));

        foreach (BoopVector pos in selectedSquares) {
            BoardSquareModel sm = _squares[pos.x, pos.y];
            sm.square.SetBaseColor(Color.white);
            sm.square.FlashColor(Color.green);
            RemovePiece(pos);
        }
    }

    public void NextTurn() {
        _state = BoardState.Placing;
        _selectedSquare = null;
        _alignedSquares.Clear();
    }

    public void Win(string[] infos) {

    }
}
