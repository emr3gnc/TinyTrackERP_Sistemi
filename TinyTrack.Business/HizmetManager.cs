using TinyTrack.DataAccess;
using TinyTrack.Entities;

namespace TinyTrack.Business;

public class HizmetManager
{
    private readonly HizmetDal _hizmetDal = new();
    private readonly RezervasyonDal _rezervasyonDal = new();

    public List<Hizmet> HizmetleriGetir()
    {
        return _hizmetDal.GetAll();
    }

    public List<Hizmet> RezervasyonHizmetleriGetir(string rezervasyonID)
    {
        return _hizmetDal.GetByRezervasyon(rezervasyonID);
    }

    public bool HizmetEkle(Hizmet hizmet)
    {
        Validate(hizmet);
        hizmet.HizmetID = string.IsNullOrWhiteSpace(hizmet.HizmetID)
            ? IdGenerator.NewId("HIZ")
            : hizmet.HizmetID;
        var result = _hizmetDal.Insert(hizmet);
        RezervasyonToplaminiYenile(hizmet.RezervasyonID);
        return result;
    }

    public bool HizmetGuncelle(Hizmet hizmet)
    {
        Validate(hizmet);
        if (string.IsNullOrWhiteSpace(hizmet.HizmetID))
        {
            throw new BusinessRuleException("Guncellenecek hizmet secilmelidir.");
        }

        var result = _hizmetDal.Update(hizmet);
        RezervasyonToplaminiYenile(hizmet.RezervasyonID);
        return result;
    }

    public bool HizmetSil(string hizmetID)
    {
        var hizmet = _hizmetDal.GetById(hizmetID)
            ?? throw new BusinessRuleException("Hizmet bulunamadi.");
        var result = _hizmetDal.Delete(hizmetID);
        RezervasyonToplaminiYenile(hizmet.RezervasyonID);
        return result;
    }

    private void RezervasyonToplaminiYenile(string rezervasyonID)
    {
        var rezervasyon = _rezervasyonDal.GetById(rezervasyonID);
        if (rezervasyon is null)
        {
            return;
        }

        var varlik = new VarlikDal().GetById(rezervasyon.VarlikID);
        if (varlik is null)
        {
            return;
        }

        var gece = Math.Max(1, (rezervasyon.SonTarih.Date - rezervasyon.BasTarih.Date).Days);
        rezervasyon.ToplamUcret = (varlik.GunlukUcret * gece) + _hizmetDal.GetToplamHizmetUcreti(rezervasyonID);
        _rezervasyonDal.Update(rezervasyon);
    }

    private static void Validate(Hizmet hizmet)
    {
        if (string.IsNullOrWhiteSpace(hizmet.RezervasyonID))
        {
            throw new BusinessRuleException("Hizmet icin rezervasyon secilmelidir.");
        }

        if (string.IsNullOrWhiteSpace(hizmet.Ad))
        {
            throw new BusinessRuleException("Hizmet adi zorunludur.");
        }

        if (hizmet.Ucret < 0)
        {
            throw new BusinessRuleException("Hizmet ucreti negatif olamaz.");
        }
    }
}
