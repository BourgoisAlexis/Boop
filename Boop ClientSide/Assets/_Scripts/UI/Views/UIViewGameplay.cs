using DG.Tweening;
using UnityEngine;

public class UIViewGameplay : UIView {
    [SerializeField] private UIRules _rules;
    [SerializeField] private RectTransform _turnIndicator;
    [SerializeField] private UIPlayerLayout[] _layouts;

    protected override void Init(params object[] parameters) {
        base.Init(parameters);

        _rules.Init();

        BoardModel boardModel = parameters[0] as BoardModel;
        boardModel.onPlayerModelsUpdate += UpdateCounts;
        RoomModel roomModel = parameters[1] as RoomModel;

        for (int i = 0; i < roomModel.usernames.Length; i++)
            _layouts[i].Init(roomModel.usernames[i], i, boardModel.PlayerModels[i]);
    }

    private void UpdateCounts(PlayerModel[] models) {
        for (int i = 0; i < models.Length; i++)
            _layouts[i].UpdateCount(models[i]);
    }

    public void SetCurrentPlayer(int index) {
        Vector3 rotation = new Vector3(0, 0, index * 45);
        _turnIndicator.DORotate(rotation, AppConst.globalAnimDuration).SetEase(Ease.InExpo);

        for (int i = 0; i < _layouts.Length; i++)
            _layouts[i].ShowIndicator(i == index);
    }
}
