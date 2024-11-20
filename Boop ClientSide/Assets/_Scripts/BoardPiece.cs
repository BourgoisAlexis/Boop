using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class BoardPiece : MonoBehaviour {
    #region Variables
    [SerializeField] private Transform _visual;
    [SerializeField] private MeshRenderer[] _meshRenderers;

    private Vector3 _baseScale;
    private Vector3 _basePos;
    private Ease _ease = Ease.InExpo;
    private float _amplitude = 1.5f;
    #endregion


    public void Init(int value) {
        bool large = Math.Abs(value) > 1;
        _baseScale = new Vector3(large ? 1 : 0.5f, 0.2f, large ? 1 : 0.5f);
        _basePos = _visual.localPosition;
        _visual.eulerAngles = Vector3.up * (value > 0 ? 45 : 0);
        _visual.localScale = _baseScale;

        foreach (MeshRenderer m in _meshRenderers)
            m.material.color = AppConst.GetColor(ColorVariant.Shade, value);

        StartCoroutine(SpawnCorout(AppConst.GetColor(ColorVariant.Default, value)));
    }

    public void Delete(Action onEnd) {
        StartCoroutine(DeleteCorout(onEnd));
    }


    //Anim Coroutines
    private IEnumerator SpawnCorout(Color color) {
        _visual.localPosition = Vector3.up * (_basePos.y + _amplitude);
        GameObject instantiated = GlobalManager.Instance.PoolManager.Dequeue(AppConst.popKey, transform);
        instantiated.transform.position = _visual.position;
        instantiated.GetComponent<BoardPop>().Init(color);

        _visual.DOScale(_baseScale + Vector3.up * _amplitude, AppConst.globalAnimDuration).SetEase(_ease);
        yield return _visual.DOLocalMoveY(_basePos.y, AppConst.globalAnimDuration).SetEase(_ease).WaitForCompletion();

        yield return new WaitForSeconds(0.1f);
        yield return _visual.DOScale(_baseScale, AppConst.globalAnimDuration).SetEase(_ease).WaitForCompletion();

        GlobalManager.Instance.PoolManager.Enqueue(AppConst.popKey, instantiated);
    }

    private IEnumerator DeleteCorout(Action onEnd) {
        yield return Utils.BumpAnim(transform, _amplitude, 0);
        onEnd?.Invoke();
        GlobalManager.Instance.PoolManager.Enqueue(AppConst.pieceKey, gameObject);
    }
}
