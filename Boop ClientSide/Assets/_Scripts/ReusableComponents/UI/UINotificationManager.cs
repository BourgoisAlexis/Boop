using DG.Tweening;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UINotificationManager : MonoBehaviour {
    #region Variables
    [SerializeField] private TextMeshProUGUI _tmproContent;
    [SerializeField] private RectTransform _frame;

    private Vector4 _anchors;
    #endregion


    public void Awake() {
        _anchors = new Vector4(_frame.anchorMin.x, _frame.anchorMin.y, _frame.anchorMax.x, _frame.anchorMax.y);
        _frame.anchorMin = new Vector2(_anchors.x, _anchors.y + 1);
        _frame.anchorMax = new Vector2(_anchors.z, _anchors.w + 1);
    }


    public async void Show(string content) {
        if (string.IsNullOrEmpty(content)) {
            CommonUtils.ErrorOnParams("UINotificationManager", "Show");
            return;
        }

        _tmproContent.text = content;

        _frame.DOAnchorMin(new Vector2(_anchors.x, _anchors.y), AppConst.globalAnimDuration);
        _frame.DOAnchorMax(new Vector2(_anchors.z, _anchors.w), AppConst.globalAnimDuration);
        await Task.Delay(3000);
        Hide();
    }

    public void Hide() {
        _frame.DOAnchorMin(new Vector2(_anchors.x, _anchors.y + 1), AppConst.globalAnimDuration);
        _frame.DOAnchorMax(new Vector2(_anchors.z, _anchors.w + 1), AppConst.globalAnimDuration);
    }
}
