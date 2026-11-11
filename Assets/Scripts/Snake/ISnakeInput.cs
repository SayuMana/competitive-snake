using UnityEngine;

namespace CompetitiveSnake
{
    public interface ISnakeInput
    {
        Vector2Int DecideDirection(SnakeBody self, SnakeBody opponent, Vector2Int food, GridBoard board);
    }
}
