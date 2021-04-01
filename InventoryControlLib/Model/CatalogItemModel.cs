using InventoryControlLib.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace InventoryControlLib.Model
{
    public class CatalogItemModel : ItemModel
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        [XmlIgnore]
        public bool IsDefault 
        {
            get; set;
        }

        private CatalogItemModel()
            :base()
        { }

        public CatalogItemModel(string id, string name, IList<ItemType> type, string cost, string weight, string rarity, string attunement, string properties, string description, string source, double cellWidth, double cellHeight, int spanX = 1, int spanY = 1, bool isStackable = false, string imageUri = "", bool isDefault = false)
            : base(id, name, type, cost, weight, rarity, attunement, properties, description, source, (cellWidth * spanX), (cellHeight * spanY), spanX, spanY, isStackable, imageUri)
        {
            logger.Debug($">< CatalogItemModel(id: {id}, cellWidth: {cellWidth}, cellHeight: {cellHeight}, spanX: {spanX}, spanY: {spanY}, isStackable: {isStackable}, image: {imageUri}, isDefault: {isDefault})");
            IsDefault = isDefault;
        }
        
        public CatalogItemModel(string id, string name, ItemType type, string cost, string weight, string rarity, string attunement, string properties, string description, string source, double cellWidth, double cellHeight, int spanX = 1, int spanY = 1, bool isStackable = false, string imageUri = "", bool isDefault = false)
        :this(id, name, new List<ItemType> { type }, cost, weight, rarity, attunement, properties, description, source, cellWidth, cellHeight, spanX, spanY, isStackable, imageUri, isDefault)
        { }

        public CatalogItemModel(CatalogItemModel model)
            : this(model.ID, model.Name, model.Type, model.Cost, model.Weight, model.Rarity, model.Attunement, model.Properties, model.Description, model.Source, model.Width / model.CellSpanX, model.Height / model.CellSpanY, model.CellSpanX, model.CellSpanY, model.IsStackable, model.ImageUri, model.IsDefault)
        { }
    }
}
