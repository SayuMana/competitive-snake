using UnityEngine;
using UnityEngine.UI;

namespace CompetitiveSnake
{
    [RequireComponent(typeof(Button))]
    public class DpadButton : MonoBehaviour
    {
        [SerializeField] Vector2Int direction;

        void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnPressed);
        }

        void OnPressed()
        {
            if (MatchController.Instance == null) return;
            MatchController.Instance.PlayerInput.Push(direction);
        }
    }
}
