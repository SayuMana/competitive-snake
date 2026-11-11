using UnityEngine;

namespace CompetitiveSnake
{
    public class TouchPad : MonoBehaviour
    {
        [SerializeField] RectTransform up;
        [SerializeField] RectTransform down;
        [SerializeField] RectTransform left;
        [SerializeField] RectTransform right;

        void Update()
        {
            Vector2 position;
            if (!TryGetPress(out position)) return;

            if (RectTransformUtility.RectangleContainsScreenPoint(up, position, null)) Push(Vector2Int.up);
            else if (RectTransformUtility.RectangleContainsScreenPoint(down, position, null)) Push(Vector2Int.down);
            else if (RectTransformUtility.RectangleContainsScreenPoint(left, position, null)) Push(Vector2Int.left);
            else if (RectTransformUtility.RectangleContainsScreenPoint(right, position, null)) Push(Vector2Int.right);
        }

        static bool TryGetPress(out Vector2 position)
        {
            if (Input.GetMouseButtonDown(0))
            {
                position = Input.mousePosition;
                return true;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase != TouchPhase.Began) continue;

                position = touch.position;
                return true;
            }

            position = Vector2.zero;
            return false;
        }

        static void Push(Vector2Int direction)
        {
            if (MatchController.Instance == null) return;
            MatchController.Instance.PlayerInput.Push(direction);
        }
    }
}
