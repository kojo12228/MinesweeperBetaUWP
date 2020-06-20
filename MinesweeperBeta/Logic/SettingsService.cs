using MinesweeperBeta.Models;
using MinesweeperBeta.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperBeta.Logic
{
    class SettingsService
    {
        private readonly SettingsRepository repository = new SettingsRepository();

        public bool UpdateTime(GameComplexityEnum complexity, double gameDuration)
        {
            double? bestTime = repository.GetTime(complexity);

            if (bestTime.HasValue && gameDuration > bestTime.Value)
            {
                return false;
            }

            repository.SetTime(complexity, gameDuration);
            return true;
        }

        public double? GetBestTime(GameComplexityEnum complexity)
        {
            return repository.GetTime(complexity);
        }
    }
}
