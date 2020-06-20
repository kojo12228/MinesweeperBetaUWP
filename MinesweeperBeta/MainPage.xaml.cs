
using MinesweeperBeta.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MinesweeperBeta.Logic;
using Windows.UI;
using System.Collections.ObjectModel;

namespace MinesweeperBeta
{
    /// <summary>
    /// Page showing the playing field, game title, bombs and flags available.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private GameComplexity prevGameComplexity;
        /// <summary>
        /// Whether the player has yet to visit a cell in a game.
        /// </summary>
        /// <remarks>
        /// Should be reset to true after a new game and set to false after a cell is visited.
        /// </remarks>
        private bool firstMove = true;

        /// <summary>
        /// Timer to tick every millisecond.
        /// </summary>
        private readonly DispatcherTimer Timer = new DispatcherTimer();
        /// <summary>
        /// DateTime of the start of the current game.
        /// </summary>
        private DateTime gameStartTime;

        private readonly SettingsService settings = new SettingsService();

        /// <summary>
        /// Matrix of cells represented by buttons on the field.
        /// </summary>
        private Button[,] cells;
        /// <summary>
        /// Underlying logic of the game using <see cref="GameGrid"/>.
        /// </summary>
        private GameGrid game;

        /// <summary>
        /// Complexity options available to the user.
        /// </summary>
        private readonly ObservableCollection<GameComplexity> complexityOptions = new ObservableCollection<GameComplexity>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            PlayingField_ViewBox.Stretch = Stretch.Uniform;

            InitialiseComplexityOptions();
            RestorePlayingField();
            ReevalBestTimes();


