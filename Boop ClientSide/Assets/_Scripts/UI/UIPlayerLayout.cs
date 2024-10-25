using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerLayout : MonoBehaviour {
    [SerializeField] TextMeshProUGUI _tmproPlayerName;
    [SerializeField] Image _turnIndicator;
    [SerializeField] UIPieceCount _smallPieces;
    [SerializeField] UIPieceCount _largePieces;

    public void Init(params object[] parameters) {
        _tmproPlayerName.text = parameters[0] as string;
        PlayerModel model = parameters[1] as PlayerModel;

        _smallPieces.UpdateCount(model.pieces[0]);
        _largePieces.UpdateCount(model.pieces[1]);
    }

    public void UpdateCount(PlayerModel model) {
        _smallPieces.UpdateCount(model.pieces[0]);
        _largePieces.UpdateCount(model.pieces[1]);
    }

    public void ShowIndicator(bool show) {
        _turnIndicator.color = show ? Color.yellow : new Color(0, 0, 0, 0);
    }
}
