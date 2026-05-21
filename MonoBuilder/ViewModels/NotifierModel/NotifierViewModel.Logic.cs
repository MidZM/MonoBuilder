using MonoBuilder.Commands;
using MonoBuilder.Models;
using MonoBuilder.Models.character_management;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.image_management;
using MonoBuilder.Models.notification_management;
using MonoBuilder.ViewModels._generic_models;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using static System.Resources.ResXFileRef;

namespace MonoBuilder.ViewModels.NotifierModel
{
    public partial class NotifierViewModel
    {
		public RelayCommand MoveNotifiersCommand { get; }

		public RelayCommand SaveCommand { get; }
		public RelayCommand CancelCommand { get; }

		public RelayCommand AddNotifiersCommand { get; }
		public RelayCommand ModifyNotifiersCommand { get; }
		public RelayCommand RemoveNotifiersCommand { get; }
		public RelayCommand SaveToScriptCommand { get; }
		public RelayCommand ImportNotifiersCommand { get; }

		public RelayCommand ExitCommand { get; }

		#region Utility Methods
		private string StripFileKey(string initializedFileKey)
		{
			if (initializedFileKey.StartsWith('('))
			{
				int startKey = 1;
				int endKey = initializedFileKey.IndexOf(')');
				return initializedFileKey[startKey..endKey];
			}

			return initializedFileKey;
		}

		private void ClearNotifierEditors()
		{
			BoxNameText = string.Empty;
			BoxTitleText = string.Empty;
			BoxSubtitleText = string.Empty;
			BoxBodyText.Text = string.Empty;
			BoxCloseText = string.Empty;
			BoxFileSelectedFile = AvailableFiles.FirstOrDefault() ?? string.Empty;
		}
		#endregion

		#region Execution Methods
		private void ExecuteMoveNotifiersCommand(object? element)
		{
			if (element is not MenuItem item) return;

			string? selectedItem = item.Header.ToString() ?? null;

			if (SelectedEntities.Count > 0 && selectedItem != null)
			{
				var match = GetFileKey.Match(selectedItem);
				if (match.Success)
				{
					var targetFileKey = match.Groups[1].Value;
					if (targetFileKey != null)
					{
						foreach (Notification notification in SelectedEntities.ToArray())
						{
							notification.FileKey = targetFileKey;
							NotifierData.UpdateNotification(notification.EntityID, notification);
						}
					}
					else
					{
						DialogBox.Show(
							$"The selected file path for {selectedItem} is not set. Please set it before moving characters.",
							"File Path Not Set",
							DialogButtonDefaults.OK,
							DialogIcon.Warning);
					}
				}
			}
		}

		private void ExecuteSaveCommand(object? _)
		{
			var type = Mode == "Messages" ? NotifierType.Message : NotifierType.Notification;
			var isMessageType = type == NotifierType.Message;

			string name = BoxNameText;
			string title = BoxTitleText;
			string subtitle = BoxSubtitleText;
			string body = BoxBodyText.Text;
			string closeAction = BoxCloseText;
			string fileKey = StripFileKey(BoxFileSelectedFile);

			if (ModifyingNotifier != null)
			{
				ModifyingNotifier.Name = name;
				ModifyingNotifier.Title = title;
				ModifyingNotifier.FileKey = fileKey;
				ModifyingNotifier.Body = body;

				if (isMessageType)
				{
					ModifyingNotifier.Subtitle = subtitle;
					ModifyingNotifier.CloseAction = closeAction.Length > 0 ? closeAction : null;
				}
				else ModifyingNotifier.Icon = subtitle;

				NotifierData.UpdateNotification(ModifyingNotifier.EntityID, ModifyingNotifier, false);
			}
			else
			{
				Notification newNotifier = new(
					name,
					title,
					isMessageType ? null : subtitle,
					type);

				if (isMessageType)
				{
					newNotifier.CloseAction = closeAction.Length > 0 ? closeAction : null;
					newNotifier.Subtitle = subtitle;
				}
				else newNotifier.Icon = subtitle;

				newNotifier.FileKey = fileKey;
				newNotifier.Body = body;

				NotifierData.AddNotification(newNotifier);
			}

			NotifierData.SaveData();

			if (ModifyingNotifier != null)
				ShowPreviewMessage(ModifyingNotifier);

			ModifyingNotifier = null;
			IsEditingNotifier.Value = false;
		}

