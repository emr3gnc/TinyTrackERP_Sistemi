using Microsoft.Data.SqlClient;
using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

public class OdemeDal
{
    private const string SelectSql = """
        SELECT o.odemeID, o.rezervasyonID, o.ucret, o.odemetarihi, o.odemetipi, o.aciklama,
               CONCAT(m.ad, ' ', m.soyad) AS musteriAdSoyad
        FROM dbo.odeme o
        INNER JOIN dbo.rezervasyon r ON r.rezervasyonID = o.rezervasyonID
        INNER JOIN dbo.musteri m ON m.musteriID = r.musteriID
        """;

    public List<Odeme> GetAll()
    {
        return DbHelper.ExecuteList($"{SelectSql} ORDER BY o.odemetarihi DESC", Map);
    }

    public Odeme? GetById(string odemeID)
    {
        return DbHelper.ExecuteSingle(
            $"{SelectSql} WHERE o.odemeID = @odemeID",
            Map,
            DbHelper.Parameter("@odemeID", odemeID));
    }

    public List<Odeme> GetByRezervasyon(string rezervasyonID)
    {
        return DbHelper.ExecuteList(
            $"{SelectSql} WHERE o.rezervasyonID = @rezervasyonID ORDER BY o.odemetarihi DESC",
            Map,
            DbHelper.Parameter("@rezervasyonID", rezervasyonID));
    }

    public bool Insert(Odeme odeme)
    {
        const string sql = """
            INSERT INTO dbo.odeme (odemeID, rezervasyonID, ucret, odemetarihi, odemetipi, aciklama)
            VALUES (@odemeID, @rezervasyonID, @ucret, @odemetarihi, @odemetipi, @aciklama)
            """;
        return DbHelper.ExecuteNonQuery(sql, Parameters(odeme)) > 0;
    }

    public bool Update(Odeme odeme)
    {
        const string sql = """
            UPDATE dbo.odeme
            SET rezervasyonID = @rezervasyonID,
                ucret = @ucret,
                odemetarihi = @odemetarihi,
                odemetipi = @odemetipi,
                aciklama = @aciklama
            WHERE odemeID = @odemeID
            """;
        return DbHelper.ExecuteNonQuery(sql, Parameters(odeme)) > 0;
    }

    public bool Delete(string odemeID)
    {
        return DbHelper.ExecuteNonQuery(
            "DELETE FROM dbo.odeme WHERE odemeID = @odemeID",
            DbHelper.Parameter("@odemeID", odemeID)) > 0;
    }

    private static SqlParameter[] Parameters(Odeme odeme)
    {
        return
        [
            DbHelper.Parameter("@odemeID", odeme.OdemeID),
            DbHelper.Parameter("@rezervasyonID", odeme.RezervasyonID),
            DbHelper.Parameter("@ucret", odeme.Ucret),
            DbHelper.Parameter("@odemetarihi", odeme.OdemeTarihi.Date),
            DbHelper.Parameter("@odemetipi", odeme.OdemeTipi.ToString()),
            DbHelper.Parameter("@aciklama", odeme.Aciklama)
        ];
    }

    private static Odeme Map(SqlDataReader reader)
    {
        Enum.TryParse(reader.ReadString("odemetipi"), out OdemeTipi odemeTipi);
        return new Odeme
        {
            OdemeID = reader.ReadString("odemeID"),
            RezervasyonID = reader.ReadString("rezervasyonID"),
            Ucret = reader.ReadDecimal("ucret"),
            OdemeTarihi = reader.ReadDateTime("odemetarihi"),
            OdemeTipi = odemeTipi,
            Aciklama = reader.ReadString("aciklama"),
            MusteriAdSoyad = reader.ReadString("musteriAdSoyad")
        };
    }
}
