using Krypton.Toolkit;
using System.Windows.Forms;

namespace MonoBuilder.Screens.ScreenUtils
{
    partial class AddModifyCharacter
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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddModifyCharacter));
			lblHeader = new KryptonLabel();
			namePanel = new FlowLayoutPanel();
			inputName = new KryptonTextBox();
			lblName = new KryptonLabel();
			tagPanel = new FlowLayoutPanel();
			inputTag = new KryptonTextBox();
			lblTag = new KryptonLabel();
			colorPanel = new FlowLayoutPanel();
			inputColor = new KryptonTextBox();
			btnColorSelect = new KryptonColorButton();
			lblColor = new KryptonLabel();
			pathPanel = new FlowLayoutPanel();
			inputPath = new KryptonTextBox();
			lblPath = new KryptonLabel();
			settingsPanel = new KryptonPanel();
			btnCancel = new KryptonButton();
			btnSave = new KryptonButton();
			TableContainer = new TableLayoutPanel();
			addPanel = new FlowLayoutPanel();
			btnAdd = new KryptonButton();
			colorWheel = new ColorDialog();
			inputError = new ErrorProvider(components);
			kryptonPanel1 = new KryptonPanel();
			KryptonColorWheel = new KryptonColorDialog();
			namePanel.SuspendLayout();
			tagPanel.SuspendLayout();
			colorPanel.SuspendLayout();
			pathPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)settingsPanel).BeginInit();
			settingsPanel.SuspendLayout();
			TableContainer.SuspendLayout();
			addPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)inputError).BeginInit();
			((System.ComponentModel.ISupportInitialize)kryptonPanel1).BeginInit();
			kryptonPanel1.SuspendLayout();
			SuspendLayout();
			// 
			// lblHeader
			// 
			lblHeader.Location = new Point(3, 10);
			lblHeader.Margin = new Padding(10);
			lblHeader.Name = "lblHeader";
			lblHeader.Size = new Size(147, 28);
			lblHeader.StateCommon.ShortText.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblHeader.TabIndex = 0;
			lblHeader.Values.Text = "Add Character";
			// 
			// namePanel
			// 
			namePanel.Controls.Add(inputName);
			namePanel.Location = new Point(3, 34);
			namePanel.Name = "namePanel";
			namePanel.Size = new Size(187, 41);
			namePanel.TabIndex = 0;
			// 
			// inputName
			// 
			inputName.InputControlStyle = InputControlStyle.Custom1;
			inputName.Location = new Point(5, 5);
			inputName.Margin = new Padding(5);
			inputName.Name = "inputName";
			inputName.Size = new Size(160, 32);
			inputName.TabIndex = 1;
			inputName.TextChanged += StringInputValidation;
			// 
			// lblName
			// 
			lblName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblName.Location = new Point(5, 5);
			lblName.Margin = new Padding(5);
			lblName.Name = "lblName";
			lblName.Size = new Size(183, 21);
			lblName.StateCommon.ShortText.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Bold);
			lblName.TabIndex = 0;
			lblName.Values.Text = "Name";
			// 
			// tagPanel
			// 
			tagPanel.Controls.Add(inputTag);
			tagPanel.Location = new Point(196, 34);
			tagPanel.Name = "tagPanel";
			tagPanel.Size = new Size(183, 41);
			tagPanel.TabIndex = 1;
			// 
			// inputTag
			// 
			inputTag.InputControlStyle = InputControlStyle.Custom1;
			inputTag.Location = new Point(5, 5);
			inputTag.Margin = new Padding(5);
			inputTag.Name = "inputTag";
			inputTag.Size = new Size(155, 32);
			inputTag.TabIndex = 2;
			inputTag.TextChanged += StringInputValidation;
			// 
			// lblTag
			// 
			lblTag.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblTag.Location = new Point(198, 5);
			lblTag.Margin = new Padding(5);
			lblTag.Name = "lblTag";
			lblTag.Size = new Size(179, 21);
			lblTag.StateCommon.ShortText.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Bold);
			lblTag.TabIndex = 0;
			lblTag.Values.Text = "Tag";
			// 
			// colorPanel
			// 
			colorPanel.Controls.Add(inputColor);
			colorPanel.Controls.Add(btnColorSelect);
			colorPanel.Location = new Point(385, 34);
			colorPanel.Name = "colorPanel";
			colorPanel.Size = new Size(188, 41);
			colorPanel.TabIndex = 2;
			// 
			// inputColor
			// 
			inputColor.InputControlStyle = InputControlStyle.Custom1;
			inputColor.Location = new Point(5, 5);
			inputColor.Margin = new Padding(5);
			inputColor.Name = "inputColor";
			inputColor.Size = new Size(107, 32);
			inputColor.TabIndex = 3;
			inputColor.TextChanged += inputColor_TextChanged;
			// 
			// btnColorSelect
			// 
			btnColorSelect.ButtonStyle = ButtonStyle.Alternate;
			btnColorSelect.Cursor = Cursors.Hand;
			btnColorSelect.EmptyBorderColor = Color.Transparent;
			btnColorSelect.Location = new Point(122, 5);
			btnColorSelect.Margin = new Padding(5);
			btnColorSelect.Name = "btnColorSelect";
			btnColorSelect.SelectedRect = new Rectangle(0, -1, 25, 25);
			btnColorSelect.Size = new Size(61, 32);
			btnColorSelect.Splitter = false;
			btnColorSelect.StateCommon.Content.Padding = new Padding(0);
			btnColorSelect.TabIndex = 3;
			btnColorSelect.TabStop = false;
			btnColorSelect.Tag = "inputColor";
			btnColorSelect.Values.Text = "";
			btnColorSelect.SelectedColorChanged += btnColorSelect_ColorChange;
			btnColorSelect.DoubleClick += btnColorSelect_Click;
			// 
			// lblColor
			// 
			lblColor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblColor.Location = new Point(387, 5);
			lblColor.Margin = new Padding(5);
			lblColor.Name = "lblColor";
			lblColor.Size = new Size(184, 21);
			lblColor.StateCommon.ShortText.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Bold);
			lblColor.TabIndex = 0;
			lblColor.Values.Text = "Color";
			// 
			// pathPanel
			// 
			pathPanel.Controls.Add(inputPath);
			pathPanel.Location = new Point(579, 34);
			pathPanel.Name = "pathPanel";
			pathPanel.Size = new Size(301, 41);
			pathPanel.TabIndex = 3;
			// 
			// inputPath
			// 
			inputPath.InputControlStyle = InputControlStyle.Custom1;
			inputPath.Location = new Point(5, 5);
			inputPath.Margin = new Padding(5);
			inputPath.Name = "inputPath";
			inputPath.Size = new Size(261, 32);
			inputPath.TabIndex = 4;
			// 
			// lblPath
			// 
			lblPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblPath.Location = new Point(581, 5);
			lblPath.Margin = new Padding(5);
			lblPath.Name = "lblPath";
			lblPath.Size = new Size(339, 21);
			lblPath.StateCommon.ShortText.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Bold);
			lblPath.TabIndex = 0;
			lblPath.Values.Text = "Dictionary (Path)";
			// 
			// settingsPanel
			// 
			settingsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			settingsPanel.AutoScroll = true;
			settingsPanel.Controls.Add(btnCancel);
			settingsPanel.Controls.Add(btnSave);
			settingsPanel.Controls.Add(TableContainer);
			settingsPanel.Location = new Point(3, 51);
			settingsPanel.Name = "settingsPanel";
			settingsPanel.Padding = new Padding(5);
			settingsPanel.Size = new Size(935, 225);
			settingsPanel.TabIndex = 0;
			// 
			// btnCancel
			// 
			btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			btnCancel.ButtonStyle = ButtonStyle.Alternate;
			btnCancel.Cursor = Cursors.Hand;
			btnCancel.Location = new Point(781, 167);
			btnCancel.Margin = new Padding(20);
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new Size(127, 40);
			btnCancel.TabIndex = 7;
			btnCancel.Values.DropDownArrowColor = Color.Empty;
			btnCancel.Values.Text = "Cancel";
			btnCancel.Click += btnCancel_Click;
			// 
			// btnSave
			// 
			btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			btnSave.ButtonStyle = ButtonStyle.Alternate;
			btnSave.Cursor = Cursors.Hand;
			btnSave.Enabled = false;
			btnSave.Location = new Point(644, 167);
			btnSave.Margin = new Padding(20);
			btnSave.Name = "btnSave";
			btnSave.Size = new Size(127, 40);
			btnSave.TabIndex = 6;
			btnSave.Values.DropDownArrowColor = Color.Empty;
			btnSave.Values.Text = "Save";
			btnSave.Click += btnSave_Click;
			// 
			// TableContainer
			// 
			TableContainer.AutoScroll = true;
			TableContainer.BackColor = Color.Transparent;
			TableContainer.ColumnCount = 4;
			TableContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 193F));
			TableContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 189F));
			TableContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 194F));
			TableContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 15F));
			TableContainer.Controls.Add(lblPath, 3, 0);
			TableContainer.Controls.Add(lblColor, 2, 0);
			TableContainer.Controls.Add(lblTag, 1, 0);
			TableContainer.Controls.Add(lblName, 0, 0);
			TableContainer.Controls.Add(colorPanel, 2, 1);
			TableContainer.Controls.Add(tagPanel, 1, 1);
			TableContainer.Controls.Add(namePanel, 0, 1);
			TableContainer.Controls.Add(pathPanel, 3, 1);
			TableContainer.Controls.Add(addPanel, 0, 2);
			TableContainer.Dock = DockStyle.Top;
			TableContainer.Location = new Point(5, 5);
			TableContainer.Name = "TableContainer";
			TableContainer.RowCount = 3;
			TableContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
			TableContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 47F));
			TableContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 8F));
			TableContainer.Size = new Size(925, 139);
			TableContainer.TabIndex = 0;
			// 
			// addPanel
			// 
			addPanel.Controls.Add(btnAdd);
			addPanel.Dock = DockStyle.Fill;
			addPanel.Location = new Point(3, 81);
			addPanel.Name = "addPanel";
			addPanel.Size = new Size(187, 55);
			addPanel.TabIndex = 4;
			// 
			// btnAdd
			// 
			btnAdd.ButtonStyle = ButtonStyle.Alternate;
			btnAdd.Cursor = Cursors.Hand;
			btnAdd.Location = new Point(5, 5);
			btnAdd.Margin = new Padding(5);
			btnAdd.Name = "btnAdd";
			btnAdd.Size = new Size(35, 32);
			btnAdd.StateCommon.Content.ShortText.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
			btnAdd.StateCommon.Content.ShortText.Hint = PaletteTextHint.AntiAlias;
			btnAdd.TabIndex = 5;
			btnAdd.Values.DropDownArrowColor = Color.Empty;
			btnAdd.Values.Text = "+";
			btnAdd.Click += btnAdd_Click;
			// 
			// inputError
			// 
			inputError.ContainerControl = this;
			// 
			// kryptonPanel1
			// 
			kryptonPanel1.Controls.Add(lblHeader);
			kryptonPanel1.Controls.Add(settingsPanel);
			kryptonPanel1.Dock = DockStyle.Fill;
			kryptonPanel1.Location = new Point(0, 0);
			kryptonPanel1.Name = "kryptonPanel1";
			kryptonPanel1.Size = new Size(941, 319);
			kryptonPanel1.TabIndex = 8;
			// 
			// KryptonColorWheel
			// 
			KryptonColorWheel.Color = Color.Black;
			KryptonColorWheel.FullOpen = true;
			KryptonColorWheel.Icon = (Icon)resources.GetObject("KryptonColorWheel.Icon");
			KryptonColorWheel.Title = null;
			// 
			// AddModifyCharacter
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(941, 319);
			Controls.Add(kryptonPanel1);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MaximumSize = new Size(957, 750);
			Name = "AddModifyCharacter";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "MonoBuilder | Add Character";
			Load += AddModifyCharacter_Load;
			namePanel.ResumeLayout(false);
			namePanel.PerformLayout();
			tagPanel.ResumeLayout(false);
			tagPanel.PerformLayout();
			colorPanel.ResumeLayout(false);
			colorPanel.PerformLayout();
			pathPanel.ResumeLayout(false);
			pathPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)settingsPanel).EndInit();
			settingsPanel.ResumeLayout(false);
			TableContainer.ResumeLayout(false);
			TableContainer.PerformLayout();
			addPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)inputError).EndInit();
			((System.ComponentModel.ISupportInitialize)kryptonPanel1).EndInit();
			kryptonPanel1.ResumeLayout(false);
			kryptonPanel1.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private KryptonLabel lblHeader;
        private KryptonTextBox inputName;
        private KryptonLabel lblName;
        private KryptonLabel lblColor;
        private KryptonLabel lblTag;
        private KryptonTextBox inputTag;
        private KryptonButton btnSave;
        private KryptonButton btnCancel;
        private KryptonLabel lblPath;
        private KryptonTextBox inputPath;
        private ColorDialog colorWheel;
        private KryptonColorButton btnColorSelect;
        private ErrorProvider inputError;
        private TableLayoutPanel TableContainer;
        private KryptonButton btnAdd;
        private FlowLayoutPanel colorPanel;
        private FlowLayoutPanel tagPanel;
        private FlowLayoutPanel namePanel;
        private FlowLayoutPanel pathPanel;
        private FlowLayoutPanel addPanel;
        private KryptonTextBox inputColor;
        private KryptonPanel settingsPanel;
        private KryptonPanel kryptonPanel1;
        private KryptonColorDialog KryptonColorWheel;
    }
}