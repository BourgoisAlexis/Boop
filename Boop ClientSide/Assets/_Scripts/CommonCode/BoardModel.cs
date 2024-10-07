using System;
using System.Collections.Generic;
using System.Linq;

public class BoardModel {
    #region Variables
    public Action<BoopVector, BoopVector> onBoop;
    public Action<List<BoopVector>, int> onWin;
    public Action<PlayerModel[]> onPlayerModelsUpdate;

    private int _boardSize = 6;
    private int _maxPieceNumber = 8;
    private int[,] _board;

    private PlayerModel[] _playerModels;
    private GameState _gameState;

    public int Size => _boardSize;
    public int[,] Board => _board;
    #endregion


    public void Init() {
        _board = new int[_boardSize, _boardSize];
        _playerModels = new PlayerModel[] {
            new PlayerModel(_maxPieceNumber),
            new PlayerModel(_maxPieceNumber)
        };
        _gameState = GameState.Gameplay;
    }

    public int AddPieceOnBoard(BoopVector v, int pieceValue) {
        return AddPieceOnBoard(v, Math.Abs(pieceValue) == 2, pieceValue < 0 ? -1 : 1);
    }

    public int AddPieceOnBoard(BoopVector v, bool large, int playerValue) {
        if (_board[v.x, v.y] != 0) {
            Utils.LogError(this, "AddPieceOnBoard", $"{v.x} {v.y} is already occupied");
            return 0;
        }

        int pieceValue = (large ? 2 : 1) * playerValue;

        int playerIndex = playerValue < 0 ? 0 : 1;
        int pieceIndex = Math.Abs(pieceValue) - 1;

        if (_playerModels[playerIndex].pieces[pieceIndex] < 1) {
            Utils.LogError(this, "AddPieceOnBoard", $"player {playerIndex} has no piece of type {(large ? "large" : "normal")}");
            return 0;
        }

        _board[v.x, v.y] = pieceValue;
        _playerModels[playerIndex].pieces[pieceIndex]--;

        onPlayerModelsUpdate?.Invoke(_playerModels);

        return pieceValue;
    }

    public void RemovePieceFromBoard(BoopVector v, bool upgrade = false) {
        int pieceValue = _board[v.x, v.y];

        if (pieceValue == 0)
            return;

        int playerIndex = pieceValue < 0 ? 0 : 1;
        int pieceIndex = Math.Abs(pieceValue) - 1;

        _playerModels[playerIndex].pieces[pieceIndex + (upgrade ? 1 : 0)]++;
        _board[v.x, v.y] = 0;

        onPlayerModelsUpdate?.Invoke(_playerModels);
    }


    public BoopVector[] GetPossibleDirections(BoopVector v) {
        List<BoopVector> directions = new List<BoopVector>();
        List<int> ys = new List<int>();
        List<int> xs = new List<int>();

        if (v.x > 0)
            xs.Add(-1);

        xs.Add(0);

        if (v.x < _boardSize - 1)
            xs.Add(1);

        if (v.y < _boardSize - 1)
            ys.Add(1);

        ys.Add(0);

        if (v.y > 0)
            ys.Add(-1);

        foreach (int i in xs)
            foreach (int j in ys)
                if (i != 0 || j != 0)
                    directions.Add(new BoopVector(i, j));

        return directions.ToArray();
    }

    public void Simulate(BoopVector v, out List<BoopVector>[] alignedPerPlayer) {
        List<BoopVector> aligned = new List<BoopVector>();
        BoopVector[] directions = GetPossibleDirections(v);
        List<BoopVector> modified = new List<BoopVector>() { v };

        foreach (BoopVector direction in directions) {
            BoopVector adjacentPos = v + direction;

            int adjacentValue = _board[adjacentPos.x, adjacentPos.y];

            if (adjacentValue != 0 && Math.Abs(adjacentValue) <= Math.Abs(_board[v.x, v.y]))
                Boop(adjacentPos, direction, ref modified);
        }

        CheckForAlignment(modified, ref aligned);
        aligned = aligned.Distinct(new VectorComparer()).ToList();

        alignedPerPlayer = new List<BoopVector>[] {
            aligned.FindAll(x => _board[x.x, x.y] < 0),
            aligned.FindAll(x => _board[x.x, x.y] > 0)
        };
    }

    private void Boop(BoopVector v, BoopVector direction, ref List<BoopVector> modified) {
        BoopVector destination = v + direction;

        if (destination.x >= 0 && destination.x < _boardSize && destination.y >= 0 && destination.y < _boardSize) {
            if (_board[destination.x, destination.y] == 0) {
                _board[destination.x, destination.y] = _board[v.x, v.y];
                modified.Add(destination);
                _board[v.x, v.y] = 0;
                onBoop?.Invoke(v, destination);
            }
        }
        else {
            RemovePieceFromBoard(v);
            onBoop?.Invoke(v, destination);
        }
    }

