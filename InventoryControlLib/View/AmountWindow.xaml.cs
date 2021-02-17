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
using System.Windows.Shapes;

namespace InventoryControlLib.View
{
    /// <summary>
    /// Interaction logic for AmountWindow.xaml
    /// </summary>
    public partial class AmountWindow : Window
    {
        public readonly AmountViewModel _viewModel;
        public AmountWindow()
        {
            InitializeComponent();
            _viewModel = new AmountViewModel();
            DataContext = _viewModel;
        }
    }
}
