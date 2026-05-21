using TinyTrack.DataAccess;
using TinyTrack.Entities;

namespace TinyTrack.Business;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class OdemeManager
{
    private readonly OdemeDal _odemeDal = new();
    private readonly RezervasyonDal _rezervasyonDal = new();

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Odeme> OdemeleriGetir()
    {
        return _odemeDal.TumunuGetir();
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Odeme? OdemeGetir(string odemeID)
    {
        return _odemeDal.IdIleGetir(odemeID);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool OdemeEkle(Odeme odeme)
    {
        Dogrula(odeme);
        var rezervasyon = _rezervasyonDal.IdIleGetir(odeme.RezervasyonID)
            ?? throw new BusinessRuleException("Ã–deme yapÄ±lacak rezervasyon bulunamadÄ±.");

        var kalan = rezervasyon.ToplamUcret - _rezervasyonDal.OdenenTutariGetir(odeme.RezervasyonID);
        if (odeme.Ucret > kalan)
        {
            throw new BusinessRuleException($"Ã–deme tutarÄ± kalan tutarÄ± aÅŸamaz. Kalan: {kalan:C2}");
        }

        odeme.OdemeID = string.IsNullOrWhiteSpace(odeme.OdemeID)
            ? IdGenerator.YeniId("ODE")
            : odeme.OdemeID;
        return _odemeDal.Ekle(odeme);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool OdemeGuncelle(Odeme odeme)
    {
        if (string.IsNullOrWhiteSpace(odeme.OdemeID))
        {
            throw new BusinessRuleException("GÃ¼ncellenecek Ã¶deme seÃ§ilmelidir.");
        }

        Dogrula(odeme);
        var mevcutOdeme = _odemeDal.IdIleGetir(odeme.OdemeID)
            ?? throw new BusinessRuleException("Ã–deme bulunamadÄ±.");
        var rezervasyon = _rezervasyonDal.IdIleGetir(odeme.RezervasyonID)
            ?? throw new BusinessRuleException("Ã–deme yapÄ±lacak rezervasyon bulunamadÄ±.");
        var odenenDigerTutar = _rezervasyonDal.OdenenTutariGetir(odeme.RezervasyonID);
        if (mevcutOdeme.RezervasyonID == odeme.RezervasyonID)
        {
            odenenDigerTutar -= mevcutOdeme.Ucret;
        }

        var kalan = rezervasyon.ToplamUcret - odenenDigerTutar;
        if (odeme.Ucret > kalan)
        {
            throw new BusinessRuleException($"Ã–deme tutarÄ± kalan tutarÄ± aÅŸamaz. Kalan: {kalan:C2}");
        }

        return _odemeDal.Guncelle(odeme);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool OdemeSil(string odemeID)
    {
        if (string.IsNullOrWhiteSpace(odemeID))
        {
            throw new BusinessRuleException("Silinecek Ã¶deme seÃ§ilmelidir.");
        }

        return _odemeDal.Sil(odemeID);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public decimal KalanTutarGetir(string rezervasyonID)
    {
        var rezervasyon = _rezervasyonDal.IdIleGetir(rezervasyonID)
            ?? throw new BusinessRuleException("Rezervasyon bulunamadÄ±.");
        return rezervasyon.ToplamUcret - _rezervasyonDal.OdenenTutariGetir(rezervasyonID);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static void Dogrula(Odeme odeme)
    {
        if (string.IsNullOrWhiteSpace(odeme.RezervasyonID))
        {
            throw new BusinessRuleException("Rezervasyon seÃ§ilmelidir.");
        }

        if (odeme.Ucret <= 0)
        {
            throw new BusinessRuleException("Ã–deme tutarÄ± 0'dan bÃ¼yÃ¼k olmalÄ±dÄ±r.");
        }
    }
}
