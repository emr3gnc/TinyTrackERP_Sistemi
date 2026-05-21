using Microsoft.Data.SqlClient;

namespace TinyTrack.DataAccess;

public static class DbHelper
{
    public static string ConnectionString { get; set; } =
        "Server=(localdb)\\MSSQLLocalDB;Database=TinyTrackDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

    public static SqlConnection CreateConnection()
    {
        return new SqlConnection(ConnectionString);
    }

    public static SqlParameter Parameter(string name, object? value)
    {
        return new SqlParameter(name, value ?? DBNull.Value);
    }

    public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    public static T? ExecuteScalar<T>(string sql, params SqlParameter[] parameters)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        connection.Open();
        var value = command.ExecuteScalar();
        if (value is null || value == DBNull.Value)
        {
            return default;
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }

    public static List<T> ExecuteList<T>(string sql, Func<SqlDataReader, T> mapper, params SqlParameter[] parameters)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        connection.Open();

        using var reader = command.ExecuteReader();
        var list = new List<T>();
        while (reader.Read())
        {
            list.Add(mapper(reader));
        }

        return list;
    }

    public static T? ExecuteSingle<T>(string sql, Func<SqlDataReader, T> mapper, params SqlParameter[] parameters)
    {
        return ExecuteList(sql, mapper, parameters).FirstOrDefault();
    }
}
