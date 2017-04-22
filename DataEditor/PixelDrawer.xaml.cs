﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DataEditor
{
    public partial class PixelDrawer : UserControl
    {
        public static readonly DependencyProperty LetterProperty = DependencyProperty.Register(
            "Letter",
            typeof(Letter),
            typeof(PixelDrawer),
            new PropertyMetadata());

        public Letter Letter
        {
            get { return (Letter) GetValue(LetterProperty); }
            set { SetValue(LetterProperty, value); }
        }

        public static readonly DependencyProperty IsReadonlyProperty = DependencyProperty.Register(
            "IsReadonly",
            typeof(bool),
            typeof(PixelDrawer),
            new PropertyMetadata());

        public bool IsReadonly
        {
            get { return (bool) GetValue(IsReadonlyProperty); }
            set { SetValue(IsReadonlyProperty, value); }
        }

        public PixelDrawer()
        {
            InitializeComponent();

            LayoutRoot.DataContext = this;
        }
        
        private Pixel GetPixelByPosition(FrameworkElement container, Point position)
        {
            var dx = (container.ActualWidth + 1) / Letter.Columns;
            var dy = (container.ActualHeight + 1) / Letter.Rows;

            var row = (int) Math.Floor(position.Y / dy);
            var column = (int) Math.Floor(position.X / dx);

            return Letter[row, column];
        }

        private void MouseHandler(object sender, MouseEventArgs e)
        {
            if (IsReadonly)
            {
                return;
            }

            var container = (FrameworkElement) sender;
            var pixel = GetPixelByPosition(container, e.GetPosition(container));

            if (e.RightButton.HasFlag(MouseButtonState.Pressed))
            {
                pixel.IsSelected = false;
            }
            else if (e.LeftButton.HasFlag(MouseButtonState.Pressed))
            {
                pixel.IsSelected = true;
            }
        }
    }
}
