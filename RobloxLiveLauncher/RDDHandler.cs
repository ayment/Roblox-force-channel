using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

public class RDDService
{
    private readonly HttpClient _httpClient;
    private const string HostPath = "https://setup-aws.rbxcdn.com";

    private static readonly Dictionary<string, string> PlayerExtractRoots = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "RobloxApp.zip", "" },
        { "redist.zip", "" },
        { "shaders.zip", "shaders/" },
        { "ssl.zip", "ssl/" },
        { "WebView2.zip", "" },
        { "WebView2RuntimeInstaller.zip", "WebView2RuntimeInstaller/" },
        { "content-avatar.zip", "content/avatar/" },
        { "content-configs.zip", "content/configs/" },
        { "content-fonts.zip", "content/fonts/" },
        { "content-sky.zip", "content/sky/" },
        { "content-sounds.zip", "content/sounds/" },
        { "content-textures2.zip", "content/textures/" },
        { "content-models.zip", "content/models/" },
        { "content-platform-fonts.zip", "PlatformContent/pc/fonts/" },
        { "content-platform-dictionaries.zip", "PlatformContent/pc/shared_compression_dictionaries/" },
        { "content-terrain.zip", "PlatformContent/pc/terrain/" },
        { "content-textures3.zip", "PlatformContent/pc/textures/" },
        { "extracontent-luapackages.zip", "ExtraContent/LuaPackages/" },
        { "extracontent-translations.zip", "ExtraContent/translations/" },
        { "extracontent-models.zip", "ExtraContent/models/" },
        { "extracontent-textures.zip", "ExtraContent/textures/" },
        { "extracontent-places.zip", "ExtraContent/places/" }
    };

    public RDDService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(10);
    }

    public async Task<string> DownloadAndAssembleVersion(string channel, string binaryType, string version, string outputPath)
    {
        string blobDir = GetBlobDirectory(binaryType);
        string versionPath = $"{HostPath}{blobDir}{version}-";

        string manifestUrl = $"{versionPath}rbxPkgManifest.txt";
        string manifest = await _httpClient.GetStringAsync(manifestUrl);

        var packages = ParseManifest(manifest);

        Directory.CreateDirectory(outputPath);

        foreach (var package in packages)
        {
            string packageUrl = $"{versionPath}{package}";
            await DownloadAndExtractPackage(packageUrl, outputPath, binaryType, package);
        }

        CreateAppSettings(outputPath);

        CreateClientSettings(outputPath, channel);

        return Path.Combine(outputPath, "RobloxPlayerBeta.exe");
    }

    private string GetBlobDirectory(string binaryType)
    {
        return binaryType switch
        {
            "WindowsPlayer" or "WindowsStudio64" => "/",
            "MacPlayer" or "MacStudio" => "/mac/",
            _ => "/"
        };
    }

    private List<string> ParseManifest(string manifest)
    {
        var lines = manifest.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line) && line.EndsWith(".zip"))
            .ToList();

        return lines;
    }

    private async Task DownloadAndExtractPackage(string packageUrl, string outputPath, string binaryType, string packageName)
    {
        byte[] packageData = await _httpClient.GetByteArrayAsync(packageUrl);

        using (var memoryStream = new MemoryStream(packageData))
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
        {
            string extractPath = DetermineExtractPath(packageName, binaryType, outputPath);

            foreach (var entry in archive.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                string entryOutputPath = Path.Combine(extractPath, entry.FullName);
                Directory.CreateDirectory(Path.GetDirectoryName(entryOutputPath));

                using (var entryStream = entry.Open())
                using (var fileStream = File.Create(entryOutputPath))
                {
                    await entryStream.CopyToAsync(fileStream);
                }
            }
        }
    }

    private string DetermineExtractPath(string packageName, string binaryType, string basePath)
    {
        if (binaryType == "WindowsPlayer" && PlayerExtractRoots.TryGetValue(packageName, out string relativePath))
        {
            return Path.Combine(basePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        return Path.Combine(basePath, "ExtraContent", Path.GetFileNameWithoutExtension(packageName));
    }

    private void CreateAppSettings(string outputPath)
    {
        string appSettingsPath = Path.Combine(outputPath, "AppSettings.xml");
        string appSettingsContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Settings>
    <ContentFolder>content</ContentFolder>
    <BaseUrl>http://www.roblox.com</BaseUrl>
</Settings>";

        File.WriteAllText(appSettingsPath, appSettingsContent);
    }

    private void CreateClientSettings(string outputPath, string channel)
    {
        string clientSettingsDir = Path.Combine(outputPath, "ClientSettings");
        Directory.CreateDirectory(clientSettingsDir);

        string clientSettingsPath = Path.Combine(clientSettingsDir, "ClientAppSettings.json");
        string clientSettingsContent = $@"{{ ""Channel"": ""{channel}"" }}";

        File.WriteAllText(clientSettingsPath, clientSettingsContent);
    }

}