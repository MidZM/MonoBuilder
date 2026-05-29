using MonoBuilder.Commands;
using MonoBuilder.Models;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Forms;

namespace MonoBuilder.ViewModels
{
	public class TabEntries<T> : BaseViewModel where T : TabEntry
	{
		private readonly ObservableCollection<T> _tabs = new();

		public ObservableCollection<T> Tabs => _tabs;

		public TabEntries(IEnumerable<T>? initialTabs = null)
		{
			if (initialTabs != null)
			{
				foreach (var tab in initialTabs)
				{
					_tabs.Add(tab);
				}
			}
		}

		public T? GetTab(string name) => _tabs.FirstOrDefault(n => n.Name == name);

		public void Add(T tab) => _tabs.Add(tab);
		public void Remove(T tab) => _tabs.Remove(tab);
		public void Clear() => _tabs.Clear();
	}

	public class TabEntries : TabEntries<TabEntry>
	{
		public TabEntries(IEnumerable<TabEntry>? initialTabs = null) : base(initialTabs)
		{}
	}

	public class FolderTabEntries : TabEntries<FolderTabEntry>
	{
		public FolderTabEntries(IEnumerable<FolderTabEntry>? initialTabs = null) : base(initialTabs)
		{}
	}

	public class FileTabEntries : TabEntries<FileTabEntry>
	{
		public FileTabEntries(IEnumerable<FileTabEntry>? initialTabs = null) : base(initialTabs)
		{}

	}

	public class TabEntry : BaseViewModel
	{
		private string _name;
		private string? _type;
		private string? _tag;
		
		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}

		public string? Type
		{
			get => _type;
			set => SetProperty(ref _type, value);
		}

		public string? Tag
		{
			get => _tag;
			set => SetProperty(ref _tag, value);
		}

