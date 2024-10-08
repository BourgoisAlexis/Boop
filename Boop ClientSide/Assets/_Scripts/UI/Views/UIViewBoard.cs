using System.Collections.Generic;
using UnityEngine;

public class UIViewBoard : MonoBehaviour {
    [SerializeField] private List<UIPieceCount> _counts = new List<UIPieceCount>();

    public void Init(BoardModel model) {
        model.onPlayerModelsUpdate += UpdateCounts;

        foreach (UIPieceCount count in _counts)
            count.Init();
    }

    private void UpdateCounts(PlayerModel[] models) {
        int index = 0;
        foreach (PlayerModel model in models) {
            _counts[index].UpdateCount(model.pieces[0]);
            _counts[index + 1].UpdateCount(model.pieces[1]);

            index += 2;
        }
    }
}
