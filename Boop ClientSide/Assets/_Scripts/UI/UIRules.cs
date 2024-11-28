using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;

[Serializable]
public class Rule {
    public int start;
    public int end;
}

public class UIRules : MonoBehaviour {
    #region Variables
    [SerializeField] private GameObject _view;
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private TextAsset _rulesAsset;
    [SerializeField] private List<Rule> _rules = new List<Rule>();

    [Header("Buttons")]
    [SerializeField] private UIButton _showButton;
    [SerializeField] private UIButton _closeButton;
    [SerializeField] private UIButton _nextButton;
    [SerializeField] private UIButton _previousButton;

    private int _currentIndex;
    [SerializeField] private List<string> _actualRules = new List<string>();
    #endregion


    public void Init() {
        _currentIndex = 0;
        _background.color = AppConst.GetColor(ColorVariant.SuperTone, GlobalManager.Instance.PlayerValue);
        _title.color = AppConst.GetColor(ColorVariant.Light, GlobalManager.Instance.PlayerValue);
        _text.color = AppConst.GetColor(ColorVariant.Light, GlobalManager.Instance.PlayerValue);

        _showButton.onClick.AddListener(Show);
        _closeButton.onClick.AddListener(Hide);
        _nextButton.onClick.AddListener(Next);
        _previousButton.onClick.AddListener(Previous);

        GetRules();
        Hide();
    }

    private void GetRules() {
        string[] lines = _rulesAsset.text.Split('\n');
        StringBuilder sb = new StringBuilder();

        foreach (var rule in _rules) {
            sb.Clear();
            for (int i = rule.start - 1; i < rule.end; i++)
                sb.AppendLine(lines[i]);

            _actualRules.Add(sb.ToString());
        }
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
        _text.text = _actualRules[_currentIndex];
    }
}
