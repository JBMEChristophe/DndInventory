using DNDinventory.Model;
using DNDinventory.SocketFileTransfer;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using WinForms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using Easy.MessageHub;
using System.Windows.Controls;
using System.Windows;
using InventoryControlLib.View;
using InventoryControlLib.Model;

namespace DNDinventory.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        private const string version = "0.1.0";
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private List<CatalogItemModel> catalogItems;
        private readonly IMessageHub hub;
        private Listener listener;
        private List<TransferClient> transferClients;
        private SettingsFileHandler settingsFileHandler;
        private string outputFolder;
        private string settingsFileLocation;
        private Timer timerOverallProgress;

        private bool serverRunning;

        private void AddInventory(string name, Size size)
        {
            logger.Debug($"> AddInventory(name:{name}, size:[{size}])");
            var inv = new InventoryControlLib.InventoryGrid()
            {
                Columns = (int)size.Width,
                Rows = (int)size.Height,
                MessageHub = Hub,
                Name = name
            };
            inv.Init();

            InventoryContent.Children.Add(inv);
            OnPropertyChange("InventoryContent");
            logger.Debug($"< AddInventory({name}, {size})");
        }

        private void setupInv()
        {
            logger.Debug($"> setupInv()");
            string[] test = { "Inv1", "Inv", "Inv3", "MegaInv" };
            int[,] test2 = { { 3, 7 }, { 5, 5 }, { 4, 5 }, { 15, 25 } };
            for (int i = 0; i < test.Length; i++)
            {
                AddInventory(test[i], new Size(test2[i, 0], test2[i, 1]));
            }

            List<CatalogItemModel> catalogItems;
            if (File.Exists("Catalogs/Items.xml"))
            {
                catalogItems = XmlHelper<List<CatalogItemModel>>.ReadFromXml("Catalogs/Items.xml");
            }
            else
            {
                catalogItems = XmlHelper<List<CatalogItemModel>>.ReadFromXml("DefaultItems.xml");
            }

            foreach (var item in catalogItems)
            {
                item.Width = 50 * item.CellSpanX;
                item.Height = 50 * item.CellSpanY;
                if(string.IsNullOrEmpty(item.ImageUri))
                { 
                    item.ImageUri = "Images/No_image_available.png";
                }
                Catalog._viewModel.Items.Add(new CatalogItem(hub, item));
            }

            //XmlHelper<List<CatalogItemModel>>.WriteToXml("Items.xml", catalogItems);
            logger.Debug($"< setupInv()");
        }

        private StackPanel inventoryContent;
        public StackPanel InventoryContent
        {
            get
            {
                if(inventoryContent == null)
                {
                    inventoryContent = new StackPanel();
                    inventoryContent.HorizontalAlignment = HorizontalAlignment.Left;
                    inventoryContent.VerticalAlignment = VerticalAlignment.Top;
                    inventoryContent.Orientation = Orientation.Horizontal;
                }
                return inventoryContent;
            }
        }

        private ItemCatalog catalog;
        public ItemCatalog Catalog
        {
            get
            {
                if(catalog == null)
                {
                    catalog = new ItemCatalog();
                }
                return catalog;
            }
        }

        public IMessageHub Hub
        {
            get
            {
                return hub;
            }
        }

        public string Version
        {
            get
            {
                return version;
            }
        }

        private string host;
        public string Host
        {
            get
            {
                return host;
            }
            set
            {
                if(host!=value)
                {
                    host = value;
                    OnPropertyChange("Host");
                }
            }
        }

        private string port;
        public string Port
        {
            get
            {
                return port;
            }
            set
            {
                if (port != value)
                {
                    port = value;
                    OnPropertyChange("Port");
                }
            }
        }

        private string connectionStatus;
        public string ConnectionStatus
        {
            get
            {
                return connectionStatus;
            }
            set
            {
                if (connectionStatus != value)
                {
                    connectionStatus = value;
                    OnPropertyChange("ConnectionStatus");
                }
            }
        }

        private string connectText;
        public string ConnectText
        {
            get
            {
                return connectText;
            }
            set
            {
                if (connectText != value)
                {
                    connectText = value;
                    OnPropertyChange("ConnectText");
                }
            }
        }

        private string setFolderOutputTxt;
        public string SetFolderOutputTxt
        {
            get
            {
                return setFolderOutputTxt;
            }
            set
            {
                if (setFolderOutputTxt != value)
                {
                    setFolderOutputTxt = $"Set file folder ({value})";
                    OnPropertyChange("SetFolderOutputTxt");
                }
            }
        }

        private string settingsFileLocationTxt;
        public string SettingsFileLocationTxt {
            get {
                return settingsFileLocationTxt;
            }
            set {
                if (settingsFileLocationTxt != value) {
                    settingsFileLocationTxt = $"Current settings file ({value})";
                    OnPropertyChange("SettingsFileLocationTxt");
                    Properties.Settings.Default.SettingsFileLocation = value;
                }
            }
        }

        private int progressOverall;
        public int ProgressOverall
        {
            get
            {
                return progressOverall;
            }
            set
            {
                if (progressOverall != value)
                {
                    progressOverall = value;
                    OnPropertyChange("ProgressOverall");
                }
            }
        }

        private ObservableCollection<KeyValuePair<string, Transfer>> transfers;
        public ObservableCollection<KeyValuePair<string, Transfer>> Transfers
        {
            get
            {
                return transfers;
            }
            set
            {
                if (transfers != value)
                {
                    transfers = value;
                    OnPropertyChange("Transfers");
                }
            }
        }

        private void AddTransfer(Transfer t)
        {
            logger.Info($"> AddTransfer(transfer: [{t}])");
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                Transfers.Add(new KeyValuePair<string, Transfer>(t.Id, t));
                OnPropertyChange("Transfers");
            });
            logger.Info($"< AddTransfer(transfer: [{t}])");
        }

        DelegateCommand connectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                if (connectCommand == null)
                {
                    connectCommand = new DelegateCommand(ExecuteConnect, CanExecuteConnect);
                }
                return connectCommand;
            }
        }

        private void ExecuteConnect()
        {
            logger.Info($"> ExecuteConnect()");
            if (transferClients.Count <= 0)
            {
                logger.Info($">< ExecuteConnect().Connect");
                var transferClient = new TransferClient();
                transferClients.Add(transferClient);
                transferClient.Connect(Host.Trim(), int.Parse(Port.Trim()), connectCallback);
            }
            else
            {
                logger.Info($">< ExecuteConnect().Disconnect");
                foreach (var client in transferClients)
                {
                    client.Close();
                }
                transferClients.Clear();
            }

            sendFileCommand.RaiseCanExecuteChanged();
            logger.Info($"< ExecuteConnect()");
        }

        private bool CanExecuteConnect()
        {
            return !serverRunning;
        }

        DelegateCommand startServerCommand;
        public ICommand StartServerCommand
        {
            get
            {
                if (startServerCommand == null)
                {
                    startServerCommand = new DelegateCommand(ExecuteStartServer, CanExecuteStartServer);
                }
                return startServerCommand;
            }
        }

        private void ExecuteStartServer()
        {
            logger.Info($"> ExecuteStartServer()");
            if (serverRunning)
            {
                return;
            }
            serverRunning = true;

            try
            {
                listener.Start(int.Parse(Port.Trim()));
                ConnectionStatus = "Waiting...";
                startServerCommand.RaiseCanExecuteChanged();
                stopServerCommand.RaiseCanExecuteChanged();
                connectCommand?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Unable to listing on port {Port.Trim()}";
                logger.Warn(ex, ConnectionStatus);
            }
            logger.Info($"< ExecuteStartServer()");
        }

        private bool CanExecuteStartServer()
        {
            return !serverRunning;
        }

        DelegateCommand stopServerCommand;
        public ICommand StopServerCommand
        {
            get
            {
                if (stopServerCommand == null)
                {
                    stopServerCommand = new DelegateCommand(ExecuteStopServer, CanExecuteStopServer);
                }
                return stopServerCommand;
            }
        }

        private void ExecuteStopServer()
        {
            logger.Info($"> ExecuteStopServer()");
            if (!serverRunning)
            {
                return;
            }

            if (transferClients.Count > 0)
            {
                foreach (var client in transferClients)
                {
                    client.Close();
                }
            }

            listener.Stop();
            timerOverallProgress.Stop();
            ConnectionStatus = "No connection";
            serverRunning = false;
            startServerCommand.RaiseCanExecuteChanged();
            stopServerCommand.RaiseCanExecuteChanged();
            connectCommand?.RaiseCanExecuteChanged();
            logger.Info($"< ExecuteStopServer()");
        }

        private bool CanExecuteStopServer()
        {
            return serverRunning;
        }

        DelegateCommand sendFileCommand;
        public ICommand SendFileCommand
        {
            get
            {
                if (sendFileCommand == null)
                {
                    sendFileCommand = new DelegateCommand(ExecuteSendFile, CanExecuteSendFile);
                }
                return sendFileCommand;
            }
        }

        private void ExecuteSendFile()
        {
            logger.Info($"> ExecuteSendFile()");
            if (transferClients.Count <= 0)
            {
                return;
            }

            try
            {
                using (WinForms.OpenFileDialog ofd = new WinForms.OpenFileDialog())
                {
                    ofd.Filter = "All Files (*.*)|*.*";
                    ofd.Multiselect = true;

                    if (ofd.ShowDialog() == WinForms.DialogResult.OK)
                    {
                        foreach (var file in ofd.FileNames)
                        {
                            foreach (var client in transferClients)
                            {
                                client.QueueTransfer(file);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Something bad happened");
            }
            logger.Info($"< ExecuteSendFile()");
        }

        private bool CanExecuteSendFile()
        {
            return (transferClients.Count > 0);
        }

        DelegateCommand<Transfer> pauseTransferCommand;
        public ICommand PauseTransferCommand
        {
            get
            {
                if (pauseTransferCommand == null)
                {
                    pauseTransferCommand = new DelegateCommand<Transfer>(ExecutePauseTransfer, CanExecutePauseTransfer);
                }
                return pauseTransferCommand;
            }
        }

        private void ExecutePauseTransfer(Transfer transfer)
        {
            logger.Info($"> ExecutePauseTransfer(transfer: [{transfer}])");
            var queue = transfer.Queue;
            queue.Client.PauseTransfer(queue);
            logger.Info($"< ExecutePauseTransfer(transfer: [{transfer}])");
        }

        private bool CanExecutePauseTransfer(Transfer transfer)
        {
            return true;
        }

        DelegateCommand<Transfer> stopTransferCommand;
        public ICommand StopTransferCommand
        {
            get
            {
                if (stopTransferCommand == null)
                {
                    stopTransferCommand = new DelegateCommand<Transfer>(ExecuteStopTransfer, CanExecuteStopTransfer);
                }
                return stopTransferCommand;
            }
        }

        private void ExecuteStopTransfer(Transfer transfer)
        {
            logger.Info($"> ExecuteStopTransfer(transfer: [{transfer}])");
            var queue = transfer.Queue;
            queue.Client.StopTransfer(queue);

            progressOverall = 0;
            logger.Info($"< ExecuteStopTransfer(transfer: [{transfer}])");
        }

        private bool CanExecuteStopTransfer(Transfer transfer)
        {
            return true;
        }

        DelegateCommand setOutputFolderCommand;
        public ICommand SetOutputFolderCommand
        {
            get
            {
                if (setOutputFolderCommand == null)
                {
                    setOutputFolderCommand = new DelegateCommand(ExecuteSetOutputFolder);
                }
                return setOutputFolderCommand;
            }
        }

        private void ExecuteSetOutputFolder()
        {
            logger.Info($">< ExecuteSetOutputFolder()");
            using (WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog())
            {
                fbd.SelectedPath = outputFolder;
                if (fbd.ShowDialog() == WinForms.DialogResult.OK)
                {
                    outputFolder = fbd.SelectedPath;

                    if (transferClients.Count > 0)
                    {
                        foreach (var client in transferClients)
                        {
                            client.OutputFolder = outputFolder;
                        }
                    }

                    SetFolderOutputTxt = outputFolder;
                }
            }
        }

        DelegateCommand saveSettingsCommand;
        public ICommand SaveSettingsCommand {
            get {
                if (saveSettingsCommand == null) {
                    saveSettingsCommand = new DelegateCommand(ExecuteSaveSettings);
                }
                return saveSettingsCommand;
            }
        }

        private void ExecuteSaveSettings () 
        {
            logger.Info($"> ExecuteSaveSettings()");
            using (WinForms.SaveFileDialog sfd = new WinForms.SaveFileDialog()) {
                sfd.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                sfd.FilterIndex = 1;
                sfd.InitialDirectory = settingsFileLocation;

                if (sfd.ShowDialog() == WinForms.DialogResult.OK) {
                    settingsFileLocation = sfd.FileName;
                    SettingsFileLocationTxt = settingsFileLocation;

                    // save the settings file using quicksave
                    ExecuteQuickSaveSettings();
                }
            }
            logger.Info($"< ExecuteSaveSettings()");
        }

        DelegateCommand quickSaveSettingsCommand;
        public ICommand QuickSaveSettingsCommand {
            get {
                if (quickSaveSettingsCommand == null) {
                    quickSaveSettingsCommand = new DelegateCommand(ExecuteQuickSaveSettings);
                }
                return quickSaveSettingsCommand;
            }
        }

        private void ExecuteQuickSaveSettings ()
        {
            logger.Info($"> ExecuteQuickSaveSettings()");
            PrepareSavedSettings();
            settingsFileHandler.WriteToXml(settingsFileLocation);
            logger.Info($"< ExecuteQuickSaveSettings()");
        }

        DelegateCommand loadSettingsCommand;
        public ICommand LoadSettingsCommand {
            get {
                if (loadSettingsCommand == null) {
                    loadSettingsCommand = new DelegateCommand(ExecuteLoadSettings);
                }
                return loadSettingsCommand;
            }
        }

        private void ExecuteLoadSettings ()
        {
            logger.Info($"> ExecuteLoadSettings()");
            using (WinForms.OpenFileDialog ofd = new WinForms.OpenFileDialog()) {
                ofd.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.InitialDirectory = settingsFileLocation;

                if (ofd.ShowDialog() == WinForms.DialogResult.OK) {
                    settingsFileLocation = ofd.FileName;
                    SettingsFileLocationTxt = settingsFileLocation;

                    // load the settings file using quickload
                    ExecuteQuickLoadSettings();
                }
            }
            logger.Info($"< ExecuteLoadSettings()");
        }

        DelegateCommand quickLoadSettingsCommand;
        public ICommand QuickLoadSettingsCommand {
            get {
                if (quickLoadSettingsCommand == null) {
                    quickLoadSettingsCommand = new DelegateCommand(ExecuteQuickLoadSettings);
                }
                return quickLoadSettingsCommand;
            }
        }

        private void ExecuteQuickLoadSettings ()
        {
            logger.Info($"> ExecuteQuickLoadSettings()");
            // Load settings from settingsFileLocation
            settingsFileHandler.ReadFromXml(settingsFileLocation);
            UpdateSavedSettings();
            logger.Info($"< ExecuteQuickLoadSettings()");
        }

        DelegateCommand clearCompletedCommand;
        public ICommand ClearCompletedCommand
        {
            get
            {
                if (clearCompletedCommand == null)
                {
                    clearCompletedCommand = new DelegateCommand(ExecuteclearCompleted);
                }
                return clearCompletedCommand;
            }
        }

        private void ExecuteclearCompleted()
        {
            logger.Info($">< ExecuteclearCompleted()");
            foreach (var item in Transfers.ToList())
            {
                var queue = item.Value.Queue;

                if (queue.Progress == 100 || !queue.Running)
                {
                    Transfers.Remove(item);
                }
            }
        }

        private void connectCallback(object sender, string error)
        {
            logger.Info($"> connectCallback(sender: {sender}, error: {error})");
            if (sender is TransferClient)
            {
                var transferClient = sender as TransferClient;

                if (error != null)
                {
                    foreach (var client in transferClients)
                    {
                        client.Close();
                    }
                    transferClients.Clear();
                    ConnectionStatus = error;
                    return;
                }

                registerEvents(transferClient);
                transferClient.OutputFolder = outputFolder;
                transferClient.Run();
                ConnectionStatus = transferClient.EndPoint.Address.ToString();
                timerOverallProgress.Start();
                ConnectText = "Disconnect";
            }
            logger.Info($"< connectCallback(sender: {sender}, error: {error})");
        }

        private void registerEvents(TransferClient transferClient)
        {
            logger.Info($"> registerEvents(transferClient: {transferClient})");
            transferClient.Complete += TransferClient_Complete;
            transferClient.Disconnected += TransferClient_Disconnected;
            transferClient.ProgressChanged += TransferClient_ProgressChanged;
            transferClient.Queued += TransferClient_Queued;
            transferClient.Stopped += TransferClient_Stopped;
            transferClient.Paused += TransferClient_Paused;
            logger.Info($"< registerEvents(transferClient: {transferClient})");
        }

        private void TransferClient_Stopped(object sender, TransferQueue queue)
        {
            logger.Info($"> TransferClient_Stopped(sender: {sender}, queue: [{queue}])");
            var keyValuePair = Transfers.ToList().Single(i => i.Value.Id == queue.Id.ToString());
            keyValuePair.Value.State = TransferState.Stopped;
            
            if (transferClients.Count <= 0)
            {
                return;
            }

            int progress = 0;

            foreach (var client in transferClients)
            {
                progress += client.GetOverallProgress();
            }

            ProgressOverall = progress / transferClients.Count;
            logger.Info($"< TransferClient_Stopped(sender: {sender}, queue: [{queue}])");
        }

        private void TransferClient_Paused(object sender, TransferQueue queue)
        {
            logger.Info($"> TransferClient_Paused(sender: {sender}, queue: [{queue}])");
            var transfer = Transfers.ToList().Single(i => i.Value.Id == queue.Id.ToString()).Value;
            transfer.State = (transfer.State == TransferState.Paused ? TransferState.Running : TransferState.Paused);
            logger.Info($"< TransferClient_Paused(sender: {sender}, queue: [{queue}])");
        }

        private void TransferClient_Queued(object sender, TransferQueue queue)
        {
            logger.Info($"> TransferClient_Queued(sender: {sender}, queue: [{queue}])");
            if (sender is TransferClient)
            {
                var transferClient = sender as TransferClient;

                Transfer transfer = new Transfer
                {
                    Id = queue.Id.ToString(),
                    FileName = queue.Filename,
                    Type = (queue.Type == QueueType.Download ? "Download" : "Upload"),
                    Progress = "0%",
                    Queue = queue,
                    State = TransferState.Running
                };
                AddTransfer(transfer);

                if (queue.Type == QueueType.Download)
                {
                    transferClient.StartTransfer(queue);
                }
            }
            logger.Info($"< TransferClient_Queued(sender: {sender}, queue: [{queue}])");
        }

        private void TransferClient_ProgressChanged(object sender, TransferQueue queue)
        {
            logger.Trace($"> TransferClient_ProgressChanged(sender: {sender}, queue: [{queue}])");
            try
            {
                List<KeyValuePair<string, Transfer>> tmp = new List<KeyValuePair<string, Transfer>>(Transfers);
                foreach (var item in tmp)
                {
                    if (item.Value.Id == queue.Id.ToString())
                    {
                        item.Value.Progress = $"{queue.Progress}%";
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex, $"Something bad happened");
            }
            logger.Trace($"< TransferClient_ProgressChanged(sender: {sender}, queue: [{queue}])");
        }

        private void TransferClient_Disconnected(object sender, EventArgs e)
        {
            logger.Info($"> TransferClient_Disconnected(sender: {sender}, EventArgs: {e})");
            if (sender is TransferClient)
            {
                var transferClient = sender as TransferClient;

                deregisterEvents(transferClient);

                int progress = 0;
                foreach (var item in Transfers.ToList())
                {
                    var queue = item.Value.Queue;
                    if (queue.Client == transferClient)
                    {
                        queue.Close();

                        App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                        {
                            Transfers.Remove(item);
                        });
                    }
                    else
                    {
                        progress += queue.Progress;
                    }
                }

                ProgressOverall = progress / Transfers.Count;

                transferClients.Remove(transferClient);
                ConnectionStatus = "No connection";

                if (serverRunning)
                {
                    listener.Start(int.Parse(Port.Trim()));
                    ConnectionStatus = "Waiting...";
                }
                else
                {
                    ConnectText = "Connect";
                }
            }
            logger.Info($"< TransferClient_Disconnected(sender: {sender}, EventArgs: {e})");
        }

        private void TransferClient_Complete(object sender, TransferQueue queue)
        {
            logger.Info($"> TransferClient_Complete(sender: {sender}, queue: [{queue}])");
            List<KeyValuePair<string, Transfer>> tmp = new List<KeyValuePair<string, Transfer>>(Transfers);
            foreach (var item in tmp)
            {
                if (item.Value.Id == queue.Id.ToString())
                {
                    item.Value.State = TransferState.Completed;

                    if (serverRunning && !queue.SelfCreated)
                    {
                        foreach (var client in transferClients)
                        {
                            if(client != (sender as TransferClient))
                            {
                                client.QueueTransfer(Path.Combine(outputFolder,queue.Filename));
                            }
                        }
                    }
                    break;
                }
            }
            logger.Info($"< TransferClient_Complete(sender: {sender}, queue: [{queue}])");
        }

        private void deregisterEvents(TransferClient transferClient)
        {
            logger.Info($"> deregisterEvents(transferClient: {transferClient})");
            if (transferClient == null)
            {
                return;
            }

            transferClient.Complete -= TransferClient_Complete;
            transferClient.Disconnected -= TransferClient_Disconnected;
            transferClient.ProgressChanged -= TransferClient_ProgressChanged;
            transferClient.Queued -= TransferClient_Queued;
            transferClient.Stopped -= TransferClient_Stopped;
            logger.Info($"< deregisterEvents(transferClient: {transferClient})");
        }

        public MainViewModel()
        {
            logger.Info($"> MainViewModel()");
            transferClients = new List<TransferClient>();
            catalogItems = new List<CatalogItemModel>();
            hub = new MessageHub();
            listener = new Listener();
            listener.Accepted += Listener_Accepted;
            settingsFileHandler = new SettingsFileHandler();

            timerOverallProgress = new Timer();
            timerOverallProgress.Interval = 1000;
            timerOverallProgress.Elapsed += TimerOverallProgress_Elapsed;

            initDefaults();

            Transfers = new ObservableCollection<KeyValuePair<string, Transfer>>();

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            setupInv();
            logger.Info($"< MainViewModel()");
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            logger.Info($"> OnWindowClosing()");
            foreach (var client in transferClients)
            {
                deregisterEvents(client);
            }
            Properties.Settings.Default.Save();
            logger.Info($"< OnWindowClosing()");
        }

        private void initDefaults()
        {
            logger.Debug($"> initDefaults()");
            settingsFileLocation = Properties.Settings.Default.SettingsFileLocation;
            ExecuteQuickLoadSettings();
            ConnectionStatus = "No connection";
            ConnectText = "Connect";
            serverRunning = false;
            ProgressOverall = 0;
            SetFolderOutputTxt = outputFolder;
            SettingsFileLocationTxt = settingsFileLocation;
            logger.Debug($"< initDefaults()");
        }

        private void UpdateSavedSettings() {
            Host = settingsFileHandler.currentSettings.host;
            Port = settingsFileHandler.currentSettings.port;
            outputFolder = settingsFileHandler.currentSettings.outputFolder;
            SetFolderOutputTxt = outputFolder;
        }

        private void PrepareSavedSettings() {
            settingsFileHandler.currentSettings.host = host;
            settingsFileHandler.currentSettings.port = port;
            settingsFileHandler.currentSettings.outputFolder = outputFolder;
        }

        private void TimerOverallProgress_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (transferClients.Count <= 0)
            {
                return;
            }

            int progress = 0;
            foreach (var client in transferClients)
            {
                progress += client.GetOverallProgress();
            }
            ProgressOverall = progress / transferClients.Count;
        }

        private void Listener_Accepted(object sender, SocketAcceptedEventArgs e)
        {
            logger.Info($"> Listener_Accepted(sender: {sender}, SocketAcceptedEventArgs: {e})");
            var transferClient = new TransferClient(e.Accepted);
            transferClients.Add(transferClient);
            transferClient.OutputFolder = outputFolder;

            registerEvents(transferClient);
            transferClient.Run();
            timerOverallProgress.Start();
            ConnectionStatus = transferClient.EndPoint.Address.ToString();

            sendFileCommand.RaiseCanExecuteChanged();
            logger.Info($"< Listener_Accepted(sender: {sender}, SocketAcceptedEventArgs: {e})");
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
