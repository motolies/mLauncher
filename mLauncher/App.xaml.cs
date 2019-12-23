using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace mLauncher
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private static string mutexName = Assembly.GetExecutingAssembly().GetName().Name;
        private static Mutex mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createNew;
            mutex = new Mutex(true, mutexName, out createNew);
            if (createNew)
            {
                mutex.ReleaseMutex();
            }
            else
            {
                // 이전에 실행한 프로그램이 있다면 닫고 새로 연다
                List<Process> processes = Process.GetProcessesByName("mLauncher").Where(p => p.HandleCount > 0).ToList();
                foreach (Process proc in processes)
                {
                    if (proc.ProcessName.Equals(mutexName) && proc.Id != Process.GetCurrentProcess().Id)
                    {
                        proc.Kill();
                    }
                }
            }
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (mutex != null)
                mutex.ReleaseMutex();
            base.OnExit(e);
        }
    }
}
