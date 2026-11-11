using UnityEngine;
using UnityEngine.UI;

namespace CompetitiveSnake
{
    public class HudView : MonoBehaviour
    {
        [SerializeField] Text timeText;
        [SerializeField] Text lengthText;
        [SerializeField] GameObject readyPanel;
        [SerializeField] GameObject resultPanel;
        [SerializeField] Text resultText;
        [SerializeField] Text resultDetailText;

        public void ShowReady()
        {
            readyPanel.SetActive(true);
            resultPanel.SetActive(false);
        }

        public void ShowPlaying()
        {
            readyPanel.SetActive(false);
            resultPanel.SetActive(false);
        }

        public void ShowResult(MatchOutcome outcome, int playerLength, int botLength)
        {
            resultPanel.SetActive(true);

            switch (outcome)
            {
                case MatchOutcome.Win:
                    resultText.text = "YOU WIN";
                    break;
                case MatchOutcome.Lose:
                    resultText.text = "YOU LOSE";
                    break;
                default:
                    resultText.text = "DRAW";
                    break;
            }

            resultDetailText.text = "YOU " + playerLength + "   BOT " + botLength;
        }

        public void SetTime(float seconds)
        {
            int total = Mathf.CeilToInt(Mathf.Max(0f, seconds));
            timeText.text = total.ToString();
        }

        public void SetLengths(int playerLength, int botLength)
        {
            lengthText.text = "YOU " + playerLength + "   BOT " + botLength;
        }
    }
}
