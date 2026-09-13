using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SOSGame.Logic;
using SOSGame.Controller;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using System.Collections.Generic;
using Avalonia.Threading;


namespace SOSGame.GUI
{
    public partial class MainWindow : Window
    {
        private GameController? gameController;
        public GameController GameController => gameController!;

        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            InitializeComponent();

            GameGrid.SetParent(this);
            GameGrid.OnScoreUpdated = (p1, p2) => UpdateScoreboard(p1, p2);
            GameGrid.OnTurnChanged = isP1 => UpdatePlayerTurnDisplay(isP1);
            GameGrid.OnLetterCleared = () => ClearLetterSelection();
            GameGrid.OnBannerDisplayed = msg => ShowVictoryBanner(msg);

            StartGameButton.Click += (_, __) => StartGame();
            NewGameButton.Click += (_, __) => NewGame();

            ReplayButton.Click += async (_, __) => await ReplayGame();

            SaveScoreCheckBox.IsCheckedChanged += async (_, __) =>
            {
                if (SaveScoreCheckBox.IsChecked == true)
                    await ShowSaveFileDialog();
            };

            ScoreBoard.Text = "Red Player: [ 0 | 0 ] : Blue Player";
            WireUpEnableLogic();
        }

        // Start the replay with the loaded moves
        private async Task StartReplayGame(string filePath)
        {
            //  Load saved data
            var result = await GameController.LoadGameFromCSV(filePath);
            int gridSize = result.gridSize;
            string gameMode = result.gameMode;
            var movesList = result.moves;

            // Create a fresh controller instance
            gameController = new GameController(gridSize, gameMode,
                redIsCpu: false, blueIsCpu: false, 'S', 'O', msg => ShowVictoryBanner(msg));

            // Hook up your UI update logic to OnMoveMade
            gameController.OnMoveMade += (r, c, l) =>
            {
                GameGrid.SetCellContent(r, c, l);
                GameGrid.RefreshOverlay();
                UpdateScoreboard(gameController.PlayerOneScore, gameController.PlayerTwoScore);
                UpdatePlayerTurnDisplay(gameController.IsPlayerOneTurn());
            };

            // Reset the board size & logic so grid is ready
            GameGrid.AllowUnsafeGridSize = false;
            GameGrid.SetGridSize(gridSize);
            GameGrid.SetGameLogic(gameController.GameLogic);

            // Kick off the replay
            await gameController.ReplayGameAsync(movesList);

            // Final UI sync
            Dispatcher.UIThread.Post(() =>
            {
                UpdateScoreboard(gameController.PlayerOneScore, gameController.PlayerTwoScore);
                UpdatePlayerTurnDisplay(gameController.IsPlayerOneTurn());
            });
        }

