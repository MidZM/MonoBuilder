using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MonoBuilder.Screens.ScreenUtils
{
    /// <summary>Represents a customizable selection box for displaying messages, titles, icons, and button configurations.</summary>
    public partial class SelectionBox : Window
    {
        private static readonly int MaxDialogWidth = 600;
        private static readonly int MinDialogWidth = 200;
        private bool _isInternalClose = false;
        public SelectionBoxChoice? _result = null;

        /// <summary>Converts a System.Drawing.Bitmap to a WPF ImageSource.</summary>
        private static ImageSource ToImageSource(Bitmap bitmap)
        {
            var handle = bitmap.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(
                    handle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                NativeMethods.DeleteObject(handle);
            }
        }

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
        }

        /// <summary>Contains the default icon images for error, warning, question, and information messages.</summary>
        private static readonly ImageSource[] DefaultIcons = [
                ToImageSource(SystemIcons.Error.ToBitmap()),
                ToImageSource(SystemIcons.Warning.ToBitmap()),
                ToImageSource(SystemIcons.Question.ToBitmap()),
                ToImageSource(SystemIcons.Information.ToBitmap())
            ];

        public SelectionBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the dialog box's message text.
        /// </summary>
        /// <param name="diag">The dialog box to updated.</param>
        /// <param name="message">The message text to display.</param>
        private static void SetMessage(SelectionBox diag, string message)
        {
            diag.TextContent.Text = message;
        }

        /// <summary>
        /// Sets the dialog box's title and message text.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        /// <param name="message">The message text to display.</param>
        /// <param name="title">The title to display.</param>
        private static void SetMessage(SelectionBox diag, string message, string title)
        {
            diag.Title = title;
            diag.TextContent.Text = message;
        }

        private static void HideMessage(SelectionBox diag)
        {
            diag.TextContent.Visibility = Visibility.Collapsed;
            Grid.SetRow(diag.SelectionOptions, 0);
            Grid.SetRowSpan(diag.SelectionOptions, 2);
        }

        /// <summary>
        /// Hides the icon in the dialog box by setting its visibility to collapsed.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        private static void HideIconPanel(SelectionBox diag)
        {
            diag.TypeImage.Visibility = Visibility.Collapsed;
            diag.ContentContainer.ColumnDefinitions[0].Width = new GridLength(0);
        }

        private static void SetSelectionOptions(SelectionBox diag, IEnumerable<string> options)
        {
            diag.SelectionOptions.ItemsSource = options;
        }

        private static void WireSelectionChangedEvent(SelectionBox diag, object? tag = null)
        {
            diag.SelectionOptions.SelectionChanged += (s, e) =>
            {
                if (diag.SelectionOptions.SelectedItem is string selectedOption)
                {
                    int selectedIndex = diag.SelectionOptions.SelectedIndex;
                    diag._result = new SelectionBoxChoice(selectedOption, selectedIndex, tag);
                }
            };
        }

        /// <summary>
        /// Resizes the dialog window to fit its content.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        private static void AutoSizeWindow(SelectionBox diag)
        {
            diag.SizeToContent = SizeToContent.WidthAndHeight;
            diag.MinWidth = MinDialogWidth;
            diag.MaxWidth = MaxDialogWidth;
        }

        /// <summary>
        /// Sets the owner and start position of the specified dialog box based on the currently active window.
        /// </summary>
        /// <param name="diag">The dialog box whose owner and start position are to be set.</param>
        /// <returns>The window assigned as the owner, or null if no suitable owner is found.</returns>
        private static Window? SetDialogOwner(SelectionBox diag)
        {
            Window? owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (owner != null)
            {
                diag.Owner = owner;
                diag.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                diag.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            return owner;
        }

        /// <summary>
        /// Displays a selection dialog box that presents a list of selectable options to the user.
        /// </summary>
        /// <remarks>The dialog box is modal and blocks interaction with other windows until the user
        /// makes a selection or closes the dialog. The order of options in the collection determines their display
        /// order.</remarks>
        /// <param name="content">A collection of strings representing the selectable options presented to the user. Cannot be null.</param>
        /// <param name="tag">An optional tag object to associate with the selection.</param>
        /// <returns>A DialogBoxResult value indicating the user's selection or action in the dialog box.</returns>
        public static SelectionBoxChoice? Show(IEnumerable<string> content, object? tag = null)
        {
            var diag = new SelectionBox();

            HideMessage(diag);
            HideIconPanel(diag);
            SetSelectionOptions(diag, content);
            AutoSizeWindow(diag);
            WireSelectionChangedEvent(diag, tag);

            SetDialogOwner(diag);
            diag.ShowDialog();
            return diag._result;
        }

        /// <summary>
        /// Displays a selection dialog box with a specified message and a list of selectable options.
        /// </summary>
        /// <remarks>The dialog box is modal and blocks input to other windows until the user makes a
        /// selection or closes the dialog. The order of options in the collection determines their display
        /// order.</remarks>
        /// <param name="message">The message to display above the ComboBox. This provides context or instructions to the user.</param>
        /// <param name="content">A collection of strings representing the selectable options presented to the user. Cannot be null.</param>
        /// <param name="tag">An optional tag object to associate with the selection.</param>
        /// <returns>A DialogBoxResult value indicating the user's selection or the result of the dialog box interaction.</returns>
        public static SelectionBoxChoice? Show(string message, IEnumerable<string> content, object? tag = null)
        {
            var diag = new SelectionBox();

            SetMessage(diag, message);
            HideIconPanel(diag);
            SetSelectionOptions(diag, content);
            AutoSizeWindow(diag);
            WireSelectionChangedEvent(diag, tag);

            SetDialogOwner(diag);
            diag.ShowDialog();
            return diag._result;
        }

        /// <summary>
        /// Displays a selection dialog box with a specified message, title, and a list of selectable options.
        /// </summary>
        /// <remarks>The dialog box is modal and blocks input to other windows until the user makes a
        /// selection or closes the dialog. The order of options in the collection determines their display
        /// order.</remarks>
        /// <param name="message">The message to display above the ComboBox. Provides context or instructions to the user.</param>
        /// <param name="title">The title of the dialog box. Assists the user in determining the purpose at a glance if it were to appear outside of the window area.</param>
        /// <param name="content">A collection of strings representing the selectable options presented to the user. Cannot be null.</param>
        /// <param name="tag">An optional tag object to associate with the selection.</param>
        /// <returns>A DialogBoxResult value indicating the user's selection or the result of the dialog box interaction.</returns>
        public static SelectionBoxChoice? Show(string message, string title, IEnumerable<string> content, object? tag = null)
        {
            var diag = new SelectionBox();

            SetMessage(diag, message, title);
            HideIconPanel(diag);
            SetSelectionOptions(diag, content);
            AutoSizeWindow(diag);
            WireSelectionChangedEvent(diag, tag);

            SetDialogOwner(diag);
            diag.ShowDialog();
            return diag._result;
        }

        /// <summary>
        /// Displays a selection dialog box with a specified message, title, and a list of selectable options.
        /// </summary>
        /// <remarks>The dialog box is modal and blocks input to other windows until the user makes a
        /// selection or closes the dialog. The order of options in the collection determines their display
        /// order.</remarks>
        /// <param name="message">The message to display above the ComboBox. Provides context or instructions to the user.</param>
        /// <param name="title">The title of the dialog box. Assists the user in determining the purpose at a glance if it were to appear outside of the window area.</param>
        /// <param name="icon">The icon to display in the dialog box, which visually indicates the type or purpose of the message.</param>
        /// <param name="content">A collection of strings representing the selectable options presented to the user. Cannot be null.</param>
        /// <param name="tag">An optional tag object to associate with the selection.</param>
        /// <returns>A DialogBoxResult value indicating the user's selection or the result of the dialog box interaction.</returns>
        public static SelectionBoxChoice? Show(string message, string title, DialogIcon icon, IEnumerable<string> content, object? tag = null)
        {
            var diag = new SelectionBox();

            SetMessage(diag, message, title);
            SetSelectionOptions(diag, content);
            AutoSizeWindow(diag);
            WireSelectionChangedEvent(diag, tag);

            diag.TypeImage.Source = DefaultIcons[(int)icon];

            SetDialogOwner(diag);
            diag.ShowDialog();
            return diag._result;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            _isInternalClose = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isInternalClose)
            {
                _result = null;
            }
        }
    }

    public class SelectionBoxChoice
    {
        public string SelectionText { get; set; }
        public int SelectionIndex { get; set; }
        public object? SelectionTag { get; set; }

        public SelectionBoxChoice(string text, int index, object? tag = null)
        {
            SelectionText = text;
            SelectionIndex = index;
            SelectionTag = tag;
        }
    }
}
