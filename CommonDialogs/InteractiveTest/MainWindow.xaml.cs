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

using Monkeyshines;

namespace InteractiveTest
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonOpenColorDialog_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush? asSolidColorBrush = SwatchCurrent.Fill as SolidColorBrush;

            if(asSolidColorBrush is not null)
            {
                ColorDialog dialog = new ColorDialog(asSolidColorBrush.Color);
                bool? result = dialog.ShowDialog();

                if(result is not null && result is true)
                {
                    SwatchCurrent.Fill = new SolidColorBrush(dialog.CurrentColor);

                    foreach(Color color in dialog.CachedColors)
                    {
                        StackPanelSwatchesCached.Children.Add(new Swatch() { Fill = new SolidColorBrush(color), Height = 24, Width = 24, Margin = new Thickness(5) });
                    }
                }
            }            
        }

        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            SwatchCurrent.Fill = new SolidColorBrush(ColorDialog.ColorCodeToColor(TextBoxCode.Text));
        }
    }
}
