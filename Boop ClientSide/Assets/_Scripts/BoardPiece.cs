using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class BoardPiece : MonoBehaviour {
    #region Variables
    [SerializeField] private MeshRenderer[] _visuals;

    private Transform _activeVisual;
    private Vector3 _baseScale;
    private Vector3 _basePos;
    private Ease _ease = Ease.InExpo;
    #endregion


    public void Init(int value) {
        foreach (MeshRenderer m in _visuals)
            m.gameObject.SetActive(false);

        int index = Mathf.Abs(value) - 1 + (value < 0 ? 0 : 2);
        MeshRenderer visual = _visuals[index];

        visual.gameObject.SetActive(true);
        visual.material.color = value > 0 ? Color.blue : Color.red;

        _activeVisual = visual.transform;
        _baseScale = _activeVisual.localScale;
        _basePos = _activeVisual.localPosition;

        StartCoroutine(SpawnCorout());
    }

    public void Delete(Action onEnd) {
        StartCoroutine(DeleteCorout(onEnd));
    }


    //Anim Coroutines
    private IEnumerator SpawnCorout() {
        float amplitude = 1.5f;
        _activeVisual.localPosition = Vector3.up * (_basePos.y + amplitude);
        _activeVisual.DOScale(_baseScale + Vector3.up * amplitude, AppConst.globalAnimDuration).SetEase(_ease);
        yield return _activeVisual.DOLocalMoveY(_basePos.y, AppConst.globalAnimDuration).SetEase(_ease).WaitForCompletion();
        _activeVisual.DOScale(_baseScale, AppConst.globalAnimDuration);
    }

    private IEnumerator DeleteCorout(Action onEnd) {
        yield return transform.DOScale(0, AppConst.globalAnimDuration).SetEase(_ease).WaitForCompletion();
        onEnd?.Invoke();
        Destroy(gameObject);
    }
}
