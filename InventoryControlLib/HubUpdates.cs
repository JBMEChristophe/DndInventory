using InventoryControlLib.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InventoryControlLib
{
    class ItemPositionUpdate
    {
        public Item Item { get; set; }
        public Point Position { get; set; }
    }

    class UpdateGrid
    {
        public Point ScreenPoint { get; set; }
        public Size Size { get; set; }
        public Size CellSize { get; set; }
    }

    class GridAddUpdate
    {
        public UpdateGrid Grid { get; set; }
    }
}
