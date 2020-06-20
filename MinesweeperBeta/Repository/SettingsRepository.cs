using MinesweeperBeta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MinesweeperBeta.Repository
{
    class SettingsRepository
    {
        private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private readonly Dictionary<DifficultyEnum, string> complexityKeys = new Dictionary<DifficultyEnum, string>()
        {
            [DifficultyEnum.Easy] = "easyBestTime",
            [DifficultyEnum.Moderate] = "modBestTime",
            [DifficultyEnum.Hard] = "hardBestTime",
            [DifficultyEnum.Pro] = "proBestTime"
        };
        
        public double? GetTime(DifficultyEnum difficulty)
        {
            return localSettings.Values[complexityKeys[difficulty]] as double?;
        }

        public void SetTime(DifficultyEnum difficulty, double time)
        {
            localSettings.Values[complexityKeys[difficulty]] = time;
        }
    }
}
