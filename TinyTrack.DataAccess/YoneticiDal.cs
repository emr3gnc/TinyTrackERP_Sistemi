using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

public class YoneticiDal
{
    public DashboardOzet GetDashboardData()
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var nextMonth = monthStart.AddMonths(1);

        var bugunkuGiris = DbHelper.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM dbo.rezervasyon WHERE durum = N'Aktif' AND bastarih = @today",
            DbHelper.Parameter("@today", today));

        var bugunkuCikis = DbHelper.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM dbo.rezervasyon WHERE durum = N'Aktif' AND sontarih = @today",
            DbHelper.Parameter("@today", today));

        var temizliktekiVarlik = DbHelper.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM dbo.varlik WHERE durum = N'Temizlikte'");

        var aktifRezervasyon = DbHelper.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM dbo.rezervasyon WHERE durum = N'Aktif'");

        var aylikGelir = GetAylikToplamGelir(monthStart.Month, monthStart.Year);

        var dolu = DbHelper.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM dbo.varlik WHERE durum = N'Dolu'");
        var toplam = DbHelper.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM dbo.varlik");
        var dolulukOrani = toplam == 0 ? 0 : Math.Round((double)dolu / toplam * 100, 1);

        var siradakiGiris = DbHelper.ExecuteScalar<string>(
            """
            SELECT TOP 1 CONCAT(m.ad, ' ', m.soyad, ' - ', v.ad, ' - ', CONVERT(varchar(10), r.bastarih, 104))
            FROM dbo.rezervasyon r
            INNER JOIN dbo.musteri m ON m.musteriID = r.musteriID
            INNER JOIN dbo.varlik v ON v.varlikID = r.varlikID
            WHERE r.durum = N'Aktif' AND r.bastarih >= @today
            ORDER BY r.bastarih
            """,
            DbHelper.Parameter("@today", today)) ?? "Planli giris yok";

        return new DashboardOzet
        {
            BugunkuGiris = bugunkuGiris,
            BugunkuCikis = bugunkuCikis,
            TemizliktekiVarlik = temizliktekiVarlik,
            AktifRezervasyon = aktifRezervasyon,
            AylikGelir = aylikGelir,
            DolulukOrani = dolulukOrani,
            SiradakiGiris = siradakiGiris
        };
    }

    public decimal GetAylikToplamGelir(int ay, int yil)
    {
        var start = new DateTime(yil, ay, 1);
        var end = start.AddMonths(1);
        var odemeGeliri = DbHelper.ExecuteScalar<decimal>(
            """
            SELECT COALESCE(SUM(ucret), 0)
            FROM dbo.odeme
            WHERE odemetarihi >= @start AND odemetarihi < @end
            """,
            DbHelper.Parameter("@start", start),
            DbHelper.Parameter("@end", end));

        if (odemeGeliri > 0)
        {
            return odemeGeliri;
        }

        return DbHelper.ExecuteScalar<decimal>(
            """
            SELECT COALESCE(SUM(toplamucret), 0)
            FROM dbo.rezervasyon
            WHERE durum <> N'Iptal' AND bastarih >= @start AND bastarih < @end
            """,
            DbHelper.Parameter("@start", start),
            DbHelper.Parameter("@end", end));
    }

    public double GetDolulukOrani()
    {
        var dolu = DbHelper.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM dbo.varlik WHERE durum = N'Dolu'");
        var toplam = DbHelper.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM dbo.varlik");
        return toplam == 0 ? 0 : Math.Round((double)dolu / toplam * 100, 1);
    }

    public int GetAktifRezervasyonSayisi()
    {
        return DbHelper.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM dbo.rezervasyon WHERE durum = N'Aktif'");
    }

    public int GetTemizliktekiVarlikSayisi()
    {
        return DbHelper.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM dbo.varlik WHERE durum = N'Temizlikte'");
    }

    public int GetGunlukGirisCikisSayisi(DateTime tarih)
    {
        return DbHelper.ExecuteScalar<int>(
            """
            SELECT COUNT(1)
            FROM dbo.rezervasyon
            WHERE durum = N'Aktif' AND (bastarih = @tarih OR sontarih = @tarih)
            """,
            DbHelper.Parameter("@tarih", tarih.Date));
    }

    public bool CheckAdminLogin(string kullaniciAdi, string sifre)
    {
        var count = DbHelper.ExecuteScalar<int>(
            """
            SELECT COUNT(1)
            FROM dbo.kullanici
            WHERE (email = @kullaniciAdi OR adSoyad = @kullaniciAdi) AND sifre = @sifre AND rol = N'Yonetici'
            """,
            DbHelper.Parameter("@kullaniciAdi", kullaniciAdi),
            DbHelper.Parameter("@sifre", sifre));
        return count > 0;
    }
}
