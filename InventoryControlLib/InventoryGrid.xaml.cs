using Easy.MessageHub;
using InventoryControlLib.Model;
using InventoryControlLib.View;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utilities;
using Utilities.Sockets.EventSocket.Messages;

namespace InventoryControlLib
{
    /// <summary>
    /// Interaction logic for InventoryGrid.xaml
    /// </summary>
    public partial class InventoryGrid : UserControl, INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public delegate bool InventoryGridEvent(InventoryGrid sender, bool drop);
        public delegate void InventoryPickupEvent(object sender, InventoryGrid grid);

        public event InventoryGridEvent InventoryRemoved;
        public event InventoryPickupEvent InventoryPickUpClicked;

        private IMessageHub hub;
        private Guid itemSubscriptionToken;
        private Guid retrieveAllItemsSubscriptionToken;
        private Guid catalogSubscriptionToken;
        private Guid dmTabUpdateSubscriptionToken;

        private GridManager manager;
        private readonly string dmTabId;
        private string currentDmTabId;

        public InventoryGrid(string name, string backgroundPath, Size size, IMessageHub hub, bool canBeEdited = true, bool canBeDeleted = true, string DMTabId = null)
        {
            this.DataContext = this;
            InitializeComponent();

            id = Guid.NewGuid();
            InventoryName = name;
            InventoryBackground = backgroundPath;
            Columns = (int)size.Width;
            Rows = (int)size.Height;
            MessageHub = hub;
            CanBeDeleted = canBeDeleted;
            CanBeEdited = canBeEdited;
            dmTabId = DMTabId;
        }

        void RemoveInventory(bool drop = false)
        {
            var result = (InventoryRemoved?.Invoke(this, drop));
            if (result.HasValue && result.Value)
            {
                hub.Unsubscribe(itemSubscriptionToken);
                hub.Unsubscribe(catalogSubscriptionToken);
                hub.Unsubscribe(retrieveAllItemsSubscriptionToken);
                hub.Unsubscribe(dmTabUpdateSubscriptionToken);
                hub.Publish(new DeleteGrid
                {
                    Id = Id
                });
            }
        }

        public bool IsActive { get { return (dmTabId == currentDmTabId); } }

