using System.Linq;

namespace CodecControl.Web.Helpers
{
    public static class StringExtensions
    {
        public static bool IsNumeric(this string s)
        {
            s = s ?? string.Empty;
            return s.All(char.IsDigit);
        }

    }
}