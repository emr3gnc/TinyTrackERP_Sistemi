using Microsoft.Data.SqlClient;
using TinyTrack.Entities;

namespace TinyTrack.DataAccess;

public class RezervasyonDal
{
    private const string SelectSql = """
        SELECT r.rezervasyonID, r.musteriID, r.varlikID, r.bastarih, r.sontarih,
               r.toplamucret, r.durum, r.kayitTarihi,
               CONCAT(m.ad, ' ', m.soyad) AS musteriAdSoyad,
               v.ad AS varlikAdi
        FROM dbo.rezervasyon r
        INNER JOIN dbo.musteri m ON m.musteriID = r.musteriID
        INNER JOIN dbo.varlik v ON v.varlikID = r.varlikID
        """;

    public List<Rezervasyon> GetAll()
    {
        return DbHelper.ExecuteList($"{SelectSql} ORDER BY r.bastarih DESC", Map);
    }

    public Rezervasyon? GetById(string rezervasyonID)
    {
        return DbHelper.ExecuteSingle(
            $"{SelectSql} WHERE r.rezervasyonID = @rezervasyonID",
            Map,
            DbHelper.Parameter("@rezervasyonID", rezervasyonID));
    }

    public bool Insert(Rezervasyon rezervasyon)
    {
        const string sql = """
            INSERT INTO dbo.rezervasyon (rezervasyonID, musteriID, varlikID, bastarih, sontarih, toplamucret, durum, kayitTarihi)
            VALUES (@rezervasyonID, @musteriID, @varlikID, @bastarih, @sontarih, @toplamucret, @durum, @kayitTarihi)
            """;
        return DbHelper.ExecuteNonQuery(sql, Parameters(rezervasyon)) > 0;
    }

    public bool Update(Rezervasyon rezervasyon)
    {
        const string sql = """
            UPDATE dbo.rezervasyon
            SET musteriID = @musteriID,
                varlikID = @varlikID,
                bastarih = @bastarih,
                sontarih = @sontarih,
                toplamucret = @toplamucret,
                durum = @durum
            WHERE rezervasyonID = @rezervasyonID
            """;
        return DbHelper.ExecuteNonQuery(sql, Parameters(rezervasyon)) > 0;
    }

    public bool Delete(string rezervasyonID)
    {
        return DbHelper.ExecuteNonQuery(
            "DELETE FROM dbo.rezervasyon WHERE rezervasyonID = @rezervasyonID",
            DbHelper.Parameter("@rezervasyonID", rezervasyonID)) > 0;
    }

    public bool HasOverlap(string varlikID, DateTime basTarih, DateTime sonTarih, string exceptRezervasyonID = "")
    {
        var count = DbHelper.ExecuteScalar<int>(
            """
            SELECT COUNT(1)
            FROM dbo.rezervasyon
            WHERE varlikID = @varlikID
              AND rezervasyonID <> @exceptRezervasyonID
              AND durum = N'Aktif'
              AND @basTarih < sontarih
              AND @sonTarih > bastarih
            """,
            DbHelper.Parameter("@varlikID", varlikID),
            DbHelper.Parameter("@exceptRezervasyonID", exceptRezervasyonID),
            DbHelper.Parameter("@basTarih", basTarih.Date),
            DbHelper.Parameter("@sonTarih", sonTarih.Date));
        return count > 0;
    }

    public List<Rezervasyon> Search(string q)
    {
        return DbHelper.ExecuteList(
            $"""
            {SelectSql}
            WHERE r.rezervasyonID LIKE @q OR m.ad LIKE @q OR m.soyad LIKE @q OR v.ad LIKE @q OR r.durum LIKE @q
            ORDER BY r.bastarih DESC
            """,
            Map,
            DbHelper.Parameter("@q", $"%{q}%"));
    }

    public decimal GetOdenenTutar(string rezervasyonID)
    {
        return DbHelper.ExecuteScalar<decimal>(
            "SELECT COALESCE(SUM(ucret), 0) FROM dbo.odeme WHERE rezervasyonID = @rezervasyonID",
            DbHelper.Parameter("@rezervasyonID", rezervasyonID));
    }

    private static SqlParameter[] Parameters(Rezervasyon rezervasyon)
    {
        return
        [
            DbHelper.Parameter("@rezervasyonID", rezervasyon.RezervasyonID),
            DbHelper.Parameter("@musteriID", rezervasyon.MusteriID),
            DbHelper.Parameter("@varlikID", rezervasyon.VarlikID),
            DbHelper.Parameter("@bastarih", rezervasyon.BasTarih.Date),
            DbHelper.Parameter("@sontarih", rezervasyon.SonTarih.Date),
            DbHelper.Parameter("@toplamucret", rezervasyon.ToplamUcret),
            DbHelper.Parameter("@durum", rezervasyon.Durum.ToString()),
            DbHelper.Parameter("@kayitTarihi", rezervasyon.KayitTarihi)
        ];
    }

    private static Rezervasyon Map(SqlDataReader reader)
    {
        Enum.TryParse(reader.ReadString("durum"), out RezervasyonDurumu durum);
        return new Rezervasyon
        {
            RezervasyonID = reader.ReadString("rezervasyonID"),
            MusteriID = reader.ReadString("musteriID"),
            VarlikID = reader.ReadString("varlikID"),
            BasTarih = reader.ReadDateTime("bastarih"),
            SonTarih = reader.ReadDateTime("sontarih"),
            ToplamUcret = reader.ReadDecimal("toplamucret"),
            Durum = durum,
            KayitTarihi = reader.ReadDateTime("kayitTarihi"),
            MusteriAdSoyad = reader.ReadString("musteriAdSoyad"),
            VarlikAdi = reader.ReadString("varlikAdi")
        };
    }
}
