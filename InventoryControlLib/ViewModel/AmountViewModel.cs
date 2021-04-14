using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InventoryControlLib.ViewModel
{
    public class AmountViewModel : INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public AmountViewModel()
        {
            logger.Info($">< AmountViewModel()");
            MinValue = 1;
            MaxValue = 10000;
            Value = MinValue;
        }

        private int minValue;
        public int MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                if (minValue != value)
                {
                    minValue = value;
                    OnPropertyChange("MinValue");
                }
            }
        }

        private int maxValue;
        public int MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                if (maxValue != value)
                {
                    maxValue = value;
                    OnPropertyChange("MaxValue");
                }
            }
        }

        private int currentValue;
        public int Value
        {
            get
            {
                return currentValue;
            }
            set
            {
                if (currentValue != value)
                {
                    currentValue = value;
                    OnPropertyChange("Value");
                }
            }
        }

        DelegateCommand<Window> okCommand;
        public ICommand OkCommand
        {
            get
            {
                if (okCommand == null)
                {
                    okCommand = new DelegateCommand<Window>(ExecuteOk);
                }
                return okCommand;
            }
        }

        private void ExecuteOk(Window window)
        {
            logger.Debug("> ExecuteOk()");
            if (Value > 0)
            {
                logger.Debug("< ExecuteOk()"); 
                if (window != null)
                {
                    window.Close();
                }
            }
            else
            {
                logger.Warn("Value 0 or lower");
            }

            logger.Debug("< ExecuteOk()");
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
