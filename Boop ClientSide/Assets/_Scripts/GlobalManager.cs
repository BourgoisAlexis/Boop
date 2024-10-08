using System.Text;
using UnityEngine;

public class GlobalManager : MonoBehaviour {
    public static GlobalManager Instance;

    #region Variables
    public bool showLowPriorityLogs;
    public bool useLocalPlayerIO;

    private CommonConst _commonConst;
    private PlayerIOManager _playerIOManager;
    private NavigationManager _navigationManager;
    private UITransitionManager _uiTransitionManager;
    private SceneManager _sceneManager;
    private Loading _loading;

    //Accessors
    public CommonConst CommonConst => _commonConst;
    public PlayerIOManager PlayerIOManager => _playerIOManager;
    public NavigationManager NavigationManager => _navigationManager;
    public UITransitionManager UITransitionManager => _uiTransitionManager;
    public SceneManager SceneManager => _sceneManager;
    public Loading Loading => _loading;

    //Gameplay
    private BoardController _boardController => _sceneManager as BoardController;
    private int _playerIndex;
    private int _currentPlayerIndex;

    public int playerValue => _playerIndex == 0 ? -1 : 1;
    public int currentPlayerValue => _currentPlayerIndex == 0 ? -1 : 1;
    #endregion

    private void Awake() {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        DontDestroyOnLoad(gameObject);
        GetSceneManager();
    }

    private void Start() {
        _commonConst = new CommonConst();
        _playerIOManager = new PlayerIOManager();
        _navigationManager = new NavigationManager();

        _loading = GetComponent<Loading>();

        _uiTransitionManager = FindObjectOfType<UITransitionManager>();

        _navigationManager.onSceneLoaded += GetSceneManager;
        ConnectToPlayerIO();

        _loading.Load(false);
    }

    private void GetSceneManager() {
        _sceneManager = FindObjectOfType<SceneManager>();
        _sceneManager?.Init();
    }


    public void ConnectToPlayerIO() {
        _loading.Load(true);

        _playerIOManager.Init("boop-icbnqap9eeykmbikigg6xw", "Alexis", null);

        _playerIOManager.HandleMessage(_commonConst.serverMessageError, OnlineError);
        _playerIOManager.HandleMessage(_commonConst.serverMessageJoin, Join);
        _playerIOManager.HandleMessage(_commonConst.serverMessageGameInit, InitGame);
        _playerIOManager.HandleMessage(_commonConst.serverMessageNextTurn, NextTurn);
        _loading.Load(false);
    }


    private void OnlineError(string[] infos) {
        StringBuilder sb = new StringBuilder();
        foreach (string info in infos)
            sb.AppendLine(info);

        Utils.LogError(this, "OnlineError", sb.ToString());
    }

    private void Join(string[] infos) {
        _playerIndex = int.Parse(infos[0]);
    }

    private void InitGame(string[] infos) {
        _navigationManager.LoadScene(1);
    }

    private void NextTurn(string[] infos) {
        _currentPlayerIndex = int.Parse(infos[0]);
        string serverBoard = infos[1];
        string localBoard = CommonUtils.BoardState(_boardController.Model.Board);

        if (serverBoard != localBoard)
            Utils.LogError(this, "NextTurn", "Need synchronisation");

        if (_currentPlayerIndex == _playerIndex)
            _boardController.NextTurn();
    }
}
