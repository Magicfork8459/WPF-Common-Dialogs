using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Monkeyshines
{
    internal class ValidationRuleHexCode : ValidationRule
    {
        public int CharacterCount { get; set; } = 6;
        private Regex HexCodeRegex { get; set; }
        //static Regex HexCodeRegex = new Regex(@"^#?([A-F|0-9]{8})$");

        public ValidationRuleHexCode(): base()
        {
            HexCodeRegex = new Regex(@$"^?([A-F|0-9]{{{ CharacterCount }}})$");
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(HexCodeRegex.IsMatch((string)value))
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, "Illegal characters in string");
            }
        }
    }
}
