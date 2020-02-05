namespace iChronoMe.Core.Interfaces
{
    public interface IProgressChangedHandler
    {
        void StartProgress(string cTitle);
        void SetProgress(int progress, int max, string cMessage);
        void SetProgressDone();
        void ShowToast(string cMessage);
        void ShowError(string cMessage);
    }
}
