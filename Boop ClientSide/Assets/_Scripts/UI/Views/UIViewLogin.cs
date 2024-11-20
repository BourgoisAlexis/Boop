using TMPro;
using UnityEngine;

public class UIViewLogin : UIView {
    [SerializeField] private UIButton _loginButton;
    [SerializeField] private TMP_InputField _inputField;


    protected override void Init(params object[] parameters) {
        base.Init(parameters);

        _loginButton.onClick.AddListener(Login);
        Utils.InitializeInputField(_inputField);
    }

    private void Login() {
        if (string.IsNullOrEmpty(_inputField.text))
            return;

        GlobalManager.Instance.ConnectToPlayerIO(_inputField.text);
    }
}
