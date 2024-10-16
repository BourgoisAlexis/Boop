using UnityEngine;

public class UIView : MonoBehaviour {
    #region Variables
    [SerializeField] protected UIButton _backButton;

    private bool _initialized;
    #endregion


    public virtual void Show(params object[] parameters) {
        if (!_initialized)
            Init(parameters);
    }

    protected virtual void Init(params object[] parameters) {
        _backButton?.onClick.AddListener(Back);
        _initialized = true;
    }

    public virtual void Back() {
        GlobalManager.Instance.SceneManager.Back();
    }
}
