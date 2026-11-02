using System;
using System.Collections.Generic;

namespace Brave_Updater
{
    internal enum UpdateScope
    {
        Existing,
        All
    }

    internal sealed class CommandLineOptions
    {
        public bool HasCliArgs;
        public bool ShowHelp;
        public bool ShowVersion;
        public bool IsAutomation;
        public UpdateScope Scope = UpdateScope.Existing;
        public bool LaunchBrave;
        public string LaunchEdition;
        public bool QuitAfter;
        public bool Force;
        public bool CheckOnly;
        public bool DryRun;
        public bool Quiet;
        public string LogPath;
        public bool WaitForBrave;
        public int TimeoutSeconds = 300;
        public readonly List<string> BraveArguments = new List<string>();
        public string Error;

        public static CommandLineOptions Parse(string[] args)
        {
            var options = new CommandLineOptions();
            options.HasCliArgs = args != null && args.Length > 0;

            if (!options.HasCliArgs)
            {
                return options;
            }

            bool endOfOptions = false;
            bool sawIfNew = false;
            bool sawUpdate = false;
            bool sawTimeout = false;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (endOfOptions)
                {
                    options.BraveArguments.Add(arg);
                    continue;
                }

                if (arg == "--")
                {
                    endOfOptions = true;
                    continue;
                }

                if (!arg.StartsWith("--", StringComparison.Ordinal))
                {
                    options.Error = "Unexpected argument: " + arg;
                    return options;
                }

                string name;
                string value;
                int equals = arg.IndexOf('=');
                if (equals >= 0)
                {
                    name = arg.Substring(0, equals);
                    value = arg.Substring(equals + 1);
                }
                else
                {
                    name = arg;
                    value = null;
                }

                switch (name.ToLowerInvariant())
                {
                    case "--help":
                        options.ShowHelp = true;
                        break;
                    case "--version":
                        options.ShowVersion = true;
                        break;
                    case "--update":
                        sawUpdate = true;
                        value = value ?? TakeValue(args, ref i, name, options);
                        if (options.Error != null)
                        {
                            return options;
                        }
                        if (string.Equals(value, "existing", StringComparison.OrdinalIgnoreCase))
                        {
                            options.Scope = UpdateScope.Existing;
                        }
                        else if (string.Equals(value, "all", StringComparison.OrdinalIgnoreCase))
                        {
                            options.Scope = UpdateScope.All;
                        }
                        else
                        {
                            options.Error = "--update expects 'existing' or 'all'.";
                            return options;
                        }
                        break;
                    case "--launch-brave":
                        options.LaunchBrave = true;
                        if (value == null && i + 1 < args.Length && IsEditionSelector(args[i + 1]))
                        {
                            value = args[++i];
                        }
                        if (value != null)
                        {
                            if (!IsEditionSelector(value))
                            {
                                options.Error = "--launch-brave expects channel:arch, e.g. stable:x64.";
                                return options;
                            }
                            options.LaunchEdition = value.ToLowerInvariant();
                        }
                        break;
                    case "--quit-after":
                        options.QuitAfter = true;
                        break;
                    case "--if-new":
                        sawIfNew = true;
                        break;
                    case "--force":
                        options.Force = true;
                        break;
                    case "--check":
                        options.CheckOnly = true;
                        break;
                    case "--dry-run":
                        options.DryRun = true;
                        break;
                    case "--quiet":
                        options.Quiet = true;
                        break;
                    case "--log":
                        value = value ?? TakeValue(args, ref i, name, options);
                        if (options.Error != null)
                        {
                            return options;
                        }
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            options.Error = "--log requires a path.";
                            return options;
                        }
                        options.LogPath = value;
                        break;
                    case "--wait-for-brave":
                        options.WaitForBrave = true;
                        break;
                    case "--timeout":
                        sawTimeout = true;
                        value = value ?? TakeValue(args, ref i, name, options);
                        if (options.Error != null)
                        {
                            return options;
                        }
                        int timeout;
                        if (!int.TryParse(value, out timeout) || timeout <= 0)
                        {
                            options.Error = "--timeout expects a positive number of seconds.";
                            return options;
                        }
                        options.TimeoutSeconds = timeout;
                        break;
                    default:
                        options.Error = "Unknown option: " + name;
                        return options;
                }
            }

            if (options.ShowHelp || options.ShowVersion)
            {
                return options;
            }

            options.IsAutomation = true;

            if (sawIfNew && options.Force)
            {
                options.Error = "--if-new and --force cannot be combined.";
                return options;
            }

            if (options.LaunchBrave && (options.CheckOnly || options.DryRun))
            {
                options.Error = "--launch-brave cannot be combined with --check or --dry-run.";
                return options;
            }

            if (sawTimeout && !options.WaitForBrave)
            {
                options.Error = "--timeout requires --wait-for-brave.";
                return options;
            }

            if (options.BraveArguments.Count > 0 && !options.LaunchBrave)
            {
                options.Error = "Arguments after '--' require --launch-brave.";
                return options;
            }

            if (!sawUpdate)
            {
                options.Scope = UpdateScope.Existing;
            }

            return options;
        }

        private static string TakeValue(string[] args, ref int index, string option, CommandLineOptions options)
        {
            if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                options.Error = option + " requires a value.";
                return null;
            }

            index++;
            return args[index];
        }

        private static bool IsEditionSelector(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string[] parts = value.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            string channel = parts[0].ToLowerInvariant();
            string arch = parts[1].ToLowerInvariant();

            bool validChannel = channel == "nightly" || channel == "dev" || channel == "beta" || channel == "stable";
            bool validArch = arch == "x86" || arch == "x64";
            return validChannel && validArch;
        }

        public const string HelpText =
@"Portable Brave Updater

Usage:
  Portable Brave Updater.exe [options] [-- Brave arguments...]

Options:
  --update=existing|all     Update scope (default: existing)
  --launch-brave[=ch:arch]  Start Brave after a successful update
  --quit-after              Exit after completion
  --if-new                  Update only when a newer version exists (default)
  --force                   Reinstall even if the same version is installed
  --check                   Check versions only, do not install
  --dry-run                 Show the plan, do not install
  --quiet                   No GUI or dialogs; logs to stdout/stderr
  --log <path>              Also write a log file
  --wait-for-brave          Wait while Brave is running
  --timeout <seconds>       Maximum wait time (default: 300)
  --help                    Show this help
  --version                 Show the application version

Exit codes:
  0 success / no action      1 general error
  2 invalid arguments        3 Brave still running / timeout
  4 network or check error   5 update available (--check)
  6 launch failed or edition not unique
";
    }
}
