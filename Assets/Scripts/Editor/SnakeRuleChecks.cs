using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CompetitiveSnake.EditorTools
{
    public static class SnakeRuleChecks
    {
        const string PrefKey = "CompetitiveSnake.ChecksVersion";
        const int ChecksVersion = 1;

        static int passed;
        static int failed;

        [InitializeOnLoadMethod]
        static void AutoRun()
        {
            if (EditorPrefs.GetInt(PrefKey, 0) >= ChecksVersion) return;

            EditorApplication.delayCall += () =>
            {
                RunAll();
                EditorPrefs.SetInt(PrefKey, ChecksVersion);
            };
        }

        [MenuItem("Tools/Competitive Snake/Run Rule Checks")]
        public static void RunAll()
        {
            passed = 0;
            failed = 0;

            CheckHeadOn();
            CheckSwap();
            CheckTailFollow();
            CheckWall();
            CheckBothDeadCompare();
            CheckTimerCompare();
            CheckReverseRejected();
            CheckBotLegality();

            Debug.Log("[Checks] done passed=" + passed + " failed=" + failed);
        }

        static void CheckHeadOn()
        {
            var board = new GridBoard(20, 20);
            var player = Snake(new Vector2Int(5, 5), Vector2Int.right, 4, SnakeSide.Player);
            var bot = Snake(new Vector2Int(7, 5), Vector2Int.left, 4, SnakeSide.Bot);

            new TurnResolver(board).Step(player, bot, new Vector2Int(0, 19), Vector2Int.zero, Vector2Int.zero);

            Check("head-on: both die", !player.Alive && !bot.Alive);
        }

        static void CheckSwap()
        {
            var board = new GridBoard(20, 20);
            var player = Snake(new Vector2Int(5, 5), Vector2Int.right, 4, SnakeSide.Player);
            var bot = Snake(new Vector2Int(6, 5), Vector2Int.left, 4, SnakeSide.Bot);

            new TurnResolver(board).Step(player, bot, new Vector2Int(0, 19), Vector2Int.zero, Vector2Int.zero);

            Check("swap: both die", !player.Alive && !bot.Alive);
        }

        static void CheckTailFollow()
        {
            var board = new GridBoard(20, 20);
            var cells = new[]
            {
                new Vector2Int(5, 5), new Vector2Int(5, 4),
                new Vector2Int(4, 4), new Vector2Int(4, 5)
            };

            var player = SnakeFrom(cells, Vector2Int.left, SnakeSide.Player);
            var bot = Snake(new Vector2Int(15, 15), Vector2Int.left, 4, SnakeSide.Bot);

            new TurnResolver(board).Step(player, bot, new Vector2Int(0, 19), Vector2Int.zero, Vector2Int.zero);

            Check("tail follow: stays alive", player.Alive && player.Head == new Vector2Int(4, 5));
        }

        static void CheckWall()
        {
            var board = new GridBoard(20, 20);
            var player = Snake(new Vector2Int(0, 0), Vector2Int.left, 2, SnakeSide.Player);
            var bot = Snake(new Vector2Int(15, 15), Vector2Int.left, 4, SnakeSide.Bot);

            new TurnResolver(board).Step(player, bot, new Vector2Int(0, 19), Vector2Int.zero, Vector2Int.zero);

            Check("wall: player dies", !player.Alive && bot.Alive);
        }

        static void CheckBothDeadCompare()
        {
            var board = new GridBoard(20, 20);
            var player = Snake(new Vector2Int(5, 5), Vector2Int.right, 5, SnakeSide.Player);
            var bot = Snake(new Vector2Int(6, 5), Vector2Int.left, 3, SnakeSide.Bot);

            new TurnResolver(board).Step(player, bot, new Vector2Int(0, 19), Vector2Int.zero, Vector2Int.zero);

            MatchOutcome outcome = WinConditionEvaluator.Evaluate(player, bot, 30f);

            Check("both dead: longer snake wins", !player.Alive && !bot.Alive && outcome == MatchOutcome.Win);
        }

        static void CheckTimerCompare()
        {
            var board = new GridBoard(20, 20);
            var player = Snake(new Vector2Int(5, 5), Vector2Int.right, 4, SnakeSide.Player);
            var bot = Snake(new Vector2Int(15, 15), Vector2Int.left, 6, SnakeSide.Bot);

            MatchOutcome outcome = WinConditionEvaluator.Evaluate(player, bot, 0f);

            Check("timer: shorter snake loses", outcome == MatchOutcome.Lose);
        }

        static void CheckReverseRejected()
        {
            var snake = Snake(new Vector2Int(5, 5), Vector2Int.right, 4, SnakeSide.Player);
            bool accepted = snake.TryQueueTurn(Vector2Int.left);

            Check("reverse turn rejected", !accepted);
        }

        static void CheckBotLegality()
        {
            var config = ScriptableObject.CreateInstance<SnakeConfig>();
            var brain = new BotBrain(config);
            var random = new System.Random(1234);

            int illegal = 0;
            int zeroWhileLegal = 0;

            for (int i = 0; i < 200; i++)
            {
                var board = new GridBoard(20, 20);
                var player = Snake(new Vector2Int(3, 10), RandomDirection(random), 4, SnakeSide.Player);
                var bot = Snake(new Vector2Int(16, 10), RandomDirection(random), 4, SnakeSide.Bot);
                board.PlaceFood(board.RandomFreeCell(player, bot));

                Vector2Int move = brain.DecideDirection(bot, player, board.Food, board);
                if (move == Vector2Int.zero)
                {
                    zeroWhileLegal++;
                    continue;
                }

                Vector2Int target = bot.Head + move;

                if (!board.InBounds(target)) illegal++;
                if (move == -bot.Direction) illegal++;
                if (GridBoard.Contains(bot, target) && target != bot.Tail) illegal++;
                if (GridBoard.Contains(player, target)) illegal++;
            }

            Check("bot: never illegal", illegal == 0);
            Check("bot: never idle on empty board", zeroWhileLegal == 0);
        }

        static Vector2Int RandomDirection(System.Random random)
        {
            int pick = random.Next(4);
            if (pick == 0) return Vector2Int.up;
            if (pick == 1) return Vector2Int.down;
            if (pick == 2) return Vector2Int.left;
            return Vector2Int.right;
        }

        static SnakeBody Snake(Vector2Int head, Vector2Int direction, int length, SnakeSide side)
        {
            var segments = new List<Vector2Int>(length);

            for (int i = 0; i < length; i++)
            {
                segments.Add(head - direction * i);
            }

            return new SnakeBody(side, segments, direction);
        }

        static SnakeBody SnakeFrom(Vector2Int[] cells, Vector2Int direction, SnakeSide side)
        {
            return new SnakeBody(side, new List<Vector2Int>(cells), direction);
        }

        static void Check(string name, bool condition)
        {
            if (condition)
            {
                passed++;
                Debug.Log("[Checks] PASS " + name);
                return;
            }

            failed++;
            Debug.LogError("[Checks] FAIL " + name);
        }
    }
}
