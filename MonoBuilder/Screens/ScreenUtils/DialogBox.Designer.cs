namespace MonoBuilder.Screens.ScreenUtils
{
    partial class DialogBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DialogContainer = new Krypton.Toolkit.KryptonPanel();
            DialogButtonPanel = new Krypton.Toolkit.KryptonTableLayoutPanel();
            DialogPanel = new Krypton.Toolkit.KryptonTableLayoutPanel();
            DialogLabel = new Krypton.Toolkit.KryptonWrapLabel();
            DialogIcon = new Krypton.Toolkit.KryptonPictureBox();
            ((System.ComponentModel.ISupportInitialize)DialogContainer).BeginInit();
            DialogContainer.SuspendLayout();
            DialogPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DialogIcon).BeginInit();
            SuspendLayout();
            // 
            // DialogContainer
            // 
            DialogContainer.Controls.Add(DialogButtonPanel);
            DialogContainer.Controls.Add(DialogPanel);
            DialogContainer.Dock = DockStyle.Fill;
            DialogContainer.Location = new Point(0, 0);
            DialogContainer.Margin = new Padding(0);
            DialogContainer.Name = "DialogContainer";
            DialogContainer.Size = new Size(434, 146);
            DialogContainer.TabIndex = 0;
            // 
            // DialogButtonPanel
            // 
            DialogButtonPanel.ColumnCount = 1;
            DialogButtonPanel.ColumnStyles.Add(new ColumnStyle());
            DialogButtonPanel.Dock = DockStyle.Bottom;
            DialogButtonPanel.Location = new Point(0, 73);
            DialogButtonPanel.Margin = new Padding(5);
            DialogButtonPanel.Name = "DialogButtonPanel";
            DialogButtonPanel.Padding = new Padding(8);
            DialogButtonPanel.RowCount = 1;
            DialogButtonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            DialogButtonPanel.Size = new Size(434, 73);
            DialogButtonPanel.StateCommon.Color1 = Color.FromArgb(35, 35, 35);
            DialogButtonPanel.TabIndex = 0;
            // 
            // DialogPanel
            // 
            DialogPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            DialogPanel.AutoSize = true;
            DialogPanel.ColumnCount = 2;
            DialogPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));
            DialogPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 348F));
            DialogPanel.Controls.Add(DialogLabel, 1, 0);
            DialogPanel.Controls.Add(DialogIcon, 0, 0);
            DialogPanel.Location = new Point(0, 0);
            DialogPanel.Margin = new Padding(5);
            DialogPanel.Name = "DialogPanel";
            DialogPanel.RowCount = 1;
            DialogPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            DialogPanel.Size = new Size(434, 66);
            DialogPanel.TabIndex = 0;
            // 
            // DialogLabel
            // 
            DialogLabel.Dock = DockStyle.Fill;
            DialogLabel.LabelStyle = Krypton.Toolkit.LabelStyle.AlternateControl;
            DialogLabel.Location = new Point(89, 0);
            DialogLabel.Name = "DialogLabel";
            DialogLabel.Size = new Size(342, 66);
            DialogLabel.Text = "Empty";
            DialogLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // DialogIcon
            // 
            DialogIcon.Dock = DockStyle.Fill;
            DialogIcon.Location = new Point(3, 3);
            DialogIcon.Name = "DialogIcon";
            DialogIcon.Size = new Size(80, 60);
            DialogIcon.SizeMode = PictureBoxSizeMode.CenterImage;
            DialogIcon.TabIndex = 1;
            DialogIcon.TabStop = false;
            // 
            // DialogBox
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(434, 146);
            Controls.Add(DialogContainer);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DialogBox";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            StateCommon.Back.Color1 = Color.FromArgb(35, 35, 35);
            StateCommon.Back.Color2 = Color.FromArgb(35, 35, 35);
            StateCommon.Border.Color1 = Color.FromArgb(35, 35, 35);
            StateCommon.Border.Color2 = Color.FromArgb(35, 35, 35);
            StateCommon.Header.Back.Color1 = Color.FromArgb(35, 35, 35);
            StateCommon.Header.Back.Color2 = Color.FromArgb(35, 35, 35);
            StateCommon.Header.Content.LongText.Color1 = Color.White;
            StateCommon.Header.Content.ShortText.Color1 = Color.White;
            ((System.ComponentModel.ISupportInitialize)DialogContainer).EndInit();
            DialogContainer.ResumeLayout(false);
            DialogContainer.PerformLayout();
            DialogPanel.ResumeLayout(false);
            DialogPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)DialogIcon).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Krypton.Toolkit.KryptonPanel DialogContainer;
        private Krypton.Toolkit.KryptonTableLayoutPanel DialogPanel;
        private Krypton.Toolkit.KryptonTableLayoutPanel DialogButtonPanel;
        private Krypton.Toolkit.KryptonPictureBox DialogIcon;
        private Krypton.Toolkit.KryptonWrapLabel DialogLabel;
    }
}