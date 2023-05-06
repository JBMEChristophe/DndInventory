using Easy.MessageHub;
using InventoryControlLib.Model;
using InventoryControlLib.View;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Utilities;
using static InventoryControlLib.View.CatalogItem;

namespace InventoryControlLib.ViewModel
{
    public delegate void NewItemEvent(object sender, CatalogItemModel model);

    public class ItemCatalogViewModel : INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private CollectionViewSource itemCollection;
        private string filterText;
        private const string LastUsedFiltersFilePath = "LastUsedFilters.xml";

        public event NewItemEvent ItemAdded;
        public event CatalogEvent SaveCatalog;
        public event CatalogEvent DeleteCatalog;

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

        CatalogItem selectedCatalogItem;
        public CatalogItem SelectedCatalogItem
        {
            get
            {
                return selectedCatalogItem;
            }
            set
            {
                if (selectedCatalogItem != value)
                {
                    selectedCatalogItem = value;
                    OnPropertyChange("SelectedCatalogItem");
                    duplicateItemCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public void AddItem(CatalogItem item)
        {
            item.SaveCatalog += Item_SaveCatalog;
            item.DeleteCatalog += Item_DeleteCatalog;
            Items.Add(item);
        }

        private void Item_DeleteCatalog(CatalogItem sender)
        {
            if (sender is CatalogItem)
            {
                var item = sender as CatalogItem;

                if (!item.Model.IsDefault)
                {
                    var result = MessageBox.Show($"Are you sure you want to delete {item.Model.Name}?", "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        items.Remove(item);
                        DeleteCatalog?.Invoke(sender);
                    }
                }
            }
        }

        private void Item_SaveCatalog(CatalogItem sender)
        {
            SaveCatalog?.Invoke(sender);
        }

        public ICollectionView SourceCollection
        {
            get
            {
                return this.itemCollection.View;
            }
        }

        DelegateCommand createNewItemCommand;
        public ICommand CreateNewItemCommand
        {
            get
            {
                if (createNewItemCommand == null)
                {
                    createNewItemCommand = new DelegateCommand(ExecuteCreateNewItem);
                }
                return createNewItemCommand;
            }
        }

        private void ExecuteCreateNewItem()
        {
            logger.Info($"> ExecuteCreateNewItem()");

            var viewModel = new ItemEditViewModel(new CatalogItemModel("CUSTOM_New", "New", ItemType.Unknown, "", "", "", "", "", "", "CUSTOM", 50, 50));
            var editWindow = new ItemEditWindow(viewModel);
            editWindow.ShowDialog();

            ItemAdded?.Invoke(this, viewModel.Model);

            logger.Info($"< ExecuteCreateNewItem()");
        }

        DelegateCommand duplicateItemCommand;
        public ICommand DuplicateItemCommand
        {
            get
            {
                if (duplicateItemCommand == null)
                {
                    duplicateItemCommand = new DelegateCommand(ExecuteDuplicateItem, CanExecuteDuplicateItem);
                }
                return duplicateItemCommand;
            }
        }

        private void ExecuteDuplicateItem()
        {
            logger.Info($"> ExecuteDuplicateItem()");

            var itemModel = new CatalogItemModel(SelectedCatalogItem.Model);
            itemModel.Name += " (Copy)";
            itemModel.ID += "_(Copy)";
            var viewModel = new ItemEditViewModel(itemModel);
            var editWindow = new ItemEditWindow(viewModel);
            editWindow.ShowDialog();

            ItemAdded?.Invoke(this, viewModel.Model);

            logger.Info($"< ExecuteDuplicateItem()");
        }

        private bool CanExecuteDuplicateItem()
        {
            return (SelectedCatalogItem != null);
        }

        public ItemCatalogViewModel()
        {
            logger.Info("> ItemCatalogViewModel()");
            SelectedCatalogItem = null;
            Items = new ObservableCollection<CatalogItem>();
            itemCollection = new CollectionViewSource();
            itemCollection.Source = Items;
            itemCollection.Filter += itemCollection_Filter;
            Items.CollectionChanged += Items_CollectionChanged;
            ExecutedFilters = new ObservableCollection<string>();
            loadFilters();
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

        private ObservableCollection<string> executedFilters;
        public ObservableCollection<string> ExecutedFilters
        {
            get
            {
                return executedFilters;
            }
            set
            {
                executedFilters = value;
                OnPropertyChange("ExecutedFilters");
            }
        }

        private string selectedFilter;
        public string SelectedFilter
        {
            get
            {
                return selectedFilter;
            }
            set
            {
                if (selectedFilter != value)
                {
                    selectedFilter = value;
                    FilterText = selectedFilter;
                    OnPropertyChange("SelectedFilter");
                }
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

        DelegateCommand saveToLastExecutedCommand;
        public ICommand SaveToLastExecutedCommand
        {
            get
            {
                if (saveToLastExecutedCommand == null)
                {
                    saveToLastExecutedCommand = new DelegateCommand(ExecuteSaveToLastExecuted);
                }
                return saveToLastExecutedCommand;
            }
        }

        private void ExecuteSaveToLastExecuted()
        {
            logger.Debug("> ExecuteSaveToLastExecuted()");
            if(!string.IsNullOrEmpty(FilterText.Trim()))
            {
                if(ExecutedFilters.Count>15)
                {
                    ExecutedFilters.RemoveAt(0);
                }
                ExecutedFilters.Add(FilterText.Trim());

                saveFilters();
            }
            logger.Debug("< ExecuteSaveToLastExecuted()");
        }

        private void saveFilters()
        {
            XmlHelper<List<string>>.WriteToXml(LastUsedFiltersFilePath, ExecutedFilters.ToList());
        }

        private void loadFilters()
        {
            if (File.Exists(LastUsedFiltersFilePath))
            {
                ExecutedFilters = new ObservableCollection<string>(XmlHelper<List<string>>.ReadFromXml(LastUsedFiltersFilePath));
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
                    string[] f = { pair[1] };
                    if (pair[1].Contains("&"))
                    {
                        f = pair[1].Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    foreach (var filterItem in f)
                    {
                        dict.Add(new KeyValuePair<string, string>(pair[0], filterItem));
                    }
                }
            }

            CatalogItemModel item = (e.Item as CatalogItem).Model;

            bool accept = false;

            List<bool> accepts = new List<bool>();
            if (dict.Count > 0)
            {
                foreach (var filterItem in dict)
                {
                    switch (filterItem.Key.ToUpper())
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
                        case "RARITY":
                            if (string.IsNullOrEmpty(filterItem.Value))
                            {
                                accepts.Add(true);
                            }
                            else
                            {
                                if (item.Rarity.ToUpper().Contains(filterItem.Value))
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
            }
            else
            {
                string[] f = { filter };
                if(filter.Contains("&"))
                {
                    f = filter.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                }

                foreach (var filterItem in f)
                {
                    var tmpAccept = false;
                    if (item.Name.ToUpper().Contains(filterItem))
                    {
                        tmpAccept = true;
                    }
                    if (item.TypeStr.ToUpper().Contains(filterItem))
                    {
                        tmpAccept = true;
                    }
                    if (item.Source.ToUpper().Contains(filterItem))
                    {
                        tmpAccept = true;
                    }
                    accepts.Add(tmpAccept);
                }
            }

            if (!accepts.Contains(false))
            {
                accept = true;
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
