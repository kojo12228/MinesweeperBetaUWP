using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperBeta.Models
{
    /// <summary>
    /// Class defining information about the playing field given a difficulty.
    /// </summary>
    class GameDifficultyDefinition
    {
        public int Rows;
        public int Columns;
        public int Bombs;
        public DifficultyEnum Complexity;

        public static readonly int DefaultOptionIndex = 1;

        public override string ToString()
        {
            return String.Format("{0}: {1} x {2} ({3} Bombs)", Complexity.ToString(), Rows, Columns, Bombs);
        }

        public static IEnumerable<GameDifficultyDefinition> DifficultyOptions()
        {
            var options = new List<GameDifficultyDefinition>(4) {
                new GameDifficultyDefinition
                {
                    Rows = 10,
                    Columns = 10,
                    Bombs = 5,
                    Complexity = DifficultyEnum.Easy
                },
                new GameDifficultyDefinition
                {
                    Rows = 13,
                    Columns = 13,
                    Bombs = 20,
                    Complexity = DifficultyEnum.Moderate
                },
                new GameDifficultyDefinition
                {
                    Rows = 15,
                    Columns = 15,
                    Bombs = 40,
                    Complexity = DifficultyEnum.Hard
                },
                new GameDifficultyDefinition
                {
                    Rows = 15,
                    Columns = 15,
                    Bombs = 60,
                    Complexity = DifficultyEnum.Pro
                }
            };

            return options;
        }
    }
}
