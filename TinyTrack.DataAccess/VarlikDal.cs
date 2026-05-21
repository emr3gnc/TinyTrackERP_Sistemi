using Microsoft.Data.SqlClient;
using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

public class VarlikDal
{
    private const string SelectSql = """
        SELECT varlikID, varliktipi, ad, kapasite, gunlukucret, durum, konum
        FROM dbo.varlik
        """;

    public List<Varlik> GetAll()
    {
        return DbHelper.ExecuteList($"{SelectSql} ORDER BY ad", Map);
    }

    public Varlik? GetById(string varlikID)
    {
        return DbHelper.ExecuteSingle(
            $"{SelectSql} WHERE varlikID = @varlikID",
            Map,
            DbHelper.Parameter("@varlikID", varlikID));
    }

    public bool Insert(Varlik varlik)
    {
        const string sql = """
            INSERT INTO dbo.varlik (varlikID, varliktipi, ad, kapasite, gunlukucret, durum, konum)
            VALUES (@varlikID, @varliktipi, @ad, @kapasite, @gunlukucret, @durum, @konum)
            """;
        return DbHelper.ExecuteNonQuery(sql, Parameters(varlik)) > 0;
    }

    public bool Update(Varlik varlik)
    {
        const string sql = """
            UPDATE dbo.varlik
            SET varliktipi = @varliktipi,
                ad = @ad,
                kapasite = @kapasite,
                gunlukucret = @gunlukucret,
                durum = @durum,
                konum = @konum
            WHERE varlikID = @varlikID
            """;
        return DbHelper.ExecuteNonQuery(sql, Parameters(varlik)) > 0;
    }

    public bool Delete(string varlikID)
    {
        return DbHelper.ExecuteNonQuery(
            "DELETE FROM dbo.varlik WHERE varlikID = @varlikID",
            DbHelper.Parameter("@varlikID", varlikID)) > 0;
    }

    public bool UpdateDurum(string varlikID, VarlikDurumu durum)
    {
        return DbHelper.ExecuteNonQuery(
            "UPDATE dbo.varlik SET durum = @durum WHERE varlikID = @varlikID",
            DbHelper.Parameter("@durum", durum.ToString()),
            DbHelper.Parameter("@varlikID", varlikID)) > 0;
    }

    public List<Varlik> Search(string q)
    {
        return DbHelper.ExecuteList(
            $"""
            {SelectSql}
            WHERE varliktipi LIKE @q OR ad LIKE @q OR durum LIKE @q OR konum LIKE @q
            ORDER BY ad
            """,
            Map,
            DbHelper.Parameter("@q", $"%{q}%"));
    }

    private static SqlParameter[] Parameters(Varlik varlik)
    {
        return
        [
            DbHelper.Parameter("@varlikID", varlik.VarlikID),
            DbHelper.Parameter("@varliktipi", varlik.VarlikTipi),
            DbHelper.Parameter("@ad", varlik.Ad),
            DbHelper.Parameter("@kapasite", varlik.Kapasite),
            DbHelper.Parameter("@gunlukucret", varlik.GunlukUcret),
            DbHelper.Parameter("@durum", varlik.Durum.ToString()),
            DbHelper.Parameter("@konum", varlik.Konum)
        ];
    }

    private static Varlik Map(SqlDataReader reader)
    {
        Enum.TryParse(reader.ReadString("durum"), out VarlikDurumu durum);
        return new Varlik
        {
            VarlikID = reader.ReadString("varlikID"),
            VarlikTipi = reader.ReadString("varliktipi"),
            Ad = reader.ReadString("ad"),
            Kapasite = reader.ReadInt32("kapasite"),
            GunlukUcret = reader.ReadDecimal("gunlukucret"),
            Durum = durum,
            Konum = reader.ReadString("konum")
        };
    }
}
