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

//BUG Allowed me to put any alphanumeric character into TextBoxes

namespace Monkeyshines
{
    internal class ColorChangedEventArgs : EventArgs
    {
        public Color NewColor { get; set; }
        public Color OldColor { get; set; }
    }

    internal delegate void ColorChangedEventHandler(object sender, ColorChangedEventArgs args);

    public partial class ColorDialog : Window
    {
        private static ValidationRuleHexCode validateHexCode = new ValidationRuleHexCode(6);
        private static ValidationRuleHexCode validateAlphaCode = new ValidationRuleHexCode(2);
        
        private List<MenuItem> staticSwatchMenuItems = new();
        private List<MenuItem> dynamicSwatchMenuItems = new();
        private Swatch? selectedSwatch = null;        

        public ObservableCollection<Swatch> CachedSwatches = new();

        Color currentColor;
        private uint updates = 1;

        //! Constructor that takes in a Brush for Current
        public ColorDialog(SolidColorBrush current)
        {
            List<Swatch> swatches = new();
            
            InitializeComponent();
            
            CachedSwatches.CollectionChanged    += CachedSwatches_CollectionChanged;
            TextBoxHexCode.TextChanged          += TextBoxHexCode_TextChanged;
            TextBoxAlphaHexCode.TextChanged     += TextBoxHexCode_TextChanged;

            if (!CachedSwatches.Any())
            {
                CachedSwatches.Add(new Swatch(Brushes.Black, Brushes.Black) { Width = 24, Height = 24, Margin = new Thickness(5) });
                CachedSwatches.Add(new Swatch(Brushes.White, Brushes.Black) { Width = 24, Height = 24, Margin = new Thickness(5) });
            }

            MenuItem menuItemDeleteSwatch = new MenuItem() { Header = "Delete" };
            {
                menuItemDeleteSwatch.Click += MenuItemDeleteSwatch_Click;
            }

            MenuItem menuItemCopy = new MenuItem() { Header = "Copy to Clipboard" };
            {
                menuItemCopy.Click += MenuItemCopy_Click;
            }

            MenuItem menuItemCache = new() { Header = "Cache" };
            {
                menuItemCache.Click += MenuItemCache_Click;
            }

            staticSwatchMenuItems.Add(menuItemCache);
            staticSwatchMenuItems.Add(menuItemCopy);

            dynamicSwatchMenuItems.Add(menuItemDeleteSwatch);
            dynamicSwatchMenuItems.Add(menuItemCopy);

            swatches.AddRange(CachedSwatches);
            swatches.Add(SwatchCurrent);

            foreach (Swatch swatch in swatches)
            {
                swatch.Selected += Swatch_Selected;
            }

            ColorUpdate(this, current.Color);
        }


        private void SliderValueUpdate(Color color)
        {
            byte max = Math.Max(color.R, Math.Max(color.G, color.B));
            byte brightness = (byte)(max / (ColorSliderRed.Maximum / ColorSliderBrightness.Maximum));

            ColorSliderAlpha.Value = color.A;
            ColorSliderRed.Value = color.R;
            ColorSliderGreen.Value = color.G;
            ColorSliderBlue.Value = color.B;
           // ColorSliderBrightness.Value = brightness;
        }

        private void TextBoxTextUpdate(Color color)
        {
            byte max = Math.Max(color.R, Math.Max(color.G, color.B));
            byte brightness = (byte)(max / (ColorSliderRed.Maximum / ColorSliderBrightness.Maximum));

            TextBoxAlpha.Text = color.A.ToString();
            TextBoxRed.Text = color.R.ToString();
            TextBoxGreen.Text = color.G.ToString();
            TextBoxBlue.Text = color.B.ToString();
            TextBoxBrightness.Text = brightness.ToString();
        }        

