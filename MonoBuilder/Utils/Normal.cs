using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoBuilder.Utils
{
    public class Normal : Character
    {
        public BindingSource SpritesSource = new BindingSource();
        public BindingList<Sprite> Sprites = new BindingList<Sprite>();

        public Normal(string name, string tag)
            : base(name, tag)
        {
            SpritesSource.DataSource = Sprites;
        }

        public Normal(string name, string tag, string color)
            : base(name, tag, color)
        {
            SpritesSource.DataSource = Sprites;
        }

        public Normal(string name, string tag, string color, string dictionary)
            : base(name, tag, color, dictionary)
        {
            SpritesSource.DataSource = Sprites;
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

        public string? GetSprite(string spriteName)
        {
            Sprite? sprite = Sprites.FirstOrDefault(s => s.Name == spriteName);
            if (sprite != null)
            {
                return (string)sprite.Layer;
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
    }
}
