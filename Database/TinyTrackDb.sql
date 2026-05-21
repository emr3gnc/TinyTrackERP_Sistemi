IF DB_ID('TinyTrackDb') IS NULL
BEGIN
    CREATE DATABASE TinyTrackDb;
END
GO

USE TinyTrackDb;
GO

IF OBJECT_ID('dbo.musteri', 'U') IS NULL
CREATE TABLE dbo.musteri (
    musteriID NVARCHAR(20) NOT NULL PRIMARY KEY,
    ad NVARCHAR(60) NOT NULL,
    soyad NVARCHAR(60) NOT NULL,
    telefon NVARCHAR(20) NOT NULL,
    adres NVARCHAR(250) NOT NULL,
    kimlikno NVARCHAR(11) NOT NULL UNIQUE,
    kayitTarihi DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

IF OBJECT_ID('dbo.varlik', 'U') IS NULL
CREATE TABLE dbo.varlik (
    varlikID NVARCHAR(20) NOT NULL PRIMARY KEY,
    varliktipi NVARCHAR(40) NOT NULL,
    ad NVARCHAR(80) NOT NULL,
    kapasite INT NOT NULL,
    gunlukucret DECIMAL(12,2) NOT NULL,
    durum NVARCHAR(20) NOT NULL,
    konum NVARCHAR(120) NOT NULL DEFAULT ''
);

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
);

IF OBJECT_ID('dbo.odeme', 'U') IS NULL
CREATE TABLE dbo.odeme (
    odemeID NVARCHAR(20) NOT NULL PRIMARY KEY,
    rezervasyonID NVARCHAR(20) NOT NULL,
    ucret DECIMAL(12,2) NOT NULL,
    odemetarihi DATE NOT NULL,
    odemetipi NVARCHAR(30) NOT NULL,
    aciklama NVARCHAR(200) NOT NULL DEFAULT '',
    CONSTRAINT FK_odeme_rezervasyon FOREIGN KEY (rezervasyonID) REFERENCES dbo.rezervasyon(rezervasyonID)
);

IF OBJECT_ID('dbo.operasyon', 'U') IS NULL
CREATE TABLE dbo.operasyon (
    operasyonID NVARCHAR(20) NOT NULL PRIMARY KEY,
    varlikID NVARCHAR(20) NOT NULL,
    operasyonTipi NVARCHAR(30) NOT NULL,
    durum BIT NOT NULL,
    tarih DATE NOT NULL,
    notlar NVARCHAR(250) NOT NULL DEFAULT '',
    CONSTRAINT FK_operasyon_varlik FOREIGN KEY (varlikID) REFERENCES dbo.varlik(varlikID)
);

IF OBJECT_ID('dbo.hizmet', 'U') IS NULL
CREATE TABLE dbo.hizmet (
    hizmetID NVARCHAR(20) NOT NULL PRIMARY KEY,
    rezervasyonID NVARCHAR(20) NOT NULL,
    ad NVARCHAR(80) NOT NULL,
    ucret DECIMAL(12,2) NOT NULL,
    CONSTRAINT FK_hizmet_rezervasyon FOREIGN KEY (rezervasyonID) REFERENCES dbo.rezervasyon(rezervasyonID)
);

IF OBJECT_ID('dbo.kullanici', 'U') IS NULL
CREATE TABLE dbo.kullanici (
    kullaniciID NVARCHAR(20) NOT NULL PRIMARY KEY,
    adSoyad NVARCHAR(100) NOT NULL,
    email NVARCHAR(120) NOT NULL UNIQUE,
    sifre NVARCHAR(80) NOT NULL,
    rol NVARCHAR(40) NOT NULL
);

IF OBJECT_ID('dbo.isletme_ayarlari', 'U') IS NULL
CREATE TABLE dbo.isletme_ayarlari (
    ayarID NVARCHAR(20) NOT NULL PRIMARY KEY,
    isletmeAdi NVARCHAR(100) NOT NULL,
    paraBirimi NVARCHAR(40) NOT NULL,
    dil NVARCHAR(40) NOT NULL,
    rezervasyonBildirimleri BIT NOT NULL,
    temizlikUyarilari BIT NOT NULL
);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_rezervasyon_varlik_tarih' AND object_id = OBJECT_ID('dbo.rezervasyon'))
CREATE INDEX IX_rezervasyon_varlik_tarih ON dbo.rezervasyon(varlikID, bastarih, sontarih, durum);
