using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoBuilder.Utils
{
    public abstract class Character
    {
        public int CharacterID { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public string? Color { get; set; }
        public string? Directory { get; set; }

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

        public Character(string name, string tag, string color, string directory)
        {
            Name = name;
            Tag = tag;
            Color = color;
            Directory = directory;
        }
    }
}
