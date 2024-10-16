using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    #region Variables
    public UnityEvent onClick;

    private Image _image;
    private TextMeshProUGUI _tmproContent;
    private float _fadeDuration = 0.15f;
    #endregion


    private void Awake() {
        _image = GetComponent<Image>();
        _tmproContent = GetComponentInChildren<TextMeshProUGUI>();
        ReInit();
    }

    private void OnEnable() {
        ReInit();
    }

    private void ReInit() {
        if (_image != null)
            _image.color = Color.white;

        if (_tmproContent != null)
            _tmproContent.color = Color.black;
    }


    public void OnPointerEnter(PointerEventData eventData) {
        _image?.DOColor(Color.black, _fadeDuration);
        _tmproContent?.DOColor(Color.white, _fadeDuration);
        transform.DOScale(1.1f, _fadeDuration);
    }

    public void OnPointerExit(PointerEventData eventData) {
        _image?.DOColor(Color.white, _fadeDuration);
        _tmproContent?.DOColor(Color.black, _fadeDuration);
        transform.DOScale(1f, _fadeDuration);
    }

    public void OnPointerClick(PointerEventData eventData) {
        onClick?.Invoke();
    }
}
