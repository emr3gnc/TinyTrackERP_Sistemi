using TinyTrack.DataAccess;
using TinyTrack.Entities;

namespace TinyTrack.Business;

public class VarlikManager
{
    private readonly VarlikDal _varlikDal = new();
    private readonly OperasyonDal _operasyonDal = new();

    public List<Varlik> VarliklariGetir()
    {
        return _varlikDal.GetAll();
    }

    public Varlik? VarlikGetir(string varlikID)
    {
        return _varlikDal.GetById(varlikID);
    }

    public bool VarlikEkle(Varlik varlik)
    {
        Validate(varlik);
        varlik.VarlikID = string.IsNullOrWhiteSpace(varlik.VarlikID)
            ? IdGenerator.NewId("VAR")
            : varlik.VarlikID;
        var result = _varlikDal.Insert(varlik);
        if (result)
        {
            OperasyonGerekiyorsaOlustur(varlik.VarlikID, varlik.Durum, null);
        }

        return result;
    }

    public bool VarlikGuncelle(Varlik varlik)
    {
        if (string.IsNullOrWhiteSpace(varlik.VarlikID))
        {
            throw new BusinessRuleException("Guncellenecek varlik secilmelidir.");
        }

        var eskiVarlik = _varlikDal.GetById(varlik.VarlikID);
        Validate(varlik);
        var result = _varlikDal.Update(varlik);
        if (result)
        {
            OperasyonGerekiyorsaOlustur(varlik.VarlikID, varlik.Durum, eskiVarlik?.Durum);
        }

        return result;
    }

    public bool VarlikSil(string varlikID)
    {
        if (string.IsNullOrWhiteSpace(varlikID))
        {
            throw new BusinessRuleException("Silinecek varlik secilmelidir.");
        }

        return _varlikDal.Delete(varlikID);
    }

    public bool VarlikDurumuGuncelle(string varlikID, VarlikDurumu durum)
    {
        if (string.IsNullOrWhiteSpace(varlikID))
        {
            throw new BusinessRuleException("Varlik secilmelidir.");
        }

        var eskiVarlik = _varlikDal.GetById(varlikID);
        var result = _varlikDal.UpdateDurum(varlikID, durum);
        if (result)
        {
            OperasyonGerekiyorsaOlustur(varlikID, durum, eskiVarlik?.Durum);
        }

        return result;
    }

    public List<Varlik> VarlikAra(string anahtar)
    {
        return string.IsNullOrWhiteSpace(anahtar)
            ? _varlikDal.GetAll()
            : _varlikDal.Search(anahtar.Trim());
    }

    private static void Validate(Varlik varlik)
    {
        if (string.IsNullOrWhiteSpace(varlik.Ad) || string.IsNullOrWhiteSpace(varlik.VarlikTipi))
        {
            throw new BusinessRuleException("Varlik adi ve tipi zorunludur.");
        }

        if (varlik.Kapasite <= 0)
        {
            throw new BusinessRuleException("Kapasite 0'dan buyuk olmalidir.");
        }

        if (varlik.GunlukUcret <= 0)
        {
            throw new BusinessRuleException("Gunluk ucret 0'dan buyuk olmalidir.");
        }
    }

    private void OperasyonGerekiyorsaOlustur(string varlikID, VarlikDurumu yeniDurum, VarlikDurumu? eskiDurum)
    {
        if (eskiDurum == yeniDurum)
        {
            return;
        }

        var operasyonTipi = yeniDurum switch
        {
            VarlikDurumu.Temizlikte => OperasyonTipi.Temizlik,
            VarlikDurumu.Bakimda => OperasyonTipi.Bakim,
            _ => (OperasyonTipi?)null
        };

        if (operasyonTipi is null)
        {
            return;
        }

        _operasyonDal.Insert(new Operasyon
        {
            OperasyonID = IdGenerator.NewId("OPR"),
            VarlikID = varlikID,
            OperasyonTipi = operasyonTipi.Value,
            Durum = false,
            Tarih = DateTime.Today,
            Notlar = yeniDurum == VarlikDurumu.Temizlikte
                ? "Varlik durumu Temizlikte olarak guncellendi."
                : "Varlik durumu Bakimda olarak guncellendi."
        });
    }
}
