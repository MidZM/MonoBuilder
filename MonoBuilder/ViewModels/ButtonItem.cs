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
		private Func<Window>? _createWindow = null;
		private Action? _doAction = null;

		public string Text
		{
			get => _text;
			set
			{
				if (SetProperty(ref _text, value))
				{
					OnPropertyChanged(nameof(Text));
				}
			}
		}

		public bool IsEnabled
		{
			get => _isEnabled;
			set
			{
				if (SetProperty(ref _isEnabled, value))
				{
					OnPropertyChanged(nameof(IsEnabled));
				}
			}
		}

		public Func<Window>? CreateWindow
		{
			get => _createWindow;
			set
			{
				if (SetProperty(ref _createWindow, value))
				{
					OnPropertyChanged(nameof(CreateWindow));
				}
			}
		}

		public Action? DoAction
		{
			get => _doAction;
			set
			{
				if (SetProperty(ref _doAction, value))
				{
					OnPropertyChanged(nameof(DoAction));
				}
			}
		}

		public ButtonItem(string text, bool isEnabled, Func<Window>? createWindow = null, Action? doAction = null)
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
			set
			{
				if (SetProperty(ref _width, value))
				{
					OnPropertyChanged(nameof(Width));
				}
			}
		}

		public int Height
		{
			get => _height;
			set
			{
				if (SetProperty(ref _height, value))
				{
					OnPropertyChanged(nameof(Height));
				}
			}
		}

		public int Padding
		{
			get => _padding;
			set
			{
				if (SetProperty(ref _padding, value))
				{
					OnPropertyChanged(nameof(Padding));
				}
			}
		}

		public int ImgWidth
		{
			get => _imgWidth;
			set
			{
				if (SetProperty(ref _imgWidth, value))
				{
					OnPropertyChanged(nameof(ImgWidth));
				}
			}
		}

		public int ImgHeight
		{
			get => _imgHeight;
			set
			{
				if (SetProperty(ref _imgHeight, value))
				{
					OnPropertyChanged(nameof(ImgHeight));
				}
			}
		}

		public ImageSource Source
		{
			get => _source;
			set
			{
				if (SetProperty(ref _source, value))
				{
					OnPropertyChanged(nameof(Source));
				}
			}
		}

		public string LabelText
		{
			get => _labelText;
			set
			{
				if (SetProperty(ref _labelText, value))
				{
					OnPropertyChanged(nameof(LabelText));
				}
			}
		}

		public Func<Window>? CreateWindow
		{
			get => _createWindow;
			set
			{
				if (SetProperty(ref _createWindow, value))
				{
					OnPropertyChanged(nameof(CreateWindow));
				}
			}
		}

		public Action? DoAction
		{
			get => _doAction;
			set
			{
				if (SetProperty(ref _doAction, value))
				{
					OnPropertyChanged(nameof(DoAction));
				}
			}
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
