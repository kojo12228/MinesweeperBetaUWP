using System;
using System.Collections.Generic;

namespace MinesweeperBeta.Services
{
    class GameGridService
    {
        public int BombQuantity { get; private set; }
        public int VisitedCells { get; private set; }
        public int Rows { get; private set; }
        public int Columns { get; private set; }

        public int FlagsAvailable { get; private set; }

        /// <summary>
        /// Matrix for whether cells is bomb or has other value.
        /// </summary>
        /// <remarks>
        /// -1: Bomb
        ///  0: Not a bomb, nor adjacent to a bomb
        /// >0: Adjacent to at least one bomb
        /// </remarks>
        public int[,] BombsAndValues { get; private set; }

        /// <summary>
        /// Matrix for whether a cells is visited, unvisited or flagged
        /// </summary>
        /// <remarks>
        /// -1: Flagged cell;
        ///  0: Unvisited cell;
        ///  1: Visited cell (once visited, cannot be flagged or unvisited)
        /// </remarks>
        public int[,] VisitedPoints { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameGridService"/> class.
        /// </summary>
        /// <param name="rows">
        /// Number of rows in game.
        /// </param>
        /// <param name="columns">
        /// Number of columns in game.
        /// </param>
        /// <param name="bombQuantity">
        /// Number of bombs generated in game.
        /// </param>
        public GameGridService(int rows, int columns, int bombQuantity)
        {
            this.BombQuantity = bombQuantity;
            this.FlagsAvailable = bombQuantity;
            this.Rows = rows;
            this.Columns = columns;

            //Cannot proceed if there are meant to be more bombs than cells
            if (bombQuantity > (Columns * Rows)) throw new Exception();

            ResetBoard();
        }

        /// <summary>
        /// Generate new blank arrays for bombs and visited points.
        /// Should be followed with aq call to GenerateBombs.
        /// </summary>
        public void ResetBoard()
        {
            BombsAndValues = new int[Rows, Columns];
            VisitedPoints = new int[Rows, Columns];
            FlagsAvailable = BombQuantity;
            VisitedCells = 0;
        }

        /// <summary>
        /// Generate the positions of bombs on the board so that
        /// the initial selection will never be a bomb. Reveal
        /// neighbouring tiles as usual.
        /// </summary>
        /// <param name="initRow">First visit row position.</param>
        /// <param name="initCol">First visit column position.</param>
        public void GenerateBombs(int initRow, int initCol)
        {
            Random bombRandom = new Random();

            var bombs = new List<int>();
            while (bombs.Count < BombQuantity)
            {
                int proposedBombLocation = bombRandom.Next(0, Rows * Columns);
                var notYetAddedBomb = !bombs.Contains(proposedBombLocation);

                //Proposed bomb cannot be the X and Y coordinate of first click.
                var notPositionOfFirstClick =
                    !(proposedBombLocation / Rows == initRow &&
                      proposedBombLocation % Rows == initCol);

                if (notYetAddedBomb && notPositionOfFirstClick)
                {
                    bombs.Add(proposedBombLocation);
                }
            }

            foreach (int bombLocation in bombs)
            {
                int bombX = bombLocation / Rows;
                int bombY = bombLocation % Rows;

                BombsAndValues[bombX, bombY] = -1;

                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        int positionX = bombX + x - 1;
                        int positionY = bombY + y - 1;

                        //Values must be in grid range (0 or gridWidth/Height cause issues)
                        //For all cells adjacent to a bomb that are not bombs,
                        //add one to the value of bombs that cell is adjacent to
                        if (positionX <= Rows - 1 &&
                            positionY <= Columns - 1 &&
                            positionX >= 0 && positionY >= 0 &&
                            BombsAndValues[positionX, positionY] > -1)
                        {
                            BombsAndValues[positionX, positionY] += 1;
                        }
                    }
                }
            }

            VisitNearbyPoints(initRow, initCol);
        }

        /// <summary>
        /// Select cell to reveal bomb or value.
        /// </summary>
        /// <param name="row">
        /// Zero-indexed row of cell selected
        /// </param>
        /// <param name="column">
        /// Zero-indexed column of cell selected
        /// </param>
        /// <returns>
        /// False is bomb hit, otherwise true.
        /// </returns>
        public bool SelectPoint(int row, int column)
        {
            //Return false if bomb hit
            if (BombsAndValues[row, column] == -1) return false;
            VisitNearbyPoints(row, column);
            return true;
        }

        /// <summary>
        /// Toggle flag for given cell.
        /// </summary>
        /// <param name="row">
        /// Zero-indexed row of cell to be flagged.
        /// </param>
        /// <param name="column">
        /// Zero-indexed column of cell to be flagged.
        /// </param>
        /// <returns>
        /// False if no more flags available, otherwise true.
        /// </returns>
        /// <remarks>
        /// Nothing occurs for a cell already visited.
        /// </remarks>
        public bool ToggleFlag(int row, int column)
        {
            if (VisitedPoints[row, column] == 0)
            {
                if (FlagsAvailable == 0) return false;
                VisitedPoints[row, column] = -1;
                FlagsAvailable--;
            }
            else if (VisitedPoints[row, column] == -1)
            {
                VisitedPoints[row, column] = 0;
                FlagsAvailable++;
            }
            return true;
        }

        /// <summary>
        /// Propogates visiting cells after 0 value cell selected.
        /// </summary>
        /// <param name="row">
        /// Zero-indexed row of cell.
        /// </param>
        /// <param name="column">
        /// Zero-indexed column of cell.
        /// </param>
        /// <seealso cref="SelectPoint(int, int)"/>
        /// <remarks>
        /// If a bomb is hit, do nothing. 
        /// If a non-zero cell is hit, only make it visited. 
        /// If a zero cell is hit, check all adjacent cells to see if any are also zero
        /// and branch out until all zeros that can be reached by the original zero are visited.
        /// </remarks>

        void VisitNearbyPoints(int row, int column)
        {
            if (BombsAndValues[row, column] == -1) return; //Is a bomb
            VisitedPoints[row, column] = 1;
            VisitedCells += 1;
            if (BombsAndValues[row, column] > 0) return; //Don't reveal nearby of cells next to bombs
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    int cellX = row + x;
                    int cellY = column + y;
                    if (cellX >= 0 && cellX < Rows && cellY >= 0 && cellY < Columns)
                    {
                        //Only call method on unvisited cells
                        if (VisitedPoints[cellX, cellY] == 0) VisitNearbyPoints(row + x, column + y);
                    }
                }
            }
        }

        /// <summary>
        /// Method determines whether user has won the game.
        /// </summary>
        /// <returns>
        /// Returns true when a non-bomb cells visited, otherwise
        /// returns false.
        /// </returns>
        public bool Success()
        {
            return (Rows * Columns) == BombQuantity + VisitedCells;
        }

        /// <summary>
        /// Makes all bomb cells visited.
        /// </summary>
        public void RevealBombs()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    if (BombsAndValues[row, col] == -1) VisitedPoints[row, col] = 1;
                }
            }
        }
    }
}
