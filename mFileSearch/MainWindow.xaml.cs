using mUT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

        public static ThreadWorker tw = new ThreadWorker();

        public MainWindow()
        {
            InitializeComponent();

            Folders = new ObservableCollection<TargetFolder>();
            FindingFiles = new ObservableCollection<FileFound>();

            this.DataContext = this;


            //Folders.Add(new TargetFolder() { Path = "true1", Enable = true });
            //FindingFiles.Add(new FileFound() { File = "a", Line = 1, Text = "aaa" });

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
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.IsBusy = false;
            }));
        }
        private void Tw_DoWork(object sender, EventArgs e)
        {
            //SearchFile(sender);
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

        #endregion



        private void Search_Click(object sender, RoutedEventArgs e)
        {
            this.IsBusy = true;
            tw.Run();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            tw.Stop();
            this.IsBusy = false;
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
