using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MonoBuilder.Utils
{
    public static class ScrollBehavior
    {
        public static readonly DependencyProperty IsHorizontalScrollEnabledProperty =
            DependencyProperty.RegisterAttached("IsHorizontalScrollEnabled", typeof(bool), typeof(ScrollBehavior),
                new PropertyMetadata(false, OnIsHorizontalScrollEnabledChanged));

        public static void SetIsHorizontalScrollEnabled(DependencyObject obj, bool value) => obj.SetValue(IsHorizontalScrollEnabledProperty, value);
        public static bool GetIsHorizontalScrollEnabled(DependencyObject obj) => (bool)obj.GetValue(IsHorizontalScrollEnabledProperty);

        private static void OnIsHorizontalScrollEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                    element.PreviewMouseWheel += OnPreviewMouseWheel;
                else
                    element.PreviewMouseWheel -= OnPreviewMouseWheel;
            }
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Find the ScrollViewer inside the Control (TabControl, etc.)
            var scrollViewer = FindVisualChild<ScrollViewer>(sender as DependencyObject);
            var canScroll = Helpers.ElementCanScroll(scrollViewer, e.Delta, false);

            if (scrollViewer != null)
            {
                if (canScroll)
                {
                    // Move left or right based on Delta
                    if (e.Delta > 0) scrollViewer.LineLeft();
                    else scrollViewer.LineRight();
                }
                else
                {
                    var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                    {
                        RoutedEvent = UIElement.MouseWheelEvent,
                        Source = sender
                    };

                    var parent = ((Control)sender).Parent as UIElement;
                    parent?.RaiseEvent(eventArg);
                }

                e.Handled = true;
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found) return found;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
