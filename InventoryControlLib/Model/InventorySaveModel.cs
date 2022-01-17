using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InventoryControlLib.Model
{

    public class InventorySaveModel
    {
        // Set defaults here
        public string InventoryName = "Backpack";
        public string InventoryBackground = "";
        public bool CanBeDeleted = true;
        public bool CanBeEdited = true;
        public List<UiItemModel> UiItems = new List<UiItemModel>();
        public List<InventoryItemModel> InventoryItems = new List<InventoryItemModel>();

        public override string ToString()
        {
            return $"InventoryName: {InventoryName}; InventoryBackground: {InventoryBackground}; CanBeDeleted: {CanBeDeleted}; CanBeEdited: {CanBeEdited}";
        }
    }
}
