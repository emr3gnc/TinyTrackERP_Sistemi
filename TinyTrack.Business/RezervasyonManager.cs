using TinyTrack.DataAccess;
using TinyTrack.Entities;

namespace TinyTrack.Business;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class RezervasyonManager
{
    private readonly RezervasyonDal _rezervasyonDal = new();
    private readonly VarlikDal _varlikDal = new();
    private readonly MusteriDal _musteriDal = new();
    private readonly HizmetDal _hizmetDal = new();
    private readonly OperasyonDal _operasyonDal = new();

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Rezervasyon> RezervasyonlariGetir()
    {
        return _rezervasyonDal.TumunuGetir();
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Rezervasyon? RezervasyonGetir(string rezervasyonID)
    {
        return _rezervasyonDal.IdIleGetir(rezervasyonID);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool RezervasyonEkle(Rezervasyon rezervasyon)
    {
        Dogrula(rezervasyon);
        if (CakismayiKontrolEt(rezervasyon.VarlikID, rezervasyon.BasTarih, rezervasyon.SonTarih))
        {
            throw new BusinessRuleException("SeÃ§ilen tarih aralÄ±ÄŸÄ±nda bu varlÄ±k iÃ§in aktif rezervasyon var.");
        }

        rezervasyon.RezervasyonID = string.IsNullOrWhiteSpace(rezervasyon.RezervasyonID)
            ? IdGenerator.YeniId("REZ")
            : rezervasyon.RezervasyonID;
        rezervasyon.Durum = RezervasyonDurumu.Aktif;
        rezervasyon.ToplamUcret = ToplamUcretHesapla(rezervasyon.VarlikID, rezervasyon.BasTarih, rezervasyon.SonTarih, 0);
        rezervasyon.KayitTarihi = DateTime.Now;

        var sonuc = _rezervasyonDal.Ekle(rezervasyon);
        _varlikDal.DurumuGuncelle(rezervasyon.VarlikID, VarlikDurumu.Dolu);
        return sonuc;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool RezervasyonGuncelle(Rezervasyon rezervasyon)
    {
        if (string.IsNullOrWhiteSpace(rezervasyon.RezervasyonID))
        {
            throw new BusinessRuleException("GÃ¼ncellenecek rezervasyon seÃ§ilmelidir.");
        }

        var eskiRezervasyon = _rezervasyonDal.IdIleGetir(rezervasyon.RezervasyonID)
            ?? throw new BusinessRuleException("Rezervasyon bulunamadÄ±.");
        Dogrula(rezervasyon);
        if (_rezervasyonDal.HasOverlap(rezervasyon.VarlikID, rezervasyon.BasTarih, rezervasyon.SonTarih, rezervasyon.RezervasyonID))
        {
            throw new BusinessRuleException("SeÃ§ilen tarih aralÄ±ÄŸÄ±nda bu varlÄ±k iÃ§in baÅŸka aktif rezervasyon var.");
        }

        var hizmetToplam = _hizmetDal.ToplamHizmetUcretiniGetir(rezervasyon.RezervasyonID);
        rezervasyon.ToplamUcret = ToplamUcretHesapla(rezervasyon.VarlikID, rezervasyon.BasTarih, rezervasyon.SonTarih, hizmetToplam);
        var sonuc = _rezervasyonDal.Guncelle(rezervasyon);
        if (sonuc)
        {
            if (eskiRezervasyon.VarlikID != rezervasyon.VarlikID)
            {
                _varlikDal.DurumuGuncelle(eskiRezervasyon.VarlikID, VarlikDurumu.Musait);
            }

            _varlikDal.DurumuGuncelle(rezervasyon.VarlikID, rezervasyon.Durum == RezervasyonDurumu.Aktif ? VarlikDurumu.Dolu : VarlikDurumu.Musait);
        }

        return sonuc;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool RezervasyonIptal(string rezervasyonID)
    {
        var rezervasyon = _rezervasyonDal.IdIleGetir(rezervasyonID)
            ?? throw new BusinessRuleException("Rezervasyon bulunamadÄ±.");
        rezervasyon.Durum = RezervasyonDurumu.Iptal;
        var sonuc = _rezervasyonDal.Guncelle(rezervasyon);
        _varlikDal.DurumuGuncelle(rezervasyon.VarlikID, VarlikDurumu.Musait);
        return sonuc;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool RezervasyonSil(string rezervasyonID)
    {
        if (string.IsNullOrWhiteSpace(rezervasyonID))
        {
            throw new BusinessRuleException("Silinecek rezervasyon seÃ§ilmelidir.");
        }

        var rezervasyon = _rezervasyonDal.IdIleGetir(rezervasyonID)
            ?? throw new BusinessRuleException("Rezervasyon bulunamadÄ±.");
        var sonuc = _rezervasyonDal.Sil(rezervasyonID);
        if (sonuc && rezervasyon.Durum == RezervasyonDurumu.Aktif)
        {
            _varlikDal.DurumuGuncelle(rezervasyon.VarlikID, VarlikDurumu.Musait);
        }

        return sonuc;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool CakismayiKontrolEt(string varlikId, DateTime bas, DateTime bit)
    {
        if (string.IsNullOrWhiteSpace(varlikId))
        {
            return false;
        }

        return _rezervasyonDal.HasOverlap(varlikId, bas, bit);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public void VarlikDurumuGuncelle(string varlikId, bool durum)
    {
        _varlikDal.DurumuGuncelle(varlikId, durum ? VarlikDurumu.Musait : VarlikDurumu.Dolu);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public decimal ToplamUcretHesapla(string varlikID, DateTime basTarih, DateTime sonTarih, decimal ekstraHizmetToplami)
    {
        var varlik = _varlikDal.IdIleGetir(varlikID)
            ?? throw new BusinessRuleException("SeÃ§ilen varlÄ±k bulunamadÄ±.");
        var geceSayisi = Math.Max(1, (sonTarih.Date - basTarih.Date).Days);
        return (varlik.GunlukUcret * geceSayisi) + ekstraHizmetToplami;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Rezervasyon> RezervasyonAra(string anahtar)
    {
        return string.IsNullOrWhiteSpace(anahtar)
            ? _rezervasyonDal.TumunuGetir()
            : _rezervasyonDal.Ara(anahtar.Trim());
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool CikisIslemiBaslat(string rezervasyonID)
    {
        var rezervasyon = _rezervasyonDal.IdIleGetir(rezervasyonID)
            ?? throw new BusinessRuleException("Rezervasyon bulunamadÄ±.");
        rezervasyon.Durum = RezervasyonDurumu.Tamamlandi;
        _rezervasyonDal.Guncelle(rezervasyon);

        _varlikDal.DurumuGuncelle(rezervasyon.VarlikID, VarlikDurumu.Temizlikte);
        return _operasyonDal.Ekle(new Operasyon
        {
            OperasyonID = IdGenerator.YeniId("OPR"),
            VarlikID = rezervasyon.VarlikID,
            OperasyonTipi = OperasyonTipi.Temizlik,
            Durum = false,
            Tarih = DateTime.Today,
            Notlar = "Konaklama cikisi sonrasi standart temizlik baslatildi."
        });
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void Dogrula(Rezervasyon rezervasyon)
    {
        if (_musteriDal.IdIleGetir(rezervasyon.MusteriID) is null)
        {
            throw new BusinessRuleException("GeÃ§erli bir mÃ¼ÅŸteri seÃ§ilmelidir.");
        }

        if (_varlikDal.IdIleGetir(rezervasyon.VarlikID) is null)
        {
            throw new BusinessRuleException("GeÃ§erli bir varlÄ±k seÃ§ilmelidir.");
        }

        if (rezervasyon.SonTarih.Date <= rezervasyon.BasTarih.Date)
        {
            throw new BusinessRuleException("Ã‡Ä±kÄ±ÅŸ tarihi giriÅŸ tarihinden sonra olmalÄ±dÄ±r.");
        }
    }
}
