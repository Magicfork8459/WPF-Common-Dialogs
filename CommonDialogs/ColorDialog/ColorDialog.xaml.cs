using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Specialized;

//TODO add support for selecting multiple cached swatches and then mixing them together into one color to set as the current color
using WinColor = System.Windows.Media.Color;

namespace Monkeyshines
{
    public partial class ColorDialog : Window
    {
        private enum ColorSpace
        {
            //! Red, Green, Blue
            RGB,
            //! Hue, Saturation, Brightness/Value
            HSB,
            //! Things not specific to any one color space
            NONE
        }

        private IDisposable? unsubscriber;
        private ObservableColor currentColor;
        private static ValidationRuleHexCode validateHexCode = new ValidationRuleHexCode(6);
        private static ValidationRuleHexCode validateAlphaCode = new ValidationRuleHexCode(2);
        private ColorObserver observerRGB = new();
        private ColorObserver observerHSB = new();
        private List<MenuItem> staticSwatchMenuItems = new();
        private List<MenuItem> dynamicSwatchMenuItems = new();
        private Swatch? selectedSwatch = null;       

        protected ObservableCollection<Swatch> CachedSwatches = new();

        public List<Color> CachedColors { get { return SwatchesToColors(); } }
        
        public ColorDialog(Color current)
        {
            InitializeComponent();
            currentColor = new(current);
            //TODO by default, check this but allow for HSB at construction
            RadioButtonRGB.IsChecked = true;
            List<Swatch> swatches = new();
            
            CachedSwatches.CollectionChanged    += CachedSwatches_CollectionChanged;

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

            MenuItem menuItemSetCurrent = new() { Header = "Set as Current" };
            {
                menuItemSetCurrent.Click += MenuItemSetCurrent_Click;
            }

            staticSwatchMenuItems.Add(menuItemCache);
            staticSwatchMenuItems.Add(menuItemCopy);

            dynamicSwatchMenuItems.Add(menuItemDeleteSwatch);
            dynamicSwatchMenuItems.Add(menuItemCopy);
            dynamicSwatchMenuItems.Add(menuItemSetCurrent);

            swatches.AddRange(CachedSwatches);
            swatches.Add(SwatchCurrent);

            foreach (Swatch swatch in swatches)
            {
                swatch.Selected += Swatch_Selected;
            }

            observerRGB.ColorChanged += ColorChanged_UpdateSwatch;
            observerRGB.ColorChanged += ColorChanged_UpdateHexTextBoxes;
            observerRGB.ColorChanged += ColorChanged_UpdateAlpha;
            observerRGB.ColorChanged += ColorChanged_UpdateRGB;

            observerHSB.ColorChanged += ColorChanged_UpdateSwatch;
            observerHSB.ColorChanged += ColorChanged_UpdateHexTextBoxes;
            observerHSB.ColorChanged += ColorChanged_UpdateAlpha;
            observerHSB.ColorChanged += ColorChanged_UpdateHSB;

            
        }    

        //! Constructor that takes in a Brush for Current and collection of Brushes for Cached

        public ColorDialog()
            : this(Brushes.White.Color)
        {
            
        }

