using Krypton.Toolkit;
using System.Diagnostics;

namespace MonoBuilder.Screens
{
    partial class Settings
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
                foreach (Control control in tlpSettings.Controls)
                {
                    if (control is DataGridView dgv)
                    {
                        dgv.DataBindings.Clear();
                        dgv.DataSource = null;
                    }
                }
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
			tlpSettings = new TableLayoutPanel();
			FilePath = new OpenFileDialog();
			FolderPath = new FolderBrowserDialog();
			BottomControls = new KryptonTableLayoutPanel();
			button1 = new KryptonButton();
			ChangesMadeLabel = new KryptonLabel();
			kryptonPanel1 = new KryptonPanel();
			BottomControls.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)kryptonPanel1).BeginInit();
			kryptonPanel1.SuspendLayout();
			SuspendLayout();
			// 
			// tlpSettings
			// 
			tlpSettings.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			tlpSettings.AutoScroll = true;
			tlpSettings.BackColor = Color.FromArgb(20, 20, 20);
			tlpSettings.ColumnCount = 2;
			tlpSettings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			tlpSettings.ColumnStyles.Add(new ColumnStyle());
			tlpSettings.Location = new Point(0, 0);
			tlpSettings.Name = "tlpSettings";
			tlpSettings.RowCount = 1;
			tlpSettings.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
			tlpSettings.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
			tlpSettings.Size = new Size(800, 385);
			tlpSettings.TabIndex = 0;
			// 
			// FilePath
			// 
			FilePath.FileName = "openFileDialog1";
			FilePath.RestoreDirectory = true;
			// 
			// FolderPath
			// 
			FolderPath.Description = "Select the target folder";
			// 
			// BottomControls
			// 
			BottomControls.ColumnCount = 2;
			BottomControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 88.125F));
			BottomControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.875F));
			BottomControls.Controls.Add(button1, 1, 0);
			BottomControls.Controls.Add(ChangesMadeLabel, 0, 0);
			BottomControls.Dock = DockStyle.Fill;
			BottomControls.Location = new Point(0, 0);
			BottomControls.Name = "BottomControls";
			BottomControls.RowCount = 1;
			BottomControls.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
			BottomControls.Size = new Size(800, 60);
			BottomControls.TabIndex = 2;
			// 
			// button1
			// 
			button1.ButtonStyle = ButtonStyle.Alternate;
			button1.Cursor = Cursors.Hand;
			button1.Dock = DockStyle.Right;
			button1.Location = new Point(715, 10);
			button1.Margin = new Padding(10);
			button1.Name = "button1";
			button1.Size = new Size(75, 40);
			button1.TabIndex = 0;
			button1.Values.DropDownArrowColor = Color.Empty;
			button1.Values.Text = "Exit";
			button1.Click += button1_Click;
			// 
			// ChangesMadeLabel
			// 
			ChangesMadeLabel.Dock = DockStyle.Fill;
			ChangesMadeLabel.Location = new Point(3, 3);
			ChangesMadeLabel.Name = "ChangesMadeLabel";
			ChangesMadeLabel.Size = new Size(699, 54);
			ChangesMadeLabel.TabIndex = 1;
			ChangesMadeLabel.Values.Text = "Changes Have Been Made to One or More Files or Folders";
			ChangesMadeLabel.Visible = false;
			// 
			// kryptonPanel1
			// 
			kryptonPanel1.Controls.Add(BottomControls);
			kryptonPanel1.Dock = DockStyle.Bottom;
			kryptonPanel1.Location = new Point(0, 391);
			kryptonPanel1.Name = "kryptonPanel1";
			kryptonPanel1.Size = new Size(800, 60);
			kryptonPanel1.TabIndex = 4;
			// 
			// Settings
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 451);
			Controls.Add(tlpSettings);
			Controls.Add(kryptonPanel1);
			Icon = (Icon)resources.GetObject("$this.Icon");
			Name = "Settings";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "MonoBuilder | Settings";
			Load += Settings_Load;
			BottomControls.ResumeLayout(false);
			BottomControls.PerformLayout();
			((System.ComponentModel.ISupportInitialize)kryptonPanel1).EndInit();
			kryptonPanel1.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private TableLayoutPanel tlpSettings;
        private OpenFileDialog FilePath;
        private FolderBrowserDialog FolderPath;
        private KryptonTableLayoutPanel kryptonTableLayoutPanel1;
        private KryptonButton button1;
        private KryptonLabel ChangesMadeLabel;
        private KryptonTableLayoutPanel BottomControls;
        private KryptonPanel kryptonPanel1;
    }
}