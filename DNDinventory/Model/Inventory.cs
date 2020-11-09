using DNDinventory.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDinventory.Model
{
    class Inventory : IInventory
    {
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int ImgHeight { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int ImgWidth { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IEnumerable<ItemSlot> itemSlots { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IEnumerable<Item> items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Create(Item item, Location loc)
        {
            throw new NotImplementedException();
        }

        public void Delete(Item item)
        {
            throw new NotImplementedException();
        }

        public void Remove(Item item)
        {
            throw new NotImplementedException();
        }

        public void Update(Item item)
        {
            throw new NotImplementedException();
        }
    }
}
