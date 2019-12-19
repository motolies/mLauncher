using mLauncher.Base;
using mLauncher.Control;
using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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

        public MainWindow()
        {
            InitializeComponent();

            DataBase.InitDB();

            tabs = DataBase.GetTabs();
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
                            Col = c
                        };
                        button.AllowDrop = true;
                        button.Click += new RoutedEventHandler(Button_Click);
                        button.DragEnter += new DragEventHandler(Button_DragEnter);
                        button.Drop += new DragEventHandler(Button_Drop);

                        Grid.SetRow(button, r);
                        Grid.SetColumn(button, c);
                        grid.Children.Add(button);
                    }




                }

            }
        }
       

        private bool InsertButton(MButton btn, string path, string tabId)
        {
            // 파일명 읽어와야 함
            string fileName = "tmpFileName";

            // 아이콘 blob으로 저장해야함
            ImageSource img = IconManager.FindIconForFilename(path, true);
            btn.IconImage = img;

            //if (DataBase.InsertButton(string.Format("INSERT OR REPLACE INTO Button Values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}' )", btn.TabId, btn.Col, btn.Row, fileName, path, null)) < 1)
            //    return false;


            //btn.Image = IconManager.ToBitmap(path);
            //btn.Title = Path.GetFileName(path);
            return true;
        }
        private bool DeleteButton(MButton btn)
        {
            throw new NotImplementedException();

            //if (DB.ExecuteNonQuery(string.Format("Delete From Buttons Where ID = '{0}'", btn.Name)) < 1)
            //    return false;

            //btn.Image = null;
            //btn.Title = string.Empty;
            //return true;
        }

        private void Button_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(MButton)))
            {
                // 버튼끼리의 위치이동 구현예정

                throw new NotImplementedException();

                //var origin = e.Data.GetData(typeof(MButton)) as MButton;
                //var btn = sender as MButton;
                //if (origin.Name != btn.Name)
                //{
                //    string oriPath = DB.ExecuteValue<string>(string.Format("SELECT Path FROM Buttons WHERE ID = '{0}'", origin.Name));

                //    string path = oriPath;
                //    if (!File.Exists(path) && !Directory.Exists(path))
                //        return;

                //    if (PathManager.GetType(oriPath) == PathManager.PathType.Shotcut)
                //        path = PathManager.AbsolutePath(oriPath);

                //    InsertButton(btn, path);
                //    DeleteButton(origin);
                //}
            }
            else
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    string path = file;
                    if (!File.Exists(path) && !Directory.Exists(path))
                        return;

                    if (PathManager.GetType(file) == PathManager.PathType.Shotcut)
                        path = PathManager.AbsolutePath(file);

                    var btn = sender as MButton;

                    string tabId = ((TabItem)((Grid)btn.Parent).Parent).Tag.ToString();

                    InsertButton(btn, path, tabId);
                }
            }

        }

        private void Button_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else if (e.Data.GetDataPresent(typeof(MButton)))
                e.Effects = DragDropEffects.Copy;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MButton button = sender as MButton;
            Console.WriteLine(string.Format("{0}, {1}, {2}", button.TabId, button.Col, button.Row));
        }


    }
}
