using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.helpers;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace MonoBuilder.Models.notification_management
{
	public class Notifications : MonoSystem<Notification>
	{
		private AssetStore<Notification> _messages = new()
		{
			TypeName = "Messages",
			TypeBody = "MessageType",
			MasterGuideContent = new(
				"Messages",
				"// MESSAGES_INSERTION_POINT",
				"// END_MESSAGES_INSERTION_POINT"),
			ChildGuideContent = new(
				"MessageType",
				"// MESSAGE_START",
				"// MESSAGE_END")
		};
		private AssetStore<Notification> _notifications = new()
		{
			TypeName = "Notifications",
			TypeBody = "NotificationType",
			MasterGuideContent = new(
				"Notifications",
				"// NOTIFICATIONS_INSERTION_POINT",
				"// END_NOTIFICATIONS_INSERTION_POINT"),
			ChildGuideContent = new(
				"NotificationType",
				"// NOTIFICATION_START",
				"// NOTIFICATION_END")
		};

		public override AssetStore<Notification> DataMode { get; set; }
		protected override string SaveString { get; } = "notifications";

		private static Regex BodyRegex = new(@"^([""'`]?)(?<id>.*)\1.*[:]*[\{] (// MESSAGE_START|// NOTIFICATION_START)$", RegexOptions.Compiled);
		private static Regex AttributeRegex = new(@"^([""'`]?)(?<key>.*)\1[:] (?<attr>.*)$", RegexOptions.Compiled);

		public Notifications()
		{
			AllDataModes = new()
			{
				[_messages.TypeName.ToLower()] = _messages,
				[_notifications.TypeName.ToLower()] = _notifications
			};

			DataMode = _messages;

			LoadData(SaveString);
		}

		#region Handle File Data

		#region Data Loading Utilities
		protected override void LoadElement(XElement element, ObservableCollection<Notification> collectionType, object? type)
		{
			string? notificationID = (string?)element.Attribute("NotificationID");
			string? title = (string?)element.Attribute("Title");
			string? subtitle = (string?)element.Attribute("Subtitle");
			string? icon = (string?)element.Attribute("Icon");
			string? closeAction = (string?)element.Attribute("CloseAction");
			string fileKey = (string?)element.Attribute("FileKey") ?? string.Empty;
			_ = bool.TryParse((string?)element.Attribute("IsSynced"), out bool isSynced);
			string? body = element.Value;

			var notifierType = (NotifierType?)type ?? NotifierType.Message;
			if (notificationID != null)
			{
				Notification newNotification = new(notificationID, title, icon, notifierType)
				{
					EntityID = collectionType.Count,
					FileKey = fileKey,
					IsSynced = isSynced,
					Body = body
				};

				if (notifierType == NotifierType.Message && subtitle != null)
				{
					newNotification.Subtitle = subtitle;
					newNotification.CloseAction = closeAction;
				}
				else if (notifierType == NotifierType.Notification && icon != null)
				{
					newNotification.Icon = icon;
				}

				collectionType.Add(newNotification);
			}
		}

		protected override (AssetStore<Notification>, List<XElement>)[] GetLoadData()
			=> [(_messages, SystemData.Descendants("Message").ToList()),
				(_notifications, SystemData.Descendants("Notification").ToList())];
		protected override object? GetSpecialIdentifier()
			=> DataModeTypeName == "Messages" ? NotifierType.Message : NotifierType.Notification;
		#endregion

		#region Data Saving Utilities
		private XElement SaveElement(string type, AssetStore<Notification> store, NotifierType notifierType)
		{
			return new XElement(store.TypeName,
				store.Collection.Select(element => new XElement(type,
					new XAttribute("EntityID", element.EntityID),
					new XAttribute("NotificationID", element.Name),
					new XAttribute("Title", element.Title ?? string.Empty),
					notifierType == NotifierType.Message
						? new XAttribute("Subtitle", element.Subtitle ?? string.Empty)
						: new XAttribute("Icon", element.Icon ?? string.Empty),
					notifierType == NotifierType.Message
						? new XAttribute("CloseAction", element.CloseAction ?? string.Empty)
						: null,
					!string.IsNullOrEmpty(element.FileKey)
						? new XAttribute("FileKey", element.FileKey)
						: null,
					new XAttribute("IsSynced", element.IsSynced),
					new XCData(element.Body ?? string.Empty))));
		}

		protected override XElement[] GetSaveData()
			=> [SaveElement("Message", (AssetStore<Notification>)GetDataMode("messages")!, NotifierType.Message),
				SaveElement("Notification", (AssetStore<Notification>)GetDataMode("notifications")!, NotifierType.Notification)];
		#endregion

		#endregion

		#region Sync Notifiers to the Program
		public override Dictionary<string, Notification> SyncData(bool duplicatesOnly = false)
		{
			var notifications = new Dictionary<string, Notification>(StringComparer.Ordinal);
			var files = GetDataFiles();

			if (files.Count == 0)
			{
				DialogBox.Show(
					"Something has gone wrong while building notification data!\n\nNo notification files are configured.",
					"Failed to Compile Notification Data",
					DialogButtonDefaults.OK,
					DialogIcon.Error);
				throw new Exception("Bad file data...\nNo notification files are configured.");
			}

			var type = DataMode.TypeName;
			var masterGuide = DataMode.MasterGuideContent;
			var childGuide = DataMode.ChildGuideContent!;
			var notifierType = type == "Messages" ? NotifierType.Message : NotifierType.Notification;

			foreach (var (fileKey, filePath) in files.OrderBy(entry => entry.Key))
			{
				string[] content = File.ReadAllLines(filePath);
				int start = Array.FindIndex(content, line => line.Trim() == masterGuide.GuideStart);
				int end = Array.FindIndex(content, start + 1, line => line.Trim() == masterGuide.GuideEnd);

				if (start <= -1 || end <= -1)
					continue;

				string currentId = string.Empty;
				var parsedAttributes = new Dictionary<string, string>();

				// === CHANGED TO for LOOP FOR MULTI-LINE SUPPORT ===
				for (int i = start + 1; i < end; i++)
				{
					string rawData = content[i];
					string line = rawData.Trim();

					var bodyResult = BodyRegex.Match(line);
					bool isBlockEnd = line.StartsWith("} " + childGuide.GuideEnd) ||
									  line.StartsWith("}, " + childGuide.GuideEnd);

					if (bodyResult.Success)
					{
						currentId = bodyResult.Groups["id"].Value;
						parsedAttributes.Clear();

						try
						{
							bool isDuplicate = ContainsName(currentId);
							if (!duplicatesOnly || isDuplicate)
							{
								notifications[currentId] = new Notification(currentId, null, null, notifierType)
								{
									FileKey = fileKey
								};
							}
						}
						catch (Exception error)
						{
							currentId = string.Empty;
							DialogBox.Show(
								$"Attempted to add notification data without a proper ID.\n\n{error}",
								"Bad Notification Data",
								DialogButtonDefaults.OK,
								DialogIcon.Error);
						}
						continue;
					}

					if (currentId != string.Empty && notifications.ContainsKey(currentId))
					{
						if (isBlockEnd)
						{
							var notif = notifications[currentId];
							parsedAttributes.TryGetValue("title", out string? title);
							parsedAttributes.TryGetValue("subtitle", out string? subtitle);
							parsedAttributes.TryGetValue("body", out string? body);
							parsedAttributes.TryGetValue("icon", out string? icon);
							parsedAttributes.TryGetValue("actionString", out string? actionString);

							notif.Title = title;
							notif.Subtitle = subtitle;
							notif.Body = body;
							notif.Icon = icon;
							notif.CloseAction = actionString;

							currentId = string.Empty;
							parsedAttributes.Clear();
							continue;
						}

						// === MULTI-LINE ATTRIBUTE PARSING ===
						string attrLine = line.EndsWith(',') ? line[..^1] : line;
						var attrRes = AttributeRegex.Match(attrLine);

						if (attrRes.Success)
						{
							string key = attrRes.Groups["key"].Value.Trim('"', '\'', '`');
							string valuePart = attrRes.Groups["attr"].Value.Trim();

							string finalValue;

							if (valuePart.StartsWith('`'))
							{
								// === MULTI-LINE BACKTICK VALUE ===
								var sb = new StringBuilder(valuePart[1..]); // remove opening `

								i++; // move to next line
								bool foundClosing = false;

								while (i < end && !foundClosing)
								{
									string nextRaw = content[i];
									string nextLine = nextRaw.Trim(); // preserve internal whitespace/newlines

									int closePos = nextLine.IndexOf('`');
									if (closePos >= 0)
									{
										sb.Append(nextLine.Substring(0, closePos));
										foundClosing = true;
										// Do NOT increment i here — the outer loop will handle the next iteration
									}
									else
									{
										sb.AppendLine(nextLine); // preserves original formatting
									}

									if (!foundClosing)
										i++;
								}

								finalValue = sb.ToString().Trim();
								if (!foundClosing)
								{
									// Optional: log warning or append closing backtick
									// finalValue += "`";
								}
							}
							else if (valuePart.Length >= 2 &&
									 ((valuePart.StartsWith('"') && valuePart.EndsWith('"')) ||
									  (valuePart.StartsWith('\'') && valuePart.EndsWith('\'')) ||
									  (valuePart.StartsWith('`') && valuePart.EndsWith('`'))))
							{
								finalValue = valuePart[1..^1];
							}
							else
							{
								finalValue = valuePart;
							}

							parsedAttributes[key] = finalValue;
						}
					}
				}
			}

			return notifications;
		}
		#endregion

		#region Handle Notifier Data in Files
		private string ProcessAttributeLine(string key, string? value)
		{
			if (value != null && value.Contains('\n'))
			{
				string indent = AddIndentation();
				return $"\"{key}\": `\n{indent}{indent}{indent}{value.Replace("\n", $"\n{indent}{indent}{indent}")}\n{indent}{indent}`";
			}

			return $"\"{key}\": \"{value?.Replace("\"", "\\\"") ?? string.Empty}\"";
		}

		#region Entity Checking Methods
		public override Dictionary<string, bool> EntitiesExistInScript(HashSet<string> names)
		{
			var files = GetDataFiles();
			if (files.Count == 0)
			{
				DialogBox.Show("Attempted to check notification existence without a proper file path!",
					"No File Path", DialogButtonDefaults.OK, DialogIcon.Warning);
				return [];
			}

			var masterGuide = DataMode.MasterGuideContent;
			var namesInScript = names.ToDictionary(name => name, _ => false);

			foreach (var (_, filePath) in files)
			{
				string[] lines = File.ReadAllLines(filePath);
				bool inSection = false;

				foreach (string line in lines)
				{
					string trimmed = line.Trim();

					if (trimmed == masterGuide.GuideStart) { inSection = true; continue; }
					if (inSection && trimmed == masterGuide.GuideEnd) break;

					if (inSection)
					{
						var match = BodyRegex.Match(trimmed);
						if (match.Success && names.Contains(match.Groups["id"].Value))
							namesInScript[match.Groups["id"].Value] = true;
					}
				}
			}

			return namesInScript;
		}

		public override bool EntityExistsInScript(string name, string? fileKey = null)
		{
			var files = GetDataFiles();
			if (files.Count == 0)
			{
				DialogBox.Show("Attempted to check notification existence without a proper file path!",
					"No File Path", DialogButtonDefaults.OK, DialogIcon.Warning);
				return false;
			}

			var type = DataMode.TypeName;
			var masterGuide = DataMode.MasterGuideContent;
			var filesToCheck = string.IsNullOrWhiteSpace(fileKey)
				? files.OrderBy(entry => entry.Key)
				: files.Where(entry => entry.Key == ResolveDataFileKey(fileKey));

			foreach (var (_, filePath) in filesToCheck)
			{
				string[] lines = File.ReadAllLines(filePath);
				bool inSection = false;

				foreach (string line in lines)
				{
					string trimmed = line.Trim();

					if (trimmed == masterGuide.GuideStart) { inSection = true; continue; }
					if (inSection && trimmed == masterGuide.GuideEnd) break;

					if (inSection)
					{
						var match = BodyRegex.Match(trimmed);
						if (match.Success && match.Groups["id"].Value == name)
							return true;
					}
				}
			}

			return false;
		}

		public override Dictionary<string, bool> EntityContentMatches(List<string> names, string? fileKey = null)
		{
			var results = names.ToDictionary(n => n, _ => false);
			var remaining = new HashSet<string>(names);

			var filePath = ResolveDataFilePath(fileKey, out _);
			if (filePath == null || names.Count == 0)
				return results;

			var masterGuide = DataMode.MasterGuideContent;
			var childGuide = DataMode.ChildGuideContent!;
			string[] fileContent = File.ReadAllLines(filePath);
			int start = Array.FindIndex(fileContent, line => line.Trim() == masterGuide.GuideStart);
			int end = Array.FindIndex(fileContent, start + 1, line => line.Trim() == masterGuide.GuideEnd);

			if (start == -1 || end == -1)
				return results;

			string currentId = string.Empty;
			var parsedAttributes = new Dictionary<string, string>();

			for (int i = start + 1; i < end; i++)
			{
				if (remaining.Count == 0) break;

				string rawLine = fileContent[i];
				string line = rawLine.Trim();
				var bodyMatch = BodyRegex.Match(line);
				bool isBlockEnd = line.StartsWith("} " + childGuide.GuideEnd) ||
								  line.StartsWith("}, " + childGuide.GuideEnd);

				if (bodyMatch.Success)
				{
					currentId = bodyMatch.Groups["id"].Value;
					parsedAttributes.Clear();
					continue;
				}

				if (currentId != string.Empty && remaining.Contains(currentId))
				{
					if (isBlockEnd)
					{
						if (CheckData(currentId) is Notification notif)
						{
							parsedAttributes.TryGetValue("title", out string? title);
							parsedAttributes.TryGetValue("subtitle", out string? subtitle);
							parsedAttributes.TryGetValue("body", out string? body);
							parsedAttributes.TryGetValue("icon", out string? icon);
							parsedAttributes.TryGetValue("actionString", out string? actionString);

							bool bodyCheck = string.Equals(
								(body ?? string.Empty).Replace("\r\n", "\n"),
								(notif.Body ?? string.Empty).Replace("\r\n", "\n"),
								StringComparison.Ordinal);

							results[currentId] =
								(title ?? string.Empty) == (notif.Title ?? string.Empty) &&
								(subtitle ?? string.Empty) == (notif.Subtitle ?? string.Empty) &&
								(icon ?? string.Empty) == (notif.Icon ?? string.Empty) &&
								(actionString ?? string.Empty) == (notif.CloseAction ?? string.Empty) &&
								bodyCheck;
						}

						currentId = string.Empty;
						parsedAttributes.Clear();
						continue;
					}

					string attrLine = line.EndsWith(',') ? line[..^1] : line;
					var attrRes = AttributeRegex.Match(attrLine);

					if (attrRes.Success)
					{
						string key = attrRes.Groups["key"].Value.Trim('"', '\'', '`');
						string valuePart = attrRes.Groups["attr"].Value.Trim();

						string finalValue;

						if (valuePart.StartsWith('`'))
						{
							var sb = new StringBuilder(valuePart[1..]);

							i++;
							bool foundClosing = false;

							while (i < end && !foundClosing)
							{
								string nextRaw = fileContent[i];
								string nextLine = nextRaw.Trim();

								int closePos = nextLine.IndexOf('`');
								if (closePos >= 0)
								{
									sb.Append(nextLine.Substring(0, closePos));
									foundClosing = true;
								}
								else
								{
									sb.AppendLine(nextLine);
								}

								if (!foundClosing)
									i++;
							}

							finalValue = sb.ToString().Trim();
							//if (!foundClosing)
							//{
							// Optional: log warning or append closing backtick
							//}
						}
						else if (valuePart.Length >= 2 &&
								 ((valuePart.StartsWith('"') && valuePart.EndsWith('"')) ||
								  (valuePart.StartsWith('\'') && valuePart.EndsWith('\'')) ||
								  (valuePart.StartsWith('`') && valuePart.EndsWith('`'))))
						{
							finalValue = valuePart[1..^1];
						}
						else
						{
							finalValue = valuePart;
						}

						parsedAttributes[key] = finalValue;
					}
				}
			}

			return results;
		}
		#endregion

		#region Conversion Methods
		public override Dictionary<string, string?> ConvertToScriptContent(Notification notification)
		{
			var dict = new Dictionary<string, string?>
			{
				["id"] = notification.Name,
				["title"] = notification.Title,
				["body"] = notification.Body
			};

			if (notification.Type == NotifierType.Message)
			{
				dict["subtitle"] = notification.Subtitle;
				dict["actionString"] = notification.CloseAction;
			}
			else if (notification.Type == NotifierType.Notification)
			{
				dict["icon"] = notification.Icon;
			}

			return dict;
		}

		protected override string? ConvertToScriptContent(Dictionary<string, string?> content)
		{
			if (!content.TryGetValue("id", out string? id) || string.IsNullOrEmpty(id))
				return null;

			var childGuide = DataMode.ChildGuideContent!;
			string indent = AddIndentation();
			var sb = new StringBuilder();

			sb.AppendLine($"{indent}\"{id}\": {{ {childGuide.GuideStart}");

			string[] knownKeys = ["title", "subtitle", "body", "icon", "actionString"];
			var toWrite = knownKeys
				.Where(k => content.TryGetValue(k, out string? v) && !string.IsNullOrEmpty(v))
				.ToList();

			for (int i = 0; i < toWrite.Count; i++)
			{
				string key = toWrite[i];
				string? value = content[key];
				bool isLast = i == toWrite.Count - 1;
				sb.AppendLine($"{indent}{indent}{ProcessAttributeLine(key, value)}{(isLast ? string.Empty : ",")}");
			}

			sb.Append($"{indent}}}, {childGuide.GuideEnd}");
			return sb.ToString();
		}
		#endregion

		#region Engine File Manipulation
		public override void AddEntityToScript(string name, Dictionary<string, string?> content, string? fileKeyParam = null)
		{
			(string filePath, string resolvedFileKey) = CheckKeyResolutionInFile(fileKeyParam, name);
			string tempPath = Path.GetTempFileName();

			try
			{
				var lines = File.ReadAllLines(filePath).ToList();
				var masterGuide = DataMode.MasterGuideContent;
				var childGuide = DataMode.ChildGuideContent!;

				(int startIndex, int endIndex) = GetPositionIndexInFile(lines, masterGuide, childGuide);

				// Ensure the last block's closing brace has a trailing comma
				for (int i = endIndex - 1; i > startIndex; i--)
				{
					string trimmed = lines[i].TrimEnd();
					if (!string.IsNullOrWhiteSpace(trimmed))
					{
						if (trimmed.EndsWith("} " + childGuide.GuideEnd))
							lines[i] = lines[i].Replace("} ", "}, ");
						break;
					}
				}

				if (ConvertToScriptContent(content) is string newBlock)
				{
					// Insert a blank separator line then the block lines before the end marker
					var blockLines = newBlock.Split('\n').Select(l => l.TrimEnd('\r')).ToList();
					lines.Insert(endIndex, string.Empty);
					for (int i = blockLines.Count - 1; i >= 0; i--)
						lines.Insert(endIndex + 1, blockLines[i]);

					File.WriteAllLines(tempPath, lines);
					FileWatcher.ReplaceFile(tempPath, filePath);

					if (CheckData(name) is Notification notif)
					{
						notif.FileKey = resolvedFileKey;
						notif.IsSynced = true;
						SaveData(SaveString);
					}
				}
				else
				{
					DialogBox.Show($"Failed to convert notifier data to script format for \"{name}\".",
						"Conversion Error", DialogButtonDefaults.OK, DialogIcon.Error);
				}
			}
			catch (Exception ex)
			{
				DialogBox.Show($"Failed to add notifier \"{name}\" to script.\n\n{ex}",
					"Add Failed", DialogButtonDefaults.OK, DialogIcon.Error);
				throw;
			}
			finally
			{
				if (File.Exists(tempPath)) File.Delete(tempPath);
			}
		}

		public override bool RemoveEntityFromScript(int notificationId, bool shouldSave = true)
		{
			return RemoveEntitiesFromScript(new[] { notificationId }, shouldSave);
		}

		public override bool RemoveEntitiesFromScript(int[] notificationIds, bool shouldSave = true)
		{
			if (notificationIds == null || notificationIds.Length == 0)
				return false;

			var idsToRemove = new HashSet<int>(notificationIds);
			var notificationsToRemove = new List<Notification>();

			foreach (int id in idsToRemove)
			{
				if (CheckData(id) is Notification notif)
				{
					notif.IsSynced = false;
					notificationsToRemove.Add(notif);
				}
			}

			if (notificationsToRemove.Count == 0)
				return false;

			var notificationsByFile = notificationsToRemove
				.GroupBy(notif => ResolveDataFilePath(notif, out _))
				.Where(g => g.Key != null)
				.ToDictionary(g => g.Key!, g => g.ToList());

			try
			{
				foreach (var (filePath, notificationsInFile) in notificationsByFile)
					RemoveEntityFromSingleFile(filePath, notificationsInFile);

				if (shouldSave) SaveData(SaveString);
				return true;
			}
			catch (Exception ex)
			{
				DialogBox.Show($"Failed to remove notification data from script.\n\n{ex}",
					"Remove Failed", DialogButtonDefaults.OK, DialogIcon.Error);
				throw;
			}
		}

		protected override void RemoveEntityFromSingleFile(string filePath, List<Notification> notificationsToRemove)
		{
			string tempPath = Path.GetTempFileName();
			var namesToRemove = new HashSet<string>(notificationsToRemove.Select(n => n.Name));

			try
			{
				using (var reader = new StreamReader(filePath))
				using (var writer = new StreamWriter(tempPath))
				{
					var masterGuide = DataMode.MasterGuideContent;
					var childGuide = DataMode.ChildGuideContent!;
					bool inSection = false;
					bool isRemoving = false;
					string? line;

					while ((line = reader.ReadLine()) != null)
					{
						string trimmed = line.Trim();

						if (!inSection && trimmed == masterGuide.GuideStart)
						{
							inSection = true;
							writer.WriteLine(line);
							continue;
						}

						if (inSection && trimmed == masterGuide.GuideEnd)
						{
							inSection = false;
							writer.WriteLine(line);
							continue;
						}

						if (inSection)
						{
							var match = BodyRegex.Match(trimmed);
							bool isBlockEnd = trimmed.EndsWith(childGuide.GuideEnd);

							if (!isRemoving && match.Success && namesToRemove.Contains(match.Groups["id"].Value))
							{
								isRemoving = true;
								continue;
							}

							if (isRemoving && isBlockEnd)
							{
								isRemoving = false;
								continue;
							}

							if (isRemoving)
								continue;
						}

						writer.WriteLine(line);
					}
				}

				FileWatcher.ReplaceFile(tempPath, filePath);
			}
			finally
			{
				if (File.Exists(tempPath))
					File.Delete(tempPath);
			}
		}

		public override bool UpdateEntityInScript(string name, Dictionary<string, string?> content, string? fileKeyParam = null)
		{
			(string filePath, string resolvedFileKey) = CheckKeyResolutionInFile(fileKeyParam, name);
			string tempPath = Path.GetTempFileName();

			try
			{
				var lines = File.ReadAllLines(filePath).ToList();
				var masterGuide = DataMode.MasterGuideContent;
				var childGuide = DataMode.ChildGuideContent!;

				(int baseStartIndex, int baseEndIndex) = GetPositionIndexInFile(lines, masterGuide, childGuide);

				var elementIndexes = GetPositionIndexInFile(lines[baseStartIndex..baseEndIndex], masterGuide, childGuide, name);
				int elmStartIndex = baseStartIndex + elementIndexes.Item1;
				int elmEndIndex = baseStartIndex + elementIndexes.Item2 - 1;

				if (ConvertToScriptContent(content) is string newBlock)
				{
					lines.RemoveRange(elmStartIndex, (elementIndexes.Item2 - elementIndexes.Item1) + 1);
					lines.Insert(elmStartIndex, newBlock);

					File.WriteAllLines(tempPath, lines);
					FileWatcher.ReplaceFile(tempPath, filePath);

					if (CheckData(name) is Notification notifier)
					{
						notifier.IsSynced = true;
						SaveData(SaveString);
					}
				}
				else
				{
					DialogBox.Show($"Failed to convert notifier data to script format for \"{name}\".",
						"Conversion Error", DialogButtonDefaults.OK, DialogIcon.Error);
					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				DialogBox.Show($"Failed to update notifier \"{name}\" in script.\n\n{ex}",
					"Update Failed", DialogButtonDefaults.OK, DialogIcon.Error);
				throw;
			}
			finally
			{
				if (File.Exists(tempPath)) File.Delete(tempPath);
			}
		}
		#endregion

		#region Synchronicity Checking
		public override bool CheckSynchronicity(bool showMessage = true)
		{
			if (ApplicationSettings == null)
				return false;

			bool hasChanged = false;
			string[] modes = ["messages", "notifications"];
			foreach (var mode in modes)
			{
				SetDataMode(mode);
				var files = GetDataFiles();
				if (files.Count == 0)
					return false;

				foreach (var (fileKey, _) in files)
				{
					var namesInFile = DataMode.Collection
						.Where(i => i.IsSynced &&
									(string.IsNullOrEmpty(i.FileKey)
										? fileKey == ResolveDataFileKey()
										: i.FileKey == fileKey))
						.Select(i => i.Name)
						.ToList();

					if (namesInFile.Count == 0)
						continue;

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
						"Notifiers that have been modified will appear as such when opening the notifier builder.",
						"Changes Have Been Made",
						DialogButtonDefaults.OK,
						DialogIcon.Warning);
				}

				if (hasChanged) break;
			}

			return hasChanged;
		}
		#endregion

		#endregion
	}
}