using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CompetitiveSnake
{
    public class MatchController : MonoBehaviour
    {
        public static MatchController Instance { get; private set; }

        [SerializeField] SnakeConfig config;
        [SerializeField] BoardView view;
        [SerializeField] HudView hud;

        GridBoard board;
        SnakeBody player;
        SnakeBody bot;
        TurnResolver resolver;
        PlayerInput playerInput;
        BotBrain botBrain;
        readonly KeyboardInput keyboard = new KeyboardInput();

        MatchState state = MatchState.Ready;
        float tickTimer;
        float timeLeft;
        float stateEnterTime;

        public MatchState State => state;
        public MatchOutcome Outcome { get; private set; }
        public SnakeBody Player => player;
        public SnakeBody Bot => bot;
        public GridBoard Board => board;
        public PlayerInput PlayerInput => playerInput;
        public float TimeLeft => timeLeft;

        void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;

            playerInput = new PlayerInput();
            botBrain = new BotBrain(config);
        }

        void Start()
        {
            ResetMatch();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (state == MatchState.Playing)
            {
                keyboard.Poll(playerInput);
                RunTicks();
                return;
            }

            if (Time.unscaledTime - stateEnterTime < 0.4f) return;
            if (!ConfirmPressed()) return;

            if (state == MatchState.Ready) StartMatch();
            else RestartMatch();
        }

        void ResetMatch()
        {
            board = new GridBoard(config.gridWidth, config.gridHeight);
            player = CreateSnake(SnakeSide.Player, new Vector2Int(4, 4), Vector2Int.right);
            bot = CreateSnake(SnakeSide.Bot, new Vector2Int(15, 15), Vector2Int.left);
            resolver = new TurnResolver(board);

            view.Build(config.gridWidth, config.gridHeight);
            board.PlaceFood(board.RandomFreeCell(player, bot));
            view.Render(player, bot);
            view.RenderFood(board.Food);

            timeLeft = config.matchDuration;
            tickTimer = 0f;
            state = MatchState.Ready;
            stateEnterTime = Time.unscaledTime;

            hud.ShowReady();
            hud.SetTime(timeLeft);
            hud.SetLengths(player.Length, bot.Length);
        }

        SnakeBody CreateSnake(SnakeSide side, Vector2Int head, Vector2Int direction)
        {
            var segments = new List<Vector2Int>(config.startLength);

            for (int i = 0; i < config.startLength; i++)
            {
                segments.Add(head - direction * i);
            }

            return new SnakeBody(side, segments, direction);
        }

        void RunTicks()
        {
            float interval = 1f / Mathf.Max(1f, config.tickRate);

            timeLeft -= Time.deltaTime;
            tickTimer += Time.deltaTime;

            int guard = 0;
            while (tickTimer >= interval && guard < 4)
            {
                tickTimer -= interval;
                guard++;

                Tick();
                if (state != MatchState.Playing) break;
            }

            hud.SetTime(Mathf.Max(0f, timeLeft));
        }

        void Tick()
        {
            Vector2Int playerDirection = playerInput.DecideDirection(player, bot, board.Food, board);
            Vector2Int botDirection = botBrain.DecideDirection(bot, player, board.Food, board);

            TurnResult result = resolver.Step(player, bot, board.Food, playerDirection, botDirection);

            if (result.FoodEaten)
            {
                board.PlaceFood(board.RandomFreeCell(player, bot));
                view.RenderFood(board.Food);
            }

            view.Render(player, bot);
            hud.SetLengths(player.Length, bot.Length);

            MatchOutcome outcome = WinConditionEvaluator.Evaluate(player, bot, timeLeft);
            if (outcome != MatchOutcome.Ongoing) Finish(outcome);
        }

        public void StartMatch()
        {
            state = MatchState.Playing;
            stateEnterTime = Time.unscaledTime;
            hud.ShowPlaying();
        }

        void Finish(MatchOutcome outcome)
        {
            state = MatchState.Finished;
            Outcome = outcome;
            stateEnterTime = Time.unscaledTime;
            hud.ShowResult(outcome, player.Length, bot.Length);
        }

        void RestartMatch()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        static bool ConfirmPressed()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) return true;
            if (Input.GetMouseButtonDown(0)) return true;

            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began) return true;
            }

            return false;
        }
    }
}
