using System;
using System.Collections.Generic;
using PlayerIO.GameLibrary;

namespace Boop {
    public class Player : BasePlayer {
    }

    [RoomType("Lobby")]
    public class GameCode : Game<Player> {
        #region Variables
        private CommonConst _commonConst = new CommonConst();
        private Random _random = new Random();
        #endregion


        #region Player.IO Methods
        public override void GameStarted() {
            Utils.Log(this, $"GameStarted : Room start : {RoomId}");
        }

        public override void GameClosed() {
            Utils.Log(this, $"GameClosed : Room close : {RoomId}");
        }

        public override void UserJoined(Player player) {
            player.Send(_commonConst.serverMessageJoin, (player.Id - 1).ToString());

            //if (PlayerCount == 2)
                GameInit();
        }

        public override void UserLeft(Player player) {

        }

        // This method is called when a player sends a message into the server code
        public override void GotMessage(Player player, Message m) {
            CommonUtils.LogMessage(m);

            switch (m.Type) {
                case "usermessage_addpiece":
                    BoopVector v = new BoopVector(m.GetInt(0), m.GetInt(1));
                    int pieceValue = m.GetInt(2);
                    //Model > instancier le model
                    //Répercussion > Ajouter la pièce au model
                    //Verif > Comparer les 2 valeurs
                    SpreadMessage(_commonConst.serverMessageAddPiece, m, player);
                    break;
            }
        }
        #endregion

        #region Custom Methods
        private void SpreadMessage(string messageType, Message originalMessage, Player sender) {
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
            int index = _random.NextDouble() > 0.5f ? 0 : 1;
            Broadcast(_commonConst.serverMessageCurrentPlayer, index.ToString());
            Broadcast(_commonConst.serverMessageGameInit);
        }
        #endregion
    }
}