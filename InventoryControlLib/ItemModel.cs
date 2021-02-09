using InventoryControlLib.View;
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
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public int ID { get; set; }
        public double CellWidth { get; set; }
        public double CellHeight { get; set; }
        public int CellSpanX { get; set; }
        public int CellSpanY { get; set; }
        public int CellX { get; set; }
        public int CellY { get; set; }
        public int Quantity { get; set; }
        public bool IsStackable { get; set; }
        public Item Parent { get; set; }

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
                BitMapImage.DecodePixelWidth = (int)Math.Floor(CellWidth);
            }
        }
        public BitmapImage BitMapImage { get; set; }
        
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
            Image = image;
            BitMapImage = new BitmapImage(image);
            logger.Debug($"< ItemModel(id: {id}, width: {width}, height: {height}, column: {column}, row: {row}, spanX: {spanX}, spanY: {spanY}, quantity: {quantity}, isStackable: {isStackable}, image: {image})");
        }

        public override string ToString()
        {
            return $"ID: {ID}, CellSpan: [X: {CellSpanX}, Y: {CellSpanY}], Cell: [X: {CellX}, Y: {CellY}], Quantity: {Quantity}, IsStackable: {IsStackable}";
        }
    }
}
