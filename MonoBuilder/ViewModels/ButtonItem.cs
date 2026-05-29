using MonoBuilder.Models;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MonoBuilder.ViewModels
{
	public class ButtonItem : BaseViewModel
	{
		private string _text = string.Empty;
		private bool _isEnabled = true;
		private Func<Window?>? _createWindow = null;
		private Action? _doAction = null;

		public string Text
		{
			get => _text;
			set => SetProperty(ref _text, value);
		}

		public bool IsEnabled
		{
			get => _isEnabled;
			set => SetProperty(ref _isEnabled, value);
		}

		public Func<Window?>? CreateWindow
		{
			get => _createWindow;
			set => SetProperty(ref _createWindow, value);
		}

		public Action? DoAction
		{
			get => _doAction;
			set => SetProperty(ref _doAction, value);
		}

		public ButtonItem(string text, bool isEnabled, Func<Window?>? createWindow = null, Action? doAction = null)
		{
			Text = text;
			IsEnabled = isEnabled;
			CreateWindow = createWindow;
			DoAction = doAction;
		}
	}

	public class UtilityButton : BaseViewModel
	{
		private int _width = 90;
		private int _height = 90;
		private int _padding = 5;
		private int _imgWidth = 70;
		private int _imgHeight = 70;
		private ImageSource _source;
		private string _labelText = string.Empty;

		private Func<Window>? _createWindow = null;
		private Action? _doAction = null;

		public int Width
		{
			get => _width;
			set => SetProperty(ref _width, value);
		}

		public int Height
		{
			get => _height;
			set => SetProperty(ref _height, value);
		}

		public int Padding
		{
			get => _padding;
			set => SetProperty(ref _padding, value);
		}

		public int ImgWidth
		{
			get => _imgWidth;
			set => SetProperty(ref _imgWidth, value);
		}

		public int ImgHeight
		{
			get => _imgHeight;
			set => SetProperty(ref _imgHeight, value);
		}

		public ImageSource Source
		{
			get => _source;
			set => SetProperty(ref _source, value);
		}

		public string LabelText
		{
			get => _labelText;
			set => SetProperty(ref _labelText, value);
		}

		public Func<Window>? CreateWindow
		{
			get => _createWindow;
			set => SetProperty(ref _createWindow, value);
		}

		public Action? DoAction
		{
			get => _doAction;
			set => SetProperty(ref _doAction, value); 
		}

		public UtilityButton(string source, Func<Window>? createWindow = null, Action? doAction = null)
		{
			try
			{
				_source = new BitmapImage(new Uri(source, UriKind.Relative));
				_createWindow = createWindow;
				_doAction = doAction;
			}
			catch (Exception)
			{
				_source = new BitmapImage(new Uri("Assets/fallback.jpg", UriKind.Relative));
			}
		}
	}
}
