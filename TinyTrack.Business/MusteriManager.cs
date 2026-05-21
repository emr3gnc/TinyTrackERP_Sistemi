using TinyTrack.DataAccess;
using TinyTrack.Entities;

namespace TinyTrack.Business;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class MusteriManager
{
    private readonly MusteriDal _musteriDal = new();

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Musteri> MusterileriGetir()
    {
        return _musteriDal.TumunuGetir();
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Musteri? MusteriGetir(string musteriID)
    {
        return _musteriDal.IdIleGetir(musteriID);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Musteri? MusteriKimlikNoIleGetir(string kimlikNo)
    {
        return string.IsNullOrWhiteSpace(kimlikNo) ? null : _musteriDal.KimlikNoIleGetir(kimlikNo.Trim());
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Musteri MusteriBulVeyaEkle(Musteri musteri)
    {
        Dogrula(musteri);
        if (!KimlikDogrula(musteri.KimlikNo))
        {
            throw new BusinessRuleException("T.C. kimlik numarası geçersiz.");
        }

        var mevcutMusteri = _musteriDal.KimlikNoIleGetir(musteri.KimlikNo);
        if (mevcutMusteri is not null)
        {
            return mevcutMusteri;
        }

        musteri.MusteriID = string.IsNullOrWhiteSpace(musteri.MusteriID)
            ? IdGenerator.YeniId("MUS")
            : musteri.MusteriID;
        musteri.KayitTarihi = DateTime.Now;
        _musteriDal.Ekle(musteri);
        return musteri;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool MusteriEkle(Musteri musteri)
    {
        Dogrula(musteri);
        if (!KimlikDogrula(musteri.KimlikNo))
        {
            throw new BusinessRuleException("T.C. kimlik numarası geçersiz.");
        }

        if (_musteriDal.KimlikNoExists(musteri.KimlikNo))
        {
            throw new BusinessRuleException("Bu T.C. kimlik numarası ile kayıtlı bir müşteri zaten var.");
        }

        musteri.MusteriID = string.IsNullOrWhiteSpace(musteri.MusteriID)
            ? IdGenerator.YeniId("MUS")
            : musteri.MusteriID;
        musteri.KayitTarihi = DateTime.Now;
        return _musteriDal.Ekle(musteri);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool MusteriGuncelle(Musteri musteri)
    {
        if (string.IsNullOrWhiteSpace(musteri.MusteriID))
        {
            throw new BusinessRuleException("Güncellenecek müşteri seçilmelidir.");
        }

        Dogrula(musteri);
        if (!KimlikDogrula(musteri.KimlikNo))
        {
            throw new BusinessRuleException("T.C. kimlik numarası geçersiz.");
        }

        if (_musteriDal.KimlikNoExists(musteri.KimlikNo, musteri.MusteriID))
        {
            throw new BusinessRuleException("Bu T.C. kimlik numarası başka bir müşteride kullanılıyor.");
        }

        return _musteriDal.Guncelle(musteri);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool MusteriSil(string musteriID)
    {
        if (string.IsNullOrWhiteSpace(musteriID))
        {
            throw new BusinessRuleException("Silinecek müşteri seçilmelidir.");
        }

        return _musteriDal.Sil(musteriID);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool KimlikDogrula(string kimlikNo)
    {
        if (kimlikNo.Length != 11 || kimlikNo.Any(c => !char.IsDigit(c)) || kimlikNo[0] == '0')
        {
            return false;
        }

        var digits = kimlikNo.Select(c => c - '0').ToArray();
        var oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        var evenSum = digits[1] + digits[3] + digits[5] + digits[7];
        var tenthDigit = ((oddSum * 7) - evenSum) % 10;
        var eleventhDigit = digits.Take(10).Sum() % 10;
        return digits[9] == tenthDigit && digits[10] == eleventhDigit;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Musteri> MusteriAra(string anahtar)
    {
        return string.IsNullOrWhiteSpace(anahtar)
            ? _musteriDal.TumunuGetir()
            : _musteriDal.Ara(anahtar.Trim());
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static void Dogrula(Musteri musteri)
    {
        if (string.IsNullOrWhiteSpace(musteri.Ad) || string.IsNullOrWhiteSpace(musteri.Soyad))
        {
            throw new BusinessRuleException("Müşteri adı ve soyadı zorunludur.");
        }

        if (musteri.Telefon.Length < 10 || musteri.Telefon.Any(c => !char.IsDigit(c)))
        {
            throw new BusinessRuleException("Telefon numarası en az 10 haneli ve sayısal olmalıdır.");
        }

        if (string.IsNullOrWhiteSpace(musteri.Il) || string.IsNullOrWhiteSpace(musteri.Ilce))
        {
            throw new BusinessRuleException("İl ve ilçe seçilmelidir.");
        }

        if (string.IsNullOrWhiteSpace(musteri.AcikAdres))
        {
            throw new BusinessRuleException("Açık adres alanı boş geçilemez.");
        }
    }
}
