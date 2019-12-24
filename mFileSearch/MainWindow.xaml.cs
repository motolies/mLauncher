using System;
using System.Collections.Generic;
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
            Folders = new List<TargetFolder>();

            this.DataContext = this;
   
        }

        public List<TargetFolder> Folders
        {
            get { return (List<TargetFolder>)GetValue(FoldersProperty); }
            set { SetValue(FoldersProperty, value); }
        }
        public static readonly DependencyProperty FoldersProperty = DependencyProperty.Register("Folders", typeof(List<TargetFolder>), typeof(MainWindow));

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Folders.Add(new TargetFolder() { Path = "true1", Enable = true });
            Folders.Add(new TargetFolder() { Path = "true2", Enable = true });
            Folders.Add(new TargetFolder() { Path = "false3", Enable = false });
            Folders.Add(new TargetFolder() { Path = "false4", Enable = false });
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("검색");
        }
    }

    public class TargetFolder
    {
        public string Path { get; set; }
        public bool Enable { get; set; }
    }
}
