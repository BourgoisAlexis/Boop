public class ControllerTitleScreen : SceneManager {
    public override void Init(params object[] parameters) {
        base.Init(parameters);

        GlobalManager.Instance.Loading.Load(false);
    }
}
