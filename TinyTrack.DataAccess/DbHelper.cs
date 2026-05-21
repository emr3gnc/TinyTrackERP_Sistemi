using Microsoft.Data.Sqlite;

namespace TinyTrack.DataAccess;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public static class DbHelper
{
    public static string ConnectionString { get; set; } =
        "Data Source=TinyTrackDb.sqlite";

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public static SqliteConnection BaglantiOlustur()
    {
        var baglanti = new SqliteConnection(ConnectionString);
        baglanti.Open();

        using var komut = baglanti.CreateCommand();
        komut.CommandText = "PRAGMA foreign_keys = ON";
        komut.ExecuteNonQuery();

        return baglanti;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public static SqliteParameter Parametre(string name, object? deger)
    {
        if (deger is DateTime dateTime)
        {
            deger = dateTime.TimeOfDay == TimeSpan.Zero
                ? dateTime.ToString("yyyy-MM-dd")
                : dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        return new SqliteParameter(name, deger ?? DBNull.Value);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public static int KomutCalistir(string sql, params SqliteParameter[] parameters)
    {
        using var baglanti = BaglantiOlustur();
        using var komut = new SqliteCommand(sql, baglanti);
        komut.Parameters.AddRange(parameters);
        return komut.ExecuteNonQuery();
    }

    public static T? TekDegerCalistir<T>(string sql, params SqliteParameter[] parameters)
    {
        using var baglanti = BaglantiOlustur();
        using var komut = new SqliteCommand(sql, baglanti);
        komut.Parameters.AddRange(parameters);
        var deger = komut.ExecuteScalar();
        if (deger is null || deger == DBNull.Value)
        {
            return default;
        }

        return (T)Convert.ChangeType(deger, typeof(T));
    }

    public static List<T> ListeCalistir<T>(string sql, Func<SqliteDataReader, T> esleyici, params SqliteParameter[] parameters)
    {
        using var baglanti = BaglantiOlustur();
        using var komut = new SqliteCommand(sql, baglanti);
        komut.Parameters.AddRange(parameters);

        using var okuyucu = komut.ExecuteReader();
        var liste = new List<T>();
        while (okuyucu.Read())
        {
            liste.Add(esleyici(okuyucu));
        }

        return liste;
    }

    public static T? TekKayitCalistir<T>(string sql, Func<SqliteDataReader, T> esleyici, params SqliteParameter[] parameters)
    {
        return ListeCalistir(sql, esleyici, parameters).FirstOrDefault();
    }
}
