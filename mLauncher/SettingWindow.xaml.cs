﻿using IWshRuntimeLibrary;
using mLauncher.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Path = System.IO.Path;

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
            WindowWidth = int.Parse(DataBase.GetSetting("WIDTH"));
            WindowHeight = int.Parse(DataBase.GetSetting("HEIGHT"));
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

        public int WindowWidth
        {
            get { return (int)GetValue(WindowWidthProperty); }
            set { SetValue(WindowWidthProperty, value); }
        }
        public static readonly DependencyProperty WindowWidthProperty = DependencyProperty.Register("WindowWidth", typeof(int), typeof(SettingWindow));

        public int WindowHeight
        {
            get { return (int)GetValue(WindowHeightProperty); }
            set { SetValue(WindowHeightProperty, value); }
        }
        public static readonly DependencyProperty WindowHeightProperty = DependencyProperty.Register("WindowHeight", typeof(int), typeof(SettingWindow));


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




        public void SaveSettings(object sender, RoutedEventArgs e)
        {
            DataBase.SetSetting("WIDTH", WindowWidth.ToString());
            DataBase.SetSetting("HEIGHT", WindowHeight.ToString());
            DataBase.SetSetting("COLS", ColumnCount.ToString());
            DataBase.SetSetting("ROWS", RowCount.ToString());
            DataBase.SetSetting("TOPMOST", IsTopMost ? "TRUE" : "FALSE");
            DataBase.SetSetting("STARTUP", StartUp ? "TRUE" : "FALSE");
            InstallStartUp(StartUp);

            DataBase.SetTabs(Tabs);

            // 재시작
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void InstallStartUp(bool status)
        {
            /*
             * Environment.SpecialFolder.CommonStartup 모든사용자
             * Environment.SpecialFolder.Startup 현재사용자
             */

            var startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var projectName = Assembly.GetExecutingAssembly().GetName().Name;

            string file = Path.Combine(startup, string.Format("{0}.lnk", projectName));
            FileInfo fileInfo = new FileInfo(file);

            if (status)
            {
                if(fileInfo.Exists)
                    fileInfo.Delete();

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(file);
                shortcut.TargetPath = Assembly.GetEntryAssembly().Location;
                shortcut.Description = string.Format("{0} link", projectName);
                
                shortcut.Save();
            }
            else
            {
                fileInfo.Delete();
            }

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

        private void AddRow(object sender, RoutedEventArgs e)
        {
            DataRow row = Tabs.NewRow();
            row["Id"] = string.Format("T{0}", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
            row["Name"] = "temp";
            var curSeq = Tabs.AsEnumerable().OrderByDescending(r => r.Field<Int64>("Seq")).First().Field<Int64>("Seq");
            row["Seq"] = curSeq + 1;
            Tabs.Rows.Add(row);
        }

        private void DeleteRow(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow row = (gridView.SelectedItem as DataRowView).Row;
                row.Delete();
            }
            catch (Exception) { }

        }
    }
}

