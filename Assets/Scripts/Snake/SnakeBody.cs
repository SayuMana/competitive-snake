using System.Collections.Generic;
using UnityEngine;

namespace CompetitiveSnake
{
    public class SnakeBody
    {
        const int MaxQueuedTurns = 2;

        readonly Queue<Vector2Int> turnQueue = new Queue<Vector2Int>();

        public readonly SnakeSide Side;
        public readonly List<Vector2Int> Segments = new List<Vector2Int>();

        public Vector2Int Direction { get; private set; }
        public bool Alive = true;

        public SnakeBody(SnakeSide side, List<Vector2Int> segments, Vector2Int direction)
        {
            Side = side;
            Segments.AddRange(segments);
            Direction = direction;
        }

        public Vector2Int Head => Segments[0];
        public Vector2Int Tail => Segments[Segments.Count - 1];
        public int Length => Segments.Count;

        public bool TryQueueTurn(Vector2Int direction)
        {
            if (direction == Vector2Int.zero) return false;
            if (Mathf.Abs(direction.x) + Mathf.Abs(direction.y) != 1) return false;

            Vector2Int reference = turnQueue.Count > 0 ? LastQueued() : Direction;
            if (direction == reference) return false;
            if (direction == -reference) return false;
            if (turnQueue.Count >= MaxQueuedTurns) return false;

            turnQueue.Enqueue(direction);
            return true;
        }

        public Vector2Int ConsumeTurn()
        {
            if (turnQueue.Count == 0) return Direction;

            Direction = turnQueue.Dequeue();
            return Direction;
        }

        public void Advance(Vector2Int newHead, bool grow)
        {
            Segments.Insert(0, newHead);
            if (!grow) Segments.RemoveAt(Segments.Count - 1);
        }

        Vector2Int LastQueued()
        {
            Vector2Int last = Direction;
            foreach (var direction in turnQueue) last = direction;
            return last;
        }
    }
}
