using TinyTrack.DataAccess;
using TinyTrack.Entities;
using System.Net.Mail;

namespace TinyTrack.Business;

public class ProfilManager
{
    private readonly ProfilDal _profilDal = new();

    public Kullanici? KullaniciProfilBilgileriniGetir(string kullaniciID)
    {
        return _profilDal.GetKullaniciById(kullaniciID);
    }

    public bool ProfilGuncelle(Kullanici kullanici)
    {
        if (string.IsNullOrWhiteSpace(kullanici.AdSoyad) || string.IsNullOrWhiteSpace(kullanici.Email))
        {
            throw new BusinessRuleException("Ad soyad ve e-posta zorunludur.");
        }

        if (!IsValidEmail(kullanici.Email))
        {
            throw new BusinessRuleException("Gecerli bir e-posta adresi girilmelidir.");
        }

        return _profilDal.UpdateKullanici(kullanici);
    }

    public Kullanici? SistemeGirisYap(string email, string sifre)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(sifre))
        {
            throw new BusinessRuleException("E-posta ve sifre zorunludur.");
        }

        return _profilDal.CheckLogin(email, sifre);
    }

    public bool SifreDegistir(string kullaniciID, string yeniSifre)
    {
        if (yeniSifre.Length < 6)
        {
            throw new BusinessRuleException("Sifre en az 6 karakter olmalidir.");
        }

        return _profilDal.UpdatePassword(kullaniciID, yeniSifre);
    }

    public bool SifreDegistir(string kullaniciID, string mevcutSifre, string yeniSifre, string yeniSifreTekrar)
    {
        if (string.IsNullOrWhiteSpace(mevcutSifre) || string.IsNullOrWhiteSpace(yeniSifre) || string.IsNullOrWhiteSpace(yeniSifreTekrar))
        {
            throw new BusinessRuleException("Mevcut sifre, yeni sifre ve tekrar alani zorunludur.");
        }

        if (yeniSifre != yeniSifreTekrar)
        {
            throw new BusinessRuleException("Yeni sifre ve tekrar alani ayni olmalidir.");
        }

        var kullanici = _profilDal.GetKullaniciById(kullaniciID)
            ?? throw new BusinessRuleException("Kullanici bulunamadi.");
        if (kullanici.Sifre != mevcutSifre)
        {
            throw new BusinessRuleException("Mevcut sifre hatali.");
        }

        return SifreDegistir(kullaniciID, yeniSifre);
    }

    public bool YetkiKontrolEt(string kullaniciID, string modulAdi)
    {
        var rol = _profilDal.GetKullaniciYetkiRolu(kullaniciID);
        return rol == "Yonetici" || modulAdi is "Dashboard" or "Rezervasyon";
    }

    public IsletmeAyarlari IsletmeAyarlariGetir()
    {
        return _profilDal.GetSystemSettings();
    }

    public bool IsletmeAyarlariGuncelle(IsletmeAyarlari ayarlar)
    {
        if (string.IsNullOrWhiteSpace(ayarlar.IsletmeAdi))
        {
            throw new BusinessRuleException("Isletme adi zorunludur.");
        }

        return _profilDal.UpdateSystemSettings(ayarlar);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);
            return address.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
