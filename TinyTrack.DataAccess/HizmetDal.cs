using Microsoft.Data.SqlClient;
using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

public class HizmetDal
{
    private const string SelectSql = "SELECT hizmetID, rezervasyonID, ad, ucret FROM dbo.hizmet";

    public List<Hizmet> GetAll()
    {
        return DbHelper.ExecuteList($"{SelectSql} ORDER BY ad", Map);
    }

    public List<Hizmet> GetByRezervasyon(string rezervasyonID)
    {
        return DbHelper.ExecuteList(
            $"{SelectSql} WHERE rezervasyonID = @rezervasyonID ORDER BY ad",
            Map,
            DbHelper.Parameter("@rezervasyonID", rezervasyonID));
    }

    public Hizmet? GetById(string hizmetID)
    {
        return DbHelper.ExecuteSingle(
            $"{SelectSql} WHERE hizmetID = @hizmetID",
            Map,
            DbHelper.Parameter("@hizmetID", hizmetID));
    }

    public bool Insert(Hizmet hizmet)
    {
        const string sql = """
            INSERT INTO dbo.hizmet (hizmetID, rezervasyonID, ad, ucret)
            VALUES (@hizmetID, @rezervasyonID, @ad, @ucret)
            """;
        return DbHelper.ExecuteNonQuery(sql, Parameters(hizmet)) > 0;
    }

    public bool Update(Hizmet hizmet)
    {
        const string sql = """
            UPDATE dbo.hizmet
            SET rezervasyonID = @rezervasyonID,
                ad = @ad,
                ucret = @ucret
            WHERE hizmetID = @hizmetID
            """;
        return DbHelper.ExecuteNonQuery(sql, Parameters(hizmet)) > 0;
    }

    public bool Delete(string hizmetID)
    {
        return DbHelper.ExecuteNonQuery(
            "DELETE FROM dbo.hizmet WHERE hizmetID = @hizmetID",
            DbHelper.Parameter("@hizmetID", hizmetID)) > 0;
    }

    public decimal GetToplamHizmetUcreti(string rezervasyonID)
    {
        return DbHelper.ExecuteScalar<decimal>(
            "SELECT COALESCE(SUM(ucret), 0) FROM dbo.hizmet WHERE rezervasyonID = @rezervasyonID",
            DbHelper.Parameter("@rezervasyonID", rezervasyonID));
    }

    private static SqlParameter[] Parameters(Hizmet hizmet)
    {
        return
        [
            DbHelper.Parameter("@hizmetID", hizmet.HizmetID),
            DbHelper.Parameter("@rezervasyonID", hizmet.RezervasyonID),
            DbHelper.Parameter("@ad", hizmet.Ad),
            DbHelper.Parameter("@ucret", hizmet.Ucret)
        ];
    }

    private static Hizmet Map(SqlDataReader reader)
    {
        return new Hizmet
        {
            HizmetID = reader.ReadString("hizmetID"),
            RezervasyonID = reader.ReadString("rezervasyonID"),
            Ad = reader.ReadString("ad"),
            Ucret = reader.ReadDecimal("ucret")
        };
    }
}
