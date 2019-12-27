using mEx;
using mFileSearch.Base;
using Microsoft.WindowsAPICodePack.Dialogs;
using mUT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace mFileSearch
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        object syncMatchCount = new object();
        object syncLineNumber = new object();
        public static ThreadWorker tw = new ThreadWorker();
        private bool ThreadStop = false;
        private List<string> ExtentionWithout = new List<string>();


        public MainWindow()
        {
            InitializeComponent();
            DataBase.InitDB();
            //FindingFiles = new ObservableCollection<FileFound>();

            this.DataContext = this;

            //Folders.Add(new TargetFolder() { Path = @"D:\Git\mLauncher", Enable = true });

            InitWorker();
            InitControl();
            InitData();
        }

        #region init
        public void InitData()
        {
            IList<string> conditions = DataBase.GetConditions().AsEnumerable().Select(r => r.Field<string>("Id")).ToList();
            Conditions = new ObservableCollection<string>(conditions);

            IList<string> filters = DataBase.GetFilters().AsEnumerable().Select(r => r.Field<string>("Id")).ToList();
            Filters = new ObservableCollection<string>(filters);

            IList<TargetFolder> folders = DataBase.GetFolders().AsEnumerable().Select(r =>
                new TargetFolder()
                {
                    Path = r.Field<string>("Path"),
                    Enable = r.Field<Int64>("IsEnable") == 1 ? true : false
                }
            ).ToList();
            Folders = new ObservableCollection<TargetFolder>(folders);

            ExtentionWithout = DataBase.GetExtentionWithout().AsEnumerable().Select(r => r.Field<string>("Id")).ToList();

        }

        private void InitWorker()
        {
            tw.DoWork += Tw_DoWork;
            tw.OnCompleted += Tw_OnCompleted;
            tw.OnProcessChanged += Tw_OnProcessChanged;
        }

        private void InitControl()
        {
            // https://icodebroker.tistory.com/5133
            // https://www.wpf-tutorial.com/listview-control/listview-grouping/

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(FindingFiles);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("File");
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(groupDescription);

            this.IsSubFolder = true;
        }

        #endregion

        #region thread 관련 함수
        private void Tw_OnCompleted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.IsBusy = false;
            }));
        }
        private void Tw_DoWork(object sender, EventArgs e)
        {
            IList<TargetFolder> _fol = new List<TargetFolder>();
            string _con = string.Empty, _fil = string.Empty;
            bool _sub = true, _reg = false;

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                _fol = Folders;
                _con = cboCondition.Text;
                _fil = cboFilter.Text;
                _sub = IsSubFolder;
                _reg = IsRegex;

                DataBase.SetCondition(_con);
                DataBase.SetFilter(_fil);
                IList<string> conditions = DataBase.GetConditions().AsEnumerable().Select(r => r.Field<string>("Id")).ToList();
                Conditions = new ObservableCollection<string>(conditions);
                IList<string> filters = DataBase.GetFilters().AsEnumerable().Select(r => r.Field<string>("Id")).ToList();
                Filters = new ObservableCollection<string>(filters);
            }));

            SearchFile(_fol, _con, _fil, _sub, _reg);

        }
        private void Tw_OnProcessChanged(object sender, ProgressEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ChangePercent(e.Percent);
            }));

        }
        #endregion

        private void ChangePercent(float per)
        {
            Percent = (per * 100).ToString("#,###.00") + "%";
        }

        #region 속성

        public ObservableCollection<string> Conditions
        {
            get { return (ObservableCollection<string>)GetValue(ConditionsProperty); }
            set { SetValue(ConditionsProperty, value); }
        }
        public static readonly DependencyProperty ConditionsProperty = DependencyProperty.Register("Conditions", typeof(ObservableCollection<string>), typeof(MainWindow), new PropertyMetadata(new ObservableCollection<string>()));

        public ObservableCollection<string> Filters
        {
            get { return (ObservableCollection<string>)GetValue(FiltersProperty); }
            set { SetValue(FiltersProperty, value); }
        }
        public static readonly DependencyProperty FiltersProperty = DependencyProperty.Register("Filters", typeof(ObservableCollection<string>), typeof(MainWindow), new PropertyMetadata(new ObservableCollection<string>()));


        public ObservableCollection<TargetFolder> Folders
        {
            get { return (ObservableCollection<TargetFolder>)GetValue(FoldersProperty); }
            set { SetValue(FoldersProperty, value); }
        }
        public static readonly DependencyProperty FoldersProperty = DependencyProperty.Register("Folders", typeof(ObservableCollection<TargetFolder>), typeof(MainWindow));

        public ObservableCollection<FileFound> FindingFiles
        {
            get { return (ObservableCollection<FileFound>)GetValue(FindingFilesProperty); }
            set { SetValue(FindingFilesProperty, value); }
        }
        public static readonly DependencyProperty FindingFilesProperty = DependencyProperty.Register("FindingFiles", typeof(ObservableCollection<FileFound>), typeof(MainWindow), new PropertyMetadata(new ObservableCollection<FileFound>()));

        public bool IsSubFolder
        {
            get { return (bool)GetValue(IsSubFolderProperty); }
            set { SetValue(IsSubFolderProperty, value); }
        }
        public static readonly DependencyProperty IsSubFolderProperty = DependencyProperty.Register("IsSubFolder", typeof(bool), typeof(MainWindow));

        public bool IsRegex
        {
            get { return (bool)GetValue(IsRegexProperty); }
            set { SetValue(IsRegexProperty, value); }
        }
        public static readonly DependencyProperty IsRegexProperty = DependencyProperty.Register("IsRegex", typeof(bool), typeof(MainWindow));

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }
        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register("IsBusy", typeof(bool), typeof(MainWindow));

        public string Percent
        {
            get { return (string)GetValue(PercentProperty); }
            set { SetValue(PercentProperty, value); }
        }
        public static readonly DependencyProperty PercentProperty = DependencyProperty.Register("Percent", typeof(string), typeof(MainWindow));

        public int MatchedCount
        {
            get { return (int)GetValue(MatchedCountProperty); }
            set { SetValue(MatchedCountProperty, value); }
        }
        public static readonly DependencyProperty MatchedCountProperty = DependencyProperty.Register("MatchedCount", typeof(int), typeof(MainWindow), new PropertyMetadata(0));


        #endregion

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboCondition.Text) || string.IsNullOrWhiteSpace(cboFilter.Text))
            {
                MessageBox.Show(this, "검색어와 필터는 모두 있어야 합니다.");
                return;
            }

            FindingFiles.Clear();
            MatchedCount = 0;
            this.IsBusy = true;
            ThreadStop = !this.IsBusy;
            tw.Run();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {

            ThreadStop = true;
            tw.Stop();
            this.IsBusy = false;
        }


        #region 파일검색

        public void SearchFile(IList<TargetFolder> folders, string condition, string fileFilter, bool isSubFolder, bool isRegex)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<string> files = new List<string>();
            string searchTerm = condition.ToLower();
            string[] filter = fileFilter.Trim().ToLower().Replace("*.*", string.Empty).Replace("*", string.Empty).Split(';');

            foreach (string folder in folders.Where(f => f.Enable == true).Select(f => f.Path).ToList())
            {
                SearchOption so = isSubFolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                files.AddRange(Directory.GetFiles(folder, "*.*", so));
            }

            var withoutFiles = files.Where(ext => !ext.EndWith(ExtentionWithout));
            var fileList = withoutFiles.Where(ext => ext.ToLower().EndWith(filter));

            int totalFile = fileList.Count();
            int curFile = 0;
            Parallel.ForEach(fileList, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (file, stateFile) =>
            {
                if (ThreadStop)
                    stateFile.Stop();

                int lineNumber = 1;

                Encoding encoding = FileIO.GetFileEncoding(file);
                if (encoding == null)
                    return;

                foreach (string lineString in File.ReadLines(file, encoding))
                {
                    if (ThreadStop)
                        break;

                    if ((isRegex && Regex.IsMatch(lineString, searchTerm, RegexOptions.IgnoreCase)) || lineString.ToLower().Contains(searchTerm))
                    {
                        Dispatcher.Invoke(DispatcherPriority.Render, new Action(delegate
                        {
                            MatchedCount++;
                        }));

                        lock (syncLineNumber)
                        {
                            Dispatcher.Invoke(DispatcherPriority.Render, new Action(delegate
                            {
                                //검색 중간중간에 리스트에 뿌린다
                                FindingFiles.Add(new FileFound() { File = file, Line = lineNumber, Text = lineString });
                            }));
                        }
                    }
                    lineNumber++;
                }
                curFile++;
                if (curFile % 10 == 0)
                    tw.ReportProgress((float)curFile / (float)totalFile);

            });
            tw.ReportProgress((float)1);
            sw.Stop();
            Console.WriteLine("검색종료 : " + sw.ElapsedMilliseconds);
        }


        #endregion

        #region drag and drop

        private void lvFolder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                e.Effects = System.Windows.DragDropEffects.Copy;
        }
        private void lvFolder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string str in file)
                {
                    FileAttributes attr = File.GetAttributes(str);
                    string value = string.Empty;
                    if (attr.HasFlag(FileAttributes.Directory))
                        value = str;
                    else
                        value = System.IO.Path.GetDirectoryName(str);

                    if (Folders.Where(f => f.Path == value).Count() < 1)
                    {
                        TargetFolder folder = new TargetFolder() { Path = value, Enable = true };
                        Folders.Add(folder);
                        DataBase.SetFolder(folder);
                    }
                }
            }
        }

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (TargetFolder folder in Folders)
            {
                DataBase.SetFolderEnable(folder);
            }
        }

        private void LvFolder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                this.lvFolder.SelectedItems.OfType<TargetFolder>().ToList().ForEach(folder =>
                {
                    Folders.Remove(folder);
                    DataBase.DeleteFolder(folder);
                });
            }
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow setting = new SettingWindow();
            setting.Owner = this;
            setting.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            setting.ShowDialog();
        }

        private void FolderAdd_Click(object sender, RoutedEventArgs e)
        {
            // https://phantom00.tistory.com/101
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            //dialog.InitialDirectory = "C:\\Users";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Folders.Add(new TargetFolder()
                {
                    Path = dialog.FileName,
                    Enable = true
                });
            }
        }
    }

    public class TargetFolder
    {
        public string Path { get; set; }
        public bool Enable { get; set; }
    }

    public class FileFound
    {
        public string File { get; set; }
        public int Line { get; set; }
        public string Text { get; set; }
    }



}