        DelegateCommand deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new DelegateCommand(ExecuteDelete, CanExecuteDelete);
                }
                return deleteCommand;
            }
        }

        private void ExecuteDelete()
        {
            logger.Info($"> ExecuteDelete()");
            RemoveInventory();
            logger.Info($"< ExecuteDelete()");
        }

        private bool CanExecuteDelete()
        {
            return CanBeDeleted;
        }

        DelegateCommand dropCommand;
        public ICommand DropCommand
        {
            get
            {
                if (dropCommand == null)
                {
                    dropCommand = new DelegateCommand(ExecuteDrop, CanExecuteDrop);
                }
                return dropCommand;
            }
        }

        private void ExecuteDrop()
        {
            logger.Info($"> ExecuteDrop()");
            RemoveInventory(true);
            logger.Info($"< ExecuteDrop()");
        }

        private bool CanExecuteDrop()
        {
            return CanBeDeleted;
        }

        DelegateCommand editCommand;
        public ICommand EditCommand
        {
            get
            {
                if (editCommand == null)
                {
                    editCommand = new DelegateCommand(ExecuteEdit, CanExecuteEdit);
                }
                return editCommand;
            }
        }

        private void ExecuteEdit()
        {
            logger.Info($"> ExecuteEdit()");
            InventoryEditorViewModel viewModel = new InventoryEditorViewModel(InventoryName, InventoryBackground, dmTabId != null) { XValue = Columns, YValue = Rows};
            viewModel.SaveClicked += InventoryEditorViewModel_SaveClicked; ;
            InventoryEditorWindow inventoryEditorWindow = new InventoryEditorWindow(viewModel);
            inventoryEditorWindow.ShowDialog();
            logger.Info($"< ExecuteEdit()");
        }

        private void InventoryEditorViewModel_SaveClicked(InventoryEditorViewModel sender)
        {
            InventoryName = sender.InventoryName;
            InventoryBackground = sender.BackgroundPath;
            UpdateEdit(new Size(Columns, Rows), new Size(sender.XValue, sender.YValue));
        }

        private bool CanExecuteEdit()
        {
            return CanBeEdited;
        }

        private bool canBeDeleted;
        public bool CanBeDeleted
        {
            get
            {
                return canBeDeleted;
            }
            set
            {
                if (canBeDeleted != value)
                {
                    canBeDeleted = value;
                    OnPropertyChange("CanBeDeleted");
                    deleteCommand.RaiseCanExecuteChanged();
                    dropCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool canBeEdited;
        public bool CanBeEdited
        {
            get
            {
                return canBeEdited;
            }
            set
            {
                if (canBeEdited != value)
                {
                    canBeEdited = value;
                    OnPropertyChange("CanBeEdited");
                    editCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string inventoryName;
        public string InventoryName
        {
            get
            {
                return inventoryName;
            }
            set
            {
                if (inventoryName != value)
                {
                    inventoryName = value;
                    OnPropertyChange("InventoryName");
                }
            }
        }

        private Guid id;
        public Guid Id
        { 
            get
            {
                return id;
            } 
        }

        private string inventoryBackground;
        public string InventoryBackground
        {
            get
            {
                return inventoryBackground;
            }
            set
            {
                if (inventoryBackground != value)
                {
                    inventoryBackground = value;
                    OnPropertyChange("InventoryBackground");
                }
            }
        }

        public void UpdateEdit(Size oldSize, Size newSize)
        {
            logger.Info($"({InventoryName})> UpdateEdit()");

            if (Inventory.ColumnDefinitions.Count > 0 && Inventory.RowDefinitions.Count > 0)
            {
                var widthChange = (int)(newSize.Width - oldSize.Width);
                var heightChange = (int)(newSize.Height - oldSize.Height);
                
                if (widthChange > 0)
                {
                    Columns = (int)newSize.Width;
                    for (int x = Inventory.ColumnDefinitions.Count - widthChange; x < Inventory.ColumnDefinitions.Count; x++)
                    {
                        for (int y = 0; y < oldSize.Height; y++)
                        {
                            ExtendedBorder border = new ExtendedBorder
                            {
                                Background = new SolidColorBrush(Color.FromArgb(64, 0, 0, 0)),
                                BorderBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                                BorderThickness = new Thickness(0.75),
                                Width = CellWidth,
                                Height = CellHeight,
                                CellX = x,
                                CellY = y,
                            };
                            Grid.SetColumn(border, x);
                            Grid.SetRow(border, y);
                            Inventory.Children.Add(border);
                        }
                    }
                }
                else if (widthChange < 0)
                {
                    bool canShrink = true;
                    var grid = FullGrid();
                    for (int x = (int)newSize.Width; x < (int)oldSize.Width; x++)
                    {
                        for (int y = 0; y < oldSize.Height; y++)
                        {
                            if(grid[x,y] != null && grid[x, y] is Item)
                            {
                                canShrink = false;
                            }
                        }
                    }

                    if(canShrink)
                    {
                        Columns = (int)newSize.Width;
                        for (int x = (int)newSize.Width; x < (int)oldSize.Width; x++)
                        {
                            for (int y = 0; y < oldSize.Height; y++)
                            {
                                var item = grid[x, y];
                                if(item is ExtendedBorder)
                                {
                                    var itemToRemove = item as ExtendedBorder;
                                    Inventory.Children.Remove(itemToRemove);
                                }
                            }
                        }
                    }
                }

                if (heightChange > 0)
                {
                    Rows = (int)newSize.Height;
                    for (int y = Inventory.RowDefinitions.Count - heightChange; y < Inventory.RowDefinitions.Count; y++)
                    {
                        for (int x = 0; x < Inventory.ColumnDefinitions.Count; x++)
                        {
                            ExtendedBorder border = new ExtendedBorder
                            {
                                Background = new SolidColorBrush(Color.FromArgb(64, 0, 0, 0)),
                                BorderBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                                BorderThickness = new Thickness(0.75),
                                Width = CellWidth,
                                Height = CellHeight,
                                CellX = x,
                                CellY = y,
                            };
                            Grid.SetColumn(border, x);
                            Grid.SetRow(border, y);
                            Inventory.Children.Add(border);
                        }
                    }
                }
                else if (heightChange < 0)
                {
                    bool canShrink = true;
                    var grid = FullGrid();
                    for (int y = (int)newSize.Height; y < (int)oldSize.Height; y++)
                    {
                        for (int x = 0; x < Columns; x++)
                        {
                            if (grid[x, y] != null && grid[x, y] is Item)
                            {
                                canShrink = false;
                            }
                        }
                    }

                    if (canShrink)
                    {
                        Rows = (int)newSize.Height;
                        for (int y = (int)newSize.Height; y < (int)oldSize.Height; y++)
                        {
                            for (int x = 0; x < Columns; x++)
                            {
                                var item = grid[x, y];
                                if (item is ExtendedBorder)
                                {
                                    var itemToRemove = item as ExtendedBorder;
                                    Inventory.Children.Remove(itemToRemove);
                                }
                            }
                        }
                    }
                }

                hub.Publish(new UpdateGrid
                {
                    Id = Id,
                    Name = InventoryName,
                    Inventory = this,
                    Grid = Inventory,
                    CellSize = new Size(CellWidth, CellHeight),
                    Size = new Size(Columns * CellWidth, Rows * CellHeight)
                });
            }

            deleteCommand.RaiseCanExecuteChanged();
            dropCommand.RaiseCanExecuteChanged();
            logger.Info($"({InventoryName})< UpdateEdit()");
        }

        public void Init()
        {
            logger.Info($"({InventoryName})> Init()");
            if (Inventory.ColumnDefinitions.Count > 0 && Inventory.RowDefinitions.Count > 0)
            {
                manager = GridManager.Instance;
                manager.SetHub(MessageHub);

                for (int y = 0; y < Inventory.ColumnDefinitions.Count; y++)
                {
                    for (int x = 0; x < Inventory.RowDefinitions.Count; x++)
                    {
                        ExtendedBorder border = new ExtendedBorder
                        {
                            Background = new SolidColorBrush(Color.FromArgb(64,0,0,0)),
                            BorderBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                            BorderThickness = new Thickness(0.75),
                            Width = CellWidth,
                            Height = CellHeight,
                            CellX = x,
                            CellY = y,
                        };
                        Grid.SetColumn(border, y);
                        Grid.SetRow(border, x);
                        Inventory.Children.Add(border);
                    }
                }

                hub = MessageHub;
                itemSubscriptionToken = hub.Subscribe<ItemPositionUpdate>(ItemPositionUpdate);
                retrieveAllItemsSubscriptionToken = hub.Subscribe<MoveAllItemsTo>(RetrieveAllItems);
                catalogSubscriptionToken = hub.Subscribe<CatalogItemPositionUpdate>(CatalogItemPositionUpdate);
                dmTabUpdateSubscriptionToken = hub.Subscribe<DmTabUpdate>(DmTabUpdate);

                hub.Publish(new UpdateGrid
                {
                    Id = Id,
                    Name = InventoryName,
                    Inventory = this,
                    Grid = Inventory,
                    CellSize = new Size(CellWidth, CellHeight),
                    Size = new Size(Columns * CellWidth, Rows * CellHeight)
                });
            }
            deleteCommand.RaiseCanExecuteChanged();
            dropCommand.RaiseCanExecuteChanged();
            logger.Info($"({InventoryName})< Init()");
        }

        void DmTabUpdate(DmTabUpdate dmTabUpdate)
        {
            currentDmTabId = dmTabUpdate.Header;
        }

        void RetrieveAllItems(MoveAllItemsTo move)
        {
            if(move.MoveToId == Id)
            {
                foreach (var item in move.Items)
                {
                    var cell = NextAvailableCell(item.Model.CellSpanX, item.Model.CellSpanY);
                    if (cell.HasValue)
                    {
                        AddItem(item.Model, (int)cell.Value.X, (int)cell.Value.Y, item.Model.Quantity);
                    }
                    else 
                    {
                        if (move.FallBackIds.Count > 0)
                        {
                            hub.Publish(new MoveAllItemsTo
                            {
                                MoveToId = move.FallBackIds.First(),
                                Items = new List<Item>() { item },
                                FallBackIds = move.FallBackIds.GetRange(1, move.FallBackIds.Count() - 1)
                            });
                        }
                    }
                }
            }
        }

        public void Save(string path)
        {
            logger.Info($"({InventoryName})> Save(path: {path})");
            var save = new InventorySaveModel();
            save.InventoryName = InventoryName;
            save.InventoryBackground = InventoryBackground;
            save.CanBeDeleted = CanBeDeleted;
            save.CanBeEdited = CanBeEdited;
            foreach (var item in Inventory.Children)
            {
                if (item is Item)
                {
                    var itemModel = (item as Item).Model;
                    if (itemModel is InventoryItemModel)
                    {
                        save.InventoryItems.Add(itemModel as InventoryItemModel);
                    }
                    else if (itemModel is UiItemModel)
                    {
                        save.UiItems.Add(itemModel);
                    }
                }
            }

            XmlHelper<InventorySaveModel>.WriteToXml(path, save);
            logger.Info($"({InventoryName})< Save(path: {path})");
        }

        public void Load(string path)
        {
            logger.Info($"({InventoryName})> Load(path: {path})");
            var save = XmlHelper<InventorySaveModel>.ReadFromXml(path);
            if (save != null)
            {
                InventoryName = save.InventoryName;
                InventoryBackground = save.InventoryBackground;
                CanBeDeleted = save.CanBeDeleted;
                CanBeEdited = save.CanBeEdited;
                foreach (UiItemModel item in save.UiItems)
                {
                    AddItem(item, item.CellX, item.CellY, item.Quantity);
                }
                foreach (InventoryItemModel item in save.InventoryItems)
                {
                    AddItem(item, item.CellX, item.CellY, item.Quantity);
                }
            }
            logger.Info($"({InventoryName})< Load(path: {path})");
        }

        public void AddItem(string id, string name, IList<ItemType> types, string cost, string weight, string rarity, string attunement, string properties, string description, string source, int x, int y, string imagePath, int spanX = 1, int spanY = 1, int quantity = 1, bool isStackable = false, ItemModel itemModel = null)
        {
            logger.Info($"({InventoryName})> AddItem(id: {id}, x: {x}, y: {y}, imagePath: {imagePath}, spanX: {spanX}, spanY: {spanY}, quantity: {quantity}, isStackable: {isStackable})");
            Item item;
            if(itemModel != null && itemModel is InventoryItemModel)
            {
                //item = new Item(hub, Inventory, itemModel as UiItemModel);
                item = new Item(hub, Inventory, new InventoryItemModel(itemModel as InventoryItemModel));
            }
            else
            {
                item = new Item(hub, Inventory, new UiItemModel(id, name, types, cost, weight, rarity, attunement, properties, description, source, CellWidth, CellHeight, x, y, spanX, spanY, quantity, isStackable, imagePath));
            }
            Grid.SetColumnSpan(item, spanX);
            Grid.SetRowSpan(item, spanY);
            Grid.SetColumn(item, x);
            Grid.SetRow(item, y);
            item.MouseReleased += Item_MouseReleased;
            item.MousePressed += Item_MousePressed;
            item.ItemSplitClicked += Item_SplitClicked;
            item.ItemStackClicked += Item_StackClicked;
            item.ItemDeleteClicked += Item_ItemDeleteClicked;
            item.InventoryPickUpClicked += Item_InventoryPickUpClicked;
            Inventory.Children.Add(item);
            deleteCommand.RaiseCanExecuteChanged();
            dropCommand.RaiseCanExecuteChanged();
            logger.Info($"({InventoryName})< AddItem(id: {id}, x: {x}, y: {y}, imagePath: {imagePath}, spanX: {spanX}, spanY: {spanY}, quantity: {quantity}, isStackable: {isStackable})");
        }

        public void AddItem(ItemModel item, int x, int y, int quantity)
        {
            AddItem(item.ID, item.Name, item.Type, item.Cost, item.Weight, item.Rarity, item.Attunement, item.Properties, item.Description, item.Source, x, y, item.ImageUri, item.CellSpanX, item.CellSpanY, quantity, item.IsStackable, item);
        }

        private void Item_InventoryPickUpClicked(object sender)
        {
            InventoryPickUpClicked?.Invoke(sender, this);
        }

        private void Item_ItemDeleteClicked(Item sender)
        {
            DeleteItem(sender);
        }

        public void DeleteItem(Item item)
        {
            if (item.GridParent.Children.Contains(item))
            {
                item.GridParent.Children.Remove(item);
                item.RemoveEvents();
                dropCommand.RaiseCanExecuteChanged();
                deleteCommand.RaiseCanExecuteChanged();
            }
        }

        private object[,] FullGrid()
        {
            object[,] grid = ArrayHelper.GetNew2DArray<object>(Columns, Rows, null);
            foreach (var invItem in Inventory.Children)
            {
                if (invItem is ExtendedBorder)
                {
                    var curItem = invItem as ExtendedBorder;
                    grid[curItem.CellX, curItem.CellY] = curItem;
                }
                if (invItem is Item)
                {
                    var curItem = invItem as Item;
                    for (int y = curItem.Model.CellY; y < curItem.Model.CellY + curItem.Model.CellSpanY; y++)
                    {
                        for (int x = curItem.Model.CellX; x < curItem.Model.CellX + curItem.Model.CellSpanX; x++)
                        {
                            grid[x, y] = curItem;
                        }
                    }
                }
            }
            return grid;
        }
        private Item[,] OccupiedGrid(Item currentItem = null)
        {
            Item[,] grid = ArrayHelper.GetNew2DArray<Item>(Rows, Columns, null);
            foreach (var invItem in Inventory.Children)
            {
                if (invItem is Item)
                {
                    var curItem = invItem as Item;
                    if (currentItem != curItem)
                    {
                        for (int y = curItem.Model.CellY; y < curItem.Model.CellY + curItem.Model.CellSpanY; y++)
                        {
                            for (int x = curItem.Model.CellX; x < curItem.Model.CellX + curItem.Model.CellSpanX; x++)
                            {
                                grid[y, x] = curItem;
                            }
                        }
                    }
                }
            }
            return grid;
        }

        public Point? NextAvailableCell(int spanX, int spanY)
        {
            logger.Info($"({InventoryName})> NextAvailableCell(spanX: {spanX}, spanY: {spanY})");

            // Check if item even fits in grid
            if (spanX <= Columns && spanY <= Rows)
            {
                Point? point = null;
                var occupyGrid = OccupiedGrid();
                for (int row = 0; row < Rows; row++)
                {
                    for (int column = 0; column < Columns; column++)
                    {
                        bool cellAvailable = (occupyGrid[row, column]==null);
                        if ((row + spanY - 1) >= Rows || (column + spanX - 1) >= Columns)
                        {
                            cellAvailable = false;
                        }
                        for (int y = row; y < row + spanY && cellAvailable; y++)
                        {
                            for (int x = column; x < column + spanX && cellAvailable; x++)
                            {
                                if(occupyGrid[y,x]!=null)
                                { 
                                    cellAvailable = false;
                                }
                            }
                        }
                        if(cellAvailable)
                        {
                            point = new Point(column, row);
                            logger.Info($"< NextAvailableCell(spanX: {spanX}, spanY: {spanY}).return({point.Value})");
                            return point.Value;
                        }
                    }
                }
            }

            logger.Info($"({InventoryName})< NextAvailableCell(spanX: {spanX}, spanY: {spanY}).return(null)");
            return null;
        }

        private void Item_MousePressed(object sender, Point mousePosition)
        {
            logger.Trace($"({InventoryName})>< Item_MousePressed(sender: {sender}, mousePosition: {mousePosition})");
            if (sender is Item)
            {
                var item = sender as Item;
                Panel.SetZIndex(this, 998);
                Panel.SetZIndex(item, 999);
            }
        }

        private void Item_SplitClicked(object sender)
        {
            logger.Debug($"({InventoryName})> Item_SplitClicked(sender: {sender})");
            if (sender is Item)
            {
                var item = sender as Item;
                ItemSplitViewModel viewModel = new ItemSplitViewModel(item.Model);
                viewModel.SplitClicked += ItemDetailViewModel_SplitClicked;
                ItemSplitWindow detailWindow = new ItemSplitWindow(viewModel);
                detailWindow.ShowDialog();
            }
            logger.Debug($"({InventoryName})< Item_SplitClicked(sender: {sender})");
        }

        private void Item_StackClicked(object sender)
        {
            logger.Debug($"({InventoryName})> Item_StackClicked(sender: {sender})");
            if (sender is Item)
            {
                List<Item> removedItems = new List<Item>();
                var item = sender as Item;
                foreach (var invItem in Inventory.Children)
                {
                    if (invItem is Item)
                    {
                        var curItem = invItem as Item;
                        if (curItem == item)
                        {
                            continue;
                        }
                        else if(curItem.Model.ID == item.Model.ID)
                        {
                            item.Model.Quantity += curItem.Model.Quantity;
                            removedItems.Add(curItem);
                        }
                    }
                }
                foreach (var curItem in removedItems)
                {
                    curItem.GridParent.Children.Remove(curItem);
                    curItem.RemoveEvents();
                }
            }
            logger.Debug($"({InventoryName})< Item_StackClicked(sender: {sender})");
        }

        private bool ItemDetailViewModel_SplitClicked(UiItemModel sender, int value)
        {
            logger.Debug($"({InventoryName})> ItemDetailViewModel_SplitClicked(sender: {sender}, value: {value})");
            var cell = NextAvailableCell(sender.CellSpanX, sender.CellSpanY);
            if (cell.HasValue)
            {
                AddItem(sender, (int)cell.Value.X, (int)cell.Value.Y, value);
                sender.Quantity -= value;
                logger.Debug($"< ItemDetailViewModel_SplitClicked(sender: {sender}, value: {value}).return(True)");
                return true;
            }
            logger.Debug($"({InventoryName})< ItemDetailViewModel_SplitClicked(sender: {sender}, value: {value}).return(False)");
            return false;
        }

        private bool gridCellsOccupied(int newX, int newY, int spanX, int spanY, Item i2)
        {
            logger.Debug($"({InventoryName})> gridCellsOccupied(newX: {newX}, newY: {newY}, spanX: {spanX}, spanY: {spanY}, item: [{i2}])");
            var i1Start = new Point(newX, newY);
            var i1End = new Point(newX + spanX - 1, newY + spanY - 1);
            var i2Start = new Point(i2.Model.CellX, i2.Model.CellY);
            var i2End = new Point(i2.Model.CellX + i2.Model.CellSpanX - 1, i2.Model.CellY + i2.Model.CellSpanY - 1);

            var result = (i2End.X >= i1Start.X && i2Start.X <= i1End.X) && (i2End.Y >= i1Start.Y && i2Start.Y <= i1End.Y);
            logger.Debug($"({InventoryName})< gridCellsOccupied(newX: {newX}, newY: {newY}, spanX: {spanX}, spanY: {spanY}, item: [{i2}]).return({result})");
            return result;
        }

        private bool IsWithinGridBoundary(int newX, int newY, int spanX, int spanY)
        {
            logger.Debug($"({InventoryName})> IsWithinGridBoundary(newX: {newX}, newY: {newY}, spanX: {spanX}, spanY: {spanY})");
            var i1Start = new Point(newX, newY);
            var i1End = new Point(newX + spanX - 1, newY + spanY - 1);
            var i2Start = new Point(0, 0);
            var i2End = new Point(Inventory.ColumnDefinitions.Count - 1, Inventory.RowDefinitions.Count - 1);

            var result = (i1Start.X >= i2Start.X && i1End.X <= i2End.X) && (i1Start.Y >= i2Start.Y && i1End.Y <= i2End.Y);
            logger.Debug($"({InventoryName})< IsWithinGridBoundary(newX: {newX}, newY: {newY}, spanX: {spanX}, spanY: {spanY}).return({result})");
            return result;
        }

        private void ItemMovedEvent(string itemId, ItemMovedMessage message)
        {

        }

        private void ItemPositionUpdate(ItemPositionUpdate positionUpdate)
        {
            logger.Debug($"({InventoryName})> ItemPositionUpdate(positionUpdate: {positionUpdate})");
            var item = positionUpdate.Item;
            var releasePoint = positionUpdate.Position;

            var width = Columns * CellWidth;
            var height = Rows * CellHeight;
            var screenPoint = Inventory.TranslatePoint(new Point(0, 0), Application.Current.MainWindow);

            if (IsActive && (releasePoint.X < screenPoint.X + width && releasePoint.Y < screenPoint.Y + height
                && releasePoint.X > screenPoint.X && releasePoint.Y > screenPoint.Y))
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
                if (!IsWithinGridBoundary(icellX, icellY, item.Model.CellSpanX, item.Model.CellSpanY))
                {
                    cancelMove = true;
                }

                var occupyGrid = OccupiedGrid(item);
                if (!cancelMove)
                {
                    if (occupyGrid[icellY, icellX] != null)
                    {
                        if (item.Model.ID == occupyGrid[icellY, icellX].Model.ID && item.Model.IsStackable)
                        {
                            occupyGrid[icellY, icellX].Model.Quantity += item.Model.Quantity;
                            stacked = true;
                        }
                        else
                        {
                            cancelMove = true;
                        }
                    }
                }

                if (!cancelMove && !stacked)
                {
                    for (int itemY = icellY; itemY < icellY + item.Model.CellSpanY; itemY++)
                    {
                        for (int itemX = icellX; itemX < icellX + item.Model.CellSpanX; itemX++)
                        {
                            if (occupyGrid[itemY, itemX] != null)
                            {
                                cancelMove = true;
                            }
                        }
                    }
                }
                

                if (cancelMove)
                {
                    var parentPoint = item.GridParent.TranslatePoint(new Point(0, 0), Application.Current.MainWindow);
                    var startingX = item.Model.CellX * CellWidth + parentPoint.X;
                    var startingY = item.Model.CellY * CellHeight + parentPoint.Y;
                    item.Transform(new Point(startingX, startingY));
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
                        item.ItemStackClicked += Item_StackClicked;
                        item.ItemDeleteClicked += Item_ItemDeleteClicked;
                        Inventory.Children.Add(item);
                        item.GridParent = Inventory;
                        item.Model.CellX = icellX;
                        item.Model.CellY = icellY;
                        item.Transform(new Point(x, y));
                    }
                }
            }
            else
            {
                logger.Debug($"Ignored");
            }
            deleteCommand.RaiseCanExecuteChanged();
            dropCommand.RaiseCanExecuteChanged();

            logger.Debug($"({InventoryName})< ItemPositionUpdate(positionUpdate: {positionUpdate})");
        }

        private void CatalogItemPositionUpdate(CatalogItemPositionUpdate positionUpdate)
        {
            logger.Debug($"({InventoryName})> CatalogItemPositionUpdate(positionUpdate: {positionUpdate})");
            var item = positionUpdate.Item;
            var releasePoint = positionUpdate.Position;

            var width = Columns * CellWidth;
            var height = Rows * CellHeight;
            var screenPoint = Inventory.TranslatePoint(new Point(0, 0), Application.Current.MainWindow);

            if (IsActive && (releasePoint.X < Application.Current.MainWindow.Width - 394 &&
                releasePoint.X < screenPoint.X + width && releasePoint.Y < screenPoint.Y + height
                && releasePoint.X > screenPoint.X && releasePoint.Y > screenPoint.Y))
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

            logger.Debug($"({InventoryName})< ItemPositionUpdate(positionUpdate: {positionUpdate})");
        }

        private void Item_MouseReleased(object sender, Point mousePosition)
        {
            logger.Trace($"({InventoryName})>< Item_MouseReleased(sender: {sender}, mousePosition: {mousePosition})");
            if (sender is Item)
            {
                var item = sender as Item;
                Panel.SetZIndex(this, 1);
                Panel.SetZIndex(item, 2);
            }
        }

        public List<Item> GetAllItems()
        {
            return Inventory.Children.OfType<Item>().ToList();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
