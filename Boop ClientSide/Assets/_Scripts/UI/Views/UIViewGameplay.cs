using UnityEngine;

public class UIViewGameplay : UIView {
    [SerializeField] private UIPlayerLayout[] _layouts;

    protected override void Init(params object[] parameters) {
        base.Init(parameters);

        BoardModel model = (BoardModel)parameters[0];
        model.onPlayerModelsUpdate += UpdateCounts;

        foreach (UIPlayerLayout l in _layouts)
            l.Init();
    }

    private void UpdateCounts(PlayerModel[] models) {
        for (int i = 0; i < models.Length; i++)
            _layouts[i].UpdateCount(models[i]);
    }

    public void SetCurrentPlayer(int index) {
        for (int i = 0; i < _layouts.Length; i++)
            _layouts[i].ShowIndicator(i == index);
    }
}
