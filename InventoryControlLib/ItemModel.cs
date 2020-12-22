using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace InventoryControlLib
{
    public class ItemModel
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public int CellSpanX { get; set; }
        public int CellSpanY { get; set; }
        public int Quantity { get; set; }
        public bool IsStackable { get; set; }

        Uri imageUri;
        public Uri Image 
        { 
            get
            {
                return imageUri;
            }
            set
            {
                imageUri = value;
                BitMapImage = new BitmapImage(imageUri);
                BitMapImage.DecodePixelWidth = (int)Math.Floor(Width);
            }
        }
        public BitmapImage BitMapImage { get; set; }

        public ItemModel(double width, double height) : this(width, height, 1, 1, 1, false, null)
        { }

        public ItemModel(double width, double height, int spanX, int spanY) : this(width, height, spanX, spanY, 1, false, null)
        { }

        public ItemModel(double width, double height, int quantity) : this(width, height, 1, 1, quantity, quantity > 1, null)
        { }

        public ItemModel(double width, double height, int spanX, int spanY, int quantity) : this(width, height, spanX, spanY, quantity, true, null)
        { }

        public ItemModel(double width, double height, int spanX, int spanY, Uri image) : this(width, height, spanX, spanY, 1, false, image)
        { }

        public ItemModel(double width, double height, Uri image) : this(width, height, 1, 1, 1, false, image)
        { }

        public ItemModel(double width, double height, int spanX, int spanY, int quantity, bool isStackable, Uri image)
        {
            Width = width;
            Height = height;
            CellSpanX = spanX;
            CellSpanY = spanY;
            Quantity = quantity;
            IsStackable = isStackable;
            Image = image;
            BitMapImage = new BitmapImage(image);
        }
    }
}
