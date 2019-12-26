using mUT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mFileSearch
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {

        static ThreadWorker tw = new ThreadWorker();

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
        }

        #region thread 관련 함수
        private void Tw_OnCompleted(object sender, EventArgs e)
        {
            ControlState(false);
        }
        private void Tw_DoWork(object sender, EventArgs e)
        {
            //SearchFile(sender);
        }
        private void Tw_OnProcessChanged(object sender, ProgressEventArgs e)
        {
            //ChangePercentDele(e.Percent);
        }
        #endregion

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

        #endregion



        private void Search_Click(object sender, RoutedEventArgs e)
        {
            BeforeSearch();
            tw.Run();
        }

        private void BeforeSearch()
        {
            //isStop = false;
            //listView.Items.Clear();
            //listView.Groups.Clear();
            //MatchedCount = 0;
            //txtTotalCount.Text = string.Format("총 {0}개 검색", MatchedCount);
            //groupList.Clear();
            //ControlState(true);

            //if (!cbCondition.Items.Contains(cbCondition.Text))
            //    cbCondition.Items.Add(cbCondition.Text);

        }

        private void ControlState(bool stat)
        {
            // 콘트롤 설정
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //InitControl();
            
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
