using UnityEngine;
using TMPro;
using System;

public class UIViewCreateJoin : UIView {
    [SerializeField] private UIButton _createButton;
    [SerializeField] private UIButton _joinButton;
    [SerializeField] private UIButton _copyButton;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TextMeshProUGUI _roomID;

    private int _index;


    public override void Show(params object[] parameters) {
        base.Show(parameters);
        ReInit();
    }

    protected override void Init(params object[] parameters) {
        base.Init(parameters);
        _createButton.onClick.AddListener(Create);
        _joinButton.onClick.AddListener(Join);
        _copyButton.onClick.AddListener(Copy);
    }

    private void Join() {
        if (_index == 0) {
            _inputField.gameObject.SetActive(true);
            _createButton.gameObject.SetActive(false);
        }
        else if (_index == 1) {
            GlobalManager.Instance.Loading.Load(true);
            Action act = () => { GlobalManager.Instance.Loading.Load(false); };
            GlobalManager.Instance.PlayerIOManager.JoinRoom(_inputField.text, act, act);
        }

        _index++;
    }

    private void Create() {
        _createButton.gameObject.SetActive(false);
        _joinButton.gameObject.SetActive(false);

        Action<string> action = (string roomID) => {
            _copyButton.gameObject.SetActive(true);
            _roomID.gameObject.SetActive(true);
            _roomID.text = roomID;
        };
        GlobalManager.Instance.PlayerIOManager.CreateRoom(action);

        _index++;
    }

    public void Copy() {
        GUIUtility.systemCopyBuffer = _roomID.text;
        GlobalManager.Instance.UINotificationManager.Show("Room ID copied in clipboard.");
    }

    public override void Back() {
        if (_index == 0) {
            base.Back();
            return;
        }

        GlobalManager.Instance.PlayerIOManager.LeaveRoom();

        ReInit();
    }

    private void ReInit() {
        _roomID.gameObject.SetActive(false);
        _inputField.gameObject.SetActive(false);

        _joinButton.gameObject.SetActive(true);
        _createButton.gameObject.SetActive(true);
        _copyButton.gameObject.SetActive(false);

        _index = 0;
    }
}
