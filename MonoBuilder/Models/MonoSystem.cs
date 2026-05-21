using MonoBuilder.Models.generics.interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace MonoBuilder.Models
{
	public abstract class MonoSystem
	{
		protected AppSettings? ApplicationSettings { get; set; }
		protected XDocument SystemData { get; set; } = new();
		protected Dictionary<string, List<string>> ContentGuides { get; set; } = new();
		public virtual string DataModeTypeName => string.Empty;

		public abstract void LoadData();
		public abstract void SaveData();
	}

	public abstract class MonoSystem<T> : MonoSystem where T : INamedEntity
	{
		public virtual AssetStore<T> DataMode { get; set; } = new() { TypeName = string.Empty };
		public override string DataModeTypeName => DataMode.TypeName;
	}
}
