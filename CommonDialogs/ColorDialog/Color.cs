using System;
using System.Windows.Media;

using WinColor = System.Windows.Media.Color;

namespace Monkeyshines
{
    public class ColorChangedEventArgs : EventArgs
    {
        public Color NewColor { get; set; }
        public Color? OldColor { get; set; }
    }

    public delegate void ColorChangedEventHandler(object sender, ColorChangedEventArgs args);

    public record struct TupleRGB(byte red, byte green, byte blue);
    public record struct TupleHSB(ushort hue, double saturation, double brightness);
    public class Color
    {
        //TODO When one of these is set, adjust the others appropriately
        private byte red = 255;        
        private byte green = 255;
        private byte blue = 255;
        private ushort hue = 0;
        private double saturation = 0;
        private double brightness = 0;
        public byte Red
        {
            get { return red; }
            set 
            { 
                if(value != red)
                {
                    red = value;
                    RGBUpdate();
                }                
            }
        }
        
        public byte Green 
        { 
            get { return green; } 
            set 
            {
                if(value != green)
                {
                    green = value;
                    RGBUpdate();
                }
            } 
        }
        
        public byte Blue
        {
            get { return blue; }
            set 
            {
                if(value != blue)
                {
                    blue = value;
                    RGBUpdate();
                }
            }
        }
        public byte Alpha { get; set; } = 255;
        public ushort Hue 
        { 
            get { return hue; } 
            set 
            {
                if(value != hue)
                {
                    hue = value;
                    HSBUpdate();
                }
            } 
        }
        public double Saturation 
        {
            get { return saturation; }
            set
            {
                if(value != saturation)
                {
                    saturation = value;
                    HSBUpdate();
                }
            }
        }
        public double Brightness
        {
            get { return brightness; }
            set
            {
                if (value != brightness)
                {
                    brightness = value;
                    HSBUpdate();
                }
            }
        }

        //public ColorChangedEventHandler? ColorChanged;

        public Color()
        {

        }

        public Color(Color that)
        {
            red = that.Red;
            green = that.Green;
            blue = that.Blue;
            hue = that.Hue;
            saturation = that.Saturation;
            brightness = that.Brightness;
        }

        public Color(byte alpha, byte red, byte green, byte blue)
        {
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;
        }

        public Color(byte red, byte green, byte blue)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;

            RGBToHSB(new TupleRGB(Red, Green, Blue), out TupleHSB hsb);

            hue = hsb.hue;
            saturation = hsb.saturation;
            brightness = hsb.brightness;
        }

        public Color(ushort hue, double saturation, double brightness)
        {
            this.hue = hue;
            this.saturation = saturation;
            this.brightness = brightness;

            HSBToRGB(new TupleHSB(Hue, Saturation, Brightness), out TupleRGB rgb);

            red = rgb.red;
            green = rgb.green;
            blue = rgb.blue;
        }

        public Color(string hexCode)
            : this()
        {
            if(string.IsNullOrEmpty(hexCode))
            {
                // throw bad argument exception
            }
            if (hexCode[0].Equals('#'))
            {
                hexCode = hexCode.Remove(0, 1);
            }

            if(hexCode.Length >= 8)
            {
                Alpha = Convert.ToByte(hexCode.Substring(0, 2), 16);
            }
            
            Red = Convert.ToByte(hexCode.Substring(2, 2), 16);
            Green = Convert.ToByte(hexCode.Substring(4, 2), 16);
            Blue = Convert.ToByte(hexCode.Substring(6, 2), 16);            
        }

        public override string ToString()
        {
            return ARGBHexCode();
        }

        public string ARGBHexCode()
        {
            return $"#{Alpha.ToString("X2")}{Red.ToString("X2")}{Green.ToString("X2")}{Blue.ToString("X2")}";
        }

        public string RGBHexCode()
        {
            return $"#{Red.ToString("X2")}{Green.ToString("X2")}{Blue.ToString("X2")}";
        }

        private void RGBUpdate()
        {

            RGBToHSB(new TupleRGB(Red, Green, Blue), out TupleHSB hsb);

            hue = hsb.hue;
            saturation = hsb.saturation;
            brightness = hsb.brightness;

            Console.WriteLine($"Red: {Red} Green {Green} Blue {Blue}");
        }

        private void HSBUpdate()
        {
            HSBToRGB(new TupleHSB(Hue, Saturation, Brightness), out TupleRGB rgb);

            red = rgb.red;
            green = rgb.green;
            blue = rgb.blue;

            Console.WriteLine($"Hue: {Hue} Saturation {Saturation} Brightness {Brightness}");
        }

        public static implicit operator WinColor(Color color)
        {
            return new WinColor() { A = color.Alpha, R = color.Red, G = color.Green, B = color.Blue};
        }

        public static implicit operator Color(WinColor color)
        {
            return new Color(color.A, color.R, color.G, color.B);
        }

        public static void HSBToRGB(in TupleHSB hsb, out TupleRGB rgb)
        {
            double hue = Convert.ToDouble(hsb.hue);
            double s = hsb.saturation;
            double b = hsb.brightness;
            double c = b * s;
            double x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
            double m = b - c;
            double rPrime, gPrime, bPrime;
            {
                rPrime = gPrime = bPrime = 0;
            }

            switch (hue)
            {
                case double n when (n >= 0 && n < 60):
                    rPrime = c;
                    gPrime = x;
                    bPrime = 0;
                    break;
                case double n when (n >= 60 && n < 120):
                    rPrime = x;
                    gPrime = c;
                    bPrime = 0;
                    break;
                case double n when (n >= 120 && n < 180):
                    rPrime = 0;
                    gPrime = c;
                    bPrime = x;
                    break;
                case double n when (n >= 180 && n < 240):
                    rPrime = 0;
                    gPrime = x;
                    bPrime = c;
                    break;
                case double n when (n >= 240 && n < 300):
                    rPrime = x;
                    gPrime = 0;
                    bPrime = c;
                    break;
                case double n when (n >= 300 && n <= 360):
                    rPrime = c;
                    gPrime = 0;
                    bPrime = x;
                    break;
            }

            rgb = new
                (
                    (byte)((rPrime + m) * 255),
                    (byte)((gPrime + m) * 255),
                    (byte)((bPrime + m) * 255)
                );
        }

        public static void RGBToHSB(in TupleRGB rgb, out TupleHSB hsb)
        {
            byte red = rgb.red;
            byte green = rgb.green;
            byte blue = rgb.blue;
            double max = Math.Max(red, Math.Max(green, blue));
            double min = Math.Min(red, Math.Min(green, blue));
            double brightness = max / 255;
            double saturation = max > 0 ? 1 - (min / max) : 0;
            double preSqrt = Math.Pow(red, 2) + Math.Pow(green, 2) + Math.Pow(blue, 2) - (red * green) - (red * blue) - (green * blue);
            double pre = (red - (green * 0.5) - (blue * 0.5)) / Math.Sqrt(preSqrt);
            double huePrime = Math.Acos(pre);
            {
                huePrime = huePrime * 180 / Math.PI;
            }
            double hue;

            if(green >= blue)
            {
                hue = huePrime;
            }
            else
            {
                hue = 360 - huePrime;
            }            

            hsb = new
                (
                    (ushort)(double.IsNaN(hue) ? 0 : hue),
                    saturation,
                    brightness
                );
        }
    }
}
