using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RobloxLiveBootstrapper
{
    public static class RobloxManager
    {
        private static readonly string RobloxDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox");
        private static readonly string VersionsDir = Path.Combine(RobloxDir, "Versions");

        public static async Task<string?> GetInstalledChannelAsync()
        {
            if (!Directory.Exists(VersionsDir)) return null;

            foreach (var dir in Directory.GetDirectories(VersionsDir))
            {
                var exe = Path.Combine(dir, "RobloxPlayerBeta.exe");
                if (File.Exists(exe))
                {
                    var csPath = Path.Combine(dir, "ClientSettings", "ClientAppSettings.json");
                    if (File.Exists(csPath))
                    {
                        var text = await File.ReadAllTextAsync(csPath);
                        if (text.Contains("\"Channel\": \"LIVE\"", StringComparison.OrdinalIgnoreCase))
                            return "LIVE";
                    }
                }
            }
            return null;
        }

        private static async Task<string> GetLatestLiveClientVersionUploadAsync()
        {
            using var client = new HttpClient();
            var json = await client.GetStringAsync("https://clientsettings.roblox.com/v2/client-version/WindowsPlayer/channel/LIVE");
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("clientVersionUpload").GetString()!;
        }

        public static async Task<string> InstallLiveAsync(Action<string>? onStatus = null)
        {
            onStatus?.Invoke("Fetching LIVE version...");
            var versionUpload = await GetLatestLiveClientVersionUploadAsync();

            var outDir = Path.Combine(VersionsDir, versionUpload);
            Directory.CreateDirectory(outDir);

            var rddService = new RDDService();
            onStatus?.Invoke("Downloading and assembling LIVE package...");

            string exePath = await rddService.DownloadAndAssembleVersion(
                "LIVE",
                "WindowsPlayer",
                versionUpload,
                outDir
            );

            if (!File.Exists(exePath))
                throw new FileNotFoundException("RobloxPlayerBeta.exe not found after assembly!");

            ShortcutHelper.CreateRobloxShortcuts(exePath);
            ShortcutHelper.RegisterRobloxProtocol(outDir);

            return exePath;
        }

        public static void UninstallRoblox(Action<string>? onStatus = null)
        {
            string versionsPath = Path.Combine(RobloxDir, "Versions");

            if (Directory.Exists(versionsPath))
            {
                onStatus?.Invoke("Removing Roblox Player (keeping Studio)...");
                try
                {
                    Directory.Delete(versionsPath, true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to uninstall Roblox Player: {ex}");
                }
            }
        }


        public static async Task UninstallBootstrapperAsync(bool removeRoblox = false)
        {
            if (removeRoblox)
            {
                UninstallRoblox();
            }
            ShortcutHelper.RemoveAll();

            ConfigManager.Delete();

            await Task.CompletedTask;
        }

        public static void LaunchRoblox(string exePath, Action<string>? onStatus = null)
        {
            if (!File.Exists(exePath))
                throw new FileNotFoundException("Roblox executable not found.", exePath);

            onStatus?.Invoke("Launching Roblox...");
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true
            });
        }
    }
}