using mFileSearch.Base;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;

namespace mFileSearch
{
    /// <summary>
    /// SettingWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
            ExtentionWithout = DataBase.GetExtentionWithout();
            ExplorerContext = GetContextFolder();
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.chkExplorerMenu.Checked += new RoutedEventHandler(ExplorerContext_Checked);
            this.chkExplorerMenu.Unchecked += new RoutedEventHandler(ExplorerContext_UnChecked);
        }

        public static bool GetContextFolder()
        {
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"Directory\shell\mFileSearch");
            return key != null;
        }

        private void ConditionReset_Click(object sender, RoutedEventArgs e)
        {
            DataBase.ResetCondition();
        }

        private void FilterReset_Click(object sender, RoutedEventArgs e)
        {
            DataBase.ResetFilter();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (Owner as MainWindow).InitData();
        }

        public DataTable ExtentionWithout
        {
            get { return (DataTable)GetValue(ExtentionWithoutProperty); }
            set { SetValue(ExtentionWithoutProperty, value); }
        }
        public static readonly DependencyProperty ExtentionWithoutProperty = DependencyProperty.Register("ExtentionWithout", typeof(DataTable), typeof(SettingWindow));

        public bool ExplorerContext
        {
            get { return (bool)GetValue(ExplorerContextProperty); }
            set { SetValue(ExplorerContextProperty, value); }
        }
        public static readonly DependencyProperty ExplorerContextProperty = DependencyProperty.Register("ExplorerContext", typeof(bool), typeof(SettingWindow));

        private void SaveSettingsd(object sender, RoutedEventArgs e)
        {
            DataBase.SetExtentionWithout(ExtentionWithout);
            this.Close();
        }

        private void CancelWindows(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExplorerContext_Checked(object sender, RoutedEventArgs e)
        {
            string result = string.Empty;
            MessageBoxResult dialog = MessageBox.Show("탐색기 메뉴를 레지스트리에 등록하기 위해서는 관리자 권한이 필요합니다. 계속 진행하시겠습니까?", "권한요청", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dialog == MessageBoxResult.Yes)
            {
                string param = string.Format("cmd=mfilesearch_folder_install path=\"{0}\"", System.Reflection.Assembly.GetExecutingAssembly().Location);
                RegistryHelper.RunRunasRegistry(param, out result);
            }
        }

        private void ExplorerContext_UnChecked(object sender, RoutedEventArgs e)
        {
            string result = string.Empty;
            MessageBoxResult dialog = MessageBox.Show("탐색기 메뉴를 레지스트리에서 삭제하기 위해서는 관리자 권한이 필요합니다. 계속 진행하시겠습니까?", "권한요청", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dialog == MessageBoxResult.Yes)
            {
                RegistryHelper.RunRunasRegistry("cmd=mfilesearch_folder_uninstall", out result);
            }
        }

   
    }
}
