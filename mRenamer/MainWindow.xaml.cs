using mRenamer.Modal;
using mRenamer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace mRenamer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }




        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/events/routed-events-overview?view=netdesktop-6.0&redirectedfrom=MSDN
            FrameworkElement sourceFrameworkElement = e.Source as FrameworkElement;
            switch (sourceFrameworkElement.Name)
            {
                case "ClearFileInfo":
                    this.FileInfos.Clear();
                    break;
                case "RemoveFileName":
                    //FileInfos.ToList().ForEach(f => f.targetFileName = "");
                    FileInfos.ToList().ForEach(f => f.RemoveFileName());
                    lv.Items.Refresh();
                    break;
                case "Cancel":
                    Rollback();
                    break;
                case "RemovePosition":
                    RemovePositionWindow_Open();
                    break;
                case "AddNumber":
                    AddNumberWindow_Open();
                    break;
                case "ZeroPadding":
                    ZeroPaddingWindow_Open();
                    break;
                case "AddPrefix":
                    StringAddWindow_Open(PositionEnum.PreFix);
                    break;
                case "AddPostfix":
                    StringAddWindow_Open(PositionEnum.PostFix);
                    break;
                case "Replace":
                    StringReplaceWindow_Open();
                    break;
                case "Apply":
                    Apply_Run();
                    break;

            }
            e.Handled = true;
            Console.WriteLine(sourceFrameworkElement.Name);
        }


        private void Apply_Run()
        {
            // origin 파일의 존재 체크
            List<FileInfo> notExists = FileInfos.Where(f => !f.ExistsOriginFile()).ToList();
            if (notExists.Count > 0)
            {
                MessageBox.Show("원본 파일에 누락이 있습니다.");
                return;
            }

            // target 파일의 파일명 중복 체크
            List<string> duplicate = FileInfos.GroupBy(f => f.targetFileName).Where(g => g.Count() > 1).Select(x => x.Key).ToList();
            if (duplicate.Count > 0)
            {
                MessageBox.Show("변경할 파일명 중 중복인 이름이 있습니다.");
                return;
            }

            try
            {
                // TODO : 파일명 변경 가능한 상태인지 체크(권한? 오픈?)



                // origin 파일을 임시로 이름 변경함
                FileInfos.ToList().ForEach(f => f.ChangeTempFileName());
                if (FileInfos.Where(f => f.status == StatusEnum.Error).Count() > 0)
                {
                    MessageBox.Show("실패하였습니다.");
                    return;
                }

                // 임시로 변경한 파일들을 다시 target 파일명으로 변경
                FileInfos.ToList().ForEach(f => f.ChangeTargetFileName());
                if (FileInfos.Where(f => f.status == StatusEnum.Error).Count() > 0)
                {
                    MessageBox.Show("실패하였습니다.");
                    return;
                }

                lv.Items.Refresh();
                MessageBox.Show("완료");
            }
            catch (Exception ex)
            {
                // 실패하면 전부 원복
                Rollback();
            }


        }

        private void Rollback()
        {
            FileInfos.ToList().ForEach(f => f.Rollback());
            lv.Items.Refresh();
        }

        private void StringReplaceWindow_Open()
        {
            StringReplaceWindow stringReplaceWindow = new StringReplaceWindow();
            stringReplaceWindow.OK += StringReplaceWindow_OK;
            stringReplaceWindow.Owner = Application.Current.MainWindow; // We must also set the owner for this to work.
            stringReplaceWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            stringReplaceWindow.ShowDialog();
        }

        private void StringReplaceWindow_OK(string from, string to)
        {
            if (!string.IsNullOrEmpty(from))
            {
                FileInfos.ToList().ForEach(f => f.ReplaceName(from, to));
                lv.Items.Refresh();
            }
        }

        private void StringAddWindow_Open(PositionEnum position)
        {
            StringAddWindow stringAddWindow = new StringAddWindow(position);
            stringAddWindow.OK += StringAdd_OK;
            stringAddWindow.Owner = Application.Current.MainWindow; // We must also set the owner for this to work.
            stringAddWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            stringAddWindow.ShowDialog();
        }

        private void StringAdd_OK(PositionEnum position, string adder)
        {
            if (!string.IsNullOrEmpty(adder))
            {
                FileInfos.ToList().ForEach(f => f.AddName(position, adder));
                lv.Items.Refresh();
            }
        }

        private void ZeroPaddingWindow_Open()
        {
            ZeroPaddingWindow zeroPaddingWindow = new ZeroPaddingWindow();
            zeroPaddingWindow.OK += ZeroPaddingWindow_OK;
            zeroPaddingWindow.Owner = Application.Current.MainWindow; // We must also set the owner for this to work.
            zeroPaddingWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            zeroPaddingWindow.ShowDialog();

        }

        private void ZeroPaddingWindow_OK(int length, PositionEnum position)
        {
            FileInfos.ToList().ForEach(f => f.ZeroPadding(position, length));
            lv.Items.Refresh();
        }

        private void AddNumberWindow_Open()
        {
            AddNumberWindow addNumberWindow = new AddNumberWindow();
            addNumberWindow.OK += AddNumberWindow_OK;
            addNumberWindow.Owner = Application.Current.MainWindow; // We must also set the owner for this to work.
            addNumberWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addNumberWindow.ShowDialog();
        }

        private void AddNumberWindow_OK(int start, int length, PositionEnum position)
        {
            for (int i = 0 + start; i < FileInfos.Count + start; i++)
            {
                string zeroPad = "D" + length.ToString();
                string number = i.ToString(zeroPad);

                FileInfo fileInfo = FileInfos[i - start];
                fileInfo.AddName(position, number);
            }
            lv.Items.Refresh();
        }

        private void RemovePositionWindow_Open()
        {
            RemovePositionWindow removePositionWindow = new RemovePositionWindow();
            removePositionWindow.OK += RemovePositionWindow_OK;
            removePositionWindow.Owner = Application.Current.MainWindow; // We must also set the owner for this to work.
            removePositionWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            removePositionWindow.ShowDialog();
        }

        private void RemovePositionWindow_OK(int start, int length)
        {
            if (start > 0)
            {
                FileInfos.ToList().ForEach(f => f.RemoveFileNameWithPosition(start, length));
                lv.Items.Refresh();
            }
        }

        private void lv_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                this.lv.SelectedItems.OfType<FileInfo>().ToList().ForEach(folder =>
                {
                    FileInfos.Remove(folder);
                });
            }
        }



        #region drag and drop

        private void lv_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                e.Effects = System.Windows.DragDropEffects.Copy;
        }
        private void lv_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string str in file)
                {
                    Console.WriteLine(str);
                    FileAttributes attr = File.GetAttributes(str);
                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        // TODO :  폴더인 경우 나중애 추가하고 지금은 하지 말자
                        return;
                    }
                    else
                    {
                        if (FileInfos.Where(f => f.fullPath == str).Count() < 1)
                        {
                            FileInfo fileInfo = new FileInfo()
                            {
                                originalFileName = System.IO.Path.GetFileName(str),
                                targetFileName = System.IO.Path.GetFileName(str),
                                extentionName = System.IO.Path.GetExtension(str),
                                path = System.IO.Path.GetDirectoryName(str),
                                status = StatusEnum.Ready
                            };
                            FileInfos.Add(fileInfo);
                        }

                    }

                }
                FileInfos.OrderBy(f => f.fullPath);
                FileInfos = new ObservableCollection<FileInfo>(FileInfos.OrderBy(f => f.originalFileName));
                Console.WriteLine(FileInfos.Count());
            }
        }


        #endregion


        #region 콜렉션 바인딩 속성
        // http://timokorinth.de/dependency-property-type-collection/
        public ObservableCollection<FileInfo> FileInfos
        {
            get { return (ObservableCollection<FileInfo>)GetValue(FileInfosProperty); }
            set { SetValue(FileInfosProperty, value); }
        }
        public static readonly DependencyProperty FileInfosProperty = DependencyProperty.Register("FileInfos", typeof(ObservableCollection<FileInfo>), typeof(MainWindow), new UIPropertyMetadata(new ObservableCollection<FileInfo>()));

        #endregion



    }
}
