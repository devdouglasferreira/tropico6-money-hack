using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Tropico6MoneyTrainer.Core
{
    public class Tropico6MemoryProcessWriter : IDisposable
    {
        private int[] _offsets = new int[] { 0x03CE4AD0, 0x8F0, 0xE30, 0x498, 0x3B8, 0x230, 0x9D8 };
        
        private IntPtr _gameBaseMemoryAddress;
        private Process _gameProcess;
        public bool IsGameLoaded { get; set; }
        public bool IsTargetAddresfound { get; set; }

        public Tropico6MemoryProcessWriter() { }

        public bool TryLoadProcess()
        {
            _gameProcess = Process.GetProcesses("Tropico6")?.FirstOrDefault();
            _gameBaseMemoryAddress = _gameProcess?.MainModule?.BaseAddress ?? default;

            if (_gameProcess != null && _gameBaseMemoryAddress != default)
            {
                IsGameLoaded = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        
        [DllImport("kernetl32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);

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
            _gameBaseMemoryAddress = IntPtr.Zero;
            _gameProcess.Dispose();
        }

        ~Tropico6MemoryProcessWriter()
        {
            Dispose(false);
        }
    }
}
