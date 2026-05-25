using MonoBuilder.Models.generics.enums;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace MonoBuilder.Models.media_management
{
	public class MediaHandler : MonoSystem<Media>
	{
		private readonly AssetStore<Media> _music = new()
		{
			TypeName = "Music",
			MasterGuideContent = new(
				"Music",
				"// MUSIC_INSERTION_POINT",
				"// END_MUSIC_INSERTION_POINT")
		};

		private readonly AssetStore<Media> _sounds = new()
		{
			TypeName = "Sounds",
			MasterGuideContent = new(
				"Sounds",
				"// SOUNDS_INSERTION_POINT",
				"// END_SOUNDS_INSERTION_POINT")
		};

		private readonly AssetStore<Media> _voices = new()
		{
			TypeName = "Voices",
			MasterGuideContent = new(
				"Voices",
				"// VOICES_INSERTION_POINT",
				"// END_VOICES_INSERTION_POINT")
		};

		private readonly AssetStore<Media> _videos = new()
		{
			TypeName = "Videos",
			MasterGuideContent = new(
				"Videos",
				"// VIDEOS_INSERTION_POINT",
				"// END_VIDEOS_INSERTION_POINT")
		};

		public override AssetStore<Media> DataMode { get; set; }
		protected override string SaveString { get; } = "media";

		private static Regex MediaRegex { get; set; } = new(@"^([""'`]?)(?<name>.*)\1.*[:]*[""'`](?<content>.*)\1[,]?$", RegexOptions.Compiled);

		public MediaHandler()
		{
			AllDataModes = new()
			{
				[_music.TypeName.ToLower()] = _music,
				[_sounds.TypeName.ToLower()] = _sounds,
				[_voices.TypeName.ToLower()] = _voices,
				[_videos.TypeName.ToLower()] = _videos
			};

			DataMode = _music;

			LoadData(SaveString);
		}

		#region Handle File Data

		#region Data Loading Utilities
		protected override void LoadElement(XElement media, ObservableCollection<Media> collectionType, object? special = null)
		{
			string? name = (string?)media.Attribute("Name");
			string? path = (string?)media.Attribute("Path");
			string? fileKey = (string?)media.Attribute("FileKey") ?? string.Empty;
			_ = bool.TryParse((string?)media.Attribute("IsSynced"), out bool isSynced);

			if (name != null && path != null)
			{
				Media newMedia = new(name, path)
				{
					EntityID = collectionType.Count,
					FileKey = fileKey,
					IsSynced = isSynced
				};

				collectionType.Add(newMedia);
			}
		}

		protected override (AssetStore<Media>, List<XElement>)[] GetLoadData()
			=> [(_music, SystemData.Descendants("Song").ToList()),
				(_sounds, SystemData.Descendants("Sound").ToList()),
				(_voices, SystemData.Descendants("Voice").ToList()),
				(_videos, SystemData.Descendants("Video").ToList())];
		#endregion

		#region Data Saving Utilities
		private XElement SaveElement(string type, AssetStore<Media> store)
		{
			return new XElement(store.TypeName,
				store.Collection.Select(element => new XElement(type,
					new XAttribute("EntityID", element.EntityID),
					new XAttribute("Name", element.Name),
					new XAttribute("Path", element.Path),
					!string.IsNullOrEmpty(element.FileKey) ? new XAttribute("FileKey", element.FileKey) : null,
					new XAttribute("IsSynced", element.IsSynced)
					))
				);
		}

		protected override XElement[] GetSaveData()
			=> [SaveElement("Song", (AssetStore<Media>)GetDataMode("music")!),
				SaveElement("Sound", (AssetStore<Media>)GetDataMode("sounds")!),
				SaveElement("Voice", (AssetStore<Media>)GetDataMode("voices")!),
				SaveElement("Video", (AssetStore<Media>)GetDataMode("videos")!)];
		#endregion

		#endregion

		public override void AddEntityToScript(string name, Dictionary<string, string?> content, string? fileKeyParam = null)
		{
			throw new NotImplementedException();
		}

		public override bool CheckSynchronicity(bool showMessage = true)
		{
			throw new NotImplementedException();
		}

		public override Dictionary<string, string?> ConvertToScriptContent(Media type)
		{
			throw new NotImplementedException();
		}

		public override Dictionary<string, bool> EntitiesExistInScript(HashSet<string> names)
		{
			throw new NotImplementedException();
		}

		public override Dictionary<string, bool> EntityContentMatches(List<string> names, string? fileKey = null)
		{
			throw new NotImplementedException();
		}

		public override bool EntityExistsInScript(string name, string? fileKey = null)
		{
			throw new NotImplementedException();
		}

		public override bool RemoveEntitiesFromScript(int[] entitiyIds, bool shouldSave = true)
		{
			throw new NotImplementedException();
		}

		public override bool RemoveEntityFromScript(int entityId, bool shouldSave = true)
		{
			throw new NotImplementedException();
		}

		public override Dictionary<string, Media> SyncData(bool duplicatesOnly = false)
		{
			throw new NotImplementedException();
		}

		public override bool UpdateEntityInScript(string name, Dictionary<string, string?> content, string? fileKeyParam = null)
		{
			throw new NotImplementedException();
		}

		protected override string? ConvertToScriptContent(Dictionary<string, string?> content)
		{
			throw new NotImplementedException();
		}

		protected override void RemoveEntityFromSingleFile(string filePath, List<Media> entityToRemove)
		{
			throw new NotImplementedException();
		}
	}
}
