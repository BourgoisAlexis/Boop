using DG.Tweening;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class BoardSquare : MonoBehaviour {
    [SerializeField] private MeshRenderer _visual;
    [SerializeField] private float _scaleValue;
    [SerializeField] private Color _highlightColor;

    private int _x;
    private int _y;
    private Vector3 _baseScale;
    private Color _color = Color.white;

    public int X => _x;
    public int Y => _y;
    public BoopVector Pos => new BoopVector(_x, _y);

    public void Init(int x, int y) {
        _x = x;
        _y = y;
        _baseScale = _visual.transform.localScale;
    }


    public void SetBaseColor(Color color) {
        _color = color;
        SetColor(color);
    }

    public async void SetColor(Color color, float duration = 0) {
        await _visual.material.DOColor(color, AppConst.globalAnimDuration).AsyncWaitForCompletion();

        if (duration <= 0)
            return;

        await Task.Delay(Mathf.RoundToInt(duration * 1000));
        _visual.material.DOColor(_color, AppConst.globalAnimDuration);
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

        _visual.material.DOColor(highlight ? _highlightColor : _color, AppConst.globalAnimDuration);
    }

    public async void Click() {
        float diff = Math.Abs(1 - _scaleValue) + 0.2f;
        await _visual.transform.DOScale(new Vector3(1 - diff, _baseScale.y, 1 - diff), AppConst.globalAnimDuration).AsyncWaitForCompletion();
        _visual.transform.DOScale(_baseScale, AppConst.globalAnimDuration);
    }
}
