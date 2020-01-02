using mFileSearch.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace mFileSearch
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {

            var parsedArgs = e.Args.Select(args => {
                string[] strArray = args.Split(new[] { '=' });
                string value = string.Empty;
                for (int i = 1; i < strArray.Length; i++)
                    value += strArray[i];
                return new ArgsOptions()
                {
                    Key = strArray[0],
                    Value = value
                };
            }).ToList();

            MainWindow mainWindow = new MainWindow();
            mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mainWindow.Topmost = true;

            foreach (var p in parsedArgs)
            {
                if (p.Key == "dir")
                    mainWindow.Folders.Add(new TargetFolder() { Path = p.Value, Enable = true });
            }

            mainWindow.Show();
            mainWindow.Topmost = false;

        }
        class ArgsOptions
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}
