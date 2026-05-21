using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class YoneticiDal
{
    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public DashboardOzet PanelVerisiniGetir()
    {
        var today = DateTime.Today;
        var todayText = today.ToString("yyyy-MM-dd");
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var nextMonth = monthStart.AddMonths(1);

        var bugunkuGiris = DbHelper.TekDegerCalistir<int>(
            "SELECT COUNT(1) FROM rezervasyon WHERE durum = 'Aktif' AND bastarih = @today",
            DbHelper.Parametre("@today", todayText));

        var bugunkuCikis = DbHelper.TekDegerCalistir<int>(
            "SELECT COUNT(1) FROM rezervasyon WHERE durum = 'Aktif' AND sontarih = @today",
            DbHelper.Parametre("@today", todayText));

        var temizliktekiVarlik = DbHelper.TekDegerCalistir<int>(
            "SELECT COUNT(1) FROM varlik WHERE durum = 'Temizlikte'");

        var aktifRezervasyon = DbHelper.TekDegerCalistir<int>(
            "SELECT COUNT(1) FROM rezervasyon WHERE durum = 'Aktif'");

        var aylikGelir = AylikToplamGeliriGetir(monthStart.Month, monthStart.Year);

        var dolu = DbHelper.TekDegerCalistir<int>(
            "SELECT COUNT(1) FROM varlik WHERE durum = 'Dolu'");
        var toplam = DbHelper.TekDegerCalistir<int>(
            "SELECT COUNT(1) FROM varlik");
        var dolulukOrani = toplam == 0 ? 0 : Math.Round((double)dolu / toplam * 100, 1);

        var siradakiGiris = DbHelper.TekDegerCalistir<string>(
            """
            SELECT m.ad || ' ' || m.soyad || ' - ' || v.ad || ' - ' || strftime('%d.%m.%Y', r.bastarih)
            FROM rezervasyon r
            INNER JOIN musteri m ON m.musteriID = r.musteriID
            INNER JOIN varlik v ON v.varlikID = r.varlikID
            WHERE r.durum = 'Aktif' AND r.bastarih >= @today
            ORDER BY r.bastarih
            LIMIT 1
            """,
            DbHelper.Parametre("@today", todayText)) ?? "Planli giris yok";

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

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public decimal AylikToplamGeliriGetir(int ay, int yil)
    {
        var start = new DateTime(yil, ay, 1);
        var end = start.AddMonths(1);
        var odemeGeliri = DbHelper.TekDegerCalistir<decimal>(
            """
            SELECT COALESCE(SUM(ucret), 0)
            FROM odeme
            WHERE odemetarihi >= @start AND odemetarihi < @end
            """,
            DbHelper.Parametre("@start", start),
            DbHelper.Parametre("@end", end));

        if (odemeGeliri > 0)
        {
            return odemeGeliri;
        }

        return DbHelper.TekDegerCalistir<decimal>(
            """
            SELECT COALESCE(SUM(toplamucret), 0)
            FROM rezervasyon
            WHERE durum <> 'Iptal' AND bastarih >= @start AND bastarih < @end
            """,
            DbHelper.Parametre("@start", start),
            DbHelper.Parametre("@end", end));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public double DolulukOraniniGetir()
    {
        var dolu = DbHelper.TekDegerCalistir<int>(
            "SELECT COUNT(1) FROM varlik WHERE durum = 'Dolu'");
        var toplam = DbHelper.TekDegerCalistir<int>(
            "SELECT COUNT(1) FROM varlik");
        return toplam == 0 ? 0 : Math.Round((double)dolu / toplam * 100, 1);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public int AktifRezervasyonSayisiniGetir()
    {
        return DbHelper.TekDegerCalistir<int>(
            "SELECT COUNT(1) FROM rezervasyon WHERE durum = 'Aktif'");
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public int TemizliktekiVarlikSayisiniGetir()
    {
        return DbHelper.TekDegerCalistir<int>(
            "SELECT COUNT(1) FROM varlik WHERE durum = 'Temizlikte'");
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public int GunlukGirisCikisSayisiniGetir(DateTime tarih)
    {
        return DbHelper.TekDegerCalistir<int>(
            """
            SELECT COUNT(1)
            FROM rezervasyon
            WHERE durum = 'Aktif' AND (bastarih = @tarih OR sontarih = @tarih)
            """,
            DbHelper.Parametre("@tarih", tarih.ToString("yyyy-MM-dd")));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool CheckAdminLogin(string kullaniciAdi, string sifre)
    {
        var sayi = DbHelper.TekDegerCalistir<int>(
            """
            SELECT COUNT(1)
            FROM kullanici
            WHERE (email = @kullaniciAdi OR adSoyad = @kullaniciAdi) AND sifre = @sifre AND rol = 'Yonetici'
            """,
            DbHelper.Parametre("@kullaniciAdi", kullaniciAdi),
            DbHelper.Parametre("@sifre", sifre));
        return sayi > 0;
    }
}
