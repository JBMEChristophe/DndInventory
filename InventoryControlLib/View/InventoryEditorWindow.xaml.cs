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
    /// Interaction logic for InventoryEditorWindow.xaml
    /// </summary>
    public partial class InventoryEditorWindow : Window
    {
        public readonly InventoryEditorViewModel _viewModel;

        public InventoryEditorWindow(InventoryEditorViewModel viewModel)
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;

            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            if (_viewModel.CloseWindow == null)
                _viewModel.CloseWindow = new Action(this.Close);
        }
    }
}
