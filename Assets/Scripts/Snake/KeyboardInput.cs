using UnityEngine;

namespace CompetitiveSnake
{
    public class KeyboardInput
    {
        public void Poll(PlayerInput target)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) target.Push(Vector2Int.up);
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) target.Push(Vector2Int.down);
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) target.Push(Vector2Int.left);
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) target.Push(Vector2Int.right);
        }
    }
}
