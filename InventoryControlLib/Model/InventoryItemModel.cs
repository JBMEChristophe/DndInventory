using InventoryControlLib.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Utilities;

namespace InventoryControlLib.Model
{
    public class InventoryItemModel : UiItemModel
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public List<UiItemModel> Items { get; set; }
        public Size Size { get; set; }

        public InventoryItemModel(string id, string name, Size size, int column, int row, List<UiItemModel> items, string imageUri = "")
            : base(id, name, new List<ItemType>() { ItemType.Inventory }, CalculateCost(items), CalculateWeight(items), "-", "-", "-", GenerateDescription(items), "-", 50, 50, column, row, 1, 1, 1, false, imageUri)
        {
            logger.Debug($">< InventoryItemModel(id: {id}, name: {name}, column: {column}, row: {row}, image: {imageUri})");
            Items = items;
            Size = size;
        }

        public InventoryItemModel(InventoryItemModel model)
            : this(model.ID, model.Name, model.Size, model.CellX, model.CellY, model.Items, model.ImageUri)
        { }

        public InventoryItemModel() { }

        private static string CalculateCost(List<UiItemModel> items)
        {
            Currency cost = new Currency(0.0, CurrencyType.CP);
            foreach (var item in items)
            {
                var currency = CurrencyHelper.ConvertStringToCurrency(item.TotalCost);
                cost += currency;
            }
            cost.ConvertToMaxType();
            return cost.ToString();
        }

        private static string CalculateWeight(List<UiItemModel> items)
        {
            return "";
        }

        private static string GenerateDescription(List<UiItemModel> items)
        {
            string desc = string.Empty;
            foreach (var item in items)
            {
                desc += "\n - ";
                desc += item.Name;
                if(item.IsStackable)
                {
                    desc += $" ({item.Quantity})";
                }
            }
            return desc;
        }

        public override string ToString()
        {
            return $"{base.ToString()}";
        }
    }
}
