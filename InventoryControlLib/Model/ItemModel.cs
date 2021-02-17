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
    public abstract class ItemModel : INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public int ID
        {
            get; private set;
        }

        public string Name
        {
            get; private set;
        }

        public List<ItemType> Type
        {
            get; private set;
        }

        public string TypeStr
        {
            get 
            {
                if (Type != null)
                {
                    return string.Join(",", EnumHelper.GetDescriptionListFromEnumList(Type));
                }
                return "";
            }
        }

        public string Cost
        {
            get; private set;
        }

        public string Weight
        {
            get; private set;
        }

        public string Source
        {
            get; private set;
        }

        public bool IsStackable
        {
            get; private set;
        }

        public int CellSpanX
        {
            get; private set;
        }

        public int CellSpanY
        {
            get; private set;
        }

        public double Width
        {
            get; private set;
        }

        public double Height
        {
            get; private set;
        }

        Uri imageUri;
        public Uri ImageUri
        { 
            get
            {
                return imageUri;
            }
            set
            {
                imageUri = value;
                Image = new BitmapImage(imageUri);
                Image.DecodePixelWidth = (int)Math.Floor(Width);
                OnPropertyChange("ImageUri");
                OnPropertyChange("Image");
            }
        }
        public BitmapImage Image { get; private set; }
        
        public ItemModel(int id, string name, List<ItemType> type, string cost, string weight, string source, double width, double height, int spanX = 1, int spanY = 1, bool isStackable = false, Uri image = null)
        {
            logger.Debug($"> ItemModel(id: {id}, name: {name}, type: {TypeStr}, cost: {cost}, weight: {weight}, source: {source}, width: {width}, height: {height}, isStackable: {isStackable}, image: {image})");
            ID = id;
            Name = name;
            Type = type;
            Cost = cost;
            Weight = weight;
            CellSpanX = spanX;
            CellSpanY = spanY;
            Source = source;
            IsStackable = isStackable;
            Width = width;
            Height = height;
            ImageUri = image;
            Image = new BitmapImage(image);
            logger.Debug($"< ItemModel(id: {id}, name: {name}, type: {TypeStr}, cost: {cost}, weight: {weight}, source: {source}, width: {width}, height: {height}, isStackable: {isStackable}, image: {image})");
        }

        public ItemModel(int id, string name, ItemType type, string cost, string weight, string source, double width, double height, int spanX = 1, int spanY = 1, bool isStackable = false, Uri image = null)
            :this(id, name, new List<ItemType> { type }, cost, weight, source, width, height, spanX, spanY, isStackable, image)
        { }

        public override string ToString()
        {
            return $"ID: {ID}, Name: {Name}, Size: [X: {Width}, Y: {Height}], IsStackable: {IsStackable}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
