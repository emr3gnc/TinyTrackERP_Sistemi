using TinyTrack.DataAccess;
using TinyTrack.Entities;

namespace TinyTrack.Business;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class VarlikManager
{
    private readonly VarlikDal _varlikDal = new();
    private readonly OperasyonDal _operasyonDal = new();

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Varlik> VarliklariGetir()
    {
        return _varlikDal.TumunuGetir();
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Varlik? VarlikGetir(string varlikID)
    {
        return _varlikDal.IdIleGetir(varlikID);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool VarlikEkle(Varlik varlik)
    {
        Dogrula(varlik);
        varlik.VarlikID = string.IsNullOrWhiteSpace(varlik.VarlikID)
            ? IdGenerator.YeniId("VAR")
            : varlik.VarlikID;
        var sonuc = _varlikDal.Ekle(varlik);
        if (sonuc)
        {
            OperasyonGerekiyorsaOlustur(varlik.VarlikID, varlik.Durum, null);
        }

        return sonuc;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool VarlikGuncelle(Varlik varlik)
    {
        if (string.IsNullOrWhiteSpace(varlik.VarlikID))
        {
            throw new BusinessRuleException("GÃ¼ncellenecek varlÄ±k seÃ§ilmelidir.");
        }

        var eskiVarlik = _varlikDal.IdIleGetir(varlik.VarlikID);
        Dogrula(varlik);
        var sonuc = _varlikDal.Guncelle(varlik);
        if (sonuc)
        {
            OperasyonGerekiyorsaOlustur(varlik.VarlikID, varlik.Durum, eskiVarlik?.Durum);
        }

        return sonuc;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool VarlikSil(string varlikID)
    {
        if (string.IsNullOrWhiteSpace(varlikID))
        {
            throw new BusinessRuleException("Silinecek varlÄ±k seÃ§ilmelidir.");
        }

        var rezervasyonSayisi = _varlikDal.RezervasyonSayisiniGetir(varlikID);
        var operasyonSayisi = _varlikDal.OperasyonSayisiniGetir(varlikID);
        if (rezervasyonSayisi > 0 || operasyonSayisi > 0)
        {
            throw new BusinessRuleException(
                $"Bu varlÄ±k silinemez. VarlÄ±ÄŸa baÄŸlÄ± {rezervasyonSayisi} rezervasyon ve {operasyonSayisi} operasyon kaydÄ± var. " +
                "GeÃ§miÅŸ kayÄ±tlarÄ±n bozulmamasÄ± iÃ§in varlÄ±ÄŸÄ± silmek yerine durumunu BakÄ±mda veya MÃ¼sait olarak gÃ¼ncelleyin.");
        }

        return _varlikDal.Sil(varlikID);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool VarlikDurumuGuncelle(string varlikID, VarlikDurumu durum)
    {
        if (string.IsNullOrWhiteSpace(varlikID))
        {
            throw new BusinessRuleException("VarlÄ±k seÃ§ilmelidir.");
        }

        var eskiVarlik = _varlikDal.IdIleGetir(varlikID);
        var sonuc = _varlikDal.DurumuGuncelle(varlikID, durum);
        if (sonuc)
        {
            OperasyonGerekiyorsaOlustur(varlikID, durum, eskiVarlik?.Durum);
        }

        return sonuc;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Varlik> VarlikAra(string anahtar)
    {
        return string.IsNullOrWhiteSpace(anahtar)
            ? _varlikDal.TumunuGetir()
            : _varlikDal.Ara(anahtar.Trim());
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static void Dogrula(Varlik varlik)
    {
        if (string.IsNullOrWhiteSpace(varlik.Ad) || string.IsNullOrWhiteSpace(varlik.VarlikTipi))
        {
            throw new BusinessRuleException("VarlÄ±k adÄ± ve tipi zorunludur.");
        }

        if (varlik.Kapasite <= 0)
        {
            throw new BusinessRuleException("Kapasite 0'dan bÃ¼yÃ¼k olmalÄ±dÄ±r.");
        }

        if (varlik.GunlukUcret <= 0)
        {
            throw new BusinessRuleException("GÃ¼nlÃ¼k Ã¼cret 0'dan bÃ¼yÃ¼k olmalÄ±dÄ±r.");
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
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

        _operasyonDal.Ekle(new Operasyon
        {
            OperasyonID = IdGenerator.YeniId("OPR"),
            VarlikID = varlikID,
            OperasyonTipi = operasyonTipi.Value,
            Durum = false,
            Tarih = DateTime.Today,
            Notlar = yeniDurum == VarlikDurumu.Temizlikte
                ? "VarlÄ±k durumu Temizlikte olarak gÃ¼ncellendi."
                : "VarlÄ±k durumu BakÄ±mda olarak gÃ¼ncellendi."
        });
    }
}
