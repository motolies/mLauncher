using mFileSearch.Base;
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
            this.DataContext = this;
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

        private void SaveSettingsd(object sender, RoutedEventArgs e)
        {
            DataBase.SetExtentionWithout(ExtentionWithout);
            this.Close();
        }

        private void CancelWindows(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
