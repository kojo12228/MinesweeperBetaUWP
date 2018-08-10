using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperBeta.Logic
{
    class GameGrid
    {
        public int BombQuantity { get; private set; }
        public int VisitedCells { get; private set; }
        public int GridWidth { get; private set; }
        public int GridHeight { get; private set; }

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
        /// Initializes a new instance of the <see cref="GameGrid"/> class.
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
        public GameGrid(int rows, int columns, int bombQuantity)
        {
            this.BombQuantity = bombQuantity;
            this.FlagsAvailable = bombQuantity;
            this.GridWidth = rows;
            this.GridHeight = columns;

            //Cannot proceed if there are meant to be more bombs than cells
            if (bombQuantity > (GridHeight * GridWidth)) throw new Exception();

            StartNewGame();
        }
        
        /// <summary>
        /// Method resets board (all cells unvisited) and new bombs.
        /// </summary>
        public void StartNewGame()
        {
            BombsAndValues = new int[GridWidth, GridHeight];
            VisitedPoints = new int[GridWidth, GridHeight];
            FlagsAvailable = BombQuantity;
            VisitedCells = 0;

            Random bombRandom = new Random();

            var bombs = new List<int>();
            while (bombs.Count < BombQuantity)
            {
                int proposedBombLocation = bombRandom.Next(0, GridWidth * GridHeight);
                if (!bombs.Contains(proposedBombLocation))
                    bombs.Add(proposedBombLocation);
            }

            foreach (int bombLocation in bombs)
            {
                int bombX = bombLocation / GridWidth;
                int bombY = bombLocation % GridHeight;

                BombsAndValues[bombX, bombY] = -1;

                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        int positionX = bombX + x - 1;
                        int positionY = bombY + y - 1;
                        //Values must be in grid range (0 or gridWidth/Height cause issues)
                        if (positionX <= GridWidth - 1 &&
                            positionY <= GridHeight - 1 &&
                            positionX >= 0 && positionY >= 0)
                        {
                            //For all cells adjacent to a bomb that are not bombs,
                            //add one to the value of bombs that cell is adjacent to
                            if (BombsAndValues[positionX, positionY] > -1)
                                BombsAndValues[positionX, positionY] += 1;
                        }
                    }
                }
            }
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
                    if (cellX >= 0 && cellX < GridWidth && cellY >= 0 && cellY < GridHeight)
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
            return (GridWidth * GridHeight) == BombQuantity + VisitedCells;
        }

        /// <summary>
        /// Makes all bomb cells visited.
        /// </summary>
        public void RevealBombs()
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    if (BombsAndValues[x, y] == -1) VisitedPoints[x, y] = 1;
                }
            }
        }
    }
}
