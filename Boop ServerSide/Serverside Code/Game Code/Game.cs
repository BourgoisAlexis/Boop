using System;
using System.Collections.Generic;
using System.Linq;
using PlayerIO.GameLibrary;

namespace Boop {
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
        public int _currentPlayerValue => _currentPlayerIndex == 0 ? -1 : 1;
        #endregion


        #region Player.IO Methods
        public override void GameStarted() {
            Utils.Log(this, $"GameStarted : Room start : {RoomId}");
            _model = new BoardModel();
            _model.Init();
        }

        public override void GameClosed() {
            Utils.Log(this, $"GameClosed : Room close : {RoomId}");
        }

        public override void UserJoined(Player player) {
            player.Send(_commonConst.serverMessageJoin, (player.Id - 1).ToString());

            if (PlayerCount == 2)
                GameInit();
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
            Broadcast(_commonConst.serverMessageGameInit);
            NextTurn();
        }

        private void AddPiece(Player player, Message m) {
            BoopVector v = BoopVector.FromString(m.GetString(0));
            int pieceValue = m.GetInt(1);
            bool large = Math.Abs(pieceValue) == 2;
            int actualValue = _model.AddPieceOnBoard(v, large, _currentPlayerValue);

            if (actualValue != pieceValue) {
                Utils.LogError(this, "GotMessage", "received value is different from value");
                return;
            }

            SpreadMessage(_commonConst.serverMessageAddPiece, player, m);

            _model.Simulate(v, out List<BoopVector>[] alignedPerPlayer);
            List<BoopVector> aligned = alignedPerPlayer[_currentPlayerIndex];

            if (aligned != null && aligned.Count > 1) {
                Utils.Log(this, "AddPiece", $"{aligned.Count}");
                string[] infos = new string[aligned.Count];
                for (int i = 0; i < aligned.Count; i++)
                    infos[i] = aligned[i].ToString();
                player.Send(_commonConst.serverMessageAlignedPieces, infos);
            }
            else {
                NextTurn();
            }
        }

        private void SelectPieces(Player player, Message m) {
            string[] infos = CommonUtils.GetMessageParams(m);
            _model.EvaluateAlignment(BoopVector.FromString(infos[0]), BoopVector.FromString(infos[2]), out List<BoopVector> list);

            if (list != null && list.Count == 3)
                SpreadMessage(_commonConst.serverMessageSelectPieces, player, m);
        }

        private void NextTurn() {
            _currentPlayerIndex++;
            if (_currentPlayerIndex > Players.Count() - 1)
                _currentPlayerIndex = 0;

            Broadcast(_commonConst.serverMessageNextTurn, _currentPlayerIndex.ToString());
        }
        #endregion
    }
}