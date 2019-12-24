using mEx;
using mHK.Keyboad;
using mLauncher.Base;
using mLauncher.Control;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Form = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Reflection;

namespace mLauncher
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private int WindowWidth = 0;
        private int WindowHeight = 0;
        private bool IsTopMost;
        private int tabCount = 1;
        private int colCount = 0;
        private int rowCount = 0;
        private DataTable tabs;
        private DataTable buttons;
        static System.Threading.Timer listClearTimer;
        private System.Windows.Controls.ContextMenu contextMenu = new System.Windows.Controls.ContextMenu();

        DispatcherTimer ResizeTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 2, 0), IsEnabled = false };

        public MainWindow()
        {
            InitializeComponent();

            DataBase.InitDB();

            tabs = DataBase.GetTabs();
            buttons = DataBase.GetButtons();
            tabCount = tabs.Rows.Count;

            string width = DataBase.GetSetting("WIDTH");
            string height = DataBase.GetSetting("HEIGHT");
            this.Width = WindowWidth = int.Parse(width);
            this.Height = WindowHeight = int.Parse(height);

            string topMost = DataBase.GetSetting("TOPMOST");
            IsTopMost = bool.Parse(topMost);

            string cols = DataBase.GetSetting("COLS");
            string rows = DataBase.GetSetting("ROWS");
            colCount = int.Parse(cols);
            rowCount = int.Parse(rows);

            this.PreviewMouseWheel += new MouseWheelEventHandler(Window_PreviewMouseWheel);
            ResizeTimer.Tick += Window_ResizeTimer_Tick;


            SetContextMenu();
            DrawLauncher();
            LocalKeyboardHook();
            GlobalKeyboardHook();
            LocalTimer();

        }






        #region context menu

        private void SetContextMenu()
        {
            contextMenu.AddHandler(System.Windows.Controls.MenuItem.ClickEvent, new RoutedEventHandler(MenuItem_Click));

            var MenuItems = new Dictionary<string, string>()
            {
                { "DeleteButton", "삭제" },
                { "Explorer", "탐색기" },
                { "Settings", "설정" },
            };

            foreach (var item in MenuItems)
            {
                var menu = new System.Windows.Controls.MenuItem();
                menu.Name = item.Key;
                menu.Header = item.Value;
                contextMenu.Items.Add(menu);
            }
        }

        private void Button_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                var btn = sender as MButton;

                System.Windows.Controls.ContextMenu cm = btn.ContextMenu;

                if (string.IsNullOrWhiteSpace(btn.Path))
                {
                    foreach (System.Windows.Controls.MenuItem mi in cm.Items)
                    {
                        if (mi.Name == "DeleteButton" || mi.Name == "Explorer")
                            mi.IsEnabled = false;
                    }
                }
                else
                {
                    foreach (System.Windows.Controls.MenuItem mi in cm.Items)
                    {
                        if (mi.Name == "DeleteButton" || mi.Name == "Explorer")
                            mi.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var placementTarget = ((System.Windows.Controls.ContextMenu)sender).PlacementTarget as MButton;
            var menuItem = e.Source as System.Windows.Controls.MenuItem;

            switch (menuItem.Name)
            {
                case "DeleteButton":
                    DeleteButton(placementTarget);
                    break;
                case "Explorer":
                    Process.Start("explorer.exe", Path.GetDirectoryName(placementTarget.Path));
                    break;
                case "Settings":
                    SettingWindowsShow();

                    break;
                default:
                    break;
            }
        }

        #endregion

        private void SettingWindowsShow()
        {
            SaveWindowSize();

            SettingWindow settingWindow = new SettingWindow();
            settingWindow.Owner = this;
            settingWindow.ShowDialog();
        }

        private void DrawLauncher()
        {
            // 탭부터 만들기
            foreach (DataRow row in tabs.Rows)
            {
                TabItem tab = new TabItem();
                tab.Header = row["Name"];
                tab.Tag = row["Id"];
                Grid grid = new Grid();
                tab.Content = grid;
                tabControl.Items.Add(tab);


                for (int r = 0; r < rowCount; r++)
                {
                    grid.RowDefinitions.Add(new RowDefinition()
                    {
                        Height = new GridLength(1, GridUnitType.Star)
                    });

                    for (int c = 0; c < colCount; c++)
                    {

                        if (r == 0)
                        {
                            grid.ColumnDefinitions.Add(new ColumnDefinition()
                            {
                                Width = new GridLength(1, GridUnitType.Star)
                            });
                        }

                        // col 선택 후 버튼 삽입
                        MButton button = new MButton()
                        {
                            TabId = tab.Tag.ToString(),
                            Row = r,
                            Col = c,
                        };
                        button.AllowDrop = true;
                        button.Click += new RoutedEventHandler(Button_Click);

                        button.ContextMenu = this.contextMenu;
                        button.ContextMenuOpening += Button_ContextMenuOpening;

                        button.DragEnter += new System.Windows.DragEventHandler(Button_DragEnter);
                        button.Drop += new System.Windows.DragEventHandler(Button_Drop);
                        button.PreviewMouseDown += new MouseButtonEventHandler(Button_PreviewMouseDown);
                        button.PreviewMouseMove += new System.Windows.Input.MouseEventHandler(Button_PreviewMouseMove);
                        button.PreviewMouseUp += new MouseButtonEventHandler(Button_PreviewMouseUp);


                        Grid.SetRow(button, r);
                        Grid.SetColumn(button, c);
                        grid.Children.Add(button);

                        // button 가져오기
                        DataRow rButton = buttons.AsEnumerable().FirstOrDefault(dr =>
                            dr.Field<string>("TabId") == row["Id"].ToString() && dr.Field<Int64>("Col") == c && dr.Field<Int64>("Row") == r);
                        if (rButton != null)
                        {
                            button.Text = rButton.Field<string>("Name");
                            button.IconImage = IconManager.ByteToImage(rButton.Field<byte[]>("Icon"));
                            button.Path = rButton.Field<string>("Path");
                        }

                    }
                }
            }
        }

        #region drag and drop

        private bool DraggingFromGrid = false;
        private System.Windows.Point DraggingStartPoint = new System.Windows.Point();

        private void Button_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                e.Effects = System.Windows.DragDropEffects.Copy;
            else if (e.Data.GetDataPresent(typeof(MButton)))
                e.Effects = System.Windows.DragDropEffects.Copy;
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DraggingFromGrid = true;
                DraggingStartPoint = e.GetPosition(this);
            }
        }

        private void Button_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var button = sender as MButton;

            if (DraggingFromGrid && button.IconImage != null)
            {
                System.Windows.Point point = e.GetPosition(this);
                if (System.Math.Abs(point.X - DraggingStartPoint.X) > 10 || System.Math.Abs(point.Y - DraggingStartPoint.Y) > 10)
                {
                    DraggingFromGrid = false;

                    DragDrop.DoDragDrop((MButton)sender, button, System.Windows.DragDropEffects.Copy);

                    Console.WriteLine(" mouse up ");
                }
            }
        }
        private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DraggingFromGrid)
            {
                DraggingFromGrid = false;
            }
        }


        private void Button_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(MButton)))
            {
                // 버튼끼리의 위치이동 구현예정

                var origin = e.Data.GetData(typeof(MButton)) as MButton;
                var btn = sender as MButton;
                if (origin.Col != btn.Col || origin.Row != btn.Row)
                {
                    string path = origin.Path;
                    if (!File.Exists(path) && !Directory.Exists(path))
                        return;

                    if (PathManager.GetType(path) == PathManager.PathType.Shotcut)
                        path = PathManager.AbsolutePath(path);

                    InsertButton(btn, path);
                    DeleteButton(origin);
                }
            }
            else
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);

                string path = files.AsEnumerable().First(); ;
                if (!File.Exists(path) && !Directory.Exists(path))
                    return;

                if (PathManager.GetType(path) == PathManager.PathType.Shotcut)
                    path = PathManager.AbsolutePath(path);

                var btn = sender as MButton;
                InsertButton(btn, path);
            }

        }

        #endregion

        #region local hotkey

        public static RoutedCommand LocalHotKey = new RoutedCommand();
        private void LocalKeyboardHook()
        {
            // CommandBinding에서 무엇을 눌렀나 확인해보려고 했는데, 프레임워크가 감춘다...

            LocalHotKey.InputGestures.Add(new KeyGesture(Key.Escape, System.Windows.Input.ModifierKeys.None));
            LocalHotKey.InputGestures.Add(new KeyGesture(Key.F12, System.Windows.Input.ModifierKeys.None));
            LocalHotKey.InputGestures.Add(new KeyGesture(Key.Delete, System.Windows.Input.ModifierKeys.None));

            //LocalHotKey.InputGestures.Add(new KeyGesture(Key.F, System.Windows.Input.ModifierKeys.Control));
            //LocalHotKey.InputGestures.Add(new KeyGesture(Key.S, System.Windows.Input.ModifierKeys.Control));

            this.CommandBindings.Add(new CommandBinding(LocalHotKey, Local_KeyPressed));
        }

        private void Local_KeyPressed(object sender, ExecutedRoutedEventArgs e)
        {
            // 이렇게 구분이 가능한 거 같긴 한데 꼼수인거 같다
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                // 숨기기
                Visibility = Visibility.Collapsed;
            }
            else if (Keyboard.IsKeyDown(Key.F12))
            {
                SettingWindowsShow();
            }
            else if (Keyboard.IsKeyDown(Key.Delete))
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    System.Windows.Point point = Mouse.GetPosition(System.Windows.Application.Current.MainWindow);
                    //VisualTreeHelper.HitTest(this, new HitTestFilterCallback(MyHitTestFilter), new HitTestResultCallback(MyHitTestResult), new PointHitTestParameters(point));
                    VisualTreeHelper.HitTest(this, null, new HitTestResultCallback(MyHitTestResult), new PointHitTestParameters(point));
                }));
            }



        }


        private HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            Console.WriteLine("result : " + result.VisualHit.GetType());

            // wpf 에서 의도적으로 구현했는지 모르겠지만, userControl은 hitTest에서 result로 떨어지지 않는다.
            // 그래서 눈에 보이는 놈 하나만 발견하면 다 중지시켜버리고
            // 돔 구조니까 부모를 따라가도록 한다.

            var std = result.VisualHit as DependencyObject;
            MButton button = std.GetVisualParent<MButton>();

            DeleteButton(button);

            return HitTestResultBehavior.Stop;

        }

        private HitTestFilterBehavior MyHitTestFilter(DependencyObject potentialHitTestTarget)
        {
            // https://docs.microsoft.com/en-us/dotnet/framework/wpf/graphics-multimedia/hit-testing-in-the-visual-layer?redirectedfrom=MSDN#using_a_hit_test_filter_callback

            // if문에 있는 애들은 모두 제외대상 클래스
            if (potentialHitTestTarget.GetType() == typeof(System.Windows.Controls.Label))
                return HitTestFilterBehavior.ContinueSkipSelf;
            else if (potentialHitTestTarget.GetType() == typeof(System.Windows.Controls.Image))
                return HitTestFilterBehavior.ContinueSkipSelf;
            else if (potentialHitTestTarget.GetType() == typeof(System.Windows.Controls.TextBlock))
                return HitTestFilterBehavior.ContinueSkipSelf;
            else if (potentialHitTestTarget.GetType() == typeof(System.Windows.Controls.Border))
                return HitTestFilterBehavior.ContinueSkipSelf;
            else
                return HitTestFilterBehavior.Continue;
        }





        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var maxIndex = tabControl.Items.Count;
            var currentIndex = tabControl.SelectedIndex;

            if (e.Delta > 0)
            {
                if (0 < currentIndex)
                    tabControl.SelectedIndex--;
            }
            else
            {
                if (maxIndex > currentIndex)
                    tabControl.SelectedIndex++;
            }
        }



        #endregion

        #region global hotkey hook

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool GetCursorPos(out POINT pt);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        KeyboardHook hook = new KeyboardHook();

        private void GlobalKeyboardHook()
        {
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(Global_KeyPressed);

            hook.RegisterHotKey(mHK.Keyboad.ModifierKeys.Control | mHK.Keyboad.ModifierKeys.Shift, Form.Keys.A);
            // hook.RegisterHotKey(mHOOK.Keyboad.ModifierKeys.None, Form.Keys.Delete);

        }

        private void Global_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            Console.WriteLine(e.Modifier.ToString() + " + " + e.Key.ToString());
            if (e.Modifier == (mHK.Keyboad.ModifierKeys.Control | mHK.Keyboad.ModifierKeys.Shift) && e.Key == Form.Keys.A)
            {
                WindowShow();
            }

        }



        #endregion

        private void WindowShow()
        {
            // 현재 커서위치 알아오기
            POINT CursorPoint = new POINT();
            GetCursorPos(out CursorPoint);

            this.Left = CursorPoint.X;
            this.Top = CursorPoint.Y;
            if (this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Normal;
            // 우선은 최상단에 보이게 한 후에 다시 설정값으로 변경
            this.Topmost = true;
            this.Show();
            this.Topmost = IsTopMost;
        }


        private bool InsertButton(MButton btn, string path)
        {
            // 파일명 읽어와야 함
            string fileName = Path.GetFileName(path);
            ImageSource imgIcon = IconManager.GetIcon(path);
            byte[] baIcon = IconManager.ImageSourceToByte(imgIcon);

            if (DataBase.InsertButton(btn.TabId, btn.Col, btn.Row, fileName, path, baIcon) < 1)
                return false;

            btn.IconImage = imgIcon;
            btn.Text = fileName;
            btn.Path = path;
            return true;
        }

        private bool DeleteButton(MButton btn)
        {

            if (DataBase.DeleteButton(btn.TabId, btn.Col, btn.Row) < 1)
                return false;

            btn.IconImage = null;
            btn.Text = string.Empty;
            btn.Path = string.Empty;
            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MButton btn = sender as MButton;
            if (string.IsNullOrWhiteSpace(btn.Path))
                return;

            Process.Start(btn.Path);
        }

        #region thread timer
        private void LocalTimer()
        {

            //스레드 타이머
            TimerCallback tc = new TimerCallback((o) =>
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    //txtDatetime.Text = DateTime.Now.ToString("yyyy-MM-dd(ddd) tt hh:mm:ss");
                }));

            });
            listClearTimer = new System.Threading.Timer(tc, Dispatcher, 0, 500);
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            //this.WindowState = WindowState.Minimized;
            Visibility = Visibility.Collapsed;

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // ResizeEnd 이벤트가 없어서 타이머 돌린다
            ResizeTimer.IsEnabled = true;
            ResizeTimer.Stop();
            ResizeTimer.Start();
        }

        private void Window_ResizeTimer_Tick(object sender, EventArgs e)
        {
            ResizeTimer.IsEnabled = false;
            SaveWindowSize();
        }

        private void SaveWindowSize()
        {
            WindowWidth = Convert.ToInt32(this.Width);
            DataBase.SetSetting("WIDTH", WindowWidth.ToString());
            WindowHeight = Convert.ToInt32(this.Height);
            DataBase.SetSetting("HEIGHT", WindowHeight.ToString());
        }


    }
}
