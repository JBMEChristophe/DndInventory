using Easy.MessageHub;
using InventoryControlLib.Model;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InventoryControlLib.View
{
    /// <summary>
    /// Interaction logic for Item.xaml
    /// </summary>
    public partial class CatalogItem : UserControl, INotifyPropertyChanged
    {
        private readonly IMessageHub hub;
        TranslateTransform transform = new TranslateTransform();
        Point currentPoint;
        Point startingPoint;
        bool isInDrag = false;
        Popup popup;

        public event MousePressEvent MouseReleased;
        public event MousePressEvent MousePressed;

        public CatalogItem(IMessageHub hub, double width, double height, CatalogItemModel model = null, bool popupItem = false)
        {
            InitializeComponent();
            if (popupItem)
            {
                this.Model = new CatalogItemModel(model, width, height);

                OnPropertyChange("Model");
                return;
            }

            this.hub = hub;
            if (model == null)
            {
                this.Model = new CatalogItemModel(0, "No Name", ItemType.Unknown, "No Cost", "No Weight", "UNKOWN", width, height, 0, 0);
            }
            else
            {
                this.Model = new CatalogItemModel(model, width, height);
            }

            popup = new Popup { Child = new CatalogItem(hub, width, height, model, true), PlacementTarget = this, Placement = PlacementMode.Relative };

            OnPropertyChange("Model");
        }

        public CatalogItemModel Model { get; }

        public void RemoveEvents()
        {
            MouseReleased = null;
            MousePressed = null;
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
            startingPoint = e.GetPosition(this);
            element.CaptureMouse();
            isInDrag = true;
            Panel.SetZIndex(this, 999);
            e.Handled = true;
            popup.IsOpen = true;
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
                MouseReleased?.Invoke(this, e.GetPosition(null));
                var itemPosition = TranslatePoint(new Point(0, 0), Application.Current.MainWindow);
                hub.Publish(new CatalogItemPositionUpdate { Item = this, Position = itemPosition });
                Matrix m = this.RenderTransform.Value;
                m.SetIdentity();
                this.RenderTransform = new MatrixTransform(m);
                Panel.SetZIndex(this, 2);
                popup.IsOpen = false;
            }
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isInDrag)
            {
                currentPoint = e.GetPosition(null);

                if (currentPoint.X < Application.Current.MainWindow.RenderSize.Width && currentPoint.Y < Application.Current.MainWindow.RenderSize.Height
                    && currentPoint.X > 0 && currentPoint.Y > 0)
                {
                    correctItemOnMousePos();
                }
                else
                {
                    var screenPoint = TranslatePoint(new Point(0, 0), Application.Current.MainWindow);
                    if (screenPoint.X + Model.Width > Application.Current.MainWindow.RenderSize.Width)
                    {
                        transform.X += ((Application.Current.MainWindow.Width - screenPoint.X) - Model.Width);
                    }
                    if (screenPoint.X < 0)
                    {
                        transform.X += (-screenPoint.X);
                    }
                    if (screenPoint.Y + Model.Height > Application.Current.MainWindow.RenderSize.Height)
                    {
                        transform.Y += (Application.Current.MainWindow.Height - (screenPoint.Y + Model.Height));
                    }
                    if (screenPoint.Y < 0)
                    {
                        transform.Y += (-screenPoint.Y);
                    }
                    this.RenderTransform = transform;
                }
            }
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (isInDrag)
            {
                correctItemOnMousePos();
            }
        }

        private void correctItemOnMousePos()
        {
            if (isInDrag)
            {
                var currentPointDiff = Mouse.GetPosition(this);
                transform.X += currentPointDiff.X - startingPoint.X;
                transform.Y += currentPointDiff.Y - startingPoint.Y;
                this.RenderTransform = transform;
                popup.HorizontalOffset += 1;
                popup.HorizontalOffset -= 1;
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

        public override string ToString()
        {
            return Model.ToString();
        }
    }
}
