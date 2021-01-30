using System;
using System.Diagnostics;
using System.Linq;
using Tropico6MoneyTrainer.Core.Helpers;

namespace Tropico6MoneyTrainer.Core
{
    public class Tropico6MemoryProcessWriter : IDisposable
    {
        private readonly int _treasuryFirstOffset = 0x03CE4AD0;
        private readonly int[] _treasuryOffsets = { 0x30, 0x8F0, 0xE30, 0x498, 0x3B8, 0x230, 0x9D8 };
        
        private Process _gameProcess;
        private IntPtr _treasuryMemoryAddress;
        private IntPtr _swissBankAccountAddress;

        public bool IsGameLoaded { get; set; }
        public bool IsTargetAddressfound { get; set; }
        public bool IsSetupComplete => IsGameLoaded & IsTargetAddressfound;

        public bool TryLoadProcess()
        {
            _gameProcess = Process.GetProcessesByName("Tropico6-Win64-Shipping").FirstOrDefault();

            if (_gameProcess != null)
                IsGameLoaded = true;

            return IsGameLoaded;
        }

        public bool TryGetTargetMemoryPointers()
        {
            try
            {
                long baseAddress = _gameProcess?.MainModule?.BaseAddress.ToInt64() + _treasuryFirstOffset ?? 0;
                long processingAddress = baseAddress;

                foreach (var offset in _treasuryOffsets)
                    processingAddress = ExternalMemoryAccess.ReadInt64(_gameProcess.Handle, (IntPtr)processingAddress) + offset;

                _treasuryMemoryAddress = (IntPtr)processingAddress;

                IsTargetAddressfound = true;
                return IsTargetAddressfound;
            }
            catch 
            {
                return IsTargetAddressfound;
            }
        }

        public float GetTreasury()
        {
            byte[] buffer = new byte[4];
            _ = ExternalMemoryAccess.ReadProcessMemory(_gameProcess.Handle, _treasuryMemoryAddress, buffer, buffer.Length, out _);
            return BitConverter.ToSingle(buffer, 0);
        }

        public void OverrideTreasure(float amount)
        {
            byte[] buffer = BitConverter.GetBytes(amount);
            ExternalMemoryAccess.WriteProcessMemory(_gameProcess.Handle, _treasuryMemoryAddress, buffer, buffer.Length, out _);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                _gameProcess.Dispose();
            }
        }

        private void ReleaseUnmanagedResources()
        {
            _gameProcess.Dispose();
            _treasuryMemoryAddress = IntPtr.Zero;
            _swissBankAccountAddress = IntPtr.Zero;
        }

        ~Tropico6MemoryProcessWriter()
        {
            Dispose(false);
        }
    }
}
