using Microsoft.Data.Sqlite;

namespace TinyTrack.DataAccess;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
internal static class SqlReaderExtensions
{
    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public static string MetinOku(this SqliteDataReader okuyucu, string sutun)
    {
        var sira = okuyucu.GetOrdinal(sutun);
        return okuyucu.IsDBNull(sira) ? string.Empty : okuyucu.GetString(sira);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public static int TamsayiOku(this SqliteDataReader okuyucu, string sutun)
    {
        var sira = okuyucu.GetOrdinal(sutun);
        return okuyucu.IsDBNull(sira) ? 0 : okuyucu.GetInt32(sira);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public static decimal OndalikOku(this SqliteDataReader okuyucu, string sutun)
    {
        var sira = okuyucu.GetOrdinal(sutun);
        return okuyucu.IsDBNull(sira) ? 0 : okuyucu.GetDecimal(sira);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public static DateTime TarihOku(this SqliteDataReader okuyucu, string sutun)
    {
        var sira = okuyucu.GetOrdinal(sutun);
        return okuyucu.IsDBNull(sira) ? DateTime.MinValue : okuyucu.GetDateTime(sira);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public static bool MantiksalOku(this SqliteDataReader okuyucu, string sutun)
    {
        var sira = okuyucu.GetOrdinal(sutun);
        return !okuyucu.IsDBNull(sira) && okuyucu.GetBoolean(sira);
    }
}
