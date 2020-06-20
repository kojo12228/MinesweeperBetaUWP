using MinesweeperBeta.Services;
using MinesweeperBeta.Models;
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

namespace MinesweeperBeta
{
    public sealed partial class HighestScorePanel : UserControl
    {
        /// <summary>
        /// Access to service to set high scores for different game difficulties.
        /// </summary>
        private readonly SettingsService settings = new SettingsService();

        public HighestScorePanel()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Format double as a decimal to 2 d.p. without rounding.
        /// </summary>
        /// <param name="time">Time in seconds.</param>
        /// <returns>String of time to 2 decimal places.</returns>
        private string TimeToString(double? time)
        {
            return time.HasValue ? String.Format("{0:#,0.00}", time.Value) : "N/A";
        }

        /// <summary>
        /// Retrieve high score for each game difficulty and update
        /// displayed best score.
        /// </summary>
        public void Refresh()
        {
            BestTime_Easy_TextBlock.Text =
                TimeToString(settings.GetBestTime(DifficultyEnum.Easy));
            BestTime_Mod_TextBlock.Text =
                TimeToString(settings.GetBestTime(DifficultyEnum.Moderate));
            BestTime_Hard_TextBlock.Text =
                TimeToString(settings.GetBestTime(DifficultyEnum.Hard));
            BestTime_Pro_TextBlock.Text =
                TimeToString(settings.GetBestTime(DifficultyEnum.Pro));
        }
    }
}
