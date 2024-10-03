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

    public static void Loading(bool loading) {
        GlobalManager.Instance.UIManager.Load(loading);
    }
}