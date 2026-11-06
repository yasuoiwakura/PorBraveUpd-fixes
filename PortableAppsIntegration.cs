using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Brave_Updater
{
    internal sealed class PortableAppsSettings
    {
        public bool IncludeUpdaterApp = false;
        public bool IncludeLauncher = true;
        public bool IncludeUpdater = false;
        public string Edition = "stable:x64";
        public string UpdateIcon = "updater";
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
                    case "includeupdaterapp":
                        settings.IncludeUpdaterApp = ParseBool(value, settings.IncludeUpdaterApp);
                        break;
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
                    case "updateicon":
                        settings.UpdateIcon = NormalizeUpdateIcon(value);
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
            builder.AppendLine("IncludeUpdaterApp=" + (IncludeUpdaterApp ? "true" : "false"));
            builder.AppendLine("IncludeLauncher=" + (IncludeLauncher ? "true" : "false"));
            builder.AppendLine("IncludeUpdater=" + (IncludeUpdater ? "true" : "false"));
            builder.AppendLine("Edition=" + Edition);
            builder.AppendLine("UpdateIcon=" + NormalizeUpdateIcon(UpdateIcon));
            builder.AppendLine("SuppressTrackingWarning=" + (SuppressTrackingWarning ? "true" : "false"));
            File.WriteAllText(file, builder.ToString(), Encoding.ASCII);
        }

        private static bool ParseBool(string value, bool fallback)
        {
            bool parsed;
            return bool.TryParse(value, out parsed) ? parsed : fallback;
        }

        private static string NormalizeUpdateIcon(string value)
        {
            return string.Equals(value, "brave", StringComparison.OrdinalIgnoreCase) ? "brave" : "updater";
        }
    }

    internal static class PortableAppsIntegration
    {
        private static readonly int[] IconSizes = new int[] { 16, 32, 75, 128, 256 };
        private static readonly int[] IcoSizes = new int[] { 16, 24, 32, 48, 64, 128, 256 };

        public static void Write(string appPath, PortableAppsSettings settings, out string report)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (!settings.IncludeUpdaterApp && !settings.IncludeLauncher && !settings.IncludeUpdater)
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
            string updateBatch = Path.Combine(appPath, "PortableAppsUpdate-stable-x64.bat");

            if (settings.IncludeUpdater)
            {
                WriteBatch(updateBatch, "\"Portable Brave Updater.exe\" --update=existing --launch-brave=stable:x64 --quit-after --log \"%~dp0PortableAppsUpdate.log\" -- %*");
                generated.Add(updateBatch);
            }

            string appInfoPath = Path.Combine(appInfoDir, "appinfo.ini");
            File.WriteAllText(appInfoPath, BuildAppInfo(appPath, settings), Encoding.ASCII);
            generated.Add(appInfoPath);

            GenerateIcons(appPath, appInfoDir, settings);
            int iconCount = GetIconCount(settings);
            generated.Add(Path.Combine(appInfoDir, "appicon.ico"));
            for (int i = 1; i <= iconCount; i++)
            {
                generated.Add(Path.Combine(appInfoDir, "appicon" + i + ".ico"));
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

        private static string BuildAppInfo(string appPath, PortableAppsSettings settings)
        {
            string launcherRelative = settings.IncludeLauncher ? ResolveLauncher(appPath) : null;
            var entries = BuildEntries(settings, launcherRelative);
            if (entries.Count == 0)
            {
                throw new InvalidOperationException("Select at least one PortableApps entry.");
            }

            string version = FileVersionInfo.GetVersionInfo(Path.Combine(appPath, "Portable Brave Updater.exe")).FileVersion;
            if (string.IsNullOrEmpty(version))
            {
                version = "1.4.10";
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
                builder.AppendLine();
            string mainStart = settings.IncludeLauncher && launcherRelative != null
                ? launcherRelative
                : (settings.IncludeUpdaterApp
                    ? "Portable Brave Updater.exe"
                    : "PortableAppsUpdate-stable-x64.bat");

            builder.AppendLine("[Control]");
            builder.AppendLine("Icons=" + entries.Count);
            builder.AppendLine("Start=" + mainStart);
            for (int i = 0; i < entries.Count; i++)
            {
                builder.AppendLine("Start" + (i + 1) + "=" + entries[i].Start);
                builder.AppendLine("Name" + (i + 1) + "=" + entries[i].Name);
                if (!string.IsNullOrEmpty(entries[i].Description))
                {
                    builder.AppendLine("Description" + (i + 1) + "=" + entries[i].Description);
                }
            }

            return builder.ToString();
        }

        private static List<Entry> BuildEntries(PortableAppsSettings settings, string launcherRelative)
        {
            var entries = new List<Entry>();
            if (settings.IncludeUpdaterApp)
            {
                entries.Add(new Entry("Portable Brave Updater.exe", "Portable Brave Updater", "updater", null));
            }
            if (settings.IncludeLauncher && launcherRelative != null)
            {
                entries.Add(new Entry(launcherRelative, "Brave", "brave", FormatEditionDescription(settings.Edition)));
            }
            if (settings.IncludeUpdater)
            {
                entries.Add(new Entry("PortableAppsUpdate-stable-x64.bat", FormatUpdateEntryName(settings), "selected", "Update Brave, then start Brave"));
            }

            return entries;
        }

        private static string FormatEditionDescription(string edition)
        {
            if (string.IsNullOrEmpty(edition))
            {
                return "stable, x64";
            }

            return edition.Replace(":", ", ");
        }

        internal static string FormatUpdateEntryName(PortableAppsSettings settings)
        {
            string edition = settings == null ? null : settings.Edition;
            string channel = string.IsNullOrEmpty(edition) ? "stable" : edition;
            int separator = channel.IndexOf(':');
            if (separator >= 0)
            {
                channel = channel.Substring(0, separator);
            }

            channel = channel.Trim().ToLowerInvariant();
            if (channel.Length == 0)
            {
                channel = "stable";
            }

            return "Brave (newest " + channel + ")";
        }

        private static int GetIconCount(PortableAppsSettings settings)
        {
            int count = 0;
            if (settings.IncludeUpdaterApp)
            {
                count++;
            }
            if (settings.IncludeLauncher)
            {
                count++;
            }
            if (settings.IncludeUpdater)
            {
                count++;
            }

            return count;
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

        private static void GenerateIcons(string appPath, string appInfoDir, PortableAppsSettings settings)
        {
            string updaterExe = Path.Combine(appPath, "Portable Brave Updater.exe");
            using (var brave = LoadBraveSource(appPath, updaterExe))
            using (var updater = LoadUpdaterSource(updaterExe))
            {
                IconSource updateEntry = string.Equals(settings.UpdateIcon, "brave", StringComparison.OrdinalIgnoreCase)
                    ? brave
                    : updater;
                IconSource mainIcon = settings.IncludeLauncher
                    ? brave
                    : (settings.IncludeUpdaterApp ? updater : updateEntry);

                WriteIconSet(appInfoDir, "appicon", mainIcon);
                List<Entry> entries = BuildEntries(settings, settings.IncludeLauncher ? ResolveLauncher(appPath) : null);
                for (int i = 0; i < entries.Count; i++)
                {
                    IconSource icon = entries[i].Icon == "updater"
                        ? updater
                        : (entries[i].Icon == "brave" ? brave : updateEntry);
                    WriteIconSet(appInfoDir, "appicon" + (i + 1), icon);
                }

                for (int i = entries.Count + 1; i <= 3; i++)
                {
                    RemoveIconSet(appInfoDir, "appicon" + i);
                }
            }
        }

        private static void RemoveIconSet(string appInfoDir, string baseName)
        {
            string icon = Path.Combine(appInfoDir, baseName + ".ico");
            if (File.Exists(icon))
            {
                File.Delete(icon);
            }

            foreach (int size in IconSizes)
            {
                string png = Path.Combine(appInfoDir, baseName + "_" + size + ".png");
                if (File.Exists(png))
                {
                    File.Delete(png);
                }
            }
        }

        private static IconSource LoadBraveSource(string appPath, string updaterExe)
        {
            string braveExe = Path.Combine(appPath, "Brave", "Brave.exe");
            if (!File.Exists(braveExe))
            {
                braveExe = Path.Combine(appPath, "App", "Brave", "Brave.exe");
            }

            if (File.Exists(braveExe))
            {
                Bitmap highResolution = ExtractIconBitmap(braveExe, 256);
                if (highResolution != null)
                {
                    return new IconSource(braveExe, highResolution);
                }
            }

            string logo = FindBraveLogo(appPath);
            if (!string.IsNullOrEmpty(logo))
            {
                return new IconSource(null, Image.FromFile(logo));
            }

            Bitmap fallback = ExtractIconBitmap(updaterExe, 256);
            if (fallback != null)
            {
                return new IconSource(null, fallback);
            }

            throw new InvalidOperationException("Could not load a Brave icon.");
        }

        private static string FindBraveLogo(string appPath)
        {
            string[] directCandidates = new string[]
            {
                Path.Combine(appPath, "SmallLogo.png"),
                Path.Combine(appPath, "Brave", "VisualElements", "Logo.png"),
                Path.Combine(appPath, "Brave", "VisualElements", "SmallLogo.png")
            };
            foreach (string candidate in directCandidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            string braveRoot = Path.Combine(appPath, "Brave");
            if (Directory.Exists(braveRoot))
            {
                foreach (string directory in Directory.GetDirectories(braveRoot))
                {
                    string large = Path.Combine(directory, "VisualElements", "Logo.png");
                    if (File.Exists(large))
                    {
                        return large;
                    }

                    string small = Path.Combine(directory, "VisualElements", "SmallLogo.png");
                    if (File.Exists(small))
                    {
                        return small;
                    }
                }
            }

            return null;
        }

        private static IconSource LoadUpdaterSource(string updaterExe)
        {
            Bitmap highResolution = ExtractIconBitmap(updaterExe, 256);
            if (highResolution != null)
            {
                return new IconSource(updaterExe, highResolution);
            }

            Icon icon = Icon.ExtractAssociatedIcon(updaterExe);
            if (icon == null)
            {
                throw new InvalidOperationException("Could not load the updater icon.");
            }

            using (icon)
            {
                return new IconSource(null, icon.ToBitmap());
            }
        }

        private static void WriteIconSet(string appInfoDir, string baseName, IconSource source)
        {
            WriteIcon(Path.Combine(appInfoDir, baseName + ".ico"), source);

            foreach (int size in IconSizes)
            {
                using (var bitmap = source.GetFrame(size))
                {
                    bitmap.Save(Path.Combine(appInfoDir, baseName + "_" + size + ".png"), ImageFormat.Png);
                }
            }
        }

        private static void WriteIcon(string path, IconSource source)
        {
            var frames = new List<IcoFrame>();
            foreach (int size in IcoSizes)
            {
                using (var bitmap = source.GetFrame(size))
                {
                    frames.Add(new IcoFrame(size, size == 256 ? EncodePng(bitmap) : EncodeDib(bitmap)));
                }
            }

            using (var stream = File.Create(path))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((ushort)0);
                writer.Write((ushort)1);
                writer.Write((ushort)frames.Count);

                int offset = 6 + (16 * frames.Count);
                foreach (IcoFrame frame in frames)
                {
                    writer.Write((byte)(frame.Size >= 256 ? 0 : frame.Size));
                    writer.Write((byte)(frame.Size >= 256 ? 0 : frame.Size));
                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    writer.Write((ushort)1);
                    writer.Write((ushort)32);
                    writer.Write((uint)frame.Data.Length);
                    writer.Write((uint)offset);
                    offset += frame.Data.Length;
                }

                foreach (IcoFrame frame in frames)
                {
                    writer.Write(frame.Data);
                }
            }
        }

        private static byte[] EncodePng(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private static byte[] EncodeDib(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int xorSize = width * height * 4;
            int maskStride = ((width + 31) / 32) * 4;

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((uint)40);
                writer.Write(width);
                writer.Write(height * 2);
                writer.Write((ushort)1);
                writer.Write((ushort)32);
                writer.Write((uint)0);
                writer.Write((uint)xorSize);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);

                BitmapData data = bitmap.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);
                try
                {
                    byte[] row = new byte[width * 4];
                    for (int y = height - 1; y >= 0; y--)
                    {
                        Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), row, 0, row.Length);
                        writer.Write(row);
                    }
                }
                finally
                {
                    bitmap.UnlockBits(data);
                }

                byte[] mask = new byte[maskStride];
                for (int y = 0; y < height; y++)
                {
                    writer.Write(mask);
                }

                return stream.ToArray();
            }
        }

        private static Bitmap ResizeImage(Image source, int size)
        {
            var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            bitmap.SetResolution(96, 96);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var attributes = new ImageAttributes())
                {
                    attributes.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(
                        source,
                        new Rectangle(0, 0, size, size),
                        0,
                        0,
                        source.Width,
                        source.Height,
                        GraphicsUnit.Pixel,
                        attributes);
                }
            }

            return bitmap;
        }

        private static Bitmap ExtractIconBitmap(string executable, int size)
        {
            var handles = new IntPtr[1];
            var ids = new uint[1];
            uint count = PrivateExtractIcons(executable, 0, size, size, handles, ids, 1, 0);
            if (count == 0 || handles[0] == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                using (Icon icon = Icon.FromHandle(handles[0]))
                {
                    return icon.ToBitmap();
                }
            }
            finally
            {
                DestroyIcon(handles[0]);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint PrivateExtractIcons(
            string file,
            int iconIndex,
            int cx,
            int cy,
            IntPtr[] handles,
            uint[] ids,
            uint count,
            uint flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr handle);

        private static string BackupExisting(string appPath)
        {
            var items = new List<string>();
            string appInfoDir = Path.Combine(appPath, "App", "AppInfo");
            if (Directory.Exists(appInfoDir))
            {
                items.Add(appInfoDir);
            }

            foreach (string name in new string[] { "PortableAppsUpdater-stable-x64.bat", "PortableAppsStart-stable-x64.bat", "PortableAppsUpdate-stable-x64.bat", "PortableBraveUpdater.ini" })
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
            public readonly string Icon;
            public readonly string Description;

            public Entry(string start, string name, string icon, string description)
            {
                Start = start;
                Name = name;
                Icon = icon;
                Description = description;
            }
        }

        private sealed class IconSource : IDisposable
        {
            public readonly string Executable;
            public readonly Image Image;

            public IconSource(string executable, Image image)
            {
                Executable = executable;
                Image = image;
            }

            public Bitmap GetFrame(int size)
            {
                if (!string.IsNullOrEmpty(Executable))
                {
                    Bitmap extracted = ExtractIconBitmap(Executable, size);
                    if (extracted != null)
                    {
                        if (extracted.Width == size && extracted.Height == size)
                        {
                            return extracted;
                        }

                        using (extracted)
                        {
                            return ResizeImage(extracted, size);
                        }
                    }
                }

                return ResizeImage(Image, size);
            }

            public void Dispose()
            {
                if (Image != null)
                {
                    Image.Dispose();
                }
            }
        }

        private sealed class IcoFrame
        {
            public readonly int Size;
            public readonly byte[] Data;

            public IcoFrame(int size, byte[] data)
            {
                Size = size;
                Data = data;
            }
        }
    }
}
