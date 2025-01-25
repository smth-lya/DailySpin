using System.Data;

namespace DailySpin.ORM;

static class DataReaderExtensions
{
    private static bool TryGetOridinal(this IDataReader reader, string column, out int order)
    {
        order = -1;
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), column, StringComparison.OrdinalIgnoreCase))
            {
                order = i;
                return true;
            }
        }

        return false;
    }

    public static string? GetString(this IDataReader reader, string columnName)
    {
        if (reader.TryGetOridinal(columnName, out int order)) 
            return reader.GetString(order);
        return default;
    }

    public static int GetInt32(this IDataReader reader, string columnName)
    {
        if (reader.TryGetOridinal(columnName, out int order))
            reader.GetInt32(order);
        return default;
    }

    public static Guid GetGuid(this IDataReader reader, string columnName)
    {
        if (reader.TryGetOridinal(columnName, out int order))
            return reader.GetGuid(order);
        return default;
    }

    public static DateTime GetDateTime(this IDataReader reader, string columnName)
    {
        if (reader.TryGetOridinal(columnName, out int order))
            return reader.GetDateTime(order);
        return default;
    }
}
