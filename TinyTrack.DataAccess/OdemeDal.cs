using Microsoft.Data.Sqlite;
using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class OdemeDal
{
    private const string SelectSql = """
        SELECT o.odemeID, o.rezervasyonID, o.ucret, o.odemetarihi, o.odemetipi, o.aciklama,
               m.ad || ' ' || m.soyad AS musteriAdSoyad
        FROM odeme o
        INNER JOIN rezervasyon r ON r.rezervasyonID = o.rezervasyonID
        INNER JOIN musteri m ON m.musteriID = r.musteriID
        """;

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Odeme> TumunuGetir()
    {
        return DbHelper.ListeCalistir($"{SelectSql} ORDER BY o.odemetarihi DESC", Esle);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Odeme? IdIleGetir(string odemeID)
    {
        return DbHelper.TekKayitCalistir(
            $"{SelectSql} WHERE o.odemeID = @odemeID",
            Esle,
            DbHelper.Parametre("@odemeID", odemeID));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Odeme> RezervasyonaGoreGetir(string rezervasyonID)
    {
        return DbHelper.ListeCalistir(
            $"{SelectSql} WHERE o.rezervasyonID = @rezervasyonID ORDER BY o.odemetarihi DESC",
            Esle,
            DbHelper.Parametre("@rezervasyonID", rezervasyonID));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Ekle(Odeme odeme)
    {
        const string sql = """
            INSERT INTO odeme (odemeID, rezervasyonID, ucret, odemetarihi, odemetipi, aciklama)
            VALUES (@odemeID, @rezervasyonID, @ucret, @odemetarihi, @odemetipi, @aciklama)
            """;
        return DbHelper.KomutCalistir(sql, Parametreler(odeme)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Guncelle(Odeme odeme)
    {
        const string sql = """
            UPDATE odeme
            SET rezervasyonID = @rezervasyonID,
                ucret = @ucret,
                odemetarihi = @odemetarihi,
                odemetipi = @odemetipi,
                aciklama = @aciklama
            WHERE odemeID = @odemeID
            """;
        return DbHelper.KomutCalistir(sql, Parametreler(odeme)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Sil(string odemeID)
    {
        return DbHelper.KomutCalistir(
            "DELETE FROM odeme WHERE odemeID = @odemeID",
            DbHelper.Parametre("@odemeID", odemeID)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static SqliteParameter[] Parametreler(Odeme odeme)
    {
        return
        [
            DbHelper.Parametre("@odemeID", odeme.OdemeID),
            DbHelper.Parametre("@rezervasyonID", odeme.RezervasyonID),
            DbHelper.Parametre("@ucret", odeme.Ucret),
            DbHelper.Parametre("@odemetarihi", odeme.OdemeTarihi.Date),
            DbHelper.Parametre("@odemetipi", odeme.OdemeTipi.ToString()),
            DbHelper.Parametre("@aciklama", odeme.Aciklama)
        ];
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static Odeme Esle(SqliteDataReader okuyucu)
    {
        Enum.TryParse(okuyucu.MetinOku("odemetipi"), out OdemeTipi odemeTipi);
        return new Odeme
        {
            OdemeID = okuyucu.MetinOku("odemeID"),
            RezervasyonID = okuyucu.MetinOku("rezervasyonID"),
            Ucret = okuyucu.OndalikOku("ucret"),
            OdemeTarihi = okuyucu.TarihOku("odemetarihi"),
            OdemeTipi = odemeTipi,
            Aciklama = okuyucu.MetinOku("aciklama"),
            MusteriAdSoyad = okuyucu.MetinOku("musteriAdSoyad")
        };
    }
}
