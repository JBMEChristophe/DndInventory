using InventoryControlLib.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InventoryControlLib
{
    class ItemPositionUpdate
    {
        public Item Item { get; set; }
        public Point Position { get; set; }

        public override string ToString()
        {
            return $"item: [{Item}]; position: [{Position}]";
        }
    }

    class CatalogItemPositionUpdate
    {
        public CatalogItem Item { get; set; }
        public Point Position { get; set; }

        public override string ToString()
        {
            return $"item: [{Item}]; position: [{Position}]";
        }
    }

    class UpdateGrid
    {
        public Grid Grid { get; set; }
        public Size Size { get; set; }
        public Size CellSize { get; set; }

        public override string ToString()
        {
            return $"size: [{Size}]; cellSize: [{CellSize}]";
        }
    }

    class GridAddUpdate
    {
        public UpdateGrid Grid { get; set; }

        public override string ToString()
        {
            return $"Grid: {Grid}";
        }
    }
}
