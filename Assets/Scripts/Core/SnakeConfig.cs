using UnityEngine;

namespace CompetitiveSnake
{
    [CreateAssetMenu(menuName = "Competitive Snake/Config", fileName = "SnakeConfig")]
    public class SnakeConfig : ScriptableObject
    {
        [Header("Board")]
        public int gridWidth = 20;
        public int gridHeight = 20;

        [Header("Match")]
        public float tickRate = 8f;
        public float matchDuration = 60f;
        public int startLength = 4;

        [Header("Bot")]
        public int botSafetyMargin = 2;

        [Header("Layout")]
        public float bottomUiFraction = 0.36f;
        public float topUiFraction = 0.12f;
    }
}
