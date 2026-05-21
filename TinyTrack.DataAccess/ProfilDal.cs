using Microsoft.Data.SqlClient;
using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

public class ProfilDal
{
    public Kullanici? GetKullaniciById(string kullaniciID)
    {
        return DbHelper.ExecuteSingle(
            """
            SELECT kullaniciID, adSoyad, email, sifre, rol
            FROM dbo.kullanici
            WHERE kullaniciID = @kullaniciID
            """,
            MapKullanici,
            DbHelper.Parameter("@kullaniciID", kullaniciID));
    }

    public Kullanici? CheckLogin(string email, string sifre)
    {
        return DbHelper.ExecuteSingle(
            """
            SELECT kullaniciID, adSoyad, email, sifre, rol
            FROM dbo.kullanici
            WHERE email = @email AND sifre = @sifre
            """,
            MapKullanici,
            DbHelper.Parameter("@email", email),
            DbHelper.Parameter("@sifre", sifre));
    }

    public bool UpdateKullanici(Kullanici kullanici)
    {
        return DbHelper.ExecuteNonQuery(
            """
            UPDATE dbo.kullanici
            SET adSoyad = @adSoyad,
                email = @email,
                rol = @rol
            WHERE kullaniciID = @kullaniciID
            """,
            DbHelper.Parameter("@adSoyad", kullanici.AdSoyad),
            DbHelper.Parameter("@email", kullanici.Email),
            DbHelper.Parameter("@rol", kullanici.Rol),
            DbHelper.Parameter("@kullaniciID", kullanici.KullaniciID)) > 0;
    }

    public bool UpdatePassword(string kullaniciID, string yeniSifre)
    {
        return DbHelper.ExecuteNonQuery(
            "UPDATE dbo.kullanici SET sifre = @sifre WHERE kullaniciID = @kullaniciID",
            DbHelper.Parameter("@sifre", yeniSifre),
            DbHelper.Parameter("@kullaniciID", kullaniciID)) > 0;
    }

    public string GetKullaniciYetkiRolu(string kullaniciID)
    {
        return DbHelper.ExecuteScalar<string>(
            "SELECT rol FROM dbo.kullanici WHERE kullaniciID = @kullaniciID",
            DbHelper.Parameter("@kullaniciID", kullaniciID)) ?? string.Empty;
    }

    public IsletmeAyarlari GetSystemSettings()
    {
        return DbHelper.ExecuteSingle(
            """
            SELECT ayarID, isletmeAdi, paraBirimi, dil, rezervasyonBildirimleri, temizlikUyarilari
            FROM dbo.isletme_ayarlari
            WHERE ayarID = 'SET-001'
            """,
            MapSettings) ?? new IsletmeAyarlari();
    }

    public bool UpdateSystemSettings(IsletmeAyarlari ayarlar)
    {
        return DbHelper.ExecuteNonQuery(
            """
            UPDATE dbo.isletme_ayarlari
            SET isletmeAdi = @isletmeAdi,
                paraBirimi = @paraBirimi,
                dil = @dil,
                rezervasyonBildirimleri = @rezervasyonBildirimleri,
                temizlikUyarilari = @temizlikUyarilari
            WHERE ayarID = @ayarID
            """,
            DbHelper.Parameter("@isletmeAdi", ayarlar.IsletmeAdi),
            DbHelper.Parameter("@paraBirimi", ayarlar.ParaBirimi),
            DbHelper.Parameter("@dil", ayarlar.Dil),
            DbHelper.Parameter("@rezervasyonBildirimleri", ayarlar.RezervasyonBildirimleri),
            DbHelper.Parameter("@temizlikUyarilari", ayarlar.TemizlikUyarilari),
            DbHelper.Parameter("@ayarID", ayarlar.AyarID)) > 0;
    }

    private static Kullanici MapKullanici(SqlDataReader reader)
    {
        return new Kullanici
        {
            KullaniciID = reader.ReadString("kullaniciID"),
            AdSoyad = reader.ReadString("adSoyad"),
            Email = reader.ReadString("email"),
            Sifre = reader.ReadString("sifre"),
            Rol = reader.ReadString("rol")
        };
    }

    private static IsletmeAyarlari MapSettings(SqlDataReader reader)
    {
        return new IsletmeAyarlari
        {
            AyarID = reader.ReadString("ayarID"),
            IsletmeAdi = reader.ReadString("isletmeAdi"),
            ParaBirimi = reader.ReadString("paraBirimi"),
            Dil = reader.ReadString("dil"),
            RezervasyonBildirimleri = reader.ReadBoolean("rezervasyonBildirimleri"),
            TemizlikUyarilari = reader.ReadBoolean("temizlikUyarilari")
        };
    }
}
