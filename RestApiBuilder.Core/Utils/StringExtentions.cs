using System.Text;

namespace RestApiClientBuilder.Core.Utils
{
    public static class StringExtentions
    {
        public static string FirstLetterToLower(this string me)
        {
            if (string.IsNullOrWhiteSpace(me))
            {
                return me;
            }

            StringBuilder builder = new StringBuilder(char.ToLower(me[0]).ToString());
            builder.Append(me.Substring(1));
            return builder.ToString();
        }
    }
}