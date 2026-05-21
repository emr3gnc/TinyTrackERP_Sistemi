using Microsoft.Data.Sqlite;
using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class OperasyonDal
{
    private const string SelectSql = """
        SELECT o.operasyonID, o.varlikID, o.operasyonTipi, o.durum, o.tarih, o.notlar,
               v.ad AS varlikAdi
        FROM operasyon o
        INNER JOIN varlik v ON v.varlikID = o.varlikID
        """;

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public List<Operasyon> TumunuGetir()
    {
        return DbHelper.ListeCalistir($"{SelectSql} ORDER BY o.tarih DESC", Esle);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public Operasyon? IdIleGetir(string operasyonID)
    {
        return DbHelper.TekKayitCalistir(
            $"{SelectSql} WHERE o.operasyonID = @operasyonID",
            Esle,
            DbHelper.Parametre("@operasyonID", operasyonID));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Ekle(Operasyon operasyon)
    {
        const string sql = """
            INSERT INTO operasyon (operasyonID, varlikID, operasyonTipi, durum, tarih, notlar)
            VALUES (@operasyonID, @varlikID, @operasyonTipi, @durum, @tarih, @notlar)
            """;
        return DbHelper.KomutCalistir(sql, Parametreler(operasyon)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Guncelle(Operasyon operasyon)
    {
        const string sql = """
            UPDATE operasyon
            SET varlikID = @varlikID,
                operasyonTipi = @operasyonTipi,
                durum = @durum,
                tarih = @tarih,
                notlar = @notlar
            WHERE operasyonID = @operasyonID
            """;
        return DbHelper.KomutCalistir(sql, Parametreler(operasyon)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool Sil(string operasyonID)
    {
        return DbHelper.KomutCalistir(
            "DELETE FROM operasyon WHERE operasyonID = @operasyonID",
            DbHelper.Parametre("@operasyonID", operasyonID)) > 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public int VarligaGoreAcikSayisiniGetir(string varlikID)
    {
        return DbHelper.TekDegerCalistir<int>(
            "SELECT COUNT(1) FROM operasyon WHERE varlikID = @varlikID AND durum = 0",
            DbHelper.Parametre("@varlikID", varlikID));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static SqliteParameter[] Parametreler(Operasyon operasyon)
    {
        return
        [
            DbHelper.Parametre("@operasyonID", operasyon.OperasyonID),
            DbHelper.Parametre("@varlikID", operasyon.VarlikID),
            DbHelper.Parametre("@operasyonTipi", operasyon.OperasyonTipi.ToString()),
            DbHelper.Parametre("@durum", operasyon.Durum),
            DbHelper.Parametre("@tarih", operasyon.Tarih.Date),
            DbHelper.Parametre("@notlar", operasyon.Notlar)
        ];
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static Operasyon Esle(SqliteDataReader okuyucu)
    {
        Enum.TryParse(okuyucu.MetinOku("operasyonTipi"), out OperasyonTipi operasyonTipi);
        return new Operasyon
        {
            OperasyonID = okuyucu.MetinOku("operasyonID"),
            VarlikID = okuyucu.MetinOku("varlikID"),
            OperasyonTipi = operasyonTipi,
            Durum = okuyucu.MantiksalOku("durum"),
            Tarih = okuyucu.TarihOku("tarih"),
            Notlar = okuyucu.MetinOku("notlar"),
            VarlikAdi = okuyucu.MetinOku("varlikAdi")
        };
    }
}
