using TMPro;
using UnityEngine;

public class UIPieceCount : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _tmproCount;

    public void Init() {
        _tmproCount.text = "0";
    }

    public void UpdateCount(int count) {
        _tmproCount.text = count.ToString();
    }
}
