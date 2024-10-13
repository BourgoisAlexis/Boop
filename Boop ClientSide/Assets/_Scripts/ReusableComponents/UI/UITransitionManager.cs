using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;

public class UITransitionManager : MonoBehaviour {
    #region Variables
    [SerializeField] private Transform _background;
    [SerializeField] private Transform[] _stripes;

    [SerializeField] private float[] _stripesPos;
    private CanvasGroup _canvasGroup;
    private int _delay = 50;
    #endregion


    private void Awake() {
        _canvasGroup = GetComponent<CanvasGroup>();

        int n = _stripes.Length;
        _stripesPos = new float[n];

        for (int i = 0; i < n; i++)
            _stripesPos[i] = _stripes[i].localPosition.x;

        Hide(true);
    }


    public async Task Show() {
        _background.gameObject.SetActive(true);
        await Task.Delay(_delay);
        _background.gameObject.SetActive(false);
        await Task.Delay(_delay);
        _background.gameObject.SetActive(true);
        await Task.Delay(_delay);

        for (int i = _stripes.Length - 1; i > -1; i--) {
            AnimStripe(_stripes[i], _stripesPos[i]);
            await Task.Delay(_delay);
        }
    }

    public async Task Hide(bool instant = false) {
        if (instant) {
            _background.gameObject.SetActive(false);
            foreach (Transform t in _stripes)
                t.localPosition = Vector3.left * 2000;

            return;
        }

        for (int i = 0; i < _stripes.Length; i++) {
            AnimStripe(_stripes[i], -2000);
            await Task.Delay(_delay);
        }

        _background.gameObject.SetActive(false);
    }


    private void AnimStripe(Transform t, float destination) {
        if (t == null) {
            CommonUtils.ErrorOnParams("UITransitionManager", "AnimeStripe");
            return;
        }

        t.DOLocalMoveX(destination, 0.08f).SetEase(Ease.InOutSine);
    }
}
