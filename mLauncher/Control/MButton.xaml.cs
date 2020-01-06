using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using mLauncher.Base;

namespace mLauncher.Control
{
    /// <summary>
    /// MButton.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MButton : UserControl
    {
        public MButton()
        {
            InitializeComponent();
            this.DataContext = this;
            this.IsHitTestVisible = true;
        }

        public string TabId { get; set; }
        public int Col { get; set; }
        public int Row { get; set; }
        public string Path { get; set; }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MButton));

        public ImageSource IconImage
        {
            get { return (ImageSource)GetValue(IconImageProperty); }
            set { SetValue(IconImageProperty, value); }
        }
        public static readonly DependencyProperty IconImageProperty =
            DependencyProperty.Register("IconImage", typeof(ImageSource), typeof(MButton), new UIPropertyMetadata(null));

        #region events
        public event RoutedEventHandler Click;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }


        #endregion

        public override string ToString()
        {
            string msg = string.Format("MButton [TabId = {0}, Col = {1}, Row = {2}, Path = {3}]", TabId, Col, Row, Path);
            return base.ToString() + Environment.NewLine + msg;
        }

        private void ButtonSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double width = this.ActualWidth;
            double height = this.ActualHeight;

            if (width > 50 && height > 50)
            {
                textBlockDefinition.Height = new GridLength(20);
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRow(textBlock, 1);
            }
            else
            {
                textBlockDefinition.Height = new GridLength(0);
                textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                Grid.SetRow(textBlock, 0);
                Console.WriteLine("stroke text 구현????????");

            }
        }




    }
}
