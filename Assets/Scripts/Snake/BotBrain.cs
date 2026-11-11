using System.Collections.Generic;
using UnityEngine;

namespace CompetitiveSnake
{
    public class BotBrain : ISnakeInput
    {
        static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        readonly SnakeConfig config;

        public BotBrain(SnakeConfig config)
        {
            this.config = config;
        }

        public Vector2Int DecideDirection(SnakeBody self, SnakeBody opponent, Vector2Int food, GridBoard board)
        {
            if (!self.Alive) return Vector2Int.zero;

            Vector2Int toFood = FirstStepTowards(self, opponent, board, food);
            if (toFood != Vector2Int.zero && IsSafe(self, opponent, board, toFood)) return toFood;

            return BestSurvivalMove(self, opponent, board);
        }

        Vector2Int FirstStepTowards(SnakeBody self, SnakeBody opponent, GridBoard board, Vector2Int target)
        {
            if (!board.InBounds(target)) return Vector2Int.zero;
            if (target == self.Head) return Vector2Int.zero;

            var visited = new bool[board.Width, board.Height];
            var cameFrom = new Vector2Int[board.Width, board.Height];
            var queue = new Queue<Vector2Int>();

            Vector2Int start = self.Head;
            visited[start.x, start.y] = true;
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                if (current == target) return Reconstruct(cameFrom, start, target);

                foreach (var direction in Directions)
                {
                    Vector2Int next = current + direction;
                    if (!board.InBounds(next)) continue;
                    if (visited[next.x, next.y]) continue;
                    if (IsBlocked(next, self, opponent, true)) continue;

                    visited[next.x, next.y] = true;
                    cameFrom[next.x, next.y] = current;
                    queue.Enqueue(next);
                }
            }

            return Vector2Int.zero;
        }

        static Vector2Int Reconstruct(Vector2Int[,] cameFrom, Vector2Int start, Vector2Int target)
        {
            Vector2Int current = target;
            int guard = 0;

            while (cameFrom[current.x, current.y] != start && guard++ < 4096)
            {
                current = cameFrom[current.x, current.y];
            }

            return current - start;
        }

        bool IsSafe(SnakeBody self, SnakeBody opponent, GridBoard board, Vector2Int direction)
        {
            Vector2Int newHead = self.Head + direction;
            if (!board.InBounds(newHead)) return false;

            bool grows = newHead == board.Food;
            if (IsBlocked(newHead, self, opponent, !grows)) return false;

            int space = FreeSpace(self, opponent, board, newHead, !grows);
            return space >= self.Length + config.botSafetyMargin;
        }

        Vector2Int BestSurvivalMove(SnakeBody self, SnakeBody opponent, GridBoard board)
        {
            Vector2Int best = Vector2Int.zero;
            int bestSpace = -1;

            foreach (var direction in Directions)
            {
                if (direction == -self.Direction) continue;

                Vector2Int newHead = self.Head + direction;
                if (!board.InBounds(newHead)) continue;

                bool grows = newHead == board.Food;
                if (IsBlocked(newHead, self, opponent, !grows)) continue;

                int space = FreeSpace(self, opponent, board, newHead, !grows);
                if (space <= bestSpace) continue;

                bestSpace = space;
                best = direction;
            }

            return best;
        }

        int FreeSpace(SnakeBody self, SnakeBody opponent, GridBoard board, Vector2Int start, bool selfTailFrees)
        {
            if (!board.InBounds(start)) return 0;

            var visited = new bool[board.Width, board.Height];
            var queue = new Queue<Vector2Int>();

            visited[start.x, start.y] = true;
            queue.Enqueue(start);

            int count = 0;

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                count++;

                foreach (var direction in Directions)
                {
                    Vector2Int next = current + direction;
                    if (!board.InBounds(next)) continue;
                    if (visited[next.x, next.y]) continue;
                    if (IsBlocked(next, self, opponent, selfTailFrees)) continue;

                    visited[next.x, next.y] = true;
                    queue.Enqueue(next);
                }
            }

            return count;
        }

        static bool IsBlocked(Vector2Int cell, SnakeBody self, SnakeBody opponent, bool selfTailFrees)
        {
            if (GridBoard.Contains(self, cell))
            {
                if (!(selfTailFrees && cell == self.Tail)) return true;
            }

            if (GridBoard.Contains(opponent, cell)) return true;

            return false;
        }
    }
}
