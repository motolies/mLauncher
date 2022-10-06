using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace mRenamer.Modal
{
    /// <summary>
    /// RemovePositionWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RemovePositionWindow : Window
    {
        public delegate void OkEventHandler(int start, int length);
        public event OkEventHandler OK;

        public RemovePositionWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            OK?.Invoke(Start, Length);
            this.Close();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }


        public int Start
        {
            get { return (int)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }
        public static readonly DependencyProperty StartProperty = DependencyProperty.Register("Start", typeof(int), typeof(RemovePositionWindow), new PropertyMetadata(0));

        public int Length
        {
            get { return (int)GetValue(LengthProperty); }
            set { SetValue(LengthProperty, value); }
        }
        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register("Length", typeof(int), typeof(RemovePositionWindow), new PropertyMetadata(0));





    }
}
