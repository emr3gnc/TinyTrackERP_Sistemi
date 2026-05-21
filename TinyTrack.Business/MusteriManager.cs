using TinyTrack.DataAccess;
using TinyTrack.Entities;

namespace TinyTrack.Business;

public class MusteriManager
{
    private readonly MusteriDal _musteriDal = new();

    public List<Musteri> MusterileriGetir()
    {
        return _musteriDal.GetAll();
    }

    public Musteri? MusteriGetir(string musteriID)
    {
        return _musteriDal.GetById(musteriID);
    }

    public bool MusteriEkle(Musteri musteri)
    {
        Validate(musteri);
        if (!KimlikDogrula(musteri.KimlikNo))
        {
            throw new BusinessRuleException("T.C. kimlik numarasi gecersiz.");
        }

        if (_musteriDal.KimlikNoExists(musteri.KimlikNo))
        {
            throw new BusinessRuleException("Bu T.C. kimlik numarasi ile kayitli bir musteri zaten var.");
        }

        musteri.MusteriID = string.IsNullOrWhiteSpace(musteri.MusteriID)
            ? IdGenerator.NewId("MUS")
            : musteri.MusteriID;
        musteri.KayitTarihi = DateTime.Now;
        return _musteriDal.Insert(musteri);
    }

    public bool MusteriGuncelle(Musteri musteri)
    {
        if (string.IsNullOrWhiteSpace(musteri.MusteriID))
        {
            throw new BusinessRuleException("Guncellenecek musteri secilmelidir.");
        }

        Validate(musteri);
        if (!KimlikDogrula(musteri.KimlikNo))
        {
            throw new BusinessRuleException("T.C. kimlik numarasi gecersiz.");
        }

        if (_musteriDal.KimlikNoExists(musteri.KimlikNo, musteri.MusteriID))
        {
            throw new BusinessRuleException("Bu T.C. kimlik numarasi baska bir musteride kullaniliyor.");
        }

        return _musteriDal.Update(musteri);
    }

    public bool MusteriSil(string musteriID)
    {
        if (string.IsNullOrWhiteSpace(musteriID))
        {
            throw new BusinessRuleException("Silinecek musteri secilmelidir.");
        }

        return _musteriDal.Delete(musteriID);
    }

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

    public List<Musteri> MusteriAra(string anahtar)
    {
        return string.IsNullOrWhiteSpace(anahtar)
            ? _musteriDal.GetAll()
            : _musteriDal.Search(anahtar.Trim());
    }

    private static void Validate(Musteri musteri)
    {
        if (string.IsNullOrWhiteSpace(musteri.Ad) || string.IsNullOrWhiteSpace(musteri.Soyad))
        {
            throw new BusinessRuleException("Musteri adi ve soyadi zorunludur.");
        }

        if (musteri.Telefon.Length < 10 || musteri.Telefon.Any(c => !char.IsDigit(c)))
        {
            throw new BusinessRuleException("Telefon numarasi en az 10 haneli ve sayisal olmalidir.");
        }

        if (string.IsNullOrWhiteSpace(musteri.Adres))
        {
            throw new BusinessRuleException("Adres alani bos gecilemez.");
        }
    }
}
