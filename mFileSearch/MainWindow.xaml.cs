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
        public MainWindow()
        {
            InitializeComponent();

            Folders = new ObservableCollection<TargetFolder>();
            FindingFiles = new ObservableCollection<FileFound>();

            this.DataContext = this;
            

            Folders.Add(new TargetFolder() { Path = "true1", Enable = true });
            Folders.Add(new TargetFolder() { Path = "true4", Enable = true });

            FindingFiles.Add(new FileFound() { File = "a", Line = 1, Text = "aaa" });
            FindingFiles.Add(new FileFound() { File = "b", Line = 1, Text = "bbb" });
            FindingFiles.Add(new FileFound() { File = "a", Line = 2, Text = "aa2" });

            InitControl();

            
        }

        private void InitControl()
        {
            // https://icodebroker.tistory.com/5133
            // https://www.wpf-tutorial.com/listview-control/listview-grouping/

            //ResultListView.ItemsSource = FindingFiles;
            //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ResultListView.ItemsSource);

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(FindingFiles);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("File");
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(groupDescription);


        }

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


        private void Search_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("검색");
            FindingFiles.Add(new FileFound() { File = "c", Line = 2, Text = "ccd" });
            Folders.Add(new TargetFolder() { Path = "false1", Enable = false });
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
