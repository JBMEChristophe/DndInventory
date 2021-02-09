﻿using Easy.MessageHub;
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
        private Guid subscriptionToken;

        private GridManager manager;

        public InventoryGrid()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        public void Init()
        {
            logger.Info($"> Init()");
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
                subscriptionToken = hub.Subscribe<ItemPositionUpdate>(ItemPositionUpdate);

                hub.Publish(new GridAddUpdate
                {
                    Grid = new UpdateGrid
                    {
                        Grid = Inventory,
                        CellSize = new Size(CellWidth, CellHeight),
                        Size = new Size(Columns * CellWidth, Rows * CellHeight)
                    }
                });

                AddItem(0, 0, 0, "https://www.clipartmax.com/png/full/414-4147920_bow-arrow-symbol-vector-icon-illustration-triangle.png", isStackable: true, quantity: 5);
                AddItem(1, 1, 0, "https://icons.iconarchive.com/icons/chanut/role-playing/256/Sword-icon.png", spanY: 2);
                AddItem(2, 0, 2, "https://icons.iconarchive.com/icons/google/noto-emoji-objects/128/62967-shield-icon.png", spanY: 3, spanX: 3);
                AddItem(3, 2, 1, "https://i.pinimg.com/originals/8f/ef/44/8fef443afeefd9ab9ea353fc8db7bbf3.png", isStackable: true, quantity: 10);
            }
            logger.Info($"< Init()");
        }

        private void AddItem(int id, int x, int y, string imagePath, int spanX = 1, int spanY = 1, int quantity = 1, bool isStackable = false)
        {
            logger.Info($"> AddItem(id: {id}, x: {x}, y: {y}, imagePath: {imagePath}, spanX: {spanX}, spanY: {spanY}, quantity: {quantity}, isStackable: {isStackable})");
            Item item = new Item(hub, Inventory, CellWidth, CellHeight, new ItemModel(id, CellWidth, CellHeight, x, y, spanX, spanY, quantity, isStackable, new Uri(imagePath)));
            Grid.SetColumnSpan(item, spanX);
            Grid.SetRowSpan(item, spanY);
            Grid.SetColumn(item, x);
            Grid.SetRow(item, y);
            item.MouseReleased += Item_MouseReleased;
            item.MousePressed += Item_MousePressed;
            item.ItemSplitClicked += Item_SplitClicked;
            Inventory.Children.Add(item); 
            logger.Info($"< AddItem(id: {id}, x: {x}, y: {y}, imagePath: {imagePath}, spanX: {spanX}, spanY: {spanY}, quantity: {quantity}, isStackable: {isStackable})");
        }

        private void AddItem(ItemModel item, int x, int y, int quantity)
        {
            AddItem(item.ID, x, y, item.Image.ToString(), item.CellSpanX, item.CellSpanY, quantity, item.IsStackable);
        }

        private Point? NextAvailableCell(int spanX, int spanY)
        {
            logger.Info($"> NextAvailableCell(spanX: {spanX}, spanY: {spanY})");
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

            logger.Info($"< NextAvailableCell(spanX: {spanX}, spanY: {spanY}).return(null)");
            return null;
        }

        private void Item_MousePressed(object sender, Point mousePosition)
        {
            logger.Trace($">< Item_MousePressed(sender: {sender}, mousePosition: {mousePosition})");
            if (sender is Item)
            {
                var item = sender as Item;
                Panel.SetZIndex(this, 998);
                Panel.SetZIndex(item, 999);
            }
        }
        private void Item_SplitClicked(object sender)
        {
            logger.Debug($"> Item_SplitClicked(sender: {sender})");
            if (sender is Item)
            {
                var item = sender as Item;
                ItemSplitViewModel viewModel = new ItemSplitViewModel(item.Model);
                viewModel.SplitClicked += ItemDetailViewModel_SplitClicked;
                ItemSplitWindow detailWindow = new ItemSplitWindow(viewModel);
                detailWindow.ShowDialog();
            }
            logger.Debug($"< Item_SplitClicked(sender: {sender})");
        }

        private bool ItemDetailViewModel_SplitClicked(ItemModel sender, int value)
        {
            logger.Debug($"> ItemDetailViewModel_SplitClicked(sender: {sender}, value: {value})");
            var cell = NextAvailableCell(sender.CellSpanX, sender.CellSpanY);
            if (cell.HasValue)
            {
                AddItem(sender, (int)cell.Value.X, (int)cell.Value.Y, value);
                sender.Parent.Quantity -= value;
                logger.Debug($"< ItemDetailViewModel_SplitClicked(sender: {sender}, value: {value}).return(True)");
                return true;
            }
            logger.Debug($"< ItemDetailViewModel_SplitClicked(sender: {sender}, value: {value}).return(False)");
            return false;
        }

        private bool gridCellsOccupied(int newX, int newY, int spanX, int spanY, Item i2)
        {
            logger.Debug($"> gridCellsOccupied(newX: {newX}, newY: {newY}, spanX: {spanX}, spanY: {spanY}, item: [{i2}])");
            var i1Start = new Point(newX, newY);
            var i1End = new Point(newX + spanX - 1, newY + spanY - 1);
            var i2Start = new Point(i2.Column, i2.Row);
            var i2End = new Point(i2.Column + i2.ColumnSpan - 1, i2.Row + i2.RowSpan - 1);

            var result = (i2End.X >= i1Start.X && i2Start.X <= i1End.X) && (i2End.Y >= i1Start.Y && i2Start.Y <= i1End.Y);
            logger.Debug($"< gridCellsOccupied(newX: {newX}, newY: {newY}, spanX: {spanX}, spanY: {spanY}, item: [{i2}]).return({result})");
            return result;
        }

        private bool IsWithinGridBoundary(int newX, int newY, int spanX, int spanY)
        {
            logger.Debug($"> IsWithinGridBoundary(newX: {newX}, newY: {newY}, spanX: {spanX}, spanY: {spanY})");
            var i1Start = new Point(newX, newY);
            var i1End = new Point(newX + spanX - 1, newY + spanY - 1);
            var i2Start = new Point(0, 0);
            var i2End = new Point(Inventory.ColumnDefinitions.Count - 1, Inventory.RowDefinitions.Count - 1);

            var result = (i1Start.X >= i2Start.X && i1End.X <= i2End.X) && (i1Start.Y >= i2Start.Y && i1End.Y <= i2End.Y);
            logger.Debug($"> IsWithinGridBoundary(newX: {newX}, newY: {newY}, spanX: {spanX}, spanY: {spanY}).reutn({result})");
            return result;
        }

        private void ItemPositionUpdate(ItemPositionUpdate positionUpdate)
        {
            logger.Debug($"> ItemPositionUpdate(positionUpdate: {positionUpdate})");
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
                        if (gridCellsOccupied(icellX, icellY, item.ColumnSpan, item.RowSpan, curItem))
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
                if (!IsWithinGridBoundary(icellX, icellY, item.ColumnSpan, item.RowSpan))
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
                        item.Column = icellX;
                        item.Row = icellY;
                        item.Transform(p);
                    }
                }
            }
            else
            {
                logger.Debug($"Ignored");
            }

            logger.Debug($"< ItemPositionUpdate(positionUpdate: {positionUpdate})");
        }

        private void Item_MouseReleased(object sender, Point mousePosition)
        {
            logger.Trace($">< Item_MouseReleased(sender: {sender}, mousePosition: {mousePosition})");
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
