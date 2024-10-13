using System;
using System.Collections.Generic;
using System.Linq;
using PlayerIO.GameLibrary;

namespace Boop {
    public class MessageWaiting {
        public string messageType;
        public int messageNumber;
        public Action onSuccess;

        public MessageWaiting(string messageType, int messageNumber, Action onSuccess) {
            this.messageType = messageType;
            this.messageNumber = messageNumber;
            this.onSuccess = onSuccess;
        }
    }

    public class Player : BasePlayer {
    }

    [RoomType("Lobby")]
    public class GameCode : Game<Player> {
        #region Variables
        private CommonConst _commonConst = new CommonConst();
        private BoardModel _model;
        private Random _random = new Random();

        //Gameplay
        public int _currentPlayerIndex;
        public MessageWaiting _messageWaiting;
        public int _currentPlayerValue => _currentPlayerIndex == 0 ? -1 : 1;
        #endregion


        #region Player.IO Methods
        public override void GameStarted() {
            Utils.Log(this, $"GameStarted : Room start : {RoomId}");
            _model = new BoardModel();
            _model.Init();
            _model.onWin += Win;
        }

        public override void GameClosed() {
            Utils.Log(this, $"GameClosed : Room close : {RoomId}");
        }

        public override void UserJoined(Player player) {
            //TODO : checker les playerJoinData pour la version du jeu
            player.Send(_commonConst.serverMessageJoin, (player.Id - 1).ToString());

            int playerN = 1;

            if (PlayerCount == playerN) {
                Broadcast(_commonConst.serverMessageLoadScene, 1.ToString());
                _messageWaiting = new MessageWaiting(_commonConst.userMessageSceneLoaded, playerN, GameInit);
            }
        }

        public override void UserLeft(Player player) {

        }

        // This method is called when a player sends a message into the server code
        public override void GotMessage(Player player, Message m) {
            CommonUtils.LogMessage(m);

            switch (m.Type) {
                case "usermessage_addpiece":
                    AddPiece(player, m);
                    break;

                case "usermessage_selectpieces":
                    SelectPieces(player, m);
                    break;
            }

            if (_messageWaiting != null && _messageWaiting.messageNumber > 0) {
                if (m.Type == _messageWaiting.messageType) {
                    _messageWaiting.messageNumber--;
                    if (_messageWaiting.messageNumber == 0)
                        _messageWaiting.onSuccess?.Invoke();
                }
            }
        }
        #endregion

        #region Custom Methods
        private void SpreadMessage(string messageType, Player sender, Message originalMessage) {
            List<object> args = new List<object>();

            for (int i = 0; i < originalMessage.Count; i++)
                args.Add(originalMessage[(uint)i]);

            foreach (Player p in Players) {
                if (p.Id == sender.Id)
                    continue;

                p.Send(messageType, args.ToArray());
            }
        }

        private Player GetPlayerFromID(string id) {
            foreach (Player p in Players)
                if (p.Id == int.Parse(id))
                    return p;

            return null;
        }

        private void GameInit() {
            _currentPlayerIndex = _random.NextDouble() > 0.5f ? 0 : 1;
            NextTurn();
        }

        private void AddPiece(Player player, Message m) {
            BoopVector v = BoopVector.FromString(m.GetString(0));
            int pieceValue = m.GetInt(1);
            bool large = Math.Abs(pieceValue) == 2;
            int actualValue = _model.AddPieceOnBoard(v, large, _currentPlayerValue);

            if (actualValue != pieceValue) {
                Utils.LogError(this, "AddPiece", "received value is different from value");
                return;
            }

            SpreadMessage(_commonConst.serverMessageAddPiece, player, m);

            _model.Simulate(v, out List<BoopVector>[] alignedPerPlayer);

            if (alignedPerPlayer != null && alignedPerPlayer.Length > 0) {
                if (alignedPerPlayer[0].Count == 0 && alignedPerPlayer[1].Count == 0) {
                    NextTurn();
                    return;
                }

                _messageWaiting = new MessageWaiting(_commonConst.userMessageSelectPieces, 0, NextTurn);

                for (int i = 0; i < alignedPerPlayer.Length; i++) {
                    List<BoopVector> aligned = alignedPerPlayer[i];

                    if (aligned != null && aligned.Count > 1) {
                        string[] infos = new string[aligned.Count];
                        for (int j = 0; j < aligned.Count; j++)
                            infos[j] = aligned[j].ToString();

                        GetPlayerFromID((i + 1).ToString()).Send(_commonConst.serverMessageAlignedPieces, infos);
                        _messageWaiting.messageNumber++;
                    }
                }
            }
            else {
                NextTurn();
            }
        }

        private void SelectPieces(Player player, Message m) {
            string[] infos = CommonUtils.GetMessageParams(m);
            List<BoopVector> selectedSquares = _model.EvaluateAlignmentFromTo(BoopVector.FromString(infos[0]), BoopVector.FromString(infos[2]));

            if (selectedSquares != null && selectedSquares.Count == 3)
                SpreadMessage(_commonConst.serverMessageSelectPieces, player, m);
        }

        private void NextTurn() {
            _currentPlayerIndex++;
            if (_currentPlayerIndex > Players.Count() - 1)
                _currentPlayerIndex = 0;

            Broadcast(_commonConst.serverMessageNextTurn, _currentPlayerIndex.ToString(), CommonUtils.BoardState(_model.Board));
        }

        private void Win(List<BoopVector> aligned, int playerIndex) {
            if (aligned == null || aligned.Count < 3) {
                CommonUtils.ErrorOnParams("Game", "Win");
                return;
            }

            Utils.Log(this, "Win", $"Player {playerIndex} won");
            Broadcast(_commonConst.serverMessageWin, playerIndex.ToString());
        }
        #endregion
    }
}