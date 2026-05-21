using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.generics.interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MonoBuilder.Models.notification_management
{
	public class Notification : BaseViewModel, INamedEntity, IMultiFile
	{
		private int _id = -1;

		private string _notificationId = "";
		private string? _subtitle = "";
		private string? _title = "";
		private string? _body = "";
		private string? _icon = "";
		private string? _closeAction = "";

		private string _fileKey = "";
		private bool _synced = false;
		private NotifierType _type;

		public int EntityID
		{
			get => _id;
			set => SetProperty(ref _id, value);
		}

		public string Name
		{
			get => _notificationId;
			set => SetProperty(ref _notificationId, value);
		}

		public string? Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}

		public string? Subtitle
		{
			get => _subtitle;
			set => SetProperty(ref _subtitle, value);
		}

		public string? Body
		{
			get => _body;
			set => SetProperty(ref _body, value);
		}

		public string? Icon
		{
			get => _icon;
			set => SetProperty(ref _icon, value);
		}

		public string? CloseAction
		{
			get => _closeAction;
			set => SetProperty(ref _closeAction, value);
		}

		public string FileKey
		{
			get => _fileKey;
			set => SetProperty(ref _fileKey, value);
		}

		public bool IsSynced
		{
			get => _synced;
			set => SetProperty(ref _synced, value);
		}

		public NotifierType Type
		{
			get => _type;
			set => SetProperty(ref _type, value);
		}

		public Notification(string notificationId,  string? title, string? icon, NotifierType type)
		{
			_notificationId = notificationId;
			_title = title;
			_icon = icon;

			_type = type;
		}
	}
}
