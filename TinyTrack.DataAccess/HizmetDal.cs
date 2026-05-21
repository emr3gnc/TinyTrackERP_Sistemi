using Microsoft.Data.Sqlite;
using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class HizmetDal
{
    private const string SelectSql = "SELECT hizmetID, rezervasyonID, ad, ucret FROM hizmet";

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Hizmet> TumunuGetir()
    {
        return DbHelper.ListeCalistir($"{SelectSql} ORDER BY ad", Esle);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Hizmet> RezervasyonaGoreGetir(string rezervasyonID)
    {
        return DbHelper.ListeCalistir(
            $"{SelectSql} WHERE rezervasyonID = @rezervasyonID ORDER BY ad",
            Esle,
            DbHelper.Parametre("@rezervasyonID", rezervasyonID));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Hizmet? IdIleGetir(string hizmetID)
    {
        return DbHelper.TekKayitCalistir(
            $"{SelectSql} WHERE hizmetID = @hizmetID",
            Esle,
            DbHelper.Parametre("@hizmetID", hizmetID));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Ekle(Hizmet hizmet)
    {
        const string sql = """
            INSERT INTO hizmet (hizmetID, rezervasyonID, ad, ucret)
            VALUES (@hizmetID, @rezervasyonID, @ad, @ucret)
            """;
        return DbHelper.KomutCalistir(sql, Parametreler(hizmet)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Guncelle(Hizmet hizmet)
    {
        const string sql = """
            UPDATE hizmet
            SET rezervasyonID = @rezervasyonID,
                ad = @ad,
                ucret = @ucret
            WHERE hizmetID = @hizmetID
            """;
        return DbHelper.KomutCalistir(sql, Parametreler(hizmet)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Sil(string hizmetID)
    {
        return DbHelper.KomutCalistir(
            "DELETE FROM hizmet WHERE hizmetID = @hizmetID",
            DbHelper.Parametre("@hizmetID", hizmetID)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public decimal ToplamHizmetUcretiniGetir(string rezervasyonID)
    {
        return DbHelper.TekDegerCalistir<decimal>(
            "SELECT COALESCE(SUM(ucret), 0) FROM hizmet WHERE rezervasyonID = @rezervasyonID",
            DbHelper.Parametre("@rezervasyonID", rezervasyonID));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static SqliteParameter[] Parametreler(Hizmet hizmet)
    {
        return
        [
            DbHelper.Parametre("@hizmetID", hizmet.HizmetID),
            DbHelper.Parametre("@rezervasyonID", hizmet.RezervasyonID),
            DbHelper.Parametre("@ad", hizmet.Ad),
            DbHelper.Parametre("@ucret", hizmet.Ucret)
        ];
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static Hizmet Esle(SqliteDataReader okuyucu)
    {
        return new Hizmet
        {
            HizmetID = okuyucu.MetinOku("hizmetID"),
            RezervasyonID = okuyucu.MetinOku("rezervasyonID"),
            Ad = okuyucu.MetinOku("ad"),
            Ucret = okuyucu.OndalikOku("ucret")
        };
    }
}
