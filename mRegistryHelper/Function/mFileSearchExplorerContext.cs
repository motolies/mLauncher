using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mRegistryHelper.Function
{
    internal class FileSearchExplorerContext
    {
        static readonly string addr = @"Directory\shell\mFileSearch";

        public static bool GetContextFolder()
        {
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(addr);
            return key != null;
        }

        public static string SetContextFolder(string path)
        {
            if (!Base.Inspect.IsAdministrator())
            {
                Console.WriteLine(Base.Inspect.RunAdminMsg);
                string result = Console.ReadLine();
                if (result.ToLower() == "y")
                {
                    Base.Inspect.RunRunas(string.Format("cmd=mfilesearch_folder_install path=\"{0}\"", path));
                }
                else
                {
                    return "false";
                }
            }

            bool exists = File.Exists(path);

            if (!exists)
                throw new Exception("mFileSearch.exe 파일을 찾을 수 없습니다.");

            string pathParam = string.Format("\"{0}\" dir=\"%L\"", path);

            RegistryKey key = Registry.ClassesRoot.CreateSubKey(addr, RegistryKeyPermissionCheck.ReadWriteSubTree);
            key.SetValue("", "mFileSearch로 열기");
            key.SetValue("Icon", path, RegistryValueKind.String);

            RegistryKey cmd = key.CreateSubKey("Command", RegistryKeyPermissionCheck.ReadWriteSubTree);
            cmd.SetValue("", pathParam);

            key.Close();
            return "true";
        }

        public static string DeleteContextFolder()
        {
            if (!Base.Inspect.IsAdministrator())
            {
                Console.WriteLine(Base.Inspect.RunAdminMsg);
                string result = Console.ReadLine();
                if (result.ToLower() == "y")
                    Base.Inspect.RunRunas("cmd=mfilesearch_folder_uninstall");
                else
                    return "false";
            }

            RegistryKey key = Registry.ClassesRoot;
            if (key != null)
            {
                key.DeleteSubKeyTree(addr, false);
                key.Close();
            }

            return "true";
        }


    }
}
