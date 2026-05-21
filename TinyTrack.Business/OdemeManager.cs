using TinyTrack.DataAccess;
using TinyTrack.Entities;

namespace TinyTrack.Business;

public class OdemeManager
{
    private readonly OdemeDal _odemeDal = new();
    private readonly RezervasyonDal _rezervasyonDal = new();

    public List<Odeme> OdemeleriGetir()
    {
        return _odemeDal.GetAll();
    }

    public Odeme? OdemeGetir(string odemeID)
    {
        return _odemeDal.GetById(odemeID);
    }

    public bool OdemeEkle(Odeme odeme)
    {
        Validate(odeme);
        var rezervasyon = _rezervasyonDal.GetById(odeme.RezervasyonID)
            ?? throw new BusinessRuleException("Odeme yapilacak rezervasyon bulunamadi.");

        var kalan = rezervasyon.ToplamUcret - _rezervasyonDal.GetOdenenTutar(odeme.RezervasyonID);
        if (odeme.Ucret > kalan)
        {
            throw new BusinessRuleException($"Odeme tutari kalan tutari asamaz. Kalan: {kalan:C2}");
        }

        odeme.OdemeID = string.IsNullOrWhiteSpace(odeme.OdemeID)
            ? IdGenerator.NewId("ODE")
            : odeme.OdemeID;
        return _odemeDal.Insert(odeme);
    }

    public bool OdemeGuncelle(Odeme odeme)
    {
        if (string.IsNullOrWhiteSpace(odeme.OdemeID))
        {
            throw new BusinessRuleException("Guncellenecek odeme secilmelidir.");
        }

        Validate(odeme);
        var mevcutOdeme = _odemeDal.GetById(odeme.OdemeID)
            ?? throw new BusinessRuleException("Odeme bulunamadi.");
        var rezervasyon = _rezervasyonDal.GetById(odeme.RezervasyonID)
            ?? throw new BusinessRuleException("Odeme yapilacak rezervasyon bulunamadi.");
        var odenenDigerTutar = _rezervasyonDal.GetOdenenTutar(odeme.RezervasyonID);
        if (mevcutOdeme.RezervasyonID == odeme.RezervasyonID)
        {
            odenenDigerTutar -= mevcutOdeme.Ucret;
        }

        var kalan = rezervasyon.ToplamUcret - odenenDigerTutar;
        if (odeme.Ucret > kalan)
        {
            throw new BusinessRuleException($"Odeme tutari kalan tutari asamaz. Kalan: {kalan:C2}");
        }

        return _odemeDal.Update(odeme);
    }

    public bool OdemeSil(string odemeID)
    {
        if (string.IsNullOrWhiteSpace(odemeID))
        {
            throw new BusinessRuleException("Silinecek odeme secilmelidir.");
        }

        return _odemeDal.Delete(odemeID);
    }

    public decimal KalanTutarGetir(string rezervasyonID)
    {
        var rezervasyon = _rezervasyonDal.GetById(rezervasyonID)
            ?? throw new BusinessRuleException("Rezervasyon bulunamadi.");
        return rezervasyon.ToplamUcret - _rezervasyonDal.GetOdenenTutar(rezervasyonID);
    }

    private static void Validate(Odeme odeme)
    {
        if (string.IsNullOrWhiteSpace(odeme.RezervasyonID))
        {
            throw new BusinessRuleException("Rezervasyon secilmelidir.");
        }

        if (odeme.Ucret <= 0)
        {
            throw new BusinessRuleException("Odeme tutari 0'dan buyuk olmalidir.");
        }
    }
}