		public TabEntry(string name)
		{
			_name = name;
		}
	}

	public class FileTabEntry : TabEntry
	{
		private string? BasePath { get; set; } = null;
		private AppSettings ApplicationSettings { get; set; }
		public ObservableCollection<FileEntry> Files { get; } = new();
		public RelayCommand AddNewFileCommand { get; }

		public FileTabEntry(AppSettings settings, string name, string basePath, IEnumerable<FileEntry>? files = null) : base(name)
		{
			ApplicationSettings = settings;
			BasePath = basePath;
			AddNewFileCommand = new RelayCommand(ExecuteAddNewFileCommand);

			if (files != null)
				foreach (var f in files)
				{
					if (f.DataCollection == null)
					{
						f.DataCollection = Files;
					}

					Files.Add(f);
				}
		}

		public FileTabEntry(AppSettings settings, string name, string basePath, IEnumerable<(string, string, string, AppSettings, string?)> files) : base(name)
		{
			ApplicationSettings = settings;
			BasePath = basePath;
			AddNewFileCommand = new RelayCommand(ExecuteAddNewFileCommand);

			foreach (var f in files)
			{
				Files.Add(new FileEntry(f.Item1, f.Item2, f.Item3, f.Item4, f.Item5)
				{
					DataCollection = Files
				});
			}
		}

		private void ExecuteAddNewFileCommand(object? parameter = null)
		{
			var key = $"{Name}:{Files.Count}";
			var newFile = new FileEntry(string.Empty, key, string.Empty, ApplicationSettings, BasePath)
			{
				DataCollection = Files
			};
			Files.Add(newFile);
		}

		public FileEntry? GetEntry(string key) =>
			Files.FirstOrDefault(f => f.Key == key);
	}

	public class FolderTabEntry : TabEntry
	{
		private SettingsModel.StringWrapper BasePath { get; set; }
		private AppSettings ApplicationSettings { get; set; }
		public ObservableCollection<FileEntry> Folders { get; } = new();
		public RelayCommand SelectFolderCommand { get; set; }

		public FolderTabEntry(AppSettings settings, string name, SettingsModel.StringWrapper basePath, IEnumerable<FileEntry>? folders = null)
			: base(name)
		{
			BasePath = basePath;
			ApplicationSettings = settings;

			SelectFolderCommand = new RelayCommand(ExecuteSelectFolderCommand);

			if (folders != null)
				foreach (var f in folders)
				{
					if (f.DataCollection == null)
					{
						f.DataCollection = Folders;
					}

					Folders.Add(f);
				}
		}

		public FolderTabEntry(AppSettings settings, string name, SettingsModel.StringWrapper basePath, IEnumerable<(string, string, string, AppSettings, string?)> folders)
			: base(name)
		{
			BasePath = basePath;
			ApplicationSettings = settings;

			SelectFolderCommand = new RelayCommand(ExecuteSelectFolderCommand);

			foreach (var f in folders)
			{
				Folders.Add(new FileEntry(f.Item1, f.Item2, f.Item3, f.Item4, f.Item5)
				{
					DataCollection = Folders
				});
			}
		}

		public FileEntry? GetEntry(string key) =>
			Folders.FirstOrDefault(f => f.Key == key);

		private void ExecuteSelectFolderCommand(object? key)
		{
			var dialog = new FolderBrowserDialog();
			dialog.InitialDirectory = BasePath.Value;

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				var folder = Folders.FirstOrDefault(f => f.Key == (string?)key);
				if (folder != null)
				{
					folder.Path = dialog.SelectedPath;

					ApplicationSettings.AddReplaceFolderPath((key as string)!, folder.Path);
					ApplicationSettings.SaveDirectories();

					if (key is string str && string.Equals(str, "Base", StringComparison.OrdinalIgnoreCase))
					{
						BasePath.Value = folder.Path;
					}
				}
			}
		}
	}

	public class FileEntry : BaseViewModel
	{
		private AppSettings ApplicationSettings { get; set; }
		public ObservableCollection<FileEntry>? DataCollection { get; set; }
		public RelayCommand SelectFileCommand { get; set; }
		public RelayCommand RemoveFileCommand { get; set; }

		private string _name;
		private string _path;
		private string? _basePath = null;
		private string _truncatedPath = string.Empty;
		private string _key;

		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}

		public string Path
		{
			get => _path;
			set
			{
				if (SetProperty(ref _path, value))
				{
					TruncatePath(_path, _basePath);
				}
			}
		}

		public string? BasePath
		{
			get => _basePath;
			private set
			{
				if (SetProperty(ref _basePath, value))
				{
					TruncatePath(Path, _basePath);
				}
			}
		}

		public string TruncatedPath
		{
			get => _truncatedPath;
			set => SetProperty(ref _truncatedPath, value);
		}

		public string Key
		{
			get => _key;
			set => SetProperty(ref _key, value);
		}

		public FileEntry(string name, string key, string path, AppSettings settings, string? basePath = null)
		{
			ApplicationSettings = settings;
			SelectFileCommand = new RelayCommand(ExecuteSelectFileCommand);
			RemoveFileCommand = new RelayCommand(ExecuteRemoveFileCommand);

			_name = name;
			_key = key;
			_path = path;
			_basePath = basePath;

			if (path != string.Empty)
			{
				TruncatePath(path, basePath);
			}
		}

		private void TruncatePath(string fullPath, string? basePath = null)
		{
			if (fullPath == string.Empty) return;

			string? trueBasePath = (basePath != string.Empty && basePath != fullPath
				? basePath : null);

			string userProfile = trueBasePath ??
				Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			string? result = "...\\" +
				System.IO.Path.GetRelativePath(userProfile, fullPath);

			TruncatedPath = result;
		}

		// Prevent accidental base path updates unless it was specifically requested.
		public void UpdateBasePath(string newPath)
		{
			BasePath = newPath;
		}

		private void ExecuteSelectFileCommand(object? parameters = null)
		{
			string? activeBase = ApplicationSettings.GetFolderPath("Base");
			OpenFileDialog filePath = new();

			filePath.Filter = "JavaScript files (*.js)|*.js|All files (*.*)|*.*";
			filePath.InitialDirectory = activeBase ?? string.Empty;
			filePath.FilterIndex = 1;
			filePath.Title = $"Select {_name} File";

			if (filePath.ShowDialog() == DialogResult.OK)
			{
				Path = filePath.FileName;

				ApplicationSettings.AddReplaceFilePath(Key, Path);
				ApplicationSettings.SaveDirectories();
			}
		}

		private void ExecuteRemoveFileCommand(object? parameters = null)
		{
			if (DataCollection == null) return;

			ApplicationSettings.RemoveFilePath(Key);
			ApplicationSettings.SaveDirectories();

			DataCollection.Remove(this);
		}
	}
}
