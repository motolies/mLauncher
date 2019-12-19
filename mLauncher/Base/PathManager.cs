using IWshRuntimeLibrary;
using System;
using System.Diagnostics;
using System.IO;

namespace mLauncher.Base
{
    public class PathManager
    {
        public enum PathType
        {
            Null, NotExists, Shotcut, Directory, File
        }

        public static PathType GetType(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return PathType.Null;

            if (!System.IO.File.Exists(path) && !Directory.Exists(path))
                return PathType.NotExists;

            FileAttributes attr = System.IO.File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.Directory))
                return PathType.Directory;
            else
            {
                if (Path.GetExtension(path).ToLower().Contains(".lnk"))
                    return PathType.Shotcut;
                else
                    return PathType.File;
            }
        }

        public static string AbsolutePath(string shotcutPath)
        {
            IWshShell shell = new WshShell();// 바로가기 절대경로
            try
            {
                IWshShortcut link = (IWshShortcut)shell.CreateShortcut(shotcutPath);
                return link.TargetPath;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        public static void OpenPath(string path)
        {
            Process.Start("explorer.exe", Path.GetDirectoryName(path));
        }
    }
}

