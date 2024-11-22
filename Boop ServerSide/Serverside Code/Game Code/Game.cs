using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    [RoomType("Standard")]
    public class GameCode : Game<Player> {
        #region Variables
        private CommonConst _commonConst = new CommonConst();
        private BoardModel _model;
        private Random _random = new Random();
        private int _numberOfPlayers;

        //Gameplay
        public MessageWaiting _messageWaiting;
        #endregion


        #region Player.IO Methods
        public override void GameStarted() {
            if (RoomData[_commonConst.gameVersionKey] != _commonConst.version) {
                foreach (Player p in Players)
                    p.Disconnect();

                return;
            }

            _numberOfPlayers = int.Parse(RoomData[_commonConst.numberOfPlayerKey]);
            Utils.Log(this, $"GameStarted : Room start : {RoomId} {_numberOfPlayers}");
        }

        public override void GameClosed() {
            Utils.Log(this, $"GameClosed : Room close : {RoomId}");
        }

        public override void UserJoined(Player player) {
            if (player.JoinData[_commonConst.gameVersionKey] != _commonConst.version) {
                player.Send(_commonConst.serverMessageError, "Wrong version of the game");
                player.Disconnect();
                return;
            }

            //TODO : checker les playerJoinData pour la version du jeu
            player.Send(_commonConst.serverMessageJoin, (player.Id - 1).ToString());

            string[] userNames = Players.Select(x => x.ConnectUserId).ToArray();
            StringBuilder sb = new StringBuilder();
            foreach (string name in userNames) {
                sb.Append(name);
                if (name != userNames.Last())
                    sb.Append(_commonConst.separator);
            }

            Broadcast(_commonConst.serverMessagePlayerJoinRoom, RoomId, sb.ToString());

            if (PlayerCount == _numberOfPlayers) {
                Broadcast(_commonConst.serverMessageLoadScene, "1");
                _messageWaiting = new MessageWaiting(_commonConst.userMessageSceneLoaded, _numberOfPlayers, GameInit);
            }
        }

        public override void UserLeft(Player player) {
            Broadcast(_commonConst.serverMessagePlayerLeaveRoom);
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

                case "usermessage_quit":
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
        //Send message to every players except the one mentioned
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

        private int GetPlayerIndex(Player player) {
            int index = 0;

            foreach (Player p in Players) {
                if (p.Id == player.Id)
                    break;

                index++;
            }

            return index;
        }

        private void PlayAgain() {
            Broadcast(_commonConst.serverMessageLoadScene, 1.ToString());
            _messageWaiting = new MessageWaiting(_commonConst.userMessageSceneLoaded, _numberOfPlayers, GameInit);
        }

        private void GameInit() {
            if (_model == null) {
                _model = new BoardModel();
                _model.onWin += Win;
            }

            _model.Init();

            _model.NextTurn(_random.NextDouble() > 0.5f ? 0 : 1);
            NextTurn();
        }

        private void AddPiece(Player player, Message m) {
            BoopVector v = BoopVector.FromString(m.GetString(0));
            int pieceValue = m.GetInt(1);
            bool large = Math.Abs(pieceValue) == 2;
            int actualValue = _model.AddPieceOnBoard(v, large, _model.CurrentPlayerValue);

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

                        foreach (BoopVector bv in aligned)
                            Utils.Log("Game", "AddPiece", bv.ToString());

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
            if (_model.GameState == GameState.Ended)
                return;

            int value = _model.CurrentPlayerIndex + 1;
            if (value > Players.Count() - 1)
                value = 0;

            _model.NextTurn(value);

            Broadcast(_commonConst.serverMessageNextTurn, _model.CurrentPlayerIndex.ToString(), CommonUtils.BoardState(_model.Board));
        }

        private void Win(List<BoopVector> aligned, int playerIndex) {
            if (aligned == null || aligned.Count < 3) {
                CommonUtils.ErrorOnParams("Game", "Win");
                return;
            }

            Utils.Log(this, "Win", $"Player {playerIndex} won");
            Broadcast(_commonConst.serverMessageWin, playerIndex.ToString());
            _messageWaiting = new MessageWaiting(_commonConst.userMessagePlayAgain, _numberOfPlayers, PlayAgain);
        }
        #endregion
    }
}