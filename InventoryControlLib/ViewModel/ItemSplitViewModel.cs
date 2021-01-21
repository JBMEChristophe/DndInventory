using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InventoryControlLib.ViewModel
{
    public delegate bool SplitEvent(ItemModel sender, int value);

    public class ItemSplitViewModel : INotifyPropertyChanged
    {
        public event SplitEvent SplitClicked;

        private ItemModel item;

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

                    IsEnabled = (Value > 0);
                }
            }
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnPropertyChange("IsEnabled");
                    splitCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        DelegateCommand splitCommand;
        public ICommand SplitCommand
        {
            get
            {
                if (splitCommand == null)
                {
                    splitCommand = new DelegateCommand(ExecuteSplit, CanExecuteSplit);
                }
                return splitCommand;
            }
        }

        private void ExecuteSplit()
        {
            if (Value > 0)
            {
                if (SplitClicked != null && SplitClicked(item, Value))
                {
                    MinValue = (item.Quantity > 1) ? 1 : 0;
                    MaxValue = item.Quantity - 1;
                    Value = (Value < MaxValue) ? Value : MaxValue;

                    IsEnabled = item.IsStackable;

                    if (IsEnabled)
                    {
                        IsEnabled = MaxValue > 0;
                    }
                }
            }
        }

        private bool CanExecuteSplit()
        {
            return IsEnabled;
        }

        public ItemSplitViewModel(ItemModel item)
        {
            MinValue = (item.Quantity > 1) ? 1 : 0;
            MaxValue = item.Quantity - 1;
            Value = MinValue;

            IsEnabled = item.IsStackable;

            if(IsEnabled)
            {
                IsEnabled = MaxValue > 0;
            }

            this.item = item;
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
