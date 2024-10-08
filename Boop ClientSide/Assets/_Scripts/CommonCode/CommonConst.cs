public class CommonConst {
    public string defaultRoomID = "Lobby";
    public int userLimitPerRoom = 30;

    //Server Messages
    public string serverMessageError = "servermessage_error";
    public string serverMessageJoin = "servermessage_join";
    public string serverMessageGameInit = "servermessage_gameinit";
    public string serverMessageAddPiece = "servermessage_addpiece";
    public string serverMessageAlignedPieces = "servermessage_alignedpieces";
    public string serverMessageSelectPieces = "servermessage_selectpieces";
    public string serverMessageNextTurn = "servermessage_nextturn";

    //User Messages
    public string userMessageAddPiece = "usermessage_addpiece";
    public string userMessageSelectPieces = "usermessage_selectpieces";
}

public enum GameState {
    MainScreen,
    Gameplay,
    Ended,

    Default
}

public enum BoardState {
    Placing,
    Selecting,
    Waiting,

    Default
}