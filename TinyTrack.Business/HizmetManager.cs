using TinyTrack.DataAccess;
using TinyTrack.Entities;

namespace TinyTrack.Business;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class HizmetManager
{
    private readonly HizmetDal _hizmetDal = new();
    private readonly RezervasyonDal _rezervasyonDal = new();

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Hizmet> HizmetleriGetir()
    {
        return _hizmetDal.TumunuGetir();
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Hizmet> RezervasyonHizmetleriGetir(string rezervasyonID)
    {
        return _hizmetDal.RezervasyonaGoreGetir(rezervasyonID);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool HizmetEkle(Hizmet hizmet)
    {
        Dogrula(hizmet);
        hizmet.HizmetID = string.IsNullOrWhiteSpace(hizmet.HizmetID)
            ? IdGenerator.YeniId("HIZ")
            : hizmet.HizmetID;
        var sonuc = _hizmetDal.Ekle(hizmet);
        RezervasyonToplaminiYenile(hizmet.RezervasyonID);
        return sonuc;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool HizmetGuncelle(Hizmet hizmet)
    {
        Dogrula(hizmet);
        if (string.IsNullOrWhiteSpace(hizmet.HizmetID))
        {
            throw new BusinessRuleException("GÃ¼ncellenecek hizmet seÃ§ilmelidir.");
        }

        var sonuc = _hizmetDal.Guncelle(hizmet);
        RezervasyonToplaminiYenile(hizmet.RezervasyonID);
        return sonuc;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool HizmetSil(string hizmetID)
    {
        var hizmet = _hizmetDal.IdIleGetir(hizmetID)
            ?? throw new BusinessRuleException("Hizmet bulunamadÄ±.");
        var sonuc = _hizmetDal.Sil(hizmetID);
        RezervasyonToplaminiYenile(hizmet.RezervasyonID);
        return sonuc;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void RezervasyonToplaminiYenile(string rezervasyonID)
    {
        var rezervasyon = _rezervasyonDal.IdIleGetir(rezervasyonID);
        if (rezervasyon is null)
        {
            return;
        }

        var varlik = new VarlikDal().IdIleGetir(rezervasyon.VarlikID);
        if (varlik is null)
        {
            return;
        }

        var gece = Math.Max(1, (rezervasyon.SonTarih.Date - rezervasyon.BasTarih.Date).Days);
        rezervasyon.ToplamUcret = (varlik.GunlukUcret * gece) + _hizmetDal.ToplamHizmetUcretiniGetir(rezervasyonID);
        _rezervasyonDal.Guncelle(rezervasyon);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static void Dogrula(Hizmet hizmet)
    {
        if (string.IsNullOrWhiteSpace(hizmet.RezervasyonID))
        {
            throw new BusinessRuleException("Hizmet iÃ§in rezervasyon seÃ§ilmelidir.");
        }

        if (string.IsNullOrWhiteSpace(hizmet.Ad))
        {
            throw new BusinessRuleException("Hizmet adÄ± zorunludur.");
        }

        if (hizmet.Ucret < 0)
        {
            throw new BusinessRuleException("Hizmet ucreti negatif olamaz.");
        }
    }
}
