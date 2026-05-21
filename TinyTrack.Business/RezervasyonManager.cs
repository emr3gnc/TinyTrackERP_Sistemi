using TinyTrack.DataAccess;
using TinyTrack.Entities;

namespace TinyTrack.Business;

public class RezervasyonManager
{
    private readonly RezervasyonDal _rezervasyonDal = new();
    private readonly VarlikDal _varlikDal = new();
    private readonly MusteriDal _musteriDal = new();
    private readonly HizmetDal _hizmetDal = new();
    private readonly OperasyonDal _operasyonDal = new();

    public List<Rezervasyon> RezervasyonlariGetir()
    {
        return _rezervasyonDal.GetAll();
    }

    public Rezervasyon? RezervasyonGetir(string rezervasyonID)
    {
        return _rezervasyonDal.GetById(rezervasyonID);
    }

    public bool RezervasyonEkle(Rezervasyon rezervasyon)
    {
        Validate(rezervasyon);
        if (CakismayiKontrolEt(rezervasyon.VarlikID, rezervasyon.BasTarih, rezervasyon.SonTarih))
        {
            throw new BusinessRuleException("Secilen tarih araliginda bu varlik icin aktif rezervasyon var.");
        }

        rezervasyon.RezervasyonID = string.IsNullOrWhiteSpace(rezervasyon.RezervasyonID)
            ? IdGenerator.NewId("REZ")
            : rezervasyon.RezervasyonID;
        rezervasyon.Durum = RezervasyonDurumu.Aktif;
        rezervasyon.ToplamUcret = ToplamUcretHesapla(rezervasyon.VarlikID, rezervasyon.BasTarih, rezervasyon.SonTarih, 0);
        rezervasyon.KayitTarihi = DateTime.Now;

        var result = _rezervasyonDal.Insert(rezervasyon);
        _varlikDal.UpdateDurum(rezervasyon.VarlikID, VarlikDurumu.Dolu);
        return result;
    }

    public bool RezervasyonGuncelle(Rezervasyon rezervasyon)
    {
        if (string.IsNullOrWhiteSpace(rezervasyon.RezervasyonID))
        {
            throw new BusinessRuleException("Guncellenecek rezervasyon secilmelidir.");
        }

        var eskiRezervasyon = _rezervasyonDal.GetById(rezervasyon.RezervasyonID)
            ?? throw new BusinessRuleException("Rezervasyon bulunamadi.");
        Validate(rezervasyon);
        if (_rezervasyonDal.HasOverlap(rezervasyon.VarlikID, rezervasyon.BasTarih, rezervasyon.SonTarih, rezervasyon.RezervasyonID))
        {
            throw new BusinessRuleException("Secilen tarih araliginda bu varlik icin baska aktif rezervasyon var.");
        }

        var hizmetToplam = _hizmetDal.GetToplamHizmetUcreti(rezervasyon.RezervasyonID);
        rezervasyon.ToplamUcret = ToplamUcretHesapla(rezervasyon.VarlikID, rezervasyon.BasTarih, rezervasyon.SonTarih, hizmetToplam);
        var result = _rezervasyonDal.Update(rezervasyon);
        if (result)
        {
            if (eskiRezervasyon.VarlikID != rezervasyon.VarlikID)
            {
                _varlikDal.UpdateDurum(eskiRezervasyon.VarlikID, VarlikDurumu.Musait);
            }

            _varlikDal.UpdateDurum(rezervasyon.VarlikID, rezervasyon.Durum == RezervasyonDurumu.Aktif ? VarlikDurumu.Dolu : VarlikDurumu.Musait);
        }

        return result;
    }

    public bool RezervasyonIptal(string rezervasyonID)
    {
        var rezervasyon = _rezervasyonDal.GetById(rezervasyonID)
            ?? throw new BusinessRuleException("Rezervasyon bulunamadi.");
        rezervasyon.Durum = RezervasyonDurumu.Iptal;
        var result = _rezervasyonDal.Update(rezervasyon);
        _varlikDal.UpdateDurum(rezervasyon.VarlikID, VarlikDurumu.Musait);
        return result;
    }

    public bool RezervasyonSil(string rezervasyonID)
    {
        if (string.IsNullOrWhiteSpace(rezervasyonID))
        {
            throw new BusinessRuleException("Silinecek rezervasyon secilmelidir.");
        }

        var rezervasyon = _rezervasyonDal.GetById(rezervasyonID)
            ?? throw new BusinessRuleException("Rezervasyon bulunamadi.");
        var result = _rezervasyonDal.Delete(rezervasyonID);
        if (result && rezervasyon.Durum == RezervasyonDurumu.Aktif)
        {
            _varlikDal.UpdateDurum(rezervasyon.VarlikID, VarlikDurumu.Musait);
        }

        return result;
    }

    public bool CakismayiKontrolEt(string varlikId, DateTime bas, DateTime bit)
    {
        if (string.IsNullOrWhiteSpace(varlikId))
        {
            return false;
        }

        return _rezervasyonDal.HasOverlap(varlikId, bas, bit);
    }

    public void VarlikDurumuGuncelle(string varlikId, bool durum)
    {
        _varlikDal.UpdateDurum(varlikId, durum ? VarlikDurumu.Musait : VarlikDurumu.Dolu);
    }

    public decimal ToplamUcretHesapla(string varlikID, DateTime basTarih, DateTime sonTarih, decimal ekstraHizmetToplami)
    {
        var varlik = _varlikDal.GetById(varlikID)
            ?? throw new BusinessRuleException("Secilen varlik bulunamadi.");
        var geceSayisi = Math.Max(1, (sonTarih.Date - basTarih.Date).Days);
        return (varlik.GunlukUcret * geceSayisi) + ekstraHizmetToplami;
    }

    public List<Rezervasyon> RezervasyonAra(string anahtar)
    {
        return string.IsNullOrWhiteSpace(anahtar)
            ? _rezervasyonDal.GetAll()
            : _rezervasyonDal.Search(anahtar.Trim());
    }

    public bool CikisIslemiBaslat(string rezervasyonID)
    {
        var rezervasyon = _rezervasyonDal.GetById(rezervasyonID)
            ?? throw new BusinessRuleException("Rezervasyon bulunamadi.");
        rezervasyon.Durum = RezervasyonDurumu.Tamamlandi;
        _rezervasyonDal.Update(rezervasyon);

        _varlikDal.UpdateDurum(rezervasyon.VarlikID, VarlikDurumu.Temizlikte);
        return _operasyonDal.Insert(new Operasyon
        {
            OperasyonID = IdGenerator.NewId("OPR"),
            VarlikID = rezervasyon.VarlikID,
            OperasyonTipi = OperasyonTipi.Temizlik,
            Durum = false,
            Tarih = DateTime.Today,
            Notlar = "Konaklama cikisi sonrasi standart temizlik baslatildi."
        });
    }

    private void Validate(Rezervasyon rezervasyon)
    {
        if (_musteriDal.GetById(rezervasyon.MusteriID) is null)
        {
            throw new BusinessRuleException("Gecerli bir musteri secilmelidir.");
        }

        if (_varlikDal.GetById(rezervasyon.VarlikID) is null)
        {
            throw new BusinessRuleException("Gecerli bir varlik secilmelidir.");
        }

        if (rezervasyon.SonTarih.Date <= rezervasyon.BasTarih.Date)
        {
            throw new BusinessRuleException("Cikis tarihi giris tarihinden sonra olmalidir.");
        }
    }
}
