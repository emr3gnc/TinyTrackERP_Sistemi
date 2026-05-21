using Microsoft.Data.SqlClient;
using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

public class MusteriDal
{
    private const string SelectSql = """
        SELECT musteriID, ad, soyad, telefon, adres, kimlikno, kayitTarihi
        FROM dbo.musteri
        """;

    public List<Musteri> GetAll()
    {
        return DbHelper.ExecuteList($"{SelectSql} ORDER BY ad, soyad", Map);
    }

    public Musteri? GetById(string musteriID)
    {
        return DbHelper.ExecuteSingle(
            $"{SelectSql} WHERE musteriID = @musteriID",
            Map,
            DbHelper.Parameter("@musteriID", musteriID));
    }

    public bool KimlikNoExists(string kimlikNo, string exceptMusteriID = "")
    {
        var count = DbHelper.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM dbo.musteri WHERE kimlikno = @kimlikNo AND musteriID <> @exceptMusteriID",
            DbHelper.Parameter("@kimlikNo", kimlikNo),
            DbHelper.Parameter("@exceptMusteriID", exceptMusteriID));
        return count > 0;
    }

    public bool Insert(Musteri musteri)
    {
        const string sql = """
            INSERT INTO dbo.musteri (musteriID, ad, soyad, telefon, adres, kimlikno, kayitTarihi)
            VALUES (@musteriID, @ad, @soyad, @telefon, @adres, @kimlikno, @kayitTarihi)
            """;
        return DbHelper.ExecuteNonQuery(sql, Parameters(musteri)) > 0;
    }

    public bool Update(Musteri musteri)
    {
        const string sql = """
            UPDATE dbo.musteri
            SET ad = @ad,
                soyad = @soyad,
                telefon = @telefon,
                adres = @adres,
                kimlikno = @kimlikno
            WHERE musteriID = @musteriID
            """;
        return DbHelper.ExecuteNonQuery(sql, Parameters(musteri)) > 0;
    }

    public bool Delete(string musteriID)
    {
        return DbHelper.ExecuteNonQuery(
            "DELETE FROM dbo.musteri WHERE musteriID = @musteriID",
            DbHelper.Parameter("@musteriID", musteriID)) > 0;
    }

    public List<Musteri> Search(string q)
    {
        return DbHelper.ExecuteList(
            $"""
            {SelectSql}
            WHERE ad LIKE @q OR soyad LIKE @q OR telefon LIKE @q OR kimlikno LIKE @q
            ORDER BY ad, soyad
            """,
            Map,
            DbHelper.Parameter("@q", $"%{q}%"));
    }

    private static SqlParameter[] Parameters(Musteri musteri)
    {
        return
        [
            DbHelper.Parameter("@musteriID", musteri.MusteriID),
            DbHelper.Parameter("@ad", musteri.Ad),
            DbHelper.Parameter("@soyad", musteri.Soyad),
            DbHelper.Parameter("@telefon", musteri.Telefon),
            DbHelper.Parameter("@adres", musteri.Adres),
            DbHelper.Parameter("@kimlikno", musteri.KimlikNo),
            DbHelper.Parameter("@kayitTarihi", musteri.KayitTarihi)
        ];
    }

    private static Musteri Map(SqlDataReader reader)
    {
        return new Musteri
        {
            MusteriID = reader.ReadString("musteriID"),
            Ad = reader.ReadString("ad"),
            Soyad = reader.ReadString("soyad"),
            Telefon = reader.ReadString("telefon"),
            Adres = reader.ReadString("adres"),
            KimlikNo = reader.ReadString("kimlikno"),
            KayitTarihi = reader.ReadDateTime("kayitTarihi")
        };
    }
}