        // Replay Button Click Event
        private async Task ReplayGame()
        {
            // Open the file dialog to select a CSV file
            var result = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Saved Game CSV",
                FileTypeFilter = new[] { new FilePickerFileType("CSV Files") { Patterns = new[] { "*.csv" } } },
                AllowMultiple = false
            });

            if (result != null && result.Count > 0)
            {
                string filePath = result[0].Path.LocalPath;

                // Start replay with the selected file path
                await StartReplayGame(filePath);
            }
        }

        public bool IsGeneralGameModeSelected =>
            GeneralGameRadio.IsChecked == true;

        public bool IsVulnerableGameSelected =>
            VulnerableGameRadio.IsChecked == true;

        private void WireUpEnableLogic()
        {
            redCpuOptionsPanel.IsVisible = true;
            blueCpuOptionsPanel.IsVisible = true;

            void refresh()
            {
                bool valid = IsSelectionValid();
                StartGameButton.IsEnabled = valid;
                NewGameButton.IsEnabled = valid;
            }

            playerVsPlayerRadio.IsCheckedChanged += (_, __) =>
            {
                redHumanRadio.IsChecked = true;
                blueHumanRadio.IsChecked = true;
                redCpuRadio.IsChecked = false;
                blueCpuRadio.IsChecked = false;
                refresh();
            };

            playerVsAIRadio.IsCheckedChanged += (_, __) =>
            {
                redHumanRadio.IsChecked = false;
                redCpuRadio.IsChecked = false;
                blueHumanRadio.IsChecked = false;
                blueCpuRadio.IsChecked = false;
                refresh();
            };

            cpuVsCpuRadio.IsCheckedChanged += (_, __) =>
            {
                redCpuRadio.IsChecked = true;
                blueCpuRadio.IsChecked = true;
                redHumanRadio.IsChecked = false;
                blueHumanRadio.IsChecked = false;
                refresh();
            };

            redHumanRadio.IsCheckedChanged += (_, __) => refresh();
            redCpuRadio.IsCheckedChanged += (_, __) => refresh();
            blueHumanRadio.IsCheckedChanged += (_, __) => refresh();
            blueCpuRadio.IsCheckedChanged += (_, __) => refresh();

            SimpleGameRadio.IsCheckedChanged += (_, __) => refresh();
            GeneralGameRadio.IsCheckedChanged += (_, __) => refresh();
            VulnerableGameRadio.IsCheckedChanged += (_, __) => refresh();
        }

        private void StartGame()
        {
            Debug.WriteLine("[DEBUG] StartGame() triggered.");
            ResetBanner();
            ClearLetterSelection();

            int gridSize;

            if (IsVulnerableGameSelected)
            {
                if (!int.TryParse(GridSizeInput.Text, out gridSize))
                {
                    ShowErrorDialog("Invalid board size input.");
                    return;
                }
            }
            else
            {
                if (!int.TryParse(GridSizeInput.Text, out gridSize)
                    || gridSize < 3 || gridSize > 12)
                {
                    ShowErrorDialog("Invalid board size input. Please enter a number between 3 and 12.");
                    return;
                }
            }

            GameGrid.AllowUnsafeGridSize = IsVulnerableGameSelected;
            GameGrid.SetGridSize(gridSize);

            bool redIsCpu = redCpuRadio.IsChecked == true;
            bool blueIsCpu = blueCpuRadio.IsChecked == true;

            if (cpuVsCpuRadio.IsChecked == true)
            {
                redIsCpu = true;
                blueIsCpu = true;
            }

            string gameMode =
            IsVulnerableGameSelected ? "VulnerableGameState" :
            (IsGeneralGameModeSelected ? "General" : "Simple");


            gameController = new GameController(
                gridSize,
                gameMode,
                redIsCpu,
                blueIsCpu,
                'S',
                IsGeneralGameModeSelected ? 'S' : 'O',
                msg => ShowVictoryBanner(msg)
            );


            gameController.OnMoveMade += (r, c, l) =>
            {
                GameGrid.SetCellContent(r, c, l);
                GameGrid.RefreshOverlay();
                UpdateScoreboard(
                    gameController.GetPlayerOneScore(),
                    gameController.GetPlayerTwoScore());
                UpdatePlayerTurnDisplay(gameController.IsPlayerOneTurn());
            };

            GameGrid.SetGameLogic(gameController.GameLogic);

            if (((redIsCpu && gameController.CurrentPlayer == 'R') ||
                 (blueIsCpu && gameController.CurrentPlayer == 'B'))
                && !gameController.IsGameOver())
            {
                gameController.TryAutoPlayNext();
            }

            ResetBanner();
            UpdateScoreboard(0, 0);
            UpdatePlayerTurnDisplay(gameController.IsPlayerOneTurn());
        }

        private void NewGame() => StartGame();

        public char? GetSelectedLetter()
        {
            if (gameController != null)
            {
                if (gameController.CurrentPlayer == 'R' && redCpuRadio.IsChecked == true)
                    return 'S';
                if (gameController.CurrentPlayer == 'B' && blueCpuRadio.IsChecked == true)
                    return 'O';
            }

            bool s = SelectSCheckBox.IsChecked == true;
            bool o = SelectOCheckBox.IsChecked == true;
            if (s == o)
            {
                ShowErrorDialog("Please select exactly one of S or O.");
                return null;
            }
            return s ? 'S' : 'O';
        }

        public void ClearLetterSelection()
        {
            SelectSCheckBox.IsChecked = false;
            SelectOCheckBox.IsChecked = false;
        }

        public void ShowVictoryBanner(string message)
        {
            VictoryBannerText.Text = message;
            VictoryBannerContainer.IsVisible = true;
            playerTurnDisplay.IsVisible = false;
        }

        public void ResetBanner()
        {
            VictoryBannerContainer.IsVisible = false;
            playerTurnDisplay.IsVisible = true;
        }

        public void UpdateScoreboard(int redScore, int blueScore)
        {
            ScoreBoard.Text = $"Red Player: [ {redScore} | {blueScore} ] : Blue Player";
        }


        public void UpdatePlayerTurnDisplay(bool isP1)
        {
            playerTurnDisplay.Text = isP1
                ? "Player 1's Turn (Red)"
                : "Player 2's Turn (Blue)";
            playerTurnDisplay.Foreground = isP1
                ? Brushes.Red : Brushes.Blue;
        }

        private bool IsSelectionValid()
        {
            bool modeOK = SimpleGameRadio.IsChecked == true
                       || GeneralGameRadio.IsChecked == true
                       || VulnerableGameRadio.IsChecked == true;

            bool sideOK =
                 (playerVsPlayerRadio.IsChecked == true
                    && redHumanRadio.IsChecked == true
                    && blueHumanRadio.IsChecked == true)
              || (playerVsAIRadio.IsChecked == true
                    && ((redHumanRadio.IsChecked == true && blueCpuRadio.IsChecked == true)
                     || (redCpuRadio.IsChecked == true && blueHumanRadio.IsChecked == true)))
              || (cpuVsCpuRadio.IsChecked == true
                    && redCpuRadio.IsChecked == true
                    && blueCpuRadio.IsChecked == true);

            bool sizeOK = int.TryParse(GridSizeInput.Text, out int sz) && sz >= 3;

            return modeOK && sideOK && sizeOK;
        }

        public async void ShowErrorDialog(string message)
        {
            var dlg = new Window
            {
                Title = "Error",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var txt = new TextBlock { Text = message, Margin = new Thickness(10) };
            var btn = new Button
            {
                Content = "OK",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            btn.Click += (_, __) => dlg.Close();
            dlg.Content = new StackPanel { Children = { txt, btn }, Spacing = 10 };
            await dlg.ShowDialog(this);
        }

        private async Task ShowSaveFileDialog()
        {
            if (StorageProvider != null && gameController != null)
            {
                // Open the file save picker to let the user choose the file path
                var file = await StorageProvider
                    .SaveFilePickerAsync(new FilePickerSaveOptions
                    {
                        Title = "Save Game Score",
                        SuggestedFileName = "sos_game_score.csv",
                        FileTypeChoices = new[]
                        {
                    new FilePickerFileType("CSV")
                    {
                        Patterns = new[] { "*.csv" },
                        MimeTypes = new[] { "text/csv" }
                    }
                        }
                    });

                // If the user selected a file location, attempt to save the game state
                if (file != null)
                {
                    bool fileSavedSuccessfully = false;
                    int retries = 5;

                    while (!fileSavedSuccessfully && retries > 0)
                    {
                        try
                        {
                            await gameController.SaveGameAsync(file.Path.LocalPath);

                            Dispatcher.UIThread.Post(() =>
                            {
                                UpdateScoreboard(gameController.PlayerOneScore, gameController.PlayerTwoScore);
                                UpdatePlayerTurnDisplay(gameController.IsPlayerOneTurn());
                            });

                            fileSavedSuccessfully = true;  
                        }
                        catch (IOException ex)
                        {
                            retries--;

                            if (retries == 0)
                            {
                                await ShowErrorMessageAsync($"Failed to save the file: {ex.Message}. Please try again later.");
                            }
                            else
                            {
                                await Task.Delay(500);  // Wait for half a second before retrying
                            }
                        }
                    }
                }
            }
        }



        // Helper method to show error message
        private async Task ShowErrorMessageAsync(string message)
        {
            // Display the error message in a simple dialog
            var errorDialog = new Window
            {
                Title = "Error",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var textBlock = new TextBlock { Text = message, Margin = new Thickness(10) };
            var button = new Button
            {
                Content = "OK",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            button.Click += (_, __) => errorDialog.Close();
            errorDialog.Content = new StackPanel { Children = { textBlock, button }, Spacing = 10 };
            await errorDialog.ShowDialog(this);  // Show the dialog to the user
        }


    }
}
