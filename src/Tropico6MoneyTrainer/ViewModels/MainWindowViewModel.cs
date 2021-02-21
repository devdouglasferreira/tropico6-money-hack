using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Tropico6MoneyTrainer.Core;
using Tropico6MoneyTrainer.ViewModels.Abstractions;

namespace Tropico6MoneyTrainer.ViewModels
{
    public class MainWindowViewModel : BaseViewModel, IDisposable
    {
        private readonly DispatcherTimer _setupTimer, _updateTimer;
        private readonly Tropico6MemoryProcessWriter _trainer;

        private ICommand _increaseTreasuryCommand;
        private ICommand _increaseSwissBankCommand;

        public MainWindowViewModel()
        {
            _trainer = new Tropico6MemoryProcessWriter();

            _setupTimer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher.CurrentDispatcher);
            _setupTimer.Interval = new TimeSpan(0, 0, 0, 3);
            _setupTimer.Tick += UpdateSetupStatus;
            _setupTimer.Start();

            _updateTimer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher.CurrentDispatcher);
            _updateTimer.Interval = new TimeSpan(0, 0, 0, 1);
            _updateTimer.Tick += UpdateGameValues;

            TryLoadProcessAndTargetMemoryAddress();
        }
        ~MainWindowViewModel()
        {
            Dispose(false);
        }

        public SolidColorBrush StatusTextColor { get; set; }
        public string StatusMessage { get; set; }
        public float Treasury { get; set; }
        public float SwissBank { get; set; }
        public float TreasuryIncrease { get; set; }
        public float SwissBankIncrease { get; set; }
        public bool IsReady { get; set; }

        public ICommand IncreaseTreasuryCommand => _increaseTreasuryCommand ??= new CommandHandler(IncreaseTreasury, () => true);
        public ICommand IncreaseSwissBankCommand => _increaseSwissBankCommand ??= new CommandHandler(IncreaseSwissBank, () => true);

        private void TryLoadProcessAndTargetMemoryAddress()
        {
            if (!_trainer.IsGameLoaded)
                _ = _trainer.TryLoadProcess();

            if (_trainer.IsGameLoaded && !_trainer.IsTargetAddressesFound)
                _ = _trainer.TryGetTargetMemoryPointers();

            UpdateStatusText();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void UpdateStatusText()
        {
            if (!_trainer.IsGameLoaded)
            {
                StatusTextColor = new SolidColorBrush(Colors.Red);
                StatusMessage = StatusMessages.GameNotLoaded;
                IsReady = false;

            }
            else if (_trainer.IsGameLoaded && !_trainer.IsTargetAddressesFound)
            {
                StatusTextColor = new SolidColorBrush(Color.FromRgb(255, 192, 0));
                StatusMessage = StatusMessages.ScenarioMemoryTargetNotFound;
                IsReady = false;
            }
            else
            {
                StatusTextColor = new SolidColorBrush(Colors.ForestGreen);
                StatusMessage = StatusMessages.Success;
                IsReady = true;
                UpdateGameValues();
            }

            OnPropertyChanged(nameof(IsReady));
            OnPropertyChanged(nameof(StatusMessage));
            OnPropertyChanged(nameof(StatusTextColor));
        }

        private void UpdateGameValues()
        {
            if (_trainer.IsSetupComplete)
            {
                Treasury = _trainer.GetTreasury();
                SwissBank = _trainer.GetSwissBankAccount();
                _trainer.CheckTargetEmptiness(Treasury, SwissBank);
                OnPropertyChanged(nameof(Treasury));
                OnPropertyChanged(nameof(SwissBank));
            }
            else
            {
                _setupTimer.Start();
                _updateTimer.Stop();

                OnPropertyChanged(nameof(StatusMessage));
                OnPropertyChanged(nameof(StatusTextColor));
            }
        }

        private void UpdateGameValues(object sender, EventArgs args)
        {
            UpdateGameValues();
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
            _trainer.OverrideSwissBankAccount(swissBankIncrease);
            OnPropertyChanged(nameof(SwissBank));
        }

        private void UpdateSetupStatus(object sender, EventArgs args)
        {
            if (!_trainer.IsSetupComplete)
                TryLoadProcessAndTargetMemoryAddress();
            else
            {
                _setupTimer.Stop();
                _updateTimer.Start();
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _trainer.Dispose();
            }
        }
    }
}
