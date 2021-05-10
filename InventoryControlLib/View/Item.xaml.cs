﻿using Easy.MessageHub;
using InventoryControlLib.Model;
using Prism.Commands;
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

namespace InventoryControlLib.View
{
    public delegate void MousePressEvent(object sender, Point mousePosition);
    public delegate void RightClickEvent(object sender);

    /// <summary>
    /// Interaction logic for Item.xaml
    /// </summary>
    public partial class Item : UserControl, INotifyPropertyChanged
    {
        public delegate void ItemEvent(Item sender);
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMessageHub hub;
        TranslateTransform transform = new TranslateTransform();
        Grid parent;
        Point currentPoint;
        Point startingPoint;
        bool isInDrag = false;

        public event MousePressEvent MouseReleased;
        public event MousePressEvent MousePressed;
        public event RightClickEvent ItemSplitClicked;
        public event RightClickEvent ItemStackClicked;
        public event ItemEvent ItemDeleteClicked;

        public Item(IMessageHub hub, Grid parent, UiItemModel model = null)
        {
            InitializeComponent();
            this.hub = hub;
            this.parent = parent;
            if (model == null)
            {
                this.Model = new UiItemModel("No_ID", "No Name", ItemType.Unknown, "No Cost", "No Weight", "No Rarity", "No Attunement", "No Properties", "No Description", "UNKOWN", 0, 0, 0, 0);
            }
            else
            {
                this.Model = model;
            }
            OnPropertyChange("Model");
        }

        DelegateCommand deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new DelegateCommand(Executedelete);
                }
                return deleteCommand;
            }
        }

        private void Executedelete()
        {
            logger.Info($"> Executedelete()");

            ItemDeleteClicked?.Invoke(this);

            logger.Info($"< Executedelete()");
        }

        public Grid GridParent
        {
            get
            {
                return parent;
            }
            set
            {
                if(parent != value)
                {
                    parent = value;
                }
            }
        }

        public UiItemModel Model { get; }

        DelegateCommand splitCommand;
        public ICommand SplitCommand
        {
            get
            {
                if (splitCommand == null)
                {
                    splitCommand = new DelegateCommand(ExecuteSplit, CanExecuteSplit);
                }
                return splitCommand;
            }
        }

        private void ExecuteSplit()
        {
            ItemSplitClicked?.Invoke(this);
            splitCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteSplit()
        {
            return Model.Quantity > 1;
        }

        DelegateCommand stackCommand;
        public ICommand StackCommand
        {
            get
            {
                if (stackCommand == null)
                {
                    stackCommand = new DelegateCommand(ExecuteStack);
                }
                return stackCommand;
            }
        }

        private void ExecuteStack()
        {
            ItemStackClicked?.Invoke(this);
            splitCommand.RaiseCanExecuteChanged();
        }

        public void RemoveEvents()
        {
            MouseReleased = null;
            MousePressed = null;
            ItemSplitClicked = null;
            ItemDeleteClicked = null;
        }

        public void Transform(Point p)
        {
            var screenPoint = TranslatePoint(new Point(0, 0), Application.Current.MainWindow);
            transform.X += (p.X - screenPoint.X);
            transform.Y += (p.Y - screenPoint.Y);
            this.RenderTransform = transform;
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            startingPoint = e.GetPosition(this);
            element.CaptureMouse();
            isInDrag = true;
            e.Handled = true;
            MousePressed?.Invoke(this, e.GetPosition(null));
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isInDrag)
            {
                var element = sender as FrameworkElement;
                element.ReleaseMouseCapture();
                isInDrag = false;
                e.Handled = true;
                MouseReleased?.Invoke(this, e.GetPosition(null));
                var itemPosition = TranslatePoint(new Point(0, 0), Application.Current.MainWindow);
                hub.Publish(new ItemPositionUpdate { Item = this, Position = itemPosition });
            }
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isInDrag)
            {
                currentPoint = e.GetPosition(null);

                if (currentPoint.X < Application.Current.MainWindow.RenderSize.Width && currentPoint.Y < Application.Current.MainWindow.RenderSize.Height
                    && currentPoint.X > 0 && currentPoint.Y > 0)
                {
                    correctItemOnMousePos();
                }
                else
                {
                    var screenPoint = TranslatePoint(new Point(0, 0), Application.Current.MainWindow);
                    if (screenPoint.X + Model.Width > Application.Current.MainWindow.RenderSize.Width)
                    {
                        transform.X += ((Application.Current.MainWindow.Width - screenPoint.X) - Model.Width);
                    }
                    if (screenPoint.X < 0)
                    {
                        transform.X += (-screenPoint.X);
                    }
                    if (screenPoint.Y + Model.Height > Application.Current.MainWindow.RenderSize.Height)
                    {
                        transform.Y += (Application.Current.MainWindow.Height - (screenPoint.Y + Model.Height));
                    }
                    if (screenPoint.Y < 0)
                    {
                        transform.Y += (-screenPoint.Y);
                    }
                    this.RenderTransform = transform;
                }
            }
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (isInDrag)
            {
                correctItemOnMousePos();
            }
        }

        private void correctItemOnMousePos()
        {
            if (isInDrag)
            {
                var currentPointDiff = Mouse.GetPosition(this);
                transform.X += currentPointDiff.X - startingPoint.X;
                transform.Y += currentPointDiff.Y - startingPoint.Y;
                this.RenderTransform = transform;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override string ToString()
        {
            return Model.ToString();
        }
    }
}
