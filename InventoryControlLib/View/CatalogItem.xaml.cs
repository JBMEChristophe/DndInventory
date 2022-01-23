using Easy.MessageHub;
using InventoryControlLib.Model;
using InventoryControlLib.ViewModel;
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
        public delegate void CatalogEvent(CatalogItem sender);

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IMessageHub hub;
        TranslateTransform transform = new TranslateTransform();
        Point currentPoint;
        Point startingPoint;
        bool isInDrag = false;
        Popup popup;

        public event MousePressEvent MouseReleased;
        public event MousePressEvent MousePressed;
        public event CatalogEvent SaveCatalog;
        public event CatalogEvent DeleteCatalog;

        public CatalogItem(IMessageHub hub, CatalogItemModel model = null, bool popupItem = false)
        {
            logger.Info($"> CatalogItem(hub: {hub}, model: {model}, popup: {popupItem})");
            InitializeComponent();
            if (popupItem)
            {
                this.Model = new CatalogItemModel(model);

                OnPropertyChange("Model");
                return;
            }

            this.hub = hub;
            if (model == null)
            {
                this.Model = new CatalogItemModel("No_ID", "No Name", ItemType.Unknown, "No Cost", "No Weight", "No Rarity", "No Attunement", "No Properties", "No Description", "UNKOWN", 50, 50, 0, 0);
            }
            else
            {
                this.Model = model;
            }

            popup = new Popup { Child = new CatalogItem(hub, model, true), 
                PlacementTarget = this, 
                Placement = PlacementMode.Relative, 
                AllowsTransparency = true };

            OnPropertyChange("Model");
            logger.Info($"< CatalogItem(hub: {hub}, model: {model}, popup: {popupItem})");
        }

        public CatalogItemModel Model { get; }

        DelegateCommand editCommand;
        public ICommand EditCommand
        {
            get
            {
                if (editCommand == null)
                {
                    editCommand = new DelegateCommand(ExecuteEdit);
                }
                return editCommand;
            }
        }

        private void ExecuteEdit()
        {
            logger.Info($"> ExecuteEdit()");

            var viewModel = new ItemEditViewModel(Model);
            var editWindow = new ItemEditWindow(viewModel);
            editWindow.ShowDialog();

            popup = new Popup
            {
                Child = new CatalogItem(hub, Model, true),
                PlacementTarget = this,
                Placement = PlacementMode.Relative,
                AllowsTransparency = true
            };

            SaveCatalog?.Invoke(this);

            logger.Info($"< ExecuteEdit()");
        }

        DelegateCommand deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new DelegateCommand(Executedelete);
                }
                return deleteCommand;
            }
        }

        private void Executedelete()
        {
            logger.Info($"> Executedelete()");

            DeleteCatalog?.Invoke(this);

            logger.Info($"< Executedelete()");
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            startingPoint = e.GetPosition(this);
            element.CaptureMouse();
            isInDrag = true;
            Panel.SetZIndex(this, 999);
            e.Handled = true;
            if (popup != null)
            {
                popup.IsOpen = true;
            }
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
