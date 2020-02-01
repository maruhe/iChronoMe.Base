using System;
using System.Collections.Generic;
using System.Text;

namespace iChronoMe.Core.Interfaces
{
    public interface IProgressChangedHandler
    {
        void StartProgress(string cTitle);

        void SetProgress(int progress, int max, string cMessage);

        void SetProgressDone();

        void ShowError(string cMessage);
    }
}
