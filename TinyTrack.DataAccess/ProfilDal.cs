using Microsoft.Data.Sqlite;
using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class ProfilDal
{
    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Kullanici? KullaniciyiIdIleGetir(string kullaniciID)
    {
        return DbHelper.TekKayitCalistir(
            """
            SELECT kullaniciID, adSoyad, email, sifre, rol
            FROM kullanici
            WHERE kullaniciID = @kullaniciID
            """,
            MapKullanici,
            DbHelper.Parametre("@kullaniciID", kullaniciID));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Kullanici? CheckLogin(string email, string sifre)
    {
        return DbHelper.TekKayitCalistir(
            """
            SELECT kullaniciID, adSoyad, email, sifre, rol
            FROM kullanici
            WHERE email = @email AND sifre = @sifre
            """,
            MapKullanici,
            DbHelper.Parametre("@email", email),
            DbHelper.Parametre("@sifre", sifre));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool KullaniciyiGuncelle(Kullanici kullanici)
    {
        return DbHelper.KomutCalistir(
            """
            UPDATE kullanici
            SET adSoyad = @adSoyad,
                email = @email,
                rol = @rol
            WHERE kullaniciID = @kullaniciID
            """,
            DbHelper.Parametre("@adSoyad", kullanici.AdSoyad),
            DbHelper.Parametre("@email", kullanici.Email),
            DbHelper.Parametre("@rol", kullanici.Rol),
            DbHelper.Parametre("@kullaniciID", kullanici.KullaniciID)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool SifreyiGuncelle(string kullaniciID, string yeniSifre)
    {
        return DbHelper.KomutCalistir(
            "UPDATE kullanici SET sifre = @sifre WHERE kullaniciID = @kullaniciID",
            DbHelper.Parametre("@sifre", yeniSifre),
            DbHelper.Parametre("@kullaniciID", kullaniciID)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public string KullaniciYetkiRolunuGetir(string kullaniciID)
    {
        return DbHelper.TekDegerCalistir<string>(
            "SELECT rol FROM kullanici WHERE kullaniciID = @kullaniciID",
            DbHelper.Parametre("@kullaniciID", kullaniciID)) ?? string.Empty;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public IsletmeAyarlari SistemAyarlariniGetir()
    {
        return DbHelper.TekKayitCalistir(
            """
            SELECT ayarID, isletmeAdi, paraBirimi, dil, rezervasyonBildirimleri, temizlikUyarilari
            FROM isletme_ayarlari
            WHERE ayarID = 'SET-001'
            """,
            MapSettings) ?? new IsletmeAyarlari();
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool SistemAyarlariniGuncelle(IsletmeAyarlari ayarlar)
    {
        return DbHelper.KomutCalistir(
            """
            UPDATE isletme_ayarlari
            SET isletmeAdi = @isletmeAdi,
                paraBirimi = @paraBirimi,
                dil = @dil,
                rezervasyonBildirimleri = @rezervasyonBildirimleri,
                temizlikUyarilari = @temizlikUyarilari
            WHERE ayarID = @ayarID
            """,
            DbHelper.Parametre("@isletmeAdi", ayarlar.IsletmeAdi),
            DbHelper.Parametre("@paraBirimi", ayarlar.ParaBirimi),
            DbHelper.Parametre("@dil", ayarlar.Dil),
            DbHelper.Parametre("@rezervasyonBildirimleri", ayarlar.RezervasyonBildirimleri),
            DbHelper.Parametre("@temizlikUyarilari", ayarlar.TemizlikUyarilari),
            DbHelper.Parametre("@ayarID", ayarlar.AyarID)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static Kullanici MapKullanici(SqliteDataReader okuyucu)
    {
        return new Kullanici
        {
            KullaniciID = okuyucu.MetinOku("kullaniciID"),
            AdSoyad = okuyucu.MetinOku("adSoyad"),
            Email = okuyucu.MetinOku("email"),
            Sifre = okuyucu.MetinOku("sifre"),
            Rol = okuyucu.MetinOku("rol")
        };
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static IsletmeAyarlari MapSettings(SqliteDataReader okuyucu)
    {
        return new IsletmeAyarlari
        {
            AyarID = okuyucu.MetinOku("ayarID"),
            IsletmeAdi = okuyucu.MetinOku("isletmeAdi"),
            ParaBirimi = okuyucu.MetinOku("paraBirimi"),
            Dil = okuyucu.MetinOku("dil"),
            RezervasyonBildirimleri = okuyucu.MantiksalOku("rezervasyonBildirimleri"),
            TemizlikUyarilari = okuyucu.MantiksalOku("temizlikUyarilari")
        };
    }
}
