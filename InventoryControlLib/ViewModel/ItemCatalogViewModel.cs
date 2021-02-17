using Easy.MessageHub;
using InventoryControlLib.Model;
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

        ObservableCollection<CatalogItem> items;
        public ObservableCollection<CatalogItem> Items 
        { 
            get
            {
                return items;
            }
            set
            {
                if(items!=value)
                {
                    items = value;
                    OnPropertyChange("Items");
                    OnPropertyChange("ItemCount");
                }
            }
        }

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
            Items.CollectionChanged += Items_CollectionChanged;
            logger.Info("< ItemCatalogViewModel()");
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChange("ItemCount");
            OnPropertyChange("FilterItemCount");
        }

        public int ItemCount
        {
            get
            {
                return Items.Count;
            }
        }

        public int FilterItemCount
        {
            get
            {
                return itemCollection.View.Cast<object>().Count();
            }
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
                OnPropertyChange("FilterItemCount");
            }
        }

        void itemCollection_Filter(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(FilterText))
            {
                e.Accepted = true;
                return;
            }

            List<KeyValuePair<string, string>> dict = new List<KeyValuePair<string, string>>();
            var filter = FilterText.ToUpper();
            if (filter.Contains("="))
            {
                var tmp = filter.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var tmp2 = tmp.Select(part => part.Split('='));
                foreach (var pair in tmp2)
                {
                    dict.Add(new KeyValuePair<string, string>(pair[0], pair[1]));
                }
            }

            CatalogItemModel item = (e.Item as CatalogItem).Model;

            bool accept = false;

            if (dict.Count > 0)
            {
                List<bool> accepts = new List<bool>();
                foreach (var filterItem in dict)
                {
                    switch (filterItem.Key)
                    {
                        case "NAME":
                            if (string.IsNullOrEmpty(filterItem.Value))
                            {
                                accepts.Add(true);
                            }
                            else
                            {
                                if (item.Name.ToUpper().Contains(filterItem.Value))
                                {
                                    accepts.Add(true);
                                }
                                else
                                {
                                    accepts.Add(false);
                                }
                            }
                            break;
                        case "TYPE":
                            if (string.IsNullOrEmpty(filterItem.Value))
                            {
                                accepts.Add(true);
                            }
                            else
                            {
                                if (item.TypeStr.ToUpper().Contains(filterItem.Value))
                                {
                                    accepts.Add(true);
                                }
                                else
                                {
                                    accepts.Add(false);
                                }
                            }
                            break;
                        case "SOURCE":
                            if (string.IsNullOrEmpty(filterItem.Value))
                            {
                                accepts.Add(true);
                            }
                            else
                            {
                                if (item.Source.ToUpper().Contains(filterItem.Value))
                                {
                                    accepts.Add(true);
                                }
                                else
                                {
                                    accepts.Add(false);
                                }
                            }
                            break;
                        default:
                            accepts.Add(true);
                            break;
                    }
                }
                if(!accepts.Contains(false))
                {
                    accept = true;
                }
            }
            else
            {
                if (item.Name.ToUpper().Contains(filter))
                {
                    accept = true;
                }
                if (item.TypeStr.ToUpper().Contains(filter))
                {
                    accept = true;
                }
                if (item.Source.ToUpper().Contains(filter))
                {
                    accept = true;
                }
            }

            e.Accepted = accept;
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