        private void ColorUpdate(object updater, Color color)
        {
            TextBox? asTextbox = updater as TextBox;
            //byte max = Math.Max(color.R, Math.Max(color.G, color.B));
            //byte brightness = (byte)(max / (ColorSliderRed.Maximum / ColorSliderBrightness.Maximum));
            string colorCode = color.ToString();
            string alphaCode = colorCode.Substring(1, 2);
            string hexColorCode = colorCode.Substring(3);

            TextBoxHexCode.Text = hexColorCode;
            TextBoxAlphaHexCode.Text = alphaCode;

            ColorSliderAlpha.Resources["ControlBeginColor"] = Color.FromArgb((byte)ColorSliderAlpha.Minimum, color.R, color.G, color.B);
            ColorSliderAlpha.Resources["ControlEndColor"] = Color.FromArgb((byte)ColorSliderAlpha.Maximum, color.R, color.G, color.B);

            ColorSliderRed.Resources["ControlBeginColor"] = Color.FromArgb(color.A, (byte)ColorSliderRed.Minimum, color.G, color.B);
            ColorSliderRed.Resources["ControlEndColor"] = Color.FromArgb(color.A, (byte)ColorSliderRed.Maximum, color.G, color.B);

            ColorSliderGreen.Resources["ControlBeginColor"] = Color.FromArgb(color.A, color.R, (byte)ColorSliderGreen.Minimum, color.B);
            ColorSliderGreen.Resources["ControlEndColor"] = Color.FromArgb(color.A, color.R, (byte)ColorSliderGreen.Maximum, color.B);

            ColorSliderBlue.Resources["ControlBeginColor"] = Color.FromArgb(color.A, color.R, color.G, (byte)ColorSliderBlue.Minimum);
            ColorSliderBlue.Resources["ControlEndColor"] = Color.FromArgb(color.A, color.R, color.G, (byte)ColorSliderBlue.Maximum);

            //ColorSliderBrightness.Resources["ControlBeginColor"] = Color.FromArgb(color.A, (byte)ColorSliderRed.Minimum, (byte)ColorSliderGreen.Minimum, (byte)ColorSliderBlue.Minimum);
            //ColorSliderBrightness.Resources["ControlEndColor"] = Color.FromArgb(color.A, (byte)ColorSliderRed.Value, (byte)ColorSliderGreen.Value, (byte)ColorSliderBlue.Value);

            switch (updater)
            {
                case TextBox asTextBox:
                    SliderValueUpdate(color);
                    break;
                case Slider asSlider:
                    //if (ReferenceEquals(asSlider, ColorSliderBrightness))
                    //{
                    //    SliderValueUpdate(color);
                    //    //ColorSliderBrightness.Value = brightness;
                    //}
                    //else
                    //{
                    //    TextBoxTextUpdate(color);
                    //}
                    TextBoxTextUpdate(color);
                    break;
                default:
                    SliderValueUpdate(color);
                    TextBoxTextUpdate(color);
                    break;
            }

            SwatchCurrent.Fill = new SolidColorBrush(color);
            //Color.
            //if (!ReferenceEquals(updater, ColorSliderBrightness))
            { //! The point of the brightness is to get shades of the same color, so we have to maintain that color in memory
                currentColor = color;
            }
        }
        //! Constructor that takes in a Brush for Current and collection of Brushes for Cached

        public ColorDialog()
            : this(Brushes.White)
        {
            
        }

        //TODO should be a better way of doing the equals stuff, have to look into IEquatable or whatever for these WPF objects
        private void MenuItemCache_Click(object sender, RoutedEventArgs e)
        {
            if(selectedSwatch is not null)
            {
                bool found = false;
                foreach(Swatch swatch in CachedSwatches)
                {
                    if(swatch.ToString().Equals(selectedSwatch.ToString()))
                    {
                        found = true;
                        break;
                    }
                }

                if(!found)
                {
                    CachedSwatches.Add((Swatch) selectedSwatch.Clone());
                }
            }
        }

        private void MenuItemCopy_Click(object sender, RoutedEventArgs e)
        {
            if(selectedSwatch is not null)
            {
                Clipboard.SetText(selectedSwatch.ToString());
            }
        }

        private void MenuItemDeleteSwatch_Click(object sender, RoutedEventArgs e)
        {
            if(selectedSwatch is not null)
            {
                if(CachedSwatches.Contains(selectedSwatch))
                {
                    CachedSwatches.Remove(selectedSwatch);
                }
            }
        }

        private void SelectSwatch(Swatch swatch)
        {
            Swatch? previousSelection = null;

            if (selectedSwatch is null)
            {
                selectedSwatch = swatch;
            }
            else if(ReferenceEquals(selectedSwatch, swatch))
            {
                previousSelection = swatch;             
                selectedSwatch = null;
            }
            else
            {
                previousSelection = selectedSwatch;
                selectedSwatch = swatch;
            }

            if (previousSelection is not null)
            {
                previousSelection.Stroke = Brushes.Black;
                previousSelection.StrokeThickness = 1;
            }
                
            if (selectedSwatch is not null)
            {
                selectedSwatch.Stroke = Brushes.DarkCyan;
                selectedSwatch.StrokeThickness = 2;
            }
                
        }

