using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Brave_Updater
{
    internal static class ConsoleHelper
    {
        private const int AttachParentProcess = -1;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        public static void Attach()
        {
            if (!AttachConsole(AttachParentProcess))
            {
                AllocConsole();
            }

            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
        }
    }
}
