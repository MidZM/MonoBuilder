using System;
using System.Collections.Generic;
using System.Text;

namespace MonoBuilder.Models.generics.interfaces
{
    public interface INamedEntity
    {
        string Name { get; set; }
        int EntityID { get; set; }
    }
}
