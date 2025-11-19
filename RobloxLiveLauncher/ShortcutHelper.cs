using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;


namespace RobloxLiveBootstrapper
{
    public static class ShortcutHelper
    {
        private static string ExePath => Process.GetCurrentProcess().MainModule!.FileName!;
        private static string ExeDir => Path.GetDirectoryName(ExePath)!;


        public static void CreateDesktopShortcut()
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string lnk = Path.Combine(desktop, "Roblox LIVE.lnk");
            CreateShortcut(lnk, ExePath, ExeDir, "Roblox LIVE Bootstrapper");
        }


        public static void CreateStartMenuShortcut()
        {
            string programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            string folder = Path.Combine(programs, "Roblox LIVE");
            Directory.CreateDirectory(folder);
            string lnk = Path.Combine(folder, "Roblox LIVE.lnk");
            CreateShortcut(lnk, ExePath, ExeDir, "Roblox LIVE Bootstrapper");
        }


        public static void RemoveAll()
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                var d1 = Path.Combine(desktop, "Roblox LIVE.lnk");
                var d2 = Path.Combine(programs, "Roblox LIVE", "Roblox LIVE.lnk");
                if (File.Exists(d1)) File.Delete(d1);
                if (File.Exists(d2)) File.Delete(d2);
                var folder = Path.Combine(programs, "Roblox LIVE");
                if (Directory.Exists(folder)) Directory.Delete(folder, true);
            }
            catch { }
        }


        private static void CreateShortcut(string shortcutPath, string targetPath, string workingDir, string description)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell")!;
                dynamic shell = Activator.CreateInstance(shellType)!;
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = workingDir;
                shortcut.Description = description;
                shortcut.Save();
            }
            catch { }
        }
        public static void CreateRobloxShortcuts(string robloxExePath)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string desktopShortcut = Path.Combine(desktop, "Roblox Player.lnk");
            CreateShortcut(desktopShortcut, robloxExePath, Path.GetDirectoryName(robloxExePath)!, "Roblox Player");
            string programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            string folder = Path.Combine(programs, "Roblox");
            Directory.CreateDirectory(folder);
            string startShortcut = Path.Combine(folder, "Roblox Player.lnk");
            CreateShortcut(startShortcut, robloxExePath, Path.GetDirectoryName(robloxExePath)!, "Roblox Player");
        }
        public static void RegisterRobloxProtocol(string versionDir)
        {
            string launcher = Path.Combine(versionDir, "RobloxPlayerLauncher.exe");
            string exe = Path.Combine(versionDir, "RobloxPlayerBeta.exe");

            RegistryKey key = Registry.ClassesRoot.CreateSubKey("roblox-player");
            key.SetValue("", "URL: Roblox Player Protocol");
            key.SetValue("URL Protocol", "");

            key.CreateSubKey("DefaultIcon").SetValue("", exe);

            key.CreateSubKey(@"shell\open\command")
               .SetValue("", $"\"{launcher}\" \"%1\"");
        }

    }
}