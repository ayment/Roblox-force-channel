using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace RobloxLiveBootstrapper
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            if (e.Args.Any(a => string.Equals(a, "--uninstall", StringComparison.OrdinalIgnoreCase)))
            {
                _ = RobloxManager.UninstallBootstrapperAsync(removeRoblox: true);
                Shutdown();
                return;
            }


            var cfg = ConfigManager.Load();
            if (!cfg.FirstRunDone)
            {
                var first = new FirstRunWindow(cfg);
                first.Show();
            }
            else
            {
                var main = new MainWindow(cfg);
                main.Show();
            }
        }
    }
}