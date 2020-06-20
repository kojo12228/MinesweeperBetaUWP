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

        private readonly Dictionary<GameComplexityEnum, string> complexityKeys = new Dictionary<GameComplexityEnum, string>()
        {
            [GameComplexityEnum.Easy] = "easyBestTime",
            [GameComplexityEnum.Moderate] = "modBestTime",
            [GameComplexityEnum.Hard] = "hardBestTime",
            [GameComplexityEnum.Pro] = "proBestTime"
        };
        
        public double? GetTime(GameComplexityEnum complexity)
        {
            return localSettings.Values[complexityKeys[complexity]] as double?;
        }

        public void SetTime(GameComplexityEnum complexity, double time)
        {
            localSettings.Values[complexityKeys[complexity]] = time;
        }
    }
}
