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
        private Regex hexCodeRegex;

        public ValidationRuleHexCode(int characterLimit): base()
        {
            hexCodeRegex = new Regex(@$"^([A-F|0-9]{{{ characterLimit }}})$");
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(hexCodeRegex.IsMatch((string)value))
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
