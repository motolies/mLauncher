using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace mLauncher.Base
{
    public static class IconManager
    {
        // https://stackoverflow.com/questions/2701263/get-the-icon-for-a-given-extension


        //private static readonly Dictionary<string, ImageSource> _smallIconCache = new Dictionary<string, ImageSource>();
        //private static readonly Dictionary<string, ImageSource> _largeIconCache = new Dictionary<string, ImageSource>();







        public static ImageSource GetIcon(string path)
        {
            System.Console.WriteLine(PathManager.GetType(path).ToString());
            switch (PathManager.GetType(path))
            {
                case PathManager.PathType.Shotcut:
                    {
                        //using (Icon i = Icon.ExtractAssociatedIcon(PathManager.AbsolutePath(path)))
                        //{
                        //    return  new Bitmap(i.ToBitmap());
                        //}
                        string abPath = PathManager.AbsolutePath(path);
                        return FindIconForFilename(abPath, true);
                    }
                case PathManager.PathType.File:
                    {
                        return FindIconForFilename(path, true);
                    }
                case PathManager.PathType.NotExists:
                    {
                        return BitmapToImageSource(Properties.Resources.xicon);
                    }
                case PathManager.PathType.Directory:
                    {
                        //return GetIcon(path, false, false).ToBitmap();
                        return FindIconForFilename(path, true, true);
                    }

                case PathManager.PathType.Null:
                default:
                    return null;
            }
        }


        public static ImageSource ByteToImage(byte[] imageData)
        {
            Image image;
            using (var ms = new MemoryStream(imageData))
            {
                image = Image.FromStream(ms);
            }

            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                return bitmapImage;
            }

        }

        public static byte[] ImageSourceToByte(ImageSource img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img as BitmapSource));
                encoder.Save(ms);
                ms.Flush();
                return ms.ToArray();
            }
        }

        public static byte[] ImageToByte(Image img)
        {

            byte[] imageBytes;

            using (var ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                imageBytes = ms.ToArray();
            }
            return imageBytes;
        }

        public static Image ImageSourceToImage(ImageSource image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image as BitmapSource));
                encoder.Save(ms);
                ms.Flush();
                return Image.FromStream(ms);
            }
        }


        /// <summary>
        /// Get an bitmap icon for a given filename
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Bitmap ToBitmap(string fileName)
        {
            ImageSource imageSource = FindIconForFilename(fileName, true);

            BitmapSource source = (BitmapSource)imageSource;

            Bitmap bmp = new Bitmap(source.PixelWidth, source.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            return bmp;

        }


        /// <summary>
        /// Get an icon for a given filename
        /// </summary>
        /// <param name="fileName">any filename</param>
        /// <param name="large">16x16 or 32x32 icon</param>
        /// <returns>null if path is null, otherwise - an icon</returns>
        public static ImageSource FindIconForFilename(string fileName, bool large, bool isfolder = false)
        {
            var extension = Path.GetExtension(fileName);
            if (extension == null)
                return null;
            //var cache = large ? _largeIconCache : _smallIconCache;
            ImageSource icon;
            //if (cache.TryGetValue(extension, out icon))
            //    icon = IconReader.GetFileIcon(fileName, large ? IconReader.IconSize.Large : IconReader.IconSize.Small, false, true).ToImageSource();
            //else
            //    icon = IconReader.GetFileIcon(fileName, large ? IconReader.IconSize.Large : IconReader.IconSize.Small, false).ToImageSource();

            if (isfolder)
                icon = IconReader.GetFileIcon(fileName, large ? IconReader.IconSize.Large : IconReader.IconSize.Small, false, true).ToImageSource();
            else
                icon = IconReader.GetFileIcon(fileName, large ? IconReader.IconSize.Large : IconReader.IconSize.Small, false).ToImageSource();

            //cache.Add(extension, icon);
            return icon;
        }
        /// <summary>
        /// http://stackoverflow.com/a/6580799/1943849
        /// </summary>
        static ImageSource ToImageSource(this Icon icon)
        {
            var imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            return imageSource;
        }



        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static ImageSource BitmapToImageSource(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }


 

        //public static Bitmap ImageSourceToBitmap(ImageSource ImageSource)
        //{
        //    Image image = new Bitmap(ImageSource);

        //}



        /// <summary>
        /// Provides static methods to read system icons for both folders and files.
        /// </summary>
        /// <example>
        /// <code>IconReader.GetFileIcon("c:\\general.xls");</code>
        /// </example>
        static class IconReader
        {
            /// <summary>
            /// Options to specify the size of icons to return.
            /// </summary>
            public enum IconSize
            {
                /// <summary>
                /// Specify large icon - 32 pixels by 32 pixels.
                /// </summary>
                Large = 0,
                /// <summary>
                /// Specify small icon - 16 pixels by 16 pixels.
                /// </summary>
                Small = 1
            }
            /// <summary>
            /// Returns an icon for a given file - indicated by the name parameter.
            /// </summary>
            /// <param name="name">Pathname for file.</param>
            /// <param name="size">Large or small</param>
            /// <param name="linkOverlay">Whether to include the link icon</param>
            /// <returns>System.Drawing.Icon</returns>
            public static Icon GetFileIcon(string name, IconSize size, bool linkOverlay, bool isFolder = false)
            {
                var shfi = new Shell32.Shfileinfo();
                var flags = Shell32.ShgfiIcon | Shell32.ShgfiUsefileattributes;
                if (linkOverlay) flags += Shell32.ShgfiLinkoverlay;
                /* Check the size specified for return. */
                if (IconSize.Small == size)
                    flags += Shell32.ShgfiSmallicon;
                else
                    flags += Shell32.ShgfiLargeicon;

                if (isFolder)
                {
                    flags = Shell32.ShgfiIcon | Shell32.shgopenIcon;
                    Shell32.SHGetFileInfo(name, 256, ref shfi, (uint)Marshal.SizeOf(shfi), flags);
                }
                else
                {
                    Shell32.SHGetFileInfo(name, Shell32.FileAttributeNormal, ref shfi, (uint)Marshal.SizeOf(shfi), flags);
                }

                // Copy (clone) the returned icon to a new object, thus allowing us to clean-up properly
                var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
                User32.DestroyIcon(shfi.hIcon);     // Cleanup
                return icon;
            }
        }
        /// <summary>
        /// Wraps necessary Shell32.dll structures and functions required to retrieve Icon Handles using SHGetFileInfo. Code
        /// courtesy of MSDN Cold Rooster Consulting case study.
        /// </summary>
        static class Shell32
        {
            private const int MaxPath = 256;
            [StructLayout(LayoutKind.Sequential)]
            public struct Shfileinfo
            {
                private const int Namesize = 80;
                public readonly IntPtr hIcon;
                private readonly int iIcon;
                private readonly uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxPath)]
                private readonly string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Namesize)]
                private readonly string szTypeName;
            };
            public const uint ShgfiIcon = 0x000000100;     // get icon
            public const uint shgopenIcon = 0x00000002;
            public const uint ShgfiLinkoverlay = 0x000008000;     // put a link overlay on icon
            public const uint ShgfiLargeicon = 0x000000000;     // get large icon
            public const uint ShgfiSmallicon = 0x000000001;     // get small icon
            public const uint ShgfiUsefileattributes = 0x000000010;     // use passed dwFileAttribute
            public const uint FileAttributeNormal = 0x00000080;
            [DllImport("Shell32.dll")]
            public static extern IntPtr SHGetFileInfo(
                string pszPath,
                uint dwFileAttributes,
                ref Shfileinfo psfi,
                uint cbFileInfo,
                uint uFlags
                );
        }
        /// <summary>
        /// Wraps necessary functions imported from User32.dll. Code courtesy of MSDN Cold Rooster Consulting example.
        /// </summary>
        static class User32
        {
            /// <summary>
            /// Provides access to function required to delete handle. This method is used internally
            /// and is not required to be called separately.
            /// </summary>
            /// <param name="hIcon">Pointer to icon handle.</param>
            /// <returns>N/A</returns>
            [DllImport("User32.dll")]
            public static extern int DestroyIcon(IntPtr hIcon);
        }
    }
}