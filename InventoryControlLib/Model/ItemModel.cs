using InventoryControlLib.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace InventoryControlLib.Model
{
    public abstract class ItemModel : INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public int ID
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public List<ItemType> Type
        {
            get; set;
        }

        [XmlIgnore]
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
            get; set;
        }

        public string Weight
        {
            get; set;
        }

        public string Source
        {
            get; set;
        }

        public bool IsStackable
        {
            get; set;
        }

        public int CellSpanX
        {
            get; set;
        }

        public int CellSpanY
        {
            get; set;
        }

        [XmlIgnore, DefaultValue("50")]
        public double Width
        {
            get; set;
        }

        [XmlIgnore, DefaultValue("50")]
        public double Height
        {
            get; set;
        }

        string xmlImageUrl;
        [XmlElement(elementName: "ImageUri")]
        public string XmlImageUrl
        {
            get
            {
                return xmlImageUrl;
            }
            set
            {
                xmlImageUrl = value;
                ImageUri = xmlImageUrl;
            }
        }

        Uri imageUri;
        [XmlIgnore]
        public string ImageUri
        { 
            get
            {
                if(imageUri == null)
                {
                    return null;
                }

                if (imageUri.IsAbsoluteUri)
                {
                    return imageUri.AbsoluteUri;
                }
                else
                {
                    var tmp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imageUri.ToString());
                    return tmp;
                }
            }
            set
            {
                imageUri = new Uri(value, UriKind.RelativeOrAbsolute);
                OnPropertyChange("ImageUri");
                OnPropertyChange("Image");
            }
        }

        [XmlIgnore]
        public BitmapImage Image 
        { 
            get
            {
                var Image = new BitmapImage(imageUri);
                Image.DecodePixelWidth = (int)Math.Floor(Width);
                return Image;
            }
        }

        public ItemModel()
        {
        }

        public ItemModel(int id, string name, List<ItemType> type, string cost, string weight, string source, double width, double height, int spanX = 1, int spanY = 1, bool isStackable = false, string image = null)
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
            logger.Debug($"< ItemModel(id: {id}, name: {name}, type: {TypeStr}, cost: {cost}, weight: {weight}, source: {source}, width: {width}, height: {height}, isStackable: {isStackable}, image: {image})");
        }

        public ItemModel(int id, string name, ItemType type, string cost, string weight, string source, double width, double height, int spanX = 1, int spanY = 1, bool isStackable = false, string image = null)
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
