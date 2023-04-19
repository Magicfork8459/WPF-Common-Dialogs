using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Collections.Specialized;

//TODO
// When CurrentSwatch gets set
// Set Lightest, Lighter, Darker, Darkest swatches
// Tooltips
// Hex Codes over every swatch

namespace Monkeyshines
{
    public partial class ColorDialog : Window
    {
        private static ValidationRuleHexCode validateHexCode = new ValidationRuleHexCode();
        //! Default White

        //! Black and White by default
        public ObservableCollection<Swatch> CachedSwatches;
        public string HexCode { get; set; }

        //! Constructor that takes in a Brush for Current
        //! Constructor that takes in a Brush for Current and collection of Brushes for Cached

        public ColorDialog()
        {
            InitializeComponent();

            CachedSwatches = new();

            CachedSwatches.CollectionChanged += CachedSwatches_CollectionChanged;
            CachedSwatches.Add(new Swatch(Brushes.Black, Brushes.Black) { Width = 24, Height = 24, Margin = new Thickness(5) });
            CachedSwatches.Add(new Swatch(Brushes.White, Brushes.Black) { Width = 24, Height = 24, Margin = new Thickness(5) });

            TextBoxHexCode.TextChanged += TextBoxHexCode_TextChanged;
            
        }

        private void TextBoxHexCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TODO need to make sure this was user input, if it wasn't we could have problems
            
            TextBox asTextBox = (TextBox) sender;
            if(validateHexCode.Validate(asTextBox.Text, System.Globalization.CultureInfo.CurrentUICulture).IsValid)
            {
                SwatchCurrent.Fill = new SolidColorBrush((Color) ColorConverter.ConvertFromString(asTextBox.Text));
                SwatchCurrent.UpdateLayout();
            }
            else
            {
                //asTextBox.BorderBrush = Brushes.Red;
                //TODO some sort of feedback to the user
            }

            //TODO if the first character isn't #, then show one in front of the text?
        }

        private void CachedSwatches_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            switch(args.Action)
            {
                case NotifyCollectionChangedAction.Add:                    
                    if (args.NewItems is not null)
                    {
                        foreach (var item in args.NewItems)
                        {
                            PanelCachedSwatches.Children.Add((Swatch) item);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (args.OldItems is not null)
                    {
                        foreach(var item in args.OldItems)
                        {
                            PanelCachedSwatches.Children.Remove((Swatch) item);
                        }
                    }
                    break;
                    // Replace
                    // Move
                    // Reset
            }
        }

        public void SubInitialize()
        {

        }
    }
}
