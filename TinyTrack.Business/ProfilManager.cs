using TinyTrack.DataAccess;
using TinyTrack.Entities;
using System.Net.Mail;

namespace TinyTrack.Business;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class ProfilManager
{
    private readonly ProfilDal _profilDal = new();

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Kullanici? KullaniciProfilBilgileriniGetir(string kullaniciID)
    {
        return _profilDal.KullaniciyiIdIleGetir(kullaniciID);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool ProfilGuncelle(Kullanici kullanici)
    {
        if (string.IsNullOrWhiteSpace(kullanici.AdSoyad) || string.IsNullOrWhiteSpace(kullanici.Email))
        {
            throw new BusinessRuleException("Ad soyad ve e-posta zorunludur.");
        }

        if (!IsValidEmail(kullanici.Email))
        {
            throw new BusinessRuleException("GeÃ§erli bir e-posta adresi girilmelidir.");
        }

        return _profilDal.KullaniciyiGuncelle(kullanici);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Kullanici? SistemeGirisYap(string email, string sifre)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(sifre))
        {
            throw new BusinessRuleException("E-posta ve ÅŸifre zorunludur.");
        }

        return _profilDal.CheckLogin(email, sifre);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool SifreDegistir(string kullaniciID, string yeniSifre)
    {
        if (yeniSifre.Length < 6)
        {
            throw new BusinessRuleException("Åifre en az 6 karakter olmalÄ±dÄ±r.");
        }

        return _profilDal.SifreyiGuncelle(kullaniciID, yeniSifre);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool SifreDegistir(string kullaniciID, string mevcutSifre, string yeniSifre, string yeniSifreTekrar)
    {
        if (string.IsNullOrWhiteSpace(mevcutSifre) || string.IsNullOrWhiteSpace(yeniSifre) || string.IsNullOrWhiteSpace(yeniSifreTekrar))
        {
            throw new BusinessRuleException("Mevcut ÅŸifre, yeni ÅŸifre ve tekrar alanÄ± zorunludur.");
        }

        if (yeniSifre != yeniSifreTekrar)
        {
            throw new BusinessRuleException("Yeni ÅŸifre ve tekrar alanÄ± aynÄ± olmalÄ±dÄ±r.");
        }

        var kullanici = _profilDal.KullaniciyiIdIleGetir(kullaniciID)
            ?? throw new BusinessRuleException("KullanÄ±cÄ± bulunamadÄ±.");
        if (kullanici.Sifre != mevcutSifre)
        {
            throw new BusinessRuleException("Mevcut ÅŸifre hatalÄ±.");
        }

        return SifreDegistir(kullaniciID, yeniSifre);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool YetkiKontrolEt(string kullaniciID, string modulAdi)
    {
        var rol = _profilDal.KullaniciYetkiRolunuGetir(kullaniciID);
        return rol == "Yonetici" || modulAdi is "Dashboard" or "Rezervasyon";
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public IsletmeAyarlari IsletmeAyarlariGetir()
    {
        return _profilDal.SistemAyarlariniGetir();
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool IsletmeAyarlariGuncelle(IsletmeAyarlari ayarlar)
    {
        if (string.IsNullOrWhiteSpace(ayarlar.IsletmeAdi))
        {
            throw new BusinessRuleException("Ä°ÅŸletme adÄ± zorunludur.");
        }

        return _profilDal.SistemAyarlariniGuncelle(ayarlar);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
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
