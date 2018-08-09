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

        // -1: Bomb
        //  0: Not a bomb, nor adjacent to a bomb
        // >0: Adjacent to at least one bomb
        private int[,] bombsAndValues;

        // -1: Flagged cell
        //  0: Unvisited cell
        //  1: Visited cell (once visited, cannot be flagged or unvisited)
        private int[,] visitedPoints;

        public GameGrid(int width, int height, int bombQuantity)
        {
            this.BombQuantity = bombQuantity;
            this.FlagsAvailable = bombQuantity;
            this.GridWidth = width;
            this.GridHeight = height;

            //Cannot proceed if there are meant to be more bombs than cells
            if (bombQuantity > (GridHeight * GridWidth)) throw new Exception();

            StartNewGame();
        }

        public void StartNewGame()
        {
            bombsAndValues = new int[GridWidth, GridHeight];
            visitedPoints = new int[GridWidth, GridHeight];

            Random bombRandom = new Random();
            for (int i = 0; i < BombQuantity; i++)
            {
                //Since Random.Next() is min inclusive but max exclusive,
                //only non-negative values less than gridWidth or gridHeight will be generated
                int bombX = bombRandom.Next(0, GridWidth);
                int bombY = bombRandom.Next(0, GridHeight);

                //Guarantees the number of bombs desired will appear
                //and a bomb will not be placed in the same cell more
                //than once
                while (bombsAndValues[bombX, bombY] == -1)
                {
                    bombX = bombRandom.Next(0, GridWidth);
                    bombY = bombRandom.Next(0, GridHeight);
                }

                bombsAndValues[bombX, bombY] = -1;
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        int positionX = bombX + x - 1;
                        int positionY = bombY + y - 1;
                        //Values must be in grid range (0 or gridWidth/Height cause issues)
                        if (positionX <= GridWidth - 1 && positionY <= GridHeight - 1 && positionX >= 0 && positionY >= 0)
                        {
                            //For all cells adjacent to a bomb that are not bombs,
                            //add one to the value of bombs that cell is adjacent to
                            if (bombsAndValues[positionX, positionY] > -1) bombsAndValues[positionX, positionY] += 1;
                        }
                    }
                }
            }
        }

        public int ViewPointState(int width, int height)
        {
            return visitedPoints[width, height];
        }

        public int ViewPointInfo(int width, int height)
        {
            return bombsAndValues[width, height];
        }

        public bool SelectPoint(int width, int height)
        {
            if (bombsAndValues[width, height] == -1) return false; //Return false if bomb hit
            VisitNearbyPoints(width, height);
            return true;
        }

        public bool ToggleFlag(int cellX, int cellY)
        {
            //Nothing occurs for a cell already visited

            if (visitedPoints[cellX, cellY] == 0)
            {
                if (FlagsAvailable == 0) return false;
                visitedPoints[cellX, cellY] = -1;
                FlagsAvailable--;
            }
            else if (visitedPoints[cellX, cellY] == -1)
            {
                visitedPoints[cellX, cellY] = 0;
                FlagsAvailable++;
            }
            return true;
        }

        //If a bomb is hit, do nothing
        //If a non-zero cell is hit, only make it visited
        //If a zero cell is hit, check all adjacent cells to see if any are also zero
        //and branch out until all zeros that can be reached by the original zero are visited
        void VisitNearbyPoints(int width, int height)
        {
            if (bombsAndValues[width, height] == -1) return; //Is a bomb
            visitedPoints[width, height] = 1;
            VisitedCells += 1;
            if (bombsAndValues[width, height] > 0) return; //Don't reveal nearby of cells next to bombs
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    int cellX = width + x;
                    int cellY = height + y;
                    if (cellX >= 0 && cellX < GridWidth && cellY >= 0 && cellY < GridHeight)
                    {
                        //Only call method on unvisited cells
                        if (visitedPoints[cellX, cellY] == 0) VisitNearbyPoints(width + x, height + y);
                    }
                }
            }
        }

        public bool Success()
        {
            return (GridWidth * GridHeight) == BombQuantity + VisitedCells;
        }

        public void RevealBombs()
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    if (bombsAndValues[x, y] == -1) visitedPoints[x, y] = 1;
                }
            }
        }
    }
}
