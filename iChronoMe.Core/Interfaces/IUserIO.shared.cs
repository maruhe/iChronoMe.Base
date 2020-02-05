using System.Threading.Tasks;

using iChronoMe.Core.Classes;

namespace iChronoMe.Core.Interfaces
{
    public interface IUserIO : IProgressChangedHandler
    {
        void TriggerSingleChoiceClicked(int which);
        void TriggerNegativeButtonClicked();
        void TriggerDialogCanceled();
        void TriggerAbortProzess();
        Task<SelectPositionResult> TriggerSelectMapsLocation();
    }
}
