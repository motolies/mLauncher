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
using System.Windows.Shapes;

namespace mRenamer.Modal
{
    /// <summary>
    /// StringReplace.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StringReplaceWindow : Window
    {
        public delegate void OkEventHandler(string from, string to);
        public event OkEventHandler OK;

        public StringReplaceWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            OK?.Invoke(From, To);
            this.Close();
        }

        public string From
        {
            get { return (string)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(string), typeof(StringReplaceWindow), new PropertyMetadata(""));

        public string To
        {
            get { return (string)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(string), typeof(StringReplaceWindow), new PropertyMetadata(""));

    }
}
