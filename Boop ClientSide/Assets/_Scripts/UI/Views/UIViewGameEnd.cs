using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;

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

        StartCoroutine(AnimCorout());
    }

    protected override void Init(params object[] parameters) {
        base.Init(parameters);

        GlobalManager gm = GlobalManager.Instance;
        _playAgainButton.onClick.AddListener(() => {
            gm.PlayerIOManager.SendMessage(gm.CommonConst.userMessagePlayAgain);
            GlobalManager.Instance.Loading.Load(true);
            GlobalManager.Instance.UINotificationManager.Show("Waiting for your opponent...");
        });
        _quitButton.onClick.AddListener(() => {
            gm.PlayerIOManager.SendMessage(gm.CommonConst.userMessageQuit);
            gm.NavigationManager.AutoClearingActionOnLoaded(() => { gm.PlayerIOManager.LeaveRoom(); });
            gm.NavigationManager.LoadScene(0);
        });
    }

    private IEnumerator AnimCorout() {
        GlobalManager.Instance.SFXManager.PlayAudio(14);

        for (int i = 0; i <= _tmproText.text.Length; i++) {
            yield return SoundAndDelayCorout();
            _tmproText.maxVisibleCharacters = i;
        }

        yield return new WaitForSeconds(1);

        yield return SoundAndDelayCorout();
        _playAgainButton.gameObject.SetActive(true);

        yield return SoundAndDelayCorout();
        _quitButton.gameObject.SetActive(true);

        yield break;
    }

    private IEnumerator SoundAndDelayCorout() {
        GlobalManager.Instance.SFXManager.PlayAudio(7);
        yield return new WaitForSeconds(AppConst.globalAnimDuration / 2);
    }
}
