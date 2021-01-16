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

namespace DNDinventory.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        private readonly IMessageHub hub;
        private Listener listener;
        private TransferClient transferClient;
        private string outputFolder;
        private string settingsFileLocation;
        private Timer timerOverallProgress;

        private bool serverRunning;

        public IMessageHub Hub
        {
            get
            {
                return hub;
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
                    Properties.Settings.Default.Host = host;
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
                    Properties.Settings.Default.Port = port;
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
                    Properties.Settings.Default.OutputFolder = value;
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
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                Transfers.Add(new KeyValuePair<string, Transfer>(t.Id, t));
                OnPropertyChange("Transfers");
            });
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
            if (transferClient==null)
            {
                transferClient = new TransferClient();
                transferClient.Connect(Host.Trim(), int.Parse(Port.Trim()), connectCallback);
            }
            else
            {
                transferClient.Close();
                transferClient = null;
            }

            sendFileCommand.RaiseCanExecuteChanged();
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
            if(serverRunning)
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
            catch (Exception)
            {
                ConnectionStatus = $"Unable to listing on port {Port.Trim()}";
            }
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
            if (!serverRunning)
            {
                return;
            }

            if (transferClient!=null)
            {
                transferClient.Close();
            }

            listener.Stop();
            timerOverallProgress.Stop();
            ConnectionStatus = "No connection";
            serverRunning = false;
            startServerCommand.RaiseCanExecuteChanged();
            stopServerCommand.RaiseCanExecuteChanged();
            connectCommand?.RaiseCanExecuteChanged();
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
            if (transferClient == null)
            {
                return;
            }

            using (WinForms.OpenFileDialog ofd = new WinForms.OpenFileDialog())
            {
                ofd.Filter = "All Files (*.*)|*.*";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == WinForms.DialogResult.OK)
                {
                    foreach (var file in ofd.FileNames)
                    {
                        transferClient.QueueTransfer(file);
                    }
                }
            }
        }

        private bool CanExecuteSendFile()
        {
            return (transferClient != null);
        }

        DelegateCommand<string> pauseTransferCommand;
        public ICommand PauseTransferCommand
        {
            get
            {
                if (pauseTransferCommand == null)
                {
                    pauseTransferCommand = new DelegateCommand<string>(ExecutePauseTransfer, CanExecutePauseTransfer);
                }
                return pauseTransferCommand;
            }
        }

        private void ExecutePauseTransfer(string id)
        {
            if (transferClient == null)
            {
                return;
            }

            Transfer transfer = Transfers.Single(i => i.Value.Id == id).Value;
            var queue = transfer.Queue;
            queue.Client.PauseTransfer(queue);
        }

        private bool CanExecutePauseTransfer(string id)
        {
            return true;
        }

        DelegateCommand<string> stopTransferCommand;
        public ICommand StopTransferCommand
        {
            get
            {
                if (stopTransferCommand == null)
                {
                    stopTransferCommand = new DelegateCommand<string>(ExecuteStopTransfer, CanExecuteStopTransfer);
                }
                return stopTransferCommand;
            }
        }

        private void ExecuteStopTransfer(string id)
        {
            if (transferClient == null)
            {
                return;
            }

            var keyValuePair = Transfers.Single(i => i.Value.Id == id);
            var queue = keyValuePair.Value.Queue;
            queue.Client.StopTransfer(queue);

            progressOverall = 0;
        }

        private bool CanExecuteStopTransfer(string id)
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
            using (WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog())
            {
                fbd.SelectedPath = outputFolder;
                if (fbd.ShowDialog() == WinForms.DialogResult.OK)
                {
                    outputFolder = fbd.SelectedPath;

                    if (transferClient!=null)
                    {
                        transferClient.OutputFolder = outputFolder;
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

        private void ExecuteSaveSettings () {
            using (WinForms.SaveFileDialog sfd = new WinForms.SaveFileDialog()) {
                sfd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                sfd.FilterIndex = 1;
                sfd.InitialDirectory = settingsFileLocation;

                if (sfd.ShowDialog() == WinForms.DialogResult.OK) {
                    settingsFileLocation = sfd.FileName;

                    // Actually save the settings file
                    ExecuteQuickSaveSettings();

                    SettingsFileLocationTxt = settingsFileLocation;
                }
            }
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

        private void ExecuteQuickSaveSettings () {
            // Save settings file to settingsFileLocation
            StreamWriter sw = new StreamWriter(settingsFileLocation);

            sw.WriteLine(Host);
            sw.WriteLine(Port);
            sw.WriteLine(outputFolder);

            sw.Close();
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

        private void ExecuteLoadSettings () {
            using (WinForms.OpenFileDialog ofd = new WinForms.OpenFileDialog()) {
                ofd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.InitialDirectory = settingsFileLocation;

                if (ofd.ShowDialog() == WinForms.DialogResult.OK) {
                    settingsFileLocation = ofd.FileName;

                    // Actually load the settings file
                    ExecuteQuickLoadSettings();

                    SettingsFileLocationTxt = settingsFileLocation;
                }
            }
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

        private void ExecuteQuickLoadSettings () {
            // Load settings file from settingsFileLocation
            if (File.Exists(settingsFileLocation)) {
                StreamReader sr = new StreamReader(settingsFileLocation);

                try {
                    Host = sr.ReadLine();
                    Port = sr.ReadLine();
                    outputFolder = sr.ReadLine();
                    setFolderOutputTxt = outputFolder;
                } 
                catch {
                    throw new EndOfStreamException("Settings file incomplete/missing");
                }
                sr.Close();
            }
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
            if (error!=null)
            {
                transferClient.Close();
                transferClient = null;
                ConnectionStatus = error;
                return;
            }

            registerEvents();
            transferClient.OutputFolder = outputFolder;
            transferClient.Run();
            ConnectionStatus = transferClient.EndPoint.Address.ToString();
            timerOverallProgress.Start();
            ConnectText = "Disconnect";

        }

        private void registerEvents()
        {
            transferClient.Complete += TransferClient_Complete;
            transferClient.Disconnected += TransferClient_Disconnected;
            transferClient.ProgressChanged += TransferClient_ProgressChanged;
            transferClient.Queued += TransferClient_Queued;
            transferClient.Stopped += TransferClient_Stopped;
            transferClient.Paused += TransferClient_Paused;
        }

        private void TransferClient_Stopped(object sender, TransferQueue queue)
        {
            var keyValuePair = Transfers.Single(i => i.Value.Id == queue.Id.ToString());
            keyValuePair.Value.State = TransferState.Stopped;
            
            if (transferClient == null)
            {
                return;
            }

            ProgressOverall = transferClient.GetOverallProgress();
        }

        private void TransferClient_Paused(object sender, TransferQueue queue)
        {
            var transfer = Transfers.Single(i => i.Value.Id == queue.Id.ToString()).Value;
            transfer.State = (transfer.State == TransferState.Paused ? TransferState.Running : TransferState.Paused);
        }

        private void TransferClient_Queued(object sender, TransferQueue queue)
        {
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

            if (queue.Type==QueueType.Download)
            {
                transferClient.StartTransfer(queue);
            }
        }

        private void TransferClient_ProgressChanged(object sender, TransferQueue queue)
        {
            var keyValuePair = Transfers.Single(i => i.Value.Id == queue.Id.ToString());
            keyValuePair.Value.Progress = $"{queue.Progress}%";
        }

        private void TransferClient_Disconnected(object sender, EventArgs e)
        {
            deregisterEvents();

            foreach (var item in Transfers)
            {
                var queue = item.Value.Queue;
                queue.Close();
            }

            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                Transfers.Clear();
            });
            ProgressOverall = 0;

            transferClient = null;
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

        private void TransferClient_Complete(object sender, TransferQueue queue)
        {
            Transfers.Single(i => i.Value.Id == queue.Id.ToString()).Value.State = TransferState.Completed;
        }

        private void deregisterEvents()
        {
            if(transferClient == null)
            {
                return;
            }

            transferClient.Complete -= TransferClient_Complete;
            transferClient.Disconnected -= TransferClient_Disconnected;
            transferClient.ProgressChanged -= TransferClient_ProgressChanged;
            transferClient.Queued -= TransferClient_Queued;
            transferClient.Stopped -= TransferClient_Stopped;
        }

        public MainViewModel()
        {
            hub = new MessageHub();
            listener = new Listener();
            listener.Accepted += Listener_Accepted;

            timerOverallProgress = new Timer();
            timerOverallProgress.Interval = 1000;
            timerOverallProgress.Elapsed += TimerOverallProgress_Elapsed;
            
            //outputFolder = "Transfers"; //#TODO load from save
            outputFolder = Properties.Settings.Default.OutputFolder;

            Transfers = new ObservableCollection<KeyValuePair<string, Transfer>>();

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            initDefaults();
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            deregisterEvents();
            Properties.Settings.Default.Save();
        }

        private void initDefaults()
        {
            //Host = "localhost"; //#TODO load from save
            Host = Properties.Settings.Default.Host;
            //Port = "100"; //#TODO load from save
            Port = Properties.Settings.Default.Port;
            settingsFileLocation = Properties.Settings.Default.SettingsFileLocation;
            ConnectionStatus = "No connection";
            ConnectText = "Connect";
            serverRunning = false;
            ProgressOverall = 0;
            SetFolderOutputTxt = outputFolder;
            SettingsFileLocationTxt = settingsFileLocation;
        }

        private void TimerOverallProgress_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (transferClient == null)
            {
                return;
            }

            ProgressOverall = transferClient.GetOverallProgress();
        }

        private void Listener_Accepted(object sender, SocketAcceptedEventArgs e)
        {
            listener.Stop();

            transferClient = new TransferClient(e.Accepted);
            transferClient.OutputFolder = outputFolder;

            registerEvents();
            transferClient.Run();
            timerOverallProgress.Start();
            ConnectionStatus = transferClient.EndPoint.Address.ToString();

            sendFileCommand.RaiseCanExecuteChanged();
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
