using MonoBuilder.Models.generics.enums;
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
		private AssetStore<Notification> _messages = new() { TypeName = "Messages", TypeBody = "MessageType" };
		private AssetStore<Notification> _notifications = new() { TypeName = "Notifications", TypeBody = "NotificationType" };

		public override AssetStore<Notification> DataMode { get; set; }

		public ObservableCollection<Notification> AllMessages { get; private set; }
		public ObservableCollection<Notification> AllNotifications { get; private set; }

		private static Regex BodyRegex = new(@"^([""'`]?)(?<id>.*)\1.*[:]*[\{] (// MESSAGE_START|// NOTIFICATION_START)$", RegexOptions.Compiled);
		private static Regex AttributeRegex = new(@"^([""'`]?)(?<key>.*)\1[:] (?<attr>.*)$", RegexOptions.Compiled);

		public Notifications()
		{
			AllMessages = _messages.Collection;
			AllNotifications = _notifications.Collection;

			DataMode = _messages;

			ContentGuides = new()
			{
				{
					"Messages",
					[
						"// MESSAGES_INSERTION_POINT",
						"// END_MESSAGES_INSERTION_POINT"
					]
				},
				{
					"MessageType",
					[
						"// MESSAGE_START",
						"// MESSAGE_END"
					]
				},
				{
					"Notifications",
					[
						"// NOTIFICATIONS_INSERTION_POINT",
						"// END_NOTIFICATIONS_INSERTION_POINT"
					]
				},
				{
					"NotificationType",
					[
						"// NOTIFICATION_START",
						"// NOTIFICATION_END"
					]
				}
			};

			LoadData();
		}

		#region Mode Management
		public void SetDataMode(string mode)
		{
			switch (mode)
			{
				case "messages": DataMode = _messages; break;
				case "notifications": DataMode = _notifications; break;
			}
		}

		public AssetStore<Notification> GetDataMode(string mode)
		{
			return mode switch
			{
				"messages" => _messages,
				"notifications" => _notifications,
				_ => _messages
			};
		}
		#endregion

		#region Handle File Data
		private void LoadNotification(XElement element, NotifierType type, ObservableCollection<Notification> collectionType)
		{
			string? notificationID = (string?)element.Attribute("NotificationID");
			string? title = (string?)element.Attribute("Title");
			string? subtitle = (string?)element.Attribute("Subtitle");
			string? icon = (string?)element.Attribute("Icon");
			string? closeAction = (string?)element.Attribute("CloseAction");
			string fileKey = (string?)element.Attribute("FileKey") ?? string.Empty;
			_ = bool.TryParse((string?)element.Attribute("IsSynced"), out bool isSynced);
			string? body = element.Value;

			if (notificationID != null)
			{
				Notification newNotification = new(notificationID, title, icon, type)
				{
					EntityID = collectionType.Count,
					FileKey = fileKey,
					IsSynced = isSynced,
					Body = body
				};

				if (type == NotifierType.Message && subtitle != null)
				{
					newNotification.Subtitle = subtitle;
					newNotification.CloseAction = closeAction;
				}
				else if (type == NotifierType.Notification && icon != null)
				{
					newNotification.Icon = icon;
				}

				collectionType.Add(newNotification);
			}
		}

		public override void LoadData()
		{
			try
			{
				if (!Directory.Exists("data"))
					Directory.CreateDirectory("data");

				if (!File.Exists("data/notifications.xml"))
				{
					SaveData();
					return;
				}

				SystemData = XDocument.Load("data/notifications.xml");
				_messages.Collection.Clear();
				_notifications.Collection.Clear();

				foreach (XElement message in SystemData.Descendants("Message").ToList())
					LoadNotification(message, NotifierType.Message, _messages.Collection);

				foreach (XElement notification in SystemData.Descendants("Notification").ToList())
					LoadNotification(notification, NotifierType.Notification, _notifications.Collection);

				if (_messages.Collection.Any())
					_messages.NextId = _messages.Collection.Max(i => i.EntityID) + 1;

				if (_notifications.Collection.Any())
					_notifications.NextId = _notifications.Collection.Max(s => s.EntityID) + 1;

				RebuildLookups("messages");
				RebuildLookups("notifications");
			}
			catch (FileNotFoundException error)
			{
				DialogBox.Show($"Save Data Reading Failure!\r\n{error}", "Error", DialogButtonDefaults.OK, DialogIcon.Error);
			}
			catch (XmlException error)
			{
				DialogBox.Show($"Notifications File Reading Failure!\r\n{error}", "Error", DialogButtonDefaults.OK, DialogIcon.Error);
			}
			catch (Exception error)
			{
				DialogBox.Show($"Something went wrong!\r\n{error}", "Error", DialogButtonDefaults.OK, DialogIcon.Error);
			}
		}

		public override void SaveData()
		{
			SystemData = new XDocument(
				new XDeclaration("1.0", "utf-8", "yes"),
				new XElement("Root",
					new XElement(GetDataMode("messages").TypeName,
						_messages.Collection.Select(m => new XElement("Message",
							new XAttribute("DataID", m.EntityID),
							new XAttribute("NotificationID", m.Name),
							new XAttribute("Title", m.Title ?? string.Empty),
							new XAttribute("Subtitle", m.Subtitle ?? string.Empty),
							new XAttribute("CloseAction", m.CloseAction ?? string.Empty),
							!string.IsNullOrEmpty(m.FileKey) ? new XAttribute("FileKey", m.FileKey) : null,
							new XAttribute("IsSynced", m.IsSynced),
							new XCData(m.Body ?? string.Empty)
						))
					),
					new XElement(GetDataMode("notifications").TypeName,
						_notifications.Collection.Select(n => new XElement("Notification",
							new XAttribute("DataID", n.EntityID),
							new XAttribute("NotificationID", n.Name),
							new XAttribute("Title", n.Title ?? string.Empty),
							new XAttribute("Icon", n.Icon ?? string.Empty),
							!string.IsNullOrEmpty(n.FileKey) ? new XAttribute("FileKey", n.FileKey) : null,
							new XAttribute("IsSynced", n.IsSynced),
							new XCData(n.Body ?? string.Empty)
						))
					)
				)
			);

			SystemData.Save("data/notifications.xml");
		}

		public void LoadSettings(AppSettings settings)
		{
			ApplicationSettings = settings;
		}

		private Dictionary<string, string> GetNotificationFiles()
		{
			if (ApplicationSettings == null)
				return [];

			var files = ApplicationSettings.GetAllFilePaths(DataMode.TypeName);
			if (files.Count == 0)
			{
				var legacyPath = ApplicationSettings.GetFilePath(DataMode.TypeName);
				if (!string.IsNullOrEmpty(legacyPath))
					files[DataMode.TypeName] = legacyPath;
			}

			return files;
		}

		private string? ResolveNotificationFileKey(string? fileKey = null)
		{
			var files = GetNotificationFiles();
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

		private string? ResolveNotificationFilePath(string? fileKey, out string resolvedFileKey)
		{
			resolvedFileKey = ResolveNotificationFileKey(fileKey) ?? string.Empty;
			if (string.IsNullOrEmpty(resolvedFileKey))
				return null;

			return ApplicationSettings?.GetAllFilePaths().TryGetValue(resolvedFileKey, out string? filePath) == true
				? filePath
				: null;
		}

		private string? ResolveNotificationFilePath(Notification? notification, out string resolvedFileKey)
		{
			return ResolveNotificationFilePath(notification?.FileKey, out resolvedFileKey);
		}
		#endregion

		private Notification? FindNotification(string name) => DataMode.Collection.FirstOrDefault(i => i.Name == name);
		private Notification? FindNotification(int index) => DataMode.Collection.FirstOrDefault(i => i.EntityID == index);

		#region Sync notifiers to the program
		public Dictionary<string, Notification> SyncNotifications(bool duplicatesOnly = false)
		{
			var notifications = new Dictionary<string, Notification>(StringComparer.Ordinal);
			var files = GetNotificationFiles();

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
			var typeBody = DataMode.TypeBody!;
			var notifierType = type == "Messages" ? NotifierType.Message : NotifierType.Notification;

			foreach (var (fileKey, filePath) in files.OrderBy(entry => entry.Key))
			{
				string[] content = File.ReadAllLines(filePath);
				int start = Array.FindIndex(content, line => line.Trim() == ContentGuides[type][0]);
				int end = Array.FindIndex(content, start + 1, line => line.Trim() == ContentGuides[type][1]);

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
					bool isBlockEnd = line.StartsWith("} " + ContentGuides[typeBody][1]) ||
									  line.StartsWith("}, " + ContentGuides[typeBody][1]);

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

		#region Handle notifier data in program
		public bool CheckForDuplicates(Notification notificationToCheck)
		{
			foreach (Notification notification in DataMode.Collection)
			{
				if (notification.Name == notificationToCheck.Name)
					return true;
			}
			return false;
		}

		public List<string> CheckForDuplicates(HashSet<Notification> notificationsToCheck)
		{
			List<string> list = new();
			foreach (Notification notification in DataMode.Collection)
			{
				if (notificationsToCheck.Contains(notification))
					list.Add(notification.Name);
			}
			return list;
		}

		public void RebuildLookups()
		{
			DataMode.ByName.Clear();
			DataMode.ById.Clear();

			foreach (var notif in DataMode.Collection)
			{
				DataMode.ByName[notif.Name] = notif;
				DataMode.ById[notif.EntityID] = notif;
			}
		}

		private void RebuildLookups(string type)
		{
			var dataMode = GetDataMode(type);

			dataMode.ByName.Clear();
			dataMode.ById.Clear();

			foreach (var notif in dataMode.Collection)
			{
				dataMode.ByName[notif.Name] = notif;
				dataMode.ById[notif.EntityID] = notif;
			}
		}

		public void AddNotification(Notification notification, bool shouldSave = true)
		{
			if (string.IsNullOrEmpty(notification.FileKey))
				notification.FileKey = ResolveNotificationFileKey(DataMode.TypeName) ?? string.Empty;

			notification.EntityID = DataMode.NextId++;
			DataMode.Collection.Add(notification);

			DataMode.ByName[notification.Name] = notification;
			DataMode.ById[notification.EntityID] = notification;

			if (shouldSave) SaveData();
		}

		public bool RemoveNotification(int notificationId, bool shouldSave = true)
		{
			if (!DataMode.ById.TryGetValue(notificationId, out var notif))
				return false;

			DataMode.ById.Remove(notificationId);
			DataMode.ByName.Remove(notif.Name);
			DataMode.Collection.RemoveAt(DataMode.Collection.IndexOf(notif));
			RebuildLookups();

			if (shouldSave) SaveData();
			return true;
		}

		public void RemoveNotifications(int[] notificationIds, bool shouldSave = true)
		{
			if (notificationIds == null || notificationIds.Length == 0)
				return;

			var idsToRemove = new HashSet<int>(notificationIds);

			foreach (int id in idsToRemove)
			{
				if (DataMode.ById.TryGetValue(id, out var notif))
				{
					DataMode.ByName.Remove(notif.Name);
					DataMode.ById.Remove(id);
				}
			}

			DataMode.Collection.RemoveByIds(idsToRemove);

			if (shouldSave) SaveData();
		}

		public bool ContainsName(string name) => DataMode.ByName.ContainsKey(name);
		public Notification? CheckNotification(string notificationName) => DataMode.ByName.TryGetValue(notificationName, out var notif) ? notif : null;
		public Notification? CheckNotification(int notificationId) => DataMode.ById.TryGetValue(notificationId, out var notif) ? notif : null;

		public Notification UpdateNotification(int notificationId, Notification newData, bool shouldSave = true)
		{
			if (!DataMode.ById.TryGetValue(notificationId, out var existing))
				throw new KeyNotFoundException($"Notification ID {notificationId} not found");

			string oldName = existing.Name;

			existing.Name = newData.Name;
			existing.Title = newData.Title;
			existing.Subtitle = newData.Subtitle;
			existing.Body = newData.Body;
			existing.Icon = newData.Icon;
			existing.FileKey = newData.FileKey ?? existing.FileKey;
			existing.IsSynced = newData.IsSynced;

			if (oldName != newData.Name)
			{
				DataMode.ByName.Remove(oldName);
				DataMode.ByName[newData.Name] = existing;
			}

			if (shouldSave) SaveData();

			return existing;
		}
		#endregion

		#region Utility File Methods
		private (string, string) CheckKeyResolutionInFile(string? fileKeyParam, string name)
		{
			string? fileKey = fileKeyParam ?? CheckNotification(name)?.FileKey;
			var filePath = ResolveNotificationFilePath(fileKey, out string resolvedFileKey);

			if (filePath == null)
			{
				DialogBox.Show($"Failed to resolve file path for notifier \"{name}\".",
					"Bad File Path", DialogButtonDefaults.OK, DialogIcon.Error);
				throw new ArgumentOutOfRangeException($"Bad file data for {name}");
			}

			return (filePath, resolvedFileKey);
		}

		private (int, int) GetPositionIndexInFile(List<string> lines, string type, string typeBody, string? name = null)
		{
			int startIndex = name == null
				? lines.FindIndex(l => l.Trim() == ContentGuides[type][0])
				: lines.FindIndex(l => l.Contains(name) && l.TrimEnd().EndsWith(ContentGuides[typeBody][0]));
			int endIndex = name == null
				? lines.FindIndex(startIndex + 1, l => l.Trim() == ContentGuides[type][1])
				: lines.FindIndex(startIndex + 1, l => l.TrimEnd().EndsWith(ContentGuides[typeBody][1]));

			if (startIndex == -1 || endIndex == -1)
			{
				DialogBox.Show(
					$"Missing proper notification section markers in script file.\n\n" +
					$"You need opening and closing tags like:\n" +
					$"{ContentGuides[type][0]}\n{AddIndentation()}\"Example\": {{ {ContentGuides[typeBody][0]}\n" +
					$"{AddIndentation()}{AddIndentation()}\"title\": \"...\",\n" +
					$"{AddIndentation()}}} {ContentGuides[typeBody][1]}\n{ContentGuides[type][1]}",
					"Missing Notification Section", DialogButtonDefaults.OK, DialogIcon.Error);

				string phrasing = name == null ? "base" : "element";
				throw new IndexOutOfRangeException($"Failed to find opening and closing {phrasing} tags in the selected file...");
			}

			return (startIndex, endIndex);
		}
		#endregion

		#region Handle notifier data in file
		private string AddIndentation()
		{
			return ApplicationSettings?.GetIndentationType() switch
			{
				"Tabs" => "\t",
				"Spaces" => new string(' ', ApplicationSettings.GetIndentationAmount()),
				_ => new string(' ', 4)
			};
		}

		private int FindFirstNonWhitespaceIndex(string input)
		{
			for (int i = 0; i < input.Length; i++)
			{
				if (!char.IsWhiteSpace(input[i]))
					return i;
			}
			return -1;
		}

		private string ProcessAttributeLine(string key, string? value)
		{
			if (value != null && value.Contains('\n'))
			{
				string indent = AddIndentation();
				return $"\"{key}\": `\n{indent}{indent}{indent}{value.Replace("\n", $"\n{indent}{indent}{indent}")}\n{indent}{indent}`";
			}

			return $"\"{key}\": \"{value?.Replace("\"", "\\\"") ?? string.Empty}\"";
		}

		public Dictionary<string, string?> ConvertToScriptContent(NotifierType type, Notification notification)
		{
			var dict = new Dictionary<string, string?>
			{
				["id"] = notification.Name,
				["title"] = notification.Title,
				["body"] = notification.Body
			};

			if (type == NotifierType.Message)
			{
				dict["subtitle"] = notification.Subtitle;
				dict["actionString"] = notification.CloseAction;
			}
			else if (type == NotifierType.Notification)
			{
				dict["icon"] = notification.Icon;
			}

			return dict;
		}

		private string? ConvertToScriptContent(Dictionary<string, string?> content, bool shouldAddComma = false)
		{
			if (!content.TryGetValue("id", out string? id) || string.IsNullOrEmpty(id))
				return null;

			var typeBody = DataMode.TypeBody!;
			string indent = AddIndentation();
			string comma = shouldAddComma ? "," : string.Empty;
			var sb = new StringBuilder();

			sb.AppendLine($"{indent}\"{id}\": {{ {ContentGuides[typeBody][0]}");

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

			sb.Append($"{indent}}}{comma} {ContentGuides[typeBody][1]}");
			return sb.ToString();
		}

		public Dictionary<string, bool> NotificationsExistsInScript(HashSet<string> names)
		{
			var files = GetNotificationFiles();
			if (files.Count == 0)
			{
				DialogBox.Show("Attempted to check notification existence without a proper file path!",
					"No File Path", DialogButtonDefaults.OK, DialogIcon.Warning);
				return [];
			}

			var type = DataMode.TypeName;
			var namesInScript = names.ToDictionary(name => name, _ => false);

			foreach (var (_, filePath) in files)
			{
				string[] lines = File.ReadAllLines(filePath);
				bool inSection = false;

				foreach (string line in lines)
				{
					string trimmed = line.Trim();

					if (trimmed == ContentGuides[type][0]) { inSection = true; continue; }
					if (inSection && trimmed == ContentGuides[type][1]) break;

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

		public bool NotificationExistsInScript(string name, string? fileKey = null)
		{
			var files = GetNotificationFiles();
			if (files.Count == 0)
			{
				DialogBox.Show("Attempted to check notification existence without a proper file path!",
					"No File Path", DialogButtonDefaults.OK, DialogIcon.Warning);
				return false;
			}

			var type = DataMode.TypeName;
			var filesToCheck = string.IsNullOrWhiteSpace(fileKey)
				? files.OrderBy(entry => entry.Key)
				: files.Where(entry => entry.Key == ResolveNotificationFileKey(fileKey));

			foreach (var (_, filePath) in filesToCheck)
			{
				string[] lines = File.ReadAllLines(filePath);
				bool inSection = false;

				foreach (string line in lines)
				{
					string trimmed = line.Trim();

					if (trimmed == ContentGuides[type][0]) { inSection = true; continue; }
					if (inSection && trimmed == ContentGuides[type][1]) break;

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

		public bool NotificationExistsInScript(int notificationId)
		{
			if (CheckNotification(notificationId) is not Notification notif)
				return false;

			return NotificationExistsInScript(notif.Name, notif.FileKey);
		}

		public void AddNotificationToScript(string name, Dictionary<string, string?> content, string? fileKeyParam = null)
		{
			(string filePath, string resolvedFileKey) = CheckKeyResolutionInFile(fileKeyParam, name);
			string tempPath = Path.GetTempFileName();

			try
			{
				var lines = File.ReadAllLines(filePath).ToList();
				var type = DataMode.TypeName;
				var typeBody = DataMode.TypeBody!;

				(int startIndex, int endIndex) = GetPositionIndexInFile(lines, type, typeBody);

				// Ensure the last block's closing brace has a trailing comma
				for (int i = endIndex - 1; i > startIndex; i--)
				{
					string trimmed = lines[i].TrimEnd();
					if (!string.IsNullOrWhiteSpace(trimmed))
					{
						if (trimmed.EndsWith("} " + ContentGuides[typeBody][1]))
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

					if (CheckNotification(name) is Notification notif)
					{
						notif.FileKey = resolvedFileKey;
						notif.IsSynced = true;
						SaveData();
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

		public bool RemoveNotificationFromScript(int notificationId, bool shouldSave = true)
		{
			return RemoveNotificationsFromScript(new[] { notificationId }, shouldSave);
		}

		public bool RemoveNotificationsFromScript(int[] notificationIds, bool shouldSave = true)
		{
			if (notificationIds == null || notificationIds.Length == 0)
				return false;

			var idsToRemove = new HashSet<int>(notificationIds);
			var notificationsToRemove = new List<Notification>();

			foreach (int id in idsToRemove)
			{
				if (CheckNotification(id) is Notification notif)
				{
					notif.IsSynced = false;
					notificationsToRemove.Add(notif);
				}
			}

			if (notificationsToRemove.Count == 0)
				return false;

			var notificationsByFile = notificationsToRemove
				.GroupBy(notif => ResolveNotificationFilePath(notif, out _))
				.Where(g => g.Key != null)
				.ToDictionary(g => g.Key!, g => g.ToList());

			try
			{
				foreach (var (filePath, notificationsInFile) in notificationsByFile)
					RemoveNotificationsFromSingleFile(filePath, notificationsInFile);

				if (shouldSave) SaveData();
				return true;
			}
			catch (Exception ex)
			{
				DialogBox.Show($"Failed to remove notification data from script.\n\n{ex}",
					"Remove Failed", DialogButtonDefaults.OK, DialogIcon.Error);
				throw;
			}
		}

		private void RemoveNotificationsFromSingleFile(string filePath, List<Notification> notificationsToRemove)
		{
			string tempPath = Path.GetTempFileName();
			var namesToRemove = new HashSet<string>(notificationsToRemove.Select(n => n.Name));

			try
			{
				using (var reader = new StreamReader(filePath))
				using (var writer = new StreamWriter(tempPath))
				{
					var type = DataMode.TypeName;
					var typeBody = DataMode.TypeBody!;
					bool inSection = false;
					bool isRemoving = false;
					string? line;

					while ((line = reader.ReadLine()) != null)
					{
						string trimmed = line.Trim();

						if (!inSection && trimmed == ContentGuides[type][0])
						{
							inSection = true;
							writer.WriteLine(line);
							continue;
						}

						if (inSection && trimmed == ContentGuides[type][1])
						{
							inSection = false;
							writer.WriteLine(line);
							continue;
						}

						if (inSection)
						{
							var match = BodyRegex.Match(trimmed);
							bool isBlockEnd = trimmed.EndsWith(ContentGuides[typeBody][1]);

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

		public bool UpdateNotificationInScript(string name, Dictionary<string, string?> content, string? fileKeyParam = null)
		{
			(string filePath, string resolvedFileKey) = CheckKeyResolutionInFile(fileKeyParam, name);
			string tempPath = Path.GetTempFileName();

			try
			{
				var lines = File.ReadAllLines(filePath).ToList();
				string type = DataMode.TypeName;
				string typeBody = DataMode.TypeBody!;

				(int baseStartIndex, int baseEndIndex) = GetPositionIndexInFile(lines, type, typeBody);

				var elementIndexes = GetPositionIndexInFile(lines[baseStartIndex..baseEndIndex], type, typeBody, name);
				int elmStartIndex = baseStartIndex + elementIndexes.Item1;
				int elmEndIndex = baseStartIndex + elementIndexes.Item2 - 1;

				if (ConvertToScriptContent(content, true) is string newBlock)
				{
					lines.RemoveRange(elmStartIndex, (elementIndexes.Item2 - elementIndexes.Item1) + 1);
					lines.Insert(elmStartIndex, newBlock);

					File.WriteAllLines(tempPath, lines);
					FileWatcher.ReplaceFile(tempPath, filePath);

					if (CheckNotification(name) is Notification notifier)
					{
						notifier.IsSynced = true;
						SaveData();
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

		public Dictionary<string, bool> NotificationContentMatches(List<string> names, string? fileKey = null)
		{
			var results = names.ToDictionary(n => n, _ => false);
			var remaining = new HashSet<string>(names);

			var filePath = ResolveNotificationFilePath(fileKey, out _);
			if (filePath == null || names.Count == 0)
				return results;

			var type = DataMode.TypeName;
			var typeBody = DataMode.TypeBody!;
			string[] fileContent = File.ReadAllLines(filePath);
			int start = Array.FindIndex(fileContent, line => line.Trim() == ContentGuides[type][0]);
			int end = Array.FindIndex(fileContent, start + 1, line => line.Trim() == ContentGuides[type][1]);

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
				bool isBlockEnd = line.StartsWith("} " + ContentGuides[typeBody][1]) ||
								  line.StartsWith("}, " + ContentGuides[typeBody][1]);

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
						if (CheckNotification(currentId) is Notification notif)
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

		public bool CheckSynchronicity(bool showMessage = true)
		{
			if (ApplicationSettings == null)
				return false;

			bool hasChanged = false;
			string[] modes = ["messages", "notifications"];
			foreach (var mode in modes)
			{
				SetDataMode(mode);
				var files = GetNotificationFiles();
				if (files.Count == 0)
					return false;

				foreach (var (fileKey, _) in files)
				{
					var namesInFile = DataMode.Collection
						.Where(i => i.IsSynced &&
									(string.IsNullOrEmpty(i.FileKey)
										? fileKey == ResolveNotificationFileKey()
										: i.FileKey == fileKey))
						.Select(i => i.Name)
						.ToList();

					if (namesInFile.Count == 0)
						continue;

					var contentMatches = NotificationContentMatches(namesInFile, fileKey);

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
	}
}