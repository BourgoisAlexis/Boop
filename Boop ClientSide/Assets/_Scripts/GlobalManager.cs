using System.Text;
using UnityEngine;

public class GlobalManager : MonoBehaviour {
    public static GlobalManager Instance { get; private set; }

    #region Variables
    private CommonConst _commonConst;
    private PlayerIOManager _playerIOManager;
    private NavigationManager _navigationManager;
    private UITransitionManager _uiTransitionManager;
    private PoolManager _poolManager;
    private SFXManager _sfxManager;
    private SceneManager _sceneManager;
    private Loading _loading;
    private UINotificationManager _uiNotificationManager;
    private RoomModel _roomModel;

    //Debug
    [Header("Debug")]
    public bool showLowPriorityLogs;
    public bool useLocalPlayerIO;
    public int numberOfPlayer;

    //Gameplay
    private int _playerIndex;

    public int PlayerIndex => _playerIndex;
    public int PlayerValue => CommonUtils.PlayerValueFromIndex(_playerIndex);
    private ControllerBoard _controllerBoard => _sceneManager as ControllerBoard;

    //Accessors
    public CommonConst CommonConst => _commonConst;
    public PlayerIOManager PlayerIOManager => _playerIOManager;
    public NavigationManager NavigationManager => _navigationManager;
    public UITransitionManager UITransitionManager => _uiTransitionManager;
    public PoolManager PoolManager => _poolManager;
    public SFXManager SFXManager => _sfxManager;
    public SceneManager SceneManager => _sceneManager;
    public Loading Loading => _loading;
    public UINotificationManager UINotificationManager => _uiNotificationManager;
    public RoomModel RoomModel => _roomModel;
    #endregion


    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        _commonConst = new CommonConst();
        _playerIOManager = new PlayerIOManager();
        _navigationManager = new NavigationManager();

        _loading = GetComponent<Loading>();
        _poolManager = GetComponent<PoolManager>();
        _sfxManager = GetComponent<SFXManager>();

        _uiTransitionManager = FindObjectOfType<UITransitionManager>();
        _uiNotificationManager = FindObjectOfType<UINotificationManager>();

        _navigationManager.onLoaded += OnSceneLoaded;

        OnSceneLoaded();
    }


    private void OnSceneLoaded() {
        _sceneManager = FindObjectOfType<SceneManager>();
        _sceneManager?.Init();
        _sfxManager?.Init();
    }

    public void ConnectToPlayerIO(string userID) {
        if (string.IsNullOrEmpty(userID)) {
            Utils.LogError(this, "ConnectToPlayerIO", "userID is null or empty");
            return;
        }

        _loading.Load(true);

        _playerIOManager.Init("boop-icbnqap9eeykmbikigg6xw", userID, null);

        _playerIOManager.HandleMessage(_commonConst.serverMessageError, OnlineError);
        _playerIOManager.HandleMessage(_commonConst.serverMessageJoin, Join, 1);
        _playerIOManager.HandleMessage(_commonConst.serverMessagePlayerJoinRoom, PlayerJoinRoom, 2);
        _playerIOManager.HandleMessage(_commonConst.serverMessageLoadScene, LoadScene, 1);

        _playerIOManager.HandleMessage(_commonConst.serverMessageNextTurn, NextTurn, 2);

        _sceneManager.GoToView(1);
        _loading.Load(false);
    }


    private void OnlineError(string[] infos) {
        StringBuilder sb = new StringBuilder();
        foreach (string info in infos)
            sb.AppendLine(info);

        _uiNotificationManager.Show(sb.ToString());
        Utils.LogError(this, "OnlineError", sb.ToString());
    }

    private void Join(string[] infos) {
        if (int.TryParse(infos[0], out _playerIndex) == false)
            Utils.LogError(this, "Join", "can't parse infos[0]");
    }

    private void PlayerJoinRoom(string[] infos) {
        _roomModel = new RoomModel(
            infos[0],
            infos[1].Split(_commonConst.separator)
        );
    }

    private void NextTurn(string[] infos) {
        if (int.TryParse(infos[0], out int currentPlayerIndex) == false) {
            Utils.LogError(this, "NextTurn", "can't parse infos[0]");
            return;
        }

        string serverBoard = infos[1];
        string localBoard = CommonUtils.BoardState(_controllerBoard.Model.Board);

        if (serverBoard != localBoard)
            Utils.LogError(this, "NextTurn", "Need synchronisation");
    }

    private void LoadScene(string[] infos) {
        if (int.TryParse(infos[0], out int index) == false)
            Utils.LogError(this, "Join", "can't parse infos[0]");

        _navigationManager.AutoClearingActionOnLoaded(() => { _playerIOManager.SendMessage(_commonConst.userMessageSceneLoaded); });
        _navigationManager.LoadScene(index);
    }
}
