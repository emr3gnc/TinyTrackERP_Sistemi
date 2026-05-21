using Microsoft.Data.Sqlite;

namespace TinyTrack.DataAccess;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public static class DatabaseInitializer
{
    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public static void OlusturuldugunuVeOrnekVerininHazirlandiginiGarantiEt()
    {
        var shouldSeed = OrnekVeriEklenmeliMi();
        TablolariGarantiEt();
        if (shouldSeed)
        {
            OrnekVeriEkle();
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static bool OrnekVeriEklenmeliMi()
    {
        using var baglanti = DbHelper.BaglantiOlustur();
        using var komut = baglanti.CreateCommand();
        komut.CommandText = "SELECT COUNT(1) FROM sqlite_master WHERE type = 'table' AND name = 'musteri'";
        return Convert.ToInt32(komut.ExecuteScalar()) == 0;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static void TablolariGarantiEt()
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS musteri (
                musteriID TEXT NOT NULL PRIMARY KEY,
                ad TEXT NOT NULL,
                soyad TEXT NOT NULL,
                telefon TEXT NOT NULL,
                adres TEXT NOT NULL,
                il TEXT NOT NULL DEFAULT '',
                ilce TEXT NOT NULL DEFAULT '',
                acikAdres TEXT NOT NULL DEFAULT '',
                postaKodu TEXT NOT NULL DEFAULT '',
                kimlikno TEXT NOT NULL UNIQUE,
                kayitTarihi TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS varlik (
                varlikID TEXT NOT NULL PRIMARY KEY,
                varliktipi TEXT NOT NULL,
                ad TEXT NOT NULL,
                kapasite INTEGER NOT NULL,
                gunlukucret REAL NOT NULL,
                durum TEXT NOT NULL,
                konum TEXT NOT NULL DEFAULT ''
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS rezervasyon (
                rezervasyonID TEXT NOT NULL PRIMARY KEY,
                musteriID TEXT NOT NULL,
                varlikID TEXT NOT NULL,
                bastarih TEXT NOT NULL,
                sontarih TEXT NOT NULL,
                toplamucret REAL NOT NULL,
                durum TEXT NOT NULL,
                kayitTarihi TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                CONSTRAINT FK_rezervasyon_musteri FOREIGN KEY (musteriID) REFERENCES musteri(musteriID),
                CONSTRAINT FK_rezervasyon_varlik FOREIGN KEY (varlikID) REFERENCES varlik(varlikID)
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS odeme (
                odemeID TEXT NOT NULL PRIMARY KEY,
                rezervasyonID TEXT NOT NULL,
                ucret REAL NOT NULL,
                odemetarihi TEXT NOT NULL,
                odemetipi TEXT NOT NULL,
                aciklama TEXT NOT NULL DEFAULT '',
                CONSTRAINT FK_odeme_rezervasyon FOREIGN KEY (rezervasyonID) REFERENCES rezervasyon(rezervasyonID)
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS operasyon (
                operasyonID TEXT NOT NULL PRIMARY KEY,
                varlikID TEXT NOT NULL,
                operasyonTipi TEXT NOT NULL,
                durum INTEGER NOT NULL,
                tarih TEXT NOT NULL,
                notlar TEXT NOT NULL DEFAULT '',
                CONSTRAINT FK_operasyon_varlik FOREIGN KEY (varlikID) REFERENCES varlik(varlikID)
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS hizmet (
                hizmetID TEXT NOT NULL PRIMARY KEY,
                rezervasyonID TEXT NOT NULL,
                ad TEXT NOT NULL,
                ucret REAL NOT NULL,
                CONSTRAINT FK_hizmet_rezervasyon FOREIGN KEY (rezervasyonID) REFERENCES rezervasyon(rezervasyonID)
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS kullanici (
                kullaniciID TEXT NOT NULL PRIMARY KEY,
                adSoyad TEXT NOT NULL,
                email TEXT NOT NULL UNIQUE,
                sifre TEXT NOT NULL,
                rol TEXT NOT NULL
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS isletme_ayarlari (
                ayarID TEXT NOT NULL PRIMARY KEY,
                isletmeAdi TEXT NOT NULL,
                paraBirimi TEXT NOT NULL,
                dil TEXT NOT NULL,
                rezervasyonBildirimleri INTEGER NOT NULL,
                temizlikUyarilari INTEGER NOT NULL
            )
            """,
            "CREATE INDEX IF NOT EXISTS IX_rezervasyon_varlik_tarih ON rezervasyon(varlikID, bastarih, sontarih, durum)"
        };

        foreach (var statement in statements)
        {
            DbHelper.KomutCalistir(statement);
        }

        SutunuGarantiEt("musteri", "il", "TEXT NOT NULL DEFAULT ''");
        SutunuGarantiEt("musteri", "ilce", "TEXT NOT NULL DEFAULT ''");
        SutunuGarantiEt("musteri", "acikAdres", "TEXT NOT NULL DEFAULT ''");
        SutunuGarantiEt("musteri", "postaKodu", "TEXT NOT NULL DEFAULT ''");
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static void SutunuGarantiEt(string table, string sutun, string definition)
    {
        var exists = DbHelper.TekDegerCalistir<int>(
            $"SELECT COUNT(1) FROM pragma_table_info('{table}') WHERE name = @sutun",
            DbHelper.Parametre("@sutun", sutun));
        if (exists == 0)
        {
            DbHelper.KomutCalistir($"ALTER TABLE {table} ADD COLUMN {sutun} {definition}");
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static void OrnekVeriEkle()
    {
        DbHelper.KomutCalistir(
            """
            INSERT INTO kullanici (kullaniciID, adSoyad, email, sifre, rol)
            SELECT 'USR-001', 'Ahmet Yılmaz', 'admin@tinytrack.local', '123456', 'Yönetici'
            WHERE NOT EXISTS (SELECT 1 FROM kullanici WHERE kullaniciID = 'USR-001')
            """);

        DbHelper.KomutCalistir(
            """
            INSERT INTO isletme_ayarlari (ayarID, isletmeAdi, paraBirimi, dil, rezervasyonBildirimleri, temizlikUyarilari)
            SELECT 'SET-001', 'TinyTrack ERP', 'Türk Lirası', 'Türkçe', 1, 1
            WHERE NOT EXISTS (SELECT 1 FROM isletme_ayarlari WHERE ayarID = 'SET-001')
            """);

        DbHelper.KomutCalistir(
            """
            INSERT INTO musteri (musteriID, ad, soyad, telefon, adres, il, ilce, acikAdres, postaKodu, kimlikno)
            SELECT 'MUS-001', 'Ahmet', 'Yılmaz', '05551234567', 'Kuzey Ormanları, Sakarya', 'Sakarya', 'Adapazarı', 'Kuzey Ormanları', '54000', '10000000146'
            WHERE NOT EXISTS (SELECT 1 FROM musteri WHERE musteriID = 'MUS-001')
            """);

        DbHelper.KomutCalistir(
            """
            INSERT INTO musteri (musteriID, ad, soyad, telefon, adres, il, ilce, acikAdres, postaKodu, kimlikno)
            SELECT 'MUS-002', 'Selim', 'Aras', '05557654321', 'Ege Kıyıları, Muğla', 'Muğla', 'Bodrum', 'Ege Kıyıları', '48400', '10000000214'
            WHERE NOT EXISTS (SELECT 1 FROM musteri WHERE musteriID = 'MUS-002')
            """);

        DbHelper.KomutCalistir(
            """
            UPDATE musteri
            SET kimlikno = '10000000214'
            WHERE musteriID = 'MUS-002' AND kimlikno = '10000000284'
            """);

        DbHelper.KomutCalistir(
            """
            INSERT INTO varlik (varlikID, varliktipi, ad, kapasite, gunlukucret, durum, konum)
            SELECT 'VAR-001', 'Tiny House', 'Tiny House 1', 3, 2500, 'Dolu', 'Kuzey Ormanları, Sakarya'
            WHERE NOT EXISTS (SELECT 1 FROM varlik WHERE varlikID = 'VAR-001')
            """);

        DbHelper.KomutCalistir(
            """
            INSERT INTO varlik (varlikID, varliktipi, ad, kapasite, gunlukucret, durum, konum)
            SELECT 'VAR-002', 'Karavan', 'Caravan 2', 4, 1800, 'Temizlikte', 'Ege Kiyilari, Mugla'
            WHERE NOT EXISTS (SELECT 1 FROM varlik WHERE varlikID = 'VAR-002')
            """);

        DbHelper.KomutCalistir(
            """
            INSERT INTO varlik (varlikID, varliktipi, ad, kapasite, gunlukucret, durum, konum)
            SELECT 'VAR-003', 'Bungalov', 'Modern Loft', 2, 3200, 'Musait', 'Abant, Bolu'
            WHERE NOT EXISTS (SELECT 1 FROM varlik WHERE varlikID = 'VAR-003')
            """);

        DbHelper.KomutCalistir(
            """
            INSERT INTO rezervasyon (rezervasyonID, musteriID, varlikID, bastarih, sontarih, toplamucret, durum)
            SELECT 'REZ-001', 'MUS-001', 'VAR-001', date('now'), date('now', '+3 day'), 7500, 'Aktif'
            WHERE NOT EXISTS (SELECT 1 FROM rezervasyon WHERE rezervasyonID = 'REZ-001')
            """);

        DbHelper.KomutCalistir(
            """
            INSERT INTO rezervasyon (rezervasyonID, musteriID, varlikID, bastarih, sontarih, toplamucret, durum)
            SELECT 'REZ-002', 'MUS-002', 'VAR-003', date('now', '+5 day'), date('now', '+8 day'), 9600, 'Aktif'
            WHERE NOT EXISTS (SELECT 1 FROM rezervasyon WHERE rezervasyonID = 'REZ-002')
            """);

        DbHelper.KomutCalistir(
            """
            INSERT INTO operasyon (operasyonID, varlikID, operasyonTipi, durum, tarih, notlar)
            SELECT 'OPR-001', 'VAR-002', 'Temizlik', 0, date('now'), 'Standart cikis temizligi devam ediyor'
            WHERE NOT EXISTS (SELECT 1 FROM operasyon WHERE operasyonID = 'OPR-001')
            """);
    }
}
