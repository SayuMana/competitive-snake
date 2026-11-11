using System.Collections.Generic;
using UnityEngine;

namespace CompetitiveSnake
{
    public class GridBoard
    {
        public readonly int Width;
        public readonly int Height;

        public Vector2Int Food { get; private set; }

        public GridBoard(int width, int height)
        {
            Width = width;
            Height = height;
            Food = new Vector2Int(-1, -1);
        }

        public bool InBounds(Vector2Int cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;
        }

        public void PlaceFood(Vector2Int cell)
        {
            Food = cell;
        }

        public bool IsFree(Vector2Int cell, SnakeBody a, SnakeBody b)
        {
            if (!InBounds(cell)) return false;
            if (Contains(a, cell)) return false;
            if (Contains(b, cell)) return false;
            return true;
        }

        public Vector2Int RandomFreeCell(SnakeBody a, SnakeBody b)
        {
            var free = new List<Vector2Int>(Width * Height);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var cell = new Vector2Int(x, y);
                    if (IsFree(cell, a, b)) free.Add(cell);
                }
            }

            if (free.Count == 0) return new Vector2Int(-1, -1);
            return free[Random.Range(0, free.Count)];
        }

        public static bool Contains(SnakeBody snake, Vector2Int cell)
        {
            if (snake == null) return false;

            for (int i = 0; i < snake.Segments.Count; i++)
            {
                if (snake.Segments[i] == cell) return true;
            }

            return false;
        }
    }
}
