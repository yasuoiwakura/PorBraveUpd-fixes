using System;
using System.Drawing;
using System.Windows.Forms;

namespace Brave_Updater
{
    internal sealed class PortableAppsSettingsForm : Form
    {
        private readonly string appPath;
        private readonly CheckBox includeUpdaterApp;
        private readonly CheckBox includeLauncher;
        private readonly CheckBox includeUpdater;
        private readonly ComboBox updateIcon;
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
            ClientSize = new Size(580, 320);

            string edition = string.IsNullOrEmpty(settings.Edition) ? "stable:x64" : settings.Edition;
            string updateName = PortableAppsIntegration.FormatUpdateEntryName(settings);

            var intro = new Label
            {
                AutoSize = false,
                Location = new Point(12, 12),
                Size = new Size(550, 28),
                Text = "Write PortableApps metadata to App\\AppInfo."
            };

            includeUpdaterApp = new CheckBox
            {
                AutoSize = true,
                Location = new Point(12, 50),
                Text = "Portable Brave Updater",
                Checked = settings.IncludeUpdaterApp
            };

            var buildLabel = new Label
            {
                AutoSize = false,
                Location = new Point(12, 82),
                Size = new Size(300, 20),
                Text = "For the selected build: " + edition
            };

            includeLauncher = new CheckBox
            {
                AutoSize = true,
                Location = new Point(28, 106),
                Text = "Brave",
                Checked = settings.IncludeLauncher
            };

            includeUpdater = new CheckBox
            {
                AutoSize = true,
                Location = new Point(28, 134),
                Text = updateName,
                Checked = settings.IncludeUpdater
            };

            updateIcon = new ComboBox
            {
                Location = new Point(230, 131),
                Size = new Size(160, 24),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            updateIcon.Items.Add("Updater icon");
            updateIcon.Items.Add("Brave icon");
            updateIcon.SelectedIndex = string.Equals(settings.UpdateIcon, "brave", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

            var warning = new Label
            {
                AutoSize = false,
                Location = new Point(12, 174),
                Size = new Size(550, 55),
                Text = "PortableApps cannot reliably track Brave or close it with the Platform. '" + updateName + "' is experimental."
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
            Controls.Add(includeUpdaterApp);
            Controls.Add(buildLabel);
            Controls.Add(includeLauncher);
            Controls.Add(includeUpdater);
            Controls.Add(updateIcon);
            Controls.Add(warning);
            Controls.Add(writeButton);
            Controls.Add(closeButton);

            includeUpdaterApp.CheckedChanged += delegate { UpdateWriteButton(); };
            includeLauncher.CheckedChanged += delegate { UpdateWriteButton(); };
            includeUpdater.CheckedChanged += delegate { UpdateWriteButton(); };
            writeButton.Click += WriteButton_Click;
            CancelButton = closeButton;

            UpdateWriteButton();
        }

        private void UpdateWriteButton()
        {
            writeButton.Enabled = includeUpdaterApp.Checked || includeLauncher.Checked || includeUpdater.Checked;
            updateIcon.Enabled = includeUpdater.Checked;
        }

        private void WriteButton_Click(object sender, EventArgs e)
        {
            PortableAppsSettings settings = PortableAppsSettings.Load(appPath);
            settings.IncludeUpdaterApp = includeUpdaterApp.Checked;
            settings.IncludeLauncher = includeLauncher.Checked;
            settings.IncludeUpdater = includeUpdater.Checked;
            settings.Edition = "stable:x64";
            settings.UpdateIcon = updateIcon.SelectedIndex == 1 ? "brave" : "updater";

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
