using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Brave_Updater
{
    internal sealed class PortableAppsSettings
    {
        public bool IncludeLauncher = true;
        public bool IncludeUpdater = false;
        public string Edition = "stable:x64";
        public bool SuppressTrackingWarning = false;

        public static PortableAppsSettings Load(string appPath)
        {
            var settings = new PortableAppsSettings();
            string file = Path.Combine(appPath, "PortableBraveUpdater.ini");
            if (!File.Exists(file))
            {
                return settings;
            }

            bool inSection = false;
            foreach (string raw in File.ReadAllLines(file))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith(";", StringComparison.Ordinal))
                {
                    continue;
                }

                if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal))
                {
                    inSection = line.Equals("[PortableApps]", StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                if (!inSection)
                {
                    continue;
                }

                int equals = line.IndexOf('=');
                if (equals < 0)
                {
                    continue;
                }

                string key = line.Substring(0, equals).Trim();
                string value = line.Substring(equals + 1).Trim();
                switch (key.ToLowerInvariant())
                {
                    case "includelauncher":
                        settings.IncludeLauncher = ParseBool(value, settings.IncludeLauncher);
                        break;
                    case "includeupdater":
                        settings.IncludeUpdater = ParseBool(value, settings.IncludeUpdater);
                        break;
                    case "edition":
                        if (value.Length > 0)
                        {
                            settings.Edition = value;
                        }
                        break;
                    case "suppresstrackingwarning":
                        settings.SuppressTrackingWarning = ParseBool(value, settings.SuppressTrackingWarning);
                        break;
                }
            }

            return settings;
        }

        public void Save(string appPath)
        {
            string file = Path.Combine(appPath, "PortableBraveUpdater.ini");
            var builder = new StringBuilder();
            builder.AppendLine("[PortableApps]");
            builder.AppendLine("IncludeLauncher=" + (IncludeLauncher ? "true" : "false"));
            builder.AppendLine("IncludeUpdater=" + (IncludeUpdater ? "true" : "false"));
            builder.AppendLine("Edition=" + Edition);
            builder.AppendLine("SuppressTrackingWarning=" + (SuppressTrackingWarning ? "true" : "false"));
            File.WriteAllText(file, builder.ToString(), Encoding.ASCII);
        }

        private static bool ParseBool(string value, bool fallback)
        {
            bool parsed;
            return bool.TryParse(value, out parsed) ? parsed : fallback;
        }
    }

    internal static class PortableAppsIntegration
    {
        private static readonly int[] IconSizes = new int[] { 16, 32, 75, 128, 256 };

        public static void Write(string appPath, PortableAppsSettings settings, out string report)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (!settings.IncludeLauncher && !settings.IncludeUpdater)
            {
                throw new InvalidOperationException("Select at least one PortableApps entry.");
            }

            string updaterExe = Path.Combine(appPath, "Portable Brave Updater.exe");
            if (!File.Exists(updaterExe))
            {
                throw new FileNotFoundException("Portable Brave Updater.exe not found.", updaterExe);
            }

            string launcherRelative = null;
            if (settings.IncludeLauncher)
            {
                launcherRelative = ResolveLauncher(appPath);
                if (launcherRelative == null)
                {
                    throw new FileNotFoundException("Brave Stable x64 launcher not found.");
                }
            }

            string backupDir = BackupExisting(appPath);
            string appInfoDir = Path.Combine(appPath, "App", "AppInfo");
            Directory.CreateDirectory(appInfoDir);

            var generated = new List<string>();
            string startBatch = Path.Combine(appPath, "PortableAppsStart-stable-x64.bat");
            string updateBatch = Path.Combine(appPath, "PortableAppsUpdate-stable-x64.bat");

            if (settings.IncludeLauncher)
            {
                WriteBatch(startBatch, "\"" + launcherRelative + "\" %*");
                generated.Add(startBatch);
            }

            if (settings.IncludeUpdater)
            {
                WriteBatch(updateBatch, "\"Portable Brave Updater.exe\" --update=existing --launch-brave=stable:x64 --quit-after --log \"%~dp0PortableAppsUpdate.log\" -- %*");
                generated.Add(updateBatch);
            }

            string appInfoPath = Path.Combine(appInfoDir, "appinfo.ini");
            File.WriteAllText(appInfoPath, BuildAppInfo(appPath, settings, settings.IncludeLauncher, settings.IncludeUpdater), Encoding.ASCII);
            generated.Add(appInfoPath);

            GenerateIcons(appPath, appInfoDir, settings.IncludeLauncher, settings.IncludeUpdater);
            generated.Add(Path.Combine(appInfoDir, "appicon.ico"));
            generated.Add(Path.Combine(appInfoDir, "appicon1.ico"));
            if (settings.IncludeLauncher && settings.IncludeUpdater)
            {
                generated.Add(Path.Combine(appInfoDir, "appicon2.ico"));
            }

            settings.Save(appPath);
            generated.Add(Path.Combine(appPath, "PortableBraveUpdater.ini"));

            var lines = new List<string>();
            lines.Add("PortableApps files written:");
            foreach (string file in generated)
            {
                lines.Add(file);
            }
            if (backupDir != null)
            {
                lines.Add("Backup: " + backupDir);
            }
            report = string.Join(Environment.NewLine, lines);
        }

        private static string ResolveLauncher(string appPath)
        {
            if (File.Exists(Path.Combine(appPath, "Brave Launcher.exe")))
            {
                return "Brave Launcher.exe";
            }

            if (File.Exists(Path.Combine(appPath, "Brave Stable x64 Launcher.exe")))
            {
                return "Brave Stable x64 Launcher.exe";
            }

            if (File.Exists(Path.Combine(appPath, "Bin", "Launcher", "Brave Stable x64 Launcher.exe")))
            {
                return @"Bin\Launcher\Brave Stable x64 Launcher.exe";
            }

            return null;
        }

        private static string BuildAppInfo(string appPath, PortableAppsSettings settings, bool includeLauncher, bool includeUpdater)
        {
            var entries = new List<Entry>();
            if (includeLauncher)
            {
                entries.Add(new Entry("PortableAppsStart-stable-x64.bat", "Brave (Launcher)"));
            }
            if (includeUpdater)
            {
                entries.Add(new Entry("PortableAppsUpdate-stable-x64.bat", "Brave (Update then Start)"));
            }

            string version = FileVersionInfo.GetVersionInfo(Path.Combine(appPath, "Portable Brave Updater.exe")).FileVersion;
            if (string.IsNullOrEmpty(version))
            {
                version = "1.3.3.37";
            }
            version = NormalizeVersion(version);

            var builder = new StringBuilder();
            builder.AppendLine("[Format]");
            builder.AppendLine("Type=PortableApps.comFormat");
            builder.AppendLine("Version=3.9");
            builder.AppendLine();
            builder.AppendLine("[Details]");
            builder.AppendLine("Name=Brave Portable");
            builder.AppendLine("AppID=BravePortable-yasuoiwakura");
            builder.AppendLine("Publisher=Brave Software, Inc. and yasuoiwakura");
            builder.AppendLine("Homepage=https://github.com/yasuoiwakura/PorBraveUpd-fixes");
            builder.AppendLine("Category=Internet");
            builder.AppendLine("Description=Brave browser with portable updater");
            builder.AppendLine("Language=Multilingual");
            builder.AppendLine("Trademarks=Brave is a trademark of Brave Software, Inc.");
            builder.AppendLine("InstallType=");
            builder.AppendLine();
            builder.AppendLine("[License]");
            builder.AppendLine("Shareable=true");
            builder.AppendLine("OpenSource=false");
            builder.AppendLine("Freeware=true");
            builder.AppendLine("CommercialUse=true");
            builder.AppendLine("EULAVersion=1");
            builder.AppendLine();
            builder.AppendLine("[Version]");
            builder.AppendLine("PackageVersion=" + version);
            builder.AppendLine("DisplayVersion=" + version);
            builder.AppendLine();
            builder.AppendLine("[Control]");
            builder.AppendLine("Icons=" + entries.Count);
            builder.AppendLine("Start=" + entries[0].Start);
            for (int i = 0; i < entries.Count; i++)
            {
                builder.AppendLine("Start" + (i + 1) + "=" + entries[i].Start);
                builder.AppendLine("Name" + (i + 1) + "=" + entries[i].Name);
            }

            return builder.ToString();
        }

        private static string NormalizeVersion(string version)
        {
            string[] parts = version.Split('.');
            var normalized = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                normalized.Add(i < parts.Length && parts[i].Length > 0 ? parts[i] : "0");
            }
            return string.Join(".", normalized.ToArray());
        }

        private static void WriteBatch(string path, string command)
        {
            var builder = new StringBuilder();
            builder.AppendLine("@echo off");
            builder.AppendLine("setlocal");
            builder.AppendLine("cd /d \"%~dp0\"");
            builder.AppendLine();
            builder.AppendLine(command);
            builder.AppendLine("exit /b %ERRORLEVEL%");
            File.WriteAllText(path, builder.ToString(), Encoding.ASCII);
        }

        private static void GenerateIcons(string appPath, string appInfoDir, bool includeLauncher, bool includeUpdater)
        {
            string updaterExe = Path.Combine(appPath, "Portable Brave Updater.exe");
            using (var brave = LoadBraveSource(appPath, updaterExe))
            using (var updater = LoadUpdaterSource(updaterExe))
            {
                IconSource first = includeLauncher ? brave : updater;
                IconSource second = includeLauncher && includeUpdater ? updater : null;

                WriteIconSet(appInfoDir, "appicon", first);
                WriteIconSet(appInfoDir, "appicon1", first);
                if (second != null)
                {
                    WriteIconSet(appInfoDir, "appicon2", second);
                }
            }
        }

        private static IconSource LoadBraveSource(string appPath, string updaterExe)
        {
            Icon icon = null;
            Image image = null;

            string braveExe = Path.Combine(appPath, "Brave", "Brave.exe");
            if (File.Exists(braveExe))
            {
                icon = Icon.ExtractAssociatedIcon(braveExe);
            }

            string logo = Path.Combine(appPath, "SmallLogo.png");
            if (File.Exists(logo))
            {
                image = Image.FromFile(logo);
            }

            if (image == null && icon != null)
            {
                image = icon.ToBitmap();
            }

            if (icon == null)
            {
                icon = Icon.ExtractAssociatedIcon(updaterExe);
            }

            if (icon == null || image == null)
            {
                if (icon != null)
                {
                    icon.Dispose();
                }
                if (image != null)
                {
                    image.Dispose();
                }
                throw new InvalidOperationException("Could not load a Brave icon.");
            }

            return new IconSource(icon, image);
        }

        private static IconSource LoadUpdaterSource(string updaterExe)
        {
            Icon icon = Icon.ExtractAssociatedIcon(updaterExe);
            if (icon == null)
            {
                throw new InvalidOperationException("Could not load the updater icon.");
            }

            return new IconSource(icon, icon.ToBitmap());
        }

        private static void WriteIconSet(string appInfoDir, string baseName, IconSource source)
        {
            using (var stream = File.Create(Path.Combine(appInfoDir, baseName + ".ico")))
            {
                source.Icon.Save(stream);
            }

            foreach (int size in IconSizes)
            {
                using (var bitmap = new Bitmap(size, size))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(source.Image, 0, 0, size, size);
                    bitmap.Save(Path.Combine(appInfoDir, baseName + "_" + size + ".png"), ImageFormat.Png);
                }
            }
        }

        private static string BackupExisting(string appPath)
        {
            var items = new List<string>();
            string appInfoDir = Path.Combine(appPath, "App", "AppInfo");
            if (Directory.Exists(appInfoDir))
            {
                items.Add(appInfoDir);
            }

            foreach (string name in new string[] { "PortableAppsStart-stable-x64.bat", "PortableAppsUpdate-stable-x64.bat", "PortableBraveUpdater.ini" })
            {
                string path = Path.Combine(appPath, name);
                if (File.Exists(path))
                {
                    items.Add(path);
                }
            }

            if (items.Count == 0)
            {
                return null;
            }

            string backupDir = Path.Combine(appPath, "PortableApps-backup-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backupDir);
            foreach (string item in items)
            {
                string target = Path.Combine(backupDir, Path.GetFileName(item));
                if (Directory.Exists(item))
                {
                    CopyDirectory(item, target);
                }
                else
                {
                    File.Copy(item, target, true);
                }
            }

            return backupDir;
        }

        private static void CopyDirectory(string source, string target)
        {
            Directory.CreateDirectory(target);
            foreach (string file in Directory.GetFiles(source))
            {
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
            }
            foreach (string directory in Directory.GetDirectories(source))
            {
                CopyDirectory(directory, Path.Combine(target, Path.GetFileName(directory)));
            }
        }

        private sealed class Entry
        {
            public readonly string Start;
            public readonly string Name;

            public Entry(string start, string name)
            {
                Start = start;
                Name = name;
            }
        }

        private sealed class IconSource : IDisposable
        {
            public readonly Icon Icon;
            public readonly Image Image;

            public IconSource(Icon icon, Image image)
            {
                Icon = icon;
                Image = image;
            }

            public void Dispose()
            {
                if (Image != null)
                {
                    Image.Dispose();
                }
                if (Icon != null)
                {
                    Icon.Dispose();
                }
            }
        }
    }
}
