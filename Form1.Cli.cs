using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brave_Updater
{
    public partial class Form1
    {
        private sealed class CliEdition
        {
            public int Index;
            public string Channel;
            public string Arch;
            public string Dir;
            public bool SingleFolder;
            public string InstalledVersion;
        }

        public int CliExitCode { get; private set; }

        private async Task RunCliAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(cliOptions.LogPath))
                {
                    string fullPath = Path.GetFullPath(cliOptions.LogPath);
                    string directory = Path.GetDirectoryName(fullPath);
                    if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                    {
                        cliOptions.LogPath = null;
                        LogError("Log directory does not exist: " + directory);
                        CliExitCode = 2;
                        return;
                    }
                }

                if (initializationException != null)
                {
                    LogError("Version check failed: " + initializationException.Message);
                    CliExitCode = 4;
                    return;
                }

                if (cliOptions.CheckOnly || cliOptions.DryRun)
                {
                    CliExitCode = await RunCheckAsync();
                    return;
                }

                List<CliEdition> installed = GetInstalledEditions();
                List<CliEdition> targets = GetTargets(installed);

                if (targets.Count == 0)
                {
                    if (cliOptions.LaunchBrave)
                    {
                        CliExitCode = LaunchBrave(installed, new List<CliEdition>());
                    }
                    else
                    {
                        LogInfo("No installed edition found.");
                        CliExitCode = 0;
                    }
                    return;
                }

                var launcherNames = new List<string>();
                foreach (CliEdition edition in targets)
                {
                    if (NeedsAction(edition))
                    {
                        launcherNames.Add(edition.SingleFolder
                            ? "Brave Launcher.exe"
                            : instDir[edition.Index] + " Launcher.exe");
                    }
                }

                if (launcherNames.Count > 0)
                {
                    List<string> missing = GetMissingRuntimeFiles(launcherNames);
                    if (missing.Count > 0)
                    {
                        LogError(FormatMissingRuntimeFiles(missing));
                        CliExitCode = 7;
                        return;
                    }
                }

                if (!await EnsureBraveNotRunningAsync())
                {
                    CliExitCode = 3;
                    return;
                }

                var updated = new List<CliEdition>();
                foreach (CliEdition edition in targets)
                {
                    if (await UpdateEditionAsync(edition))
                    {
                        updated.Add(edition);
                    }
                }

                if (cliOptions.LaunchBrave)
                {
                    CliExitCode = LaunchBrave(installed, updated);
                }
                else
                {
                    CliExitCode = 0;
                }
            }
            catch (WebException ex)
            {
                LogError("Network error: " + ex.Message);
                CliExitCode = 4;
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                CliExitCode = 1;
            }
            finally
            {
                Close();
            }
        }

        private Task<int> RunCheckAsync()
        {
            List<CliEdition> installed = GetInstalledEditions();
            List<CliEdition> targets = GetTargets(installed);

            if (cliOptions.Scope == UpdateScope.Existing && targets.Count == 0)
            {
                LogInfo("No installed edition found.");
                return Task.FromResult(0);
            }

            bool action = false;
            foreach (CliEdition edition in targets)
            {
                string available = buildversion[edition.Index];
                bool needsAction = edition.InstalledVersion == null
                    || !string.Equals(edition.InstalledVersion, available, StringComparison.OrdinalIgnoreCase)
                    || (cliOptions.Force && edition.InstalledVersion != null);
                action |= needsAction;

                string label = DisplayName(edition);
                LogInfo("Installed: " + label + " " + (edition.InstalledVersion ?? "not installed"));
                LogInfo("Available: " + label + " " + available);
                string planned = needsAction
                    ? (edition.InstalledVersion == null ? "install" : "update")
                    : "none";
                LogInfo((cliOptions.DryRun ? "Would: " : "Action: ") + planned);
            }

            return Task.FromResult(action ? 5 : 0);
        }

        private bool NeedsAction(CliEdition edition)
        {
            string available = buildversion[edition.Index];
            return edition.InstalledVersion == null
                || !string.Equals(edition.InstalledVersion, available, StringComparison.OrdinalIgnoreCase)
                || cliOptions.Force;
        }

        private async Task<bool> UpdateEditionAsync(CliEdition edition)
        {
            if (!NeedsAction(edition))
            {
                LogInfo(DisplayName(edition) + " is current (" + edition.InstalledVersion + ").");
                return false;
            }

            int archIndex = edition.Arch == "x64" ? 1 : 0;
            cliPerVersionFolders = !edition.SingleFolder;
            cliForce = cliOptions.Force;
            cliCreateShortcut = false;

            LogInfo((edition.InstalledVersion == null ? "Installing " : "Updating ") + DisplayName(edition) + " ...");

            if (edition.SingleFolder)
            {
                await NewMethod1(edition.Index, archIndex, edition.Index + 1);
            }
            else
            {
                await NewMethod(edition.Index, edition.Index, archIndex, edition.Index + 1);
            }

            edition.InstalledVersion = buildversion[edition.Index];
            LogInfo("Done: " + DisplayName(edition) + " " + edition.InstalledVersion);
            return true;
        }

        private List<CliEdition> GetInstalledEditions()
        {
            var editions = new List<CliEdition>();

            string singleLog = Path.Combine(applicationPath, "Brave", "updates", "Version.log");
            if (File.Exists(singleLog))
            {
                string[] parts = File.ReadAllText(singleLog).Split('|');
                if (parts.Length >= 3)
                {
                    int ringIndex = Array.IndexOf(ring2, parts[1].Trim());
                    int archIndex = Array.IndexOf(architektur2, parts[2].Trim());
                    if (ringIndex >= 0 && ringIndex < 4 && archIndex >= 0)
                    {
                        int index = ringIndex + (archIndex == 1 ? 4 : 0);
                        editions.Add(new CliEdition
                        {
                            Index = index,
                            Channel = ring2[index],
                            Arch = architektur2[archIndex],
                            Dir = "Brave",
                            SingleFolder = true,
                            InstalledVersion = parts[0].Trim()
                        });
                    }
                }
            }

            for (int i = 0; i < 8; i++)
            {
                string log = Path.Combine(applicationPath, instDir[i], "updates", "Version.log");
                string exe = Path.Combine(applicationPath, instDir[i], "Brave.exe");
                if (!File.Exists(log) && !File.Exists(exe))
                {
                    continue;
                }

                string installedVersion = null;
                if (File.Exists(log))
                {
                    string[] parts = File.ReadAllText(log).Split('|');
                    if (parts.Length > 0)
                    {
                        installedVersion = parts[0].Trim();
                    }
                }

                editions.Add(new CliEdition
                {
                    Index = i,
                    Channel = ring2[i],
                    Arch = architektur2[i < 4 ? 0 : 1],
                    Dir = instDir[i],
                    SingleFolder = false,
                    InstalledVersion = installedVersion
                });
            }

            return editions;
        }

        private List<CliEdition> GetTargets(List<CliEdition> installed)
        {
            if (cliOptions.Scope == UpdateScope.Existing)
            {
                return installed
                    .OrderBy(e => e.Index)
                    .ThenBy(e => e.SingleFolder ? 0 : 1)
                    .ToList();
            }

            var result = new List<CliEdition>();
            var singles = installed
                .Where(e => e.SingleFolder)
                .GroupBy(e => e.Index)
                .ToDictionary(g => g.Key, g => g.First());

            for (int i = 0; i < 8; i++)
            {
                CliEdition single;
                if (singles.TryGetValue(i, out single))
                {
                    result.Add(single);
                    continue;
                }

                CliEdition existing = installed.FirstOrDefault(e => !e.SingleFolder && e.Index == i);
                result.Add(existing ?? new CliEdition
                {
                    Index = i,
                    Channel = ring2[i],
                    Arch = architektur2[i < 4 ? 0 : 1],
                    Dir = instDir[i],
                    SingleFolder = false,
                    InstalledVersion = null
                });
            }

            return result;
        }

        private async Task<bool> EnsureBraveNotRunningAsync()
        {
            if (!IsBraveRunning())
            {
                return true;
            }

            if (cliOptions.WaitForBrave)
            {
                DateTime deadline = DateTime.UtcNow.AddSeconds(cliOptions.TimeoutSeconds);
                LogInfo("Brave is running. Waiting up to " + cliOptions.TimeoutSeconds + " seconds ...");
                while (IsBraveRunning() && DateTime.UtcNow < deadline)
                {
                    await Task.Delay(2000);
                }

                if (!IsBraveRunning())
                {
                    return true;
                }

                LogError("Brave is still running. Wait timed out.");
                return false;
            }

            LogError("Brave is running. Close it before updating.");
            return false;
        }

        private static bool IsBraveRunning()
        {
            return Process.GetProcessesByName("Brave").Length > 0;
        }

        private int LaunchBrave(List<CliEdition> installed, List<CliEdition> updated)
        {
            CliEdition edition = null;

            if (!string.IsNullOrEmpty(cliOptions.LaunchEdition))
            {
                string[] parts = cliOptions.LaunchEdition.Split(':');
                string channel = parts[0] == "dev" ? "Developer" : char.ToUpperInvariant(parts[0][0]) + parts[0].Substring(1);
                int ringIndex = Array.IndexOf(ring2, channel);
                int archIndex = parts[1] == "x64" ? 1 : 0;
                if (ringIndex >= 0 && ringIndex < 4)
                {
                    int index = ringIndex + (archIndex == 1 ? 4 : 0);
                    edition = installed
                        .Where(e => e.Index == index)
                        .OrderByDescending(e => e.SingleFolder)
                        .FirstOrDefault();
                }

                if (edition == null)
                {
                    LogError("Requested edition is not installed: " + cliOptions.LaunchEdition);
                    return 6;
                }
            }
            else if (updated.Count == 1)
            {
                edition = updated[0];
            }
            else if (installed.Count == 1)
            {
                edition = installed[0];
            }
            else if (installed.Count == 0)
            {
                LogError("No installed edition found.");
                return 6;
            }
            else
            {
                LogError("Multiple editions are installed. Use --launch-brave=channel:arch.");
                return 6;
            }

            string dir = edition.SingleFolder ? "Brave" : instDir[edition.Index];
            string launcher = Path.Combine(applicationPath, edition.SingleFolder
                ? "Brave Launcher.exe"
                : dir + " Launcher.exe");
            string braveExe = Path.Combine(applicationPath, dir, "Brave.exe");
            string profileTxt = Path.Combine(applicationPath, dir, "Profile.txt");
            string passThrough = string.Join(" ", cliOptions.BraveArguments.Select(QuoteArgument));
            if (!string.IsNullOrEmpty(passThrough))
            {
                passThrough = " " + passThrough;
            }

            try
            {
                if (File.Exists(launcher) && File.Exists(profileTxt))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = launcher,
                        Arguments = passThrough.Trim(),
                        WorkingDirectory = applicationPath,
                        UseShellExecute = true
                    });
                    LogInfo("Launched: " + Path.GetFileName(launcher));
                    return 0;
                }

                if (!File.Exists(braveExe))
                {
                    LogError("Brave is not installed: " + braveExe);
                    return 6;
                }

                string profile = Path.Combine(applicationPath, dir, "profile");
                string arguments = "--user-data-dir=\"" + profile + "\"" + passThrough;
                Process.Start(new ProcessStartInfo
                {
                    FileName = braveExe,
                    Arguments = arguments,
                    WorkingDirectory = applicationPath,
                    UseShellExecute = true
                });
                LogInfo("Launched: " + Path.GetFileName(braveExe));
                return 0;
            }
            catch (Exception ex)
            {
                LogError("Launch failed: " + ex.Message);
                return 6;
            }
        }

        private static string QuoteArgument(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            if (value.IndexOf(' ') >= 0 && !(value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)))
            {
                return "\"" + value + "\"";
            }

            return value;
        }

        private string DisplayName(CliEdition edition)
        {
            return "Brave " + edition.Channel + " " + edition.Arch
                + (edition.SingleFolder ? " (single folder)" : "");
        }

        private void LogInfo(string message)
        {
            Log("INFO", message, false);
        }

        private void LogWarn(string message)
        {
            Log("WARN", message, false);
        }

        private void LogError(string message)
        {
            Log("ERROR", message, true);
        }

        private void Log(string level, string message, bool error)
        {
            string line = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + level + ": " + message;
            if (error)
            {
                Console.Error.WriteLine(line);
            }
            else if (!cliOptions.Quiet)
            {
                Console.WriteLine(line);
            }

            if (!string.IsNullOrEmpty(cliOptions.LogPath))
            {
                try
                {
                    File.AppendAllText(Path.GetFullPath(cliOptions.LogPath), line + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] ERROR: Could not write log file: " + ex.Message);
                }
            }
        }
    }
}
