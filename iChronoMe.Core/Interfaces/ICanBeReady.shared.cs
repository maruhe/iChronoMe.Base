using System;

namespace iChronoMe.Core.Interfaces
{
    public interface ICanBeReady
    {
        bool IsReady { get; }
        bool HasErrors { get; }

        event EventHandler Ready;
    }
}
