using System;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SOSGame.Logic;
using SOSGame.Logic.Players;

namespace SOSGame.Controller
{
    public class GameController
    {
        // Triggered after every placement (human or AI)
        public event Action<int, int, char>? OnMoveMade;

        private readonly BaseGame gameLogic;
        public BaseGame GameLogic => gameLogic;

        public int GridSize { get; }
        public string GameMode { get; }
        public string PlayerMode { get; private set; } = "";
        public char CurrentPlayer { get; private set; } = 'R';   // 'R' = Red, 'B' = Blue

        // Record move history
        public record ScoreEvent(char Player, int Row, int Col, char Letter);

        public List<ScoreEvent> MoveHistory { get; } = new List<ScoreEvent>();  // track all the moves
        public int PlayerOneScore => gameLogic.GetPlayerOneScore();             // track player 1 score
        public int PlayerTwoScore => gameLogic.GetPlayerTwoScore();

        private readonly IGamePlayer redPlayer;
        private readonly IGamePlayer bluePlayer;
        private readonly Action<string>? bannerCallback;

        // Is the side whose turn it is now controlled by the CPU?
        public bool CurrentPlayerIsCpu
        {
            get
            {
                var current = gameLogic.IsPlayerOneTurn() ? redPlayer : bluePlayer;
                return current is ComputerPlayer;
            }
        }

        //  What letter will the side whose turn it is now place?
        public char CurrentPlayerLetter
        {
            get
            {
                var current = gameLogic.IsPlayerOneTurn() ? redPlayer : bluePlayer;
                return current.DefaultLetter;
            }
        }

        // Human vs Human convenience ctor
        public GameController(int gridSize, string gameMode)
            : this(gridSize, gameMode, false, false, 'S', 'O', null) { }

        // Full constructor: choose CPU/human for each side, assign letters, & supply banner callback.
        public GameController(
            int gridSize,
            string gameMode,
            bool redIsCpu,
            bool blueIsCpu,
            char redLetter,
            char blueLetter,
            Action<string>? bannerCallback = null)
        {
            if (gameMode != "VulnerableGameState" && (gridSize < 3 || gridSize > 12))
                throw new ArgumentException("Board size must be between 3 and 12.");


            GridSize = gridSize;
            GameMode = gameMode;
            this.bannerCallback = bannerCallback;

            // create board logic and pipe banner messages up
            gameLogic = gameMode switch
            {
                "Simple" => new SimpleGame(gridSize, msg => bannerCallback?.Invoke(msg)),
                "General" => new GeneralGame(gridSize, msg => bannerCallback?.Invoke(msg)),
                "VulnerableGameState" => new VulnerableGame(gridSize),
                _ => throw new ArgumentException("Invalid game mode")
            };

            // player mode label
            PlayerMode = redIsCpu && blueIsCpu
                       ? "Computer vs Computer"
                       : (redIsCpu || blueIsCpu)
                         ? "Human vs Computer"
                         : "Human vs Human";

            // instantiate player strategies
            redPlayer = redIsCpu ? new ComputerPlayer(redLetter) : new HumanPlayer(redLetter);
            bluePlayer = blueIsCpu ? new ComputerPlayer(blueLetter) : new HumanPlayer(blueLetter);
        }

        // Save Game Method
        public async Task SaveGameAsync(string filePath)
        {
            var sb = new StringBuilder();

            // Save Player Names, Scores, Grid Size, Game Mode, and Player Mode
            sb.AppendLine("Player,Score,Color");
            sb.AppendLine($"Red Player,{PlayerOneScore},Red");
            sb.AppendLine($"Blue Player,{PlayerTwoScore},Blue");
            sb.AppendLine();  // Blank line to separate sections

            // Save grid size, game mode, and player mode
            sb.AppendLine($"GridSize,{GridSize}");
            sb.AppendLine($"GameMode,{GameMode}");
            sb.AppendLine($"PlayerMode,{PlayerMode}");
            sb.AppendLine();  // Blank line to separate sections

            // Record Move History (Player, Row, Column, Letter)
            sb.AppendLine("Player,Row,Column,Letter");
            foreach (var move in MoveHistory)
            {
                string playerName = (move.Player == 'R') ? "Red Player" : "Blue Player";
                sb.AppendLine($"{playerName},{move.Row},{move.Col},{move.Letter}");
            }

            // Use FileStream with FileMode.OpenOrCreate to avoid file access issues
            using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(sb.ToString());
            }
        }



