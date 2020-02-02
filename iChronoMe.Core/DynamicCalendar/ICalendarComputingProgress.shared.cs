namespace iChronoMe.Core.DynamicCalendar
{
    public interface IComputingDialog
    {
        void Prepare(string cText, bool OpenNow = false);

        void SetDone();

        void SetText(string cText);

        void ForceClose();
    }
}
