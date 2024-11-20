using UnityEngine;

public class InputManager : MonoBehaviour {
    #region Variables
    private Camera _camera;
    private BoardSquare _currentSquare;
    private ControllerBoard _controllerBoard;
    #endregion


    public void Init() {
        _camera = Camera.main;
        _controllerBoard = GetComponent<ControllerBoard>();
    }

    private void Update() {
        if (_camera == null) {
            Utils.Log(this, "Update", "_camera is null");
            return;
        }

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 200);
        BoardSquare target = null;

        foreach (RaycastHit hit in hits) {
            if (hit.collider == null)
                continue;

            GameObject go = hit.collider.gameObject;
            target = go.GetComponent<BoardSquare>();

            if (target != null)
                break;
        }

        if (target != _currentSquare) {
            _currentSquare?.Highlight(false);
            target?.Highlight(true);
        }

        _currentSquare = target;

        if (_currentSquare == null)
            return;

        if (Input.GetMouseButtonDown(0))
            _controllerBoard.Click(_currentSquare.Pos, false);
        else if (Input.GetMouseButtonDown(1))
            _controllerBoard.Click(_currentSquare.Pos, true);
    }
}
