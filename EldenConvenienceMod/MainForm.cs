using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using EldenConvenienceMod.Properties;
using static EldenConvenienceMod.Mods;

namespace EldenConvenienceMod
{
    public partial class MainForm : Form
    {
        private static string defaultPath = @"C:\Program Files (x86)\Steam\steamapps\common\ELDEN RING\Game\eldenring.exe";
        private static readonly SemaphoreSlim execBlock = new SemaphoreSlim(2);
        private Action delayCheckInstalled;
        private string selfDir;

        public MainForm()
        {
            delayCheckInstalled = Debounce(CheckInstalled, 300);

            InitializeComponent();
            int height = 0;
            foreach (ModInfo info in AllInfos)
            {
                ModControl c = new ModControl(info);
                modpanel.Controls.Add(c);
                c.Location = new Point(0, height);
                height += c.Height;
            }
            Height += height;
            MinimumSize = Size;

            bool fresh = false;
#if DEBUG
            // fresh = true;
#endif
            if (!fresh && !string.IsNullOrWhiteSpace(Settings.Default.gameexe)
                && TryGetDirectory(Settings.Default.gameexe, out string exe))
            {
                gameexeL.Text = Path.Combine(exe, "eldenring.exe");
            }
            if (!fresh && !string.IsNullOrWhiteSpace(Settings.Default.moddir)
                && TryGetDirectory(Settings.Default.moddir, out string dir))
            {
                moddirL.Text = dir;
            }

            if (string.IsNullOrWhiteSpace(gameexeL.Text))
            {
                if (File.Exists(defaultPath))
                {
                    gameexeL.Text = defaultPath;
                }
            }
            selfDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (string.IsNullOrWhiteSpace(moddirL.Text))
            {
                // selfDir is probably fine to save, as it should change when the install loc changes
                moddirL.Text = selfDir;
            }

            CheckInstalled();
        }

        private async void CheckInstalled()
        {
            foreach (Control sub in modpanel.Controls)
            {
                ModControl c = (ModControl)sub;
                c.SetInstalled(false, false);
            }
            SortedSet<Mod> installed = new SortedSet<Mod>();
            try
            {
                if (Directory.Exists(moddirL.Text) && IsOodleInstalled())
                {
                    await Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            execBlock.Wait();
                            installed = new Installer(moddirL.Text).GetInstalled();
                        }
                        finally
                        {
                            execBlock.Release();
                        }
                    });
                }
            }
            catch (Exception) { }
#if DEBUG
            Console.WriteLine($"Installed: [{string.Join(", ", installed)}]");
#endif
            foreach (Control sub in modpanel.Controls)
            {
                ModControl c = (ModControl)sub;
                c.SetInstalled(true, installed.Contains(c.Mod.Type));
            }
        }

        private void moddirL_TextChanged(object sender, EventArgs e)
        {
            if (TryGetDirectory(moddirL.Text, out string dir))
            {
                Settings.Default.moddir = dir;
                Settings.Default.Save();
            }
            delayCheckInstalled();
        }

        private void gameexeL_TextChanged(object sender, EventArgs e)
        {
            if (TryGetDirectory(gameexeL.Text, out _) && gameexeL.Text.ToLowerInvariant().EndsWith("eldenring.exe"))
            {
                Settings.Default.gameexe = gameexeL.Text;
                Settings.Default.Save();
            }
        }

        private void moddirB_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = "Select directory to install mod";
            dialog.IsFolderPicker = true;
            dialog.RestoreDirectory = true;
            if (TryGetDirectory(moddirL.Text, out string dir))
            {
                dialog.InitialDirectory = dir;
            }
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                moddirL.Text = dialog.FileName;
            }
        }

        private void gameexeB_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select Elden Ring install location";
            dialog.Filter = "Elden Ring exe|eldenring.exe";
            dialog.RestoreDirectory = true;
            if (TryGetDirectory(gameexeL.Text, out string dir))
            {
                dialog.InitialDirectory = dir;
            }
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                gameexeL.Text = dialog.FileName;
            }
        }

        private bool working = false;
        private async void runB_Click(object sender, EventArgs e)
        {
            if (working) return;
            string moddir;
            string gameexe;
            if (TryGetDirectory(moddirL.Text, out string dir))
            {
                moddir = dir;
            }
            else
            {
                MessageBox.Show($"Mod directory not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (TryGetDirectory(gameexeL.Text, out string gamedir)
                && gameexeL.Text.ToLowerInvariant().EndsWith("eldenring.exe"))
            {
                gameexe = gameexeL.Text;
                if (!IsOodleInstalled())
                {
                    MessageBox.Show($"{oodle} not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show($"Game exe not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SortedSet<Mod> install = new SortedSet<Mod>();
            SortedSet<Mod> uninstall = new SortedSet<Mod>();
            foreach (Control sub in modpanel.Controls)
            {
                ModControl c = (ModControl)sub;
                if (c.ShouldInstall) install.Add(c.Mod.Type);
                if (c.ShouldUninstall) uninstall.Add(c.Mod.Type);
            }
            Exception runEx = null;
            working = true;
            string buttonText = runB.Text;
            runB.Text = "Working...";
            runB.BackColor = Color.LightYellow;
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    new Installer(moddir, gameexe).Run(install, uninstall);
                }
                catch (Exception ex)
                {
                    runEx = ex;
                }
            });
            runB.Text = buttonText;
            runB.BackColor = SystemColors.Control;
            working = false;

            if (runEx != null)
            {
                MessageBox.Show(runEx.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            CheckInstalled();
        }

        private static readonly string oodle = "oo2core_6_win64.dll";
        private bool IsOodleInstalled()
        {
            if (File.Exists(oodle))
            {
                return true;
            }
            if (TryGetDirectory(gameexeL.Text, out string gamedir))
            {
                string gameOodle = Path.Combine(gamedir, oodle);
                if (File.Exists(gameOodle))
                {
                    File.Copy(gameOodle, oodle);
                    return true;
                }
            }
            return false;
        }

        private bool TryGetDirectory(string path, out string dir)
        {
            dir = null;
            try
            {
                if (Directory.Exists(path))
                {
                    dir = path;
                    return true;
                }
                else
                {
                    string fileDir = Path.GetDirectoryName(path);
                    if (Directory.Exists(fileDir))
                    {
                        dir = fileDir;
                        return true;
                    }
                }
            }
            catch (ArgumentException) { }
            return false;
        }

        public static Action Debounce(Action func, int ms)
        {
            CancellationTokenSource cancelTokenSource = null;

            return () =>
            {
                cancelTokenSource?.Cancel();
                cancelTokenSource = new CancellationTokenSource();
                Task.Delay(ms, cancelTokenSource.Token)
                    .ContinueWith(t =>
                    {
                        if (!t.IsCanceled)
                        {
                            func();
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            };
        }
    }
}
