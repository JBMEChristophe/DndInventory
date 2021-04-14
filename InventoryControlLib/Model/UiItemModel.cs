using InventoryControlLib.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace InventoryControlLib.Model
{
    public class UiItemModel : ItemModel
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public double CellWidth
        {
            get; private set;
        }

        public double CellHeight
        {
            get; private set;
        }

        int cellX;
        public int CellX
        {
            get
            {
                return cellX;
            }
            set
            {
                if (cellX != value)
                {
                    cellX = value;
                    OnPropertyChange("CellX");
                }
            }
        }

        int cellY;
        public int CellY
        {
            get
            {
                return cellY;
            }
            set
            {
                if (cellY != value)
                {
                    cellY = value;
                    OnPropertyChange("CellY");
                }
            }
        }

        int quantity;
        public int Quantity
        {
            get
            {
                return quantity;
            }
            set
            {
                if (quantity != value)
                {
                    quantity = value;
                    OnPropertyChange("Quantity");
                }
            }
        }
        
        public UiItemModel(string id, string name, IList<ItemType> type, string cost, string weight, string rarity, string attunement, string properties, string description, string source, double cellWidth, double cellHeight, int column, int row, int spanX = 1, int spanY = 1, int quantity = 1, bool isStackable = false, string imageUri = "")
            :base(id, name, type, cost, weight, rarity, attunement, properties, description, source, (cellWidth * spanX), (cellHeight * spanY), spanX, spanY, isStackable, imageUri)
        {
            logger.Debug($"> UiItemModel(id: {id}, cellWidth: {cellWidth}, cellHeight: {cellHeight}, column: {column}, row: {row}, spanX: {spanX}, spanY: {spanY}, quantity: {quantity}, isStackable: {isStackable}, image: {imageUri})");
            CellWidth = cellWidth;
            CellHeight = cellHeight;
            CellX = column;
            CellY = row;
            Quantity = quantity;
            logger.Debug($"< UiItemModel(id: {id}, cellWidth: {cellWidth}, cellHeight: {cellHeight}, column: {column}, row: {row}, spanX: {spanX}, spanY: {spanY}, quantity: {quantity}, isStackable: {isStackable}, image: {imageUri})");
        }

        public UiItemModel(string id, string name, ItemType type, string cost, string weight, string rarity, string attunement, string properties, string description, string source, double cellWidth, double cellHeight, int column, int row, int spanX = 1, int spanY = 1, int quantity = 1, bool isStackable = false, string imageUri = "")
            : this(id, name, new List<ItemType> { type }, cost, weight, rarity, attunement, properties, description, source, cellWidth, cellHeight, column, row, spanX, spanY, quantity, isStackable, imageUri)
        { }

        public UiItemModel(UiItemModel model, double cellWidth, double cellHeight)
            : this(model.ID, model.Name, model.Type, model.Cost, model.Weight, model.Rarity, model.Attunement, model.Properties, model.Description, model.Source, cellWidth, cellHeight, model.CellX, model.CellY, model.CellSpanX, model.CellSpanY, model.Quantity, model.IsStackable, model.ImageUri)
        { }

        public override string ToString()
        {
            return $"{base.ToString()}, Quantity: {Quantity}, CellSpan: [X: {CellSpanX}, Y: {CellSpanY}], Cell: [X: {CellX}, Y: {CellY}]";
        }
    }
}
