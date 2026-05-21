# TinyTrack ERP

TinyTrack ERP, tiny house, butik bungalov ve kiralik karavan isletmeleri icin hazirlanmis katmanli mimariye sahip bir Windows Forms final projesidir. Proje; rezervasyon, varlik, musteri, operasyon, finans, ekstra hizmet, dashboard ve ayarlar sureclerini tek uygulamada toplar.

## Proje Yapisi

- `TinyTrack.Entities`: POCO entity siniflari ve enum tipleri.
- `TinyTrack.DataAccess`: SQL Server DAL siniflari, parametreli sorgular, `DbHelper`, veritabani olusturma/seed islemleri.
- `TinyTrack.Business`: Manager siniflari, validasyonlar ve is kurallari.
- `TinyTrack.WinForms`: Rapordaki ekran tasarimina uygun Windows Forms arayuzu.
- `Database/TinyTrackDb.sql`: Manuel veritabani kurulum scripti.

## Calistirma

1. Visual Studio veya terminal ile `TinyTrackERP.slnx` dosyasini acin.
2. `TinyTrack.WinForms` projesini baslangic projesi yapin.
3. `TinyTrack.WinForms/appsettings.json` icindeki connection string varsayilan olarak LocalDB kullanir.
4. Uygulamayi calistirin. Ilk acilista `TinyTrackDb` veritabani, tablolar ve ornek veriler otomatik olusturulur.

Varsayilan kullanici:

- E-posta: `admin@tinytrack.local`
- Sifre: `123456`

## Rubrik Karsiliklari

- Katmanli mimari: UI -> Business -> DataAccess -> Database.
- UI katmanindan dogrudan SQL yoktur.
- Her ana tablo icin Entity, Manager ve Dal sinifi vardir.
- CRUD operasyonlari Musteri, Varlik, Rezervasyon, Odeme, Operasyon ve Hizmet modullerinde uygulanmistir.
- SQL sorgulari `SqlParameter` ile parametrelidir.
- Baglanti yonetimi merkezi `DbHelper` uzerindedir.
- BLL tarafinda cift rezervasyon kontrolu, T.C. kimlik dogrulama, tarih validasyonu, odeme/kalan tutar kontrolu, cikis sonrasi temizlik akisi vardir.
- Arayuz, vize raporundaki dashboard, rezervasyon, varlik ve ayarlar ekranlarinin gorsel diline gore tasarlanmistir.

## Demo Akisi

Video sunumda onerilen canli demo:

1. Dashboard acilir; giris/cikis, temizlikteki varlik, aylik gelir ve doluluk anlatilir.
2. Musteri ekraninda yeni musteri eklenir ve listede gosterilir.
3. Varlik ekraninda tiny house/karavan kaydi ve durum guncelleme gosterilir.
4. Rezervasyon ekraninda musteri + varlik + tarih secilerek rezervasyon olusturulur.
5. Ayni tarihe ikinci rezervasyon denenir ve cakismanin engellendigi gosterilir.
6. Finans ekraninda odeme ve ekstra hizmet eklenir.
7. Rezervasyon cikisi yapilir; operasyon ekraninda temizlik kaydi gorunur.
8. Ayarlar ekraninda profil/isletme ayarlari gosterilir.