        private List<Color> SwatchesToColors()
        {
            List<Color> colors = new();

            foreach (Swatch swatch in CachedSwatches)
            {
                colors.Add(new Color(swatch.ToString()));
            }

            return colors;
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

        private void MenuItemSetCurrent_Click(object sender, RoutedEventArgs args)
        {
            if(selectedSwatch is not null)
            {
                Color newColor = new Color(selectedSwatch.ToString());

                //ColorChanged?.Invoke(this, new ColorChangedEventArgs() { OldColor = currentColor, NewColor = newColor });
                //currentColor = newColor;
                currentColor.UpdateColor(newColor);
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

        private void CachedSwatches_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            switch(args.Action)
            {
                case NotifyCollectionChangedAction.Add:                    
                    if (args.NewItems is not null)
                    {
                        foreach (var item in args.NewItems)
                        {
                            Swatch? swatch = item as Swatch;

                            if(swatch is not null)
                            {
                                swatch.Stroke = Brushes.Black;
                                swatch.StrokeThickness = 1;
                                PanelCachedSwatches.Children.Add(swatch);
                            }
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

        private void ColorTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox asTextBox = (TextBox) sender;
            try
            {
                string updatedText = asTextBox.Text.Insert(asTextBox.CaretIndex, e.Text);

                if(!string.IsNullOrEmpty(asTextBox.SelectedText) && updatedText.Contains(asTextBox.SelectedText))
                {
                    updatedText = updatedText.Replace(asTextBox.SelectedText, string.Empty);
                }
                    
                short converted = Convert.ToInt16(updatedText);

                Panel? asPanel = asTextBox.Parent as Panel;

                if(asPanel is not null)
                {
                    foreach (var child in asPanel.Children)
                    {
                        Slider? asSlider = child as Slider;

                        if (asSlider is not null)
                        {
                            e.Handled = converted > asSlider.Maximum || converted < asSlider.Minimum;
                            break;
                        }
                    }
                }        
            }
            catch (Exception exception)
            {
                if (exception is OverflowException || exception is FormatException)
                    e.Handled = true;
                else
                    throw exception;
            }
        }

        private void ColorChanged_UpdateRGB(object sender, ColorChangedEventArgs args)
        {
            Color color = args.NewColor;

            ColorSliderRed.ValueChanged -= ColorSliderARGB_ValueChanged;
            TextBoxRed.TextChanged -= TextBoxARGB_TextChanged;
            ColorSliderGreen.ValueChanged -= ColorSliderARGB_ValueChanged;
            TextBoxGreen.TextChanged -= TextBoxARGB_TextChanged;
            ColorSliderBlue.ValueChanged -= ColorSliderARGB_ValueChanged;
            TextBoxBlue.TextChanged -= TextBoxARGB_TextChanged;

            ColorSliderRed.Value = args.NewColor.Red;
            TextBoxRed.Text = args.NewColor.Red.ToString();
            ColorSliderGreen.Value = color.Green;
            TextBoxGreen.Text = color.Green.ToString();
            ColorSliderBlue.Value = color.Blue;
            TextBoxBlue.Text = color.Blue.ToString();

            ColorSliderRed.Resources["ControlBeginColor"] = WinColor.FromArgb(color.Alpha, (byte)ColorSliderRed.Minimum, color.Green, color.Blue);
            ColorSliderRed.Resources["ControlEndColor"] = WinColor.FromArgb(color.Alpha, (byte)ColorSliderRed.Maximum, color.Green, color.Blue);
            ColorSliderGreen.Resources["ControlBeginColor"] = WinColor.FromArgb(color.Alpha, color.Red, (byte)ColorSliderGreen.Minimum, color.Blue);
            ColorSliderGreen.Resources["ControlEndColor"] = WinColor.FromArgb(color.Alpha, color.Red, (byte)ColorSliderGreen.Maximum, color.Blue);
            ColorSliderBlue.Resources["ControlBeginColor"] = WinColor.FromArgb(color.Alpha, color.Red, color.Green, (byte)ColorSliderBlue.Minimum);
            ColorSliderBlue.Resources["ControlEndColor"] = WinColor.FromArgb(color.Alpha, color.Red, color.Green, (byte)ColorSliderBlue.Maximum);

            ColorSliderRed.ValueChanged += ColorSliderARGB_ValueChanged;
            TextBoxRed.TextChanged += TextBoxARGB_TextChanged;
            ColorSliderGreen.ValueChanged += ColorSliderARGB_ValueChanged;
            TextBoxGreen.TextChanged += TextBoxARGB_TextChanged;
            ColorSliderBlue.ValueChanged += ColorSliderARGB_ValueChanged;
            TextBoxBlue.TextChanged += TextBoxARGB_TextChanged;
        }

        private void ColorChanged_UpdateHSB(object sender, ColorChangedEventArgs args)
        {
            Color color = args.NewColor;
            byte saturationAsByte = Convert.ToByte(color.Saturation * 100);
            byte brightnessAsByte = Convert.ToByte(color.Brightness * 100);

            ColorSliderRed.ValueChanged -= ColorSliderHSB_ValueChanged;
            TextBoxRed.TextChanged -= TextBoxHSB_TextChanged;
            ColorSliderGreen.ValueChanged -= ColorSliderHSB_ValueChanged;
            TextBoxGreen.TextChanged -= TextBoxHSB_TextChanged;
            ColorSliderBlue.ValueChanged -= ColorSliderHSB_ValueChanged;
            TextBoxBlue.TextChanged -= TextBoxHSB_TextChanged;

            //! In HSB space the hue slider gradient shouldn't change with the adjustment of the color
            ColorSliderRed.Value = Convert.ToInt16(color.Hue);
            TextBoxRed.Text = Convert.ToInt16(color.Hue).ToString();

            ColorSliderGreen.Value = saturationAsByte;
            TextBoxGreen.Text = saturationAsByte.ToString();
            ColorSliderGreen.Resources["ControlBeginColor"] = WinColor.FromArgb(color.Alpha, Convert.ToByte(255 * color.Brightness), Convert.ToByte(255 * color.Brightness), Convert.ToByte(255 * color.Brightness));
            ColorSliderGreen.Resources["ControlEndColor"] = WinColor.FromArgb(color.Alpha, Convert.ToByte(color.Red * color.Saturation), Convert.ToByte(color.Green * color.Saturation), Convert.ToByte(color.Blue * color.Saturation));

            ColorSliderBlue.Value = brightnessAsByte;
            TextBoxBlue.Text = brightnessAsByte.ToString();
            ColorSliderBlue.Resources["ControlBeginColor"] = WinColor.FromArgb(color.Alpha, 0, 0, 0);
            ColorSliderBlue.Resources["ControlEndColor"] = WinColor.FromArgb(color.Alpha, Convert.ToByte(255 * color.Saturation), Convert.ToByte(255 * color.Saturation), Convert.ToByte(255 * color.Saturation));

            ColorSliderRed.ValueChanged += ColorSliderHSB_ValueChanged;
            TextBoxRed.TextChanged += TextBoxHSB_TextChanged;
            ColorSliderGreen.ValueChanged += ColorSliderHSB_ValueChanged;
            TextBoxGreen.TextChanged += TextBoxHSB_TextChanged;
            ColorSliderBlue.ValueChanged += ColorSliderHSB_ValueChanged;
            TextBoxBlue.TextChanged += TextBoxHSB_TextChanged;
        }

        private void ColorChanged_UpdateSwatch(object sender, ColorChangedEventArgs args)
        {
            SwatchCurrent.Fill = new SolidColorBrush(args.NewColor);
        }

        private void ColorChanged_UpdateAlpha(object sender, ColorChangedEventArgs args)
        {
            Color color = args.NewColor;

            ColorSliderAlpha.ValueChanged -= ColorSliderARGB_ValueChanged;
            TextBoxAlpha.TextChanged -= TextBoxARGB_TextChanged;

            ColorSliderAlpha.Value = color.Alpha;
            TextBoxAlpha.Text = color.Alpha.ToString();

            ColorSliderAlpha.Resources["ControlBeginColor"] = WinColor.FromArgb((byte) ColorSliderAlpha.Minimum, color.Red, color.Green, color.Blue);
            ColorSliderAlpha.Resources["ControlEndColor"] = WinColor.FromArgb((byte) ColorSliderAlpha.Maximum, color.Red, color.Green, color.Blue);
                       
            ColorSliderAlpha.ValueChanged += ColorSliderARGB_ValueChanged;
            TextBoxAlpha.TextChanged += TextBoxARGB_TextChanged;
        }

        private void ColorChanged_UpdateHexTextBoxes(object sender, ColorChangedEventArgs args)
        {
            Color color = args.NewColor;
            //! Pop off the front because it'll have '#'
            string hexCode = color.ToString().Remove(0, 1);

            TextBoxAlphaHexCode.TextChanged -= TextBoxCode_TextChanged;
            TextBoxHexCode.TextChanged -= TextBoxCode_TextChanged;

            TextBoxAlphaHexCode.Text = hexCode.Substring(0, 2);
            TextBoxHexCode.Text = hexCode.Substring(2);

            TextBoxAlphaHexCode.TextChanged += TextBoxCode_TextChanged;
            TextBoxHexCode.TextChanged += TextBoxCode_TextChanged;
        }

        private void ColorSliderARGB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            byte newValueAsByte = (byte) args.NewValue;
            Color newColor = new()
            {
                Alpha = ReferenceEquals(sender, ColorSliderAlpha)   ? newValueAsByte : currentColor.Color.Alpha,
                Red = ReferenceEquals(sender, ColorSliderRed)     ? newValueAsByte : currentColor.Color.Red,
                Green = ReferenceEquals(sender, ColorSliderGreen)   ? newValueAsByte : currentColor.Color.Green,
                Blue = ReferenceEquals(sender, ColorSliderBlue)    ? newValueAsByte : currentColor.Color.Blue
            };

            args.Handled = true;
            currentColor.UpdateColor(newColor);
        }

        private void TextBoxARGB_TextChanged(object sender, TextChangedEventArgs args)
        {
            TextBox? asTextBox = sender as TextBox;

            if(asTextBox is not null)
            {
                string text = asTextBox.Text;
                byte textAsByte;

                if (string.IsNullOrEmpty(text))
                {
                    textAsByte = 0;
                    asTextBox.Text = "0";
                    asTextBox.SelectAll();
                }
                else 
                {
                    textAsByte = Convert.ToByte(text);
                }

                Color newColor = new()
                {
                    Alpha = ReferenceEquals(sender, TextBoxAlpha) ? textAsByte : currentColor.Color.Alpha,
                    Red = ReferenceEquals(sender, TextBoxRed) ? textAsByte : currentColor.Color.Red,
                    Green = ReferenceEquals(sender, TextBoxGreen) ? textAsByte : currentColor.Color.Green,
                    Blue = ReferenceEquals(sender, TextBoxBlue) ? textAsByte : currentColor.Color.Blue
                };

                args.Handled = true;
                currentColor.UpdateColor(newColor);
            }
        }

        private void TextBoxCode_TextChanged(object sender, TextChangedEventArgs args)
        {
            TextBox? asTextBox = sender as TextBox;

            if(asTextBox is not null)
            {
                ValidationRuleHexCode validator = ReferenceEquals(asTextBox, TextBoxAlphaHexCode) ? validateAlphaCode : validateHexCode;
                bool isValid = validator.Validate(asTextBox.Text, System.Globalization.CultureInfo.CurrentCulture).IsValid;

                if(isValid)
                {
                    Color newColor = new Color(TextBoxAlphaHexCode.Text + TextBoxHexCode.Text);

                    currentColor.UpdateColor(newColor);
                }
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            //TODO only return true if there's an actual difference between the opening values and the closing values
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void RadioButtonRGB_Checked(object sender, RoutedEventArgs e)
        {
            GroupBoxTop.Header = "Red";
            GroupBoxMiddle.Header = "Green";
            GroupBoxBottom.Header = "Blue";
            if (unsubscriber is not null) unsubscriber.Dispose();

            ColorSliderRed.ValueChanged -= ColorSliderHSB_ValueChanged;
            ColorSliderGreen.ValueChanged -= ColorSliderHSB_ValueChanged;
            ColorSliderBlue.ValueChanged -= ColorSliderHSB_ValueChanged;
            TextBoxRed.TextChanged -= TextBoxHSB_TextChanged;
            TextBoxGreen.TextChanged -= TextBoxHSB_TextChanged;
            TextBoxBlue.TextChanged -= TextBoxHSB_TextChanged;

            //! Top -> Red            
            ColorSliderRed.Style = (Style) Resources["ColorSlider"];
            ColorSliderRed.Maximum = 255;
            ColorSliderRed.Value = currentColor.Color.Red;
            ColorSliderRed.ValueChanged += ColorSliderARGB_ValueChanged;
            TextBoxRed.TextChanged += TextBoxARGB_TextChanged;

            //! Middle -> Green
            ColorSliderGreen.Style = (Style)Resources["ColorSlider"];
            ColorSliderGreen.Maximum = 255;
            ColorSliderGreen.Value = currentColor.Color.Green;
            ColorSliderGreen.ValueChanged += ColorSliderARGB_ValueChanged;
            TextBoxGreen.TextChanged += TextBoxARGB_TextChanged;

            //! Bottom -> Blue            
            ColorSliderBlue.Style = (Style)Resources["ColorSlider"];
            ColorSliderBlue.Maximum = 255;
            ColorSliderBlue.Value = currentColor.Color.Blue;
            ColorSliderBlue.ValueChanged += ColorSliderARGB_ValueChanged;
            TextBoxBlue.TextChanged += TextBoxARGB_TextChanged;

            unsubscriber = currentColor.Subscribe(observerRGB);
            currentColor.UpdateColor(currentColor.Color);
        }

        private void ColorSliderHSB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            currentColor.UpdateColor(new Color((ushort)ColorSliderRed.Value, (double)(ColorSliderGreen.Value / 100), (double)(ColorSliderBlue.Value / 100)));
        }

        private void TextBoxHSB_TextChanged(object sender, TextChangedEventArgs args)
        {
            TextBox? asTextBox = sender as TextBox;

            if(asTextBox is not null)
            {
                //convert the text to #
                //update corresponding slider by that number
                string text = asTextBox.Text;
                short? textAsShort = null;

                if(string.IsNullOrEmpty(text))
                {

                }
                else
                {
                    textAsShort = Convert.ToInt16(text);
                }

                Panel? asPanel = asTextBox.Parent as Panel;

                if(textAsShort is not null && asPanel is not null)
                {
                    foreach(var child in asPanel.Children)
                    {
                        Slider? asSlider = child as Slider;

                        if(asSlider is not null)
                        {
                            asSlider.Value = (double) textAsShort;
                        }
                    }
                }
            }
        }

        private void RadioButtonHSB_Checked(object sender, RoutedEventArgs e)
        {
            GroupBoxTop.Header = "Hue";
            GroupBoxMiddle.Header = "Saturation";
            GroupBoxBottom.Header = "Brightness";
            if (unsubscriber is not null) unsubscriber.Dispose();

            ColorSliderRed.ValueChanged -= ColorSliderARGB_ValueChanged;
            ColorSliderGreen.ValueChanged -= ColorSliderARGB_ValueChanged;
            ColorSliderBlue.ValueChanged -= ColorSliderARGB_ValueChanged;
            TextBoxRed.TextChanged -= TextBoxARGB_TextChanged;
            TextBoxGreen.TextChanged -= TextBoxARGB_TextChanged;
            TextBoxBlue.TextChanged -= TextBoxARGB_TextChanged;

            //! Top -> Hue
            ColorSliderRed.Style = (Style) Resources["HueSlider"];
            ColorSliderRed.Maximum = 360;
            ColorSliderRed.Value = currentColor.Color.Hue;

            //! Middle -> Saturation
            ColorSliderGreen.Maximum = 100;
            ColorSliderGreen.Value = currentColor.Color.Saturation * 100;

            //! Bottom -> Brightness
            ColorSliderBlue.Maximum = 100;
            ColorSliderBlue.Value = currentColor.Color.Brightness * 100;

            //! Set the new callbacks
            ColorSliderRed.ValueChanged += ColorSliderHSB_ValueChanged;
            ColorSliderGreen.ValueChanged += ColorSliderHSB_ValueChanged;
            ColorSliderBlue.ValueChanged += ColorSliderHSB_ValueChanged;
            TextBoxRed.TextChanged += TextBoxHSB_TextChanged;
            TextBoxGreen.TextChanged += TextBoxHSB_TextChanged;
            TextBoxBlue.TextChanged += TextBoxHSB_TextChanged;

            unsubscriber = currentColor.Subscribe(observerHSB);
            currentColor.UpdateColor(currentColor.Color);
        }
    }
}
