using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Specialized;

namespace Monkeyshines
{
    internal class ColorChangedEventArgs : EventArgs
    {
        public Color NewColor { get; set; }
        public Color? OldColor { get; set; }
    }

    internal delegate void ColorChangedEventHandler(object sender, ColorChangedEventArgs args);

    public partial class ColorDialog : Window
    {
        private static ValidationRuleHexCode validateHexCode = new ValidationRuleHexCode(6);
        private static ValidationRuleHexCode validateAlphaCode = new ValidationRuleHexCode(2);
        
        private List<MenuItem> staticSwatchMenuItems = new();
        private List<MenuItem> dynamicSwatchMenuItems = new();
        private Swatch? selectedSwatch = null;
        
        //private Dictionary<object, RoutedPropertyChangedEventHandler<double>> sliderValueHandlers;

        protected ObservableCollection<Swatch> CachedSwatches = new();
        public List<Color> CachedColors { get { return SwatchesToColors(); } }

        ColorChangedEventHandler? ColorChanged;
        Color currentColor;
        public Color CurrentColor { get { return currentColor; } }

        public ColorDialog(Color current)
        {
            InitializeComponent();

            currentColor = current;
            List<Swatch> swatches = new();            
            Dictionary<object, ColorChangedEventHandler> colorHandlers = new()
            {
                { ColorSliderAlpha, ColorChanged_UpdateAlpha },
                { ColorSliderRed, ColorChanged_UpdateRed },
                { ColorSliderGreen, ColorChanged_UpdateGreen },
                { ColorSliderBlue, ColorChanged_UpdateBlue },
                { SwatchCurrent, ColorChanged_UpdateSwatch },
                { this, ColorChanged_UpdateHexTextBoxes }
            };

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
            
            foreach(var handler in colorHandlers)
            {
                ColorChanged += handler.Value;
            }

            ColorChanged?.Invoke(this, new ColorChangedEventArgs() { OldColor = null, NewColor = current });
        }    

        //! Constructor that takes in a Brush for Current and collection of Brushes for Cached

        public ColorDialog()
            : this(Brushes.White.Color)
        {
            
        }

        static public Color ColorCodeToColor(string code)
        {
            if (code.First().Equals('#'))
            {
                code = code.Remove(0, 1);
            }

            Color color = new()
            {
                A = Convert.ToByte(code.Substring(0, 2), 16),
                R = Convert.ToByte(code.Substring(2, 2), 16),
                G = Convert.ToByte(code.Substring(4, 2), 16),
                B = Convert.ToByte(code.Substring(6, 2), 16)
            };

            return color;
        }

