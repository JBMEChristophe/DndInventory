using Easy.MessageHub;
using InventoryControlLib.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InventoryControlLib.ViewModel
{
    public class ItemCatalogViewModel : INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ObservableCollection<CatalogItem> Items { get; set; }

        public ItemCatalogViewModel()
        {
            logger.Info("> ItemCatalogViewModel()");
            Items = new ObservableCollection<CatalogItem>();
            logger.Info("< ItemCatalogViewModel()");
        }

        public void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
