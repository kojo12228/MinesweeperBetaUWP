using MinesweeperBeta.Models;
using MinesweeperBeta.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperBeta.Services
{
    class SettingsService
    {
        private readonly SettingsRepository repository = new SettingsRepository();

        /// <summary>
        /// If a given game duration is faster than the best recorded (in settings),
        /// update and return true.
        /// </summary>
        /// <param name="difficulty">
        /// Difficulty of game where time recorded.
        /// </param>
        /// <param name="gameDuration">
        /// Time in seconds of game at given difficulty.
        /// </param>
        /// <returns>
        /// True if gameDuration for given difficulty is smaller than current
        /// best value. Otherwise returns false.
        /// </returns>
        public bool UpdateTime(DifficultyEnum difficulty, double gameDuration)
        {
            double? bestTime = repository.GetTime(difficulty);

            if (bestTime.HasValue && gameDuration > bestTime.Value)
            {
                return false;
            }

            repository.SetTime(difficulty, gameDuration);
            return true;
        }

        public double? GetBestTime(DifficultyEnum complexity)
        {
            return repository.GetTime(complexity);
        }
    }
}
