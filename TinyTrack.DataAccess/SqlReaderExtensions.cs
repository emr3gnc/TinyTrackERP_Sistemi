using Microsoft.Data.SqlClient;

namespace TinyTrack.DataAccess;

internal static class SqlReaderExtensions
{
    public static string ReadString(this SqlDataReader reader, string column)
    {
        var index = reader.GetOrdinal(column);
        return reader.IsDBNull(index) ? string.Empty : reader.GetString(index);
    }

    public static int ReadInt32(this SqlDataReader reader, string column)
    {
        var index = reader.GetOrdinal(column);
        return reader.IsDBNull(index) ? 0 : reader.GetInt32(index);
    }

    public static decimal ReadDecimal(this SqlDataReader reader, string column)
    {
        var index = reader.GetOrdinal(column);
        return reader.IsDBNull(index) ? 0 : reader.GetDecimal(index);
    }

    public static DateTime ReadDateTime(this SqlDataReader reader, string column)
    {
        var index = reader.GetOrdinal(column);
        return reader.IsDBNull(index) ? DateTime.MinValue : reader.GetDateTime(index);
    }

    public static bool ReadBoolean(this SqlDataReader reader, string column)
    {
        var index = reader.GetOrdinal(column);
        return !reader.IsDBNull(index) && reader.GetBoolean(index);
    }
}
