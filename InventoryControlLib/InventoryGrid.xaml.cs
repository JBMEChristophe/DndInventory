using Easy.MessageHub;
using InventoryControlLib.Model;
using InventoryControlLib.View;
using InventoryControlLib.ViewModel;
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
    /// <summary>
    /// Interaction logic for InventoryGrid.xaml
    /// </summary>
    public partial class InventoryGrid : UserControl
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IMessageHub hub;
        private Guid itemSubscriptionToken;
        private Guid catalogSubscriptionToken;

        private GridManager manager;

        public InventoryGrid()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        public void Init()
        {
            logger.Info($"({Name})> Init()");
            if (Inventory.ColumnDefinitions.Count > 0 && Inventory.RowDefinitions.Count > 0)
            {
                manager = GridManager.Instance;
                manager.SetHub(MessageHub);

                for (int y = 0; y < Inventory.ColumnDefinitions.Count; y++)
                {
                    for (int x = 0; x < Inventory.RowDefinitions.Count; x++)
                    {
                        Border border = new Border
                        {
                            Background = new SolidColorBrush(Color.FromArgb(64,0,0,0)),
                            BorderBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                            BorderThickness = new Thickness(0.75),
                            Width = CellWidth,
                            Height = CellHeight,
                        };
                        Grid.SetColumn(border, y);
                        Grid.SetRow(border, x);
                        Inventory.Children.Add(border);
                    }
                }

                hub = MessageHub;
                itemSubscriptionToken = hub.Subscribe<ItemPositionUpdate>(ItemPositionUpdate);
                catalogSubscriptionToken = hub.Subscribe<CatalogItemPositionUpdate>(CatalogItemPositionUpdate);

                hub.Publish(new GridAddUpdate
                {
                    Grid = new UpdateGrid
                    {
                        Grid = Inventory,
                        CellSize = new Size(CellWidth, CellHeight),
                        Size = new Size(Columns * CellWidth, Rows * CellHeight)
                    }
                });
            }
            logger.Info($"({Name})< Init()");
        }

        public void AddItem(int id, string name, List<ItemType> types, string cost, string weight, string source, int x, int y, string imagePath, int spanX = 1, int spanY = 1, int quantity = 1, bool isStackable = false)
        {
            logger.Info($"({Name})> AddItem(id: {id}, x: {x}, y: {y}, imagePath: {imagePath}, spanX: {spanX}, spanY: {spanY}, quantity: {quantity}, isStackable: {isStackable})");
            Item item = new Item(hub, Inventory, CellWidth, CellHeight, new UiItemModel(id, name, types, cost, weight, source, CellWidth, CellHeight, x, y, spanX, spanY, quantity, isStackable, imagePath));
            Grid.SetColumnSpan(item, spanX);
            Grid.SetRowSpan(item, spanY);
            Grid.SetColumn(item, x);
            Grid.SetRow(item, y);
            item.MouseReleased += Item_MouseReleased;
            item.MousePressed += Item_MousePressed;
            item.ItemSplitClicked += Item_SplitClicked;
            Inventory.Children.Add(item); 
            logger.Info($"({Name})< AddItem(id: {id}, x: {x}, y: {y}, imagePath: {imagePath}, spanX: {spanX}, spanY: {spanY}, quantity: {quantity}, isStackable: {isStackable})");
        }

        private void AddItem(ItemModel item, int x, int y, int quantity)
        {
            AddItem(item.ID, item.Name, item.Type, item.Cost, item.Weight, item.Source, x, y, item.ImageUri, item.CellSpanX, item.CellSpanY, quantity, item.IsStackable);
        }

        private Point? NextAvailableCell(int spanX, int spanY)
        {
            logger.Info($"({Name})> NextAvailableCell(spanX: {spanX}, spanY: {spanY})");
            for (int y = 0; y < Inventory.RowDefinitions.Count; y++)
            {
                for (int x = 0; x < Inventory.ColumnDefinitions.Count; x++)
                {
                    bool cellAvailable = true;
                    foreach (var invItem in Inventory.Children)
                    {
                        if (invItem is Item)
                        {
                            var curItem = invItem as Item;
                            if (gridCellsOccupied(x, y, spanX, spanY, curItem))
                            {
                                cellAvailable = false;
                            }
                        }
                    }

                    if(cellAvailable)
                    {
                        var point = new Point(x, y);
                        logger.Info($"< NextAvailableCell(spanX: {spanX}, spanY: {spanY}).return({point})");
                        return point;
                    }
                }
            }

            logger.Info($"({Name})< NextAvailableCell(spanX: {spanX}, spanY: {spanY}).return(null)");
            return null;
        }

        private void Item_MousePressed(object sender, Point mousePosition)
        {
            logger.Trace($"({Name})>< Item_MousePressed(sender: {sender}, mousePosition: {mousePosition})");
            if (sender is Item)
            {
                var item = sender as Item;
                Panel.SetZIndex(this, 998);
                Panel.SetZIndex(item, 999);
            }
        }
        private void Item_SplitClicked(object sender)
        {
            logger.Debug($"({Name})> Item_SplitClicked(sender: {sender})");
            if (sender is Item)
            {
                var item = sender as Item;
                ItemSplitViewModel viewModel = new ItemSplitViewModel(item.Model);
                viewModel.SplitClicked += ItemDetailViewModel_SplitClicked;
                ItemSplitWindow detailWindow = new ItemSplitWindow(viewModel);
                detailWindow.ShowDialog();
            }
            logger.Debug($"({Name})< Item_SplitClicked(sender: {sender})");
        }

        private bool ItemDetailViewModel_SplitClicked(UiItemModel sender, int value)
        {
            logger.Debug($"({Name})> ItemDetailViewModel_SplitClicked(sender: {sender}, value: {value})");
            var cell = NextAvailableCell(sender.CellSpanX, sender.CellSpanY);
            if (cell.HasValue)
            {
                AddItem(sender, (int)cell.Value.X, (int)cell.Value.Y, value);
                sender.Quantity -= value;
                logger.Debug($"< ItemDetailViewModel_SplitClicked(sender: {sender}, value: {value}).return(True)");
                return true;
            }
            logger.Debug($"({Name})< ItemDetailViewModel_SplitClicked(sender: {sender}, value: {value}).return(False)");
            return false;
        }

        private bool gridCellsOccupied(int newX, int newY, int spanX, int spanY, Item i2)
        {
            logger.Debug($"({Name})> gridCellsOccupied(newX: {newX}, newY: {newY}, spanX: {spanX}, spanY: {spanY}, item: [{i2}])");
            var i1Start = new Point(newX, newY);
            var i1End = new Point(newX + spanX - 1, newY + spanY - 1);
            var i2Start = new Point(i2.Model.CellX, i2.Model.CellY);
            var i2End = new Point(i2.Model.CellX + i2.Model.CellSpanX - 1, i2.Model.CellY + i2.Model.CellSpanY - 1);

            var result = (i2End.X >= i1Start.X && i2Start.X <= i1End.X) && (i2End.Y >= i1Start.Y && i2Start.Y <= i1End.Y);
            logger.Debug($"({Name})< gridCellsOccupied(newX: {newX}, newY: {newY}, spanX: {spanX}, spanY: {spanY}, item: [{i2}]).return({result})");
            return result;
        }

        private bool IsWithinGridBoundary(int newX, int newY, int spanX, int spanY)
        {
            logger.Debug($"({Name})> IsWithinGridBoundary(newX: {newX}, newY: {newY}, spanX: {spanX}, spanY: {spanY})");
            var i1Start = new Point(newX, newY);
            var i1End = new Point(newX + spanX - 1, newY + spanY - 1);
            var i2Start = new Point(0, 0);
            var i2End = new Point(Inventory.ColumnDefinitions.Count - 1, Inventory.RowDefinitions.Count - 1);

            var result = (i1Start.X >= i2Start.X && i1End.X <= i2End.X) && (i1Start.Y >= i2Start.Y && i1End.Y <= i2End.Y);
            logger.Debug($"({Name})> IsWithinGridBoundary(newX: {newX}, newY: {newY}, spanX: {spanX}, spanY: {spanY}).reutn({result})");
            return result;
        }

        private void ItemPositionUpdate(ItemPositionUpdate positionUpdate)
        {
            logger.Debug($"({Name})> ItemPositionUpdate(positionUpdate: {positionUpdate})");
            var item = positionUpdate.Item;
            var releasePoint = positionUpdate.Position;

            var width = Columns * CellWidth;
            var height = Rows * CellHeight;
            var screenPoint = Inventory.TranslatePoint(new Point(0, 0), Application.Current.MainWindow);

            if (releasePoint.X < screenPoint.X + width && releasePoint.Y < screenPoint.Y + height
                && releasePoint.X > screenPoint.X && releasePoint.Y > screenPoint.Y)
            {
                var cellX = (releasePoint.X - screenPoint.X) / CellWidth;
                var cellY = (releasePoint.Y - screenPoint.Y) / CellHeight;
                var icellX = (int)Math.Floor(cellX);
                var icellY = (int)Math.Floor(cellY);

                if (cellX % 1 > 0.6)
                {
                    icellX++;
                }
                if (cellY % 1 > 0.6)
                {
                    icellY++;
                }

                var x = icellX * CellWidth + screenPoint.X;
                var y = icellY * CellHeight + screenPoint.Y;

                bool cancelMove = false;
                bool stacked = false;
                foreach (var invItem in Inventory.Children)
                {
                    if (invItem is Item)
                    {
                        var curItem = invItem as Item;
                        if (curItem == item)
                        { 
                            continue;
                        }
                        if (gridCellsOccupied(icellX, icellY, item.Model.CellSpanX, item.Model.CellSpanY, curItem))
                        {
                            if(item.Model.ID == curItem.Model.ID && item.Model.IsStackable)
                            {
                                curItem.Model.Quantity += item.Model.Quantity;
                                stacked = true;
                                break;
                            }
                            cancelMove = true;
                            break;
                        }
                    }
                }
                if (!IsWithinGridBoundary(icellX, icellY, item.Model.CellSpanX, item.Model.CellSpanY))
                {
                    cancelMove = true;
                }

                Point p = new Point(x, y);
                if (cancelMove)
                {
                    var parentPoint = item.GridParent.TranslatePoint(new Point(0, 0), Application.Current.MainWindow);
                    var startingX = item.Model.CellX * CellWidth + parentPoint.X;
                    var startingY = item.Model.CellY * CellHeight + parentPoint.Y;
                    p = new Point(startingX, startingY);
                    item.Transform(p);
                }
                else
                {
                    if (item.GridParent.Children.Contains(item))
                    {
                        item.GridParent.Children.Remove(item);
                        item.RemoveEvents();
                    }

                    if (!stacked)
                    {
                        item.MouseReleased += Item_MouseReleased;
                        item.MousePressed += Item_MousePressed;
                        item.ItemSplitClicked += Item_SplitClicked;
                        Inventory.Children.Add(item);
                        item.GridParent = Inventory;
                        item.Model.CellX = icellX;
                        item.Model.CellY = icellY;
                        item.Transform(p);
                    }
                }
            }
            else
            {
                logger.Debug($"Ignored");
            }

            logger.Debug($"({Name})< ItemPositionUpdate(positionUpdate: {positionUpdate})");
        }

        private void CatalogItemPositionUpdate(CatalogItemPositionUpdate positionUpdate)
        {
            logger.Debug($"({Name})> CatalogItemPositionUpdate(positionUpdate: {positionUpdate})");
            var item = positionUpdate.Item;
            var releasePoint = positionUpdate.Position;

            var width = Columns * CellWidth;
            var height = Rows * CellHeight;
            var screenPoint = Inventory.TranslatePoint(new Point(0, 0), Application.Current.MainWindow);

            if (releasePoint.X < screenPoint.X + width && releasePoint.Y < screenPoint.Y + height
                && releasePoint.X > screenPoint.X && releasePoint.Y > screenPoint.Y)
            {
                var cellX = (releasePoint.X - screenPoint.X) / CellWidth;
                var cellY = (releasePoint.Y - screenPoint.Y) / CellHeight;
                var icellX = (int)Math.Floor(cellX);
                var icellY = (int)Math.Floor(cellY);

                if (cellX % 1 > 0.6)
                {
                    icellX++;
                }
                if (cellY % 1 > 0.6)
                {
                    icellY++;
                }

                bool canBePlaced = true;
                Item placedItem = null;
                foreach (var invItem in Inventory.Children)
                {
                    if (invItem is Item)
                    {
                        var curItem = invItem as Item;
                        if (gridCellsOccupied(icellX, icellY, item.Model.CellSpanX, item.Model.CellSpanY, curItem))
                        {
                            canBePlaced = false;
                            placedItem = curItem;
                            break;
                        }
                    }
                }
                if (!IsWithinGridBoundary(icellX, icellY, item.Model.CellSpanX, item.Model.CellSpanY))
                {
                    canBePlaced = false;
                }

                int amount = 1;
                if(Keyboard.IsKeyDown(Key.LeftCtrl) && item.Model.IsStackable)
                {
                    var amountWindow = new AmountWindow();
                    amountWindow.Owner = Application.Current.MainWindow;
                    amountWindow.ShowDialog();
                    amount = amountWindow._viewModel.Value;
                }

                if (canBePlaced)
                {
                    AddItem(item.Model, icellX, icellY, amount);
                }
                else if(placedItem != null && placedItem.Model.ID == item.Model.ID && item.Model.IsStackable)
                {
                    placedItem.Model.Quantity += amount;
                }
            }
            else
            {
                logger.Debug($"Ignored");
            }

            logger.Debug($"({Name})< ItemPositionUpdate(positionUpdate: {positionUpdate})");
        }

        private void Item_MouseReleased(object sender, Point mousePosition)
        {
            logger.Trace($"({Name})>< Item_MouseReleased(sender: {sender}, mousePosition: {mousePosition})");
            if (sender is Item)
            {
                var item = sender as Item;
                Panel.SetZIndex(this, 1);
                Panel.SetZIndex(item, 2);
            }
        }

        #region DependencyProperty Content

        /// <summary>
        /// Registers a dependency property as backing store for the Rows property
        /// </summary>
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(int), typeof(InventoryGrid));

        /// <summary>
        /// Gets or sets the Rows.
        /// </summary>
        /// <value>The number of Rows.</value>
        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        /// <summary>
        /// Registers a dependency property as backing store for the Columns property
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(int), typeof(InventoryGrid));

        /// <summary>
        /// Gets or sets the Columns.
        /// </summary>
        /// <value>The number of Columns.</value>
        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        /// <summary>
        /// Registers a dependency property as backing store for the Columns property
        /// </summary>
        public static readonly DependencyProperty CellWidthProperty =
            DependencyProperty.Register("CellWidth", typeof(double), typeof(InventoryGrid),
            new PropertyMetadata(50.0));

        /// <summary>
        /// Gets or sets the Columns.
        /// </summary>
        /// <value>The number of Columns.</value>
        public double CellWidth
        {
            get { return (double)GetValue(CellWidthProperty); }
            set { SetValue(CellWidthProperty, value); }
        }

        /// <summary>
        /// Registers a dependency property as backing store for the Columns property
        /// </summary>
        public static readonly DependencyProperty CellHeightProperty =
            DependencyProperty.Register("CellHeight", typeof(double), typeof(InventoryGrid),
            new PropertyMetadata(50.0));

        /// <summary>
        /// Gets or sets the Columns.
        /// </summary>
        /// <value>The number of Columns.</value>
        public double CellHeight
        {
            get { return (double)GetValue(CellHeightProperty); }
            set { SetValue(CellHeightProperty, value); }
        }

        /// <summary>
        /// Registers a dependency property as backing store for the Columns property
        /// </summary>
        public static readonly DependencyProperty MessageHubProperty =
            DependencyProperty.Register("MessageHub", typeof(IMessageHub), typeof(InventoryGrid),
            new PropertyMetadata(new MessageHub()));

        /// <summary>
        /// Gets or sets the Columns.
        /// </summary>
        /// <value>The number of Columns.</value>
        public IMessageHub MessageHub
        {
            get { return (IMessageHub)GetValue(MessageHubProperty); }
            set { SetValue(MessageHubProperty, value); }
        }

        #endregion
    }
}
