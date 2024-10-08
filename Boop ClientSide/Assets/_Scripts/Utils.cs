using System;
using UnityEngine;

public static class Utils {
    public static void Log(string source, string methodName, string message = "") {
        Debug.Log($"{source} > {methodName} > {message}");
    }

    public static void Log(object source, string methodName, string message = "") {
        Log(source.GetType().ToString(), methodName, message);
    }

    public static void LogError(string source, string methodName, string message = "") {
        Debug.LogError($"{source} > {methodName} > {message}");
    }

    public static void LogError(object source, string methodName, string message = "") {
        LogError(source.GetType().ToString(), methodName, message);
    }

    /// <summary>
    /// Add actions to loadScene and remove them on scene loaded
    /// </summary>
    /// <param name="actions"></param>
    public static void AutoClearingActionOnLoad(params Action[] actions) {
        NavigationManager nav = GlobalManager.Instance.NavigationManager;

        foreach (Action action in actions)
            nav.onLoadScene += action;

        Action onLoaded = () => {
            foreach (Action action in actions)
                nav.onLoadScene -= action;
        };

        nav.onSceneLoaded += onLoaded;
    }
}