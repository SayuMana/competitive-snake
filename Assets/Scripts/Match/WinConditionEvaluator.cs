namespace CompetitiveSnake
{
    public static class WinConditionEvaluator
    {
        public static MatchOutcome Evaluate(SnakeBody player, SnakeBody bot, float timeLeft)
        {
            if (!player.Alive && !bot.Alive) return Compare(player, bot);
            if (!player.Alive) return MatchOutcome.Lose;
            if (!bot.Alive) return MatchOutcome.Win;
            if (timeLeft <= 0f) return Compare(player, bot);
            return MatchOutcome.Ongoing;
        }

        static MatchOutcome Compare(SnakeBody player, SnakeBody bot)
        {
            if (player.Length > bot.Length) return MatchOutcome.Win;
            if (player.Length < bot.Length) return MatchOutcome.Lose;
            return MatchOutcome.Draw;
        }
    }
}
