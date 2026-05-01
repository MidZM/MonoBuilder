using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
    /// <summary>Represents a customizable dialog box for displaying messages, titles, icons, and button configurations.</summary>
    public partial class DialogBox : Window
    {
        private static readonly int MaxDialogWidth = 600;
        private static readonly int MinDialogWidth = 200;
        public DialogBoxResult _result = DialogBoxResult.None;

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
        /// <summary>Provides default button configurations for common dialog scenarios.</summary>
        private static readonly (string, DialogBoxResult)[][] DefaultButtons = [
                [
                    ("OK", DialogBoxResult.OK)
                ],
                [
                    ("OK", DialogBoxResult.OK),
                    ("Cancel", DialogBoxResult.Cancel)
                ],
                [
                    ("Retry", DialogBoxResult.Retry),
                    ("Abort", DialogBoxResult.Abort),
                    ("Ignore", DialogBoxResult.Ignore)
                ],
                [
                    ("Yes", DialogBoxResult.Yes),
                    ("No", DialogBoxResult.No),
                    ("Cancel", DialogBoxResult.Cancel)
                ],
                [
                    ("Yes", DialogBoxResult.Yes),
                    ("No", DialogBoxResult.No)
                ],
                [
                    ("Retry", DialogBoxResult.Retry),
                    ("Cancel", DialogBoxResult.Cancel)
                ],
                [
                    ("Continue", DialogBoxResult.Continue),
                    ("Try Again", DialogBoxResult.TryAgain),
                    ("Cancel", DialogBoxResult.Cancel)
                ]
            ];

        /// <summary>Contains the default icon images for error, warning, question, and information messages.</summary>
        private static readonly ImageSource[] DefaultIcons = [
                ToImageSource(SystemIcons.Error.ToBitmap()),
                ToImageSource(SystemIcons.Warning.ToBitmap()),
                ToImageSource(SystemIcons.Question.ToBitmap()),
                ToImageSource(SystemIcons.Information.ToBitmap())
            ];

        public DialogBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates an array of DialogButton instances based on the specified configuration.
        /// </summary>
        /// <param name="configuration">An array of tuples containing button text and dialog result.</param>
        /// <returns>An array of DialogButton objects.</returns>
        private static DialogButton[] BuildButtons((string, DialogBoxResult)[] configuration)
        {
            DialogButton[] buttons = configuration
                .Select(tuple => new DialogButton(tuple.Item1, tuple.Item2))
                .ToArray();

            return buttons;
        }

        /// <summary>
        /// Sets the dialog box's message text.
        /// </summary>
        /// <param name="diag">The dialog box to updated.</param>
        /// <param name="message">The message text to display.</param>
        private static void SetMessage(DialogBox diag, string message)
        {
            diag.TextContent.Text = message;
        }

        /// <summary>
        /// Sets the dialog box's title and message text.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        /// <param name="message">The message text to display.</param>
        /// <param name="title">The title to display.</param>
        private static void SetMessage(DialogBox diag, string message, string title)
        {
            diag.Title = title;
            diag.TextContent.Text = message;
        }

        /// <summary>
        /// Hides the icon in the dialog box by setting its visibility to collapsed.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        private static void HideIconPanel(DialogBox diag)
        {
            diag.TypeImage.Visibility = Visibility.Collapsed;
            diag.ContentContainer.ColumnDefinitions[0].Width = new GridLength(0);
        }

        /// <summary>
        /// Configures the button panel of the dialog box by clearing its controls.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        /// <param name="amount">The number of buttons to prepare for.</param>
        private static void SetPanelControlSize(DialogBox diag, int amount)
        {
            diag.ButtonContainer.Children.Clear();
            for (int i = 0; i < amount; i++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(1, GridUnitType.Star);
                diag.ButtonContainer.ColumnDefinitions.Add(col);
            }
        }

        /// <summary>
        /// Adds a button to the dialog box's button panel.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        /// <param name="element">The button to add.</param>
        private static void SetButtonControl(DialogBox diag, Button element)
        {
            diag.ButtonContainer.Children.Add(element);
        }

        /// <summary>
        /// Adds a button to the dialog box's button panel.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        /// <param name="index">The index (not used in WPF DockPanel, kept for API compatibility).</param>
        /// <param name="numberOfElements">The total number of elements in the button panel.</param>
        /// <param name="element">The button to add.</param>
        private static void SetButtonControl(DialogBox diag, int index, int numberOfElements, Button element)
        {
            Grid.SetColumn(element, index);
            diag.ButtonContainer.Children.Add(element);
        }

        /// <summary>
        /// Checks and sets if the dialog box can be closed via the close button.
        /// </summary>
        /// <param name="diag">The dialog box instance to modify.</param>
        /// <param name="close">Indicates whether the dialog box should be prevented from closing.</param>
        private static void ShouldDisableCloseBox(DialogBox diag, bool close)
        {
            if (close)
            {
                diag.Closing += (s, e) =>
                {
                    if (diag._result == DialogBoxResult.None)
                    {
                        e.Cancel = true;
                    }
                };
            }
        }

        /// <summary>
        /// Resizes the dialog window to fit its content.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        private static void AutoSizeWindow(DialogBox diag)
        {
            diag.SizeToContent = SizeToContent.WidthAndHeight;
            diag.MinWidth = MinDialogWidth;
            diag.MaxWidth = MaxDialogWidth;
        }

        /// <summary>
        /// Resizes the dialog window to fit its content within the specified width constraints.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        /// <param name="width">The minimum width to apply when resizing the dialog.</param>
        private static void AutoSizeWindow(DialogBox diag, int width)
        {
            diag.SizeToContent = SizeToContent.WidthAndHeight;
            diag.MinWidth = Math.Max(width, MinDialogWidth);
            diag.MaxWidth = MaxDialogWidth;
        }

        /// <summary>
        /// Sets the owner and start position of the specified dialog box based on the currently active window.
        /// </summary>
        /// <param name="diag">The dialog box whose owner and start position are to be set.</param>
        /// <returns>The window assigned as the owner, or null if no suitable owner is found.</returns>
        private static Window? SetDialogOwner(DialogBox diag)
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
        /// Displays a dialog box with the specified message and an OK button.
        /// </summary>
        /// <param name="message">The message to display in the dialog box.</param>
        /// <returns>The dialog result indicating how the dialog was closed.</returns>
        public static DialogBoxResult Show(string message)
        {
            var diag = new DialogBox();
            Button OKButton = new DialogButton("OK", DialogBoxResult.OK).Btn;

            SetMessage(diag, message);
            HideIconPanel(diag);
            SetPanelControlSize(diag, 0);
            SetButtonControl(diag, OKButton);
            AutoSizeWindow(diag);

            SetDialogOwner(diag);
            diag.ShowDialog();
            return diag._result;
        }

        /// <summary>
        /// Displays a dialog box with a specified message and title with an OK button.
        /// </summary>
        /// <param name="message">The message to display in the dialog box.</param>
        /// <param name="title">The title of the dialog box.</param>
        /// <returns>A DialogResult value indicating the dialog was closed.</returns>
        public static DialogBoxResult Show(string message, string title)
        {
            var diag = new DialogBox();
            Button OKButton = new DialogButton("OK", DialogBoxResult.OK).Btn;

            SetMessage(diag, message, title);
            HideIconPanel(diag);
            SetPanelControlSize(diag, 0);
            SetButtonControl(diag, OKButton);
            AutoSizeWindow(diag);

            SetDialogOwner(diag);
            diag.ShowDialog();
            return diag._result;
        }

        /// <summary>
        /// Displays a dialog box with a specified message, title, and button configuration.
        /// </summary>
        /// <param name="message">The text to display in the dialog box.</param>
        /// <param name="title">The title of the dialog box.</param>
        /// <param name="buttons">The default button configuration for the dialog box.</param>
        /// <returns>A DialogResult value indicating which button was clicked by the user.</returns>
        public static DialogBoxResult Show(string message, string title, DialogButtonDefaults buttons)
        {
            var diag = new DialogBox();
            DialogButton[] diagButtons = BuildButtons(DefaultButtons[(int)buttons]);

            SetMessage(diag, message, title);
            HideIconPanel(diag);
            SetPanelControlSize(diag, diagButtons.Length);
            ShouldDisableCloseBox(diag, buttons != DialogButtonDefaults.OK);
            AutoSizeWindow(diag);

            for (int i = 0; i < diagButtons.Length; i++)
            {
                SetButtonControl(diag, i, diagButtons.Length, diagButtons[i].Btn);
            }

            SetDialogOwner(diag);
            diag.ShowDialog();
            return diag._result;
        }

        /// <summary>
        /// Displays a dialog box with a specified message, title, and custom buttons.
        /// </summary>
        /// <param name="message">The text to display in the dialog box.</param>
        /// <param name="title">The title of the dialog box.</param>
        /// <param name="buttons">The buttons to display in the dialog box.</param>
        /// <returns>A DialogResult value indicating which button was clicked by the user.</returns>
        public static DialogBoxResult Show(string message, string title, params DialogButton[] buttons)
        {
            var diag = new DialogBox();
            SetMessage(diag, message, title);
            HideIconPanel(diag);
            SetPanelControlSize(diag, buttons.Length);
            ShouldDisableCloseBox(diag, buttons.Length > 1);
            AutoSizeWindow(diag);

            for (int i = 0; i < buttons.Length; i++)
            {
                SetButtonControl(diag, i, buttons.Length, buttons[i].Btn);
            }

            SetDialogOwner(diag);
            diag.ShowDialog();
            return diag._result;
        }

        /// <summary>
        /// Displays a dialog box with a specified message, title, button configuration, and icon.
        /// </summary>
        /// <param name="message">The text to display in the dialog box.</param>
        /// <param name="title">The title of the dialog box.</param>
        /// <param name="buttons">The button configuration for the dialog box.</param>
        /// <param name="icon">The icon to display in the dialog box.</param>
        /// <returns>A DialogResult value indicating which button was clicked by the user.</returns>
        public static DialogBoxResult Show(string message, string title, DialogButtonDefaults buttons, DialogIcon icon)
        {
            var diag = new DialogBox();
            DialogButton[] diagButtons = BuildButtons(DefaultButtons[(int)buttons]);

            SetMessage(diag, message, title);
            SetPanelControlSize(diag, diagButtons.Length);
            ShouldDisableCloseBox(diag, buttons != DialogButtonDefaults.OK);
            AutoSizeWindow(diag);

            for (int i = 0; i < diagButtons.Length; i++)
            {
                SetButtonControl(diag, i, diagButtons.Length, diagButtons[i].Btn);
            }

            diag.TypeImage.Source = DefaultIcons[(int)icon];

            SetDialogOwner(diag);
            diag.ShowDialog();
            return diag._result;
        }

        /// <summary>
        /// Displays a dialog box with a specified message, title, icon, and custom buttons.
        /// </summary>
        /// <param name="message">The text to display in the dialog box.</param>
        /// <param name="title">The title of the dialog box.</param>
        /// <param name="icon">The icon to display in the dialog box.</param>
        /// <param name="buttons">The buttons to display in the dialog box.</param>
        /// <returns>A DialogResult value indicating which button was clicked by the user.</returns>
        public static DialogBoxResult Show(string message, string title, DialogIcon icon, params DialogButton[] buttons)
        {
            var diag = new DialogBox();
            SetMessage(diag, message, title);
            SetPanelControlSize(diag, buttons.Length);
            ShouldDisableCloseBox(diag, buttons.Length > 1);
            AutoSizeWindow(diag);

            for (int i = 0; i < buttons.Length; i++)
            {
                SetButtonControl(diag, i, buttons.Length, buttons[i].Btn);
            }

            diag.TypeImage.Source = DefaultIcons[(int)icon];

            SetDialogOwner(diag);
            diag.ShowDialog();
            return diag._result;
        }

        /// <summary>
        /// Displays a dialog box with a specified message, title, width, button configuration, and icon.
        /// </summary>
        /// <param name="message">The text to display in the dialog box.</param>
        /// <param name="title">The title of the dialog box.</param>
        /// <param name="width">The width of the dialog box in pixels.</param>
        /// <param name="buttons">The button configuration for the dialog box.</param>
        /// <param name="icon">The icon to display in the dialog box.</param>
        /// <returns>A DialogResult value indicating which button was clicked by the user.</returns>
        public static DialogBoxResult Show(string message, string title, int width, DialogButtonDefaults buttons, DialogIcon icon)
        {
            var diag = new DialogBox();
            DialogButton[] diagButtons = BuildButtons(DefaultButtons[(int)buttons]);

            SetMessage(diag, message, title);
            SetPanelControlSize(diag, diagButtons.Length);
            ShouldDisableCloseBox(diag, buttons != DialogButtonDefaults.OK);
            AutoSizeWindow(diag, width);

            for (int i = 0; i < diagButtons.Length; i++)
            {
                SetButtonControl(diag, i, diagButtons.Length, diagButtons[i].Btn);
            }

            diag.TypeImage.Source = DefaultIcons[(int)icon];

            SetDialogOwner(diag);
            diag.ShowDialog();
            return diag._result;
        }

        /// <summary>
        /// Displays a dialog box with a specified message, title, width, icon, and buttons.
        /// </summary>
        /// <param name="message">The text to display in the dialog box.</param>
        /// <param name="title">The title of the dialog box.</param>
        /// <param name="width">The width of the dialog box in pixels.</param>
        /// <param name="icon">The icon to display in the dialog box.</param>
        /// <param name="buttons">The buttons to include in the dialog box.</param>
        /// <returns>A DialogResult value indicating which button was clicked by the user.</returns>
        public static DialogBoxResult Show(string message, string title, int width, DialogIcon icon, params DialogButton[] buttons)
        {
            var diag = new DialogBox();
            SetMessage(diag, message, title);
            SetPanelControlSize(diag, buttons.Length);
            ShouldDisableCloseBox(diag, buttons.Length > 1);
            AutoSizeWindow(diag, width);

            for (int i = 0; i < buttons.Length; i++)
            {
                SetButtonControl(diag, i, buttons.Length, buttons[i].Btn);
            }

            diag.TypeImage.Source = DefaultIcons[(int)icon];

            SetDialogOwner(diag);
            diag.ShowDialog();
            return diag._result;
        }
    }

    /// <summary>
    /// Represents a button used in dialog windows, encapsulating text, result, and the underlying Button control.
    /// </summary>
    public class DialogButton
    {
        /// <summary>Gets or sets the text value.</summary>
        public string Text { get; set; }
        /// <summary>Gets or sets the result of the dialog.</summary>
        public DialogBoxResult Result { get; set; }
        /// <summary>Gets or sets the Button instance associated with this property.</summary>
        public Button Btn { get; set; } = new Button();

        public DialogButton(string text, DialogBoxResult result, string style = "DefaultButton")
        {
            Text = text;
            Result = result;

            Btn.Content = Text;
            Btn.Margin = new Thickness(5);
            Btn.Padding = new Thickness(10, 5, 10, 5);
            Btn.Style = (Style)Btn.FindResource(style) ?? (Style)Btn.FindResource("DefaultButton");
            Btn.MinWidth = 75;
            Btn.Cursor = Cursors.Hand;
            Btn.Click += (s, e) =>
            {
                if (Window.GetWindow(Btn) is DialogBox parent)
                {
                    parent._result = result;
                    parent.Close();
                }
            };
        }
    }

    /// <summary>Specify default dialog button values.</summary>
    public enum DialogButtonDefaults
    {
        /// <summary>The dialog box is given an OK button.</summary>
        OK,
        /// <summary>The dialog box is given an OK and Cancel button.</summary>
        OKCancel,
        /// <summary>The dialog box is given an Abort, Retry, and Ignore button.</summary>
        AbortRetryIgnore,
        /// <summary>The dialog box is given a Yes, No, and Cancel button.</summary>
        YesNoCancel,
        /// <summary>The dialog box is given a Yes and No button.</summary>
        YesNo,
        /// <summary>The dialog box is given a Retry and Cancel button.</summary>
        RetryCancel,
        /// <summary>The dialog box is given a Try Again, Continue, and Cancel button.</summary>
        CancelTryContinue
    }

    /// <summary>Specify default dialog icon values.</summary>
    public enum DialogIcon
    {
        /// <summary>An image of a white X in the middle of a red circle.</summary>
        Error,
        /// <summary>An image of an exclamation mark in the middle of a yellow triangle.</summary>
        Warning,
        /// <summary>An image of a question mark in the middle of a blue circle.</summary>
        Question,
        /// <summary>An image of a lower case i in the middle of a blue circle.</summary>
        Information
    }

    /// <summary>Specifies identifiers to indicate the return value of a dialog box.</summary>
    public enum DialogBoxResult
    {
        /// <summary>Nothing is returned from the dialog box.</summary>
        None = 0,
        /// <summary>The dialog box return value is OK (usually sent from a button labeled OK).</summary>
        OK = 1,
        /// <summary>The dialog box return value is Cancel (usually sent from a button labeled Cancel).</summary>
        Cancel = 2,
        /// <summary>The dialog box return value is Abort (usually sent from a button labeled Abort).</summary>
        Abort = 3,
        /// <summary>The dialog box return value is Retry (usually sent from a button labeled Retry).</summary>
        Retry = 4,
        /// <summary>The dialog box return value is Ignore (usually sent from a button labeled Ignore).</summary>
        Ignore = 5,
        /// <summary>The dialog box return value is Yes (usually sent from a button labeled Yes).</summary>
        Yes = 6,
        /// <summary>The dialog box return value is No (usually sent from a button labeled No).</summary>
        No = 7,
        /// <summary>The dialog box return value is Try Again (usually sent from a button labeled Try Again).</summary>
        TryAgain = 10,
        /// <summary>The dialog box return value is Continue (usually sent from a button labeled Continue).</summary>
        Continue = 11
    }
}
