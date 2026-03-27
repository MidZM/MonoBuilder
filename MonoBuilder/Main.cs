using Krypton.Toolkit;
using MonoBuilder.Screens;
using MonoBuilder.Screens.ScreenUtils;
using MonoBuilder.Utils;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MonoBuilder
{
	public partial class MonoBuilder : KryptonForm
	{
		private Characters CharacterData { get; } = new Characters();
		private AppSettings ApplicationSettings { get; } = new AppSettings();
		private ScriptConversion Converter { get; set; }
		private KryptonButton[] ButtonData { get; set; } = [];
		private Dictionary<string, Action<MonoBuilder>> ScriptFunctions { get; set; }

		private record ButtonStyles
		{
			public int Margin { get; } = 5;
		}

		public MonoBuilder()
		{
			ApplicationSettings.LoadDirectories();
			CharacterData.LoadSettings(ApplicationSettings);
			Converter = new ScriptConversion(CharacterData);

			Converter.SetIsFormattingColor(ApplicationSettings.GetColorFormatting());
			Converter.SetAutoSyncLabels(ApplicationSettings.GetAutoSyncLabels());
			Converter.ChangeIndentationAmount(ApplicationSettings.GetIndentationAmount());
			Converter.ChangeIndentationType(ApplicationSettings.GetIndentationType());

			InitializeComponent();
			SetupButtonPanel();
			InitializeWatcher();

			ScriptFunctions = new Dictionary<string, Action<MonoBuilder>>
			{
				{
					"btnScript",
					(self) =>
					{
						using (ScriptBuilder ScriptBuilder = new ScriptBuilder(CharacterData, ApplicationSettings, Converter))
						{
							var marginWidth = (self.Width / 2) - 100;
							var marginHeight = (self.Height / 2) - 100;
							var transitioner = new ScreenTransitioner(
								location: new Point(marginWidth, marginHeight),
								width: 100,
								height: 100,
								font: new Font(FontFamily.GenericSansSerif, 20, FontStyle.Bold),
								foreColor: Color.White,
								backColor: Color.FromArgb(5, 5, 5));

							var panel = transitioner.Show();
							self.Controls.Add(panel);
							panel.BringToFront();

							ScriptBuilder.Load += (s, e) =>
							{
								self.Hide();
								self.Controls.Remove(panel);
								transitioner.Hide();
								FileWatcher.SetCurrentContext(ScriptBuilder);
							};
							ScriptBuilder.FormClosed += (s, e) =>
							{
								self.Enabled = true;
								self.Show();
								FileWatcher.SetCurrentContext(self);
							};

							self.Enabled = false;
							ScriptBuilder.ShowDialog();
						}
					}
				}
			};
		}

		private void SetupButtonPanel()
		{
			var buttonStyles = new ButtonStyles();
			Padding margin = new Padding(all: buttonStyles.Margin);

			ButtonData = [
					new KryptonButton { Text = "Script Builder", Name = "btnScript",       Enabled = true, ButtonStyle = ButtonStyle.Alternate, Margin = margin, Cursor = Cursors.Hand },
					new KryptonButton { Text = "Character Builder", Name = "btnChar",      Enabled = false, ButtonStyle = ButtonStyle.Alternate, Margin = margin, Cursor = Cursors.No },
					new KryptonButton { Text = "Image Builder", Name = "btnImg",           Enabled = false, ButtonStyle = ButtonStyle.Alternate, Margin = margin, Cursor = Cursors.No },
					new KryptonButton { Text = "Scene Builder", Name = "btnScene",         Enabled = false, ButtonStyle = ButtonStyle.Alternate, Margin = margin, Cursor = Cursors.No },
					new KryptonButton { Text = "Audio Builder", Name = "btnAudio",         Enabled = false, ButtonStyle = ButtonStyle.Alternate, Margin = margin, Cursor = Cursors.No },
					new KryptonButton { Text = "Particle Builder", Name = "btnParticle",   Enabled = false, ButtonStyle = ButtonStyle.Alternate, Margin = margin, Cursor = Cursors.No },
					new KryptonButton { Text = "Message Builder", Name = "btnMsg",         Enabled = false, ButtonStyle = ButtonStyle.Alternate, Margin = margin, Cursor = Cursors.No },
					new KryptonButton { Text = "Notification Builder", Name = "btnNotif",  Enabled = false, ButtonStyle = ButtonStyle.Alternate, Margin = margin, Cursor = Cursors.No },
				];


			ButtonPanel.Controls.AddRange(ButtonData);
			ButtonPanel.Resize += (s, e) => ResizeButtons(buttonStyles);
			middleButtons.Resize += (s, e) => CenterContent();
			ResizeButtons(buttonStyles);
			CenterContent();
		}

		private void InitializeWatcher()
		{
			FileWatcher.InitializeWatcher(MainWatcher, ApplicationSettings, Converter, CharacterData);
			FileWatcher.SetCurrentContext(this);
		}

		private void ResizeButtons(ButtonStyles styles)
		{
			if (ButtonData.Length == 0) return;

			ButtonPanel.SuspendLayout();

			int buttonHeight = Math.Clamp(ButtonPanel.ClientSize.Height / ButtonData.Length, 45, 60);

			foreach (KryptonButton button in ButtonData)
			{
				button.Width = ButtonPanel.ClientSize.Width - 10;
				button.Height = buttonHeight - (styles.Margin * 2);
			}

			ButtonPanel.ResumeLayout(false);
		}

		private void CenterContent()
		{
			int totalControlWidth = 0;

			foreach (Control control in middleButtons.Controls)
			{
				totalControlWidth += control.Width + control.Margin.Left + control.Margin.Right;
			}

			if (totalControlWidth < middleButtons.Width)
			{
				int leftPadding = (middleButtons.Width - totalControlWidth) / 2;
				middleButtons.Padding = new Padding(leftPadding, 0, 0, 0);
			}
			else
			{
				middleButtons.Padding = new Padding(0, 0, 0, 0);
			}
		}

		private void MonoBuilder_Load(object sender, EventArgs e)
		{

			foreach (var button in ButtonData)
			{
				if (ScriptFunctions.ContainsKey(button.Name) && ScriptFunctions[button.Name] != null)
				{
					button.Click += (s, e) => ScriptFunctions[button.Name](this);
				}
			}
		}

		private void btnOptions_Click(object sender, EventArgs e)
		{
			using (Settings SettingsScreen = new Settings(CharacterData, ApplicationSettings, Converter))
			{
				var marginWidth = this.Width / 2 - 100;
				var marginHeight = this.Height / 2 - 100;
				var transitioner = new ScreenTransitioner(
					location: new Point(marginWidth, marginHeight),
					width: 100,
					height: 100,
					font: new Font(FontFamily.GenericSansSerif, 20, FontStyle.Bold),
					foreColor: Color.White,
					backColor: Color.FromArgb(5, 5, 5));

				var panel = transitioner.Show();
				this.Controls.Add(panel);
				panel.BringToFront();

				SettingsScreen.Load += (s, e) =>
				{
					this.Hide();
					this.Controls.Remove(panel);
					transitioner.Hide();
					FileWatcher.SetCurrentContext(SettingsScreen);
				};

				SettingsScreen.FormClosed += (s, e) =>
				{
					this.Enabled = true;
					this.Show();
					FileWatcher.SetCurrentContext(this);
				};

				this.Enabled = false;
				SettingsScreen.ShowDialog();
			}
		}

		private void VersionInfo_Clicked(object sender, EventArgs e)
		{
			var title = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
			var program = Application.ProductName;
			var version = Application.ProductVersion;
			var company = Application.CompanyName;
			var description = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

			DialogBox.Show(
				@$"Title:                     {title}
					Program:             {program}
					Version:                {version}
					Description:        {description}
					
					Developer:           {company}

					This program was designed and developed to make it easier to create the visual novel ""Symbiotic: Invasion"". However, it was decided early in development to turn it into an all-purpose dev tool that would help any developer using Monogatari build and manage their project in a user-friendly GUI.",
				"Version Information",
				DialogButtonDefaults.OK,
				DialogIcon.Information);

		}
	}
}
