using System;
using System.Collections.Generic;
using System.Text;

namespace MonoBuilder.Models.generics.interfaces
{
    public interface IMultiFile
    {
        string FileKey { get; set; }
		bool IsSynced { get; set; }
    }
}
