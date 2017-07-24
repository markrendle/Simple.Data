using System.Text;

namespace Shitty.Data.Extensions
{
    public static class DynamicStringExtensions
    {
        public static string ToSnakeCase(this string source)
        {
            var builder = new StringBuilder();

            foreach (var c in source)
            {
                if (char.IsUpper(c))
                {
                    if (builder.Length > 0) builder.Append('_');
                }
                builder.Append(char.ToLower(c));
            }

            return builder.ToString();
        }
    }
}
