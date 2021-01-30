using System;
using System.Timers;
using System.Windows.Media;
using Tropico6MoneyTrainer.Core;

namespace Tropico6MoneyTrainer.ViewModels
{
    public class MainWindowViewModel : BaseViewModel, IDisposable
    {
        private readonly Timer _setupTimer, _updateTimer;
        private Tropico6MemoryProcessWriter _trainer;
        
        public SolidColorBrush StatusTextColor { get; set; }
        public string StatusMessage { get; set; }
        public float BankAccount { get; set; }

        public MainWindowViewModel()
        {
            _trainer = new Tropico6MemoryProcessWriter();

            _setupTimer = new Timer { AutoReset = true, Interval = 30 * 1000 };
            _setupTimer.Elapsed += (s, e) =>
            {
                if(!_trainer.IsSetupComplete)
                    TryLoadProcessAndTargetMemoryAddress();
                else 
                    _setupTimer.Stop();
            };
            _setupTimer.Start();
            
            _updateTimer = new Timer {AutoReset = true, Interval = 1000};
            _updateTimer.Elapsed += (s, e) => UpdateGameValues();
            _updateTimer.Start();

            TryLoadProcessAndTargetMemoryAddress();
        }

        public bool TryLoadProcessAndTargetMemoryAddress()
        {
            if(!_trainer.IsGameLoaded)
              _ = _trainer.TryLoadProcess();

            if (!_trainer.IsTargetAddressfound)
                _ = _trainer.TryGetTargetMemoryPointers();

            UpdateStatusText();

            return _trainer.IsGameLoaded & _trainer.IsTargetAddressfound;
        }

        public void UpdateGameValues()
        {
            BankAccount = _trainer.GetTreasury();
            OnPropertyChanged(nameof(BankAccount));
        }

        public void UpdateStatusText()
        {
            if (!_trainer.IsGameLoaded)
            {
                StatusTextColor = new SolidColorBrush(Colors.Red);
                StatusMessage = StatusMessages.GameNotLoaded;

            } else if (_trainer.IsGameLoaded && !_trainer.IsTargetAddressfound)
            {
                StatusTextColor = new SolidColorBrush(Color.FromRgb(255, 192, 0));
                StatusMessage = StatusMessages.ScenarioMemoryTargetNotFound;
            }
            else
            {
                StatusTextColor = new SolidColorBrush(Colors.ForestGreen);
                StatusMessage = StatusMessages.Success;
                UpdateGameValues();
            }

            OnPropertyChanged(nameof(StatusMessage));
            OnPropertyChanged(nameof(StatusTextColor));
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _setupTimer.Dispose();
                _trainer.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MainWindowViewModel()
        {
            Dispose(false);
        }
    }
}
