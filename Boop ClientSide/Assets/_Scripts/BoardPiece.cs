using DG.Tweening;
using System;
using UnityEngine;

public class BoardPiece : MonoBehaviour {
    [SerializeField] private MeshRenderer[] _visuals;

    private Vector3 _baseScale;
    private Ease _ease = Ease.InExpo;

    public async void Init(int value) {
        foreach (MeshRenderer m in _visuals)
            m.gameObject.SetActive(false);

        MeshRenderer visual = _visuals[Mathf.Abs(value) - 1];

        visual.gameObject.SetActive(true);
        visual.material.color = value > 0 ? Color.blue : Color.red;

        Transform t = visual.transform;
        _baseScale = t.localScale;
        float amplitude = 1.5f;
        t.localPosition = Vector3.up * (_baseScale.y + amplitude);
        t.DOScale(_baseScale + Vector3.up * amplitude, AppConst.globalAnimDuration).SetEase(_ease);
        await t.DOLocalMoveY(_baseScale.y, AppConst.globalAnimDuration).SetEase(_ease).AsyncWaitForCompletion();
        t.DOScale(_baseScale, AppConst.globalAnimDuration);
    }

    public async void Delete(Action onEnd) {
        await transform.DOScale(0, AppConst.globalAnimDuration).SetEase(_ease).AsyncWaitForCompletion();
        onEnd?.Invoke();
        Destroy(gameObject);
    }
}
