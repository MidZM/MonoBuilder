using MonoBuilder.Commands;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.image_management;
using MonoBuilder.Models.notification_management;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Windows.Controls;

namespace MonoBuilder.ViewModels.NotifierModel
{
    public partial class NotifierViewModel
    {
		public RelayCommand SaveCommand { get; }
		public RelayCommand CancelCommand { get; }

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

		protected override void FillDataEditors()
		{
			var isMessage = SelectedEntity!.Type == NotifierType.Message;

			BoxNameText = SelectedEntity.Name;
			BoxTitleText = SelectedEntity.Title ?? string.Empty;
			BoxSubtitleText = (isMessage ? SelectedEntity.Subtitle : SelectedEntity.Icon) ?? string.Empty;
			BoxBodyText.Text = SelectedEntity.Body ?? string.Empty;
			BoxCloseText = isMessage ? SelectedEntity.CloseAction ?? string.Empty : string.Empty;
			BoxFileSelectedFile = SelectedEntity.FileKey;
		}

		protected override void ClearDataEditors()
		{
			BoxNameText = string.Empty;
			BoxTitleText = string.Empty;
			BoxSubtitleText = string.Empty;
			BoxBodyText.Text = string.Empty;
			BoxCloseText = string.Empty;
			BoxFileSelectedFile = AvailableFiles.FirstOrDefault() ?? string.Empty;
		}

		protected override void RunDataImportSetup(string notifier, Dictionary<string, Notification> notifierData)
		{
			bool isMessage = Mode == "Messages";
			var type = isMessage ? NotifierType.Message : NotifierType.Notification;
			int notifierId = DataController.DataMode.Collection.First(c => c.Name == notifier).EntityID;
			string name = notifierData[notifier].Name;
			string? title = notifierData[notifier].Title;
			string? subtitle = notifierData[notifier].Subtitle;
			string? icon = notifierData[notifier].Icon;
			string? body = notifierData[notifier].Body;
			string? actionString = notifierData[notifier].CloseAction;
			string fileKey = notifierData[notifier].FileKey;

			Notification newNotifier = new(name, title, isMessage ? null : icon, type)
			{
				EntityID = notifierId,
				Body = body,
				FileKey = fileKey,
				IsSynced = true
			};

			if (isMessage)
			{
				newNotifier.Subtitle = subtitle;
				newNotifier.CloseAction = actionString;
			}

			DataController.UpdateData(notifierId, newNotifier);

			notifierData.Remove(notifier);
		}
		#endregion

		#region Execution Methods

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

			if (ModifyingData != null)
			{
				ModifyingData.Name = name;
				ModifyingData.Title = title;
				ModifyingData.FileKey = fileKey;
				ModifyingData.Body = body;

				if (isMessageType)
				{
					ModifyingData.Subtitle = subtitle;
					ModifyingData.CloseAction = closeAction.Length > 0 ? closeAction : null;
				}
				else ModifyingData.Icon = subtitle;

				DataController.UpdateData(ModifyingData.EntityID, ModifyingData, false);
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

				DataController.AddData(newNotifier);
			}

			DataController.SaveData(DataController._SaveString);

			if (ModifyingData != null)
				ShowPreviewMessage(ModifyingData);

			ModifyingData = null;
			IsEditingData.Value = false;
		}

		private void ExecuteCancelCommand(object? _)
		{
			IsEditingData.Value = false;
		}
		#endregion
	}
}
