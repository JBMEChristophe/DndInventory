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
        public int ID { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int CellSpanX { get; set; }
        public int CellSpanY { get; set; }
        public int CellX { get; set; }
        public int CellY { get; set; }
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
        
        public ItemModel(int id, double width, double height, int column, int row, int spanX = 1, int spanY = 1, int quantity = 1, bool isStackable = false, Uri image = null)
        {
            ID = id;
            Width = width;
            Height = height;
            CellSpanX = spanX;
            CellSpanY = spanY;
            CellX = column;
            CellY = row;
            Quantity = quantity;
            IsStackable = isStackable;
            Image = image;
            BitMapImage = new BitmapImage(image);
        }
    }
}
