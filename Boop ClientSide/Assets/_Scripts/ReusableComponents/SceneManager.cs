using UnityEngine;

public abstract class SceneManager : MonoBehaviour {
    [SerializeField] protected UIViewManager _viewManager;

    public virtual void Init(params object[] parameters) {
        _viewManager.Init();
    }

    public virtual void GoToView(int index) {
        _viewManager.ShowView(index);
    }
}
