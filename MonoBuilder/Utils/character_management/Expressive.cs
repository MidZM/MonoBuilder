using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonoBuilder.Utils.character_management
{
    // Currently Unused
    // Will be used in the future for making characters with layered sprites.
    public class Expressive : Character
    {
        public ObservableCollection<Sprite> Sprites { get; set; } = new ObservableCollection<Sprite>();

        public Expressive(string name, string tag)
            : base(name, tag)
        {
        }

        public Expressive(string name, string tag, string color)
            : base(name, tag, color)
        {
        }

        public Expressive(string name, string tag, string color, string dictionary)
            : base(name, tag, color, dictionary)
        {
        }

        public void AddSprite(Sprite sprite)
        {
            Sprites.Add(sprite);
        }

        public bool RemoveSprite(string spriteName)
        {
            Sprite? sprite = Sprites.FirstOrDefault(s => s.Name == spriteName);
            if (sprite != null)
            {
                Sprites.Remove(sprite);
                return true;
            }

            return false;
        }

        public string? GetImageSprite(string spriteName)
        {
            Sprite? sprite = Sprites.FirstOrDefault(s => s.Name == spriteName);
            if (sprite != null)
            {
                return (string)sprite.ImageLayer;
            }

            return null;
        }

        public Sprite? UpdateSprite(string spriteName, Sprite sprite)
        {
            Sprite? oldSprite = Sprites.FirstOrDefault(s => s.Name == spriteName);
            if (oldSprite != null)
            {
                int index = Sprites.IndexOf(oldSprite);
                Sprites.Remove(oldSprite);
                Sprites.Insert(index, sprite);
                return sprite;
            }

            return null;
        }

        //public void AddLayer(string layer, object data, Sprite Sprite)
        //{
        //    Sprite? sprite = Sprites.FirstOrDefault(s => s == Sprite);
        //    if (sprite != null && sprite.Layer is Dictionary<string, object> objectSprite)
        //    {
        //        objectSprite[layer] = data;
        //    }
        //}

        //public bool RemoveLayer(string layer, Sprite Sprite)
        //{
        //    Sprite? sprite = Sprites.FirstOrDefault(s => s == Sprite);
        //    if (
        //        sprite != null &&
        //        sprite.Layer is Dictionary<string, object> objectSprite &&
        //        objectSprite.ContainsKey(layer)
        //    )
        //    {
        //        objectSprite.Remove(layer);
        //        return true;
        //    }

        //    return false;
        //}

        //public object? GetLayer(string layer, Sprite Sprite)
        //{
        //    Sprite? sprite = Sprites.FirstOrDefault(s => s == Sprite);
        //    if (sprite != null)
        //    {
        //        return sprite.Layer;
        //    }

        //    return null;
        //}
    }
}