            Timer.Tick += Timer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            gameStartTime = DateTime.Now;
            Timer.Start();

        }

        /// <summary>
        /// TODO: Move into Models or Repository
        /// </summary>
        private void InitialiseComplexityOptions()
        {
            const int defaultComplexity = 1;

            complexityOptions.Add(new GameComplexity
            {
                Rows = 10,
                Columns = 10,
                Bombs = 5,
                ComplexityName = "Easy",
                Complexity = GameComplexityEnum.Easy
            });
            complexityOptions.Add(new GameComplexity
            {
                Rows = 13,
                Columns = 13,
                Bombs = 20,
                ComplexityName = "Moderate",
                Complexity = GameComplexityEnum.Moderate
            });
            complexityOptions.Add(new GameComplexity
            {
                Rows = 15,
                Columns = 15,
                Bombs = 40,
                ComplexityName = "Hard",
                Complexity = GameComplexityEnum.Hard
            });
            complexityOptions.Add(new GameComplexity
            {
                Rows = 15,
                Columns = 15,
                Bombs = 60,
                ComplexityName = "Pro",
                Complexity = GameComplexityEnum.Pro
            });

            prevGameComplexity = complexityOptions[defaultComplexity];
            game = new GameGrid(prevGameComplexity.Rows, prevGameComplexity.Columns, prevGameComplexity.Bombs);
            NewGameCmplx_Combo.SelectedIndex = defaultComplexity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private string TimeToString(double? time)
        {
            return time.HasValue ? String.Format("{0:#,0.00}", time.Value) : "N/A";
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReevalBestTimes()
        {
            BestTime_Easy_TextBlock.Text =
                TimeToString(settings.GetBestTime(GameComplexityEnum.Easy));
            BestTime_Mod_TextBlock.Text =
                TimeToString(settings.GetBestTime(GameComplexityEnum.Moderate));
            BestTime_Hard_TextBlock.Text =
                TimeToString(settings.GetBestTime(GameComplexityEnum.Hard));
            BestTime_Pro_TextBlock.Text =
                TimeToString(settings.GetBestTime(GameComplexityEnum.Pro));
        }

        /// <summary>
        /// Checks the user has set a new complexity.
        /// </summary>
        /// <returns>
        /// True if new complexity is set, false otherwise. A new game grid
        /// is generated and the visual playing field is reset.
        /// </returns>
        private bool ReevalGameComplexity()
        {
            var complexity = NewGameCmplx_Combo.SelectedItem as GameComplexity;
            if (complexity != prevGameComplexity)
            {
                prevGameComplexity = complexity;
                game = new GameGrid(complexity.Rows, complexity.Columns, complexity.Bombs);
                ClearPlayingField();
                RestorePlayingField();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reset displayed board and internal board for a new game.
        /// </summary>
        private void StartNewGame()
        {
            var gameComplexityChanged = ReevalGameComplexity();

            firstMove = true;
            if (!gameComplexityChanged)
            {
                game.ResetBoard();
                UpdateCells();
                UpdateFlagsTextBlock();
                foreach (CellButton cb in cells) cb.IsEnabled = true;
            }

            Timer.Start();
            gameStartTime = DateTime.Now;
        }

        /// <summary>
        /// Removes all elements from the visual playing field.
        /// </summary>
        private void ClearPlayingField()
        {
            PlayingField.RowDefinitions.Clear();
            PlayingField.ColumnDefinitions.Clear();
            PlayingField.Children.Clear();
        }

        /// <summary>
        /// Formats the <code>PlayingField</code> with the set number
        /// of rows and columns of buttons, depending on the game.
        /// </summary>
        private void RestorePlayingField()
        {
            cells = new Button[game.Rows, game.Columns];

            //Adds one row definition per gameRow and column definition
            //per gameColumn
            for (int i = 0; i < game.Rows; i++)
            {
                PlayingField.RowDefinitions.Add(new RowDefinition());
            }
            for (int j = 0; j < game.Columns; j++)
            {
                PlayingField.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < game.Rows; i++)
            {
                for (int j = 0; j < game.Columns; j++)
                {
                    //Each button uses ButtonRevealStyle, has margin 2 all
                    //around and stretches to fill the grid spot it is in
                    CellButton button = new CellButton
                    {
                        Margin = new Thickness(2.0),
                        HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                        VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch,
                    };

                    button.x = i;
                    button.y = j;

                    button.Click += new RoutedEventHandler(CellClickAsync);
                    button.RightTapped += new RightTappedEventHandler(CellRightTap);

                    PlayingField.Children.Add(button);
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);

                    cells[i, j] = button;
                }
            }

            Bombs_TextBlock.Text = "Bombs: " + game.BombQuantity;
            UpdateFlagsTextBlock();

            UpdateCells();
        }

        /// <summary>
        /// Initiates new game process after button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewGameClick(Object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        /// <summary>
        /// Updates <see cref="Flags_TextBlock"/> with current number of flags
        /// still available.
        /// </summary>
        private void UpdateFlagsTextBlock()
        {
            Flags_TextBlock.Text = "Flags Available: " + game.FlagsAvailable;
        }

        /// <summary>
        /// Handles cell (button) right click or hold and release.
        /// </summary>
        /// <param name="sender">
        /// The cell (CellButton) upon which the selection was performed.
        /// </param>
        /// <param name="e">
        /// <see cref="RightTappedRoutedEventArgs"/> for associated
        /// <see cref="RightTappedEventHandler"/>
        /// </param>
        /// <remarks>
        /// Right click signifies toggling the flag.
        /// </remarks>
        private void CellRightTap(Object sender, RightTappedRoutedEventArgs e)
        {
            CellButton b = (CellButton)sender;
            game.ToggleFlag(b.x, b.y);
            UpdateCells();
            UpdateFlagsTextBlock();
            if (!firstMove) CheckIfWon();
        }

        /// <summary>
        /// Handles cell (button) click or tap.
        /// </summary>
        /// <param name="sender">
        /// The cell (CellButton) upon which the selection was performed.
        /// </param>
        /// <param name="e">
        /// <see cref="RoutedEventArgs"/> for associated
        /// <see cref="RoutedEvent"/>
        /// </param>
        private async void CellClickAsync(Object sender, RoutedEventArgs e)
        {
            CellButton b = (CellButton)sender;

            //If cell flagged, do nothing
            if (game.VisitedPoints[b.x, b.y] == -1) return;

            //If cell not flagged, but no other moves made
            //generate bombs so that the user cannot activate
            //bomb on first move.
            if (firstMove)
            {
                firstMove = false;
                game.GenerateBombs(b.x, b.y);
                UpdateCells();
                return;
            }

            bool notLost = game.SelectPoint(b.x, b.y);

            var lostDialog = new ContentDialog
            {
                Title = "Game Over",
                Content = "You've hit a mine!",
                PrimaryButtonText = "Restart",
                SecondaryButtonText = "Close"
            };
            UpdateCells();

            //If selection performed on bomb, reveal bombs
            //and allow the user to restart or close the dialog
            if (!notLost)
            {
                Timer.Stop();
                game.RevealBombs();
                UpdateCells();
                var dialogReturn = await lostDialog.ShowAsync();
                switch (dialogReturn)
                {
                    case ContentDialogResult.Primary: //Restart button
                        StartNewGame();
                        break;
                    case ContentDialogResult.Secondary: //Close button
                        break;
                }
            }
            else CheckIfWon();
        }

        /// <summary>
        /// Update time displayed with the difference of the game
        /// start time and the current time in seconds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, object e)
        {
            var timeDiff = DateTime.Now - gameStartTime;
            Time.Text = String.Format("{0:#,0.00}", timeDiff.TotalSeconds);
        }

        /// <summary>
        /// Updates text of cells based on their value.
        /// </summary>
        private void UpdateCells()
        {
            for (int i = 0; i < game.Rows; i++)
            {
                for (int j = 0; j < game.Columns; j++)
                {
                    int cellVisitState = game.VisitedPoints[i, j];
                    int cellValue = game.BombsAndValues[i, j];

                    switch (cellVisitState)
                    {
                        case 1:
                            switch (cellValue)
                            {
                                case -1:
                                    //Bomb emoji
                                    cells[i, j].Content = "\uD83D\uDCA3";
                                    break;
                                case 0:
                                    //No text for 0 value cells
                                    cells[i, j].Content = "";
                                    break;
                                default:
                                    //Cell value which is at least 1.
                                    cells[i, j].Content = cellValue;
                                    break;
                            }
                            //Disable cells that are visited
                            cells[i, j].IsEnabled = false;
                            break;
                        case 0:
                            //Mathematic asterix
                            cells[i, j].Content = "\u2217";
                            cells[i, j].IsEnabled = true;
                            cells[i, j].BorderBrush = new RevealBackgroundBrush();
                            break;
                        case -1:
                            //Flag symbol
                            cells[i, j].Content = "\u2691";
                            cells[i, j].BorderBrush = new RevealBackgroundBrush
                            {
                                Color = Color.FromArgb(255, 255, 255, 0)
                            };
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Displays dialog if game won.
        /// </summary>
        public async void CheckIfWon()
        {
            if (game.Success())
            {
                double gameDuration = (DateTime.Now - gameStartTime).TotalSeconds;
                Timer.Stop();

                var newFastest = settings.UpdateTime(prevGameComplexity.Complexity, gameDuration);

                var wonDialog = new ContentDialog
                {
                    Title = newFastest ? $"New High Score: {gameDuration}" : "Game Won",
                    Content = "You've successfully avoided all bombs",
                    PrimaryButtonText = "Restart",
                    SecondaryButtonText = "Close"
                };

                if (newFastest) ReevalBestTimes();

                var dialogReturn = await wonDialog.ShowAsync();
                switch (dialogReturn)
                {
                    case ContentDialogResult.Primary:
                        StartNewGame();
                        break;
                    case ContentDialogResult.Secondary:
                        break;
                }
            }
        }

        /// <summary>
        /// Button with reference to its location on grid.
        /// </summary>
        private class CellButton : Button
        {
            public int x;
            public int y;
        }

        private class GameComplexity
        {
            public int Rows;
            public int Columns;
            public int Bombs;
            public string ComplexityName;
            public GameComplexityEnum Complexity;

            public override string ToString()
            {
                return String.Format("{0}: {1} x {2} ({3} Bombs)", ComplexityName, Rows, Columns, Bombs);
            }
        }
    }
}
