using System.Text;
using UnityEngine;

public class GlobalManager : MonoBehaviour {
    public static GlobalManager Instance;

    #region Variables
    public bool showLowPriorityLogs;
    public bool useLocalPlayerIO;

    private CommonConst _commonConst;
    private BoardModel _boardModel;
    private InputManager _inputManager;
    private BoardController _boardController;
    private PlayerIOManager _playerIOManager;
    private UIManager _uiManager;

    public CommonConst CommonConst => _commonConst;
    public BoardModel BoardModel => _boardModel;
    public InputManager InputManager => _inputManager;
    public BoardController BoardController => _boardController;
    public PlayerIOManager PlayerIOManager => _playerIOManager;
    public UIManager UIManager => _uiManager;

    //Gameplay
    public int playerIndex;
    public int currentPlayerIndex;

    public int playerValue => playerIndex == 0 ? -1 : 1;
    public int currentPlayerValue => currentPlayerIndex == 0 ? -1 : 1;
    #endregion

    private void Awake() {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start() {
        Init();
    }

    private void Init() {
        _commonConst = new CommonConst();
        _boardModel = new BoardModel();
        _inputManager = GetComponent<InputManager>();
        _boardController = GetComponent<BoardController>();
        _playerIOManager = new PlayerIOManager();
        _uiManager = FindObjectOfType<UIManager>();

        ConnectToPlayerIO();
    }

    private void ConnectToPlayerIO() {
        Utils.Loading(true);

        _playerIOManager.HandleMessage(_commonConst.serverMessageError, OnlineError);
        _playerIOManager.HandleMessage(_commonConst.serverMessageJoin, (string[] infos) => { playerIndex = int.Parse(infos[0]); });
        _playerIOManager.HandleMessage(_commonConst.serverMessageCurrentPlayer, (string[] infos) => { currentPlayerIndex = int.Parse(infos[0]); });
        _playerIOManager.HandleMessage(_commonConst.serverMessageGameInit, InitGame);

        _playerIOManager.Init("Alexis", null);
    }

    private void OnlineError(string[] infos) {
        StringBuilder sb = new StringBuilder();
        foreach (string info in infos)
            sb.AppendLine(info);

        Utils.LogError(this, "OnlineError", sb.ToString());
    }

    private void InitGame(string[] infos) {
        _boardModel.Init();
        _inputManager.Init();
        _boardController.Init();
        _uiManager.Init();

        Utils.Loading(false);
    }
}
