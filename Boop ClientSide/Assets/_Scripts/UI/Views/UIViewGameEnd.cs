using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UIViewGameEnd : UIView {
    [SerializeField] private TextMeshProUGUI _tmproText;
    [SerializeField] private RectTransform[] _ornaments;
    [SerializeField] private UIButton _playAgainButton;
    [SerializeField] private UIButton _quitButton;

    public override void Show(params object[] parameters) {
        base.Show(parameters);

        _playAgainButton.gameObject.SetActive(false);
        _quitButton.gameObject.SetActive(false);

        int playerIndex = (int)parameters[0];
        _tmproText.maxVisibleCharacters = 0;
        _tmproText.text = $"{(playerIndex == GlobalManager.Instance.PlayerIndex ? "Victory !" : "Defeat...")}";

        StartCoroutine(AnimCorout((bool)parameters[1]));
    }

    protected override void Init(params object[] parameters) {
        base.Init(parameters);

        _playAgainButton.onClick.AddListener(Replay);
        _quitButton.onClick.AddListener(() => Quit(null));
    }

    private void Quit(string[] infos) {
        GlobalManager gm = GlobalManager.Instance;
        gm.PlayerIOManager.UnhandleMessage(gm.CommonConst.serverMessagePlayerLeaveRoom, Quit);
        gm.PlayerIOManager.SendMessage(gm.CommonConst.userMessageQuit);
        gm.NavigationManager.AutoClearingActionOnLoaded(() => { gm.PlayerIOManager.LeaveRoom(); });
        gm.NavigationManager.LoadScene(0);
    }

    private void Replay() {
        GlobalManager gm = GlobalManager.Instance;
        gm.PlayerIOManager.SendMessage(gm.CommonConst.userMessagePlayAgain);
        gm.Loading.Load(true);
        gm.UINotificationManager.Show("Waiting for your opponent...");
        gm.PlayerIOManager.HandleMessage(gm.CommonConst.serverMessagePlayerLeaveRoom, Quit);
    }

    private IEnumerator AnimCorout(bool playerleft = false) {
        GlobalManager.Instance.SFXManager.PlayAudio(14);

        for (int i = 0; i <= _tmproText.text.Length; i++) {
            yield return SoundAndDelayCorout();
            _tmproText.maxVisibleCharacters = i;
        }

        yield return new WaitForSeconds(1);

        if (!playerleft) {
            yield return SoundAndDelayCorout();
            _playAgainButton.gameObject.SetActive(true);
        }

        yield return SoundAndDelayCorout();
        _quitButton.gameObject.SetActive(true);

        yield break;
    }

    private IEnumerator SoundAndDelayCorout() {
        GlobalManager.Instance.SFXManager.PlayAudio(7);
        yield return new WaitForSeconds(AppConst.globalAnimDuration / 2);
    }
}
