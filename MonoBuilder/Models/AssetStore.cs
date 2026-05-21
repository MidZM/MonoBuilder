using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Models.image_management;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MonoBuilder.Models
{
	public class AssetStore<T> where T : INamedEntity
	{
		public ObservableCollection<T> Collection { get; } = new();
		public Dictionary<string, T> ByName { get; } = new(StringComparer.Ordinal);
		public Dictionary<int, T> ById { get; } = new();
		public int NextId { get; set; } = 0;
		public required string TypeName { get; init; }
		public string? TypeBody { get; init; }
	}
}
