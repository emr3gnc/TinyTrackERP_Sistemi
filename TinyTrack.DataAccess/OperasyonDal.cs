using Microsoft.Data.SqlClient;
using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

public class OperasyonDal
{
    private const string SelectSql = """
        SELECT o.operasyonID, o.varlikID, o.operasyonTipi, o.durum, o.tarih, o.notlar,
               v.ad AS varlikAdi
        FROM dbo.operasyon o
        INNER JOIN dbo.varlik v ON v.varlikID = o.varlikID
        """;

    public List<Operasyon> GetAll()
    {
        return DbHelper.ExecuteList($"{SelectSql} ORDER BY o.tarih DESC", Map);
    }

    public Operasyon? GetById(string operasyonID)
    {
        return DbHelper.ExecuteSingle(
            $"{SelectSql} WHERE o.operasyonID = @operasyonID",
            Map,
            DbHelper.Parameter("@operasyonID", operasyonID));
    }

    public bool Insert(Operasyon operasyon)
    {
        const string sql = """
            INSERT INTO dbo.operasyon (operasyonID, varlikID, operasyonTipi, durum, tarih, notlar)
            VALUES (@operasyonID, @varlikID, @operasyonTipi, @durum, @tarih, @notlar)
            """;
        return DbHelper.ExecuteNonQuery(sql, Parameters(operasyon)) > 0;
    }

    public bool Update(Operasyon operasyon)
    {
        const string sql = """
            UPDATE dbo.operasyon
            SET varlikID = @varlikID,
                operasyonTipi = @operasyonTipi,
                durum = @durum,
                tarih = @tarih,
                notlar = @notlar
            WHERE operasyonID = @operasyonID
            """;
        return DbHelper.ExecuteNonQuery(sql, Parameters(operasyon)) > 0;
    }

    public bool Delete(string operasyonID)
    {
        return DbHelper.ExecuteNonQuery(
            "DELETE FROM dbo.operasyon WHERE operasyonID = @operasyonID",
            DbHelper.Parameter("@operasyonID", operasyonID)) > 0;
    }

    public int CountOpenByVarlik(string varlikID)
    {
        return DbHelper.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM dbo.operasyon WHERE varlikID = @varlikID AND durum = 0",
            DbHelper.Parameter("@varlikID", varlikID));
    }

    private static SqlParameter[] Parameters(Operasyon operasyon)
    {
        return
        [
            DbHelper.Parameter("@operasyonID", operasyon.OperasyonID),
            DbHelper.Parameter("@varlikID", operasyon.VarlikID),
            DbHelper.Parameter("@operasyonTipi", operasyon.OperasyonTipi.ToString()),
            DbHelper.Parameter("@durum", operasyon.Durum),
            DbHelper.Parameter("@tarih", operasyon.Tarih.Date),
            DbHelper.Parameter("@notlar", operasyon.Notlar)
        ];
    }

    private static Operasyon Map(SqlDataReader reader)
    {
        Enum.TryParse(reader.ReadString("operasyonTipi"), out OperasyonTipi operasyonTipi);
        return new Operasyon
        {
            OperasyonID = reader.ReadString("operasyonID"),
            VarlikID = reader.ReadString("varlikID"),
            OperasyonTipi = operasyonTipi,
            Durum = reader.ReadBoolean("durum"),
            Tarih = reader.ReadDateTime("tarih"),
            Notlar = reader.ReadString("notlar"),
            VarlikAdi = reader.ReadString("varlikAdi")
        };
    }
}
