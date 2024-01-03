namespace user_management.Utilities;

using System.Text;

public static class StringExtentions
{
    public static string ToPascalCase(this string str)
    {
        if (str == null)
            throw new ArgumentNullException(nameof(str));

        if (str.Length < 2)
            return str;

        char previousChar = str[0];
        var sb = new StringBuilder();

        sb.Append(char.ToUpperInvariant(previousChar));
        for (int i = 1; i < str.Length; ++i)
        {
            char c = str[i];

            if (c.Equals('_'))
            {
                previousChar = c;
                continue;
            }

            if (previousChar.Equals('_'))
            {
                sb.Append(char.ToUpperInvariant(c));
                previousChar = char.ToUpperInvariant(c);
            }
            else
            {
                sb.Append(c);
                previousChar = c;
            }
        }

        return sb.ToString();
    }
    public static string ToSnakeCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            throw new ArgumentNullException(nameof(str));

        if (str.Contains('_') || str.Length < 2)
            return str;

        char previousChar = str[0];
        var sb = new StringBuilder();

        sb.Append(char.ToLowerInvariant(previousChar));
        for (int i = 1; i < str.Length; ++i)
        {
            char c = str[i];

            if (char.IsLower(previousChar) && char.IsUpper(c))
            {
                sb.Append(char.Parse("_"));
                sb.Append(char.ToLowerInvariant(c));
                previousChar = c;
                continue;
            }
            else
            {
                sb.Append(char.ToLowerInvariant(c));
                previousChar = c;
                continue;
            }
        }

        return sb.ToString();
    }
    public static string TrimStart(this string source, string value, StringComparison comparisonType)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        int valueLength = value.Length;
        int startIndex = 0;
        while (source.IndexOf(value, startIndex, comparisonType) == startIndex)
            startIndex += valueLength;

        return source[startIndex..];
    }
}
