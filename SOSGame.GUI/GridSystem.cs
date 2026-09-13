using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using SOSGame.Logic;
using System.Linq;

namespace SOSGame.GUI
{
    public class GridSystem : Grid
    {
        private int gridSize = 3;
        private bool isPlayerOneTurn = true;
        private MainWindow? parentWindow;
        private BaseGame? game;

        // Overlay canvas for drawing SOS lines.
        private Canvas overlayCanvas = new Canvas
        {
            IsHitTestVisible = false
        };

        // Public callbacks
        public Action<int, int>? OnScoreUpdated;
        public Action<bool>? OnTurnChanged;
        public Action? OnLetterCleared;
        public Action<string>? OnBannerDisplayed;
        public bool AllowUnsafeGridSize { get; set; } = false;


        public GridSystem()
        {
            InitializeGrid();
        }

        public void SetParent(MainWindow parent)
        {
            parentWindow = parent;
        }

        public void SetGridSize(int newSize)
        {
            Console.WriteLine($"[DEBUG] SetGridSize called with value: {newSize}");
            Console.WriteLine($"[DEBUG] AllowUnsafeGridSize: {AllowUnsafeGridSize}");

            if (!AllowUnsafeGridSize && (newSize < 3 || newSize > 12))
            {
                Console.WriteLine("[DEBUG] Grid size check failed. Blocking value.");
                throw new ArgumentException("ERR: The grid size must be between \n 3 and 12.");
            }

            gridSize = newSize;

            Console.WriteLine("[DEBUG] Resetting board...");
            ResetBoard();

            Console.WriteLine("[DEBUG] Creating game object...");

            if (parentWindow != null)
            {
                if (parentWindow.IsVulnerableGameSelected)
                {
                    Console.WriteLine("[DEBUG] Instantiating VulnerableGame.");
                    game = new VulnerableGame(newSize);
                }
                else if (parentWindow.IsGeneralGameModeSelected)
                {
                    Console.WriteLine("[DEBUG] Instantiating GeneralGame.");
                    game = new GeneralGame(newSize, msg => OnBannerDisplayed?.Invoke(msg));
                }
                else
                {
                    Console.WriteLine("[DEBUG] Instantiating SimpleGame.");
                    game = new SimpleGame(newSize, msg => OnBannerDisplayed?.Invoke(msg));
                }
            }
            else
            {
                Console.WriteLine("[DEBUG] No parentWindow. Using fallback.");
                game = new SimpleGame(newSize);
            }
        }




        public void SetCellContent(int row, int col, char letter)
        {
            foreach (var child in Children)
            {
                if (child is Button btn &&
                    Grid.GetRow(btn) == row &&
                    Grid.GetColumn(btn) == col)
                {
                    btn.Content = letter.ToString();
                    btn.Foreground = Brushes.Black;
                    break;
                }
            }
        }

        public void RefreshOverlay()
        {
            if (game != null)
                UpdateOverlayCanvas(game);
        }

        public int GetGridSize() => gridSize;

        public void ResetBoard()
        {
            this.Children.Clear();
            this.ColumnDefinitions.Clear();
            this.RowDefinitions.Clear();
            overlayCanvas.Children.Clear();
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            this.Width = 400;
            this.Height = 400;

            for (int i = 0; i < gridSize; i++)
            {
                this.ColumnDefinitions.Add(new ColumnDefinition());
                this.RowDefinitions.Add(new RowDefinition());
            }

            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    var cellButton = new Button
                    {
                        Content = "",
                        Background = Brushes.White,
                        Foreground = Brushes.Black,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        FontSize = 24,
                        FontWeight = FontWeight.Bold,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
                    };

                    int r = row, c = col;
                    cellButton.Click += (sender, args) => OnCellClicked(cellButton, r, c);

                    Grid.SetRow(cellButton, row);
                    Grid.SetColumn(cellButton, col);
                    this.Children.Add(cellButton);
                }
            }

            Grid.SetRowSpan(overlayCanvas, gridSize);
            Grid.SetColumnSpan(overlayCanvas, gridSize);
            this.Children.Add(overlayCanvas);
        }


        public void SetGameLogic(BaseGame logic)
        {
            game = logic;
            overlayCanvas.Children.Clear();
            this.Children.Clear();
            this.ColumnDefinitions.Clear();
            this.RowDefinitions.Clear();
            InitializeGrid();
        }

        private void OnCellClicked(Button button, int row, int col)
        {
            if (parentWindow == null || game == null)
                return;

            if (game is SimpleGame sg && sg.IsFrozen)
                return;

            if (!string.IsNullOrEmpty(button.Content?.ToString()))
                return;

            char? selectedLetter = parentWindow.GetSelectedLetter();
            if (selectedLetter == null)
                return;

            // Synchronous MakeMove
            bool moveSuccess = parentWindow.GameController.MakeMove(row, col, selectedLetter.Value);
            if (!moveSuccess)
                return;

            button.Content = selectedLetter.Value;
            button.Foreground = Brushes.Black;

            OnScoreUpdated?.Invoke(game.GetPlayerOneScore(), game.GetPlayerTwoScore());
            isPlayerOneTurn = game.IsPlayerOneTurn();
            OnTurnChanged?.Invoke(isPlayerOneTurn);
            OnLetterCleared?.Invoke();

            UpdateOverlayCanvas(game);
        }

        private void UpdateOverlayCanvas(BaseGame baseGame)
        {
            overlayCanvas.Children.Clear();

            double cellWidth = this.Bounds.Width / gridSize;
            double cellHeight = this.Bounds.Height / gridSize;

            foreach (var line in baseGame.GetSOSLines())
            {
                double startX = (line.StartCol + 0.5) * cellWidth;
                double startY = (line.StartRow + 0.5) * cellHeight;
                double endX = (line.EndCol + 0.5) * cellWidth;
                double endY = (line.EndRow + 0.5) * cellHeight;

                var shapeLine = new Line
                {
                    StartPoint = new Point(startX, startY),
                    EndPoint = new Point(endX, endY),
                    StrokeThickness = 4,
                    Stroke = line.PlayerColor.Equals("Red", StringComparison.OrdinalIgnoreCase)
                                     ? Brushes.Red
                                     : Brushes.Blue
                };

                overlayCanvas.Children.Add(shapeLine);
            }
        }

    }
}



