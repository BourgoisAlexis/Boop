using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;

public class Loading : MonoBehaviour {
    [SerializeField] private GameObject _visual;
    [SerializeField] private GameObject _icon;

    private void Awake() {
        Anim();
    }

    public void Load(bool loading) {
        _visual.SetActive(loading);
    }

    private async void Anim() {
        while (true) {
            _icon.transform.localEulerAngles = Vector3.zero;
            await _icon.transform.DORotate(new Vector3(0, 0, -180), AppConst.globalAnimDuration * 3).SetEase(Ease.InOutSine).AsyncWaitForCompletion();
            await Task.Delay(500);
        }
    }
}
