using System.Threading.Tasks;

using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;

using Xamarin.Essentials;

namespace iChronoMe.Core.Interfaces
{
    public interface IUserIO : IProgressChangedHandler
    {
        void TriggerSingleChoiceClicked(int which);
        void TriggerPositiveButtonClicked();
        void TriggerNegativeButtonClicked();
        void TriggerDialogCanceled();
        void TriggerAbortProzess();
        Task<SelectPositionResult> UserSelectMapsLocation(Location center = null, Location marker = null);
        Task<bool> UserShowYesNoMessage(string title, string message, string yes = null, string no = null);
        Task<bool> UserShowYesNoMessage(int title, int message, int? yes = null, int? no = null);
        Task<xColor?> UserSelectColor(int title, xColor? current = null, xColor[] colors = null, bool allowCustom = true, bool allowAlpha = true);
        Task<xColor?> UserSelectColor(string title, xColor? current = null, xColor[] colors = null, bool allowCustom = true, bool allowAlpha = true);
        Task<string> UserInputText(string title, string message, string placeholder);
    }
}
