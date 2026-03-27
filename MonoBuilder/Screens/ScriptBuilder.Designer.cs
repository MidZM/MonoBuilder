using Krypton.Toolkit;
using MonoBuilder.CustomComponents;

namespace MonoBuilder.Screens
{
    partial class ScriptBuilder
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptBuilder));
			labelInput = new TextBox();
			scriptInput = new EnhancedRichTextBox();
			btnLoadCopy = new KryptonButton();
			btnSaveCopy = new KryptonButton();
			btnReset = new KryptonButton();
			btnConvert = new KryptonButton();
			kryptonPanel1 = new KryptonPanel();
			kryptonTableLayoutPanel1 = new KryptonTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)kryptonPanel1).BeginInit();
			kryptonPanel1.SuspendLayout();
			kryptonTableLayoutPanel1.SuspendLayout();
			SuspendLayout();
			// 
			// labelInput
			// 
			labelInput.BackColor = Color.FromArgb(50, 50, 50);
			kryptonTableLayoutPanel1.SetColumnSpan(labelInput, 4);
			labelInput.Dock = DockStyle.Fill;
			labelInput.Font = new Font("Segoe UI", 14F);
			labelInput.ForeColor = Color.White;
			labelInput.Location = new Point(10, 10);
			labelInput.Margin = new Padding(10);
			labelInput.Name = "labelInput";
			labelInput.Size = new Size(1384, 32);
			labelInput.TabIndex = 0;
			labelInput.TextChanged += ShouldEnableConversion;
			// 
			// scriptInput
			// 
			scriptInput.AcceptsTab = true;
			scriptInput.BackColor = Color.FromArgb(50, 50, 50);
			scriptInput.BulletIndent = 1;
			kryptonTableLayoutPanel1.SetColumnSpan(scriptInput, 4);
			scriptInput.DisabledBackColor = Color.FromArgb(50, 50, 50);
			scriptInput.DisabledForeColor = Color.Gray;
			scriptInput.Dock = DockStyle.Fill;
			scriptInput.Font = new Font("Segoe UI", 12F);
			scriptInput.ForeColor = Color.White;
			scriptInput.Location = new Point(10, 59);
			scriptInput.Margin = new Padding(10);
			scriptInput.Name = "scriptInput";
			scriptInput.Size = new Size(1384, 627);
			scriptInput.TabIndex = 1;
			scriptInput.Text = "";
			scriptInput.TextChanged += ShouldEnableConversion;
			scriptInput.KeyDown += scriptInput_KeyDown;
			// 
			// btnLoadCopy
			// 
			btnLoadCopy.ButtonStyle = ButtonStyle.Alternate;
			btnLoadCopy.Cursor = Cursors.Hand;
			btnLoadCopy.Dock = DockStyle.Fill;
			btnLoadCopy.Location = new Point(1063, 706);
			btnLoadCopy.Margin = new Padding(10);
			btnLoadCopy.Name = "btnLoadCopy";
			btnLoadCopy.Size = new Size(331, 35);
			btnLoadCopy.TabIndex = 5;
			btnLoadCopy.Values.DropDownArrowColor = Color.Empty;
			btnLoadCopy.Values.Text = "Load Label";
			btnLoadCopy.Click += btnLoadCopy_Click;
			// 
			// btnSaveCopy
			// 
			btnSaveCopy.ButtonStyle = ButtonStyle.Alternate;
			btnSaveCopy.Cursor = Cursors.Hand;
			btnSaveCopy.Dock = DockStyle.Fill;
			btnSaveCopy.Enabled = false;
			btnSaveCopy.Location = new Point(712, 706);
			btnSaveCopy.Margin = new Padding(10);
			btnSaveCopy.Name = "btnSaveCopy";
			btnSaveCopy.Size = new Size(331, 35);
			btnSaveCopy.TabIndex = 4;
			btnSaveCopy.Values.DropDownArrowColor = Color.Empty;
			btnSaveCopy.Values.Text = "Save Label";
			btnSaveCopy.EnabledChanged += BtnEnabledChanged;
			btnSaveCopy.Click += btnSaveCopy_Click;
			// 
			// btnReset
			// 
			btnReset.ButtonStyle = ButtonStyle.Alternate;
			btnReset.Cursor = Cursors.Hand;
			btnReset.Dock = DockStyle.Fill;
			btnReset.Enabled = false;
			btnReset.Location = new Point(361, 706);
			btnReset.Margin = new Padding(10);
			btnReset.Name = "btnReset";
			btnReset.Size = new Size(331, 35);
			btnReset.TabIndex = 3;
			btnReset.Values.DropDownArrowColor = Color.Empty;
			btnReset.Values.Text = "Reset";
			btnReset.EnabledChanged += BtnEnabledChanged;
			btnReset.Click += btnReset_Click;
			// 
			// btnConvert
			// 
			btnConvert.ButtonStyle = ButtonStyle.Alternate;
			btnConvert.Cursor = Cursors.Hand;
			btnConvert.Dock = DockStyle.Fill;
			btnConvert.Enabled = false;
			btnConvert.Location = new Point(10, 706);
			btnConvert.Margin = new Padding(10);
			btnConvert.Name = "btnConvert";
			btnConvert.Size = new Size(331, 35);
			btnConvert.TabIndex = 2;
			btnConvert.Values.DropDownArrowColor = Color.Empty;
			btnConvert.Values.Text = "Convert";
			btnConvert.EnabledChanged += BtnEnabledChanged;
			btnConvert.Click += btnConvert_Click;
			// 
			// kryptonPanel1
			// 
			kryptonPanel1.Controls.Add(kryptonTableLayoutPanel1);
			kryptonPanel1.Dock = DockStyle.Fill;
			kryptonPanel1.Location = new Point(0, 0);
			kryptonPanel1.Name = "kryptonPanel1";
			kryptonPanel1.Padding = new Padding(10);
			kryptonPanel1.Size = new Size(1424, 771);
			kryptonPanel1.TabIndex = 9;
			// 
			// kryptonTableLayoutPanel1
			// 
			kryptonTableLayoutPanel1.ColumnCount = 4;
			kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			kryptonTableLayoutPanel1.Controls.Add(scriptInput, 0, 1);
			kryptonTableLayoutPanel1.Controls.Add(labelInput, 0, 0);
			kryptonTableLayoutPanel1.Controls.Add(btnLoadCopy, 3, 2);
			kryptonTableLayoutPanel1.Controls.Add(btnSaveCopy, 2, 2);
			kryptonTableLayoutPanel1.Controls.Add(btnConvert, 0, 2);
			kryptonTableLayoutPanel1.Controls.Add(btnReset, 1, 2);
			kryptonTableLayoutPanel1.Dock = DockStyle.Fill;
			kryptonTableLayoutPanel1.Location = new Point(10, 10);
			kryptonTableLayoutPanel1.Name = "kryptonTableLayoutPanel1";
			kryptonTableLayoutPanel1.RowCount = 3;
			kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 6.524634F));
			kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 86.1517944F));
			kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 7.190413F));
			kryptonTableLayoutPanel1.Size = new Size(1404, 751);
			kryptonTableLayoutPanel1.StateCommon.Color1 = Color.FromArgb(20, 20, 20);
			kryptonTableLayoutPanel1.StateNormal.Color1 = Color.FromArgb(20, 20, 20);
			kryptonTableLayoutPanel1.TabIndex = 6;
			// 
			// ScriptBuilder
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1424, 771);
			Controls.Add(kryptonPanel1);
			Icon = (Icon)resources.GetObject("$this.Icon");
			Name = "ScriptBuilder";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "MonoBuilder | Script Builder";
			Shown += ScriptBuilder_Shown;
			((System.ComponentModel.ISupportInitialize)kryptonPanel1).EndInit();
			kryptonPanel1.ResumeLayout(false);
			kryptonTableLayoutPanel1.ResumeLayout(false);
			kryptonTableLayoutPanel1.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private TextBox labelInput;
        private EnhancedRichTextBox scriptInput;
        private KryptonButton btnLoadCopy;
        private KryptonButton btnSaveCopy;
        private KryptonButton btnReset;
        private KryptonButton btnConvert;
        private KryptonPanel kryptonPanel1;
        private KryptonTableLayoutPanel kryptonTableLayoutPanel1;
    }
}