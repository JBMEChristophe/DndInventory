using InventoryControlLib.Model;
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
    public class ItemEditViewModel : INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public CatalogItemModel Model
        {
            get; set;
        }

        bool showIdBox;
        public bool ShowIdBox
        {
            get
            {
                return showIdBox;
            }
            set
            {
                if (showIdBox != value)
                {
                    showIdBox = value;

                    OnPropertyChange("ShowIdBox");
                }
            }
        }

        bool defaultIdGeneration;
        public bool DefaultIdGeneration
        {
            get
            {
                return defaultIdGeneration;
            }
            set
            {
                if (defaultIdGeneration != value)
                {
                    defaultIdGeneration = value;
                    ShowIdBox = !DefaultIdGeneration;

                    OnPropertyChange("DefaultIdGeneration");
                }
            }
        }

        public string ID
        {
            get
            {
                return Model.ID;
            }
            set
            {
                if (Model.ID != value)
                {
                    Model.ID = value;

                    OnPropertyChange("ID");
                }
            }
        }

        public string Name
        {
            get
            {
                return Model.Name;
            }
            set
            {
                if (Model.Name != value)
                {
                    Model.Name = value;
                    OnPropertyChange("Name");

                    if (defaultIdGeneration)
                    {
                        ID = $"{Model.Source}_{Model.Name.Replace(" ", "_")}";
                    }
                }
            }
        }

        public string Source
        {
            get
            {
                return Model.Source;
            }
            set
            {
                if (Model.Source != value)
                {
                    Model.Source = value;
                    OnPropertyChange("Source");

                    if (defaultIdGeneration)
                    {
                        ID = $"{Model.Source}_{Model.Name.Replace(" ", "_")}";
                    }
                }
            }
        }

        string selectedType;
        public string SelectedType 
        { 
            get
            {
                return selectedType;
            }
            set
            {
                if (selectedType != value)
                {
                    selectedType = value;
                    OnPropertyChange("SelectedType");
                    addSelectedCommand.RaiseCanExecuteChanged();
                }
            }
        }

        string selectedModelType;
        public string SelectedModelType
        {
            get
            {
                return selectedModelType;
            }
            set
            {
                if (selectedModelType != value)
                {
                    selectedModelType = value;
                    OnPropertyChange("SelectedModelType");
                    deleteSelectedCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public List<string> AllTypeEnums
        {
            get
            {
                return EnumHelper.GetDescriptionListFromEnumList(Enum.GetValues(typeof(ItemType)).Cast<ItemType>().ToList());
            }
        }

        public List<string> TypeList
        {
            get
            {
                return EnumHelper.GetDescriptionListFromEnumList(Model.Type);
            }
        }

        DelegateCommand deleteSelectedCommand;
        public ICommand DeleteSelectedCommand
        {
            get
            {
                if (deleteSelectedCommand == null)
                {
                    deleteSelectedCommand = new DelegateCommand(ExecuteDeleteSelected, CanExecuteDeleteSelected);
                }
                return deleteSelectedCommand;
            }
        }

        private void ExecuteDeleteSelected()
        {
            logger.Info($"> ExecuteDeleteSelected()");

            var type = EnumHelper.GetEnumValueFromDescription<ItemType>(SelectedModelType);
            if (Model.Type.Contains(type))
            {
                Model.Type.Remove(type);
                OnPropertyChange("TypeList");
            }

            logger.Info($"< ExecuteDeleteSelected()");
        }

        private bool CanExecuteDeleteSelected()
        {
            return !string.IsNullOrEmpty(SelectedModelType);
        }

        DelegateCommand addSelectedCommand;
        public ICommand AddSelectedCommand
        {
            get
            {
                if (addSelectedCommand == null)
                {
                    addSelectedCommand = new DelegateCommand(ExecuteAddSelected, CanExecuteAddSelected);
                }
                return addSelectedCommand;
            }
        }

        private void ExecuteAddSelected()
        {
            logger.Info($"> ExecuteAddSelected()");

            var type = EnumHelper.GetEnumValueFromDescription<ItemType>(SelectedType);
            if (!Model.Type.Contains(type))
            {
                Model.Type.Add(type);
                OnPropertyChange("TypeList");
            }

            logger.Info($"< ExecuteAddSelected()");
        }

        private bool CanExecuteAddSelected()
        {
            return !string.IsNullOrEmpty(SelectedType);
        }

        DelegateCommand selectFileCommand;
        public ICommand SelectFileCommand
        {
            get
            {
                if (selectFileCommand == null)
                {
                    selectFileCommand = new DelegateCommand(ExecuteSelectFile);
                }
                return selectFileCommand;
            }
        }

        private void ExecuteSelectFile()
        {
            logger.Info($"> ExecuteSelectFile()");
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.jpg;*.jpeg;*.bmp;*.png)|*.jpg;*.jpeg;*.bmp;*.png|All Files (*.*)|*.*";
            ofd.DefaultExt = ".png";

            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                Model.ImageUri = PathHelper.GetRelativePathFromApplication(ofd.FileName);
            }
            logger.Info($"< ExecuteSelectFile()");
        }


        public ItemEditViewModel(CatalogItemModel item)
        {
            logger.Info($"> ItemSplitViewModel(item: [{item}])");
            Model = item;

            DefaultIdGeneration = (Model.ID == $"{Model.Source}_{Model.Name.Replace(" ", "_")}");

            OnPropertyChange("ShowIdBox");
            OnPropertyChange("DefaultIdGeneration");

            logger.Info($"< ItemSplitViewModel(item: [{item}])");
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