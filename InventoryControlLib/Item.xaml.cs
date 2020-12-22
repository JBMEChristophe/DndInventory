using Easy.MessageHub;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InventoryControlLib
{
    public delegate void MousePressEvent(object sender, Point mousePosition);

    /// <summary>
    /// Interaction logic for Item.xaml
    /// </summary>
    public partial class Item : UserControl, INotifyPropertyChanged
    {
        private readonly IMessageHub hub;
        TranslateTransform transform = new TranslateTransform();
        Grid parent;
        ItemModel model;

        Point anchorPoint;
        Point currentPoint;
        bool isInDrag = false;

        public event MousePressEvent MouseReleased;
        public event MousePressEvent MousePressed;

        public Item(IMessageHub hub, Grid parent, double width, double height, ItemModel model = null)
        {
            InitializeComponent();
            this.hub = hub;
            this.parent = parent;
            if (model == null)
            {
                this.model = new ItemModel(width, height);
            }
            else
            {
                this.model = model;
                OnPropertyChange("Image");
            }
            cellWidth = width;
            cellHeight = height;
        }

        double _cellWidth;
        double cellWidth
        {
            get
            {
                return _cellWidth;
            }
            set
            {
                _cellWidth = value;
                OnPropertyChange("ItemWidth");
            }
        }

        public double ItemWidth 
        { 
            get
            {
                if (model == null)
                {
                    return cellWidth;
                }
                return cellWidth * model.CellSpanX;
            }
        }

        double _cellHeight;
        double cellHeight
        {
            get
            {
                return _cellHeight;
            }
            set
            {
                _cellHeight = value;
                OnPropertyChange("ItemHeight");
            }
        }

        public BitmapImage Image
        {
            get
            {
                if (model == null)
                {
                    return new BitmapImage();
                }
                    return model.BitMapImage;
            }
        }

        public Uri ImageUri
        {
            get
            {
                if (model == null)
                {
                    return new Uri("");
                }
                return model.Image;
            }
            set
            {
                model.Image = value;
                OnPropertyChange("ImageUri");
                OnPropertyChange("Image");
            }
        }

        public double ItemHeight
        {
            get
            {
                if (model == null)
                {
                    return cellHeight;
                }
                return cellHeight * model.CellSpanY;
            }
        }

        public Grid GridParent
        {
            get
            {
                return parent;
            }
            set
            {
                if(parent != value)
                {
                    parent = value;
                }
            }
        }

        public void Transform(Point p)
        {
            var screenPoint = TranslatePoint(new Point(0, 0), Application.Current.MainWindow);
            transform.X += (p.X - screenPoint.X);
            transform.Y += (p.Y - screenPoint.Y);
            this.RenderTransform = transform;
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            anchorPoint = e.GetPosition(null);
            element.CaptureMouse();
            isInDrag = true;
            e.Handled = true;
            MousePressed?.Invoke(this, e.GetPosition(null));
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isInDrag)
            {
                var element = sender as FrameworkElement;
                element.ReleaseMouseCapture();
                isInDrag = false;
                e.Handled = true;
                currentPoint = e.GetPosition(null);
                MouseReleased?.Invoke(this, currentPoint);
                hub.Publish(new ItemPositionUpdate { Item = this, Position = currentPoint});
            }
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isInDrag)
            {
                var element = sender as FrameworkElement;
                currentPoint = e.GetPosition(null);

                transform.X += (currentPoint.X - anchorPoint.X);
                transform.Y += (currentPoint.Y - anchorPoint.Y);
                if (currentPoint.X < Application.Current.MainWindow.RenderSize.Width && currentPoint.Y < Application.Current.MainWindow.RenderSize.Height
                    && currentPoint.X > 0 && currentPoint.Y > 0)
                {
                    this.RenderTransform = transform;
                    anchorPoint = currentPoint;
                }
                else
                {
                    transform = new TranslateTransform();
                    this.RenderTransform = transform;
                }
            }
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
