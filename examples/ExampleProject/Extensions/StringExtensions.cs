using System.Text.RegularExpressions;

namespace ExampleProject.Extensions
{
    public static class StringExtensions
    {
        public static bool IsValidEmailAddress(this string original)
        {
            if (string.IsNullOrEmpty(original))
            {
                return false;
            }

            try
            {
                var match = Regex.Match(original, @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
                return match.Success && (match.Index == 0) && (match.Length == original.Length);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsNullOrWhiteSpace(this string val)
        {
            return string.IsNullOrWhiteSpace(val);
        }
    }
}
