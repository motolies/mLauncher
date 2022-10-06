using mRenamer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Runtime.Remoting.Contexts;

namespace mRenamer.Modal
{
    /// <summary>
    /// AddNumberWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AddNumberWindow : Window
    {
        public delegate void OkEventHandler(int start, int length, PositionEnum position);
        public event OkEventHandler OK;

        public AddNumberWindow()
        {

            this.Statuses = new List<PositionEnum>
            {
                PositionEnum.PreFix,
                PositionEnum.PostFix
            };
            this.SelectedStatus = PositionEnum.PostFix;

            InitializeComponent();
            this.DataContext = this;
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
        public static readonly DependencyProperty StartProperty = DependencyProperty.Register("Start", typeof(int), typeof(AddNumberWindow), new PropertyMetadata(1));

        public int Length
        {
            get { return (int)GetValue(LengthProperty); }
            set { SetValue(LengthProperty, value); }
        }
        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register("Length", typeof(int), typeof(AddNumberWindow), new PropertyMetadata(4));

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            OK?.Invoke(Start, Length, (PositionEnum)cbPosition.SelectedItem);
            this.Close();
        }

        public List<PositionEnum> Statuses
        {
            get;
            set;
        }

        public PositionEnum SelectedStatus
        {
            get;
            set;
        }





    }
}
