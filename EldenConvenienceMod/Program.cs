using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EldenConvenienceMod
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Bleh
            // Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#if DEBUG
            if (args.Length > 0 && args.Contains("manual"))
            {
                if (args.Contains("events"))
                {
                    Installer.FindCommandEmevds();
                    return;
                }
                Installer installer = new Installer(
                    @"C:\Users\matt\Downloads\Mods\ModEngine-2.0.0-preview3-win64\conv",
                    @"C:\Program Files (x86)\Steam\steamapps\common\ELDEN RING\Game\eldenring.exe");
                if (args.Contains("check"))
                {
                    Console.WriteLine($"Installed: [{string.Join(", ", installer.GetInstalled())}]");
                }
                if (args.Contains("install"))
                {
                    installer.Run(Mods.AllMods, Mods.NoMods, args.ToList());
                    Console.WriteLine($"Installed: [{string.Join(", ", installer.GetInstalled())}]");
                }
                if (args.Contains("uninstall"))
                {
                    installer.Run(Mods.NoMods, Mods.AllMods, args.ToList());
                    Console.WriteLine($"Installed: [{string.Join(", ", installer.GetInstalled())}]");
                }
                return;
            }
#endif
            // Application.SetHighDpiMode(HighDpiMode.SystemAware);
            AppDomain.CurrentDomain.AssemblyResolve += (sender, rargs) =>
            {
                string resourceName = "EldenConvenienceMod." + new AssemblyName(rargs.Name).Name + ".dll";
                Console.WriteLine(resourceName);
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    byte[] assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
