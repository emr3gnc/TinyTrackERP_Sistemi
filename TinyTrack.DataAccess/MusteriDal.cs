using Microsoft.Data.Sqlite;
using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class MusteriDal
{
    private const string SelectSql = """
        SELECT musteriID, ad, soyad, telefon, adres, il, ilce, acikAdres, postaKodu, kimlikno, kayitTarihi
        FROM musteri
        """;

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Musteri> TumunuGetir()
    {
        return DbHelper.ListeCalistir($"{SelectSql} ORDER BY ad, soyad", Esle);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Musteri? IdIleGetir(string musteriID)
    {
        return DbHelper.TekKayitCalistir(
            $"{SelectSql} WHERE musteriID = @musteriID",
            Esle,
            DbHelper.Parametre("@musteriID", musteriID));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Musteri? KimlikNoIleGetir(string kimlikNo)
    {
        return DbHelper.TekKayitCalistir(
            $"{SelectSql} WHERE kimlikno = @kimlikNo",
            Esle,
            DbHelper.Parametre("@kimlikNo", kimlikNo));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool KimlikNoExists(string kimlikNo, string exceptMusteriID = "")
    {
        var sayi = DbHelper.TekDegerCalistir<int>(
            "SELECT COUNT(1) FROM musteri WHERE kimlikno = @kimlikNo AND musteriID <> @exceptMusteriID",
            DbHelper.Parametre("@kimlikNo", kimlikNo),
            DbHelper.Parametre("@exceptMusteriID", exceptMusteriID));
        return sayi > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Ekle(Musteri musteri)
    {
        const string sql = """
            INSERT INTO musteri (musteriID, ad, soyad, telefon, adres, il, ilce, acikAdres, postaKodu, kimlikno, kayitTarihi)
            VALUES (@musteriID, @ad, @soyad, @telefon, @adres, @il, @ilce, @acikAdres, @postaKodu, @kimlikno, @kayitTarihi)
            """;
        return DbHelper.KomutCalistir(sql, Parametreler(musteri)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Guncelle(Musteri musteri)
    {
        const string sql = """
            UPDATE musteri
            SET ad = @ad,
                soyad = @soyad,
                telefon = @telefon,
                adres = @adres,
                il = @il,
                ilce = @ilce,
                acikAdres = @acikAdres,
                postaKodu = @postaKodu,
                kimlikno = @kimlikno
            WHERE musteriID = @musteriID
            """;
        return DbHelper.KomutCalistir(sql, Parametreler(musteri)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Sil(string musteriID)
    {
        return DbHelper.KomutCalistir(
            "DELETE FROM musteri WHERE musteriID = @musteriID",
            DbHelper.Parametre("@musteriID", musteriID)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Musteri> Ara(string q)
    {
        return DbHelper.ListeCalistir(
            $"""
            {SelectSql}
            WHERE ad LIKE @q OR soyad LIKE @q OR telefon LIKE @q OR kimlikno LIKE @q
            ORDER BY ad, soyad
            """,
            Esle,
            DbHelper.Parametre("@q", $"%{q}%"));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static SqliteParameter[] Parametreler(Musteri musteri)
    {
        return
        [
            DbHelper.Parametre("@musteriID", musteri.MusteriID),
            DbHelper.Parametre("@ad", musteri.Ad),
            DbHelper.Parametre("@soyad", musteri.Soyad),
            DbHelper.Parametre("@telefon", musteri.Telefon),
            DbHelper.Parametre("@adres", string.IsNullOrWhiteSpace(musteri.Adres) ? musteri.TamAdres : musteri.Adres),
            DbHelper.Parametre("@il", musteri.Il),
            DbHelper.Parametre("@ilce", musteri.Ilce),
            DbHelper.Parametre("@acikAdres", musteri.AcikAdres),
            DbHelper.Parametre("@postaKodu", musteri.PostaKodu),
            DbHelper.Parametre("@kimlikno", musteri.KimlikNo),
            DbHelper.Parametre("@kayitTarihi", musteri.KayitTarihi)
        ];
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static Musteri Esle(SqliteDataReader okuyucu)
    {
        return new Musteri
        {
            MusteriID = okuyucu.MetinOku("musteriID"),
            Ad = okuyucu.MetinOku("ad"),
            Soyad = okuyucu.MetinOku("soyad"),
            Telefon = okuyucu.MetinOku("telefon"),
            Adres = okuyucu.MetinOku("adres"),
            Il = okuyucu.MetinOku("il"),
            Ilce = okuyucu.MetinOku("ilce"),
            AcikAdres = okuyucu.MetinOku("acikAdres"),
            PostaKodu = okuyucu.MetinOku("postaKodu"),
            KimlikNo = okuyucu.MetinOku("kimlikno"),
            KayitTarihi = okuyucu.TarihOku("kayitTarihi")
        };
    }
}
