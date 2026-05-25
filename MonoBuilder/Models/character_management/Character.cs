using MonoBuilder.Models.generics.interfaces;
using System;
using System.Collections.ObjectModel;

namespace MonoBuilder.Models.character_management
{
	public class Character : BaseViewModel, INamedEntity, IMultiFile
    {
        private int _id;
        private string _name = "";
        private string _tag = "";
        private string? _color;
        private string? _directory;
        private string _fileKey = "";
        private bool _synced = false;

		public int EntityID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Tag
        {
            get => _tag;
            set => SetProperty(ref _tag, value);
        }

        public string? Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public string? Directory
        {
            get => _directory;
            set => SetProperty(ref _directory, value);
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

		public ObservableCollection<Sprite> Sprites { get; set; } = new ObservableCollection<Sprite>();

		public Character(string name, string tag)
        {
            Name = name;
            Tag = tag;
        }

        public Character(string name, string tag, string colorOrDirectory)
        {
            Name = name;
            Tag = tag;
            if (colorOrDirectory.StartsWith("#"))
            {
                Color = colorOrDirectory;
            }
            else
            {
                Directory = colorOrDirectory;
            }
        }

        public Character(string name, string tag, string? color, string? directory)
        {
            Name = name;
            Tag = tag;
            if (color != null) Color = color;
            if (directory != null) Directory = directory;
		}

		public void AddSprite(Sprite sprite)
		{
			Sprites.Add(sprite);
		}

		public bool RemoveSprite(string spriteName)
		{
			if (Sprites.FirstOrDefault(s => s.Name == spriteName) is Sprite sprite)
			{
				return Sprites.Remove(sprite);
			}

			return false;
		}

		public Sprite? GetSprite(string spriteName)
		{
			return Sprites.FirstOrDefault(s => s.Name == spriteName);
		}

		public Sprite? UpdateSprite(string spriteName, Sprite sprite)
		{
			if (Sprites.FirstOrDefault(s => s.Name == spriteName) is Sprite oldSprite)
			{
				int index = Sprites.IndexOf(oldSprite);
				Sprites.RemoveAt(index);
				Sprites.Insert(index, sprite);
				return sprite;
			}

			return null;
		}

		public bool AddLayer(Sprite sprite, Layer layer)
		{
			if (sprite.Layers.FirstOrDefault(l => l.Name == layer.Name) is null)
			{
				sprite.Layers.Add(layer);
				return true;
			}

			return false;
		}

		public bool RemoveLayer(Sprite sprite, string layerName)
		{
			if (sprite.Layers.FirstOrDefault(l => l.Name == layerName) is Layer layer)
			{
				return sprite.Layers.Remove(layer);
			}

			return false;
		}

		public Layer? GetLayer(Sprite sprite, string layerName)
		{
			return sprite.Layers.FirstOrDefault(l => l.Name == layerName);
		}

		public Layer? UpdateLayer(Sprite sprite, string layerName, Layer layer)
		{
			if (sprite.Layers.FirstOrDefault(l => l.Name == layerName) is Layer oldLayer)
			{
				int index = sprite.Layers.IndexOf(oldLayer);
				sprite.Layers.RemoveAt(index);
				sprite.Layers.Insert(index, layer);
				return layer;
			}

			return null;
		}
	}
}
