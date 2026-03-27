namespace MonoBuilder
{
    using Krypton.Toolkit;
    using Properties;

    partial class MonoBuilder
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MonoBuilder));
			KryptonPallet = new KryptonCustomPaletteBase(components);
			KryptonManager = new KryptonManager(components);
			ButtonPanel = new FlowLayoutPanel();
			middleButtons = new FlowLayoutPanel();
			kryptonButton1 = new KryptonButton();
			flowLayoutPanel1 = new FlowLayoutPanel();
			pictureBox1 = new PictureBox();
			pictureBox2 = new PictureBox();
			MainContainer = new KryptonPanel();
			VersionInfoLink = new KryptonLinkLabel();
			MainWatcher = new FileSystemWatcher();
			middleButtons.SuspendLayout();
			flowLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
			((System.ComponentModel.ISupportInitialize)MainContainer).BeginInit();
			MainContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)MainWatcher).BeginInit();
			SuspendLayout();
			// 
			// KryptonPallet
			// 
			KryptonPallet.ButtonStyles.ButtonAlternate.StateCommon.Back.Color1 = SystemColors.Window;
			KryptonPallet.ButtonStyles.ButtonAlternate.StateCommon.Back.ColorStyle = PaletteColorStyle.Solid;
			KryptonPallet.ButtonStyles.ButtonAlternate.StateCommon.Border.Rounding = 5F;
			KryptonPallet.ButtonStyles.ButtonAlternate.StateCommon.Border.Width = 3;
			KryptonPallet.ButtonStyles.ButtonAlternate.StateDisabled.Back.Color1 = SystemColors.ControlDark;
			KryptonPallet.ButtonStyles.ButtonAlternate.StateDisabled.Content.LongText.Color1 = Color.FromArgb(65, 65, 65);
			KryptonPallet.ButtonStyles.ButtonAlternate.StateDisabled.Content.ShortText.Color1 = Color.FromArgb(65, 65, 65);
			KryptonPallet.ButtonStyles.ButtonAlternate.StatePressed.Back.Color1 = SystemColors.ActiveCaption;
			KryptonPallet.ButtonStyles.ButtonAlternate.StatePressed.Border.Color1 = Color.Maroon;
			KryptonPallet.ButtonStyles.ButtonAlternate.StatePressed.Border.Color2 = Color.DarkRed;
			KryptonPallet.ButtonStyles.ButtonAlternate.StateTracking.Back.Color1 = SystemColors.InactiveCaption;
			KryptonPallet.ButtonStyles.ButtonAlternate.StateTracking.Border.Color1 = Color.Brown;
			KryptonPallet.ButtonStyles.ButtonAlternate.StateTracking.Border.Color2 = Color.Maroon;
			KryptonPallet.ButtonStyles.ButtonCustom1.OverrideDefault.Back.Color1 = Color.Maroon;
			KryptonPallet.ButtonStyles.ButtonCustom1.StateCheckedTracking.Back.Color1 = Color.Goldenrod;
			KryptonPallet.ButtonStyles.ButtonCustom1.StateCommon.Back.Color1 = Color.LimeGreen;
			KryptonPallet.ButtonStyles.ButtonCustom1.StateCommon.Back.ColorStyle = PaletteColorStyle.Solid;
			KryptonPallet.ButtonStyles.ButtonCustom1.StateCommon.Border.Rounding = 5F;
			KryptonPallet.ButtonStyles.ButtonCustom1.StateCommon.Border.Width = 2;
			KryptonPallet.ButtonStyles.ButtonCustom1.StateDisabled.Back.Color1 = Color.ForestGreen;
			KryptonPallet.ButtonStyles.ButtonCustom1.StatePressed.Back.Color1 = Color.DarkRed;
			KryptonPallet.ButtonStyles.ButtonCustom1.StateTracking.Back.Color1 = Color.Goldenrod;
			KryptonPallet.ButtonStyles.ButtonCustom1.StateTracking.Back.ColorStyle = PaletteColorStyle.Solid;
			KryptonPallet.ButtonStyles.ButtonCustom2.OverrideDefault.Back.Color1 = Color.FromArgb(150, 0, 0);
			KryptonPallet.ButtonStyles.ButtonCustom2.OverrideDefault.Border.Color1 = Color.FromArgb(80, 0, 0);
			KryptonPallet.ButtonStyles.ButtonCustom2.StateCheckedPressed.Border.Color1 = Color.FromArgb(80, 0, 0);
			KryptonPallet.ButtonStyles.ButtonCustom2.StateCommon.Back.Color1 = Color.FromArgb(150, 0, 0);
			KryptonPallet.ButtonStyles.ButtonCustom2.StateCommon.Back.ColorStyle = PaletteColorStyle.Solid;
			KryptonPallet.ButtonStyles.ButtonCustom2.StateCommon.Border.Color1 = Color.FromArgb(80, 0, 0);
			KryptonPallet.ButtonStyles.ButtonCustom2.StateCommon.Border.ColorStyle = PaletteColorStyle.Solid;
			KryptonPallet.ButtonStyles.ButtonCustom2.StateCommon.Border.Rounding = 5F;
			KryptonPallet.ButtonStyles.ButtonCustom2.StateCommon.Border.Width = 3;
			KryptonPallet.ButtonStyles.ButtonCustom2.StateCommon.Content.LongText.Color1 = Color.White;
			KryptonPallet.ButtonStyles.ButtonCustom2.StateCommon.Content.ShortText.Color1 = Color.White;
			KryptonPallet.ButtonStyles.ButtonCustom2.StateDisabled.Back.Color1 = Color.FromArgb(100, 0, 0);
			KryptonPallet.ButtonStyles.ButtonCustom2.StateDisabled.Border.Color1 = Color.FromArgb(50, 0, 0);
			KryptonPallet.ButtonStyles.ButtonCustom2.StateDisabled.Content.LongText.Color1 = SystemColors.GrayText;
			KryptonPallet.ButtonStyles.ButtonCustom2.StateDisabled.Content.ShortText.Color1 = SystemColors.GrayText;
			KryptonPallet.ButtonStyles.ButtonCustom2.StatePressed.Back.Color1 = Color.IndianRed;
			KryptonPallet.ButtonStyles.ButtonCustom2.StatePressed.Border.Color1 = Color.FromArgb(180, 0, 0);
			KryptonPallet.ButtonStyles.ButtonCustom2.StateTracking.Back.Color1 = Color.Brown;
			KryptonPallet.ButtonStyles.ButtonCustom2.StateTracking.Border.Color1 = Color.FromArgb(100, 0, 0);
			KryptonPallet.FormStyles.FormMain.StateCommon.Back.Color1 = Color.FromArgb(20, 20, 20);
			KryptonPallet.FormStyles.FormMain.StateInactive.Back.Color1 = Color.FromArgb(10, 10, 10);
			KryptonPallet.GridStyles.GridCommon.StateCommon.Background.Color1 = Color.FromArgb(50, 50, 50);
			KryptonPallet.GridStyles.GridCommon.StateCommon.BackStyle = PaletteBackStyle.GridBackgroundList;
			KryptonPallet.GridStyles.GridCommon.StateDisabled.Background.Color1 = Color.FromArgb(30, 30, 30);
			KryptonPallet.InputControlStyles.InputControlCommon.StateCommon.Border.Rounding = 3F;
			KryptonPallet.InputControlStyles.InputControlCustom1.StateCommon.Back.Color1 = Color.FromArgb(50, 50, 50);
			KryptonPallet.InputControlStyles.InputControlCustom1.StateCommon.Border.Rounding = 3F;
			KryptonPallet.InputControlStyles.InputControlCustom1.StateCommon.Border.Width = 2;
			KryptonPallet.InputControlStyles.InputControlCustom1.StateCommon.Content.LongText.Color1 = Color.White;
			KryptonPallet.InputControlStyles.InputControlCustom1.StateCommon.Content.Padding = new Padding(5);
			KryptonPallet.InputControlStyles.InputControlCustom1.StateCommon.Content.ShortText.Color1 = Color.White;
			KryptonPallet.InputControlStyles.InputControlCustom1.StateDisabled.Back.Color1 = Color.FromArgb(30, 30, 30);
			KryptonPallet.InputControlStyles.InputControlCustom1.StateDisabled.Content.LongText.Color1 = SystemColors.GrayText;
			KryptonPallet.InputControlStyles.InputControlCustom1.StateDisabled.Content.ShortText.Color1 = SystemColors.GrayText;
			KryptonPallet.LabelStyles.LabelCommon.StateCommon.LongText.Color1 = Color.White;
			KryptonPallet.LabelStyles.LabelCommon.StateCommon.ShortText.Color1 = Color.White;
			KryptonPallet.LabelStyles.LabelCommon.StateDisabled.LongText.Color1 = SystemColors.GrayText;
			KryptonPallet.LabelStyles.LabelCommon.StateDisabled.ShortText.Color1 = SystemColors.GrayText;
			KryptonPallet.LabelStyles.LabelCustom1.StateCommon.LongText.Color1 = Color.White;
			KryptonPallet.LabelStyles.LabelCustom1.StateCommon.ShortText.Color1 = Color.White;
			KryptonPallet.LabelStyles.LabelCustom1.StateDisabled.LongText.Color1 = SystemColors.GrayText;
			KryptonPallet.LabelStyles.LabelCustom1.StateDisabled.ShortText.Color1 = SystemColors.GrayText;
			KryptonPallet.LabelStyles.LabelNormalControl.StateCommon.ShortText.Color1 = Color.White;
			KryptonPallet.LabelStyles.LabelNormalControl.StateDisabled.LongText.Color1 = SystemColors.GrayText;
			KryptonPallet.LabelStyles.LabelNormalControl.StateDisabled.ShortText.Color1 = SystemColors.GrayText;
			KryptonPallet.PanelStyles.PanelClient.StateCommon.Color1 = Color.FromArgb(20, 20, 20);
			KryptonPallet.PanelStyles.PanelClient.StateDisabled.Color1 = Color.FromArgb(10, 10, 10);
			KryptonPallet.RippleEffect = true;
			KryptonPallet.UseThemeFormChromeBorderWidth = InheritBool.True;
			// 
			// KryptonManager
			// 
			KryptonManager.GlobalCustomPalette = KryptonPallet;
			KryptonManager.GlobalPaletteMode = PaletteMode.Custom;
			KryptonManager.ToolkitStrings.MessageBoxStrings.LessDetails = "L&ess Details...";
			KryptonManager.ToolkitStrings.MessageBoxStrings.MoreDetails = "&More Details...";
			// 
			// ButtonPanel
			// 
			ButtonPanel.AutoScroll = true;
			ButtonPanel.BackColor = Color.Transparent;
			ButtonPanel.Dock = DockStyle.Left;
			ButtonPanel.FlowDirection = FlowDirection.TopDown;
			ButtonPanel.Location = new Point(20, 20);
			ButtonPanel.Margin = new Padding(20);
			ButtonPanel.Name = "ButtonPanel";
			ButtonPanel.Size = new Size(338, 461);
			ButtonPanel.TabIndex = 0;
			ButtonPanel.WrapContents = false;
			// 
			// middleButtons
			// 
			middleButtons.AutoScroll = true;
			middleButtons.BackColor = Color.Transparent;
			middleButtons.Controls.Add(kryptonButton1);
			middleButtons.Dock = DockStyle.Fill;
			middleButtons.FlowDirection = FlowDirection.TopDown;
			middleButtons.Location = new Point(358, 20);
			middleButtons.Margin = new Padding(20);
			middleButtons.Name = "middleButtons";
			middleButtons.Padding = new Padding(20);
			middleButtons.Size = new Size(215, 461);
			middleButtons.TabIndex = 1;
			middleButtons.WrapContents = false;
			// 
			// kryptonButton1
			// 
			kryptonButton1.ButtonStyle = ButtonStyle.Alternate;
			kryptonButton1.Cursor = Cursors.Hand;
			kryptonButton1.Location = new Point(23, 23);
			kryptonButton1.Name = "kryptonButton1";
			kryptonButton1.Size = new Size(70, 70);
			kryptonButton1.TabIndex = 2;
			kryptonButton1.Values.DropDownArrowColor = Color.Empty;
			kryptonButton1.Values.Image = Resources.Cog;
			kryptonButton1.Values.Text = "";
			kryptonButton1.Click += btnOptions_Click;
			// 
			// flowLayoutPanel1
			// 
			flowLayoutPanel1.AutoScroll = true;
			flowLayoutPanel1.BackColor = Color.Transparent;
			flowLayoutPanel1.Controls.Add(pictureBox1);
			flowLayoutPanel1.Controls.Add(pictureBox2);
			flowLayoutPanel1.Dock = DockStyle.Right;
			flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanel1.Location = new Point(573, 20);
			flowLayoutPanel1.Margin = new Padding(20);
			flowLayoutPanel1.Name = "flowLayoutPanel1";
			flowLayoutPanel1.Size = new Size(351, 461);
			flowLayoutPanel1.TabIndex = 1;
			flowLayoutPanel1.WrapContents = false;
			// 
			// pictureBox1
			// 
			pictureBox1.BackColor = Color.FromArgb(20, 20, 20);
			pictureBox1.BackgroundImage = Resources.Zone_Master_Productions_Logo;
			pictureBox1.BackgroundImageLayout = ImageLayout.Zoom;
			pictureBox1.Location = new Point(3, 3);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new Size(345, 141);
			pictureBox1.TabIndex = 0;
			pictureBox1.TabStop = false;
			// 
			// pictureBox2
			// 
			pictureBox2.BackColor = Color.Black;
			pictureBox2.BackgroundImage = Resources.Symbiotic_Cover2;
			pictureBox2.BackgroundImageLayout = ImageLayout.Zoom;
			pictureBox2.BorderStyle = BorderStyle.FixedSingle;
			pictureBox2.Location = new Point(3, 150);
			pictureBox2.Name = "pictureBox2";
			pictureBox2.Size = new Size(345, 308);
			pictureBox2.TabIndex = 1;
			pictureBox2.TabStop = false;
			// 
			// MainContainer
			// 
			MainContainer.Controls.Add(VersionInfoLink);
			MainContainer.Controls.Add(middleButtons);
			MainContainer.Controls.Add(flowLayoutPanel1);
			MainContainer.Controls.Add(ButtonPanel);
			MainContainer.Dock = DockStyle.Fill;
			MainContainer.Location = new Point(0, 0);
			MainContainer.Name = "MainContainer";
			MainContainer.Padding = new Padding(20);
			MainContainer.Size = new Size(944, 501);
			MainContainer.TabIndex = 3;
			// 
			// VersionInfoLink
			// 
			VersionInfoLink.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			VersionInfoLink.LinkBehavior = KryptonLinkBehavior.HoverUnderline;
			VersionInfoLink.Location = new Point(829, 0);
			VersionInfoLink.Name = "VersionInfoLink";
			VersionInfoLink.OverrideFocus.DrawFocus = InheritBool.False;
			VersionInfoLink.OverrideNotVisited.ShortText.Color1 = Color.Red;
			VersionInfoLink.OverridePressed.ShortText.Color1 = Color.Maroon;
			VersionInfoLink.OverrideVisited.ShortText.Color1 = Color.Red;
			VersionInfoLink.Size = new Size(119, 35);
			VersionInfoLink.TabIndex = 2;
			VersionInfoLink.Values.Text = "Version Information";
			VersionInfoLink.LinkClicked += VersionInfo_Clicked;
			// 
			// MainWatcher
			// 
			MainWatcher.EnableRaisingEvents = true;
			MainWatcher.IncludeSubdirectories = true;
			MainWatcher.SynchronizingObject = this;
			// 
			// MonoBuilder
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(944, 501);
			Controls.Add(MainContainer);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MinimumSize = new Size(920, 540);
			Name = "MonoBuilder";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "MonoBuilder";
			Load += MonoBuilder_Load;
			middleButtons.ResumeLayout(false);
			flowLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
			((System.ComponentModel.ISupportInitialize)MainContainer).EndInit();
			MainContainer.ResumeLayout(false);
			MainContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)MainWatcher).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private Krypton.Toolkit.KryptonCustomPaletteBase KryptonPallet;
        private Krypton.Toolkit.KryptonManager KryptonManager;
        private FlowLayoutPanel ButtonPanel;
        private FlowLayoutPanel middleButtons;
        private FlowLayoutPanel flowLayoutPanel1;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private KryptonPanel MainContainer;
        private KryptonButton kryptonButton1;
        private FileSystemWatcher MainWatcher;
        private KryptonLinkLabel VersionInfoLink;
    }
}
