using MonoBuilder.CustomComponents;

namespace MonoBuilder.Screens.ScreenUtils
{
    partial class LoadScripts
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadScripts));
			labelGridUnsaved = new Krypton.Toolkit.KryptonDataGridView();
			ControlPanel = new Krypton.Toolkit.KryptonPanel();
			ChangesMadeLabel = new Krypton.Toolkit.KryptonLabel();
			splitContainer = new SplitContainer();
			ScriptTypes = new TabControl();
			tabUnsavedLabels = new TabPage();
			btnSaveLabel = new Krypton.Toolkit.KryptonButton();
			tabSavedLabels = new TabPage();
			kryptonTableLayoutPanel1 = new Krypton.Toolkit.KryptonTableLayoutPanel();
			btnMergeLabels = new Krypton.Toolkit.KryptonButton();
			btnRemoveLabels = new Krypton.Toolkit.KryptonButton();
			labelGridSaved = new Krypton.Toolkit.KryptonDataGridView();
			previewBox = new EnhancedRichTextBox();
			btnClose = new Krypton.Toolkit.KryptonButton();
			btnLoad = new Krypton.Toolkit.KryptonButton();
			btnSyncLabels = new Krypton.Toolkit.KryptonButton();
			((System.ComponentModel.ISupportInitialize)labelGridUnsaved).BeginInit();
			((System.ComponentModel.ISupportInitialize)ControlPanel).BeginInit();
			ControlPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
			splitContainer.Panel1.SuspendLayout();
			splitContainer.Panel2.SuspendLayout();
			splitContainer.SuspendLayout();
			ScriptTypes.SuspendLayout();
			tabUnsavedLabels.SuspendLayout();
			tabSavedLabels.SuspendLayout();
			kryptonTableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)labelGridSaved).BeginInit();
			SuspendLayout();
			// 
			// labelGridUnsaved
			// 
			labelGridUnsaved.AllowUserToAddRows = false;
			labelGridUnsaved.AllowUserToDeleteRows = false;
			labelGridUnsaved.AllowUserToResizeColumns = false;
			labelGridUnsaved.AllowUserToResizeRows = false;
			labelGridUnsaved.AutoGenerateKryptonColumns = false;
			labelGridUnsaved.BorderStyle = BorderStyle.None;
			labelGridUnsaved.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			labelGridUnsaved.Dock = DockStyle.Top;
			labelGridUnsaved.Location = new Point(3, 3);
			labelGridUnsaved.MultiSelect = false;
			labelGridUnsaved.Name = "labelGridUnsaved";
			labelGridUnsaved.ReadOnly = true;
			labelGridUnsaved.RowHeadersVisible = false;
			labelGridUnsaved.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			labelGridUnsaved.Size = new Size(336, 340);
			labelGridUnsaved.TabIndex = 0;
			labelGridUnsaved.CellMouseDoubleClick += GridCellMouseDoubleClick;
			labelGridUnsaved.CellMouseEnter += GridCellMouseEnter;
			labelGridUnsaved.CellMouseLeave += GridCellMouseLeave;
			labelGridUnsaved.SelectionChanged += LabelGrid_SelectionChanged;
			// 
			// ControlPanel
			// 
			ControlPanel.Controls.Add(ChangesMadeLabel);
			ControlPanel.Controls.Add(splitContainer);
			ControlPanel.Controls.Add(btnClose);
			ControlPanel.Controls.Add(btnLoad);
			ControlPanel.Controls.Add(btnSyncLabels);
			ControlPanel.Dock = DockStyle.Fill;
			ControlPanel.ImeMode = ImeMode.NoControl;
			ControlPanel.Location = new Point(0, 0);
			ControlPanel.Name = "ControlPanel";
			ControlPanel.Size = new Size(900, 500);
			ControlPanel.TabIndex = 0;
			// 
			// ChangesMadeLabel
			// 
			ChangesMadeLabel.Location = new Point(135, 458);
			ChangesMadeLabel.Margin = new Padding(10);
			ChangesMadeLabel.Name = "ChangesMadeLabel";
			ChangesMadeLabel.Size = new Size(116, 30);
			ChangesMadeLabel.TabIndex = 2;
			ChangesMadeLabel.Values.Text = "Script File Changed";
			ChangesMadeLabel.Visible = false;
			// 
			// splitContainer
			// 
			splitContainer.Location = new Point(12, 12);
			splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			splitContainer.Panel1.Controls.Add(ScriptTypes);
			// 
			// splitContainer.Panel2
			// 
			splitContainer.Panel2.Controls.Add(previewBox);
			splitContainer.Size = new Size(876, 420);
			splitContainer.SplitterDistance = 350;
			splitContainer.TabIndex = 1;
			// 
			// ScriptTypes
			// 
			ScriptTypes.Controls.Add(tabUnsavedLabels);
			ScriptTypes.Controls.Add(tabSavedLabels);
			ScriptTypes.Dock = DockStyle.Fill;
			ScriptTypes.DrawMode = TabDrawMode.OwnerDrawFixed;
			ScriptTypes.Location = new Point(0, 0);
			ScriptTypes.Name = "ScriptTypes";
			ScriptTypes.SelectedIndex = 0;
			ScriptTypes.Size = new Size(350, 420);
			ScriptTypes.TabIndex = 1;
			ScriptTypes.DrawItem += ScriptTypes_DrawItem;
			ScriptTypes.SelectedIndexChanged += ScriptTypes_SelectedIndexChanged;
			// 
			// tabUnsavedLabels
			// 
			tabUnsavedLabels.BackColor = Color.FromArgb(20, 20, 20);
			tabUnsavedLabels.Controls.Add(labelGridUnsaved);
			tabUnsavedLabels.Controls.Add(btnSaveLabel);
			tabUnsavedLabels.ForeColor = Color.White;
			tabUnsavedLabels.Location = new Point(4, 24);
			tabUnsavedLabels.Name = "tabUnsavedLabels";
			tabUnsavedLabels.Padding = new Padding(3);
			tabUnsavedLabels.Size = new Size(342, 392);
			tabUnsavedLabels.TabIndex = 0;
			tabUnsavedLabels.Text = "Unsaved Labels";
			// 
			// btnSaveLabel
			// 
			btnSaveLabel.ButtonStyle = Krypton.Toolkit.ButtonStyle.Alternate;
			btnSaveLabel.Cursor = Cursors.Hand;
			btnSaveLabel.Dock = DockStyle.Bottom;
			btnSaveLabel.Enabled = false;
			btnSaveLabel.Location = new Point(3, 344);
			btnSaveLabel.Margin = new Padding(3, 10, 3, 3);
			btnSaveLabel.Name = "btnSaveLabel";
			btnSaveLabel.Size = new Size(336, 45);
			btnSaveLabel.TabIndex = 0;
			btnSaveLabel.Values.DropDownArrowColor = Color.Empty;
			btnSaveLabel.Values.Text = "Save Labels";
			btnSaveLabel.Click += btnSaveLabel_Click;
			// 
			// tabSavedLabels
			// 
			tabSavedLabels.BackColor = Color.FromArgb(20, 20, 20);
			tabSavedLabels.Controls.Add(kryptonTableLayoutPanel1);
			tabSavedLabels.Controls.Add(labelGridSaved);
			tabSavedLabels.ForeColor = Color.White;
			tabSavedLabels.Location = new Point(4, 24);
			tabSavedLabels.Name = "tabSavedLabels";
			tabSavedLabels.Padding = new Padding(3);
			tabSavedLabels.Size = new Size(342, 392);
			tabSavedLabels.TabIndex = 1;
			tabSavedLabels.Text = "Saved Labels";
			// 
			// kryptonTableLayoutPanel1
			// 
			kryptonTableLayoutPanel1.ColumnCount = 2;
			kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			kryptonTableLayoutPanel1.Controls.Add(btnMergeLabels, 0, 0);
			kryptonTableLayoutPanel1.Controls.Add(btnRemoveLabels, 1, 0);
			kryptonTableLayoutPanel1.Dock = DockStyle.Bottom;
			kryptonTableLayoutPanel1.Location = new Point(3, 343);
			kryptonTableLayoutPanel1.Margin = new Padding(0);
			kryptonTableLayoutPanel1.Name = "kryptonTableLayoutPanel1";
			kryptonTableLayoutPanel1.RowCount = 1;
			kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
			kryptonTableLayoutPanel1.Size = new Size(336, 46);
			kryptonTableLayoutPanel1.TabIndex = 2;
			// 
			// btnMergeLabels
			// 
			btnMergeLabels.ButtonStyle = Krypton.Toolkit.ButtonStyle.Alternate;
			btnMergeLabels.Cursor = Cursors.Hand;
			btnMergeLabels.Location = new Point(5, 5);
			btnMergeLabels.Margin = new Padding(5);
			btnMergeLabels.Name = "btnMergeLabels";
			btnMergeLabels.Size = new Size(158, 36);
			btnMergeLabels.TabIndex = 2;
			btnMergeLabels.Values.DropDownArrowColor = Color.Empty;
			btnMergeLabels.Values.Text = "Merge Labels to Script";
			btnMergeLabels.Click += btnMergeLabels_Click;
			// 
			// btnRemoveLabels
			// 
			btnRemoveLabels.ButtonStyle = Krypton.Toolkit.ButtonStyle.Custom2;
			btnRemoveLabels.Cursor = Cursors.Hand;
			btnRemoveLabels.Location = new Point(173, 5);
			btnRemoveLabels.Margin = new Padding(5);
			btnRemoveLabels.Name = "btnRemoveLabels";
			btnRemoveLabels.Size = new Size(158, 36);
			btnRemoveLabels.TabIndex = 2;
			btnRemoveLabels.Values.DropDownArrowColor = Color.Empty;
			btnRemoveLabels.Values.Text = "Remove Labels";
			btnRemoveLabels.Click += btnRemoveLabels_Click;
			// 
			// labelGridSaved
			// 
			labelGridSaved.AllowDrop = true;
			labelGridSaved.AllowUserToAddRows = false;
			labelGridSaved.AllowUserToDeleteRows = false;
			labelGridSaved.AllowUserToResizeColumns = false;
			labelGridSaved.AllowUserToResizeRows = false;
			labelGridSaved.AutoGenerateKryptonColumns = false;
			labelGridSaved.BorderStyle = BorderStyle.None;
			labelGridSaved.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			labelGridSaved.Dock = DockStyle.Top;
			labelGridSaved.Location = new Point(3, 3);
			labelGridSaved.Margin = new Padding(0);
			labelGridSaved.Name = "labelGridSaved";
			labelGridSaved.ReadOnly = true;
			labelGridSaved.RowHeadersVisible = false;
			labelGridSaved.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			labelGridSaved.Size = new Size(336, 340);
			labelGridSaved.TabIndex = 1;
			labelGridSaved.CellMouseDoubleClick += GridCellMouseDoubleClick;
			labelGridSaved.CellMouseEnter += GridCellMouseEnter;
			labelGridSaved.CellMouseLeave += GridCellMouseLeave;
			labelGridSaved.DataBindingComplete += DataBindingComplete;
			labelGridSaved.SelectionChanged += LabelGrid_SelectionChanged;
			labelGridSaved.DragDrop += RowDragDrop;
			labelGridSaved.DragOver += RowDragOver;
			labelGridSaved.MouseDown += RowDragMouseDown;
			labelGridSaved.MouseMove += RowDragMouseMove;
			// 
			// previewBox
			// 
			previewBox.BackColor = Color.FromArgb(20, 20, 20);
			previewBox.DisabledBackColor = Color.FromArgb(20, 20, 20);
			previewBox.DisabledForeColor = Color.Gray;
			previewBox.Dock = DockStyle.Fill;
			previewBox.ForeColor = Color.White;
			previewBox.Location = new Point(0, 0);
			previewBox.Name = "previewBox";
			previewBox.ReadOnly = true;
			previewBox.ScrollBars = RichTextBoxScrollBars.Vertical;
			previewBox.Size = new Size(522, 420);
			previewBox.TabIndex = 0;
			previewBox.Text = "";
			previewBox.TextChanged += previewBox_TextChanged;
			// 
			// btnClose
			// 
			btnClose.ButtonStyle = Krypton.Toolkit.ButtonStyle.Alternate;
			btnClose.Cursor = Cursors.Hand;
			btnClose.Location = new Point(788, 458);
			btnClose.Margin = new Padding(5, 10, 5, 3);
			btnClose.Name = "btnClose";
			btnClose.Size = new Size(100, 30);
			btnClose.TabIndex = 0;
			btnClose.Values.DropDownArrowColor = Color.Empty;
			btnClose.Values.Text = "Close";
			btnClose.Click += btnClose_Click;
			// 
			// btnLoad
			// 
			btnLoad.ButtonStyle = Krypton.Toolkit.ButtonStyle.Alternate;
			btnLoad.Cursor = Cursors.Hand;
			btnLoad.Enabled = false;
			btnLoad.Location = new Point(678, 458);
			btnLoad.Margin = new Padding(5, 10, 5, 3);
			btnLoad.Name = "btnLoad";
			btnLoad.Size = new Size(100, 30);
			btnLoad.TabIndex = 0;
			btnLoad.Values.DropDownArrowColor = Color.Empty;
			btnLoad.Values.Text = "Load";
			btnLoad.Click += btnLoad_Click;
			// 
			// btnSyncLabels
			// 
			btnSyncLabels.ButtonStyle = Krypton.Toolkit.ButtonStyle.Alternate;
			btnSyncLabels.Cursor = Cursors.Hand;
			btnSyncLabels.Location = new Point(12, 458);
			btnSyncLabels.Margin = new Padding(3, 10, 3, 3);
			btnSyncLabels.Name = "btnSyncLabels";
			btnSyncLabels.Size = new Size(110, 30);
			btnSyncLabels.TabIndex = 0;
			btnSyncLabels.Values.DropDownArrowColor = Color.Empty;
			btnSyncLabels.Values.Text = "Sync Labels";
			btnSyncLabels.Click += btnSyncScripts_Click;
			// 
			// LoadScripts
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = btnClose;
			ClientSize = new Size(900, 500);
			Controls.Add(ControlPanel);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "LoadScripts";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "Load Labels";
			FormClosing += LoadScripts_FormClosing;
			Load += LoadScripts_Load;
			((System.ComponentModel.ISupportInitialize)labelGridUnsaved).EndInit();
			((System.ComponentModel.ISupportInitialize)ControlPanel).EndInit();
			ControlPanel.ResumeLayout(false);
			ControlPanel.PerformLayout();
			splitContainer.Panel1.ResumeLayout(false);
			splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
			splitContainer.ResumeLayout(false);
			ScriptTypes.ResumeLayout(false);
			tabUnsavedLabels.ResumeLayout(false);
			tabSavedLabels.ResumeLayout(false);
			kryptonTableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)labelGridSaved).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private Krypton.Toolkit.KryptonPanel ControlPanel;
        private SplitContainer splitContainer;
        private Krypton.Toolkit.KryptonDataGridView labelGridUnsaved;
        private EnhancedRichTextBox previewBox;
        private Krypton.Toolkit.KryptonButton btnSyncLabels;
        private Krypton.Toolkit.KryptonButton btnClose;
        private Krypton.Toolkit.KryptonButton btnLoad;
        private TabControl ScriptTypes;
        private TabPage tabUnsavedLabels;
        private TabPage tabSavedLabels;
        private Krypton.Toolkit.KryptonButton btnSaveLabel;
        private Krypton.Toolkit.KryptonDataGridView labelGridSaved;
        private Krypton.Toolkit.KryptonButton btnMergeLabels;
        private Krypton.Toolkit.KryptonTableLayoutPanel kryptonTableLayoutPanel1;
        private Krypton.Toolkit.KryptonButton btnRemoveLabels;
        private Krypton.Toolkit.KryptonLabel ChangesMadeLabel;
    }
}