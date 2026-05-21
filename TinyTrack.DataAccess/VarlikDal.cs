using Microsoft.Data.Sqlite;
using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class VarlikDal
{
    private const string SelectSql = """
        SELECT varlikID, varliktipi, ad, kapasite, gunlukucret, durum, konum
        FROM varlik
        """;

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Varlik> TumunuGetir()
    {
        return DbHelper.ListeCalistir($"{SelectSql} ORDER BY ad", Esle);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Varlik? IdIleGetir(string varlikID)
    {
        return DbHelper.TekKayitCalistir(
            $"{SelectSql} WHERE varlikID = @varlikID",
            Esle,
            DbHelper.Parametre("@varlikID", varlikID));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Ekle(Varlik varlik)
    {
        const string sql = """
            INSERT INTO varlik (varlikID, varliktipi, ad, kapasite, gunlukucret, durum, konum)
            VALUES (@varlikID, @varliktipi, @ad, @kapasite, @gunlukucret, @durum, @konum)
            """;
        return DbHelper.KomutCalistir(sql, Parametreler(varlik)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Guncelle(Varlik varlik)
    {
        const string sql = """
            UPDATE varlik
            SET varliktipi = @varliktipi,
                ad = @ad,
                kapasite = @kapasite,
                gunlukucret = @gunlukucret,
                durum = @durum,
                konum = @konum
            WHERE varlikID = @varlikID
            """;
        return DbHelper.KomutCalistir(sql, Parametreler(varlik)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Sil(string varlikID)
    {
        return DbHelper.KomutCalistir(
            "DELETE FROM varlik WHERE varlikID = @varlikID",
            DbHelper.Parametre("@varlikID", varlikID)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public int RezervasyonSayisiniGetir(string varlikID)
    {
        return DbHelper.TekDegerCalistir<int>(
            "SELECT COUNT(1) FROM rezervasyon WHERE varlikID = @varlikID",
            DbHelper.Parametre("@varlikID", varlikID));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public int OperasyonSayisiniGetir(string varlikID)
    {
        return DbHelper.TekDegerCalistir<int>(
            "SELECT COUNT(1) FROM operasyon WHERE varlikID = @varlikID",
            DbHelper.Parametre("@varlikID", varlikID));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool DurumuGuncelle(string varlikID, VarlikDurumu durum)
    {
        return DbHelper.KomutCalistir(
            "UPDATE varlik SET durum = @durum WHERE varlikID = @varlikID",
            DbHelper.Parametre("@durum", durum.ToString()),
            DbHelper.Parametre("@varlikID", varlikID)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Varlik> Ara(string q)
    {
        return DbHelper.ListeCalistir(
            $"""
            {SelectSql}
            WHERE varliktipi LIKE @q OR ad LIKE @q OR durum LIKE @q OR konum LIKE @q
            ORDER BY ad
            """,
            Esle,
            DbHelper.Parametre("@q", $"%{q}%"));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static SqliteParameter[] Parametreler(Varlik varlik)
    {
        return
        [
            DbHelper.Parametre("@varlikID", varlik.VarlikID),
            DbHelper.Parametre("@varliktipi", varlik.VarlikTipi),
            DbHelper.Parametre("@ad", varlik.Ad),
            DbHelper.Parametre("@kapasite", varlik.Kapasite),
            DbHelper.Parametre("@gunlukucret", varlik.GunlukUcret),
            DbHelper.Parametre("@durum", varlik.Durum.ToString()),
            DbHelper.Parametre("@konum", varlik.Konum)
        ];
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static Varlik Esle(SqliteDataReader okuyucu)
    {
        Enum.TryParse(okuyucu.MetinOku("durum"), out VarlikDurumu durum);
        return new Varlik
        {
            VarlikID = okuyucu.MetinOku("varlikID"),
            VarlikTipi = okuyucu.MetinOku("varliktipi"),
            Ad = okuyucu.MetinOku("ad"),
            Kapasite = okuyucu.TamsayiOku("kapasite"),
            GunlukUcret = okuyucu.OndalikOku("gunlukucret"),
            Durum = durum,
            Konum = okuyucu.MetinOku("konum")
        };
    }
}
