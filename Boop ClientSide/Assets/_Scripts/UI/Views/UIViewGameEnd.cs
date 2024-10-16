using TMPro;
using UnityEngine;

public class UIViewGameEnd : UIView {
    [SerializeField] private TextMeshProUGUI _tmproPlayerName;
    [SerializeField] private UIButton _playAgainButton;
    [SerializeField] private UIButton _quitButton;

    public override void Show(params object[] parameters) {
        base.Show(parameters);

        int playerIndex = (int)parameters[0];
        _tmproPlayerName.text = $"Player {playerIndex} won";
    }

    protected override void Init(params object[] parameters) {
        base.Init(parameters);

        GlobalManager gm = GlobalManager.Instance;
        _playAgainButton.onClick.AddListener(() => { gm.PlayerIOManager.SendMessage(gm.CommonConst.userMessagePlayAgain); });
        _quitButton.onClick.AddListener(() => { 
            gm.PlayerIOManager.SendMessage(gm.CommonConst.userMessageQuit);
            gm.NavigationManager.AutoClearingActionOnLoaded(() => { gm.PlayerIOManager.LeaveRoom(); });
            gm.NavigationManager.LoadScene(0);
        });
    }
}
