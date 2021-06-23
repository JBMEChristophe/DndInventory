using DNDinventory.ViewModel;
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
using System.Windows.Shapes;

namespace DNDinventory.View
{
    /// <summary>
    /// Interaction logic for InventorySelectWindow.xaml
    /// </summary>
    public partial class InventorySelectWindow : Window
    {
        private readonly InventorySelectViewModel _viewModel;
        public InventorySelectWindow(InventorySelectViewModel viewmodel)
        {
            _viewModel = viewmodel;
            DataContext = _viewModel;
            InitializeComponent();
            _viewModel.Init(this);
        }
    }
}
