using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace mRegistryHelper.Function
{
    [ObsoleteAttribute("아직 구현되지 않았습니다.", true)]
    internal class MagnetCopy
    {
        static readonly string addr = "Magnet";

        public static bool GetMagnet()
        {
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(addr);
            return key != null;
        }

        public static string SetMagnet(string path)
        {
            if (!Base.Inspect.IsAdministrator())
            {
                Console.WriteLine(Base.Inspect.RunAdminMsg);
                string result = Console.ReadLine();
                if (result.ToLower() == "y")
                {
                    Base.Inspect.RunRunas(string.Format("cmd=magnet_install path=\"{0}\"", path));
                }
                else
                {
                    return "false";
                }
            }

            //bool exists = File.Exists(Path.Combine(Dir, consoleExeFile));
            bool exists = File.Exists(path);

            if (!exists)
                throw new Exception("MagnetCopy.exe 파일을 찾을 수 없습니다.");

            string pathParam = string.Format("\"{0}\" \"%L\"", path);

            RegistryKey key = Registry.ClassesRoot.CreateSubKey(addr, RegistryKeyPermissionCheck.ReadWriteSubTree);
            key.SetValue("", "");
            key.SetValue("URL Protocol", "", RegistryValueKind.String);
            key.SetValue("Content Type", "", RegistryValueKind.String);

            RegistryKey defaultIcon = key.CreateSubKey("DefaultIcon", RegistryKeyPermissionCheck.ReadWriteSubTree);
            defaultIcon.SetValue("", pathParam);

            RegistryKey shell = key.CreateSubKey("shell", RegistryKeyPermissionCheck.ReadWriteSubTree);
            shell.SetValue("", "");

            RegistryKey shellOpen = shell.CreateSubKey("open", RegistryKeyPermissionCheck.ReadWriteSubTree);
            shellOpen.SetValue("FriendlyAppName", "MagnetCopy", RegistryValueKind.String);

            RegistryKey shellOpenCommand = shellOpen.CreateSubKey("command", RegistryKeyPermissionCheck.ReadWriteSubTree);
            shellOpenCommand.SetValue("", pathParam);

            key.Close();
            return "true";
        }

        public static string DeleteMagnet()
        {
            if (!Base.Inspect.IsAdministrator())
            {
                Console.WriteLine(Base.Inspect.RunAdminMsg);
                string result = Console.ReadLine();
                if (result.ToLower() == "y")
                {
                    Base.Inspect.RunRunas("cmd=magnet_uninstall");
                }
                else
                {
                    return "false";
                }
            }

            RegistryKey key = Registry.ClassesRoot;
            if (key != null)
            {
                key.DeleteSubKeyTree(addr);
                key.Close();
            }

            return "true";
        }
    }
}
