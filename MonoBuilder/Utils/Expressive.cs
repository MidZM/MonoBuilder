using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoBuilder.Utils
{
    // Currently Unused
    // Will be used in the future for making characters with layered sprites.
    public class Expressive : Character
    {
        public BindingSource SpritesSource = new BindingSource();
        public BindingList<Sprite> Sprites = new BindingList<Sprite>();

        public Expressive(string name, string tag)
            : base(name, tag)
        {
            SpritesSource.DataSource = Sprites;
        }

        public Expressive(string name, string tag, string color)
            : base(name, tag, color)
        {
            SpritesSource.DataSource = Sprites;
        }

        public Expressive(string name, string tag, string color, string dictionary)
            : base(name, tag, color, dictionary)
        {
            SpritesSource.DataSource = Sprites;
        }

        public void addSprite(Sprite sprite)
        {
            Sprites.Add(sprite);
        }

        public bool removeSprite(string spriteName)
        {
            Sprite? sprite = Sprites.FirstOrDefault(s => s.Name == spriteName);
            if (sprite != null)
            {
                Sprites.Remove(sprite);
                return true;
            }

            return false;
        }

        public string? getSprite(string spriteName)
        {
            Sprite? sprite = Sprites.FirstOrDefault(s => s.Name == spriteName);
            if (sprite != null)
            {
                return (string)sprite.Layer;
            }

            return null;
        }

        public Sprite? updateSprite(string spriteName, Sprite sprite)
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

        public void addLayer(string layer, object data, Sprite Sprite)
        {
            Sprite? sprite = Sprites.FirstOrDefault(s => s == Sprite);
            if (sprite != null && sprite.Layer is Dictionary<string, object> objectSprite)
            {
                objectSprite[layer] = data;
            }
        }

        public bool removeLayer(string layer, Sprite Sprite)
        {
            Sprite? sprite = Sprites.FirstOrDefault(s => s == Sprite);
            if (
                sprite != null &&
                sprite.Layer is Dictionary<string, object> objectSprite &&
                objectSprite.ContainsKey(layer)
            )
            {
                objectSprite.Remove(layer);
                return true;
            }

            return false;
        }

        public object? getLayer(string layer, Sprite Sprite)
        {
            Sprite? sprite = Sprites.FirstOrDefault(s => s == Sprite);
            if (sprite != null)
            {
                return sprite.Layer;
            }

            return null;
        }
    }
}
