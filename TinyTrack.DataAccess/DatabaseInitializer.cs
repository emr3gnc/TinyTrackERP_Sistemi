using Microsoft.Data.SqlClient;

namespace TinyTrack.DataAccess;

public static class DatabaseInitializer
{
    public static void EnsureCreatedAndSeeded()
    {
        EnsureDatabase();
        EnsureTables();
        SeedData();
    }

    private static void EnsureDatabase()
    {
        var builder = new SqlConnectionStringBuilder(DbHelper.ConnectionString);
        var databaseName = builder.InitialCatalog;
        builder.InitialCatalog = "master";

        using var connection = new SqlConnection(builder.ConnectionString);
        using var command = new SqlCommand(
            "IF DB_ID(@databaseName) IS NULL EXEC('CREATE DATABASE [' + @databaseName + ']')",
            connection);
        command.Parameters.AddWithValue("@databaseName", databaseName);
        connection.Open();
        command.ExecuteNonQuery();
    }

    private static void EnsureTables()
    {
        var statements = new[]
        {
            """
            IF OBJECT_ID('dbo.musteri', 'U') IS NULL
            CREATE TABLE dbo.musteri (
                musteriID NVARCHAR(20) NOT NULL PRIMARY KEY,
                ad NVARCHAR(60) NOT NULL,
                soyad NVARCHAR(60) NOT NULL,
                telefon NVARCHAR(20) NOT NULL,
                adres NVARCHAR(250) NOT NULL,
                kimlikno NVARCHAR(11) NOT NULL UNIQUE,
                kayitTarihi DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
            )
            """,
            """
            IF OBJECT_ID('dbo.varlik', 'U') IS NULL
            CREATE TABLE dbo.varlik (
                varlikID NVARCHAR(20) NOT NULL PRIMARY KEY,
                varliktipi NVARCHAR(40) NOT NULL,
                ad NVARCHAR(80) NOT NULL,
                kapasite INT NOT NULL,
                gunlukucret DECIMAL(12,2) NOT NULL,
                durum NVARCHAR(20) NOT NULL,
                konum NVARCHAR(120) NOT NULL DEFAULT ''
            )
            """,
            """
            IF OBJECT_ID('dbo.rezervasyon', 'U') IS NULL
            CREATE TABLE dbo.rezervasyon (
                rezervasyonID NVARCHAR(20) NOT NULL PRIMARY KEY,
                musteriID NVARCHAR(20) NOT NULL,
                varlikID NVARCHAR(20) NOT NULL,
                bastarih DATE NOT NULL,
                sontarih DATE NOT NULL,
                toplamucret DECIMAL(12,2) NOT NULL,
                durum NVARCHAR(20) NOT NULL,
                kayitTarihi DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                CONSTRAINT FK_rezervasyon_musteri FOREIGN KEY (musteriID) REFERENCES dbo.musteri(musteriID),
                CONSTRAINT FK_rezervasyon_varlik FOREIGN KEY (varlikID) REFERENCES dbo.varlik(varlikID)
            )
            """,
            """
            IF OBJECT_ID('dbo.odeme', 'U') IS NULL
            CREATE TABLE dbo.odeme (
                odemeID NVARCHAR(20) NOT NULL PRIMARY KEY,
                rezervasyonID NVARCHAR(20) NOT NULL,
                ucret DECIMAL(12,2) NOT NULL,
                odemetarihi DATE NOT NULL,
                odemetipi NVARCHAR(30) NOT NULL,
                aciklama NVARCHAR(200) NOT NULL DEFAULT '',
                CONSTRAINT FK_odeme_rezervasyon FOREIGN KEY (rezervasyonID) REFERENCES dbo.rezervasyon(rezervasyonID)
            )
            """,
            """
            IF OBJECT_ID('dbo.operasyon', 'U') IS NULL
            CREATE TABLE dbo.operasyon (
                operasyonID NVARCHAR(20) NOT NULL PRIMARY KEY,
                varlikID NVARCHAR(20) NOT NULL,
                operasyonTipi NVARCHAR(30) NOT NULL,
                durum BIT NOT NULL,
                tarih DATE NOT NULL,
                notlar NVARCHAR(250) NOT NULL DEFAULT '',
                CONSTRAINT FK_operasyon_varlik FOREIGN KEY (varlikID) REFERENCES dbo.varlik(varlikID)
            )
            """,
            """
            IF OBJECT_ID('dbo.hizmet', 'U') IS NULL
            CREATE TABLE dbo.hizmet (
                hizmetID NVARCHAR(20) NOT NULL PRIMARY KEY,
                rezervasyonID NVARCHAR(20) NOT NULL,
                ad NVARCHAR(80) NOT NULL,
                ucret DECIMAL(12,2) NOT NULL,
                CONSTRAINT FK_hizmet_rezervasyon FOREIGN KEY (rezervasyonID) REFERENCES dbo.rezervasyon(rezervasyonID)
            )
            """,
            """
            IF OBJECT_ID('dbo.kullanici', 'U') IS NULL
            CREATE TABLE dbo.kullanici (
                kullaniciID NVARCHAR(20) NOT NULL PRIMARY KEY,
                adSoyad NVARCHAR(100) NOT NULL,
                email NVARCHAR(120) NOT NULL UNIQUE,
                sifre NVARCHAR(80) NOT NULL,
                rol NVARCHAR(40) NOT NULL
            )
            """,
            """
            IF OBJECT_ID('dbo.isletme_ayarlari', 'U') IS NULL
            CREATE TABLE dbo.isletme_ayarlari (
                ayarID NVARCHAR(20) NOT NULL PRIMARY KEY,
                isletmeAdi NVARCHAR(100) NOT NULL,
                paraBirimi NVARCHAR(40) NOT NULL,
                dil NVARCHAR(40) NOT NULL,
                rezervasyonBildirimleri BIT NOT NULL,
                temizlikUyarilari BIT NOT NULL
            )
            """,
            """
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_rezervasyon_varlik_tarih' AND object_id = OBJECT_ID('dbo.rezervasyon'))
            CREATE INDEX IX_rezervasyon_varlik_tarih ON dbo.rezervasyon(varlikID, bastarih, sontarih, durum)
            """
        };

        foreach (var statement in statements)
        {
            DbHelper.ExecuteNonQuery(statement);
        }
    }

