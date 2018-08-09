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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MinesweeperBeta
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const int gameRows = 10;
        const int gameColumns = 10;
        const int gameBombs = 15;

        Button[,] cells;
        GameGrid game = new GameGrid(gameRows, gameColumns, gameBombs);

        bool flagMode = false;

        public MainPage()
        {
            this.InitializeComponent();

            cells = new Button[gameRows, gameColumns];

            for (int i = 0; i < gameRows; i++)
            {
                PlayingField.RowDefinitions.Add(new RowDefinition());
            }
            for (int j = 0; j < gameColumns; j++)
            {
                PlayingField.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < gameRows; i++)
            {
                for (int j = 0; j < gameColumns; j++)
                {

                    CellButton button = new CellButton
                    {
                        Margin = new Windows.UI.Xaml.Thickness(4.0),
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

            Bombs_TextBlock.Text = "Bombs: " + gameBombs;
            Flags_TextBlock.Text = "Flags Available: " + game.FlagsAvailable;

            UpdateCells();
        }

        private void CellRightTap(Object sender, RightTappedRoutedEventArgs e)
        {
            CellButton b = (CellButton)sender;
            game.ToggleFlag(b.x, b.y);
            UpdateCells();
            Flags_TextBlock.Text = "Flags Available: " + game.FlagsAvailable;
            CheckIfWon();
        }

        private async void CellClickAsync(Object sender, RoutedEventArgs e)
        {
            CellButton b = (CellButton)sender;
            if (game.ViewPointState(b.x, b.y) == -1) return;
            bool notLost = game.SelectPoint(b.x, b.y);

            var lostDialog = new ContentDialog
            {
                Title = "Game Over",
                Content = "You've hit a mine!",
                PrimaryButtonText = "Restart",
                SecondaryButtonText = "Close"
            };
            UpdateCells();
            if (!notLost)
            {
                game.RevealBombs();
                UpdateCells();
                var dialogReturn = await lostDialog.ShowAsync();
                switch (dialogReturn)
                {
                    case ContentDialogResult.Primary:
                        game.StartNewGame();
                        UpdateCells();
                        break;
                    case ContentDialogResult.Secondary:
                        break;
                }
            }
            else CheckIfWon();
        }

        private void UpdateCells()
        {
            for (int i = 0; i < gameRows; i++)
            {
                for (int j = 0; j < gameColumns; j++)
                {
                    int cellVisitState = game.ViewPointState(i, j);
                    int cellValue = game.ViewPointInfo(i, j);

                    switch (cellVisitState)
                    {
                        case 1:
                            switch (cellValue)
                            {
                                case -1:
                                    cells[i, j].Content = "\uD83D\uDCA3";
                                    break;
                                case 0:
                                    cells[i, j].Content = "";
                                    break;
                                default:
                                    cells[i, j].Content = cellValue;
                                    break;
                            }
                            cells[i, j].IsEnabled = false;
                            break;
                        case 0:
                            cells[i, j].Content = "\u2217";
                            break;
                        case -1:
                            cells[i, j].Content = "\u2691";
                            break;
                    }
                }
            }
        }

        public async void CheckIfWon()
        {
            if (game.Success())
            {
                var wonDialog = new ContentDialog
                {
                    Title = "Game Won",
                    Content = "You've successfully avoided all bombs",
                    PrimaryButtonText = "Restart",
                    SecondaryButtonText = "Close"
                };
                var dialogReturn = await wonDialog.ShowAsync();
                switch (dialogReturn)
                {
                    case ContentDialogResult.Primary:
                        game.StartNewGame();
                        UpdateCells();
                        break;
                    case ContentDialogResult.Secondary:
                        break;
                }
            }
        }

        private class CellButton : Button
        {
            public int x;
            public int y;
        }
    }
}
