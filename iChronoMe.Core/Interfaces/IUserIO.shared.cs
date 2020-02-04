using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using iChronoMe.Core.Classes;

namespace iChronoMe.Core.Interfaces
{
    public interface IUserIO : IProgressChangedHandler
    {
        void TriggerSingleChoiceClicked(int which);
        void TriggerNegativeButtonClicked();
        void TriggerDialogCanceled();
        Task<SelectPositionResult> TriggerSelectMapsLocation();
    }
}
