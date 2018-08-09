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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MinesweeperBeta
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        List<Button> cells = new List<Button>();

        public MainPage()
        {
            this.InitializeComponent();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Button button = new Button
                    {
                        Content = "Button1",
                        HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                        VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch,
                    };

                    PlayingField.Children.Add(button);
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);

                    cells.Add(button);
                }
            }
        }
    }
}
