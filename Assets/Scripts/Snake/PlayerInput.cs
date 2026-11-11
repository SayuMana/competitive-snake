using UnityEngine;

namespace CompetitiveSnake
{
    public class PlayerInput : ISnakeInput
    {
        Vector2Int pending;

        public void Push(Vector2Int direction)
        {
            pending = direction;
        }

        public Vector2Int DecideDirection(SnakeBody self, SnakeBody opponent, Vector2Int food, GridBoard board)
        {
            Vector2Int direction = pending;
            pending = Vector2Int.zero;
            return direction;
        }
    }
}
