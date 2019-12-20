using mLauncher.Base;
using mLauncher.Control;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace mLauncher
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private int tabCount = 1;
        private int colCount = 0;
        private int rowCount = 0;
        private DataTable tabs;
        private DataTable buttons;

        public MainWindow()
        {
            InitializeComponent();

            DataBase.InitDB();

            tabs = DataBase.GetTabs();
            buttons = DataBase.GetButtons();
            tabCount = tabs.Rows.Count;

            string cols = DataBase.GetSetting("COLS");
            string rows = DataBase.GetSetting("ROWS");
            colCount = int.Parse(cols);
            rowCount = int.Parse(rows);

            DrawLauncher();
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

                        button.DragEnter += new DragEventHandler(Button_DragEnter);
                        button.Drop += new DragEventHandler(Button_Drop);
                        button.PreviewMouseDown += new MouseButtonEventHandler( Button_PreviewMouseDown);
                        button.PreviewMouseMove +=new MouseEventHandler( Button_PreviewMouseMove);
                        button.PreviewMouseUp += new MouseButtonEventHandler( Button_PreviewMouseUp);


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

        private void Button_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else if (e.Data.GetDataPresent(typeof(MButton)))
                e.Effects = DragDropEffects.Copy;
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DraggingFromGrid = true;
                DraggingStartPoint = e.GetPosition(this);
            }
        }

        private void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var button = sender as MButton;

            if (DraggingFromGrid && button.IconImage != null)
            {
                System.Windows.Point point = e.GetPosition(this);
                if (System.Math.Abs(point.X - DraggingStartPoint.X) > 10 || System.Math.Abs(point.Y - DraggingStartPoint.Y) > 10)
                {
                    DraggingFromGrid = false;
                    
                    DragDrop.DoDragDrop((MButton)sender, button, DragDropEffects.Copy);

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
        #endregion


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

        private void Button_Drop(object sender, DragEventArgs e)
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
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                string path = files.AsEnumerable().First(); ;
                if (!File.Exists(path) && !Directory.Exists(path))
                    return;

                if (PathManager.GetType(path) == PathManager.PathType.Shotcut)
                    path = PathManager.AbsolutePath(path);

                var btn = sender as MButton;
                InsertButton(btn, path);
            }

        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MButton button = sender as MButton;
            Console.WriteLine(string.Format("{0}, {1}, {2}", button.TabId, button.Col, button.Row));
        }


    }
}
