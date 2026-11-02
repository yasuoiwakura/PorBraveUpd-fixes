using Brave_Updater;
using System;
using System.IO;
using System.Windows.Forms;

namespace Portable_Brave_Updater
{
    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                ConsoleHelper.Attach();
            }

            CommandLineOptions options = CommandLineOptions.Parse(args);

            if (options.ShowHelp)
            {
                Console.WriteLine(CommandLineOptions.HelpText);
                return 0;
            }

            if (options.ShowVersion)
            {
                Console.WriteLine(Application.ProductVersion);
                return 0;
            }

            if (!string.IsNullOrEmpty(options.Error))
            {
                Console.Error.WriteLine("Error: " + options.Error);
                Console.Error.WriteLine("Use --help for usage.");
                return 2;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);

            if (options.IsAutomation)
            {
                Form1 form = new Form1(options);
                Application.Run(form);
                return form.CliExitCode;
            }

            Application.Run(new Form1());
            return 0;
        }
    }
}
