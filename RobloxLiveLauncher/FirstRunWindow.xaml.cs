using System;
using System.Windows;

namespace RobloxLiveBootstrapper
{
    public partial class FirstRunWindow : Window
    {
        private Config cfg;

        public FirstRunWindow(Config config)
        {
            InitializeComponent();
            cfg = config;
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            cfg.FirstRunDone = true;
            cfg.DesktopShortcut = chkDesktop.IsChecked == true;
            cfg.StartMenuShortcut = chkStartMenu.IsChecked == true;
            ConfigManager.Save(cfg);

            if (cfg.DesktopShortcut) ShortcutHelper.CreateDesktopShortcut();
            if (cfg.StartMenuShortcut) ShortcutHelper.CreateStartMenuShortcut();

            var mainWindow = new MainWindow(cfg);
            mainWindow.Show();
            this.Close();
        }
    }
}