using UnityEngine;

public class UIViewGameplay : UIView {
    [SerializeField] private UIRules _rules;
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
        string message = $"{(index == GlobalManager.Instance.PlayerIndex ? "your" : "opponent")} turn";
        GlobalManager.Instance.UINotificationManager.Show(message, 1000);

        for (int i = 0; i < _layouts.Length; i++)
            _layouts[i].ShowIndicator(i == index);
    }
}
