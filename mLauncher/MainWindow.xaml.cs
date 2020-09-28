using mHK.Keyboad;
using mLauncher.Base;
using mLauncher.Control;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Form = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Reflection;
using mEX;
using mUT;
using mWA;
using System.Windows.Interop;

namespace mLauncher
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private int WindowWidth = 0;
        private int WindowHeight = 0;
        private bool IsTopMost = true;
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
                    Process.Start("explorer.exe", string.Format(@"/select,""{0}""", placementTarget.Path));
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
                var origin = e.Data.GetData(typeof(MButton)) as MButton;
                var btn = sender as MButton;

                if (origin.Col != btn.Col || origin.Row != btn.Row)
                {
                    string path = origin.Path;
                    string bpath = btn.Path;

                    if (!File.Exists(path) && !Directory.Exists(path))
                        return;

                    if (PathManager.GetType(path) == PathManager.PathType.Shotcut)
                        path = PathManager.AbsolutePath(path);

                    InsertButton(btn, path);
                    DeleteButton(origin);

                    if (!string.IsNullOrWhiteSpace(bpath))
                        InsertButton(origin, bpath);
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




        KeyboardHook hook = new KeyboardHook();

        private void GlobalKeyboardHook()
        {
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(Global_KeyPressed);

            hook.RegisterHotKey(mHK.Keyboad.ModifierKeys.Control | mHK.Keyboad.ModifierKeys.Shift, Form.Keys.A);
            hook.RegisterHotKey(mHK.Keyboad.ModifierKeys.Control | mHK.Keyboad.ModifierKeys.Shift, Form.Keys.Q);

            // volume
            hook.RegisterHotKey(mHK.Keyboad.ModifierKeys.Control | mHK.Keyboad.ModifierKeys.Shift | mHK.Keyboad.ModifierKeys.Alt, Form.Keys.Right);
            hook.RegisterHotKey(mHK.Keyboad.ModifierKeys.Control | mHK.Keyboad.ModifierKeys.Shift | mHK.Keyboad.ModifierKeys.Alt, Form.Keys.Left);
            hook.RegisterHotKey(mHK.Keyboad.ModifierKeys.Control | mHK.Keyboad.ModifierKeys.Shift | mHK.Keyboad.ModifierKeys.Alt, Form.Keys.Down);

        }

        private void Global_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            Console.WriteLine(e.Modifier.ToString() + " + " + e.Key.ToString());
            if (e.Modifier == (mHK.Keyboad.ModifierKeys.Control | mHK.Keyboad.ModifierKeys.Shift) && e.Key == Form.Keys.A)
            {
                WindowShow();
            }
            else if (e.Modifier == (mHK.Keyboad.ModifierKeys.Control | mHK.Keyboad.ModifierKeys.Shift) && e.Key == Form.Keys.Q)
            {
                StartSubProgram("mFileSearch.exe");
            }
            else if (e.Modifier == (mHK.Keyboad.ModifierKeys.Control | mHK.Keyboad.ModifierKeys.Shift | mHK.Keyboad.ModifierKeys.Alt) && e.Key == Form.Keys.Right)
            {
                Volume.VolUp(new WindowInteropHelper(this).Handle);
            }
            else if (e.Modifier == (mHK.Keyboad.ModifierKeys.Control | mHK.Keyboad.ModifierKeys.Shift | mHK.Keyboad.ModifierKeys.Alt) && e.Key == Form.Keys.Left)
            {
                Volume.VolDown(new WindowInteropHelper(this).Handle);
            }
            else if (e.Modifier == (mHK.Keyboad.ModifierKeys.Control | mHK.Keyboad.ModifierKeys.Shift | mHK.Keyboad.ModifierKeys.Alt) && e.Key == Form.Keys.Down)
            {
                Volume.Mute(new WindowInteropHelper(this).Handle);
            }

        }

        private void StatusButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.Name)
            {
                case "btnFileSearch":
                    StartSubProgram("mFileSearch.exe");
                    break;
                case "btnShutdown":
                    {
                        MessageBoxResult dialog = MessageBox.Show("저장하지 않은 내용은 사라집니다.\r\n컴퓨터를 종료하시겠습니까?", "종료"
                            , MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Cancel);
                        if (dialog == MessageBoxResult.Yes)
                        {
                            ProcessStartInfo psi = new ProcessStartInfo();
                            psi.FileName = "shutdown.exe";
                            psi.Arguments = "-s -f -t 00";
                            Process.Start(psi);
                        }
                    }
                    break;
                case "btnWindowMove":
                    {
                        MoveWindow moveWindow = new MoveWindow();
                        moveWindow.Owner = this;
                        moveWindow.ShowDialog();
                    }
                    break;
            }
        }

        private void StartSubProgram(string file)
        {
            try
            {
                string exefile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), file);
                Process.Start(exefile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("실행에 실패하였습니다.");
                Console.WriteLine(ex.Message);
            }
        }


        #endregion

        private void WindowShow()
        {
            // 현재 커서위치 알아오기

            System.Windows.Point CursorPoint = MouseCursor.GetCursorPos(this);

            this.Left = CursorPoint.X;
            this.Top = CursorPoint.Y;
            if (this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Normal;
            // 우선은 최상단에 보이게 한 후에 다시 설정값으로 변경
            this.Topmost = true;
            this.Show();
            this.Activate();

            // 리모트 데스크탑 강제로 다시 렌더링하기
            this.UpdateLayout();


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

            try
            {
                if (File.Exists(btn.Path))
                {
                    if (Path.GetExtension(btn.Path).ToLower() == ".bat")
                    {
                        string path = Path.GetDirectoryName(btn.Path);
                        string file = Path.GetFileName(btn.Path);

                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.WorkingDirectory = path;
                        psi.FileName = "cmd.exe";
                        psi.Arguments = string.Format("/C \"{0}\"", file);
                        Process.Start(psi);
                    }
                    else
                    {
                        Process.Start(btn.Path);
                    }
                }
                else if (Directory.Exists(btn.Path))
                    Process.Start(btn.Path);
                else
                {
                    MessageBox.Show(string.Format("해당 경로의 파일이 없습니다.\r\n{0}", btn.Path));
                    return;
                }

            }
            catch { }

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

                    // window 화면이 뜰 때는 무조건 true로 띄우고 여기서 값을 바꾸자
                    this.Topmost = IsTopMost;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 이것만 로드 된 후로 변경하자
            string topMost = DataBase.GetSetting("TOPMOST");
            IsTopMost = bool.Parse(topMost);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            // 원격세션에서 창에 검정색으로 나오는 걸 막기 위해서 여기에서 한 번 더 그려준다

        }
    }
}
