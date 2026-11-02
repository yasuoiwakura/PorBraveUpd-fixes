using System;
using System.Drawing;
using System.Windows.Forms;

namespace Brave_Updater
{
    internal sealed class PortableAppsSettingsForm : Form
    {
        private readonly string appPath;
        private readonly CheckBox includeLauncher;
        private readonly CheckBox includeUpdater;
        private readonly Button writeButton;

        public PortableAppsSettingsForm(string appPath)
        {
            this.appPath = appPath;
            PortableAppsSettings settings = PortableAppsSettings.Load(appPath);

            Text = "PortableApps Integration";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(580, 330);

            var intro = new Label
            {
                AutoSize = false,
                Location = new Point(12, 12),
                Size = new Size(550, 45),
                Text = "Create PortableApps.com metadata for Brave Portable. Files are written to App\\AppInfo and next to the updater."
            };

            includeLauncher = new CheckBox
            {
                AutoSize = true,
                Location = new Point(12, 65),
                Text = "Brave (Launcher)",
                Checked = settings.IncludeLauncher
            };

            includeUpdater = new CheckBox
            {
                AutoSize = true,
                Location = new Point(12, 95),
                Text = "Brave (Update then Start) (experimental)",
                Checked = settings.IncludeUpdater
            };

            var edition = new Label
            {
                AutoSize = false,
                Location = new Point(12, 125),
                Size = new Size(550, 20),
                Text = "Edition: stable:x64 (Phase 1)"
            };

            var warning = new Label
            {
                AutoSize = false,
                Location = new Point(12, 155),
                Size = new Size(550, 90),
                Text = "PortableApps cannot reliably detect that Brave is still running. 'Close Apps on Platform Exit' may not close Brave. The updater also exits after launching Brave. The update-on-start option is experimental."
            };

            writeButton = new Button
            {
                Location = new Point(12, 265),
                Size = new Size(220, 30),
                Text = "Write PortableApps files"
            };

            var closeButton = new Button
            {
                Location = new Point(450, 265),
                Size = new Size(100, 30),
                Text = "Close",
                DialogResult = DialogResult.Cancel
            };

            Controls.Add(intro);
            Controls.Add(includeLauncher);
            Controls.Add(includeUpdater);
            Controls.Add(edition);
            Controls.Add(warning);
            Controls.Add(writeButton);
            Controls.Add(closeButton);

            includeLauncher.CheckedChanged += delegate { UpdateWriteButton(); };
            includeUpdater.CheckedChanged += delegate { UpdateWriteButton(); };
            writeButton.Click += WriteButton_Click;
            CancelButton = closeButton;

            UpdateWriteButton();
        }

        private void UpdateWriteButton()
        {
            writeButton.Enabled = includeLauncher.Checked || includeUpdater.Checked;
        }

        private void WriteButton_Click(object sender, EventArgs e)
        {
            PortableAppsSettings settings = PortableAppsSettings.Load(appPath);
            settings.IncludeLauncher = includeLauncher.Checked;
            settings.IncludeUpdater = includeUpdater.Checked;
            settings.Edition = "stable:x64";

            try
            {
                string report;
                PortableAppsIntegration.Write(appPath, settings, out report);
                MessageBox.Show(
                    this,
                    report + Environment.NewLine + Environment.NewLine + "Now run Apps > Refresh App Icons or restart the PortableApps Platform.",
                    "PortableApps Integration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    ex.Message,
                    "PortableApps Integration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
