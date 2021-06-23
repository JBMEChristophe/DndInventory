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

    public class UpdateGrid
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
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
