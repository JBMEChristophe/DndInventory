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
    public class ItemModel : INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private int id;
        public int ID
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

        private double cellWidth;
        public double CellWidth
        {
            get
            {
                return cellWidth;
            }
            set
            {
                if (cellWidth != value)
                {
                    cellWidth = value;
                    OnPropertyChange("CellWidth");
                    OnPropertyChange("Width");
                }
            }
        }

        private double cellHeight;
        public double CellHeight
        {
            get
            {
                return cellHeight;
            }
            set
            {
                if (cellHeight != value)
                {
                    cellHeight = value;
                    OnPropertyChange("CellHeight");
                    OnPropertyChange("Height");
                }
            }
        }

        public double Width
        {
            get
            {
                return cellWidth * CellSpanX;
            }
        }

        public double Height
        {
            get
            {
                return cellHeight * cellSpanY;
            }
        }

        private int cellSpanX;
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
                    cellSpanX = value;
                    OnPropertyChange("CellSpanX");
                    OnPropertyChange("Width");
                }
            }
        }

        private int cellSpanY;
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
                    cellSpanY = value;
                    OnPropertyChange("CellSpanY");
                    OnPropertyChange("Height");
                }
            }
        }

        private int cellX;
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

        private int cellY;
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

        private int quantity;
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

        private bool isStackable;
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

        //private Item parent;
        /*public Item Parent
        {
            get
            {
                return parent;
            }
            set
            {
                if (parent != value)
                {
                    parent = value;
                    OnPropertyChange("Parent");
                }
            }
        }*/

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
                Image.DecodePixelWidth = (int)Math.Floor(CellWidth);
                OnPropertyChange("ImageUri");
                OnPropertyChange("Image");
            }
        }
        public BitmapImage Image { get; private set; }
        
        public ItemModel(int id, double width, double height, int column, int row, int spanX = 1, int spanY = 1, int quantity = 1, bool isStackable = false, Uri image = null)
        {
            logger.Debug($"> ItemModel(id: {id}, width: {width}, height: {height}, column: {column}, row: {row}, spanX: {spanX}, spanY: {spanY}, quantity: {quantity}, isStackable: {isStackable}, image: {image})");
            ID = id;
            CellWidth = width;
            CellHeight = height;
            CellSpanX = spanX;
            CellSpanY = spanY;
            CellX = column;
            CellY = row;
            Quantity = quantity;
            IsStackable = isStackable;
            ImageUri = image;
            Image = new BitmapImage(image);
            logger.Debug($"< ItemModel(id: {id}, width: {width}, height: {height}, column: {column}, row: {row}, spanX: {spanX}, spanY: {spanY}, quantity: {quantity}, isStackable: {isStackable}, image: {image})");
        }

        public override string ToString()
        {
            return $"ID: {ID}, CellSpan: [X: {CellSpanX}, Y: {CellSpanY}], Cell: [X: {CellX}, Y: {CellY}], Quantity: {Quantity}, IsStackable: {IsStackable}";
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