        // Load Game from CSV Method
        public static async Task<(int gridSize, string gameMode, string playerMode, List<ScoreEvent> moves)> LoadGameFromCSV(string filePath)
        {
            var moves = new List<ScoreEvent>();
            var metadata = new Dictionary<string, string>();

            // Using FileStream with FileShare.ReadWrite to allow other processes to read and write the file
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
            {
                string line;
                bool isMoveSection = false;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (!isMoveSection)
                    {
                        if (line.StartsWith("Player,Row"))
                        {
                            // We have reached the move section, so stop reading metadata
                            isMoveSection = true;
                            continue;
                        }

                        // Parse metadata
                        ParseMetadata(line, metadata);
                    }
                    else
                    {
                        // Process the move section
                        ParseMove(line, moves);
                    }
                }
            }


            // Get metadata values with defaults
            int gridSize = int.TryParse(metadata.GetValueOrDefault("GridSize", "3"), out var parsedGridSize) ? parsedGridSize : 3;
            string gameMode = metadata.GetValueOrDefault("GameMode", "Simple");
            string playerMode = metadata.GetValueOrDefault("PlayerMode", "Human vs Human");

            return (gridSize, gameMode, playerMode, moves);
        }

        // Helper method to parse metadata lines
        private static void ParseMetadata(string line, Dictionary<string, string> metadata)
        {
            var parts = line.Split(',');
            if (parts.Length == 2)
            {
                metadata[parts[0]] = parts[1];
            }
        }

        // Helper method to parse move lines
        private static void ParseMove(string line, List<ScoreEvent> moves)
        {
            var parts = line.Split(',');
            if (parts.Length == 4)
            {
                string playerName = parts[0];
                int row = int.Parse(parts[1]);
                int col = int.Parse(parts[2]);
                char letter = parts[3][0]; // First character of letter (S or O)
                char playerChar = playerName.StartsWith("Red") ? 'R' : 'B';
                moves.Add(new ScoreEvent(playerChar, row, col, letter));
            }
        }

        // Replay Game Method
        public async Task ReplayGameAsync(List<ScoreEvent> moves)
        {
            MoveHistory.Clear();
            foreach (var move in moves)
            {
                if (!MakeMove(move.Row, move.Col, move.Letter))
                    break;
                await Task.Delay(500);
            }
        }


        // Human move entry point. Raises OnMoveMade, switches turn, and auto‑plays any AI turns.
        public bool MakeMove(int row, int col, char letter)
        {
            if (!gameLogic.MakeMove(row, col, letter))
                return false;

            // Log the move to MoveHistory
            MoveHistory.Add(new ScoreEvent(CurrentPlayer, row, col, letter));

            OnMoveMade?.Invoke(row, col, letter);  // Notify UI

            if (!gameLogic.IsGameOver())
            {
                SwitchTurn();
                TryAutoPlayNext();  // If game isn't over, trigger AI turn
            }
            return true;
        }


        // Runs consecutive AI turns until next human or game end, pausing between moves.
        public async Task TryAutoPlayNext()
        {
            int guard = 0;
            while (!gameLogic.IsGameOver())
            {
                IGamePlayer current = gameLogic.IsPlayerOneTurn() ? redPlayer : bluePlayer;
                if (current is HumanPlayer)
                    break;  // Exit if it's a human player's turn

                var (r, c, l) = current.GetNextMoveWithLetter(gameLogic);

                if (!gameLogic.MakeMove(r, c, l))
                    break;

                // Log the computer move to MoveHistory
                MoveHistory.Add(new ScoreEvent(CurrentPlayer, r, c, l));

                OnMoveMade?.Invoke(r, c, l);

                await Task.Delay(TimeSpan.FromSeconds(1));  // Delay for the AI move

                SwitchTurn();

                // Safety that prevent infinite loop if board is too large
                if (++guard > gameLogic.GetBoard().Length)
                    break;
            }
        }


        private void SwitchTurn() =>
            CurrentPlayer = (CurrentPlayer == 'R') ? 'B' : 'R';



        public int GetPlayerOneScore() => gameLogic.GetPlayerOneScore();
        public int GetPlayerTwoScore() => gameLogic.GetPlayerTwoScore();
        public bool IsPlayerOneTurn() => gameLogic.IsPlayerOneTurn();
        public bool IsGameOver() => gameLogic.IsGameOver();
    }
}