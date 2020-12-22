using Easy.MessageHub;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace InventoryControlLib
{
    /// <summary>
    /// Interaction logic for InventoryGrid.xaml
    /// </summary>
    public partial class InventoryGrid : UserControl
    {
        private IMessageHub hub;
        private Guid subscriptionToken;

        public InventoryGrid()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        public override void EndInit()
        {
            base.EndInit();

            if (Inventory.ColumnDefinitions.Count > 0 && Inventory.RowDefinitions.Count > 0)
            {

                for (int y = 0; y < Inventory.ColumnDefinitions.Count; y++)
                {
                    for (int x = 0; x < Inventory.RowDefinitions.Count; x++)
                    {
                        Border border = new Border
                        {
                            Background = Brushes.LightGray,
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.75),
                            Width = CellWidth,
                            Height = CellHeight,
                        };
                        Grid.SetColumn(border, y);
                        Grid.SetRow(border, x);
                        Inventory.Children.Add(border);
                    }
                }

                hub = MessageHub;
                subscriptionToken = hub.Subscribe<ItemPositionUpdate>(ItemPositionUpdate);

                Item arrow = new Item(hub, Inventory, CellWidth, CellHeight, new ItemModel(CellWidth, CellHeight, 1, 1, 1, true, new Uri("https://www.clipartmax.com/png/full/414-4147920_bow-arrow-symbol-vector-icon-illustration-triangle.png")));
                arrow.MouseReleased += Item_MouseReleased;
                arrow.MousePressed += Item_MousePressed;
                Inventory.Children.Add(arrow);

                Item sword = new Item(hub, Inventory, CellWidth, CellHeight, new ItemModel(CellWidth, CellHeight, 1, 2, 1, false, new Uri("https://icons.iconarchive.com/icons/chanut/role-playing/256/Sword-icon.png")));
                Grid.SetRowSpan(sword, 2);
                sword.MouseReleased += Item_MouseReleased;
                sword.MousePressed += Item_MousePressed;
                Inventory.Children.Add(sword);
            }
        }

        private void Item_MousePressed(object sender, Point mousePosition)
        {
            if (sender is Item)
            {
            }
        }

        private void ItemPositionUpdate(ItemPositionUpdate positionUpdate)
        {
            var item = positionUpdate.Item;
            var releasePoint = positionUpdate.Position;

            var width = Columns * CellWidth;
            var height = Rows * CellHeight;
            var screenPoint = TranslatePoint(new Point(0, 0), Application.Current.MainWindow);

            if (releasePoint.X < screenPoint.X + width && releasePoint.Y < screenPoint.Y + height
                && releasePoint.X > screenPoint.X && releasePoint.Y > screenPoint.Y)
            {
                var cellX = (int)Math.Floor((releasePoint.X - screenPoint.X) / CellWidth);
                var cellY = (int)Math.Floor((releasePoint.Y - screenPoint.Y) / CellHeight);
                var x = cellX * CellWidth + screenPoint.X;
                var y = cellY * CellHeight + screenPoint.Y;
                
                if (item.GridParent.Children.Contains(item))
                {
                    item.GridParent.Children.Remove(item);
                }

                Inventory.Children.Add(item);
                item.GridParent = Inventory;
                Point p = new Point(x, y);
                item.Transform(p);
            }
        }

        private void Item_MouseReleased(object sender, Point mousePosition)
        {
            if (sender is Item)
            {
            }
        }

        #region DependencyProperty Content

        /// <summary>
        /// Registers a dependency property as backing store for the Rows property
        /// </summary>
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(int), typeof(InventoryGrid));

        /// <summary>
        /// Gets or sets the Rows.
        /// </summary>
        /// <value>The number of Rows.</value>
        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        /// <summary>
        /// Registers a dependency property as backing store for the Columns property
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(int), typeof(InventoryGrid));

        /// <summary>
        /// Gets or sets the Columns.
        /// </summary>
        /// <value>The number of Columns.</value>
        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        /// <summary>
        /// Registers a dependency property as backing store for the Columns property
        /// </summary>
        public static readonly DependencyProperty CellWidthProperty =
            DependencyProperty.Register("CellWidth", typeof(double), typeof(InventoryGrid),
            new PropertyMetadata(50.0));

        /// <summary>
        /// Gets or sets the Columns.
        /// </summary>
        /// <value>The number of Columns.</value>
        public double CellWidth
        {
            get { return (double)GetValue(CellWidthProperty); }
            set { SetValue(CellWidthProperty, value); }
        }

        /// <summary>
        /// Registers a dependency property as backing store for the Columns property
        /// </summary>
        public static readonly DependencyProperty CellHeightProperty =
            DependencyProperty.Register("CellHeight", typeof(double), typeof(InventoryGrid),
            new PropertyMetadata(50.0));

        /// <summary>
        /// Gets or sets the Columns.
        /// </summary>
        /// <value>The number of Columns.</value>
        public double CellHeight
        {
            get { return (double)GetValue(CellHeightProperty); }
            set { SetValue(CellHeightProperty, value); }
        }

        /// <summary>
        /// Registers a dependency property as backing store for the Columns property
        /// </summary>
        public static readonly DependencyProperty MessageHubProperty =
            DependencyProperty.Register("MessageHub", typeof(IMessageHub), typeof(InventoryGrid),
            new PropertyMetadata(new MessageHub()));

        /// <summary>
        /// Gets or sets the Columns.
        /// </summary>
        /// <value>The number of Columns.</value>
        public IMessageHub MessageHub
        {
            get { return (IMessageHub)GetValue(MessageHubProperty); }
            set { SetValue(MessageHubProperty, value); }
        }

        #endregion
    }
}
