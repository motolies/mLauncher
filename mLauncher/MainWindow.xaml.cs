using mLauncher.Control;
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

namespace mLauncher
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private int tabCount = 1;
        private int colCount = 0;
        private int rowCount = 0;


        public MainWindow()
        {
            InitializeComponent();

            colCount = 8;
            rowCount = 4;
            DrawLauncher();
        }

        private void DrawLauncher()
        {

            for (int t = 0; t < tabCount; t++)
            {
                // tab 선택

                for (int r = 0; r < rowCount; r++)
                {
                    // row 선택
                    TabItem tab = tabControl.Items[0] as TabItem;
                    Grid grid = tab.Content as Grid;

                    grid.RowDefinitions.Add(new RowDefinition()
                    {
                        Height = new GridLength(1, GridUnitType.Star)
                    });

                    for (int c = 0; c < colCount; c++)
                    {

                        if (r == 0)
                        {
                            grid.ColumnDefinitions.Add(new ColumnDefinition()
                            {
                                Width = new GridLength(1, GridUnitType.Star)
                            });
                        }

                        // col 선택 후 버튼 삽입
                        MButton button = new MButton()
                        {
                            Name = string.Format("btn_{0}_{1}_{2}", t, r, c),
                            Tag = string.Format("btn_{0}_{1}_{2}", t, r, c),
                            Text = string.Format("btn_{0}_{1}_{2}", t, r, c)

                        };
                        button.Click += new RoutedEventHandler(Button_Click);

                        Grid.SetRow(button, r);
                        Grid.SetColumn(button, c);

                        grid.Children.Add(button);
                    }
                }




            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MButton button = sender as MButton;
            Console.WriteLine(button.Name);
        }

       
    }
}
