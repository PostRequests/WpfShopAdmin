// PhoneUtil Ч нормализует телефон к виду 8XXXXXXXXXX.
using System.Text.RegularExpressions;

namespace ConsoleShop
{
    public static class PhoneUtil
    {
        public static string? normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            string digits = Regex.Replace(input, @"\D", "");

            if (digits.Length == 11 && (digits[0] == '7' || digits[0] == '8'))
                return "8" + digits.Substring(digits.Length - 10, 10);

            if (digits.Length == 10 && digits[0] == '9')
                return "8" + digits;

            return null;
        }
    }
}
