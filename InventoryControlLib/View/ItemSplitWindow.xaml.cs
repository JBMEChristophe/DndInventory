﻿using InventoryControlLib.ViewModel;
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
    /// Interaction logic for ItemDetailWindow.xaml
    /// </summary>
    public partial class ItemSplitWindow : Window
    {
        private readonly ItemSplitViewModel _viewModel;

        public ItemSplitWindow(ItemSplitViewModel viewModel)
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;

            _viewModel = viewModel;
            DataContext = _viewModel;
            InitializeComponent();
        }
    }
}
