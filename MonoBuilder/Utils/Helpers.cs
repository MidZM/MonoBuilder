using MonoBuilder.Utils.character_management;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MonoBuilder.Utils
{
    public static class Helpers
    {
        /// <summary>
        /// Finds a child element by name in the visual tree of a WPF element.
        /// </summary>
        /// <typeparam name="T">The type of element to find.</typeparam>
        /// <param name="parent">The parent element to search within.</param>
        /// <param name="name">The name of the element to find.</param>
        /// <returns>The found element, or null if not found.</returns>
        public static T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild && typedChild.Name == name)
                {
                    return typedChild;
                }

                var result = FindVisualChild<T>(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a child element by name in the logical tree of a WPF element.
        /// </summary>
        /// <typeparam name="T">The type of element to find.</typeparam>
        /// <param name="parent">The parent element to search within.</param>
        /// <param name="name">The name of the element to find.</param>
        /// <returns>The found element, or null if not found.</returns>
        public static T? FindLogicalChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;

            foreach (var child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is T typedChild && typedChild.Name == name)
                {
                    return typedChild;
                }

                if (child is DependencyObject depChild)
                {
                    var result = FindLogicalChild<T>(depChild, name);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        public static T? FirstOfVisualType<T>(DependencyObject parent, bool deepSearch = false) where T : FrameworkElement
        {
            if (parent == null) return null;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                {
                    return typedChild;
                }

                if (deepSearch)
                {
                    var result = FirstOfVisualType<T>(child);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public static T? FirstOfLogicalType<T>(DependencyObject parent, bool deepSearch = false) where T : FrameworkElement
        {
            if (parent == null) return null;
            foreach (var child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is T typedChild)
                {
                    return typedChild;
                }
                if (deepSearch && child is DependencyObject depChild)
                {
                    var result = FirstOfLogicalType<T>(depChild);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public static ScrollViewer? GetScrollViewer(UIElement? element)
        {
            if (element == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i) as UIElement;
                if (child is ScrollViewer sv) return sv;
                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }

        public static bool ElementCanScroll(ScrollViewer? scrollViewer, int delta, bool isVertical)
        {
            if (scrollViewer == null) return false;

            var scrollIsVisible =
                scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible ||
                scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible;
            var canScroll = scrollIsVisible && (
                (
                    isVertical &&
                    (
                        (delta < 0 && scrollViewer.VerticalOffset < scrollViewer.ScrollableHeight) ||
                        (delta > 0 && scrollViewer.VerticalOffset > 0)
                    )
                ) ||
                (
                    !isVertical &&
                    (
                        (delta < 0 && scrollViewer.HorizontalOffset < scrollViewer.ScrollableWidth) ||
                        (delta > 0 && scrollViewer.HorizontalOffset > 0)
                    )
                )
            );

            return canScroll;
        }

        public static bool RichHasContent(RichTextBox richTextBox)
        {
            if (richTextBox == null || richTextBox.Document == null) return false;

            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);

            return !string.IsNullOrWhiteSpace(textRange.Text);
        }

        public static System.Windows.Media.Brush HexToBrush(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return System.Windows.Media.Brushes.Transparent;

            try
            {
                return (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
            }
            catch
            {
                return System.Windows.Media.Brushes.Transparent;
            }
        }

        public static bool HexValueIsDark(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return false;

            try
            {
                System.Drawing.Color color = ColorTranslator.FromHtml(hex);
                double luminance = GetRelativeLuminance(color);
                return luminance <= 0.179;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static double GetRelativeLuminance(System.Drawing.Color c)
        {
            double r = c.R / 255.0;
            double b = c.B / 255.0;
            double g = c.G / 255.0;

            r = (r <= 0.03928) ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
            b = (b <= 0.03928) ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);
            g = (g <= 0.03928) ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);

            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        public static bool CharacterIsSynced(Character character)
        {
            return character != null && character.IsSynced;
        }

        #region Custom RichTextBox Commands
        public static readonly RoutedUICommand ToggleBold = new RoutedUICommand(
            "Toggle Bold", "ToggleBold", typeof(Helpers),
            new InputGestureCollection { new KeyGesture(Key.B, ModifierKeys.Control) });
        public static readonly RoutedUICommand ToggleItalic = new RoutedUICommand(
            "Toggle Italic", "ToggleItalic", typeof(Helpers),
            new InputGestureCollection { new KeyGesture(Key.I, ModifierKeys.Control) });
        public static readonly RoutedUICommand ToggleBig = new RoutedUICommand(
            "Toggle Big Text", "ToggleBig", typeof(Helpers),
            new InputGestureCollection { new KeyGesture(Key.B, ModifierKeys.Alt) });
        public static readonly RoutedUICommand ToggleSmall = new RoutedUICommand(
            "Toggle Small Text", "ToggleSmall", typeof(Helpers),
            new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Alt) } );
        public static readonly RoutedUICommand ConvertScript = new RoutedUICommand(
            "Convert Script Data", "ConvertScript", typeof(Helpers),
            new InputGestureCollection { new KeyGesture(Key.C, ModifierKeys.Alt) });
        public static readonly RoutedUICommand LoadLabel = new RoutedUICommand(
            "Load Label Data", "LoadLabel", typeof(Helpers),
            new InputGestureCollection { new KeyGesture(Key.O, ModifierKeys.Control) });
        public static readonly RoutedUICommand SaveLabel = new RoutedUICommand(
            "Save Label Data", "SaveLabel", typeof(Helpers),
            new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) });
        public static readonly RoutedUICommand ResetInputLabel = new RoutedUICommand(
            "Reset Script & Label", "ResetInputLabel", typeof(Helpers),
            new InputGestureCollection { new KeyGesture(Key.R, ModifierKeys.Alt) });
        public static readonly RoutedUICommand IndentForward = new RoutedUICommand(
            "Increase Indent", "IndentForward", typeof(Helpers),
            new InputGestureCollection { new KeyGesture(Key.OemCloseBrackets, ModifierKeys.Control) });
        public static readonly RoutedUICommand IndentBackward = new RoutedUICommand(
            "Decrease Indent", "IndentBackward", typeof(Helpers),
            new InputGestureCollection { new KeyGesture(Key.OemOpenBrackets, ModifierKeys.Control) });
        #endregion
    }

    public class ObservableBoolean : BaseViewModel
    {
        public ObservableBoolean(bool value = false)
        {
            Value = value;
        }

        private bool _value;
        public bool Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }

    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string hex)
            {
                return Helpers.HexToBrush(hex);
            }
            return System.Windows.Media.Brushes.Transparent;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ShouldLightenText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string hex && Helpers.HexValueIsDark(hex))
            {
                return System.Windows.Media.Brushes.White;
            }
            return System.Windows.Media.Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorSyncedCells : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Character character && character.IsSynced)
            {
                return System.Windows.Media.Brushes.LightGreen;
            }
            return System.Windows.Media.Brushes.MistyRose;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double width && values[1] is double height)
            {
                return new Rect(0, 0, width, height);
            }
            return new Rect(0, 0, 0, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

