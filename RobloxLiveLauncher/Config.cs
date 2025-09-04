using System;
using System.IO;
using System.Text.Json;


namespace RobloxLiveBootstrapper
{
    public sealed class Config
    {
        public bool FirstRunDone { get; set; }
        public bool DesktopShortcut { get; set; }
        public bool StartMenuShortcut { get; set; }


    }


    public static class ConfigManager
    {
        private static readonly string Dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RobloxLiveBootstrapper");
        private static readonly string PathCfg = System.IO.Path.Combine(Dir, "settings.json");


        public static Config Load()
        {
            try
            {
                if (File.Exists(PathCfg))
                {
                    var json = File.ReadAllText(PathCfg);
                    return JsonSerializer.Deserialize<Config>(json) ?? new Config();
                }
            }
            catch { }
            return new Config();
        }


        public static void Save(Config cfg)
        {
            Directory.CreateDirectory(Dir);
            var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(PathCfg, json);
        }


        public static void Delete()
        {
            try
            {
                if (Directory.Exists(Dir)) Directory.Delete(Dir, true);
            }
            catch { }
        }
    }
}