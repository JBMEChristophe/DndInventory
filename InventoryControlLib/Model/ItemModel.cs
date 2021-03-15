using InventoryControlLib.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Utilities;

namespace InventoryControlLib.Model
{
    public abstract class ItemModel : INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        string id;
        public string ID
        {
            get
            {
                return id;
            }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChange("ID");
                }
            }
        }

        string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChange("Name");
                }
            }
        }

        string rarity;
        public string Rarity
        {
            get
            {
                return rarity;
            }
            set
            {
                if (rarity != value)
                {
                    rarity = value;
                    OnPropertyChange("Rarity");
                }
            }
        }

        string attunement;
        public string Attunement
        {
            get
            {
                return attunement;
            }
            set
            {
                if (attunement != value)
                {
                    attunement = value;
                    OnPropertyChange("Attunement");
                }
            }
        }

        string properties;
        public string Properties
        {
            get
            {
                return properties;
            }
            set
            {
                if (properties != value)
                {
                    properties = value;
                    OnPropertyChange("Properties");
                }
            }
        }

        string description;
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChange("Description");
                }
            }
        }

        ObservableCollection<ItemType> type;
        public ObservableCollection<ItemType> Type
        {
            get
            {
                return type;
            }
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChange("Type");
                }
            }
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

        string cost;
        public string Cost
        {
            get
            {
                return cost;
            }
            set
            {
                if (cost != value)
                {
                    cost = value;
                    OnPropertyChange("Cost");
                }
            }
        }

        string weight;
        public string Weight
        {
            get
            {
                return weight;
            }
            set
            {
                if (weight != value)
                {
                    weight = value;
                    OnPropertyChange("Weight");
                }
            }
        }

        string source;
        public string Source
        {
            get
            {
                return source;
            }
            set
            {
                if (source != value)
                {
                    source = value;
                    OnPropertyChange("Source");
                }
            }
        }

        bool isStackable;
        public bool IsStackable
        {
            get
            {
                return isStackable;
            }
            set
            {
                if (isStackable != value)
                {
                    isStackable = value;
                    OnPropertyChange("IsStackable");
                }
            }
        }

        int cellSpanX;
        public int CellSpanX
        {
            get
            {
                return cellSpanX;
            }
            set
            {
                if (cellSpanX != value)
                {
                    var cellWidth = Width / CellSpanX;

                    cellSpanX = value;
                    OnPropertyChange("CellSpanX");

                    Width = cellWidth * CellSpanX;
                }
            }
        }

        int cellSpanY;
        public int CellSpanY
        {
            get
            {
                return cellSpanY;
            }
            set
            {
                if (cellSpanY != value)
                {
                    var cellHeight = Height / CellSpanY;

                    cellSpanY = value;
                    OnPropertyChange("CellSpanY");

                    Height = cellHeight * CellSpanY;
                }
            }
        }

        double width;
        [XmlIgnore, DefaultValue("50")]
        public double Width
        {
            get
            {
                return width;
            }
            set
            {
                if (width != value)
                {
                    width = value;
                    OnPropertyChange("Width");
                }
            }
        }

        double height;
        [XmlIgnore, DefaultValue("50")]
        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                if (height != value)
                {
                    height = value;
                    OnPropertyChange("Height");
                }
            }
        }

        string xmlImageUrl;
        [XmlElement(elementName: "ImageUri")]
        public string XmlImageUrl
        {
            get
            {
                return ImageUri;
            }
            set
            {
                xmlImageUrl = value;
                OnPropertyChange("XmlImageUrl");

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
                    if (imageUri.IsFile)
                    {
                        return PathHelper.GetRelativePathFromApplication(imageUri.AbsoluteUri);
                    }
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
                if (value == "")
                {
                    imageUri = new Uri(@"Images\No_image_available.png", UriKind.Relative);
                }
                else
                {
                    imageUri = new Uri(value, UriKind.RelativeOrAbsolute);
                    if(!imageUri.IsAbsoluteUri)
                    {
                        imageUri = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imageUri.ToString()), UriKind.Absolute);
                    }
                }
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

        public ItemModel(string id, string name, IList<ItemType> type, string cost, string weight, string rarity, string attunement, string properties, string description, string source, double width, double height, int spanX = 1, int spanY = 1, bool isStackable = false, string image = "")
        {
            logger.Debug($"> ItemModel(id: {id}, name: {name}, type: {TypeStr}, cost: {cost}, weight: {weight}, source: {source}, width: {width}, height: {height}, isStackable: {isStackable}, image: {image})");
            ID = id;
            Name = name;
            Type = new ObservableCollection<ItemType>(type);
            Cost = cost;
            Weight = weight;
            Rarity = rarity;
            Attunement = attunement;
            Properties = properties;
            Description = description;
            CellSpanX = spanX;
            CellSpanY = spanY;
            Source = source;
            IsStackable = isStackable;
            Width = width;
            Height = height;
            ImageUri = image;
            logger.Debug($"< ItemModel(id: {id}, name: {name}, type: {TypeStr}, cost: {cost}, weight: {weight}, source: {source}, width: {width}, height: {height}, isStackable: {isStackable}, image: {image})");
        }

        public ItemModel(string id, string name, ItemType type, string cost, string weight, string rarity, string attunement, string properties, string description, string source, double width, double height, int spanX = 1, int spanY = 1, bool isStackable = false, string image = "")
            :this(id, name, new List<ItemType> { type }, cost, weight, rarity, attunement, properties, description, source, width, height, spanX, spanY, isStackable, image)
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
