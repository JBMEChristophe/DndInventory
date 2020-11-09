using DNDinventory.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDinventory.Interface
{
    public interface IInventory
    {
        string Name { get; set; }
        int ImgHeight { get; set; }
        int ImgWidth { get; set; }
        IEnumerable<ItemSlot> itemSlots { get; set; }
        IEnumerable<Item> items { get; set; }

        bool Create(Item item, Location loc);
        void Remove(Item item);
        void Update (Item item);
        void Delete(Item item);
    }
}
