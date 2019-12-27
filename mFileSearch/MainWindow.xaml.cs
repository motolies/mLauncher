using mEx;
using mUT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

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

        public MainWindow()
        {
            InitializeComponent();

            Folders = new ObservableCollection<TargetFolder>();
            FindingFiles = new ObservableCollection<FileFound>();

            this.DataContext = this;

            Folders.Add(new TargetFolder() { Path = @"D:\Git\mLauncher", Enable = true });

            InitWorker();
            InitControl();
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

        #region thread 관련 함수
        private void Tw_OnCompleted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Send, new Action(delegate
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
            }));

            SearchFile(_fol, _con, _fil, _sub, _reg);

        }
        private void Tw_OnProcessChanged(object sender, ProgressEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Render, new Action(delegate
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
        public static readonly DependencyProperty FindingFilesProperty = DependencyProperty.Register("FindingFiles", typeof(ObservableCollection<FileFound>), typeof(MainWindow));

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
        public static readonly DependencyProperty MatchedCountProperty = DependencyProperty.Register("MatchedCount", typeof(int), typeof(MainWindow));

    
        #endregion



        private void Search_Click(object sender, RoutedEventArgs e)
        {
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

            var withoutFiles = files.Where(ext => !ext.EndWith(new List<String>() { ".dll", ".exe", ".db", ".svn-base", ".pdb", ".cache" }));
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
                            //검색 중간중간에 리스트에 뿌린다
                            Dispatcher.Invoke(DispatcherPriority.Render, new Action(delegate
                            {
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
                        Folders.Add(new TargetFolder() { Path = value, Enable = true });
                }
            }

        }

        #endregion

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
