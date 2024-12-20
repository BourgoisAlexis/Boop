using DG.Tweening;
using PlayerIOClient;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ControllerBoard : SceneManager {
    #region Variables
    [SerializeField] private GameObject _prefabSquare;
    [SerializeField] private GameObject _prefabPiece;
    [SerializeField] private GameObject _prefabPop;
    [SerializeField] private UIViewGameplay _gameplayView;

    private BoardSquareModel[,] _squares;
    private BoardModel _model;

    private BoardState _state = BoardState.Default;
    private BoopVector _selectedSquare = null;
    private List<BoopVector> _alignedSquares = new List<BoopVector>();
    private bool _hasAlreadySelectedLine = false;

    //Accessors
    public BoardModel Model => _model;
    #endregion


    public override void Init(params object[] parameters) {
        _model = new BoardModel();
        _model.Init();
        _model.onBoop += Boop;
        _model.onNoPiecesLeft += (bool large) => { GlobalManager.Instance.UINotificationManager.Show($"You have no {(large ? "large" : "small")} pieces left"); };

        _viewManager.Init(_model, GlobalManager.Instance.RoomModel);
        _texturedCanvas.Init();
        GetComponent<InputManager>().Init();

        GlobalManager.Instance.PoolManager.CreatePool(AppConst.squareKey, _prefabSquare);
        GlobalManager.Instance.PoolManager.CreatePool(AppConst.pieceKey, _prefabPiece);
        GlobalManager.Instance.PoolManager.CreatePool(AppConst.popKey, _prefabPop);

        PlayerIOManager playerIO = GlobalManager.Instance.PlayerIOManager;
        CommonConst commonConst = GlobalManager.Instance.CommonConst;
        playerIO.HandleMessage(commonConst.serverMessageAddPiece, AddPiece, 2);
        playerIO.HandleMessage(commonConst.serverMessageAlignedPieces, AlignedPieces);
        playerIO.HandleMessage(commonConst.serverMessageSelectPieces, SelectPieces, 3);
        playerIO.HandleMessage(commonConst.serverMessageWin, Win, 1);
        playerIO.HandleMessage(commonConst.serverMessageNextTurn, NextTurn, 2);
        playerIO.HandleMessage(commonConst.serverMessagePlayerLeaveRoom, PlayerLeft);

        BoardSpawn();

        GlobalManager.Instance.Loading.Load(false);
        _hasAlreadySelectedLine = false;
    }

    private void OnDestroy() {
        PlayerIOManager playerIO = GlobalManager.Instance.PlayerIOManager;
        CommonConst commonConst = GlobalManager.Instance.CommonConst;
        playerIO.UnhandleMessage(commonConst.serverMessageAddPiece, AddPiece);
        playerIO.UnhandleMessage(commonConst.serverMessageAlignedPieces, AlignedPieces);
        playerIO.UnhandleMessage(commonConst.serverMessageSelectPieces, SelectPieces);
        playerIO.UnhandleMessage(commonConst.serverMessageWin, Win);
        playerIO.UnhandleMessage(commonConst.serverMessageNextTurn, NextTurn);
        playerIO.UnhandleMessage(commonConst.serverMessagePlayerLeaveRoom, PlayerLeft);
    }

    private void BoardSpawn() {
        _squares = new BoardSquareModel[_model.Size, _model.Size];

        Vector3 start = new Vector3(-_model.Size / 2, 0, -_model.Size / 2) + new Vector3(0.5f, 0, 0.5f);

        for (int x = 0; x < _model.Size; x++) {
            for (int y = 0; y < _model.Size; y++) {
                GameObject instantiated = GlobalManager.Instance.PoolManager.Dequeue(AppConst.squareKey, transform);
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

        if (_model.GameState == GameState.Ended)
            return;

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
        int pieceValue = _model.AddPieceOnBoard(v, large, Model.CurrentPlayerValue);

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
            GlobalManager.Instance.SFXManager.PlayAudio(7);
            _squares[v.x, v.y].square.SetBaseColor(AppConst.green);
            if (!_hasAlreadySelectedLine)
                GlobalManager.Instance.UINotificationManager.Show("Select the ending tile of the line");
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
            GlobalManager.Instance.SFXManager.PlayAudio(6);
            foreach (BoopVector p in _alignedSquares)
                _squares[p.x, p.y].square.SetBaseColor(AppConst.yellow);
            if (!_hasAlreadySelectedLine)
                GlobalManager.Instance.UINotificationManager.Show("Select the starting tile of the line");
            return;
        }

        PiecesSelected(selectedSquares);

        foreach (BoopVector p in _alignedSquares.FindAll(x => selectedSquares.Contains(x) == false))
            _squares[p.x, p.y].square.SetBaseColor(AppConst.GetColor(ColorVariant.Light, GlobalManager.Instance.PlayerValue));

        _state = BoardState.Waiting;
        _hasAlreadySelectedLine = true;
    }


    private void AddPiece(BoopVector v, int pieceValue) {
        if (v == null) {
            Utils.LogError(this, "AddPiece", "v is null");
            return;
        }

        BoardSquareModel square = _squares[v.x, v.y];

        GameObject instantiated = GlobalManager.Instance.PoolManager.Dequeue(AppConst.pieceKey, square.square.transform);
        instantiated.transform.localPosition = Vector3.zero;
        BoardPiece piece = instantiated.GetComponent<BoardPiece>();
        piece.Init(pieceValue);
        square.square.Bump();

        GlobalManager.Instance.SFXManager.PlayAudio(Math.Abs(pieceValue) > 1 ? 4 : 11);

        square.piece = piece;

        _model.Simulate(v, out List<BoopVector>[] alignedPerPlayer);
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


    //From server notice
    public void AlignedPieces(string[] infos) {
        if (!_hasAlreadySelectedLine)
            GlobalManager.Instance.UINotificationManager.Show("Select the starting tile of the line");

        _state = BoardState.Selecting;

        List<BoopVector> pos = new List<BoopVector>();
        foreach (string info in infos)
            pos.Add(BoopVector.FromString(info));

        foreach (BoopVector p in pos)
            _squares[p.x, p.y].square.SetBaseColor(AppConst.yellow);

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
        PiecesSelected(selectedSquares);
    }

    private void PiecesSelected(List<BoopVector> selectedSquares) {
        GlobalManager.Instance.SFXManager.PlayAudio(10);

        foreach (BoopVector pos in selectedSquares) {
            BoardSquareModel sm = _squares[pos.x, pos.y];
            sm.square.SetBaseColor(AppConst.GetColor(ColorVariant.Light, GlobalManager.Instance.PlayerValue));
            sm.square.FlashColor(AppConst.green);
            RemovePiece(pos);
        }
    }

    private void NextTurn(string[] infos) {
        if (int.TryParse(infos[0], out int currentPlayerIndex) == false) {
            Utils.LogError(this, "NextTurn", "can't parse infos[0]");
            return;
        }

        _model.NextTurn(currentPlayerIndex);
        _gameplayView.SetCurrentPlayer(_model.CurrentPlayerIndex);

        if (GlobalManager.Instance.PlayerIndex != _model.CurrentPlayerIndex)
            return;

        _state = BoardState.Placing;
        _selectedSquare = null;
        _alignedSquares.Clear();
    }

    private void Win(string[] infos) {
        if (int.TryParse(infos[0], out int playerIndex) == false) {
            Utils.LogError(this, "Win", "can't parse infos[0]");
            return;
        }

        GlobalManager gm = GlobalManager.Instance;
        gm.PlayerIOManager.UnhandleMessage(gm.CommonConst.serverMessagePlayerLeaveRoom, PlayerLeft);

        _viewManager.ShowView(1, playerIndex, infos.Length > 1);
    }

    private void PlayerLeft(string[] infos) {
        GlobalManager.Instance.UINotificationManager.Show("Your opponent left");

        string[] parameters = {
            GlobalManager.Instance.PlayerIndex.ToString(),
            GlobalManager.Instance.CommonConst.serverMessagePlayerLeaveRoom
        };

        Win(parameters);
    }
}
