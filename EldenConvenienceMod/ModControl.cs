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

        public bool ShouldInstall => install.Checked;
        public bool ShouldUninstall => uninstall.Checked;

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

        void SetEnabled(bool enabled)
        {
            // Enabling/disabling messes with initial focus a bit
            void recurse(Control control)
            {
                control.Enabled = enabled;
                foreach (Control sub in control.Controls)
                {
                    recurse(sub);
                }
            }
            recurse(this);
        }
    }
}
