using Microsoft.Data.Sqlite;
using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class RezervasyonDal
{
    private const string SelectSql = """
        SELECT r.rezervasyonID, r.musteriID, r.varlikID, r.bastarih, r.sontarih,
               r.toplamucret, r.durum, r.kayitTarihi,
               m.ad || ' ' || m.soyad AS musteriAdSoyad,
               v.ad AS varlikAdi
        FROM rezervasyon r
        INNER JOIN musteri m ON m.musteriID = r.musteriID
        INNER JOIN varlik v ON v.varlikID = r.varlikID
        """;

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Rezervasyon> TumunuGetir()
    {
        return DbHelper.ListeCalistir($"{SelectSql} ORDER BY r.bastarih DESC", Esle);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Rezervasyon? IdIleGetir(string rezervasyonID)
    {
        return DbHelper.TekKayitCalistir(
            $"{SelectSql} WHERE r.rezervasyonID = @rezervasyonID",
            Esle,
            DbHelper.Parametre("@rezervasyonID", rezervasyonID));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Ekle(Rezervasyon rezervasyon)
    {
        const string sql = """
            INSERT INTO rezervasyon (rezervasyonID, musteriID, varlikID, bastarih, sontarih, toplamucret, durum, kayitTarihi)
            VALUES (@rezervasyonID, @musteriID, @varlikID, @bastarih, @sontarih, @toplamucret, @durum, @kayitTarihi)
            """;
        return DbHelper.KomutCalistir(sql, Parametreler(rezervasyon)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Guncelle(Rezervasyon rezervasyon)
    {
        const string sql = """
            UPDATE rezervasyon
            SET musteriID = @musteriID,
                varlikID = @varlikID,
                bastarih = @bastarih,
                sontarih = @sontarih,
                toplamucret = @toplamucret,
                durum = @durum
            WHERE rezervasyonID = @rezervasyonID
            """;
        return DbHelper.KomutCalistir(sql, Parametreler(rezervasyon)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Sil(string rezervasyonID)
    {
        return DbHelper.KomutCalistir(
            "DELETE FROM rezervasyon WHERE rezervasyonID = @rezervasyonID",
            DbHelper.Parametre("@rezervasyonID", rezervasyonID)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool HasOverlap(string varlikID, DateTime basTarih, DateTime sonTarih, string exceptRezervasyonID = "")
    {
        var sayi = DbHelper.TekDegerCalistir<int>(
            """
            SELECT COUNT(1)
            FROM rezervasyon
            WHERE varlikID = @varlikID
              AND rezervasyonID <> @exceptRezervasyonID
              AND durum = 'Aktif'
              AND @basTarih < sontarih
              AND @sonTarih > bastarih
            """,
            DbHelper.Parametre("@varlikID", varlikID),
            DbHelper.Parametre("@exceptRezervasyonID", exceptRezervasyonID),
            DbHelper.Parametre("@basTarih", basTarih.Date),
            DbHelper.Parametre("@sonTarih", sonTarih.Date));
        return sayi > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Rezervasyon> Ara(string q)
    {
        return DbHelper.ListeCalistir(
            $"""
            {SelectSql}
            WHERE r.rezervasyonID LIKE @q OR m.ad LIKE @q OR m.soyad LIKE @q OR v.ad LIKE @q OR r.durum LIKE @q
            ORDER BY r.bastarih DESC
            """,
            Esle,
            DbHelper.Parametre("@q", $"%{q}%"));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public decimal OdenenTutariGetir(string rezervasyonID)
    {
        return DbHelper.TekDegerCalistir<decimal>(
            "SELECT COALESCE(SUM(ucret), 0) FROM odeme WHERE rezervasyonID = @rezervasyonID",
            DbHelper.Parametre("@rezervasyonID", rezervasyonID));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static SqliteParameter[] Parametreler(Rezervasyon rezervasyon)
    {
        return
        [
            DbHelper.Parametre("@rezervasyonID", rezervasyon.RezervasyonID),
            DbHelper.Parametre("@musteriID", rezervasyon.MusteriID),
            DbHelper.Parametre("@varlikID", rezervasyon.VarlikID),
            DbHelper.Parametre("@bastarih", rezervasyon.BasTarih.Date),
            DbHelper.Parametre("@sontarih", rezervasyon.SonTarih.Date),
            DbHelper.Parametre("@toplamucret", rezervasyon.ToplamUcret),
            DbHelper.Parametre("@durum", rezervasyon.Durum.ToString()),
            DbHelper.Parametre("@kayitTarihi", rezervasyon.KayitTarihi)
        ];
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static Rezervasyon Esle(SqliteDataReader okuyucu)
    {
        Enum.TryParse(okuyucu.MetinOku("durum"), out RezervasyonDurumu durum);
        return new Rezervasyon
        {
            RezervasyonID = okuyucu.MetinOku("rezervasyonID"),
            MusteriID = okuyucu.MetinOku("musteriID"),
            VarlikID = okuyucu.MetinOku("varlikID"),
            BasTarih = okuyucu.TarihOku("bastarih"),
            SonTarih = okuyucu.TarihOku("sontarih"),
            ToplamUcret = okuyucu.OndalikOku("toplamucret"),
            Durum = durum,
            KayitTarihi = okuyucu.TarihOku("kayitTarihi"),
            MusteriAdSoyad = okuyucu.MetinOku("musteriAdSoyad"),
            VarlikAdi = okuyucu.MetinOku("varlikAdi")
        };
    }
}
