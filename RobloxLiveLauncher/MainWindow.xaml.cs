using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace RobloxLiveBootstrapper
{
    public partial class MainWindow : Window
    {
        private readonly Config cfg;
        private readonly string versionsPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "Versions");

        public MainWindow(Config config)
        {
            InitializeComponent();
            cfg = config;
            Loaded += async (_, __) => await StartAsync();
        }

        private async Task StartAsync()
        {
            try
            {
                string latestVersion = await GetLatestVersionFromWebsite();

                string installedVersionPath = FindInstalledVersionPath(latestVersion);
                if (IsRobloxRunning())
                {
                    if (installedVersionPath != null)
                    {
                        LblStatus.Text = "The Channel is already set to 'LIVE'";
                        await Task.Delay(2000);
                        Application.Current.Shutdown();
                        return;
                    }
                     LblStatus.Text = "Roblox is running. Closing it...";
                    CloseRoblox();
                    await Task.Delay(2000);
                }

                LblStatus.Text = "Checking Roblox version...";


                if (installedVersionPath != null)
                {
                    LblStatus.Text = "Launching Roblox...";
                    LaunchRoblox(installedVersionPath);
                    await Task.Delay(2000);
                    Application.Current.Shutdown();
                    return;
                }

                LblStatus.Text = "Version mismatch or not installed. Updating...";
                UninstallRoblox();
                string installedExe = await InstallLatestVersion(latestVersion);

                if (File.Exists(installedExe))
                {
                    LblStatus.Text = "Launching Roblox...";
                    LaunchRoblox(installedExe);
                    await Task.Delay(2000);
                    Application.Current.Shutdown();
                }
                else
                {
                    LblStatus.Text = "Installation failed. Executable not found.";
                }
            }
            catch (Exception ex)
            {
                LblStatus.Text = $"Error: {ex.Message}";
            }
        }

        private bool IsRobloxRunning()
        {
            Process[] processes = Process.GetProcessesByName("RobloxPlayerBeta");
            return processes.Length > 0;
        }

        private void CloseRoblox()
        {
            Process[] processes = Process.GetProcessesByName("RobloxPlayerBeta");
            foreach (Process process in processes)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(5000);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error closing Roblox: {ex.Message}");
                }
            }
        }

        private async Task<string> GetLatestVersionFromWebsite()
        {
            using var client = new HttpClient();
            var json = await client.GetStringAsync("https://clientsettings.roblox.com/v2/client-version/WindowsPlayer/channel/LIVE");
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("clientVersionUpload").GetString();
        }

        private string FindInstalledVersionPath(string version)
        {
            if (!Directory.Exists(versionsPath)) return null;

            string versionDir = Path.Combine(versionsPath, version);
            if (!Directory.Exists(versionDir)) return null;

            string exePath = Path.Combine(versionDir, "RobloxPlayerBeta.exe");
            return File.Exists(exePath) ? exePath : null;
        }

        private void UninstallRoblox()
        {
            if (Directory.Exists(versionsPath))
            {
                try
                {
                    Directory.Delete(versionsPath, true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to uninstall Roblox: {ex}");
                }
            }
        }

        private async Task<string> InstallLatestVersion(string version)
        {
            LblStatus.Text = "Downloading and installing latest version...";

            var rddService = new RDDService();
            string versionDir = Path.Combine(versionsPath, version);
            Directory.CreateDirectory(versionDir);

            string exePath = await rddService.DownloadAndAssembleVersion(
                "LIVE",
                "WindowsPlayer",
                version,
                versionDir
            );

            return exePath;
        }

        private void LaunchRoblox(string exePath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true
            });
        }
    }
}