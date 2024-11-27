using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRules : MonoBehaviour {
    [SerializeField] private GameObject _view;
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _text;
    [Header("Buttons")]
    [SerializeField] private UIButton _showButton;
    [SerializeField] private UIButton _closeButton;
    [SerializeField] private UIButton _nextButton;
    [SerializeField] private UIButton _previousButton;

    [Multiline(10)]
    [SerializeField] private List<string> _rules = new List<string>();

    private int _currentIndex;

    public void Init() {
        _currentIndex = 0;
        _background.color = AppConst.GetColor(ColorVariant.SuperTone, GlobalManager.Instance.PlayerValue);
        _title.color = AppConst.GetColor(ColorVariant.Light, GlobalManager.Instance.PlayerValue);
        _text.color = AppConst.GetColor(ColorVariant.Light, GlobalManager.Instance.PlayerValue);

        _showButton.onClick.AddListener(Show);
        _closeButton.onClick.AddListener(Hide);
        _nextButton.onClick.AddListener(Next);
        _previousButton.onClick.AddListener(Previous);

        Hide();
    }

    private void Show() {
        UpdateView();
        _view.SetActive(true);
    }

    private void Hide() {
        _view.SetActive(false);
    }

    private void Next() {
        _currentIndex++;
        UpdateView();
    }

    private void Previous() {
        _currentIndex--;
        UpdateView();
    }

    private void UpdateView() {
        _previousButton.gameObject.SetActive(_currentIndex > 0);
        _nextButton.gameObject.SetActive(_currentIndex < _rules.Count - 1);
        _text.text = _rules[_currentIndex].ToString();
    }
}
