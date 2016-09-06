using IWshRuntimeLibrary;
using Microsoft.Win32;
using Splat;
using Squirrel;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;

namespace Seemon.Todo.Utilities
{
    class StartUpManager
    {
        public static void AddApplicationShortcutToCurrentUserStartup()
        {
            IUpdateManager updateManager = Locator.Current.GetService<IUpdateManager>();
            updateManager.CreateShortcutsForExecutable("todotxt.exe", ShortcutLocation.Startup, false, string.Empty, null);

            //var shortcutLocation = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            //var shell = new WshShell();
            //var shortcutPath = string.Format(@"{0}\{1}", shortcutLocation, "todo.txt.lnk");

            //if (System.IO.File.Exists(shortcutPath))
            //    System.IO.File.Delete(shortcutPath);

            //var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

            //shortcut.Description = AppInfo.Description;
            //shortcut.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //shortcut.TargetPath = Assembly.GetExecutingAssembly().Location;
            //shortcut.Save();
        }

        public static void RemovApplicationShortcutFromCurrentUserStartup()
        {
            IUpdateManager updateManager = Locator.Current.GetService<IUpdateManager>();
            updateManager.RemoveShortcutsForExecutable("todotxt.exe", ShortcutLocation.Startup);

            //var shortcutLocation = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            //var shortcutPath = string.Format(@"{0}\{1}", shortcutLocation, "todo.txt.lnk");

            //if (System.IO.File.Exists(shortcutPath))
            //    System.IO.File.Delete(shortcutPath);
        }

        public static void CreateCurrentUserShortcut(bool enableStartup)
        {
            if (enableStartup)
                AddApplicationShortcutToCurrentUserStartup();
            else
                RemovApplicationShortcutFromCurrentUserStartup();

           
        }


        public static void AddApplicationToCurrentUserStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("todo.txt", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            }
        }

        public static void AddApplicationToAllUserStartup()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("todo.txt", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            }
        }

        public static void RemoveApplicationFromCurrentUserStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue("todo.txt", false);
            }
        }

        public static void RemoveApplicationFromAllUserStartup()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue("todo.txt", false);
            }
        }

        public static bool IsApplicationAutoStartup
        {
            get
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false))
                {
                    if (key.GetValue("todo.txt") != null)
                        return true;
                }
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false))
                {
                    if (key.GetValue("todo.txt") != null)
                        return true;
                }
                return false;
            }
        }

        public static bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            try
            {
                //get the currently logged in user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        public static void ElevateApplication(bool enableStartup)
        {
            ProcessStartInfo proc = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location);
            proc.UseShellExecute = true;
            proc.Verb = "runas";

            if (enableStartup)
                proc.Arguments = "RegisterStartup";
            else
                proc.Arguments = "UnRegisterStartup";

            try { Process.Start(proc); }
            catch (Exception){ return; }
        }
    }
}
