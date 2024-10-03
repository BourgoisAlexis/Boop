public class CommonConst {
    public string defaultRoomID = "Lobby";
    public int userLimitPerRoom = 30;

    //Server Messages
    public string serverMessageError = "servermessage_error";
    public string serverMessageJoin = "servermessage_join";
    public string serverMessageGameInit = "servermessage_gameinit";
    public string serverMessageCurrentPlayer = "servermessage_currentplayer";
    public string serverMessageAddPiece = "servermessage_addpiece";

    //User Messages
    public string userMessageAddPiece = "usermessage_addpiece";
}

public enum GameState {
    MainScreen,
    Gameplay,
    Ended,

    Default
}

public enum BoardState {
    Playing,
    Selecting,
    OpponentPhase,

    Default
}