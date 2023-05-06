using Easy.MessageHub;
using InventoryControlLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using InventoryControlLib.View;
using InventoryControlLib.Model;
using DNDinventory.ViewModel;
using DNDinventory.View;
using WinForms = System.Windows.Forms;
using Utilities.Sockets.EventSocket;
using Utilities.Sockets.EventSocket.Messages;

namespace DNDinventory.Model
{
    public class DmTabItem : INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private string NO_IMAGE;

        private EventSocketServer eventServer;
        private EventSocketClient eventClient;

        public string Header { get; set; }

        private StackPanel content;
        public StackPanel Content
        {
            get
            {
                if (content == null)
                {
                    content = new StackPanel();
                    content.HorizontalAlignment = HorizontalAlignment.Left;
                    content.VerticalAlignment = VerticalAlignment.Top;
                    content.Orientation = Orientation.Horizontal;
                }
                return content;
            }
        }

        private readonly IMessageHub hub;
        public IMessageHub Hub
        {
            get
            {
                return hub;
            }
        }

        public DmTabItem(string header, IMessageHub hub, string clientIP, int port = 30503)
        {
            Header = header;
            var tmp_NO_IMAGE = new Uri(@"Images\No_image_available.png", UriKind.Relative);
            if (!tmp_NO_IMAGE.IsAbsoluteUri)
            {
                tmp_NO_IMAGE = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tmp_NO_IMAGE.ToString()), UriKind.Absolute);
            }
            NO_IMAGE = tmp_NO_IMAGE.AbsoluteUri;
            this.hub = hub;

            eventServer = new EventSocketServer(port);
            eventClient = new EventSocketClient(clientIP, port);

            eventServer.HandleMessage += EventServer_HandleMessage;
            eventServer.Start();
        }

        private void EventServer_HandleMessage(Message message)
        {
            throw new NotImplementedException();
        }

        ~DmTabItem()
        {
            eventServer.Stop();
        }

        public bool SetupDefaultInvClient()
        {
            logger.Debug($"> setupInv()");

            Dictionary<string, DefaultInventorySettings> defaultInventories = new Dictionary<string, DefaultInventorySettings>
                {
                    { "Ground", new DefaultInventorySettings(new Size(5, 10), true, false )},
                    { "Backpack", new DefaultInventorySettings(new Size(7, 7), true, true)},
                };
            foreach (var inventory in defaultInventories)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var id = AddInventory(Content, inventory.Key, NO_IMAGE, inventory.Value.Size, inventory.Value.EditRights, inventory.Value.DeleteRights);

                    if (inventory.Key == "Ground")
                    {
                        GridManager.Instance.GroundId = id;
                    }
                    if (inventory.Key == "Backpack")
                    {
                        GridManager.Instance.BackPackId = id;
                    }
                });
            }

            logger.Debug($"< setupInv()");
            return true;
        }

        private Guid AddInventory(StackPanel panel, string name, string backgroundPath, Size size, bool canBeEdited = true, bool canBeDeleted = true)
        {
            logger.Debug($"> AddInventory(name:{name}, size:[{size}])");
            var inv = new InventoryGrid(name, backgroundPath, size, Hub, canBeEdited, canBeDeleted, Header);
            inv.InventoryRemoved += Inv_InventoryRemoved;
            inv.InventoryPickUpClicked += Inv_InventoryPickUpClicked;
            inv.Init();

            panel.Children.Add(inv);
            OnPropertyChange("Content");
            logger.Debug($"< AddInventory({name}, {size})");
            return inv.Id;
        }

        private void Inv_InventoryPickUpClicked(object sender, InventoryGrid grid)
        {
            if (sender is Item)
            {
                var item = sender as Item;
                if (item.Model is InventoryItemModel)
                {
                    var inventoryItemModel = item.Model as InventoryItemModel;
                    var invGuid = AddInventory(Content, inventoryItemModel.Name, inventoryItemModel.ImageUri, inventoryItemModel.Size);
                    var inventory = GridManager.Instance.Grids.Where(e => e.Id == invGuid).First().Inventory;
                    foreach (var invItem in inventoryItemModel.Items)
                    {
                        inventory.AddItem(invItem, invItem.CellX, invItem.CellY, invItem.Quantity);
                    }
                    grid.DeleteItem(item);
                }
            }
        }

        private bool Inv_InventoryRemoved(InventoryControlLib.InventoryGrid sender, bool drop)
        {
            logger.Debug($"> Inv_InventoryRemoved({sender.InventoryName}, {drop})");

            if (drop)
            {
                if (MessageBox.Show($"Do you want to drop {sender.InventoryName}?", $"Drop {sender.InventoryName}?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var ground = GridManager.Instance.GroundGrid;
                    var nextCell = ground.Inventory.NextAvailableCell(1, 1);
                    if (nextCell.HasValue)
                    {
                        var items = sender.GetAllItems().Select(x => x.Model).ToList();
                        var item = new InventoryItemModel($"{sender.InventoryName}_" + Guid.NewGuid(), $"{sender.InventoryName}", new Size(sender.Columns, sender.Rows), (int)nextCell.Value.X, (int)nextCell.Value.Y, items, sender.InventoryBackground);
                        ground.Inventory.AddItem(item, item.CellX, item.CellY, 1);
                    }
                    else
                    {
                        logger.Debug($"< Inv_InventoryRemoved({sender.InventoryName}, {drop}).Return(false)");
                        return false;
                    }
                }
                else
                {
                    logger.Debug($"< Inv_InventoryRemoved({sender.InventoryName}, {drop}).Return(false)");
                    return false;
                }
                Content.Children.Remove(sender);
                OnPropertyChange("InventoryContent");
                logger.Debug($"< Inv_InventoryRemoved({sender.InventoryName}, {drop}).Return(true)");
                return true;
            }

            // Select where to move the items to
            var invSelectViewModel = new InventorySelectViewModel(new List<Guid>() { sender.Id });
            var invSelectWindow = new InventorySelectWindow(invSelectViewModel);
            invSelectWindow.Owner = Application.Current.MainWindow;
            invSelectWindow.ShowDialog();
            if (invSelectViewModel.DialogResult == WinForms.DialogResult.Cancel)
            {
                logger.Debug($"< Inv_InventoryRemoved({sender.InventoryName}, {drop}).Return(false)");
                return false;
            }

            var moveToId = invSelectViewModel.SelectedGridId;
            hub.Publish(new MoveAllItemsTo
            {
                MoveToId = moveToId,
                Items = sender.GetAllItems(),
                FallBackIds = invSelectViewModel.SelectedFallbackGridId.ToList()
            });
            Content.Children.Remove(sender);
            OnPropertyChange("InventoryContent");
            logger.Debug($"< Inv_InventoryRemoved({sender.InventoryName}, {drop}).Return(true)");
            return true;
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
