using Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonoBuilder.Screens.ScreenUtils
{
    /// <summary>Represents a customizable dialog box for displaying messages, titles, icons, and button configurations.</summary>
    public partial class DialogBox : KryptonForm
    {
        /// <summary>Provides default button configurations for common dialog scenarios.</summary>
        private static readonly (string, DialogResult, ButtonStyle)[][] DefaultButtons = [
                [
                    ("OK", DialogResult.OK, ButtonStyle.Alternate)
                ],
                [
                    ("OK", DialogResult.OK, ButtonStyle.Alternate),
                    ("Cancel", DialogResult.Cancel, ButtonStyle.Custom2)
                ],
                [
                    ("Retry", DialogResult.Retry, ButtonStyle.Alternate),
                    ("Abort", DialogResult.Abort, ButtonStyle.Custom2),
                    ("Ignore", DialogResult.Ignore, ButtonStyle.Custom2)
                ],
                [
                    ("Yes", DialogResult.Yes, ButtonStyle.Alternate),
                    ("No", DialogResult.No, ButtonStyle.Alternate),
                    ("Cancel", DialogResult.Cancel, ButtonStyle.Custom2)
                ],
                [
                    ("Yes", DialogResult.Yes, ButtonStyle.Alternate),
                    ("No", DialogResult.No, ButtonStyle.Alternate)
                ],
                [
                    ("Retry", DialogResult.Retry, ButtonStyle.Alternate),
                    ("Cancel", DialogResult.Cancel, ButtonStyle.Custom2)
                ],
                [
                    ("Continue", DialogResult.Continue, ButtonStyle.Alternate),
                    ("Try Again", DialogResult.TryAgain, ButtonStyle.Alternate),
                    ("Cancel", DialogResult.Cancel, ButtonStyle.Custom2)
                ]
            ];

        /// <summary>Contains the default icon images for error, warning, question, and information messages.</summary>
        private static readonly Image[] DefaultIcons = [
                SystemIcons.Error.ToBitmap(),
                SystemIcons.Warning.ToBitmap(),
                SystemIcons.Question.ToBitmap(),
                SystemIcons.Information.ToBitmap()
            ];

        private static readonly int MaxWidth = 600;
        private static readonly int MinWidth = 200;

        public DialogBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates an array of DialogButton instances based on the specified configuration.
        /// </summary>
        /// <param name="configuration">An array of tuples containing button text, dialog result, and button style.</param>
        /// <returns>An array of DialogButton objects.</returns>
        private static DialogButton[] BuildButtons((string, DialogResult, ButtonStyle)[] configuration)
        {
            DialogButton[] buttons = configuration
                .Select(tuple => new DialogButton(tuple.Item1, tuple.Item2, tuple.Item3))
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
            diag.DialogLabel.Text = message;
        }

        /// <summary>
        /// Sets the dialog box's title and message text.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        /// <param name="message">The message text to display.</param>
        /// <param name="title">The title to display.</param>
        private static void SetMessage(DialogBox diag, string message, string title)
        {
            diag.Text = title;
            diag.DialogLabel.Text = message;
        }

        /// <summary>
        /// Hides the icon panel dialog box by setting its width to zero and making its controls
        /// invisible.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        private static void HideIconPanel(DialogBox diag)
        {
            diag.DialogPanel.ColumnStyles[0].Width = 0;
            diag.DialogPanel.ColumnStyles[0].SizeType = SizeType.Absolute;

            for (int i = 0; i < diag.DialogPanel.RowCount; i++)
            {
                var control = diag.DialogPanel.GetControlFromPosition(0, i);
                if (control != null)
                {
                    control.Visible = false;
                }
            }
        }

        /// <summary>
        /// Configures the button panel of the dialog box by clearing its controls and setting the column count.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        /// <param name="amount">The number of columns to set.</param>
        private static void SetPanelControlSize(DialogBox diag, int amount)
        {
            diag.DialogButtonPanel.Controls.Clear();
            diag.DialogButtonPanel.ColumnCount = amount;
        }

        /// <summary>
        /// Adds a control to the dialog box's button panel and sets its column style to occupy 100 percent width.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        /// <param name="element">The control to add.</param>
        private static void SetButtonControl(DialogBox diag, Control element)
        {
            diag.DialogButtonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            diag.DialogButtonPanel.Controls.Add(element);
        }

        /// <summary>
        /// Adds a control to the dialog box's button panel at the specified column index and adjusts column styles based on
        /// the total number of elements.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        /// <param name="index">The column index.</param>
        /// <param name="numberOfElements">The total number of elements in the button panel.</param>
        /// <param name="element">The control to add.</param>
        private static void SetButtonControl(DialogBox diag, int index, int numberOfElements, Control element)
        {
            diag.DialogButtonPanel.ColumnStyles.Insert(index, new ColumnStyle(SizeType.Percent, 100f / numberOfElements));
            diag.DialogButtonPanel.Controls.Add(element, index, 0);
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
                diag.CloseBox = false;
            }
        }

        /// <summary>
        /// Resizes the dialog window to fit its label, icon, and button panel with appropriate padding and margins.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        private static void AutoSizeWindow(DialogBox diag)
        {
            Size preferredSize = diag.DialogLabel.GetPreferredSize(new Size(MaxWidth, 0));

            int padding = 40;
            int horizontalMargin = diag.Width - diag.ClientSize.Width;
            int verticalMargin = diag.Height - diag.ClientSize.Height;

            int finalWidth = Math.Max(preferredSize.Width + padding + diag.DialogIcon.Width, MinWidth);
            int finalHeight = preferredSize.Height + diag.DialogButtonPanel.Height + padding + verticalMargin;

            diag.Size = new Size(finalWidth, finalHeight);
        }

        /// <summary>
        /// Resizes the dialog window to fit its content within the specified width constraints.
        /// </summary>
        /// <param name="diag">The dialog box to update.</param>
        /// <param name="width">The minimum width to apply when resizing the dialog.</param>
        private static void AutoSizeWindow(DialogBox diag, int width)
        {
            Size preferredSize = diag.DialogLabel.GetPreferredSize(new Size(MaxWidth, 0));

            int padding = 40;
            int horizontalMargin = diag.Width - diag.ClientSize.Width;
            int verticalMargin = diag.Height - diag.ClientSize.Height;

            int finalWidth = Math.Clamp(preferredSize.Width + padding + diag.DialogIcon.Width, width, MaxWidth);
            int finalHeight = preferredSize.Height + diag.DialogButtonPanel.Height + padding + verticalMargin;

            diag.Size = new Size(finalWidth, finalHeight);
        }

        /// <summary>
        /// Sets the owner and start position of the specified dialog box based on the currently active form.
        /// </summary>
        /// <param name="diag">The dialog box whose owner and start position are to be set.</param>
        /// <returns>The form assigned as the owner, or null if no suitable owner is found.</returns>
        private static Form? SetDialogOwner(DialogBox diag)
        {
            Form? owner = Form.ActiveForm ?? Application.OpenForms.Cast<Form>().LastOrDefault();
            if (owner != null)
            {
                diag.Owner = owner;
                diag.StartPosition = FormStartPosition.CenterParent;
            }
            else
            {
                diag.StartPosition = FormStartPosition.CenterScreen;
            }

            return owner;
        }

        /// <summary>
        /// Displays a dialog box with the specified message and an OK button.
        /// </summary>
        /// <param name="message">The message to display in the dialog box.</param>
        /// <returns>The dialog result indicating how the dialog was closed.</returns>
        public static DialogResult Show(string message)
        {
            using (var diag = new DialogBox())
            {
                Control OKButton = new DialogButton("OK", DialogResult.OK).Btn;

                SetMessage(diag, message);
                HideIconPanel(diag);
                SetPanelControlSize(diag, 0);
                SetButtonControl(diag, OKButton);
                AutoSizeWindow(diag);

                var owner = SetDialogOwner(diag);
                return diag.ShowDialog(owner);
            }
        }

        /// <summary>
        /// Displays a dialog box with a specified message and title with an OK button.
        /// </summary>
        /// <param name="message">The message to display in the dialog box.</param>
        /// <param name="title">The title of the dialog box.</param>
        /// <returns>A DialogResult value indicating the dialog was closed.</returns>
        public static DialogResult Show(string message, string title)
        {
            using (var diag = new DialogBox())
            {
                Control OKButton = new DialogButton("OK", DialogResult.OK).Btn;

                SetMessage(diag, message, title);
                HideIconPanel(diag);
                SetPanelControlSize(diag, 0);
                SetButtonControl(diag, OKButton);
                AutoSizeWindow(diag);

                var owner = SetDialogOwner(diag);
                return diag.ShowDialog(owner);
            }
        }

        /// <summary>
        /// Displays a dialog box with a specified message, title, and button configuration.
        /// </summary>
        /// <param name="message">The text to display in the dialog box.</param>
        /// <param name="title">The title of the dialog box.</param>
        /// <param name="buttons">The default button configuration for the dialog box.</param>
        /// <returns>A DialogResult value indicating which button was clicked by the user.</returns>
        public static DialogResult Show(string message, string title, DialogButtonDefaults buttons)
        {
            using (var diag = new DialogBox())
            {
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

                var owner = SetDialogOwner(diag);
                return diag.ShowDialog(owner);
            }
        }

        /// <summary>
        /// Displays a dialog box with a specified message, title, and custom buttons.
        /// </summary>
        /// <param name="message">The text to display in the dialog box.</param>
        /// <param name="title">The title of the dialog box.</param>
        /// <param name="buttons">The buttons to display in the dialog box.</param>
        /// <returns>A DialogResult value indicating which button was clicked by the user.</returns>
        public static DialogResult Show(string message, string title, params DialogButton[] buttons)
        {
            using (var diag = new DialogBox())
            {
                SetMessage(diag, message, title);
                HideIconPanel(diag);
                SetPanelControlSize(diag, buttons.Length);
                ShouldDisableCloseBox(diag, buttons.Length > 1);
                AutoSizeWindow(diag);

                for (int i = 0; i < buttons.Length; i++)
                {
                    SetButtonControl(diag, i, buttons.Length, buttons[i].Btn);
                }

                var owner = SetDialogOwner(diag);
                return diag.ShowDialog(owner);
            }
        }

        /// <summary>
        /// Displays a dialog box with a specified message, title, button configuration, and icon.
        /// </summary>
        /// <param name="message">The text to display in the dialog box.</param>
        /// <param name="title">The title of the dialog box.</param>
        /// <param name="buttons">The button configuration for the dialog box.</param>
        /// <param name="icon">The icon to display in the dialog box.</param>
        /// <returns>A DialogResult value indicating which button was clicked by the user.</returns>
        public static DialogResult Show(string message, string title, DialogButtonDefaults buttons, DialogIcon icon)
        {
            using (var diag = new DialogBox())
            {
                DialogButton[] diagButtons = BuildButtons(DefaultButtons[(int)buttons]);

                SetMessage(diag, message, title);
                SetPanelControlSize(diag, diagButtons.Length);
                ShouldDisableCloseBox(diag, buttons != DialogButtonDefaults.OK);
                AutoSizeWindow(diag);

                for (int i = 0; i < diagButtons.Length; i++)
                {
                    SetButtonControl(diag, i, diagButtons.Length, diagButtons[i].Btn);
                }

                diag.DialogIcon.Image = DefaultIcons[(int)icon];

                var owner = SetDialogOwner(diag);
                return diag.ShowDialog(owner);
            }
        }

        /// <summary>
        /// Displays a dialog box with a specified message, title, icon, and custom buttons.
        /// </summary>
        /// <param name="message">The text to display in the dialog box.</param>
        /// <param name="title">The title of the dialog box.</param>
        /// <param name="icon">The icon to display in the dialog box.</param>
        /// <param name="buttons">The buttons to display in the dialog box.</param>
        /// <returns>A DialogResult value indicating which button was clicked by the user.</returns>
        public static DialogResult Show(string message, string title, DialogIcon icon, params DialogButton[] buttons)
        {
            using (var diag = new DialogBox())
            {
                SetMessage(diag, message, title);
                SetPanelControlSize(diag, buttons.Length);
                ShouldDisableCloseBox(diag, buttons.Length > 1);
                AutoSizeWindow(diag);

                for (int i = 0; i < buttons.Length; i++)
                {
                    SetButtonControl(diag, i, buttons.Length, buttons[i].Btn);
                }

                diag.DialogIcon.Image = DefaultIcons[(int)icon];

                var owner = SetDialogOwner(diag);
                return diag.ShowDialog(owner);
            }
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
        public static DialogResult Show(string message, string title, int width, DialogButtonDefaults buttons, DialogIcon icon)
        {
            using (var diag = new DialogBox())
            {
                DialogButton[] diagButtons = BuildButtons(DefaultButtons[(int)buttons]);

                SetMessage(diag, message, title);
                SetPanelControlSize(diag, diagButtons.Length);
                ShouldDisableCloseBox(diag, buttons != DialogButtonDefaults.OK);
                AutoSizeWindow(diag, width);

                for (int i = 0; i < diagButtons.Length; i++)
                {
                    SetButtonControl(diag, i, diagButtons.Length, diagButtons[i].Btn);
                }

                diag.DialogIcon.Image = DefaultIcons[(int)icon];

                var owner = SetDialogOwner(diag);
                return diag.ShowDialog(owner);
            }
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
        public static DialogResult Show(string message, string title, int width, DialogIcon icon, params DialogButton[] buttons)
        {
            using (var diag = new DialogBox())
            {
                SetMessage(diag, message, title);
                SetPanelControlSize(diag, buttons.Length);
                ShouldDisableCloseBox(diag, buttons.Length > 1);
                AutoSizeWindow(diag, width);

                for (int i = 0; i < buttons.Length; i++)
                {
                    SetButtonControl(diag, i, buttons.Length, buttons[i].Btn);
                }

                diag.DialogIcon.Image = DefaultIcons[(int)icon];

                var owner = SetDialogOwner(diag);
                return diag.ShowDialog(owner);
            }
        }
    }

    /// <summary>
    /// Represents a button used in dialog windows, encapsulating text, style, result, and the underlying KryptonButton
    /// control.
    /// </summary>
    public class DialogButton
    {
        /// <summary>Gets or sets the text value.</summary>
        public string Text { get; set; }
        /// <summary>Gets or sets the visual style of the button.</summary>
        public ButtonStyle Style { get; set; }
        /// <summary>Gets or sets the result of the dialog.</summary>
        public DialogResult Result { get; set; }
        /// <summary>Gets or sets the KryptonButton instance associated with this property.</summary>
        public KryptonButton Btn { get; set; } = new KryptonButton();

        public DialogButton(string text, DialogResult result, ButtonStyle style = ButtonStyle.Alternate)
        {
            Text = text;
            Result = result;
            Style = style;

            Btn.Text = Text;
            Btn.ButtonStyle = Style;
            Btn.AutoSize = true;
            Btn.Dock = DockStyle.Fill;
            Btn.Margin = new Padding(10);
            Btn.Cursor = Cursors.Hand;
            Btn.Click += (s, e) =>
            {
                if (Btn.FindForm() is Form parent)
                {
                    parent.DialogResult = result;
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
}