		private void ExecuteCancelCommand(object? _)
		{
			IsEditingNotifier.Value = false;
		}

		private void ExecuteAddNotifiersCommand(object? _)
		{
			ClearNotifierEditors();
			IsEditingNotifier.Value = true;
		}

		private void ExecuteModifyNotifiersCommand(object? _)
		{
			if (SelectedEntity == null) return;

			ClearNotifierEditors();

			var isMessage = SelectedEntity.Type == NotifierType.Message;

			BoxNameText = SelectedEntity.Name;
			BoxTitleText = SelectedEntity.Title ?? string.Empty;
			BoxSubtitleText = (isMessage ? SelectedEntity.Subtitle : SelectedEntity.Icon) ?? string.Empty;
			BoxBodyText.Text = SelectedEntity.Body ?? string.Empty;
			BoxCloseText = isMessage ? SelectedEntity.CloseAction ?? string.Empty : string.Empty;
			BoxFileSelectedFile = SelectedEntity.FileKey;

			IsEditingNotifier.Value = true;
			ModifyingNotifier = SelectedEntity;
		}

		private void ExecuteRemoveNotifierCommand(object? _)
		{
			if (SelectedEntities.Count == 0) return;

			var selectedItems = SelectedEntities;
			int maxAmount = 10;
			string phrasing = selectedItems.Count > 1 ? "these notifiers" : "this notifier";
			var notifierNames = string.Join("\n", selectedItems
				.Cast<Notification>()
				.Take(maxAmount)
				.Select(notifier => $"- {notifier.Name}"));

			if (selectedItems.Count > maxAmount)
			{
				notifierNames += $"\n- And {selectedItems.Count - maxAmount} more...";
			}

			var question = DialogBox.Show($"Are you sure you wish to remove {phrasing}?\r\n{notifierNames}",
				"Confirm Removal",
				600,
				DialogIcon.Question,
				new DialogButton("From Program", DialogBoxResult.Continue),
				new DialogButton("From Script", DialogBoxResult.Retry),
				new DialogButton("From Both", DialogBoxResult.Yes, "ErrorButton"),
				new DialogButton("Cancel", DialogBoxResult.No, "ErrorButton"));

			if (question == DialogBoxResult.Continue || question == DialogBoxResult.Retry || question == DialogBoxResult.Yes)
			{
				try
				{
					List<Notification> notificationsToRemove = new();
					foreach (Notification notifier in selectedItems)
					{
						notificationsToRemove.Add(notifier);
					}

					foreach (Notification notifier in notificationsToRemove)
					{
						if (question == DialogBoxResult.Retry ||
							question == DialogBoxResult.Yes)
						{
							if (NotifierData.NotificationExistsInScript(notifier.EntityID))
							{
								NotifierData.RemoveNotificationFromScript(notifier.EntityID, false);
							}
						}

						if (question == DialogBoxResult.Continue ||
							question == DialogBoxResult.Yes)
						{
							NotifierData.RemoveNotification(notifier.EntityID);
						}
					}

					NotifierData.SaveData();
				}
				catch (Exception error)
				{
					DialogBox.Show(
						$"Something went wrong when removing notifiers!\n\n{error}",
						"Failed to Remove Notifiers",
						DialogButtonDefaults.OK,
						DialogIcon.Error);
				}
			}
		}

