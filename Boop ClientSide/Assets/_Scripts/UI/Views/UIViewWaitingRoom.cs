using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIViewWaitingRoom : UIView {
    [SerializeField] private UIButton _startButton;
    [SerializeField] private UIButton _backButton;

    public override void Init(params object[] parameters) {
        base.Init(parameters);

        _startButton.onClick.AddListener(StartGame);
        _backButton.onClick.AddListener(Back);
    }

    private void StartGame() {

    }

    private void Back() {
        GlobalManager.Instance.SceneManager.GoToView(0);
    }
}
