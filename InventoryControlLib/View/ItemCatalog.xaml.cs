using InventoryControlLib.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InventoryControlLib.View
{
    /// <summary>
    /// Interaction logic for ItemCatalog.xaml
    /// </summary>
    public partial class ItemCatalog : UserControl
    {
        public readonly ItemCatalogViewModel _viewModel;
        public ItemCatalog()
        {
            InitializeComponent();
            _viewModel = new ItemCatalogViewModel();
            DataContext = _viewModel;
        }

        private void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.ItemListView_SelectionChanged(sender, e);
        }
    }
}
