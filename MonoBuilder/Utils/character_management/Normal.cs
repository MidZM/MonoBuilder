using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonoBuilder.Utils.character_management
{
    public class Normal : Character
    {
        public ObservableCollection<Sprite> Sprites { get; set; } = new ObservableCollection<Sprite>();

        public Normal(string name, string tag)
            : base(name, tag) {}

        public Normal(string name, string tag, string color)
            : base(name, tag, color) {}

        public Normal(string name, string tag, string? color, string? dictionary)
            : base(name, tag, color, dictionary) {}

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
