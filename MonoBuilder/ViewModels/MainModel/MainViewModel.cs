using MonoBuilder.Commands;
using MonoBuilder.Models;
using MonoBuilder.Models.character_management;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.helpers;
using MonoBuilder.Models.image_management;
using MonoBuilder.Models.media_management;
using MonoBuilder.Models.notification_management;
using MonoBuilder.Views;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace MonoBuilder.ViewModels.MainModel
{
	public class MainViewModel : BaseViewModel
	{
		public ObservableCollection<ButtonItem> BuilderButtons { get; }
		public ObservableCollection<UtilityButton> UtilityButtons { get; }

		private readonly Characters CharacterData = new();
		private readonly MonoImages ImageData = new();
		private readonly Notifications NotificationData = new();
		private readonly MediaHandler MediaData = new();
		private readonly AppSettings ApplicationSettings = new();
		private readonly FileSystemWatcher MainWatcher = new();
		private readonly ActionHelper ActionUtility = new();
		private readonly EventHelper EventUtility;
		private readonly ScriptConversion Converter;

		private readonly (string, MonoSystem)[] _systemsByStringName;

		public required Window Owner { get; set; }

		public RelayCommand OpenBuilderCommand { get; }
		public RelayCommand RunUtilityCommand { get; }

		public RelayCommand OpenVersionInfoCommand { get; }
		public RelayCommand OpenSteamStoreCommand { get; }

		public MainViewModel()
		{
			#region System Setup

			_systemsByStringName = [
				("Characters", CharacterData),
				("Images", ImageData),
				("Notifications", NotificationData)
			];

			ApplicationSettings.LoadDirectories();
			CharacterData.LoadSettings(ApplicationSettings);
			ImageData.LoadSettings(ApplicationSettings);
			NotificationData.LoadSettings(ApplicationSettings);
			MediaData.LoadSettings(ApplicationSettings);

			ActionUtility.LoadActions();

			EventUtility = new(_systemsByStringName);
			EventUtility.RegisterCollection("Characters", (CharacterData.GetDataMode("characters") as AssetStore<Character>)?.Collection ?? []);
			EventUtility.RegisterCollection("Images", (ImageData.GetDataMode("images") as AssetStore<MonoImage>)?.Collection ?? []);
			EventUtility.RegisterCollection("Scenes", (ImageData.GetDataMode("scenes") as AssetStore<MonoImage>)?.Collection ?? []);
			EventUtility.RegisterCollection("Gallery", (ImageData.GetDataMode("gallery") as AssetStore<MonoImage>)?.Collection ?? []);
			EventUtility.RegisterCollection("Messages", (NotificationData.GetDataMode("messages") as AssetStore<Notification>)?.Collection ?? []);
			EventUtility.RegisterCollection("Notifications", (NotificationData.GetDataMode("notifications") as AssetStore<Notification>)?.Collection ?? []);

			EventUtility.LoadEvents();

			Converter = new ScriptConversion(CharacterData, ActionUtility);

			Converter.SetIsFormattingColor(ApplicationSettings.GetColorFormatting());
			Converter.SetAutoSyncLabels(ApplicationSettings.GetAutoSyncLabels());
			Converter.ChangeIndentationAmount(ApplicationSettings.GetIndentationAmount());
			Converter.ChangeIndentationType(ApplicationSettings.GetIndentationType());

			InitializeMarkdown();
			InitializeActionsList();

			#endregion

			BuilderButtons = new()
			{
				new("Script Builder",		true, () => new ScriptBuilder(ApplicationSettings, Converter, ActionUtility)),
				new("Character Builder",	false),
				new("Image Builder",		true, () => new ImageBuilder(ApplicationSettings, ImageData, "Images")),
				new("Scene Builder",		true, () => new ImageBuilder(ApplicationSettings, ImageData, "Scenes")),
				new("Gallery Builder",		true, () => new ImageBuilder(ApplicationSettings, ImageData, "Gallery")),
				new("Media Builder",		true, () =>
				{
					var result = DialogBox.Show(
						"Choose an initial Media Builder...\n(You can change the builder type using the select box next to the close button.)",
						"Choose a Builder",
						600,
						DialogIcon.Information,
						new DialogButton("Music", DialogBoxResult.Continue),
						new DialogButton("Sounds", DialogBoxResult.Yes),
						new DialogButton("Voices", DialogBoxResult.OK),
						new DialogButton("Videos", DialogBoxResult.TryAgain),
						new DialogButton("Cancel", DialogBoxResult.Cancel, "ErrorButton"));

					return result == DialogBoxResult.Continue
						? new MediaBuilder(ApplicationSettings, MediaData, "Music")
						: result == DialogBoxResult.Yes
						? new MediaBuilder(ApplicationSettings, MediaData, "Sounds")
						: result == DialogBoxResult.OK
						? new MediaBuilder(ApplicationSettings, MediaData, "Voices")
						: result == DialogBoxResult.TryAgain
						? new MediaBuilder(ApplicationSettings, MediaData, "Videos")
						: null;
				}),
				new("Particle Builder",		false),
				new("Message Builder",		true, () => new NotificationBuilder(ApplicationSettings, NotificationData, "Messages")),
				new("Notification Builder",	true, () => new NotificationBuilder(ApplicationSettings, NotificationData, "Notifications"))
			}; // Particle Builder (⧉)

			UtilityButtons = new()
			{
				new("Assets/Cog.png", createWindow:  () => new Settings(CharacterData, ApplicationSettings, Converter))
					{ LabelText = "Settings" },
				new("Assets/Door.png", doAction: () => Application.Current.Shutdown())
					{ LabelText = "Exit" }
			};

			OpenBuilderCommand = new(OpenBuilder);
			RunUtilityCommand = new(OpenBuilder);

			OpenVersionInfoCommand = new(ExecuteOpenVersionInfoCommand);
			OpenSteamStoreCommand = new(ExecuteOpenSteamStoreCommand);
		}

		#region Initialization Methods
		public void InitializeWatcher()
		{
			FileWatcher.InitializeWatcher(
				MainWatcher,
				ApplicationSettings,
				Converter,
				CharacterData,
				MediaData,
				ImageData,
				NotificationData);
			FileWatcher.SetCurrentContext(Owner);

			if (CharacterData.CheckSynchronicity(true))
			{
				FileWatcher.ForciblyUpdateCharactersList(false);
			}

			if (ImageData.CheckSynchronicity(true))
			{
				FileWatcher.ForciblyUpdateImagesList(false);
			}

			if (NotificationData.CheckSynchronicity(true))
			{
				FileWatcher.ForciblyUpdateNotificationsList(false);
			}

			if (MediaData.CheckSynchronicity(true))
			{
				FileWatcher.ForciblyUpdateMediaList(false);
			}
		}

		private void InitializeMarkdown()
		{
			if (!Directory.Exists("data"))
			{
				Directory.CreateDirectory("data");
			}

			string fileName = "data/MonoBuilder.Markdown.xshd";
			Helpers.GenerateResourceIfMissing(fileName);
		}

		private void InitializeActionsList()
		{
			string fileName = "data/actions.xml";
			Helpers.GenerateResourceIfMissing(fileName);
		}
		#endregion

		#region Callers
		private void OpenBuilder(object? item)
		{
			if (item is ButtonItem button)
			{
				if (button?.CreateWindow == null) return;

				var window = button.CreateWindow();

				if (window != null)
				{
					SetupEnvironment(window);
					window.Show();
				}
			}

			if (item is UtilityButton utility)
			{
				if (utility?.CreateWindow != null)
				{
					var window = utility.CreateWindow();
					SetupEnvironment(window);
					window.Show();
				}

				if (utility?.DoAction != null)
				{
					utility.DoAction();
				}
			}
		}

		private void SetupEnvironment(Window window)
		{
			window.Owner = Owner;
			window.Loaded += (s, e) =>
			{
				Owner.Hide();
				FileWatcher.SetCurrentContext(window);
			};

			window.Closing += (s, e) =>
			{
				Owner.IsEnabled = true;
				Owner.Show();
				FileWatcher.SetCurrentContext(Owner);
			};

			Owner.IsEnabled = false;
		}

		private void ExecuteOpenVersionInfoCommand(object? _)
		{
			var title = Properties.Resources.Title;
			var program = Properties.Resources.Program;
			var version = Properties.Resources.Version;
			var description = Properties.Resources.Description;
			var developer = Properties.Resources.Developer;

			DialogBox.Show(
				@$"Title:		{title}
Program:	{program}
Version:		{version}
Description:	{description}

Developer:	{developer}

Originally, this program was designed and developed to make it easier to create the visual novel ""Symbiotic: Invasion"". However, it was decided early in development to turn it into an all-purpose dev tool that would help any developer, using Monogatari, build and manage their project in a user-friendly GUI.",
				"Version Information",
				600,
				DialogButtonDefaults.OK,
				DialogIcon.Information);
		}

		private void ExecuteOpenSteamStoreCommand(object? link)
		{
			var url = link as string;
			if (!string.IsNullOrEmpty(url))
			{
				Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
			}
		}
		#endregion
	}
}