    public void CheckForAlignment(List<BoopVector> modified, ref List<BoopVector> aligned) {
        foreach (BoopVector v in modified) {
            //Column
            Straight(v, 0, 1, ref aligned);

            ////Line
            Straight(v, 1, 0, ref aligned);

            ////Diag
            Diagonal(v, 1, 1, ref aligned);

            ////AntiDiag
            Diagonal(v, -1, 1, ref aligned);

            if (_gameState == GameState.Ended)
                break;
        }
    }

    private void Straight(BoopVector startingPos, int iterrateX, int iterrateY, ref List<BoopVector> aligned) {
        List<BoopVector> possiblePos = new List<BoopVector>();

        for (int i = 0; i < _boardSize; i++) {
            int x = iterrateX != 0 ? iterrateX * i : startingPos.x;
            int y = iterrateY != 0 ? iterrateY * i : startingPos.y;
            possiblePos.Add(new BoopVector(x, y));
        }

        possiblePos = possiblePos.Distinct(new VectorComparer()).ToList();
        EvaluateAlignment(possiblePos, ref aligned);
    }

    private void Diagonal(BoopVector startingPos, int iterrateX, int iterrateY, ref List<BoopVector> aligned) {
        List<BoopVector> possiblePos = new List<BoopVector>();
        int x = 0;
        int y = 0;

        while (startingPos.x + x < _boardSize && startingPos.y + y < _boardSize && startingPos.x + x >= 0 && startingPos.y + y >= 0) {
            possiblePos.Add(new BoopVector(startingPos.x + x, startingPos.y + y));
            x += iterrateX;
            y += iterrateY;
        }

        possiblePos.Reverse();
        x = 0;
        y = 0;

        while (startingPos.x + x < _boardSize && startingPos.y + y < _boardSize && startingPos.x + x >= 0 && startingPos.y + y >= 0) {
            possiblePos.Add(new BoopVector(startingPos.x + x, startingPos.y + y));
            x -= iterrateX;
            y -= iterrateY;
        }

        possiblePos = possiblePos.Distinct(new VectorComparer()).ToList();
        EvaluateAlignment(possiblePos, ref aligned);
    }

    private void EvaluateAlignment(List<BoopVector> positions, ref List<BoopVector> aligned) {
        int sign = 0;
        List<BoopVector> validPos = new List<BoopVector>();

        foreach (BoopVector v in positions) {
            int newSign = Math.Sign(_board[v.x, v.y]);

            if (newSign == 0)
                validPos.Clear();
            else if (sign == 0 && newSign != 0)
                validPos.Add(v);
            else if (sign != 0 && newSign == sign)
                validPos.Add(v);

            sign = newSign;

            if (validPos.Count >= 3) {
                int largePieceCount = 0;
                foreach (BoopVector pos in validPos)
                    if (IsLargePiece(pos))
                        largePieceCount++;

                if (largePieceCount >= 3) {
                    onWin?.Invoke(validPos, sign);
                    _gameState = GameState.Ended;
                    return;
                }

                aligned.AddRange(validPos);
            }
        }
    }

    public void EvaluateAlignment(BoopVector start, BoopVector end, out List<BoopVector> selectedSquares) {
        Utils.Log(this, "EvaluateAlignment", $"Evaluate from {start.ToString()} to {end.ToString()}");

        selectedSquares = new List<BoopVector>();
        BoopVector direction = end - start;
        BoopVector absDir = new BoopVector(Math.Abs(direction.x), Math.Abs(direction.y));

        if (absDir.x != 2 && absDir.y != 2)
            return;

        if (absDir.x == 2)
            if (absDir.y != 0 && absDir.y != 2)
                return;

        if (absDir.y == 2)
            if (absDir.x != 0 && absDir.x != 2)
                return;

        direction = new BoopVector(Math.Sign(direction.x), Math.Sign(direction.y));
        selectedSquares.Add(start);
        selectedSquares.Add(start + direction);
        selectedSquares.Add(end);

        int sign = Math.Sign(_board[start.x, start.y]);
        for (int i = 1; i < selectedSquares.Count; i++) {
            if (Math.Sign(_board[start.x, start.y]) == sign)
                continue;

            return;
        }

        foreach (BoopVector v in selectedSquares)
            RemovePieceFromBoard(v, !IsLargePiece(v));
    }

    public bool IsLargePiece(BoopVector v) {
        return Math.Abs(_board[v.x, v.y]) == 2;
    }
}
