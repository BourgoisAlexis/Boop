using UnityEngine;

public class InputManager : MonoBehaviour {
    private Camera _camera;
    private BoardSquare _currentSquare;
    private BoardController _boardController;

    public void Init() {
        _camera = Camera.main;
        _boardController = GetComponent<BoardController>();
    }

    private void Update() {
        if (_camera == null)
            return;

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
            _boardController.Click(_currentSquare.Pos, false);
        else if (Input.GetMouseButtonDown(1))
            _boardController.Click(_currentSquare.Pos, true);
    }
}
