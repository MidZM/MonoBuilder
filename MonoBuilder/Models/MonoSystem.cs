using MonoBuilder.Models.character_management;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Models.helpers;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace MonoBuilder.Models
{
	public abstract class MonoSystem
	{
		protected AppSettings? ApplicationSettings { get; set; }
		protected XDocument SystemData { get; set; } = new();
		public virtual string DataModeTypeName => string.Empty;


		public virtual void LoadData(string type) {}
		public virtual void SaveData(string type) {}
	}

	public interface IDataModeController
	{
		void SetDataMode(string mode);
		IAssetStore? GetDataMode(string mode);
	}

	public abstract class MonoSystem<T> : MonoSystem, IDataModeController
		where T : INamedEntity, IMultiFile
	{
		public virtual AssetStore<T> DataMode { get; set; } = new()
		{
			TypeName = string.Empty,
			MasterGuideContent = new(
				string.Empty,
				string.Empty,
				string.Empty)
		};
		protected abstract string SaveString { get; }
		public string _SaveString => SaveString;

		public override string DataModeTypeName => DataMode.TypeName;
		protected Dictionary<string, AssetStore<T>> AllDataModes { get; set; } = new();

		#region Handle modes
		public void SetDataMode(string mode)
		{
			if (AllDataModes.TryGetValue(mode, out AssetStore<T>? store))
			{
				DataMode = store;
			}
		}

		public IAssetStore? GetDataMode(string mode)
		{
			if (AllDataModes.TryGetValue(mode, out AssetStore<T>? store))
			{
				return store;
			}

			return null;
		}
		#endregion

		#region Handle File Data

		#region Data Loading Utilities
		protected abstract void LoadElement(XElement element, ObservableCollection<T> collection, object? special = null);
		protected abstract (AssetStore<T>, List<XElement>)[] GetLoadData();
		protected virtual object? GetSpecialIdentifier() => null;
		#endregion

		#region Data Saving Utilities
		protected abstract XElement[] GetSaveData();
		#endregion

		public override void LoadData(string type)
		{
			try
			{
				if (!Directory.Exists("data")) Directory.CreateDirectory("data");
				if (!File.Exists($"data/{type}.xml"))
				{
					SaveData(type);
					return;
				}

				SystemData = XDocument.Load($"data/{type}.xml");
				List<(AssetStore<T>, List<XElement>)> descendentList = [..GetLoadData()];

				foreach (var (store, list) in descendentList)
				{
					store.Collection.Clear();
					foreach (var element in list)
						LoadElement(element, store.Collection, GetSpecialIdentifier());

					if (store.Collection.Any())
						store.NextId = store.Collection.Max(element => element.EntityID) + 1;

					RebuildLookups(store.TypeName.ToLower());
				}
			}
			catch (FileNotFoundException error) { DialogBox.Show($"Save Data Reading Failure!\r\n{error}", "Error", DialogButtonDefaults.OK, DialogIcon.Error); }
			catch (XmlException error) { DialogBox.Show($"{DataMode.TypeName} File Reading Failure!\r\n{error}", "Error", DialogButtonDefaults.OK, DialogIcon.Error); }
			catch (Exception error) { DialogBox.Show($"Something went wrong!\r\n{error}", "Error", DialogButtonDefaults.OK, DialogIcon.Error); }
		}

		public override void SaveData(string type)
		{
			SystemData = new XDocument(
				new XDeclaration("1.0", "utf-8", "yes"),
				new XElement("Root", GetSaveData()));

			SystemData.Save($"data/{type}.xml");
		}

		public void LoadSettings(AppSettings settings)
		{
			ApplicationSettings = settings;
		}

		protected Dictionary<string, string> GetDataFiles()
		{
			if (ApplicationSettings == null)
				return [];

			var files = ApplicationSettings.GetAllFilePaths(DataModeTypeName);
			if (files.Count == 0)
			{
				var legacyPath = ApplicationSettings.GetFilePath(DataModeTypeName);
				if (!string.IsNullOrEmpty(legacyPath))
					files[DataModeTypeName] = legacyPath;
			}

			return files;
		}

		private Dictionary<string, string> GetDataFiles(string name)
		{
			if (ApplicationSettings == null) return [];

			var files = ApplicationSettings.GetAllFilePaths(name);
			if (files.Count == 0)
			{
				var legacyPath = ApplicationSettings.GetFilePath(name);
				if (!string.IsNullOrEmpty(legacyPath))
					files[name] = legacyPath;
			}

			return files;
		}

		protected string? ResolveDataFileKey(string? fileKey = null)
		{
			var files = GetDataFiles();
			if (files.Count == 0)
				return null;

			if (!string.IsNullOrWhiteSpace(fileKey) && files.ContainsKey(fileKey))
				return fileKey;

			if (!string.IsNullOrWhiteSpace(fileKey))
			{
				var prefixed = files.Keys
					.OrderBy(key => key)
					.FirstOrDefault(key => key.StartsWith(fileKey + ":", StringComparison.Ordinal));

				if (prefixed != null)
					return prefixed;
			}

			return files.Keys.OrderBy(key => key).FirstOrDefault();
		}

		protected string? ResolveDataFilePath(string? fileKey, out string resolvedFileKey)
		{
			resolvedFileKey = ResolveDataFileKey(fileKey) ?? string.Empty;
			if (string.IsNullOrEmpty(resolvedFileKey))
				return null;

			return ApplicationSettings?.GetAllFilePaths().TryGetValue(resolvedFileKey, out string? filePath) == true
				? filePath
				: null;
		}

		protected string? ResolveDataFilePath(T? entity, out string resolvedFileKey)
		{
			return ResolveDataFilePath(entity?.FileKey, out resolvedFileKey);
		}

		public abstract Dictionary<string, T> SyncData(bool duplicatesOnly = false);
		#endregion

		#region Handle Data in The Program

		#region Duplicate Checking
		public bool CheckForDuplicates(T entityToCheck)
		{
			foreach (T type in DataMode.Collection)
			{
				if (type is Character character && entityToCheck is Character characterToCheck)
				{
					if (character.Tag == characterToCheck.Tag)
						return true;
				}
				else
				{
					if (type.Name == entityToCheck.Name)
						return true;
				}
			}

			return false;
		}
		public List<string> CheckForDuplicates(HashSet<T> entitiesToCheck)
		{
			List<string> list = new();
			foreach (T type in DataMode.Collection)
			{
				if (type is Character character && entitiesToCheck is HashSet<Character> charactersToCheck)
				{
					if (charactersToCheck.Contains(character))
						list.Add(character.Name);
				}
				else
				{
					if (entitiesToCheck.Contains(type))
						list.Add(type.Name);
				}
			}

			return list;
		}
		#endregion

		#region Reference Rebuilding
		public void RebuildLookups()
		{
			DataMode.ByName.Clear();
			DataMode.ById.Clear();

			foreach (var ent in DataMode.Collection)
			{
				if (ent is Character cha)
					DataMode.ByName[cha.Tag] = ent;
				else
					DataMode.ByName[ent.Name] = ent;

				DataMode.ById[ent.EntityID] = ent;
			}
		}
		protected void RebuildLookups(string type)
		{
			var dataMode = GetDataMode(type) as AssetStore<T>;

			dataMode?.ByName.Clear();
			dataMode?.ById.Clear();

			foreach (var ent in dataMode?.Collection ?? [])
			{
				if (ent is Character cha)
					dataMode?.ByName[cha.Tag] = ent;
				else
					dataMode?.ByName[ent.Name] = ent;

				dataMode?.ById[ent.EntityID] = ent;
			}
		}
		#endregion

		#region Data Manipulation
		public void AddData(T entity, bool shouldSave = true)
		{
			if (string.IsNullOrEmpty(entity.FileKey))
				entity.FileKey = ResolveDataFileKey(DataModeTypeName) ?? string.Empty;

			entity.EntityID = DataMode.NextId++;
			DataMode.Collection.Add(entity);

			DataMode.ByName[entity.Name] = entity;
			DataMode.ById[entity.EntityID] = entity;

			if (shouldSave) SaveData(SaveString);
		}

		public bool RemoveData(int entityId, bool shouldSave = true)
		{
			if (!DataMode.ById.TryGetValue(entityId, out var type))
				return false;

			DataMode.ById.Remove(entityId);
			DataMode.ByName.Remove(type.Name);
			DataMode.Collection.RemoveAt(DataMode.Collection.IndexOf(type));
			RebuildLookups();

			if (shouldSave) SaveData(SaveString);
			return true;
		}

		public void RemoveData(int[] entityIds, bool shouldSave = true)
		{
			if (entityIds.Length == 0)
				return;

			var idsToRemove = new HashSet<int>(entityIds);
			foreach (int id in idsToRemove)
			{
				if (DataMode.ById.TryGetValue(id, out var type))
				{
					DataMode.ByName.Remove(type.Name);
					DataMode.ById.Remove(id);
				}
			}

			DataMode.Collection.RemoveByIds(idsToRemove);

			if (shouldSave) SaveData(SaveString);
		}

		public T UpdateData(int typeId, T newData, bool shouldSave = true)
		{
			if (!DataMode.ById.TryGetValue(typeId, out var existing))
				throw new KeyNotFoundException($"{DataModeTypeName}: Entity ID {typeId} not found...");

			if (existing.Name != newData.Name)
			{
				DataMode.ByName.Remove(existing.Name);
			}

			DataMode.ByName[newData.Name] = newData;
			DataMode.ById[existing.EntityID] = newData;
			DataMode.Collection[DataMode.Collection.IndexOf(existing)] = newData;

			if (shouldSave) SaveData(SaveString);

			return existing;
		}
		#endregion

		#region Data Seeking
		public bool ContainsName(string name) => DataMode.ByName.ContainsKey(name);
		public T? CheckData(string entityName) => DataMode.ByName.TryGetValue(entityName, out var type) ? type : default;
		public T? CheckData(int entityId) => DataMode.ById.TryGetValue(entityId, out var type) ? type : default;
		#endregion

		#endregion

		#region Utility Methods
		protected (string, string) CheckKeyResolutionInFile(string? fileKeyParam, string name)
		{
			string? fileKey = fileKeyParam ?? CheckData(name)?.FileKey;
			var filePath = ResolveDataFilePath(fileKey, out string resolvedFileKey);

			if (filePath == null)
			{
				DialogBox.Show($"Failed to resolve file path for notifier \"{name}\".",
					"Bad File Path", DialogButtonDefaults.OK, DialogIcon.Error);
				throw new ArgumentOutOfRangeException($"Bad file data for {name}");
			}

			return (filePath, resolvedFileKey);
		}

		protected (int, int) GetPositionIndexInFile(List<string> lines, ContentGuide guide)
		{
			int startIndex = lines.FindIndex(l => l.Trim() == guide.GuideStart);
			int endIndex = lines.FindIndex(l => l.Trim() == guide.GuideEnd);

			if (startIndex == -1 || endIndex == -1)
			{
				DialogBox.Show(
					$"Missing proper {DataModeTypeName} section markers in script file.\n\n" +
					$"You need opening and close tags, such as:\n" +
					$"{guide.GuideStart}\n{AddIndentation()}\"ExampleKey\": \"ExampleValue\"\n{guide.GuideEnd}",
					$"Missing {DataModeTypeName} Section", DialogButtonDefaults.OK, DialogIcon.Error);
				throw new IndexOutOfRangeException($"Failed to find opening and closing tags in the selected file...");
			}

			return (startIndex, endIndex);
		}

		protected (int, int) GetPositionIndexInFile(List<string> lines, ContentGuide masterGuide, ContentGuide childGuide, string? name = null)
		{
			string masterGuideStart = masterGuide.GuideStart;
			string masterGuideEnd = masterGuide.GuideEnd;
			string childGuideStart = childGuide.GuideStart;
			string childGuideEnd = childGuide.GuideEnd;

			int startIndex = name == null
				? lines.FindIndex(l => l.Trim() == masterGuideStart)
				: lines.FindIndex(l => l.Contains(name) && l.TrimEnd().EndsWith(childGuideStart));
			int endIndex = name == null
				? lines.FindIndex(startIndex + 1, l => l.Trim() == masterGuideEnd)
				: lines.FindIndex(startIndex + 1, l => l.TrimEnd().EndsWith(childGuideEnd));

			if (startIndex == -1 || endIndex == -1)
			{
				DialogBox.Show(
					$"Missing proper {DataModeTypeName} section markers in script file.\n\n" +
					$"You need opening and closing tags, such as:\n" +
					$"{masterGuideStart}\n{AddIndentation()}\"Example\": {{ {childGuideStart}\n" +
					$"{AddIndentation()}{AddIndentation()}\"something\": \"...\",\n" +
					$"{AddIndentation()}}} {childGuideEnd}\n{masterGuideEnd}",
					$"Missing {DataModeTypeName} Section", DialogButtonDefaults.OK, DialogIcon.Error);

				string phrasing = name == null ? "base" : "element";
				throw new IndexOutOfRangeException($"Failed to find opening and closing {phrasing} tags in the selected file...");
			}

			return (startIndex, endIndex);
		}
		#endregion

		#region Handle Data in The Engine

		#region Script Utility
		protected string AddIndentation()
		{
			return ApplicationSettings?.GetIndentationType() switch
			{
				"Tabs" => "\t",
				"Spaces" => new string(' ', ApplicationSettings.GetIndentationAmount()),
				_ => new string(' ', 4)
			};
		}
		#endregion

		#region Existance Checks
		public abstract Dictionary<string, bool> EntitiesExistInScript(HashSet<string> names);
		public abstract bool EntityExistsInScript(string name, string? fileKey = null);
		public bool EntityExistsInScript(int entityId)
		{
			if (CheckData(entityId) is not T entity)
				return false;

			if (entity is Character character)
			{
				return EntityExistsInScript(character.Tag, character.FileKey);
			}

			return EntityExistsInScript(entity.Name, entity.FileKey);
		}
		public abstract Dictionary<string, bool> EntityContentMatches(List<string> names, string? fileKey = null);
		#endregion

		#region Script Conversion
		public abstract Dictionary<string, string?> ConvertToScriptContent(T type);
		protected abstract string? ConvertToScriptContent(Dictionary<string, string?> content);
		#endregion

		#region Script Manipulation
		public abstract void AddEntityToScript(string name, Dictionary<string, string?> content, string? fileKeyParam = null);
		public abstract bool RemoveEntityFromScript(int entityId, bool shouldSave = true);
		public abstract bool RemoveEntitiesFromScript(int[] entityIds, bool shouldSave = true);
		protected abstract void RemoveEntityFromSingleFile(string filePath, List<T> entityToRemove);
		public abstract bool UpdateEntityInScript(string name, Dictionary<string, string?> content, string? fileKeyParam = null);
		#endregion

		#region Synchronicity Checks
		public bool CheckSynchronicity(bool showMessage = true)
		{
			if (ApplicationSettings == null) return false;

			bool hasChanged = false;
			var modes = AllDataModes.Values;
			foreach (var mode in modes)
			{
				var files = GetDataFiles(mode.TypeName);
				if (files.Count == 0) return false;

				foreach (var (fileKey, _) in files)
				{
					var namesInFile = mode.Collection
						.Where(elm => elm.IsSynced &&
									string.IsNullOrEmpty(elm.FileKey)
										? fileKey == ResolveDataFileKey()
										: elm.FileKey == fileKey)
						.Select(elm =>
						{
							if (elm is Character character) return character.Tag;
							else return elm.Name;
						})
						.ToList();

					if (namesInFile.Count == 0) continue;

					var contentMatches = EntityContentMatches(namesInFile, fileKey);

					foreach (string name in namesInFile)
					{
						if (contentMatches.TryGetValue(name, out bool matches) && !matches)
						{
							hasChanged = true;
							break;
						}
					}

					if (hasChanged) break;
				}

				if (hasChanged && showMessage)
				{
					DialogBox.Show(
						"It looks like something changed from the last time the program was opened.\n" +
						$"{mode.TypeName} that have been modified will appear as such when opening the image builder.",
						"Changes Have Been Made",
						DialogButtonDefaults.OK,
						DialogIcon.Warning);
					break;
				}
			}

			return hasChanged;
		}
		#endregion

		#endregion
	}
}
