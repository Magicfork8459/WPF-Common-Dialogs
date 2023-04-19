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

//TODO
// Copy/Paste grabs hex code when selected
// FillChanged event
//

namespace Monkeyshines
{
    //! Putting it in the stackpanel broke the rendering
    public partial class Swatch : UserControl
    {
        private Brush fill = Brushes.White;
        public Brush Fill { 
            get
            {
                return fill;
            }
            set 
            {
                fill = value;
                RectangleSwatch.Fill = fill;

                if(ToolTipEnabled)
                {
                    ToolTip = Fill.ToString();
                }
            } 
        }

        private Brush stroke = Brushes.Black;
        public Brush Stroke 
        { 
            get 
            {
                return stroke;
            } 
            set 
            {
                stroke = value;
                RectangleSwatch.Stroke = stroke;
            } 
        }
        private double strokeThickness = 0;
        public double StrokeThickness
        {
            get 
            {
                return strokeThickness;
            }
            set
            {
                strokeThickness = value;
                RectangleSwatch.StrokeThickness = strokeThickness;
            }
        }
        public string HexCode 
        { 
            get 
            {
                return Fill.ToString();
            } 
        }
        public bool ToolTipEnabled { get; set; } = true;
        public Swatch()
        {
            InitializeComponent();

            Fill = Brushes.White;
            Stroke = Brushes.Black;
        }
        public Swatch(SolidColorBrush brush)
        {
            InitializeComponent();
            
            Fill = brush;
        }
        public Swatch(SolidColorBrush brush, Brush stroke) : this(brush)
        {
            Stroke = stroke;

            if (StrokeThickness == 0) StrokeThickness = 1;
        }        
    }
}
