using System;
using System.Runtime.InteropServices;

namespace Tropico6MoneyTrainer.Core.Helpers
{
    public static class ExternalMemoryAccess
    {
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);

        public static long ReadInt64(IntPtr processHandle, IntPtr address)
        {
            byte[] buffer = new byte[8];
            ReadProcessMemory(processHandle, address, buffer, buffer.Length, out IntPtr bytesRead);
            
            return BitConverter.ToInt64(buffer, 0);
        }
    }
}
