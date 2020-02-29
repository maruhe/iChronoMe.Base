using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace iChronoMe.Core.DataBinding
{
    public abstract class BaseObservable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
