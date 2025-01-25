using System.Text.RegularExpressions;

internal static class SqlStringExtensions
{
    internal static string NormalizeSql(this string sql)
    {
        return Regex.Replace(sql, @"\s+", " ").Trim();
    }
}
