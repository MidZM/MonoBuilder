using MonoBuilder.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Data;

namespace MonoBuilder.Models.generics.interfaces
{
    public interface IBuilderTabbedModel<T> : IBuilderBaseModel where T : IMultiFile
	{
		public ObservableCollection<string> AvailableFiles { get; set; }

		public TabEntries DataTabs { get; set; }
		public abstract TabEntry? SelectedTab { get; set; }
		public abstract T? SelectedEntity { get; set; }
		public abstract ObservableCollection<T> SelectedEntities { get; set; }
		public CollectionViewSource? DataViewSource { get; set; }

		public abstract string GetDefaultDataFileKey(string type);
	}
}
