using System;
using System.Threading.Tasks;

namespace iChronoMe.Abstractions
{
    public interface IUserInputHandler
    {
        //Task<string> GetText(string title, Keyboard keyboard, string cPlaceholder = null, string cDefault = null);

        Task<DateTime?> GetDate(string cTitle, DateTime? tStart = null);
    }

    public static class xUserInput
    {
        public static IUserInputHandler Instance { get; set; }
    }
}
