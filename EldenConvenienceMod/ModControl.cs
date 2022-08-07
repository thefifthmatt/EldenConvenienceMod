using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static EldenConvenienceMod.Mods;

namespace EldenConvenienceMod
{
    public partial class ModControl : UserControl
    {
        internal ModInfo Mod { get; set; }
        private Color installColor;

        internal ModControl(ModInfo Mod)
        {
            InitializeComponent();
            this.Mod = Mod;
            name.Text = Mod.DisplayName;
            desc.Text = Mod.Desc;
            int extraLines = desc.Text.Count(c => c == '\n');
            Height += (int)(desc.Font.GetHeight() * extraLines);
            installedL.Visible = false;
            installColor = installedL.ForeColor;
        }

        public bool ShouldIgnore => ignore.Checked;
        public bool ShouldInstall => install.Checked;
        public bool ShouldUninstall => uninstall.Checked;

        public enum InstallState
        {
            None, Ignore, Install, Uninstall
        }

        public InstallState State
        {
            get
            {
                if (ignore.Checked)
                {
                    return InstallState.Ignore;
                }
                if (install.Checked)
                {
                    return InstallState.Install;
                }
                if (uninstall.Checked)
                {
                    return InstallState.Uninstall;
                }
                return InstallState.None;
            }
            set
            {
                manualChange = true;
                switch (value)
                {
                    case InstallState.None:
                        ignore.Checked = install.Checked = uninstall.Checked = false;
                        break;
                    case InstallState.Ignore:
                        ignore.Checked = true;
                        break;
                    case InstallState.Install:
                        install.Checked = true;
                        break;
                    case InstallState.Uninstall:
                        uninstall.Checked = true;
                        break;
                }
                manualChange = false;
            }
        }

        public event EventHandler CheckChanged;

        private bool manualChange = false;
        private void opt_CheckedChanged(object sender, EventArgs e)
        {
            if (manualChange) return;
            CheckChanged?.Invoke(this, e);
        }

        public void SetInstalled(bool initialized, bool installed)
        {
            if (!initialized)
            {
                installedL.Visible = true;
                installedL.ForeColor = SystemColors.ControlText;
                installedL.Text = "...";
            }
            else if (installed)
            {
                installedL.Visible = true;
                installedL.ForeColor = installColor;
                installedL.Text = "Installed";
            }
            else
            {
                installedL.Visible = false;
                // installedL.ForeColor = SystemColors.ControlText;
                // installedL.Text = "Not detected";
            }
        }
    }
}
