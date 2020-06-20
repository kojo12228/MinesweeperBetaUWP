using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace MinesweeperBeta.Models
{
    /// <summary>
    /// Button with reference to its location on grid.
    /// </summary>
    public class CellButton : Button
    {
        public int Row;
        public int Column;

        public CellButton(int row, int col)
        {
            Row = row;
            Column = col;
        }
    }
}
