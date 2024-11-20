using UnityEngine;
using UnityEngine.UI;

public class TexturedCanvas : MonoBehaviour {
    [SerializeField] private Image _image;

    public void Init() {
        _image.material.SetColor("_ColorTint", AppConst.GetColor(ColorVariant.SuperTone, GlobalManager.Instance.PlayerValue));
    }
}
