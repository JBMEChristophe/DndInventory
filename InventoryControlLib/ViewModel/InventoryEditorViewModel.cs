using Microsoft.Win32;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Utilities;

namespace InventoryControlLib.ViewModel
{
    public delegate void SaveEvent(InventoryEditorViewModel sender);

    public class InventoryEditorViewModel : INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public event SaveEvent SaveClicked; 
        public Action CloseWindow { get; set; }

        public InventoryEditorViewModel(string inventoryName, string backgroundPath, bool newInventory = false)
        {
            logger.Info($"> InventoryEditorViewModel()");
            InventoryName = inventoryName;
            BackgroundPath = backgroundPath;
            NewInventory = newInventory;
            XValue = 1;
            YValue = 1;
            logger.Info($"< InventoryEditorViewModel()");
        }

        bool newInventory;
        public bool NewInventory
        {
            get
            {
                return newInventory;
            }
            set
            {
                if (newInventory != value)
                {
                    newInventory = value;
                    OnPropertyChange("NewInventory");
                }
            }
        }

        string inventoryName;
        public string InventoryName
        {
            get
            {
                return inventoryName;
            }
            set
            {
                if(inventoryName !=  value)
                {
                    inventoryName = value;
                    OnPropertyChange("InventoryName");
                    saveCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        int xValue;
        public int XValue
        {
            get
            {
                return xValue;
            }
            set
            {
                if (xValue != value)
                {
                    xValue = value;
                    OnPropertyChange("XValue");
                }
            }
        }

        int yValue;
        public int YValue
        {
            get
            {
                return yValue;
            }
            set
            {
                if (yValue != value)
                {
                    yValue = value;
                    OnPropertyChange("YValue");
                }
            }
        }

        Uri backgroundPath;
        public string BackgroundPath
        {
            get
            {
                if (backgroundPath == null)
                {
                    return null;
                }

                if (backgroundPath.IsAbsoluteUri)
                {
                    return backgroundPath.AbsoluteUri;
                }
                else
                {
                    var tmp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, backgroundPath.ToString());
                    return tmp;
                }
            }
            set
            {
                if (value == "")
                {
                    backgroundPath = new Uri(@"Images\No_image_available.png", UriKind.Relative);
                }
                else
                {
                    backgroundPath = new Uri(value, UriKind.RelativeOrAbsolute);
                    if (!backgroundPath.IsAbsoluteUri)
                    {
                        backgroundPath = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, backgroundPath.ToString()), UriKind.Absolute);
                    }
                }
                OnPropertyChange("BackgroundPath");
                saveCommand?.RaiseCanExecuteChanged();
            }
        }

        DelegateCommand browseCommand;
        public ICommand BrowseCommand
        {
            get
            {
                if (browseCommand == null)
                {
                    browseCommand = new DelegateCommand(ExecuteBrowse);
                }
                return browseCommand;
            }
        }

        private void ExecuteBrowse()
        {
            logger.Debug("> ExecuteBrowse()");
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.jpg;*.jpeg;*.bmp;*.png)|*.jpg;*.jpeg;*.bmp;*.png|All Files (*.*)|*.*";
            ofd.DefaultExt = ".png";

            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                BackgroundPath = PathHelper.GetRelativePathFromApplication(ofd.FileName);
            }
            logger.Debug("< ExecuteBrowse()");
        }

        DelegateCommand saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave);
                }
                return saveCommand;
            }
        }

        public bool Saved { get; set; }

        private void ExecuteSave()
        {
            logger.Debug("> ExecuteSave()");
            SaveClicked?.Invoke(this);
            Saved = true;
            logger.Debug("< ExecuteSave()");
            CloseWindow();
        }

        private bool CanExecuteSave()
        {
            return !string.IsNullOrEmpty(BackgroundPath) && !string.IsNullOrEmpty(InventoryName);
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
