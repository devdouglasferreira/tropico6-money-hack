using System;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media;
using Tropico6MoneyTrainer.Core;
using Tropico6MoneyTrainer.ViewModels.Abstractions;

namespace Tropico6MoneyTrainer.ViewModels
{
    public class MainWindowViewModel : BaseViewModel, IDisposable
    {
        private readonly Timer _setupTimer, _updateTimer;
        private Tropico6MemoryProcessWriter _trainer;
        private ICommand _increaseTreasuryCommand;
        private ICommand _increaseSwissBankCommand;

        public MainWindowViewModel()
        {
            _trainer = new Tropico6MemoryProcessWriter();

            _setupTimer = new Timer { AutoReset = true, Interval = 30 * 1000 };
            _setupTimer.Elapsed += (s, e) =>
            {
                if (!_trainer.IsSetupComplete)
                    TryLoadProcessAndTargetMemoryAddress();
                else
                    _setupTimer.Stop();
            };
            _setupTimer.Start();

            _updateTimer = new Timer { AutoReset = true, Interval = 1000 };
            _updateTimer.Elapsed += (s, e) => UpdateGameValues();
            _updateTimer.Start();

            TryLoadProcessAndTargetMemoryAddress();
        }

        public SolidColorBrush StatusTextColor { get; set; }
        public string StatusMessage { get; set; }
        public float Treasury { get; set; }
        public float SwissBank { get; set; }
        public float TreasuryIncrease { get; set; }
        public float SwissBankIncrease { get; set; }

        public bool TryLoadProcessAndTargetMemoryAddress()
        {
            if (!_trainer.IsGameLoaded)
                _ = _trainer.TryLoadProcess();

            if (!_trainer.IsTargetAddressfound)
                _ = _trainer.TryGetTargetMemoryPointers();

            UpdateStatusText();

            return _trainer.IsGameLoaded & _trainer.IsTargetAddressfound;
        }

        public void UpdateGameValues()
        {
            Treasury = _trainer.GetTreasury();
            OnPropertyChanged(nameof(Treasury));
        }

        public void UpdateStatusText()
        {
            if (!_trainer.IsGameLoaded)
            {
                StatusTextColor = new SolidColorBrush(Colors.Red);
                StatusMessage = StatusMessages.GameNotLoaded;

            }
            else if (_trainer.IsGameLoaded && !_trainer.IsTargetAddressfound)
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
        
        public ICommand IncreaseTreasuryCommand => _increaseTreasuryCommand ??= new CommandHandler(IncreaseTreasury, () => true);

        public ICommand IncreaseSwissBankCommand => _increaseSwissBankCommand ??= new CommandHandler(IncreaseSwissBank, () => true);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void IncreaseTreasury()
        {
            float treasuryIncrease = Treasury + TreasuryIncrease;
            _trainer.OverrideTreasure(treasuryIncrease);
            OnPropertyChanged(nameof(Treasury));
        }
        private void IncreaseSwissBank()
        {
            float swissBankIncrease = SwissBank + SwissBankIncrease;
            _trainer.OverrideTreasure(swissBankIncrease);
            OnPropertyChanged(nameof(Treasury));
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _setupTimer.Dispose();
                _trainer.Dispose();
            }
        }

        ~MainWindowViewModel()
        {
            Dispose(false);
        }
    }
}
