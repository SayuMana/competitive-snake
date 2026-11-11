using UnityEngine;

namespace CompetitiveSnake
{
    public struct TurnResult
    {
        public bool FoodEaten;
        public SnakeSide Eater;
        public bool PlayerDied;
        public bool BotDied;
    }

    public class TurnResolver
    {
        readonly GridBoard board;

        public TurnResolver(GridBoard board)
        {
            this.board = board;
        }

        public TurnResult Step(SnakeBody player, SnakeBody bot, Vector2Int food, Vector2Int playerDirection, Vector2Int botDirection)
        {
            var result = new TurnResult();

            if (player.Alive) player.TryQueueTurn(playerDirection);
            if (bot.Alive) bot.TryQueueTurn(botDirection);

            Vector2Int playerStep = player.Alive ? player.ConsumeTurn() : player.Direction;
            Vector2Int botStep = bot.Alive ? bot.ConsumeTurn() : bot.Direction;

            Vector2Int playerNewHead = player.Head + playerStep;
            Vector2Int botNewHead = bot.Head + botStep;

            bool playerGrows = player.Alive && playerNewHead == food;
            bool botGrows = bot.Alive && botNewHead == food;

            bool playerTailFrees = player.Alive && !playerGrows;
            bool botTailFrees = bot.Alive && !botGrows;

            bool playerDead = player.Alive && IsDead(playerNewHead, player, bot, playerTailFrees, botTailFrees);
            bool botDead = bot.Alive && IsDead(botNewHead, bot, player, botTailFrees, playerTailFrees);

            if (player.Alive && bot.Alive)
            {
                if (playerNewHead == botNewHead)
                {
                    playerDead = true;
                    botDead = true;
                }

                if (playerNewHead == bot.Head && botNewHead == player.Head)
                {
                    playerDead = true;
                    botDead = true;
                }
            }

            if (!playerDead) player.Advance(playerNewHead, playerGrows);
            if (!botDead) bot.Advance(botNewHead, botGrows);

            player.Alive = player.Alive && !playerDead;
            bot.Alive = bot.Alive && !botDead;

            if (playerGrows && !playerDead)
            {
                result.FoodEaten = true;
                result.Eater = SnakeSide.Player;
            }

            if (botGrows && !botDead)
            {
                result.FoodEaten = true;
                result.Eater = SnakeSide.Bot;
            }

            result.PlayerDied = playerDead;
            result.BotDied = botDead;
            return result;
        }

        bool IsDead(Vector2Int newHead, SnakeBody self, SnakeBody other, bool selfTailFrees, bool otherTailFrees)
        {
            if (!board.InBounds(newHead)) return true;
            if (HitsBody(newHead, self, selfTailFrees)) return true;
            if (HitsBody(newHead, other, otherTailFrees)) return true;
            return false;
        }

        static bool HitsBody(Vector2Int cell, SnakeBody snake, bool tailFrees)
        {
            int limit = snake.Segments.Count - (tailFrees ? 1 : 0);

            for (int i = 0; i < limit; i++)
            {
                if (snake.Segments[i] == cell) return true;
            }

            return false;
        }
    }
}
