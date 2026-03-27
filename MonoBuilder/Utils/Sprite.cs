using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoBuilder.Utils
{
    public class Sprite
    {
        public string Name { get; set; }
        public object Layer { get; set; }

        public Sprite(string name, object layer)
        {
            Name = name;
            Layer = layer;
        }
    }
}
