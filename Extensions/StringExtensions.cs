namespace TF2PugBot.Extensions;

public static class StringExtensions
{
    public static string FirstLetterUppercase (this string s)
    {
        s = s.ToLower().Trim();
        s = s.Substring(0, 1).ToUpper() + s.Substring(1);
        return s;
    }
}