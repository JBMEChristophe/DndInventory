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
using System.Windows.Data;

namespace InventoryControlLib.ViewModel
{
    public class ItemCatalogViewModel : INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private CollectionViewSource itemCollection;
        private string filterText;

        public ObservableCollection<CatalogItem> Items { get; set; }

        public ICollectionView SourceCollection
        {
            get
            {
                return this.itemCollection.View;
            }
        }

        public ItemCatalogViewModel()
        {
            logger.Info("> ItemCatalogViewModel()");
            Items = new ObservableCollection<CatalogItem>();
            itemCollection = new CollectionViewSource();
            itemCollection.Source = Items;
            itemCollection.Filter += itemCollection_Filter;
            logger.Info("< ItemCatalogViewModel()");
        }

        public string FilterText
        {
            get
            {
                return filterText;
            }
            set
            {
                filterText = value;
                this.itemCollection.View.Refresh();
                OnPropertyChange("FilterText");
            }
        }

        void itemCollection_Filter(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(FilterText))
            {
                e.Accepted = true;
                return;
            }

            CatalogItem item = e.Item as CatalogItem;
            if (item.Model.Name.ToUpper().Contains(FilterText.ToUpper()))
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
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