    private static void SeedData()
    {
        DbHelper.ExecuteNonQuery(
            """
            IF NOT EXISTS (SELECT 1 FROM dbo.kullanici WHERE kullaniciID = 'USR-001')
            INSERT INTO dbo.kullanici (kullaniciID, adSoyad, email, sifre, rol)
            VALUES ('USR-001', N'Ahmet Yilmaz', 'admin@tinytrack.local', '123456', N'Yonetici')
            """);

        DbHelper.ExecuteNonQuery(
            """
            IF NOT EXISTS (SELECT 1 FROM dbo.isletme_ayarlari WHERE ayarID = 'SET-001')
            INSERT INTO dbo.isletme_ayarlari (ayarID, isletmeAdi, paraBirimi, dil, rezervasyonBildirimleri, temizlikUyarilari)
            VALUES ('SET-001', N'TinyTrack ERP', N'Turk Lirasi', N'Turkce', 1, 1)
            """);

        DbHelper.ExecuteNonQuery(
            """
            IF NOT EXISTS (SELECT 1 FROM dbo.musteri WHERE musteriID = 'MUS-001')
            INSERT INTO dbo.musteri (musteriID, ad, soyad, telefon, adres, kimlikno)
            VALUES
            ('MUS-001', N'Ahmet', N'Yilmaz', '05551234567', N'Kuzey Ormanlari, Sakarya', '10000000146'),
            ('MUS-002', N'Selim', N'Aras', '05557654321', N'Ege Kiyilari, Mugla', '10000000214')
            """);

        DbHelper.ExecuteNonQuery(
            """
            UPDATE dbo.musteri
            SET kimlikno = '10000000214'
            WHERE musteriID = 'MUS-002' AND kimlikno = '10000000284'
            """);

        DbHelper.ExecuteNonQuery(
            """
            IF NOT EXISTS (SELECT 1 FROM dbo.varlik WHERE varlikID = 'VAR-001')
            INSERT INTO dbo.varlik (varlikID, varliktipi, ad, kapasite, gunlukucret, durum, konum)
            VALUES
            ('VAR-001', N'Tiny House', N'Tiny House 1', 3, 2500, N'Dolu', N'Kuzey Ormanlari, Sakarya'),
            ('VAR-002', N'Karavan', N'Caravan 2', 4, 1800, N'Temizlikte', N'Ege Kiyilari, Mugla'),
            ('VAR-003', N'Bungalov', N'Modern Loft', 2, 3200, N'Musait', N'Abant, Bolu')
            """);

        DbHelper.ExecuteNonQuery(
            """
            IF NOT EXISTS (SELECT 1 FROM dbo.rezervasyon WHERE rezervasyonID = 'REZ-001')
            INSERT INTO dbo.rezervasyon (rezervasyonID, musteriID, varlikID, bastarih, sontarih, toplamucret, durum)
            VALUES
            ('REZ-001', 'MUS-001', 'VAR-001', CAST(GETDATE() AS date), DATEADD(day, 3, CAST(GETDATE() AS date)), 7500, N'Aktif'),
            ('REZ-002', 'MUS-002', 'VAR-003', DATEADD(day, 5, CAST(GETDATE() AS date)), DATEADD(day, 8, CAST(GETDATE() AS date)), 9600, N'Aktif')
            """);

        DbHelper.ExecuteNonQuery(
            """
            IF NOT EXISTS (SELECT 1 FROM dbo.operasyon WHERE operasyonID = 'OPR-001')
            INSERT INTO dbo.operasyon (operasyonID, varlikID, operasyonTipi, durum, tarih, notlar)
            VALUES ('OPR-001', 'VAR-002', N'Temizlik', 0, CAST(GETDATE() AS date), N'Standart cikis temizligi devam ediyor')
            """);
    }
}
