using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour {
    #region Variables
    [SerializeField] private GameObject _prefabSquare;
    [SerializeField] private GameObject _prefabPiece;

    private BoardSquareModel[,] _squares;

    private BoardState _state = BoardState.Default;
    private BoopVector _selectedSquare = null;
    private List<BoopVector> _alignedSquares = new List<BoopVector>();

    private BoardModel _model => GlobalManager.Instance.BoardModel;
    #endregion


    public void Init() {
        _model.onBoop += Boop;
        _model.onWin += Win;

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

        _model.EvaluateAlignment(_selectedSquare, v, out List<BoopVector> selectedSquares);

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

        foreach (BoopVector pos in selectedSquares) {
            BoardSquareModel sm = _squares[pos.x, pos.y];
            sm.square.SetBaseColor(Color.white);
            sm.square.FlashColor(Color.green);
            RemovePiece(pos);
        }

        _state = BoardState.Waiting;
    }


    public void RemovePiece(BoopVector v) {
        BoardSquareModel square = _squares[v.x, v.y];

        if (square.piece == null)
            return;

        square.piece.Delete(() => { square.piece = null; });
    }

    private void Win(List<BoopVector> aligned, int playerIndex) {
        Utils.Log(this, "Win", $"Player {playerIndex} won");

        //foreach (BoopVector pos in aligned)
        //    _squares[pos.x, pos.y].square.SetBaseColor(Color.yellow);
    }

    private void Boop(BoopVector origin, BoopVector destination) {
        BoardSquareModel ori = _squares[origin.x, origin.y];

        if (destination.x >= 0 && destination.x < _model.Size && destination.y >= 0 && destination.y < _model.Size) {
            BoardSquareModel desti = _squares[destination.x, destination.y];

            Transform t = ori.piece.transform;
            t.parent = desti.square.transform;
            t.transform.DOLocalMove(Vector3.zero, 0.2f);

            desti.piece = ori.piece;
            ori.piece = null;
        }
        else {
            BoopVector direction = destination - origin;
            Transform t = ori.piece.transform;
            t.parent = null;
            t.DOMove(t.transform.position + new Vector3(direction.x, 0, direction.y), 0.2f);
            RemovePiece(origin);
        }
    }

    private void AddPiece(BoopVector v, int pieceValue) {
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

        _alignedSquares = pos;

        foreach (BoopVector p in pos)
            _squares[p.x, p.y].square.SetBaseColor(Color.yellow);
    }

    public void AddPiece(string[] infos) {
        BoopVector v = BoopVector.FromString(infos[0]);
        int pieceValue = int.Parse(infos[1]);
        _model.AddPieceOnBoard(v, pieceValue);
        AddPiece(v, pieceValue);
    }

    public void SelectPieces(string[] infos) {
        _model.EvaluateAlignment(BoopVector.FromString(infos[0]), BoopVector.FromString(infos[2]), out List<BoopVector> selectedSquares);

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
}
