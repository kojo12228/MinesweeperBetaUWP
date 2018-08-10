﻿using System;
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
    /// Page showing the playing field, game title, bombs and flags available.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Fixed number of rows on playing field.
        /// </summary>
        private const int gameRows = 12;
        /// <summary>
        /// Fixed number of columns on the playing field.
        /// </summary>
        private const int gameColumns = 12;
        /// <summary>
        /// Fixed number of bombs used in the game.
        /// </summary>
        private const int gameBombs = 20;

        /// <summary>
        /// Matrix of cells represented by buttons on the field.
        /// </summary>
        private Button[,] cells;
        /// <summary>
        /// Underlying logic of the game using <see cref="GameGrid"/>.
        /// </summary>
        private GameGrid game = new GameGrid(gameRows, gameColumns, gameBombs);

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            cells = new Button[gameRows, gameColumns];

            //Adds one row definition per gameRow and column definition
            //per gameColumn
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
                    //Each button uses ButtonRevealStyle, has margin 4 all
                    //around and stretches to fill the grid spot it is in
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
            UpdateFlagsTextBlock();

            UpdateCells();
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
            CheckIfWon();
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
                game.RevealBombs();
                UpdateCells();
                var dialogReturn = await lostDialog.ShowAsync();
                switch (dialogReturn)
                {
                    case ContentDialogResult.Primary: //Restart button
                        game.StartNewGame();
                        UpdateCells();
                        foreach (CellButton cb in cells) cb.IsEnabled = true;
                        break;
                    case ContentDialogResult.Secondary: //Close button
                        break;
                }
            }
            else CheckIfWon();
        }

        /// <summary>
        /// Updates text of cells based on their value.
        /// </summary>
        private void UpdateCells()
        {
            for (int i = 0; i < gameRows; i++)
            {
                for (int j = 0; j < gameColumns; j++)
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
                            break;
                        case -1:
                            //Flag symbol
                            cells[i, j].Content = "\u2691";
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

        /// <summary>
        /// Button with reference to its location on grid.
        /// </summary>
        private class CellButton : Button
        {
            public int x;
            public int y;
        }
    }
}