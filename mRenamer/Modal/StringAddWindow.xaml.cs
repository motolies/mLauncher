using mRenamer.Model;
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
    /// FrontAdd.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StringAddWindow : Window
    {
        public delegate void OkEventHandler(PositionEnum position, string adder);
        public event OkEventHandler OK;

        private PositionEnum Position { get; set; }

        public StringAddWindow(PositionEnum position)
        {
            this.Position = position;

            if (Position == PositionEnum.PreFix)
                Description = "파일명 앞에 지정한 문자열을 붙여줍니다.";
            else
                Description = "파일명 뒤에 지정한 문자열을 붙여줍니다.";

            InitializeComponent();
            this.DataContext = this;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            OK?.Invoke(Position, Adder);
            this.Close();
        }

        public string Adder
        {
            get { return (string)GetValue(AdderProperty); }
            set { SetValue(AdderProperty, value); }
        }
        public static readonly DependencyProperty AdderProperty = DependencyProperty.Register("Adder", typeof(string), typeof(StringAddWindow), new PropertyMetadata(""));

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(StringAddWindow), new PropertyMetadata(""));

    }
}