		private void ExecuteSaveToScriptCommand(object? _)
		{
			if (SelectedEntities.Count == 0) return;

			var result = DialogBox.Show(
				$"Would you like to update the selected notifiers, or all notifiers?",
				"Save Notifiers To Script",
				DialogIcon.Question,
				new DialogButton("Selected", DialogBoxResult.Continue),
				new DialogButton("All", DialogBoxResult.Yes),
				new DialogButton("Cancel", DialogBoxResult.No, "ErrorButton"));
			if (result == DialogBoxResult.Continue || result == DialogBoxResult.Yes)
			{
				try
				{
					List<(Notification, bool)> tags = new();
					int maxNotificationMerge = 10;
					int currentNotificationMerge = 0;
					string mergeDialog = "";
					int index = 0;

					foreach (Notification row in SelectedEntities)
					{
						var inScript = NotifierData.NotificationExistsInScript(row.Name, row.FileKey);

						if (inScript)
						{
							if (currentNotificationMerge < maxNotificationMerge)
							{
								mergeDialog += $"- {row.Name}\n";
							}
							currentNotificationMerge++;
						}

						tags.Add((row, inScript));

						if (index == SelectedEntities.Count - 1)
						{
							if (currentNotificationMerge > maxNotificationMerge)
							{
								mergeDialog += $"- And {currentNotificationMerge - maxNotificationMerge} more...";
							}
						}

						index++;
					}

					if (currentNotificationMerge > 0)
					{
						if (DialogBox.Show(
							$"Detected notifiers that already exist within the script.\nWould you like to merge existing notifiers?\n(Regardless of the answer, any new notifiers will be added)\n\n{mergeDialog}",
							"Existing Notifiers Detected",
							DialogButtonDefaults.YesNo,
							DialogIcon.Warning) == DialogBoxResult.No)
						{
							Predicate<(Notification, bool)> value = tuple => tuple.Item2;
							tags.RemoveAll(value);
						}
					}

					foreach (var (notifier, inScript) in tags)
					{
						var type = Mode == "Messages" ? NotifierType.Message : NotifierType.Notification;
						var content = NotifierData.ConvertToScriptContent(type, notifier);
						if (inScript)
						{
							NotifierData.UpdateNotificationInScript(notifier.Name, content);
						}
						else
						{
							NotifierData.AddNotificationToScript(notifier.Name, content);
						}
					}

					DialogBox.Show(
						"Notifiers successfully saved to script!",
						"Success",
						DialogButtonDefaults.OK,
						DialogIcon.Information);
				}
				catch (Exception error)
				{
					DialogBox.Show(
						$"Something went wrong when adding notifiers!\n\n{error}",
						"Failed to Add Notifiers",
						DialogButtonDefaults.OK,
						DialogIcon.Error);
				}
			}
		}

		private void ExecuteImportNotifiersCommand(object? _)
		{
			try
			{
				bool isMessage = Mode == "Messages";
				var type = isMessage ? NotifierType.Message : NotifierType.Notification;
				var notifierData = NotifierData.SyncNotifications();
				var duplicates = notifierData.Keys
					.Where(name => NotifierData.ContainsName(name))
					.ToList();

				if (duplicates.Count > 0)
				{
					int selectedCount = duplicates.Count;
					int maxAmount = 10;
					string phrasing = duplicates.Count > 1 ? "multiple notifiers" : "an notifier";
					string names = string.Join("\n", duplicates.Take(maxAmount).Select(c => $"- {c}"));

					if (selectedCount > maxAmount)
					{
						names += $"\n- And {selectedCount - maxAmount} more...";
					}

					var result = DialogBox.Show(
						$"Found {phrasing} with a similar name that already exist...\nDo you want to merge them?\r\n{names}",
						"Confirm Merge Status",
						DialogButtonDefaults.YesNo,
						DialogIcon.Question);

					if (result == DialogBoxResult.Yes)
					{
						foreach (string notifier in duplicates)
						{
							int notifierId = NotifierData.DataMode.Collection.First(c => c.Name == notifier).EntityID;
							string name = notifierData[notifier].Name;
							string? title = notifierData[notifier].Title;
							string? subtitle = notifierData[notifier].Subtitle;
							string? icon = notifierData[notifier].Icon;
							string? body = notifierData[notifier].Body;
							string? actionString = notifierData[notifier].CloseAction;
							string fileKey = notifierData[notifier].FileKey;

							Notification newNotifier = new(name, title, isMessage ? null : icon, type)
							{
								Body = body,
								FileKey = fileKey,
								IsSynced = true
							};

							if (isMessage)
							{
								newNotifier.Subtitle = subtitle;
								newNotifier.CloseAction = actionString;
							}

							NotifierData.UpdateNotification(notifierId, newNotifier);

							notifierData.Remove(notifier);
						}
					}
					else
					{
						foreach (string notifier in duplicates)
						{
							notifierData.Remove(notifier);
						}
					}
				}

				foreach (Notification notifier in notifierData.Values)
				{
					notifier.IsSynced = true;
					NotifierData.AddNotification(notifier);
				}
			}
			catch
			{
				return;
			}
		}

		private void ExecuteExitCommand(object? _)
		{
			Owner.Close();
		}
		#endregion
	}
}
