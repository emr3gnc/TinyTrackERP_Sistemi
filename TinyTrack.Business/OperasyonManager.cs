using TinyTrack.DataAccess;
using TinyTrack.Entities;

namespace TinyTrack.Business;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class OperasyonManager
{
    private readonly OperasyonDal _operasyonDal = new();
    private readonly VarlikDal _varlikDal = new();

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Operasyon> OperasyonlariGetir()
    {
        return _operasyonDal.TumunuGetir();
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Operasyon? OperasyonGetir(string operasyonID)
    {
        return _operasyonDal.IdIleGetir(operasyonID);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool OperasyonEkle(Operasyon operasyon)
    {
        Dogrula(operasyon);
        operasyon.OperasyonID = string.IsNullOrWhiteSpace(operasyon.OperasyonID)
            ? IdGenerator.YeniId("OPR")
            : operasyon.OperasyonID;
        var sonuc = _operasyonDal.Ekle(operasyon);
        if (sonuc)
        {
            VarlikDurumunuOperasyonaGoreAyarla(operasyon);
        }
        return sonuc;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool OperasyonGuncelle(Operasyon operasyon)
    {
        Dogrula(operasyon);
        if (string.IsNullOrWhiteSpace(operasyon.OperasyonID))
        {
            throw new BusinessRuleException("GÃ¼ncellenecek operasyon seÃ§ilmelidir.");
        }

        var sonuc = _operasyonDal.Guncelle(operasyon);
        if (sonuc)
        {
            VarlikDurumunuOperasyonaGoreAyarla(operasyon);
        }

        return sonuc;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool OperasyonTamamla(string operasyonID)
    {
        var operasyon = _operasyonDal.IdIleGetir(operasyonID)
            ?? throw new BusinessRuleException("Operasyon bulunamadÄ±.");
        operasyon.Durum = true;
        return OperasyonGuncelle(operasyon);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool OperasyonSil(string operasyonID)
    {
        if (string.IsNullOrWhiteSpace(operasyonID))
        {
            throw new BusinessRuleException("Silinecek operasyon seÃ§ilmelidir.");
        }

        var operasyon = _operasyonDal.IdIleGetir(operasyonID)
            ?? throw new BusinessRuleException("Operasyon bulunamadÄ±.");
        var sonuc = _operasyonDal.Sil(operasyonID);
        if (sonuc && !operasyon.Durum && _operasyonDal.VarligaGoreAcikSayisiniGetir(operasyon.VarlikID) == 0)
        {
            _varlikDal.DurumuGuncelle(operasyon.VarlikID, VarlikDurumu.Musait);
        }

        return sonuc;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static void Dogrula(Operasyon operasyon)
    {
        if (string.IsNullOrWhiteSpace(operasyon.VarlikID))
        {
            throw new BusinessRuleException("Operasyon iÃ§in varlÄ±k seÃ§ilmelidir.");
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void VarlikDurumunuOperasyonaGoreAyarla(Operasyon operasyon)
    {
        if (operasyon.Durum)
        {
            _varlikDal.DurumuGuncelle(operasyon.VarlikID, VarlikDurumu.Musait);
            return;
        }

        _varlikDal.DurumuGuncelle(
            operasyon.VarlikID,
            operasyon.OperasyonTipi == OperasyonTipi.Bakim ? VarlikDurumu.Bakimda : VarlikDurumu.Temizlikte);
    }
}
