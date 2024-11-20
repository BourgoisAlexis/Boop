using UnityEngine;

public abstract class SceneManager : MonoBehaviour {
    [SerializeField] protected UIViewManager _viewManager;
    [SerializeField] protected TexturedCanvas _texturedCanvas;


    public virtual void Init(params object[] parameters) {
        _viewManager.Init();
        _texturedCanvas.Init();
    }

    public virtual void GoToView(int index) {
        _viewManager.ShowView(index);
    }

    public virtual void Back() {
        _viewManager.Back();
    }
}
