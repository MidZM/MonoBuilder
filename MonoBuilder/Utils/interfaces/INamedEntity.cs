using System;
using System.Collections.Generic;
using System.Text;

namespace MonoBuilder.Utils.interfaces
{
    public interface INamedEntity
    {
        string Name { get; set; }
        int EntityID { get; set; }
    }
}