        private List<Color> SwatchesToColors()
        {
            List<Color> colors = new();

            foreach (Swatch swatch in CachedSwatches)
            {
                colors.Add(ColorCodeToColor(swatch.ToString()));
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
                Color newColor = ColorCodeToColor(selectedSwatch.ToString());
                
                ColorChanged?.Invoke(this, new ColorChangedEventArgs() { OldColor = currentColor, NewColor = newColor });
                currentColor = newColor;
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

        private void ColorChanged_UpdateRed(object sender, ColorChangedEventArgs args)
        {
            Color color = args.NewColor;

            ColorSliderRed.ValueChanged -= ColorSliderARGB_ValueChanged;
            TextBoxRed.TextChanged -= TextBoxARGB_TextChanged;

            ColorSliderRed.Value = args.NewColor.R;
            TextBoxRed.Text = args.NewColor.R.ToString();

            ColorSliderRed.Resources["ControlBeginColor"] = Color.FromArgb(color.A, (byte) ColorSliderRed.Minimum, color.G, color.B);
            ColorSliderRed.Resources["ControlEndColor"] = Color.FromArgb(color.A, (byte)ColorSliderRed.Maximum, color.G, color.B);

            ColorSliderRed.ValueChanged += ColorSliderARGB_ValueChanged;
            TextBoxRed.TextChanged += TextBoxARGB_TextChanged;
        }

        private void ColorChanged_UpdateGreen(object sender, ColorChangedEventArgs args)
        {
            Color color = args.NewColor;

            ColorSliderGreen.ValueChanged -= ColorSliderARGB_ValueChanged;
            TextBoxGreen.TextChanged -= TextBoxARGB_TextChanged;

            ColorSliderGreen.Value = color.G;
            TextBoxGreen.Text = color.G.ToString();

            ColorSliderGreen.Resources["ControlBeginColor"] = Color.FromArgb(color.A, color.R, (byte) ColorSliderGreen.Minimum, color.B);
            ColorSliderGreen.Resources["ControlEndColor"] = Color.FromArgb(color.A, color.R, (byte) ColorSliderGreen.Maximum, color.B);

            ColorSliderGreen.ValueChanged += ColorSliderARGB_ValueChanged;
            TextBoxGreen.TextChanged += TextBoxARGB_TextChanged;
        }
        
        private void ColorChanged_UpdateBlue(object sender, ColorChangedEventArgs args)
        {
            Color color = args.NewColor;

            ColorSliderBlue.ValueChanged -= ColorSliderARGB_ValueChanged;
            TextBoxBlue.TextChanged -= TextBoxARGB_TextChanged;

            ColorSliderBlue.Value = color.B;
            TextBoxBlue.Text = color.B.ToString();

            ColorSliderBlue.Resources["ControlBeginColor"] = Color.FromArgb(color.A, color.R, color.G, (byte) ColorSliderBlue.Minimum);
            ColorSliderBlue.Resources["ControlEndColor"] = Color.FromArgb(color.A, color.R, color.G, (byte) ColorSliderBlue.Maximum);

            ColorSliderBlue.ValueChanged += ColorSliderARGB_ValueChanged;
            TextBoxBlue.TextChanged += TextBoxARGB_TextChanged;
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

            ColorSliderAlpha.Value = color.A;
            TextBoxAlpha.Text = color.A.ToString();

            ColorSliderAlpha.Resources["ControlBeginColor"] = Color.FromArgb((byte) ColorSliderAlpha.Minimum, color.R, color.G, color.B);
            ColorSliderAlpha.Resources["ControlEndColor"] = Color.FromArgb((byte) ColorSliderAlpha.Maximum, color.R, color.G, color.B);
                       
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
                A = ReferenceEquals(sender, ColorSliderAlpha)   ? newValueAsByte : currentColor.A,
                R = ReferenceEquals(sender, ColorSliderRed)     ? newValueAsByte : currentColor.R,
                G = ReferenceEquals(sender, ColorSliderGreen)   ? newValueAsByte : currentColor.G,
                B = ReferenceEquals(sender, ColorSliderBlue)    ? newValueAsByte : currentColor.B
            };

            ColorChanged?.Invoke(sender, new ColorChangedEventArgs()
            {
                OldColor = currentColor,
                NewColor = newColor
            });

            args.Handled = true;
            currentColor = newColor;
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
                    A = ReferenceEquals(sender, TextBoxAlpha) ? textAsByte : currentColor.A,
                    R = ReferenceEquals(sender, TextBoxRed) ? textAsByte : currentColor.R,
                    G = ReferenceEquals(sender, TextBoxGreen) ? textAsByte : currentColor.G,
                    B = ReferenceEquals(sender, TextBoxBlue) ? textAsByte : currentColor.B
                };

                ColorChanged?.Invoke(sender, new ColorChangedEventArgs()
                {
                    OldColor = currentColor,
                    NewColor = newColor
                });

                args.Handled = true;
                currentColor = newColor;
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
                    Color newColor = ColorCodeToColor(TextBoxAlphaHexCode.Text + TextBoxHexCode.Text);
                    ColorChanged?.Invoke(sender, new ColorChangedEventArgs()
                    {
                        OldColor = currentColor,
                        NewColor = newColor
                    });

                    currentColor = newColor;
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
    }
}
