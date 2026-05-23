using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Models.helpers;
using MonoBuilder.Models.image_management;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MonoBuilder.Models
{
	public interface IAssetStore
	{
		int NextId { get; set; }
		string TypeName { get; init; }
		string? TypeBody { get; init; }
		ContentGuide MasterGuideContent { get; init; }
		ContentGuide? ChildGuideContent { get; init; }
	}

	public class AssetStore<T> : IAssetStore where T : INamedEntity
	{
		public ObservableCollection<T> Collection { get; } = new();
		public Dictionary<string, T> ByName { get; } = new(StringComparer.Ordinal);
		public Dictionary<int, T> ById { get; } = new();
		public required ContentGuide MasterGuideContent { get; init; }
		public ContentGuide? ChildGuideContent { get; init; }
		public int NextId { get; set; } = 0;
		public required string TypeName { get; init; }
		public string? TypeBody { get; init; }
	}
}
