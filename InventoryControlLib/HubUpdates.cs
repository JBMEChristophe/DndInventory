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
    class PositionUpdate
    {
        public Point Position { get; set; }

        public override string ToString()
        {
            var str = $"Position: [{Position}]";

            return str;
        }
    }

    class ItemPositionUpdate : PositionUpdate
    {
        public Item Item { get; set; }

        public override string ToString()
        {
            return $"item: [{Item}]; {base.ToString()}";
        }
    }


    class CatalogItemPositionUpdate : PositionUpdate
    {
        public CatalogItem Item { get; set; }

        public override string ToString()
        {
            return $"item: [{Item}]; {base.ToString()}";
        }
    }

    public class DmTabUpdate
    {
        public string Header { get; set; }
    }

    public class UpdateGrid
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public InventoryGrid Inventory { get; set; }
        public Grid Grid { get; set; }
        public Size Size { get; set; }
        public Size CellSize { get; set; }

        public override string ToString()
        {
            return $"id: [{Id}], name: [{Name}], size: [{Size}]; cellSize: [{CellSize}]";
        }
    }

    class DeleteGrid
    {
        public Guid Id { get; set; }
    }

    public class MoveAllItemsTo
    {
        public Guid MoveToId { get; set; }
        public List<Guid> FallBackIds { get; set; }
        public List<Item> Items { get; set; }
    }
}
