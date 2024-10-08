using DG.Tweening;
using System;
using UnityEngine;

public class BoardPiece : MonoBehaviour {
    [SerializeField] private MeshRenderer[] _visuals;

    private Vector3 _baseScale;
    private Vector3 _basePos;
    private Ease _ease = Ease.InExpo;
    private Transform _activeVisual;


    public async void Init(int value) {
        foreach (MeshRenderer m in _visuals)
            m.gameObject.SetActive(false);

        int index = Mathf.Abs(value) - 1 + (value < 0 ? 0 : 2);
        MeshRenderer visual = _visuals[index];

        visual.gameObject.SetActive(true);
        visual.material.color = value > 0 ? Color.blue : Color.red;

        _activeVisual = visual.transform;
        _baseScale = _activeVisual.localScale;
        _basePos = _activeVisual.localPosition;

        float amplitude = 1.5f;
        _activeVisual.localPosition = Vector3.up * (_basePos.y + amplitude);
        _activeVisual.DOScale(_baseScale + Vector3.up * amplitude, AppConst.globalAnimDuration).SetEase(_ease);
        await _activeVisual.DOLocalMoveY(_basePos.y, AppConst.globalAnimDuration).SetEase(_ease).AsyncWaitForCompletion();
        _activeVisual.DOScale(_baseScale, AppConst.globalAnimDuration);
    }

    public async void Delete(Action onEnd) {
        await transform.DOScale(0, AppConst.globalAnimDuration).SetEase(_ease).AsyncWaitForCompletion();
        onEnd?.Invoke();
        Destroy(gameObject);
    }
}
