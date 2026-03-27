using MonoBuilder.CustomComponents;

namespace MonoBuilder.Screens.ScreenUtils
{
    partial class ScriptBuilderOutput
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptBuilderOutput));
			kryptonPanel1 = new Krypton.Toolkit.KryptonPanel();
			KTLP = new Krypton.Toolkit.KryptonTableLayoutPanel();
			btnReturn = new Krypton.Toolkit.KryptonButton();
			scriptOutput = new EnhancedRichTextBox();
			btnCopyClipboard = new Krypton.Toolkit.KryptonButton();
			btnAddScript = new Krypton.Toolkit.KryptonButton();
			((System.ComponentModel.ISupportInitialize)kryptonPanel1).BeginInit();
			kryptonPanel1.SuspendLayout();
			KTLP.SuspendLayout();
			SuspendLayout();
			// 
			// kryptonPanel1
			// 
			kryptonPanel1.Controls.Add(KTLP);
			kryptonPanel1.Dock = DockStyle.Fill;
			kryptonPanel1.Location = new Point(0, 0);
			kryptonPanel1.Name = "kryptonPanel1";
			kryptonPanel1.Size = new Size(986, 657);
			kryptonPanel1.TabIndex = 0;
			// 
			// KTLP
			// 
			KTLP.ColumnCount = 3;
			KTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
			KTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
			KTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
			KTLP.Controls.Add(btnReturn, 2, 1);
			KTLP.Controls.Add(scriptOutput, 0, 0);
			KTLP.Controls.Add(btnCopyClipboard, 0, 1);
			KTLP.Controls.Add(btnAddScript, 1, 1);
			KTLP.Dock = DockStyle.Fill;
			KTLP.Location = new Point(0, 0);
			KTLP.Name = "KTLP";
			KTLP.RowCount = 2;
			KTLP.RowStyles.Add(new RowStyle(SizeType.Percent, 91.78082F));
			KTLP.RowStyles.Add(new RowStyle(SizeType.Percent, 8.219178F));
			KTLP.Size = new Size(986, 657);
			KTLP.StateCommon.Color1 = Color.FromArgb(20, 20, 20);
			KTLP.StateNormal.Color1 = Color.FromArgb(20, 20, 20);
			KTLP.TabIndex = 10;
			// 
			// btnReturn
			// 
			btnReturn.ButtonStyle = Krypton.Toolkit.ButtonStyle.Custom2;
			btnReturn.Cursor = Cursors.Hand;
			btnReturn.Dock = DockStyle.Fill;
			btnReturn.Location = new Point(666, 613);
			btnReturn.Margin = new Padding(10);
			btnReturn.Name = "btnReturn";
			btnReturn.Size = new Size(310, 34);
			btnReturn.TabIndex = 10;
			btnReturn.Values.DropDownArrowColor = Color.Empty;
			btnReturn.Values.Text = "Return to Editor";
			btnReturn.Click += btnReturn_Click;
			// 
			// scriptOutput
			// 
			scriptOutput.AcceptsTab = true;
			scriptOutput.BackColor = Color.FromArgb(50, 50, 50);
			KTLP.SetColumnSpan(scriptOutput, 3);
			scriptOutput.DisabledBackColor = Color.FromArgb(50, 50, 50);
			scriptOutput.DisabledForeColor = Color.Gray;
			scriptOutput.Dock = DockStyle.Fill;
			scriptOutput.Font = new Font("Segoe UI", 12F);
			scriptOutput.ForeColor = Color.White;
			scriptOutput.Location = new Point(10, 10);
			scriptOutput.Margin = new Padding(10);
			scriptOutput.Name = "scriptOutput";
			scriptOutput.ReadOnly = true;
			scriptOutput.Size = new Size(966, 583);
			scriptOutput.TabIndex = 1;
			scriptOutput.Text = "";
			// 
			// btnCopyClipboard
			// 
			btnCopyClipboard.ButtonStyle = Krypton.Toolkit.ButtonStyle.Alternate;
			btnCopyClipboard.Cursor = Cursors.Hand;
			btnCopyClipboard.Dock = DockStyle.Fill;
			btnCopyClipboard.Location = new Point(10, 613);
			btnCopyClipboard.Margin = new Padding(10);
			btnCopyClipboard.Name = "btnCopyClipboard";
			btnCopyClipboard.Size = new Size(308, 34);
			btnCopyClipboard.TabIndex = 8;
			btnCopyClipboard.Values.DropDownArrowColor = Color.Empty;
			btnCopyClipboard.Values.Text = "Copt to Clipboard";
			btnCopyClipboard.Click += btnCopyClipboard_Click;
			// 
			// btnAddScript
			// 
			btnAddScript.ButtonStyle = Krypton.Toolkit.ButtonStyle.Alternate;
			btnAddScript.Cursor = Cursors.Hand;
			btnAddScript.Dock = DockStyle.Fill;
			btnAddScript.Enabled = false;
			btnAddScript.Location = new Point(338, 613);
			btnAddScript.Margin = new Padding(10);
			btnAddScript.Name = "btnAddScript";
			btnAddScript.Size = new Size(308, 34);
			btnAddScript.TabIndex = 9;
			btnAddScript.Values.DropDownArrowColor = Color.Empty;
			btnAddScript.Values.Text = "Add to Script";
			btnAddScript.Click += btnAddScript_Click;
			// 
			// ScriptBuilderOutput
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = btnReturn;
			ClientSize = new Size(986, 657);
			Controls.Add(kryptonPanel1);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			Name = "ScriptBuilderOutput";
			StartPosition = FormStartPosition.CenterParent;
			Text = "MonoBuilder | Script Output";
			((System.ComponentModel.ISupportInitialize)kryptonPanel1).EndInit();
			kryptonPanel1.ResumeLayout(false);
			KTLP.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private Krypton.Toolkit.KryptonPanel kryptonPanel1;
        private EnhancedRichTextBox scriptOutput;
        private Krypton.Toolkit.KryptonTableLayoutPanel KTLP;
        private Krypton.Toolkit.KryptonButton btnCopyClipboard;
        private Krypton.Toolkit.KryptonButton btnAddScript;
        private Krypton.Toolkit.KryptonButton btnReturn;
    }
}