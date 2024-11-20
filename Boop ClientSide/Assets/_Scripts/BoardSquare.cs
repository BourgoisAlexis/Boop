using DG.Tweening;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class BoardSquare : MonoBehaviour {
    #region Variables
    [SerializeField] private MeshRenderer _visual;
    [SerializeField] private float _scaleValue;

    private int _x;
    private int _y;
    private Vector3 _baseScale;
    private Color _setColor;
    private Color _highlightColor => AppConst.GetColor(ColorVariant.Tint, GlobalManager.Instance.PlayerValue);

    //Accessors
    public int X => _x;
    public int Y => _y;
    public BoopVector Pos => new BoopVector(_x, _y);
    #endregion


    public void Init(int x, int y) {
        _x = x;
        _y = y;
        _baseScale = _visual.transform.localScale;
        SetBaseColor(AppConst.GetColor(ColorVariant.Light, GlobalManager.Instance.PlayerValue));
    }


    public void SetBaseColor(Color color) {
        _setColor = color;
        SetColor(color);
    }

    private void SetColor(Color color, float duration = 0) {
        StartCoroutine(SetColorCorout(color, duration));
    }


    public void FlashColor(Color color) {
        SetColor(color, 0.5f);
    }

    public void Highlight(bool highlight) {
        float value = highlight ? _scaleValue : 0.9f;
        if (highlight)
            _visual.transform.DOScale(new Vector3(_scaleValue, _baseScale.y, _scaleValue), AppConst.globalAnimDuration);
        else
            _visual.transform.DOScale(_baseScale, AppConst.globalAnimDuration);

        _visual.material.DOColor(highlight ? _highlightColor : _setColor, AppConst.globalAnimDuration);
    }

    public void Click() {
        StartCoroutine(ClickCorout());
    }


    //Anim Coroutines
    private IEnumerator SetColorCorout(Color color, float duration = 0) {
        yield return _visual.material.DOColor(color, AppConst.globalAnimDuration).WaitForCompletion();

        if (duration <= 0)
            yield break;

        yield return new WaitForSeconds(duration);
        _visual.material.DOColor(_setColor, AppConst.globalAnimDuration);
    }

    private IEnumerator ClickCorout() {
        float diff = Math.Abs(1 - _scaleValue) + 0.2f;
        yield return _visual.transform.DOScale(new Vector3(1 - diff, _baseScale.y, 1 - diff), AppConst.globalAnimDuration).WaitForCompletion();
        _visual.transform.DOScale(_baseScale, AppConst.globalAnimDuration);
    }
}
