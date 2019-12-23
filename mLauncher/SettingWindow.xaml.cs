using mLauncher.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace mLauncher
{
    /// <summary>
    /// SettingWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
            LocalKeyboardHook();
            InitData();
            this.DataContext = this;
        }

        private void InitData()
        {
            ColumnCount = int.Parse(DataBase.GetSetting("COLS"));
            RowCount = int.Parse(DataBase.GetSetting("ROWS"));
            IsTopMost = bool.Parse(DataBase.GetSetting("TOPMOST"));
            StartUp = bool.Parse(DataBase.GetSetting("STARTUP"));
            Tabs = DataBase.GetTabs();
            Tabs.Columns["Id"].ReadOnly = true;

        }


        public DataTable Tabs
        {
            get { return (DataTable)GetValue(TabsProperty); }
            set { SetValue(TabsProperty, value); }
        }
        public static readonly DependencyProperty TabsProperty = DependencyProperty.Register("Tabs", typeof(DataTable), typeof(SettingWindow));

        public int ColumnCount
        {
            get { return (int)GetValue(ColumnCountProperty); }
            set { SetValue(ColumnCountProperty, value); }
        }
        public static readonly DependencyProperty ColumnCountProperty = DependencyProperty.Register("ColumnCount", typeof(int), typeof(SettingWindow));

        public int RowCount
        {
            get { return (int)GetValue(RowCountProperty); }
            set { SetValue(RowCountProperty, value); }
        }
        public static readonly DependencyProperty RowCountProperty = DependencyProperty.Register("RowCount", typeof(int), typeof(SettingWindow));

        public bool IsTopMost
        {
            get { return (bool)GetValue(IsTopMostProperty); }
            set { SetValue(IsTopMostProperty, value); }
        }
        public static readonly DependencyProperty IsTopMostProperty = DependencyProperty.Register("IsTopMost", typeof(bool), typeof(SettingWindow));

        public bool StartUp
        {
            get { return (bool)GetValue(StartUpProperty); }
            set { SetValue(StartUpProperty, value); }
        }
        public static readonly DependencyProperty StartUpProperty = DependencyProperty.Register("StartUp", typeof(bool), typeof(SettingWindow));




        public void SaveSettingsd(object sender, RoutedEventArgs e )
        {
            DataBase.SetSetting("COLS", ColumnCount.ToString());
            DataBase.SetSetting("ROWS", RowCount.ToString());
            DataBase.SetSetting("TOPMOST", IsTopMost ? "TRUE" : "FALSE");
            DataBase.SetSetting("STARTUP", StartUp ? "TRUE" : "FALSE");

            DataBase.SetTabs(Tabs);

            // 재시작
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void CancelWindows(object sender, RoutedEventArgs e)
        {
            this.Close();
        }








        #region hotkey
        public static RoutedCommand LocalHotKey = new RoutedCommand();
        private void LocalKeyboardHook()
        {
            LocalHotKey.InputGestures.Add(new KeyGesture(Key.Escape, System.Windows.Input.ModifierKeys.None));
            this.CommandBindings.Add(new CommandBinding(LocalHotKey, Local_KeyPressed));
        }

        private void Local_KeyPressed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
                this.Close();
        }
        #endregion



    }
}

