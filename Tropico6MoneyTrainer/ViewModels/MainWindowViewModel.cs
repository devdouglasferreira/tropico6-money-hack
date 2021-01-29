using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Tropico6MoneyTrainer.Annotations;
using Tropico6MoneyTrainer.Core;

namespace Tropico6MoneyTrainer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private Tropico6MemoryProcessWriter _trainer;
        public SolidColorBrush StatusTextColor { get; set; }
        public string StatusMessage { get; set; }

        public MainWindowViewModel()
        {
            StatusTextColor = new SolidColorBrush(Colors.Red);
            StatusMessage = StatusMessages.GameNotLoaded;
            _trainer = new Tropico6MemoryProcessWriter();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
