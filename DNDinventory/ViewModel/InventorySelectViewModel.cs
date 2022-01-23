using InventoryControlLib;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace DNDinventory.ViewModel
{
    public class InventorySelectViewModel : INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Window window;

        private List<UpdateGrid> dontShowGridFilter;
        public List<UpdateGrid> Inventories
        {
            get
            {
                return GridManager.Instance.Grids.Except(dontShowGridFilter).ToList();
            }
        }

        private bool isFallbackExpended;
        public bool IsFallbackExpended
        {
            get
            {
                return isFallbackExpended;
            }
            set
            {
                if (isFallbackExpended != value)
                {
                    isFallbackExpended = value; 
                    adjustWindowHeight();
                    OnPropertyChange("IsFallbackExpended");
                }
            }
        }

        private DialogResult dialogResult;
        public DialogResult DialogResult 
        { 
            get
            {
                return dialogResult;
            }
            set
            {
                if (dialogResult != value)
                {
                    dialogResult = value;
                    OnPropertyChange("DialogResult");
                }
            }
        }

        private Guid selectedGrid;
        public Guid SelectedGridId
        {
            get
            {
                return selectedGrid;
            }
            set
            {
                if (selectedGrid != value)
                {
                    selectedGrid = value;
                    OnPropertyChange("SelectedGridId");
                }
            }
        }

        private Guid[] selectedFallbackGridId = new Guid[4];
        public Guid[] SelectedFallbackGridId
        {
            get
            {
                return selectedFallbackGridId;
            }
            set
            {
                if (selectedFallbackGridId != value)
                {
                    selectedFallbackGridId = value;
                    OnPropertyChange("SelectedFallbackGridId");
                }
            }
        }

        DelegateCommand deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new DelegateCommand(ExecuteDelete);
                }
                return deleteCommand;
            }
        }

        private void ExecuteDelete()
        {
            logger.Info($"> ExecuteDelete()");
            DialogResult = DialogResult.OK;
            window.Close();
            logger.Info($"< ExecuteDelete()");
        }

        DelegateCommand cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                {
                    cancelCommand = new DelegateCommand(ExecuteCancel);
                }
                return cancelCommand;
            }
        }

        private void ExecuteCancel()
        {
            logger.Info($"> ExecuteCancel()");
            DialogResult = DialogResult.Cancel;
            window.Close();
            logger.Info($"< ExecuteCancel()");
        }

        private void adjustWindowHeight()
        {
            window.Height = (IsFallbackExpended) ? 170 : 75;
        }

        public InventorySelectViewModel(List<Guid> dontShowInventoryFilter = null)
        {
            dontShowGridFilter = new List<UpdateGrid>();
            if (dontShowInventoryFilter != null)
            {
                foreach (var guid in dontShowInventoryFilter)
                {
                    dontShowGridFilter.Add(GridManager.Instance.Grids.Where((e) => e.Id == guid).First());
                }
            }
        }

        public void Init(Window window)
        {
            OnPropertyChange("Inventories");
            this.window = window;
            IsFallbackExpended = false;
            adjustWindowHeight();
            SelectedGridId = GridManager.Instance.GroundId;
            for (int i = 0; i < SelectedFallbackGridId.Length; i++)
            {
                SelectedFallbackGridId[i] = SelectedGridId;
            }
            OnPropertyChange("SelectedFallbackGridId");
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
