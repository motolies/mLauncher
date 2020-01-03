using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mRegistryHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            var parsedArgs = args.Select(pr =>
            {
                string[] strArray = pr.Split(new[] { '=' });
                return new ArgsOptions()
                {
                    Key = strArray[0],
                    Value = strArray[1]
                };
            }).ToList();


            string opt = parsedArgs.Where(k => k.Key == "cmd").Select(k => k.Value).FirstOrDefault();
            if (opt == null)
            {
                Console.WriteLine("cmd 파라미터가 없습니다.");
                return;
            }

            string result = string.Empty;

            switch (opt.ToLower())
            {

                case "mfilesearch_folder_install":
                    {
                        string path = parsedArgs.Where(k => k.Key == "path").Select(k => k.Value).FirstOrDefault();
                        if (path == null)
                        {
                            Console.WriteLine("path 파라미터가 없습니다.");
                            return;
                        }
                        result = Function.FileSearchExplorerContext.SetContextFolder(path);
                    }
                    break;
                case "mfilesearch_folder_uninstall":
                    result = Function.FileSearchExplorerContext.DeleteContextFolder();
                    break;

                //case "magnet_install":
                //    {
                //        string path = parsedArgs.Where(k => k.Key == "path").Select(k => k.Value).FirstOrDefault();
                //        if (path == null)
                //        {
                //            Console.WriteLine("path 파라미터가 없습니다.");
                //            return;
                //        }

                //        result = Function.MagnetCopy.SetMagnet(path);
                //    }
                //    break;
                //case "magnet_uninstall":
                //    result = Function.MagnetCopy.DeleteMagnet();
                //    break;

            }

            Console.Write(result);
        }
    }
    class ArgsOptions
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
