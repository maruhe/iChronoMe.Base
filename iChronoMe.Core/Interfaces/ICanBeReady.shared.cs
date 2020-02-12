using System;
using System.Collections.Generic;
using System.Text;

namespace iChronoMe.Core.Interfaces
{
    public interface ICanBeReady
    {
        bool IsReady { get; }
        bool HasErrors { get; }
    }
}
