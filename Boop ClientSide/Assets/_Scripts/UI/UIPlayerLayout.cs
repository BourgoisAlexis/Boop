using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerLayout : MonoBehaviour {
    [SerializeField] TextMeshProUGUI _tmproPlayerName;
    [SerializeField] Image _turnIndicator;
    [SerializeField] UIPieceCount _smallPieces;
    [SerializeField] UIPieceCount _largePieces;

    private Color _colorTint;
    private Color _colorLight;

    public void Init(params object[] parameters) {
        GetComponent<Image>().color = AppConst.GetColor(ColorVariant.SuperTone, GlobalManager.Instance.PlayerValue);

        _tmproPlayerName.text = parameters[0] as string;
        int index = (int)parameters[1];
        _turnIndicator.transform.eulerAngles = new Vector3(0, 0, index * 45);
        _colorTint = AppConst.GetColor(ColorVariant.Tint, CommonUtils.PlayerValueFromIndex(index));
        _colorLight = AppConst.GetColor(ColorVariant.Light, CommonUtils.PlayerValueFromIndex(index));
        PlayerModel model = parameters[2] as PlayerModel;

        _smallPieces.UpdateCount(model.pieces[0]);
        _largePieces.UpdateCount(model.pieces[1]);
    }

    public void UpdateCount(PlayerModel model) {
        _smallPieces.UpdateCount(model.pieces[0]);
        _largePieces.UpdateCount(model.pieces[1]);
    }

    public void ShowIndicator(bool show) {
        StartCoroutine(IndicatorAnimCoroutine(show));
    }

    private IEnumerator IndicatorAnimCoroutine(bool show) {
        yield return Utils.BumpAnim(_turnIndicator.transform, 1.5f, show ? 1.2f : 0.8f);
        _turnIndicator.color = show ? _colorTint : _colorLight;
    }
}
