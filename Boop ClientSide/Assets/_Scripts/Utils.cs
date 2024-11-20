using DG.Tweening;
using System.Collections;
using TMPro;
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

    public static IEnumerator BumpAnim(Transform t, float scaleUp, float scaleDown) {
        yield return t.DOScale(scaleUp, AppConst.globalAnimDuration).SetEase(Ease.InExpo).WaitForCompletion();
        yield return t.DOScale(scaleDown, AppConst.globalAnimDuration).SetEase(Ease.InExpo).WaitForCompletion();
    }

    public static void InitializeInputField(TMP_InputField field) {
        field.text = string.Empty;
        field.onValueChanged.RemoveAllListeners();
        field.onValueChanged.AddListener((string s) => { GlobalManager.Instance.SFXManager.PlayAudio(6); });
    }
}