        //TODO code would be cleaner if subclass swatch and put stuff in that
        private void Swatch_Selected(object? sender, MouseButtonEventArgs mouseEvent)
        {
            Swatch? asSwatch = sender as Swatch;

            if(mouseEvent is not null && asSwatch is not null)
            {
                switch(mouseEvent.ButtonState)
                {
                    case MouseButtonState.Pressed:
                        switch(mouseEvent.ChangedButton)
                        {
                            case MouseButton.Left:
                                SelectSwatch(asSwatch);
                                break;
                            case MouseButton.Right:
                                if(!ReferenceEquals(selectedSwatch, asSwatch))
                                    SelectSwatch(asSwatch);
                                break;
                        }
                        break;
                    case MouseButtonState.Released:
                        switch(mouseEvent.ChangedButton)
                        {
                            case MouseButton.Right:
                                //TODO open up a menu
                                // Current / Shade Swatch (Cache, Copy to Clipboard)
                                // Cached Swatch (Delete, Copy to Clipboard)
                                ContextMenu menu = new() { PlacementTarget = asSwatch };

                                if (CachedSwatches.Contains(asSwatch))
                                {
                                    menu.ItemsSource = dynamicSwatchMenuItems;
                                }
                                else
                                {
                                    menu.ItemsSource = staticSwatchMenuItems;
                                }
                                
                                menu.IsOpen = true;
                                break;
                        }
                        break;
                }
            }
        }

        //TODO updates here reflect to the sliders
        private void TextBoxHexCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TODO need to make sure this was user input, if it wasn't we could have problems
            TextBox asTextBox = (TextBox)sender;
            bool validUpdate = false;
            string currentAlpha = string.IsNullOrEmpty(TextBoxAlphaHexCode.Text) ? "FF" : TextBoxAlphaHexCode.Text;
            string currentColorCode = string.IsNullOrEmpty(TextBoxHexCode.Text) ? "FFFFFF" : TextBoxHexCode.Text;
            
            if(ReferenceEquals(asTextBox, TextBoxAlphaHexCode))
            { // validate the alpha
                validUpdate = validateAlphaCode.Validate(asTextBox.Text, System.Globalization.CultureInfo.CurrentUICulture).IsValid;
            } 
            else if(ReferenceEquals(asTextBox, TextBoxHexCode))
            { // validate the color
                validUpdate = validateHexCode.Validate(asTextBox.Text, System.Globalization.CultureInfo.CurrentUICulture).IsValid;
            }
            
            // apply the new update if it was valid
            if(validUpdate)
            {
                string colorString = $"#{ currentAlpha }{ currentColorCode }";
                //! catch the FormatException?
                SwatchCurrent.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorString));
                SwatchCurrent.UpdateLayout();
            }
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

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            Color modifiedColor = Color.FromArgb((byte) ColorSliderAlpha.Value, (byte) ColorSliderRed.Value, (byte )ColorSliderGreen.Value, (byte) ColorSliderBlue.Value);
            ColorUpdate(this, modifiedColor);
        }
        
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TextBox asTextBox = sender as TextBox;
            //Panel panel = asTextBox.Parent as Panel;

            //if (!asTextBox.Text.Any())
            //{
            //    asTextBox.Text = "0";
            //    //asTextBox.CaretIndex = asTextBox.Text.Length;
            //    //TODO Only do this if this changed from user input
            //    asTextBox.SelectAll();

            //}

            //foreach (object child in panel.Children)
            //{
            //    if(child is Slider asSlider)
            //    {
            //        asSlider.Value = Convert.ToByte(asTextBox.Text);
            //    }
            //}

        }

        //XXX Doesn't like when you select and edit, only working on delete and edit
        private void ColorTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox asTextBox = (TextBox) sender;
            try
            {
                string w = e.ControlText;
                string updatedText = asTextBox.Text.Insert(asTextBox.CaretIndex, e.Text);
                byte converted = Convert.ToByte(updatedText);

                e.Handled = converted > 255 || converted < 0;
            }
            catch (Exception exception)
            {
                if (exception is OverflowException || exception is FormatException)
                    e.Handled = true;
                else
                    throw exception;
            }
        }

        private void ColorSliderBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //double multiplier   = Math.Round((byte) ColorSliderBrightness.Value * 0.01f, 2);
            //byte modifiedRed    = (byte)(currentColor.R * multiplier);
            //byte modifiedGreen  = (byte)(currentColor.G * multiplier);
            //byte modifiedBlue   = (byte)(currentColor.B * multiplier);
            //Color modifiedColor = Color.FromArgb((byte) ColorSliderAlpha.Value, modifiedRed, modifiedGreen, modifiedBlue);
            //// Decrease highest RGB values by the percentage of the brightness
            //ColorUpdate(sender, modifiedColor);

            //// Deactivate CurrentColorSetter
            //// Activate
        }
    }
}
