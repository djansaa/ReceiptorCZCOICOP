using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReceiptorCZCOICOP.ViewModels
{
    /// <summary>
    /// Base class for view models implementing INotifyPropertyChanged.
    /// </summary>
    internal abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
