using Easy.MessageHub;
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
        private IMessageHub hub;
        private Guid subscriptionToken;

        private GridManager manager;

        public InventoryGrid()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        public override void EndInit()
        {
            base.EndInit();

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
                            Background = Brushes.LightGray,
                            BorderBrush = Brushes.Black,
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
                subscriptionToken = hub.Subscribe<ItemPositionUpdate>(ItemPositionUpdate);

                hub.Publish(new GridAddUpdate
                {
                    Grid = new UpdateGrid
                    {
                        ScreenPoint = TranslatePoint(new Point(0, 0), Application.Current.MainWindow),
                        CellSize = new Size(CellWidth, CellHeight),
                        Size = new Size(Columns * CellWidth, Rows * CellHeight)
                    }
                });

                AddItem(0, 0, 0, "https://www.clipartmax.com/png/full/414-4147920_bow-arrow-symbol-vector-icon-illustration-triangle.png", isStackable: true , quantity: 5);
                AddItem(1, 1, 0, "https://icons.iconarchive.com/icons/chanut/role-playing/256/Sword-icon.png", spanY: 2);
                AddItem(2, 0, 2, "https://icons.iconarchive.com/icons/google/noto-emoji-objects/128/62967-shield-icon.png", spanY: 3, spanX:3);
            }
        }

        private void AddItem(int id, int x, int y, string imagePath, int spanX = 1, int spanY = 1, int quantity = 1, bool isStackable = false)
        {
            Item item = new Item(hub, Inventory, CellWidth, CellHeight, new ItemModel(id, CellWidth, CellHeight, x, y, spanX, spanY, quantity, isStackable, new Uri(imagePath)));
            Grid.SetColumnSpan(item, spanX);
            Grid.SetRowSpan(item, spanY);
            Grid.SetColumn(item, x);
            Grid.SetRow(item, y);
            item.MouseReleased += Item_MouseReleased;
            item.MousePressed += Item_MousePressed;
            item.ItemSplitClicked += Item_SplitClicked;
            Inventory.Children.Add(item);
        }

        private void AddItem(ItemModel item, int x, int y, int quantity)
        {
            AddItem(item.ID, x, y, item.Image.ToString(), item.CellSpanX, item.CellSpanY, quantity, item.IsStackable);
        }

        private Point? NextAvailableCell(int spanX, int spanY)
        {
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
                        return new Point(x, y);
                    }
                }
            }

            return null;
        }

        private void Item_MousePressed(object sender, Point mousePosition)
        {
            if (sender is Item)
            {
                var item = sender as Item;
                Panel.SetZIndex(this, 998);
                Panel.SetZIndex(item, 999);
            }
        }
        private void Item_SplitClicked(object sender)
        {
            if (sender is Item)
            {
                var item = sender as Item;
                ItemSplitViewModel viewModel = new ItemSplitViewModel(item.Model);
                viewModel.SplitClicked += ItemDetailViewModel_SplitClicked;
                ItemSplitWindow detailWindow = new ItemSplitWindow(viewModel);
                detailWindow.ShowDialog();
            }
        }

        private bool ItemDetailViewModel_SplitClicked(ItemModel sender, int value)
        {
            var cell = NextAvailableCell(sender.CellSpanX, sender.CellSpanY);
            if (cell.HasValue)
            {
                AddItem(sender, (int)cell.Value.X, (int)cell.Value.Y, value);
                sender.Parent.Quantity -= value;
                return true;
            }
            return false;
        }

        private bool gridCellsOccupied(int newX, int newY, int spanX, int spanY, Item i2)
        {
            var i1Start = new Point(newX, newY);
            var i1End = new Point(newX + spanX - 1, newY + spanY - 1);
            var i2Start = new Point(i2.Column, i2.Row);
            var i2End = new Point(i2.Column + i2.ColumnSpan - 1, i2.Row + i2.RowSpan - 1);

            return (i2End.X >= i1Start.X && i2Start.X <= i1End.X) && (i2End.Y >= i1Start.Y && i2Start.Y <= i1End.Y);
        }

        private bool IsWithinGridBoundary(int newX, int newY, int spanX, int spanY)
        {
            var i1Start = new Point(newX, newY);
            var i1End = new Point(newX + spanX - 1, newY + spanY - 1);
            var i2Start = new Point(0, 0);
            var i2End = new Point(Inventory.ColumnDefinitions.Count - 1, Inventory.RowDefinitions.Count - 1);

            return (i1Start.X >= i2Start.X && i1End.X <= i2End.X) && (i1Start.Y >= i2Start.Y && i1End.Y <= i2End.Y);
        }

        private void ItemPositionUpdate(ItemPositionUpdate positionUpdate)
        {
            var item = positionUpdate.Item;
            var releasePoint = positionUpdate.Position;

            var width = Columns * CellWidth;
            var height = Rows * CellHeight;
            var screenPoint = TranslatePoint(new Point(0, 0), Application.Current.MainWindow);

            if (releasePoint.X < screenPoint.X + width && releasePoint.Y < screenPoint.Y + height
                && releasePoint.X > screenPoint.X && releasePoint.Y > screenPoint.Y)
            {
                var cellX = (int)Math.Floor((releasePoint.X - screenPoint.X) / CellWidth);
                var cellY = (int)Math.Floor((releasePoint.Y - screenPoint.Y) / CellHeight);
                var x = cellX * CellWidth + screenPoint.X;
                var y = cellY * CellHeight + screenPoint.Y;

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
                        if (gridCellsOccupied(cellX, cellY, item.ColumnSpan, item.RowSpan, curItem))
                        {
                            if(item.ID == curItem.ID && item.IsStackable)
                            {
                                curItem.Quantity += item.Quantity;
                                stacked = true;
                                break;
                            }
                            cancelMove = true;
                            break;
                        }
                    }
                }
                if (!IsWithinGridBoundary(cellX, cellY, item.ColumnSpan, item.RowSpan))
                {
                    cancelMove = true;
                }

                Point p = new Point(x, y);
                if (cancelMove)
                {
                    var parentPoint = item.GridParent.TranslatePoint(new Point(0, 0), Application.Current.MainWindow);
                    var startingX = item.Column * CellWidth + parentPoint.X;
                    var startingY = item.Row * CellHeight + parentPoint.Y;
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
                        item.Column = cellX;
                        item.Row = cellY;
                        item.Transform(p);
                    }
                }
            }
        }

        private void Item_MouseReleased(object sender, Point mousePosition)
        {
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
