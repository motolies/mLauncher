using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace mLauncher
{
    /// <summary>
    /// MoveWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MoveWindow : Window
    {
        public MoveWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
      
        public string ProcessName
        {
            get { return (string)GetValue(ProcessNameProperty); }
            set { SetValue(ProcessNameProperty, value); }
        }
        public static readonly DependencyProperty ProcessNameProperty = DependencyProperty.Register("ProcessName", typeof(string), typeof(MoveWindow));
              

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            MoveWindows();
        }


        private void ProcessName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ProcessName = (sender as TextBox).Text;
                MoveWindows();
            }
        }


        #region window move
        private void MoveWindows()
        {
            Process[] processes = Process.GetProcesses();
            var target = processes.Where(p => 
                p.ProcessName.ToLower() == System.IO.Path.GetFileNameWithoutExtension(this.ProcessName).ToLower() 
                && p.MainWindowHandle != IntPtr.Zero
                ).ToList();

            foreach (var process in target)
            {
                Console.WriteLine("Process Name: {0} ", process.ProcessName);
                Move(process);
            }
        }


        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        private static void Move(Process process)
        {
            const short SWP_NOSIZE = 1;
            const short SWP_NOZORDER = 0X4;
            const int SWP_SHOWWINDOW = 0x0040;

            IntPtr handle = process.MainWindowHandle;
            if (handle != IntPtr.Zero)
            {
                SetWindowPos(process.MainWindowHandle, 0, 0, 0, 0, 0, SWP_NOZORDER | SWP_NOSIZE | SWP_SHOWWINDOW);
            }
                
        }
        #endregion


    }
}
