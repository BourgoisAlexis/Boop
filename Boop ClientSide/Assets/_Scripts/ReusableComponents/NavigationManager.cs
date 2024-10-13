using System;
using System.Threading.Tasks;
using UnityEngine;

public class NavigationManager {
    #region Variables
    public Action onLoadScene;
    public Action onSceneLoaded;

    private int _currentSceneIndex = 0;
    #endregion


    public async Task LoadScene(int index) {
        if (index < 0) {
            Utils.LogError(this, "LoadScene", "index is negative");
            return;
        }

        onLoadScene?.Invoke();

        await GlobalManager.Instance.UITransitionManager.Show();

        AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(index);
        while (!op.isDone)
            await Task.Yield();

        _currentSceneIndex = index;
        onSceneLoaded?.Invoke();

        await GlobalManager.Instance.UITransitionManager.Hide();

        Utils.Log(this, $"loaded scene {_currentSceneIndex}");
    }


    public void AutoClearingActionOnLoad(params Action[] actions) {
        foreach (Action action in actions)
            onLoadScene += action;

        Action onLoaded = () => {
            foreach (Action action in actions)
                onLoadScene -= action;
        };

        onSceneLoaded += onLoaded;
    }
}
