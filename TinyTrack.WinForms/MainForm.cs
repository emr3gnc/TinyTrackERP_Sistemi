using System.ComponentModel;
using System.Globalization;
using ClosedXML.Excel;
using TinyTrack.Business;
using TinyTrack.Entities;

namespace TinyTrack.WinForms;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public partial class MainForm : Form
{
    private const string CurrentUserId = "USR-001";

    private readonly Color _background = Color.FromArgb(241, 246, 250);
    private readonly Color _card = Color.White;
    private readonly Color _ink = Color.FromArgb(16, 24, 40);
    private readonly Color _muted = Color.FromArgb(102, 112, 133);
    private readonly Color _line = Color.FromArgb(229, 234, 242);
    private readonly Color _green = ColorTranslator.FromHtml("#6d47d5");
    private readonly Color _greenSoft = ColorTranslator.FromHtml("#bc84fb");
    private readonly Color _danger = ColorTranslator.FromHtml("#bf0a1f");
    private readonly Color _navy = Color.FromArgb(17, 24, 39);
    private readonly Color _yellow = Color.FromArgb(249, 181, 38);
    private readonly Color _blue = Color.FromArgb(37, 99, 235);
    private readonly Color _coral = Color.FromArgb(239, 68, 68);
    private readonly Color _violet = Color.FromArgb(124, 58, 237);
    private readonly Color _orange = Color.FromArgb(245, 158, 11);
    private readonly Color _calendarAvailable = ColorTranslator.FromHtml("#42992c");
    private readonly Color _calendarOperation = ColorTranslator.FromHtml("#96800f");
    private readonly Color _calendarOccupied = ColorTranslator.FromHtml("#cf2732");

    private readonly Panel _sidebarPanel = new();
    private readonly Panel _contentPanel = new();
    private readonly Label _headerUserLabel = new();
    private readonly Dictionary<string, Button> _navButtons = [];
    private DateTime _selectedCalendarDate = DateTime.Today;
    private DateTime _reservationWeekStart = HaftaBaslangici(DateTime.Today);
    private DateTime _newReservationCalendarMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private string _currentPageKey = "Dashboard";
    private bool _isRenderingPage;
    private int _lastContentWidth;

    private readonly MusteriManager _musteriManager = new();
    private readonly VarlikManager _varlikManager = new();
    private readonly RezervasyonManager _rezervasyonManager = new();
    private readonly OdemeManager _odemeManager = new();
    private readonly OperasyonManager _operasyonManager = new();
    private readonly HizmetManager _hizmetManager = new();
    private readonly ProfilManager _profilManager = new();
    private readonly YoneticiManager _yoneticiManager = new();

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public MainForm()
    {
        InitializeComponent();
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");

        KabuguOlustur();
        PaneliGoster();
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void KabuguOlustur()
    {
        _sidebarPanel.Dock = DockStyle.Left;
        _sidebarPanel.Width = 238;
        _sidebarPanel.BackColor = _greenSoft;
        _sidebarPanel.Padding = new Padding(18, 18, 18, 18);
        _sidebarPanel.Paint += (_, e) =>
        {
            using var pen = new Pen(_line);
            e.Graphics.DrawLine(pen, _sidebarPanel.Width - 1, 0, _sidebarPanel.Width - 1, _sidebarPanel.Height);
        };

        var logo = new PictureBox
        {
            Left = 20,
            Top = 22,
            Width = 48,
            Height = 48,
            BackColor = Color.Transparent,
            SizeMode = PictureBoxSizeMode.Zoom
        };
        var logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "tinytracklogo.png");
        if (File.Exists(logoPath))
        {
            using var logoImage = Image.FromFile(logoPath);
            logo.Image = new Bitmap(logoImage);
        }
        else
        {
            logo.BackColor = _green;
        }

        _sidebarPanel.Controls.Add(logo);

        var brand = new Label
        {
            Text = "TinyTrack",
            AutoSize = false,
            Width = 150,
            Height = 50,
            Left = 72,
            Top = 21,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.White
        };
        _sidebarPanel.Controls.Add(brand);

        _headerUserLabel.AutoSize = false;
        _headerUserLabel.Width = 198;
        _headerUserLabel.Height = 40;
        _headerUserLabel.Left = 20;
        _headerUserLabel.Top = 724;
        _headerUserLabel.TextAlign = ContentAlignment.MiddleLeft;
        _headerUserLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _headerUserLabel.ForeColor = Color.White;
        _headerUserLabel.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        _headerUserLabel.AutoEllipsis = true;
        _sidebarPanel.Controls.Add(_headerUserLabel);
        BaslikKullanicisiniYenile();

        var nav = new FlowLayoutPanel
        {
            Left = 18,
            Top = 104,
            Width = 202,
            Height = 420,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0),
            BackColor = _greenSoft,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        _sidebarPanel.Controls.Add(nav);

        GezintiDugmesiEkle(nav, "Dashboard", "Panel", PaneliGoster);
        GezintiDugmesiEkle(nav, "YeniRezervasyon", "Yeni Rezervasyon", YeniRezervasyonuGoster);
        GezintiDugmesiEkle(nav, "Rezervasyonlar", "Takvim", RezervasyonlariGoster);
        GezintiDugmesiEkle(nav, "Finans", "Ödeme ve Çıkış", FinansiGoster);
        GezintiDugmesiEkle(nav, "Varliklar", "Varlıklar", VarliklariGoster);
        GezintiDugmesiEkle(nav, "Musteriler", "Müşteriler", MusterileriGoster);
        GezintiDugmesiEkle(nav, "Operasyon", "Operasyon", OperasyonlariGoster);
        GezintiDugmesiEkle(nav, "Ayarlar", "Ayarlar", AyarlariGoster);

        _contentPanel.Dock = DockStyle.Fill;
        _contentPanel.AutoScroll = true;
        _contentPanel.BackColor = _background;
        _contentPanel.Padding = new Padding(30, 28, 30, 36);
        _contentPanel.Resize += (_, _) => GerekirseGuncelSayfayiYenidenCiz();
        Controls.Add(_contentPanel);
        Controls.Add(_sidebarPanel);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private Kullanici GetCurrentUser()
    {
        return _profilManager.KullaniciProfilBilgileriniGetir(CurrentUserId) ?? new Kullanici
        {
            KullaniciID = CurrentUserId,
            AdSoyad = "Yönetici",
            Email = "admin@tinytrack.local",
            Rol = "Yönetici"
        };
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void BaslikKullanicisiniYenile()
    {
        try
        {
            var user = GetCurrentUser();
            _headerUserLabel.Text = string.IsNullOrWhiteSpace(user.AdSoyad) ? "Yönetici" : user.AdSoyad;
        }
        catch
        {
            _headerUserLabel.Text = "Yönetici";
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void GezintiDugmesiEkle(FlowLayoutPanel nav, string key, string text, Action islem)
    {
        var dugme = new Button
        {
            Text = text,
            Width = 202,
            Height = 44,
            Margin = new Padding(0, 0, 0, 8),
            FlatStyle = FlatStyle.Flat,
            BackColor = _greenSoft,
            ForeColor = _muted,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TabStop = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 0, 0)
        };
        dugme.FlatAppearance.BorderSize = 0;
        dugme.Click += (_, _) => islem();
        _navButtons[key] = dugme;
        nav.Controls.Add(dugme);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void AktifGezintiyiAyarla(string key)
    {
        foreach (var item in _navButtons)
        {
            item.Value.BackColor = item.Key == key ? _green : _greenSoft;
            item.Value.ForeColor = Color.White;
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void SayfayiHazirla(string key)
    {
        _currentPageKey = key;
        _isRenderingPage = true;
        AktifGezintiyiAyarla(key);
        _contentPanel.SuspendLayout();
        _contentPanel.Controls.Clear();
        _contentPanel.AutoScrollPosition = Point.Empty;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void SayfayiBitir()
    {
        _lastContentWidth = IcerikGenisligi();
        _isRenderingPage = false;
        _contentPanel.ResumeLayout();
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void GerekirseGuncelSayfayiYenidenCiz()
    {
        if (_isRenderingPage || Math.Abs(IcerikGenisligi() - _lastContentWidth) < 48)
        {
            return;
        }

        switch (_currentPageKey)
        {
            case "Rezervasyonlar":
                RezervasyonlariGoster();
                break;
            case "YeniRezervasyon":
                YeniRezervasyonuGoster();
                break;
            case "Varliklar":
                VarliklariGoster();
                break;
            case "Musteriler":
                MusterileriGoster();
                break;
            case "Operasyon":
                OperasyonlariGoster();
                break;
            case "Finans":
                FinansiGoster();
                break;
            case "Ayarlar":
                AyarlariGoster();
                break;
            default:
                PaneliGoster();
                break;
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void PaneliGoster()
    {
        SayfayiHazirla("Dashboard");
        try
        {
            var dashboard = _yoneticiManager.DashboardVeriOzetVerileriniGetir();
            var user = GetCurrentUser();
            BaslikEkle($"Günaydin, {KisaAd(user.AdSoyad)}", DateTime.Today.ToString("dd MMMM dddd, yyyy"), null, null);
            var wideStats = IcerikGenisligi() >= 1080;
            var statsHeight = wideStats ? 145 : 280;

            var stats = new FlowLayoutPanel
            {
                Left = 0,
                Top = 126,
                Width = IcerikGenisligi(),
                Height = statsHeight,
                BackColor = _background,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };
            _contentPanel.Controls.Add(stats);

            stats.Controls.Add(IstatistikKartiOlustur("GİRİŞLER", dashboard.BugunkuGiris.ToString("00"), "Bugün", _green));
            stats.Controls.Add(IstatistikKartiOlustur("ÇIKIŞLAR", dashboard.BugunkuCikis.ToString("00"), "Bugün", _blue));
            stats.Controls.Add(IstatistikKartiOlustur("TEMİZLİKTE", dashboard.TemizliktekiVarlik.ToString("00"), "Birim", _coral));
            stats.Controls.Add(IstatistikKartiOlustur("AKTIF REZ.", dashboard.AktifRezervasyon.ToString(), "Kayit", _navy));

            var chartTop = 126 + statsHeight + 30;
            var chart = KartOlustur(0, chartTop, Math.Min(720, IcerikGenisligi()), 215, _blue);
            _contentPanel.Controls.Add(chart);
            chart.Controls.Add(EtiketOlustur("AYLIK GELIR", 18, 22, 14, FontStyle.Bold, _muted));
            chart.Controls.Add(EtiketOlustur(dashboard.AylikGelir.ToString("C0"), 18, 48, 25, FontStyle.Bold, _ink));
            chart.Controls.Add(RozetOlustur($"%{Math.Max(0, dashboard.DolulukOrani):0.0} doluluk", chart.Width - 145, 24, 120, _greenSoft, _green));
            chart.Paint += (_, e) => GelirCubuklariniCiz(e.Graphics, chart.ClientRectangle);

            var quickTop = chartTop + 240;
            var quick = KartOlustur(0, quickTop, Math.Min(720, IcerikGenisligi()), 160, _navy);
            _contentPanel.Controls.Add(quick);
            quick.Controls.Add(EtiketOlustur("HIZLI ISLEMLER", 22, 18, 14, FontStyle.Bold, _ink));
            quick.Controls.Add(IslemDugmesiOlustur("Yeni Rezervasyon", 22, 58, 280, _navy, YeniRezervasyonuGoster));
            quick.Controls.Add(IslemDugmesiOlustur("Varlık Durumu", 322, 58, 280, Color.White, VarliklariGoster, _ink));

            var wideLayout = IcerikGenisligi() >= 1180;
            var nextLeft = wideLayout ? 760 : 0;
            var nextTop = wideLayout ? chartTop : quickTop + 185;
            var nextWidth = wideLayout ? Math.Max(360, IcerikGenisligi() - 760) : Math.Min(720, IcerikGenisligi());
            var next = KartOlustur(nextLeft, nextTop, nextWidth, 400, _green);
            _contentPanel.Controls.Add(next);
            next.Controls.Add(EtiketOlustur("SIRADAKI GIRIS", 22, 22, 14, FontStyle.Bold, _green));
            next.Controls.Add(EtiketOlustur(dashboard.SiradakiGiris, 22, 58, 18, FontStyle.Bold, _ink, next.Width - 44));
            next.Controls.Add(EtiketOlustur("Takvim ve operasyon ekranlarindan bugunku sureci kontrol edebilirsiniz.", 22, 100, 10.5F, FontStyle.Regular, _muted, next.Width - 44));
            next.Controls.Add(IslemDugmesiOlustur("Rezervasyonları Aç", 22, 155, 230, _green, RezervasyonlariGoster));
        }
        catch (Exception ex)
        {
            SayfaHatasiniGoster(ex);
        }
        finally
        {
            SayfayiBitir();
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void RezervasyonlariGoster()
    {
        SayfayiHazirla("Rezervasyonlar");
        try
        {
            BaslikEkle("Takvim", "Haftalık takvimden gün seçerek rezervasyonları inceleyin.", null, null);
            var calendarWidth = Math.Min(920, IcerikGenisligi());
            var calendarLeft = Math.Max(0, (IcerikGenisligi() - calendarWidth) / 2);
            var gridWidth = Math.Min(1000, IcerikGenisligi());
            var gridLeft = Math.Max(0, (IcerikGenisligi() - gridWidth) / 2);

            RezervasyonHaftaTakvimiEkle(calendarLeft, 126, calendarWidth, selectedDate =>
            {
                _selectedCalendarDate = selectedDate.Date;
                RezervasyonlariGoster();
            });

            var dayTitle = EtiketOlustur($"{_selectedCalendarDate:dd MMMM yyyy dddd} rezervasyonları", gridLeft, 330, 18, FontStyle.Bold, _ink, gridWidth);
            _contentPanel.Controls.Add(dayTitle);

            var tablo = TabloOlustur<Rezervasyon>(gridLeft, 380, gridWidth, 430);
            _contentPanel.Controls.Add(tablo);
            void RefreshGrid()
            {
                var selectedDate = _selectedCalendarDate.Date;
                var rezervasyonlar = _rezervasyonManager.RezervasyonlariGetir()
                    .Where(r => r.Durum == RezervasyonDurumu.Aktif && RezervasyonTarihiKapsar(r, selectedDate))
                    .OrderBy(r => r.BasTarih)
                    .ToList();
                tablo.DataSource = new BindingList<Rezervasyon>(rezervasyonlar);
                SutunlariGizle(tablo, "MusteriID", "VarlikID", "KayitTarihi");
                SutunBasliklariniAyarla(tablo,
                    ("RezervasyonID", "Rezervasyon"),
                    ("BasTarih", "Giriş"),
                    ("SonTarih", "Çıkış"),
                    ("ToplamUcret", "Toplam"),
                    ("MusteriAdSoyad", "Müşteri"),
                    ("VarlikAdi", "Varlık"),
                    ("GeceSayisi", "Gece"));
            }

            RefreshGrid();
        }
        catch (Exception ex)
        {
            SayfaHatasiniGoster(ex);
        }
        finally
        {
            SayfayiBitir();
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void YeniRezervasyonuGoster()
    {
        SayfayiHazirla("YeniRezervasyon");
        try
        {
            BaslikEkle("Yeni Rezervasyon", "Müşteri bilgisi, varlık ve tarih aralığıyla yeni konaklama oluşturun.", null, null);
            var wideLayout = IcerikGenisligi() >= 1040;
            var formWidth = Math.Min(430, IcerikGenisligi());
            var calendarLeft = wideLayout ? formWidth + 24 : 0;
            var calendarTop = wideLayout ? 126 : 900;
            var calendarWidth = wideLayout ? Math.Max(520, IcerikGenisligi() - calendarLeft) : IcerikGenisligi();

            var form = KartOlustur(0, 126, formWidth, 635, _green);
            _contentPanel.Controls.Add(form);
            form.Controls.Add(EtiketOlustur("Rezervasyon Formu", 24, 24, 18, FontStyle.Bold, _ink, 390));

            var cmbVarlik = SecimKutusuEkle<Varlik>(form, "Varlık", 24, 100, 370, _varlikManager.VarliklariGetir(), "Ad", "VarlikID");
            var dtBas = TarihAlaniEkle(form, "Giriş Tarihi", 24, 168, 175, _selectedCalendarDate);
            var dtSon = TarihAlaniEkle(form, "Çıkış Tarihi", 219, 168, 175, _selectedCalendarDate.AddDays(1));
            var totalLabel = EtiketOlustur("Toplam: 0 TL", 24, 232, 13, FontStyle.Bold, _green, 370);
            form.Controls.Add(totalLabel);
            form.Controls.Add(EtiketOlustur("Müşteri Bilgileri", 24, 282, 16, FontStyle.Bold, _ink, 370));
            var txtMusteriAd = MetinKutusuEkle(form, "Ad", 24, 348, 170);
            var txtMusteriSoyad = MetinKutusuEkle(form, "Soyad", 218, 348, 170);
            var txtMusteriKimlik = MetinKutusuEkle(form, "T.C. Kimlik No", 24, 418, 364);
            var txtMusteriTelefon = MetinKutusuEkle(form, "Telefon", 24, 488, 170);
            var cmbMusteriIl = SecimKutusuEkle<string>(form, "İl", 218, 488, 170, TurkSehirleri(), null, null);
            var cmbMusteriIlce = SecimKutusuEkle<string>(form, "İlçe", 24, 558, 170, IlceleriGetir(cmbMusteriIl.Text), null, null);
            var txtMusteriPosta = MetinKutusuEkle(form, "Posta Kodu", 218, 558, 170);
            var txtMusteriAcikAdres = MetinKutusuEkle(form, "Açık Adres", 24, 628, 364);
            cmbMusteriIl.SelectedIndexChanged += (_, _) => IlceleriYenidenBagla(cmbMusteriIlce, cmbMusteriIl.Text);
            void RefreshTotal()
            {
                if (cmbVarlik.SelectedValue is string varlikID)
                {
                    GuvenliCalistir(() =>
                    {
                        totalLabel.Text = $"Toplam: {_rezervasyonManager.ToplamUcretHesapla(varlikID, dtBas.Value, dtSon.Value, 0):C0}";
                    });
                }
            }
            void RefreshAssetCalendar()
            {
                var varlikID = cmbVarlik.SelectedValue?.ToString() ?? string.Empty;
                VarlikTakvimiEkle(calendarLeft, calendarTop, calendarWidth, varlikID, tarih =>
                {
                    dtBas.Value = tarih;
                    if (dtSon.Value.Date <= tarih.Date)
                    {
                        dtSon.Value = tarih.AddDays(1);
                    }
                    RefreshTotal();
                });
            }

            dtBas.ValueChanged += (_, _) => RefreshTotal();
            dtSon.ValueChanged += (_, _) => RefreshTotal();
            cmbVarlik.SelectedIndexChanged += (_, _) =>
            {
                RefreshTotal();
                RefreshAssetCalendar();
            };
            txtMusteriKimlik.Leave += (_, _) =>
            {
                var mevcut = _musteriManager.MusteriKimlikNoIleGetir(txtMusteriKimlik.Text);
                if (mevcut is null)
                {
                    return;
                }

                txtMusteriAd.Text = mevcut.Ad;
                txtMusteriSoyad.Text = mevcut.Soyad;
                txtMusteriTelefon.Text = mevcut.Telefon;
                SecimKutusuDegeriniSec(cmbMusteriIl, mevcut.Il);
                IlceleriYenidenBagla(cmbMusteriIlce, cmbMusteriIl.Text);
                SecimKutusuDegeriniSec(cmbMusteriIlce, mevcut.Ilce);
                txtMusteriAcikAdres.Text = string.IsNullOrWhiteSpace(mevcut.AcikAdres) ? mevcut.Adres : mevcut.AcikAdres;
                txtMusteriPosta.Text = mevcut.PostaKodu;
            };

            form.Height = 735;
            form.Controls.Add(IslemDugmesiOlustur("Kaydet", 24, 678, 170, _green, () => GuvenliCalistir(() =>
            {
                var musteri = RezervasyonMusterisiniBulVeyaOlustur(
                    txtMusteriAd.Text,
                    txtMusteriSoyad.Text,
                    txtMusteriKimlik.Text,
                    txtMusteriTelefon.Text,
                    cmbMusteriIl.Text,
                    cmbMusteriIlce.Text,
                    txtMusteriAcikAdres.Text,
                    txtMusteriPosta.Text);
                _rezervasyonManager.RezervasyonEkle(new Rezervasyon
                {
                    MusteriID = musteri.MusteriID,
                    VarlikID = cmbVarlik.SelectedValue?.ToString() ?? string.Empty,
                    BasTarih = dtBas.Value,
                    SonTarih = dtSon.Value
                });
                _selectedCalendarDate = dtBas.Value.Date;
                RezervasyonlariGoster();
            }, "Rezervasyon kaydedildi.")));

            form.Controls.Add(IslemDugmesiOlustur("Takvime Dön", 214, 678, 170, Color.White, RezervasyonlariGoster, _ink));
            RefreshTotal();
            RefreshAssetCalendar();
        }
        catch (Exception ex)
        {
            SayfaHatasiniGoster(ex);
        }
        finally
        {
            SayfayiBitir();
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void MusterileriGoster()
    {
        SayfayiHazirla("Musteriler");
        try
        {
            BaslikEkle("Müşteriler", "Kimlik, iletişim ve konaklama müşteri kayıtları.", null, null);

            var wideLayout = IcerikGenisligi() >= 940;
            var formWidth = Math.Min(420, IcerikGenisligi());
            var gridLeft = wideLayout ? formWidth + 20 : 0;
            var gridTop = wideLayout ? 126 : 800;
            var gridWidth = wideLayout ? IcerikGenisligi() - gridLeft : IcerikGenisligi();

            var form = KartOlustur(0, 126, formWidth, 500, _blue);
            _contentPanel.Controls.Add(form);
            form.Height = 650;
            form.Controls.Add(EtiketOlustur("Müşteri Bilgileri", 24, 24, 20, FontStyle.Bold, _ink, 364));
            var txtAd = MetinKutusuEkle(form, "Ad", 24, 100, 170);
            var txtSoyad = MetinKutusuEkle(form, "Soyad", 218, 100, 170);
            var txtKimlik = MetinKutusuEkle(form, "T.C. Kimlik No", 24, 170, 364);
            var txtTelefon = MetinKutusuEkle(form, "Telefon", 24, 240, 364);
            var cmbIl = SecimKutusuEkle<string>(form, "İl", 24, 310, 170, TurkSehirleri(), null, null);
            var cmbIlce = SecimKutusuEkle<string>(form, "İlçe", 218, 310, 170, IlceleriGetir(cmbIl.Text), null, null);
            var txtAcikAdres = MetinKutusuEkle(form, "Açık Adres", 24, 380, 364);
            var txtPosta = MetinKutusuEkle(form, "Posta Kodu", 24, 450, 170);
            cmbIl.SelectedIndexChanged += (_, _) => IlceleriYenidenBagla(cmbIlce, cmbIl.Text);
            var tablo = TabloOlustur<Musteri>(gridLeft, gridTop, gridWidth, 500);
            _contentPanel.Controls.Add(tablo);
            var selectedID = string.Empty;
            void RefreshGrid()
            {
                tablo.DataSource = new BindingList<Musteri>(_musteriManager.MusterileriGetir());
                SutunlariGizle(tablo, "KayitTarihi", "AdSoyad", "Adres", "TamAdres");
                SutunBasliklariniAyarla(tablo,
                    ("MusteriID", "Müşteri No"),
                    ("KimlikNo", "T.C. Kimlik"),
                    ("Telefon", "Telefon"),
                    ("Il", "İl"),
                    ("Ilce", "İlçe"),
                    ("AcikAdres", "Açık Adres"),
                    ("PostaKodu", "Posta Kodu"));
            }

            tablo.SelectionChanged += (_, _) =>
            {
                if (tablo.CurrentRow?.DataBoundItem is not Musteri m)
                {
                    return;
                }

                selectedID = m.MusteriID;
                txtAd.Text = m.Ad;
                txtSoyad.Text = m.Soyad;
                txtKimlik.Text = m.KimlikNo;
                txtTelefon.Text = m.Telefon;
                SecimKutusuDegeriniSec(cmbIl, m.Il);
                IlceleriYenidenBagla(cmbIlce, cmbIl.Text);
                SecimKutusuDegeriniSec(cmbIlce, m.Ilce);
                txtAcikAdres.Text = string.IsNullOrWhiteSpace(m.AcikAdres) ? m.Adres : m.AcikAdres;
                txtPosta.Text = m.PostaKodu;
            };

            form.Controls.Add(IslemDugmesiOlustur("Güncelle", 24, 560, 170, _green, () => GuvenliCalistir(() =>
            {
                _musteriManager.MusteriGuncelle(new Musteri
                {
                    MusteriID = selectedID,
                    Ad = txtAd.Text,
                    Soyad = txtSoyad.Text,
                    KimlikNo = txtKimlik.Text,
                    Telefon = SadeceRakamlar(txtTelefon.Text),
                    Il = cmbIl.Text,
                    Ilce = cmbIlce.Text,
                    AcikAdres = txtAcikAdres.Text,
                    PostaKodu = txtPosta.Text
                });
                RefreshGrid();
            }, "Müşteri güncellendi.")));

            form.Controls.Add(IslemDugmesiOlustur("Sil", 218, 560, 170, Color.FromArgb(249, 232, 232), () =>
            {
                if (Onayla("Seçili müşteri silinsin mi?"))
                {
                    GuvenliCalistir(() =>
                    {
                        _musteriManager.MusteriSil(selectedID);
                        RefreshGrid();
                    }, "Müşteri silindi.");
                }
            }, Color.FromArgb(190, 45, 45)));

            RefreshGrid();
        }
        catch (Exception ex)
        {
            SayfaHatasiniGoster(ex);
        }
        finally
        {
            SayfayiBitir();
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void VarliklariGoster()
    {
        SayfayiHazirla("Varliklar");
        try
        {
            BaslikEkle("Varlıklar", "Tiny house, karavan ve bungalov durumlarını yönetin.", null, null);

            var wideLayout = IcerikGenisligi() >= 940;
            var formWidth = Math.Min(420, IcerikGenisligi());
            var gridLeft = wideLayout ? formWidth + 20 : 0;
            var gridTop = wideLayout ? 126 : 650;
            var gridWidth = wideLayout ? IcerikGenisligi() - gridLeft : IcerikGenisligi();

            var form = KartOlustur(0, 126, formWidth, 500, _green);
            _contentPanel.Controls.Add(form);
            form.Controls.Add(EtiketOlustur("Varlık Kaydı", 24, 24, 20, FontStyle.Bold, _ink, 364));
            var cmbTip = SecimKutusuEkle<string>(form, "Tip", 24, 100, 170, ["Tiny House", "Karavan", "Bungalov"], null, null);
            var cmbDurum = SecimKutusuEkle<VarlikDurumu>(form, "Durum", 218, 100, 170, Enum.GetValues<VarlikDurumu>().ToList(), null, null);
            var txtAd = MetinKutusuEkle(form, "Ad", 24, 170, 364);
            var txtKonum = MetinKutusuEkle(form, "Konum", 24, 240, 364);
            var numKapasite = SayisalAlanEkle(form, "Kapasite", 24, 310, 170, 1, 20);
            var numUcret = SayisalAlanEkle(form, "Günlük Ücret", 218, 310, 170, 100, 100000);
            var tablo = TabloOlustur<Varlik>(gridLeft, gridTop, gridWidth, 500);
            _contentPanel.Controls.Add(tablo);
            var selectedID = string.Empty;
            void RefreshGrid()
            {
                tablo.DataSource = new BindingList<Varlik>(_varlikManager.VarliklariGetir());
                SutunBasliklariniAyarla(tablo,
                    ("VarlikID", "Varlık No"),
                    ("VarlikTipi", "Tip"),
                    ("GunlukUcret", "Günlük Ücret"),
                    ("Kapasite", "Kapasite"),
                    ("Durum", "Durum"),
                    ("Konum", "Konum"));
            }

            tablo.SelectionChanged += (_, _) =>
            {
                if (tablo.CurrentRow?.DataBoundItem is not Varlik v)
                {
                    return;
                }

                selectedID = v.VarlikID;
                cmbTip.SelectedItem = v.VarlikTipi;
                cmbDurum.SelectedItem = v.Durum;
                txtAd.Text = v.Ad;
                txtKonum.Text = v.Konum;
                numKapasite.Value = Math.Clamp(v.Kapasite, (int)numKapasite.Minimum, (int)numKapasite.Maximum);
                numUcret.Value = Math.Clamp(v.GunlukUcret, numUcret.Minimum, numUcret.Maximum);
            };

            form.Controls.Add(IslemDugmesiOlustur("Kaydet", 24, 390, 170, _green, () => GuvenliCalistir(() =>
            {
                _varlikManager.VarlikEkle(new Varlik
                {
                    VarlikTipi = cmbTip.Text,
                    Durum = cmbDurum.SelectedItem is VarlikDurumu d ? d : VarlikDurumu.Musait,
                    Ad = txtAd.Text,
                    Konum = txtKonum.Text,
                    Kapasite = (int)numKapasite.Value,
                    GunlukUcret = numUcret.Value
                });
                RefreshGrid();
            }, "Varlık kaydedildi.")));

            form.Controls.Add(IslemDugmesiOlustur("Güncelle", 218, 390, 170, Color.White, () => GuvenliCalistir(() =>
            {
                _varlikManager.VarlikGuncelle(new Varlik
                {
                    VarlikID = selectedID,
                    VarlikTipi = cmbTip.Text,
                    Durum = cmbDurum.SelectedItem is VarlikDurumu d ? d : VarlikDurumu.Musait,
                    Ad = txtAd.Text,
                    Konum = txtKonum.Text,
                    Kapasite = (int)numKapasite.Value,
                    GunlukUcret = numUcret.Value
                });
                RefreshGrid();
            }, "Varlık güncellendi."), _ink));

            form.Controls.Add(IslemDugmesiOlustur("Sil", 24, 442, 364, Color.FromArgb(249, 232, 232), () =>
            {
                if (Onayla("Seçili varlık silinsin mi?"))
                {
                    GuvenliCalistir(() =>
                    {
                        _varlikManager.VarlikSil(selectedID);
                        RefreshGrid();
                    }, "Varlık silindi.");
                }
            }, Color.FromArgb(190, 45, 45)));

            RefreshGrid();
        }
        catch (Exception ex)
        {
            SayfaHatasiniGoster(ex);
        }
        finally
        {
            SayfayiBitir();
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void OperasyonlariGoster()
    {
        SayfayiHazirla("Operasyon");
        try
        {
            BaslikEkle("Operasyon", "Temizlik ve bakim sureclerini takip edin.", null, null);
            var wideLayout = IcerikGenisligi() >= 940;
            var formWidth = Math.Min(420, IcerikGenisligi());
            var gridLeft = wideLayout ? formWidth + 20 : 0;
            var gridTop = wideLayout ? 126 : 580;
            var gridWidth = wideLayout ? IcerikGenisligi() - gridLeft : IcerikGenisligi();

            var form = KartOlustur(0, 126, formWidth, 430, _orange);
            _contentPanel.Controls.Add(form);
            form.Controls.Add(EtiketOlustur("Operasyon Kaydı", 24, 24, 20, FontStyle.Bold, _ink, 364));
            var cmbVarlik = SecimKutusuEkle<Varlik>(form, "Varlık", 24, 100, 364, _varlikManager.VarliklariGetir(), "Ad", "VarlikID");
            var cmbTip = SecimKutusuEkle<OperasyonTipi>(form, "Tip", 24, 168, 170, Enum.GetValues<OperasyonTipi>().ToList(), null, null);
            var chkDurum = new CheckBox
            {
                Text = "Tamamlandi",
                Left = 218,
                Top = 194,
                Width = 160,
                ForeColor = _ink,
                BackColor = _card
            };
            form.Controls.Add(chkDurum);
            var txtNot = MetinKutusuEkle(form, "Not", 24, 240, 364);
            var tablo = TabloOlustur<Operasyon>(gridLeft, gridTop, gridWidth, 430);
            _contentPanel.Controls.Add(tablo);
            var selectedID = string.Empty;
            void RefreshGrid()
            {
                tablo.DataSource = new BindingList<Operasyon>(_operasyonManager.OperasyonlariGetir());
                SutunlariGizle(tablo, "VarlikID");
                SutunBasliklariniAyarla(tablo,
                    ("OperasyonID", "Operasyon No"),
                    ("OperasyonTipi", "Tip"),
                    ("Durum", "Tamamlandi"),
                    ("Tarih", "Tarih"),
                    ("Notlar", "Not"),
                    ("VarlikAdi", "Varlık"),
                    ("DurumMetni", "Durum"));
            }

            tablo.SelectionChanged += (_, _) =>
            {
                if (tablo.CurrentRow?.DataBoundItem is not Operasyon o)
                {
                    return;
                }

                selectedID = o.OperasyonID;
                cmbVarlik.SelectedValue = o.VarlikID;
                cmbTip.SelectedItem = o.OperasyonTipi;
                chkDurum.Checked = o.Durum;
                txtNot.Text = o.Notlar;
            };

            form.Controls.Add(IslemDugmesiOlustur("Güncelle", 24, 320, 112, _green, () => GuvenliCalistir(() =>
            {
                _operasyonManager.OperasyonGuncelle(new Operasyon
                {
                    OperasyonID = selectedID,
                    VarlikID = cmbVarlik.SelectedValue?.ToString() ?? string.Empty,
                    OperasyonTipi = cmbTip.SelectedItem is OperasyonTipi t ? t : OperasyonTipi.Temizlik,
                    Durum = chkDurum.Checked,
                    Tarih = DateTime.Today,
                    Notlar = txtNot.Text
                });
                RefreshGrid();
            }, "Operasyon güncellendi.")));

            form.Controls.Add(IslemDugmesiOlustur("Tamamla", 148, 320, 112, _greenSoft, () => GuvenliCalistir(() =>
            {
                _operasyonManager.OperasyonTamamla(selectedID);
                RefreshGrid();
            }, "Operasyon tamamlandi."), _green));

            form.Controls.Add(IslemDugmesiOlustur("Sil", 272, 320, 116, _danger, () =>
            {
                if (Onayla("Seçili operasyon silinsin mi?"))
                {
                    GuvenliCalistir(() =>
                    {
                        _operasyonManager.OperasyonSil(selectedID);
                        RefreshGrid();
                    }, "Operasyon silindi.");
                }
            }, _ink));

            RefreshGrid();
        }
        catch (Exception ex)
        {
            SayfaHatasiniGoster(ex);
        }
        finally
        {
            SayfayiBitir();
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void FinansiGoster()
    {
        SayfayiHazirla("Finans");
        try
        {
            BaslikEkle("Ödeme ve Çıkış", "Ekstra hizmet, ödeme ve konaklama çıkış işlemlerini yönetin.", null, null);
            var rezervasyonlar = _rezervasyonManager.RezervasyonlariGetir();
            var activeReservations = rezervasyonlar
                .Where(r => r.Durum == RezervasyonDurumu.Aktif)
                .ToList();
            var activeReservationIds = activeReservations
                .Select(r => r.RezervasyonID)
                .ToHashSet();
            var wideLayout = IcerikGenisligi() >= 940;
            var formWidth = Math.Min(420, IcerikGenisligi());
            var serviceTop = 126;
            var rightLeft = wideLayout ? formWidth + 20 : 0;
            var rightWidth = wideLayout ? IcerikGenisligi() - rightLeft : IcerikGenisligi();
            var serviceGridTop = wideLayout ? serviceTop : serviceTop + 340;
            var paymentTop = wideLayout ? serviceTop + 360 : serviceGridTop + 340;
            var paymentGridTop = wideLayout ? paymentTop : paymentTop + 410;
            var exitTop = wideLayout ? paymentTop + 430 : paymentGridTop + 410;
            var exitGridTop = wideLayout ? exitTop : exitTop + 240;

            var serviceForm = KartOlustur(0, serviceTop, formWidth, 320, _violet);
            _contentPanel.Controls.Add(serviceForm);
            serviceForm.Controls.Add(EtiketOlustur("Ekstra Hizmetler", 24, 24, 20, FontStyle.Bold, _ink, 364));
            var cmbRezHizmet = SecimKutusuEkle<Rezervasyon>(serviceForm, "Rezervasyon", 24, 100, 364, activeReservations, "SecimMetni", "RezervasyonID");
            var txtHizmet = MetinKutusuEkle(serviceForm, "Hizmet Adı", 24, 170, 170);
            var numHizmet = SayisalAlanEkle(serviceForm, "Ücret", 218, 170, 170, 0, 100000);
            var serviceGrid = TabloOlustur<Hizmet>(rightLeft, serviceGridTop, rightWidth, 320);
            _contentPanel.Controls.Add(serviceGrid);
            var selectedServiceID = string.Empty;
            void RefreshServices()
            {
                var activeServices = _hizmetManager.HizmetleriGetir()
                    .Where(h => activeReservationIds.Contains(h.RezervasyonID))
                    .ToList();
                serviceGrid.DataSource = new BindingList<Hizmet>(activeServices);
                SutunlariGizle(serviceGrid, "RezervasyonID");
                SutunBasliklariniAyarla(serviceGrid,
                    ("HizmetID", "Hizmet No"),
                    ("Ad", "Hizmet"),
                    ("Ücret", "Ücret"));
            }

            serviceGrid.SelectionChanged += (_, _) =>
            {
                if (serviceGrid.CurrentRow?.DataBoundItem is Hizmet hizmet)
                {
                    selectedServiceID = hizmet.HizmetID;
                    cmbRezHizmet.SelectedValue = hizmet.RezervasyonID;
                    txtHizmet.Text = hizmet.Ad;
                    numHizmet.Value = Math.Clamp(hizmet.Ucret, numHizmet.Minimum, numHizmet.Maximum);
                }
            };

            serviceForm.Controls.Add(IslemDugmesiOlustur("Ekle", 24, 240, 112, _green, () => GuvenliCalistir(() =>
            {
                _hizmetManager.HizmetEkle(new Hizmet
                {
                    RezervasyonID = cmbRezHizmet.SelectedValue?.ToString() ?? string.Empty,
                    Ad = txtHizmet.Text,
                    Ucret = numHizmet.Value
                });
                RefreshServices();
                FinansiGoster();
            }, "Hizmet eklendi ve rezervasyon toplamı yenilendi.")));

            serviceForm.Controls.Add(IslemDugmesiOlustur("Güncelle", 148, 240, 112, Color.White, () => GuvenliCalistir(() =>
            {
                _hizmetManager.HizmetGuncelle(new Hizmet
                {
                    HizmetID = selectedServiceID,
                    RezervasyonID = cmbRezHizmet.SelectedValue?.ToString() ?? string.Empty,
                    Ad = txtHizmet.Text,
                    Ucret = numHizmet.Value
                });
                RefreshServices();
                FinansiGoster();
            }, "Hizmet güncellendi ve rezervasyon toplamı yenilendi."), _ink));

            serviceForm.Controls.Add(IslemDugmesiOlustur("Sil", 272, 240, 116, Color.FromArgb(249, 232, 232), () =>
            {
                if (Onayla("Seçili hizmet silinsin mi?"))
                {
                    GuvenliCalistir(() =>
                    {
                        _hizmetManager.HizmetSil(selectedServiceID);
                        RefreshServices();
                        FinansiGoster();
                    }, "Hizmet silindi ve rezervasyon toplamı yenilendi.");
                }
            }, Color.FromArgb(190, 45, 45)));

            var paymentForm = KartOlustur(0, paymentTop, formWidth, 390, _blue);
            _contentPanel.Controls.Add(paymentForm);
            paymentForm.Controls.Add(EtiketOlustur("Ödeme", 24, 24, 20, FontStyle.Bold, _ink, 364));
            var cmbRez = SecimKutusuEkle<Rezervasyon>(paymentForm, "Rezervasyon", 24, 100, 364, activeReservations, "SecimMetni", "RezervasyonID");
            var numTutar = SayisalAlanEkle(paymentForm, "Tutar", 24, 170, 170, 1, 1000000);
            var cmbTip = SecimKutusuEkle<OdemeTipi>(paymentForm, "Ödeme Tipi", 218, 170, 170, Enum.GetValues<OdemeTipi>().ToList(), null, null);
            var txtAciklama = MetinKutusuEkle(paymentForm, "Açıklama", 24, 240, 364);
            var paymentInfo = EtiketOlustur("Rezervasyon seçilince tutar hesaplanır.", 24, 286, 10.5F, FontStyle.Bold, _green, 364);
            paymentInfo.Height = 44;
            paymentInfo.AutoEllipsis = false;
            paymentForm.Controls.Add(paymentInfo);
            var paymentGrid = TabloOlustur<Odeme>(rightLeft, paymentGridTop, rightWidth, 390);
            _contentPanel.Controls.Add(paymentGrid);
            var selectedPaymentID = string.Empty;
            void RefreshPayments()
            {
                var activePayments = _odemeManager.OdemeleriGetir()
                    .Where(o => activeReservationIds.Contains(o.RezervasyonID))
                    .ToList();
                paymentGrid.DataSource = new BindingList<Odeme>(activePayments);
                SutunlariGizle(paymentGrid, "RezervasyonID");
                SutunBasliklariniAyarla(paymentGrid,
                    ("OdemeID", "Ödeme No"),
                    ("Ücret", "Tutar"),
                    ("OdemeTarihi", "Tarih"),
                    ("OdemeTipi", "Tip"),
                    ("Aciklama", "Açıklama"),
                    ("MusteriAdSoyad", "Müşteri"));
            }
            void FillPaymentAmount()
            {
                if (cmbRez.SelectedItem is not Rezervasyon rezervasyon)
                {
                    paymentInfo.Text = "Rezervasyon seçilince tutar hesaplanır.";
                    return;
                }

                GuvenliCalistir(() =>
                {
                    var kalan = _odemeManager.KalanTutarGetir(rezervasyon.RezervasyonID);
                    numTutar.Value = Math.Clamp(kalan <= 0 ? rezervasyon.ToplamUcret : kalan, numTutar.Minimum, numTutar.Maximum);
                    paymentInfo.Text = $"{rezervasyon.GeceSayisi} gece - Toplam {rezervasyon.ToplamUcret:C0} - Kalan {kalan:C0}";
                });
            }

            cmbRez.SelectedIndexChanged += (_, _) => FillPaymentAmount();
            paymentGrid.SelectionChanged += (_, _) =>
            {
                if (paymentGrid.CurrentRow?.DataBoundItem is Odeme odeme)
                {
                    selectedPaymentID = odeme.OdemeID;
                    cmbRez.SelectedValue = odeme.RezervasyonID;
                    numTutar.Value = Math.Clamp(odeme.Ucret, numTutar.Minimum, numTutar.Maximum);
                    cmbTip.SelectedItem = odeme.OdemeTipi;
                    txtAciklama.Text = odeme.Aciklama;
                }
            };

            paymentForm.Controls.Add(IslemDugmesiOlustur("Kaydet", 24, 330, 112, _green, () => GuvenliCalistir(() =>
            {
                _odemeManager.OdemeEkle(new Odeme
                {
                    RezervasyonID = cmbRez.SelectedValue?.ToString() ?? string.Empty,
                    Ucret = numTutar.Value,
                    OdemeTarihi = DateTime.Today,
                    OdemeTipi = cmbTip.SelectedItem is OdemeTipi t ? t : OdemeTipi.Nakit,
                    Aciklama = txtAciklama.Text
                });
                FinansiGoster();
            }, "Ödeme kaydedildi.")));

            paymentForm.Controls.Add(IslemDugmesiOlustur("Güncelle", 148, 330, 112, Color.White, () => GuvenliCalistir(() =>
            {
                _odemeManager.OdemeGuncelle(new Odeme
                {
                    OdemeID = selectedPaymentID,
                    RezervasyonID = cmbRez.SelectedValue?.ToString() ?? string.Empty,
                    Ucret = numTutar.Value,
                    OdemeTarihi = DateTime.Today,
                    OdemeTipi = cmbTip.SelectedItem is OdemeTipi t ? t : OdemeTipi.Nakit,
                    Aciklama = txtAciklama.Text
                });
                FinansiGoster();
            }, "Ödeme güncellendi."), _ink));

            paymentForm.Controls.Add(IslemDugmesiOlustur("Sil", 272, 330, 116, Color.FromArgb(249, 232, 232), () =>
            {
                if (Onayla("Seçili ödeme silinsin mi?"))
                {
                    GuvenliCalistir(() =>
                    {
                        _odemeManager.OdemeSil(selectedPaymentID);
                        FinansiGoster();
                    }, "Ödeme silindi.");
                }
            }, Color.FromArgb(190, 45, 45)));

            var paidActiveReservations = activeReservations
                .Where(r => _odemeManager.KalanTutarGetir(r.RezervasyonID) <= 0)
                .ToList();
            var exitForm = KartOlustur(0, exitTop, formWidth, 220, _orange);
            _contentPanel.Controls.Add(exitForm);
            exitForm.Controls.Add(EtiketOlustur("Çıkış", 24, 24, 20, FontStyle.Bold, _ink, 364));
            var cmbExitRez = SecimKutusuEkle<Rezervasyon>(exitForm, "Rezervasyon", 24, 100, 364, paidActiveReservations, "SecimMetni", "RezervasyonID");
            exitForm.Controls.Add(IslemDugmesiOlustur("Çıkış Yap", 24, 160, 170, _danger, () => GuvenliCalistir(() =>
            {
                _rezervasyonManager.CikisIslemiBaslat(cmbExitRez.SelectedValue?.ToString() ?? string.Empty);
                FinansiGoster();
            }, "Çıkış işlemi tamamlandı, temizlik operasyonu başladı."), _ink));

            var paidReservationsGrid = TabloOlustur<Rezervasyon>(rightLeft, exitGridTop, rightWidth, 220);
            _contentPanel.Controls.Add(paidReservationsGrid);
            paidReservationsGrid.DataSource = new BindingList<Rezervasyon>(paidActiveReservations);
            SutunlariGizle(paidReservationsGrid, "MusteriID", "VarlikID", "KayitTarihi");
            SutunBasliklariniAyarla(paidReservationsGrid,
                ("RezervasyonID", "Rezervasyon"),
                ("MusteriAdSoyad", "Müşteri"),
                ("VarlikAdi", "Varlık"),
                ("BasTarih", "Giriş"),
                ("SonTarih", "Çıkış"),
                ("ToplamUcret", "Toplam"));

            RefreshPayments();
            FillPaymentAmount();
            RefreshServices();
        }
        catch (Exception ex)
        {
            SayfaHatasiniGoster(ex);
        }
        finally
        {
            SayfayiBitir();
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void AyarlariGoster()
    {
        SayfayiHazirla("Ayarlar");
        try
        {
            BaslikEkle("Ayarlar", "Profil, işletme ve bildirim tercihleri.", null, null);
            var user = GetCurrentUser();
            var settings = _profilManager.IsletmeAyarlariGetir();
            var wideLayout = IcerikGenisligi() >= 1120;
            var leftWidth = Math.Min(520, IcerikGenisligi());
            var listLeft = wideLayout ? leftWidth + 40 : 0;
            var listTop = wideLayout ? 126 : 780;
            var listWidth = wideLayout ? Math.Max(420, IcerikGenisligi() - listLeft) : IcerikGenisligi();

            var profile = KartOlustur(0, 126, leftWidth, 230, _green);
            _contentPanel.Controls.Add(profile);
            var profileName = EtiketOlustur(user.AdSoyad, 32, 28, 22, FontStyle.Bold, _ink, 440);
            var profileRole = EtiketOlustur(user.Rol, 34, 68, 12, FontStyle.Regular, _muted, 240);
            profile.Controls.Add(profileName);
            profile.Controls.Add(profileRole);
            var txtAdSoyad = MetinKutusuEkle(profile, "Ad Soyad", 32, 124, 205);
            var txtEmail = MetinKutusuEkle(profile, "E-posta", 260, 124, 220);
            txtAdSoyad.Text = user.AdSoyad;
            txtEmail.Text = user.Email;
            profile.Controls.Add(IslemDugmesiOlustur("Profili Güncelle", 32, 182, 205, _green, () => GuvenliCalistir(() =>
            {
                user.KullaniciID = CurrentUserId;
                user.AdSoyad = txtAdSoyad.Text;
                user.Email = txtEmail.Text;
                _profilManager.ProfilGuncelle(user);
                profileName.Text = user.AdSoyad;
                profileRole.Text = user.Rol;
                BaslikKullanicisiniYenile();
            }, "Profil güncellendi.")));

            var business = KartOlustur(0, 390, leftWidth, 360, _blue);
            _contentPanel.Controls.Add(business);
            business.Controls.Add(EtiketOlustur("İŞLETME AYARLARI", 32, 24, 13, FontStyle.Bold, _muted));
            var txtIsletme = MetinKutusuEkle(business, "İşletme Bilgileri", 32, 92, 448);
            var txtPara = MetinKutusuEkle(business, "Para Birimi", 32, 162, 205);
            var txtDil = MetinKutusuEkle(business, "Dil", 260, 162, 220);
            var chkRez = new CheckBox { Text = "Yeni rezervasyon bildirimleri", Left = 32, Top = 232, Width = 260, Checked = settings.RezervasyonBildirimleri, BackColor = _card, ForeColor = _ink };
            var chkTem = new CheckBox { Text = "Temizlik uyarıları", Left = 32, Top = 264, Width = 260, Checked = settings.TemizlikUyarilari, BackColor = _card, ForeColor = _ink };
            business.Controls.Add(chkRez);
            business.Controls.Add(chkTem);
            txtIsletme.Text = settings.IsletmeAdi;
            txtPara.Text = settings.ParaBirimi;
            txtDil.Text = settings.Dil;

            business.Controls.Add(IslemDugmesiOlustur("Ayarları Kaydet", 32, 306, 448, _green, () => GuvenliCalistir(() =>
            {
                settings.IsletmeAdi = txtIsletme.Text;
                settings.ParaBirimi = txtPara.Text;
                settings.Dil = txtDil.Text;
                settings.RezervasyonBildirimleri = chkRez.Checked;
                settings.TemizlikUyarilari = chkTem.Checked;
                _profilManager.IsletmeAyarlariGuncelle(settings);
            }, "Ayarlar kaydedildi.")));

            var liste = KartOlustur(listLeft, listTop, listWidth, 624, _coral);
            _contentPanel.Controls.Add(liste);
            liste.Controls.Add(EtiketOlustur("SİSTEM VE GÜVENLİK", 32, 24, 13, FontStyle.Bold, _muted));
            var passwordToggle = RozetOlustur("Şifre Değiştir ▼", 32, 72, Math.Min(448, liste.Width - 64), Color.White, _ink);
            passwordToggle.Height = 34;
            passwordToggle.TextAlign = ContentAlignment.MiddleLeft;
            passwordToggle.Padding = new Padding(12, 0, 0, 0);
            passwordToggle.BorderStyle = BorderStyle.FixedSingle;
            passwordToggle.Cursor = Cursors.Hand;
            liste.Controls.Add(passwordToggle);

            var passwordPanel = new Panel
            {
                Left = 32,
                Top = 122,
                Width = Math.Min(448, liste.Width - 64),
                Height = 154,
                BackColor = _card,
                Visible = false
            };
            liste.Controls.Add(passwordPanel);

            var passwordHint = EtiketOlustur("Mevcut şifrenizi ve yeni şifrenizi girin.", 0, 0, 9.5F, FontStyle.Regular, _muted, passwordPanel.Width);
            passwordPanel.Controls.Add(passwordHint);
            var txtMevcutSifre = MetinKutusuEkle(passwordPanel, "Mevcut Şifre", 0, 38, 200);
            var txtYeniSifre = MetinKutusuEkle(passwordPanel, "Yeni Şifre", 224, 38, 200);
            var txtYeniSifreTekrar = MetinKutusuEkle(passwordPanel, "Yeni Şifre Tekrar", 0, 108, 200);
            txtMevcutSifre.UseSystemPasswordChar = true;
            txtYeniSifre.UseSystemPasswordChar = true;
            txtYeniSifreTekrar.UseSystemPasswordChar = true;
            passwordPanel.Controls.Add(IslemDugmesiOlustur("Şifreyi Kaydet", 224, 108, 200, _green, () => GuvenliCalistir(() =>
            {
                _profilManager.SifreDegistir(CurrentUserId, txtMevcutSifre.Text, txtYeniSifre.Text, txtYeniSifreTekrar.Text);
                txtMevcutSifre.Clear();
                txtYeniSifre.Clear();
                txtYeniSifreTekrar.Clear();
            }, "Şifre güncellendi.")));
            void TogglePasswordPanel()
            {
                passwordPanel.Visible = !passwordPanel.Visible;
                passwordToggle.Text = passwordPanel.Visible ? "Şifre Değiştir ▲" : "Şifre Değiştir ▼";
            }

            passwordToggle.Click += (_, _) => TogglePasswordPanel();

            AyarSatiriEkle(liste, "Raporları Dışarı Aktar", "Finansal raporu Excel dosyasına aktarır.", 304, () => GuvenliCalistir(FinansalRaporuDisaAktar));
            AyarSatiriEkle(liste, "Kullanım Koşulları", "Lisans, ticari kullanım ve yasal uyarılar.", 384, KullanimKosullariPenceresiniGoster);
            liste.Controls.Add(IslemDugmesiOlustur("Çıkış Yap", 32, 540, Math.Min(440, liste.Width - 64), _danger, () => Close()));
        }
        catch (Exception ex)
        {
            SayfaHatasiniGoster(ex);
        }
        finally
        {
            SayfayiBitir();
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void BaslikEkle(string baslikMetni, string subtitle, string? buttonText, Action? buttonAction)
    {
        var hasButton = !string.IsNullOrWhiteSpace(buttonText) && buttonAction is not null;
        var reservedButtonWidth = hasButton && IcerikGenisligi() >= 720 ? 260 : 0;
        var titleLabel = EtiketOlustur(baslikMetni, 0, 0, 28, FontStyle.Bold, _ink, IcerikGenisligi() - reservedButtonWidth);
        _contentPanel.Controls.Add(titleLabel);
        var subtitleLabel = EtiketOlustur(subtitle, 2, 62, 13, FontStyle.Regular, _muted, IcerikGenisligi() - reservedButtonWidth);
        _contentPanel.Controls.Add(subtitleLabel);

        if (hasButton)
        {
            var buttonTop = IcerikGenisligi() >= 720 ? 18 : 92;
            var buttonLeft = IcerikGenisligi() >= 720 ? IcerikGenisligi() - 230 : 0;
            _contentPanel.Controls.Add(IslemDugmesiOlustur(buttonText!, buttonLeft, buttonTop, 220, _navy, buttonAction!));
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private Musteri RezervasyonMusterisiniBulVeyaOlustur(string ad, string soyad, string kimlikNo, string telefon, string il, string ilce, string acikAdres, string postaKodu)
    {
        return _musteriManager.MusteriBulVeyaEkle(new Musteri
        {
            Ad = ad.Trim(),
            Soyad = soyad.Trim(),
            KimlikNo = kimlikNo.Trim(),
            Telefon = SadeceRakamlar(telefon),
            Il = il.Trim(),
            Ilce = ilce.Trim(),
            AcikAdres = acikAdres.Trim(),
            PostaKodu = SadeceRakamlar(postaKodu)
        });
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static List<string> TurkSehirleri()
    {
        return ["İstanbul", "Ankara", "İzmir", "Bursa", "Antalya", "Muğla", "Sakarya", "Bolu"];
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static List<string> IlceleriGetir(string city)
    {
        return city switch
        {
            "İstanbul" => ["Kadıköy", "Beşiktaş", "Şişli", "Üsküdar", "Bakırköy"],
            "Ankara" => ["Çankaya", "Keçiören", "Yenimahalle", "Mamak", "Etimesgut"],
            "İzmir" => ["Konak", "Karşıyaka", "Bornova", "Buca", "Çeşme"],
            "Bursa" => ["Osmangazi", "Nilüfer", "Yıldırım", "Mudanya", "İnegöl"],
            "Antalya" => ["Muratpaşa", "Kepez", "Konyaaltı", "Alanya", "Kaş"],
            "Muğla" => ["Bodrum", "Marmaris", "Fethiye", "Menteşe", "Datça"],
            "Sakarya" => ["Adapazarı", "Serdivan", "Sapanca", "Akyazı", "Hendek"],
            "Bolu" => ["Merkez", "Abant", "Mengen", "Mudurnu", "Göynük"],
            _ => []
        };
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static void IlceleriYenidenBagla(ComboBox secimKutusu, string city)
    {
        secimKutusu.DataSource = IlceleriGetir(city);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static void SecimKutusuDegeriniSec(ComboBox secimKutusu, string deger)
    {
        if (!string.IsNullOrWhiteSpace(deger) && secimKutusu.Items.Contains(deger))
        {
            secimKutusu.SelectedItem = deger;
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void RezervasyonHaftaTakvimiEkle(int left, int top, int width, Action<DateTime> onDateSelected)
    {
        var minWeek = HaftaBaslangici(DateTime.Today.AddMonths(-3));
        var maxWeek = HaftaBaslangici(DateTime.Today.AddMonths(3));
        _reservationWeekStart = _reservationWeekStart < minWeek ? minWeek : _reservationWeekStart;
        _reservationWeekStart = _reservationWeekStart > maxWeek ? maxWeek : _reservationWeekStart;

        var strip = KartOlustur(left, top, width, 180, _blue);
        _contentPanel.Controls.Add(strip);
        strip.Controls.Add(EtiketOlustur($"{_reservationWeekStart:dd MMMM} - {_reservationWeekStart.AddDays(6):dd MMMM yyyy}", 76, 24, 15, FontStyle.Bold, _ink, width - 152));

        var previous = IslemDugmesiOlustur("<", 24, 22, 40, Color.White, () =>
        {
            if (_reservationWeekStart.AddDays(-7) >= minWeek)
            {
                _reservationWeekStart = _reservationWeekStart.AddDays(-7);
                _selectedCalendarDate = _reservationWeekStart;
                RezervasyonlariGoster();
            }
        }, _ink);
        var next = IslemDugmesiOlustur(">", width - 64, 22, 40, Color.White, () =>
        {
            if (_reservationWeekStart.AddDays(7) <= maxWeek)
            {
                _reservationWeekStart = _reservationWeekStart.AddDays(7);
                _selectedCalendarDate = _reservationWeekStart;
                RezervasyonlariGoster();
            }
        }, _ink);
        strip.Controls.Add(previous);
        strip.Controls.Add(next);

        var rezervasyonlar = _rezervasyonManager.RezervasyonlariGetir()
            .Where(r => r.Durum == RezervasyonDurumu.Aktif)
            .ToList();
        var dayWidth = Math.Max(72, (width - 48 - 6 * 10) / 7);
        for (var i = 0; i < 7; i++)
        {
            var tarih = _reservationWeekStart.AddDays(i);
            var sayi = rezervasyonlar.Count(r => RezervasyonTarihiKapsar(r, tarih));
            var selected = tarih.Date == _selectedCalendarDate.Date;
            var back = selected ? _navy : Color.FromArgb(241, 244, 248);
            var fore = selected ? Color.White : _ink;
            var dayButton = new Button
            {
                Left = 24 + i * (dayWidth + 10),
                Top = 84,
                Width = dayWidth,
                Height = 72,
                BackColor = back,
                ForeColor = fore,
                FlatStyle = FlatStyle.Flat,
                Text = $"{tarih:ddd}\r\n{tarih:dd}\r\n{sayi} rez.",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Tag = tarih,
                TabStop = false
            };
            dayButton.FlatAppearance.BorderSize = 0;
            dayButton.Click += (_, _) => onDateSelected(tarih);
            strip.Controls.Add(dayButton);
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void VarlikTakvimiEkle(int left, int top, int width, string varlikID, Action<DateTime> onDateSelected)
    {
        EtiketliKontrolleriKaldir("AssetCalendar");

        var calendar = KartOlustur(left, top, width, 460, _coral);
        calendar.Tag = "AssetCalendar";
        _contentPanel.Controls.Add(calendar);
        calendar.Controls.Add(EtiketOlustur("VARLIK TAKVIMI", 24, 24, 13, FontStyle.Bold, _muted, width - 48));
        calendar.Controls.Add(EtiketOlustur(_newReservationCalendarMonth.ToString("MMMM yyyy"), 76, 62, 18, FontStyle.Bold, _ink, width - 152));

        var minMonth = new DateTime(DateTime.Today.AddMonths(-3).Year, DateTime.Today.AddMonths(-3).Month, 1);
        var maxMonth = new DateTime(DateTime.Today.AddMonths(3).Year, DateTime.Today.AddMonths(3).Month, 1);
        calendar.Controls.Add(IslemDugmesiOlustur("<", 24, 62, 40, Color.White, () =>
        {
            if (_newReservationCalendarMonth.AddMonths(-1) >= minMonth)
            {
                _newReservationCalendarMonth = _newReservationCalendarMonth.AddMonths(-1);
                VarlikTakvimiEkle(left, top, width, varlikID, onDateSelected);
            }
        }, _ink));
        calendar.Controls.Add(IslemDugmesiOlustur(">", width - 64, 62, 40, Color.White, () =>
        {
            if (_newReservationCalendarMonth.AddMonths(1) <= maxMonth)
            {
                _newReservationCalendarMonth = _newReservationCalendarMonth.AddMonths(1);
                VarlikTakvimiEkle(left, top, width, varlikID, onDateSelected);
            }
        }, _ink));

        calendar.Controls.Add(RozetOlustur("Dolu", 24, 410, 72, _calendarOccupied, Color.White));
        calendar.Controls.Add(RozetOlustur("Operasyon", 108, 410, 104, _calendarOperation, Color.White));
        calendar.Controls.Add(RozetOlustur("Müsait", 224, 410, 82, _calendarAvailable, Color.White));

        var weekLabels = new[] { "Pzt", "Sal", "Car", "Per", "Cum", "Cmt", "Paz" };
        var cellWidth = Math.Max(50, (width - 48 - 6 * 8) / 7);
        for (var i = 0; i < weekLabels.Length; i++)
        {
            calendar.Controls.Add(EtiketOlustur(weekLabels[i], 24 + i * (cellWidth + 8), 116, 9.5F, FontStyle.Bold, _muted, cellWidth));
        }

        var firstGridDate = HaftaBaslangici(_newReservationCalendarMonth);
        var rezervasyonlar = _rezervasyonManager.RezervasyonlariGetir()
            .Where(r => r.Durum == RezervasyonDurumu.Aktif && r.VarlikID == varlikID)
            .ToList();
        var operations = _operasyonManager.OperasyonlariGetir()
            .Where(o => !o.Durum && o.VarlikID == varlikID)
            .ToList();

        for (var i = 0; i < 42; i++)
        {
            var tarih = firstGridDate.AddDays(i);
            var satir = i / 7;
            var col = i % 7;
            var isCurrentMonth = tarih.Month == _newReservationCalendarMonth.Month;
            var occupied = rezervasyonlar.Any(r => RezervasyonTarihiKapsar(r, tarih));
            var inOperation = operations.Any(o => o.Tarih.Date == tarih.Date);
            var back = occupied
                ? _calendarOccupied
                : inOperation
                    ? _calendarOperation
                    : _calendarAvailable;
            var fore = Color.White;
            var day = new Button
            {
                Left = 24 + col * (cellWidth + 8),
                Top = 150 + satir * 40,
                Width = cellWidth,
                Height = 34,
                BackColor = isCurrentMonth ? back : Color.FromArgb(245, 247, 250),
                ForeColor = isCurrentMonth ? fore : _muted,
                FlatStyle = FlatStyle.Flat,
                Text = tarih.Day.ToString(),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            day.FlatAppearance.BorderSize = 0;
            day.Click += (_, _) => onDateSelected(tarih);
            calendar.Controls.Add(day);
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void EtiketliKontrolleriKaldir(string tag)
    {
        foreach (var control in _contentPanel.Controls.Cast<Control>().Where(c => Equals(c.Tag, tag)).ToList())
        {
            _contentPanel.Controls.Remove(control);
            control.Dispose();
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void TakvimSeridiEkle(int left, int top, Action<DateTime>? onDateSelected = null)
    {
        var strip = KartOlustur(left, top, IcerikGenisligi(), 130);
        _contentPanel.Controls.Add(strip);
        strip.Controls.Add(EtiketOlustur(DateTime.Today.ToString("MMMM yyyy"), 28, 22, 15, FontStyle.Bold, _ink));

        var days = new FlowLayoutPanel
        {
            Left = 24,
            Top = 62,
            Width = strip.Width - 48,
            Height = 62,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = _card
        };
        strip.Controls.Add(days);

        for (var i = 0; i < 9; i++)
        {
            var tarih = DateTime.Today.AddDays(i);
            var active = tarih.Date == _selectedCalendarDate.Date;
            var dayButton = new Button
            {
                Width = 78,
                Height = 60,
                Margin = new Padding(0, 0, 14, 0),
                BackColor = active ? Color.Black : Color.FromArgb(241, 244, 248),
                ForeColor = active ? Color.White : _ink,
                FlatStyle = FlatStyle.Flat,
                Text = $"{tarih:ddd}\r\n{tarih:dd}",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Tag = tarih,
                TabStop = false,
                TextAlign = ContentAlignment.MiddleCenter
            };
            dayButton.FlatAppearance.BorderSize = 0;
            dayButton.Click += (_, _) =>
            {
                _selectedCalendarDate = tarih.Date;
                foreach (Button dugme in days.Controls.OfType<Button>())
                {
                    var isSelected = dugme.Tag is DateTime buttonDate && buttonDate.Date == _selectedCalendarDate.Date;
                    dugme.BackColor = isSelected ? Color.Black : Color.FromArgb(241, 244, 248);
                    dugme.ForeColor = isSelected ? Color.White : _ink;
                }

                onDateSelected?.Invoke(_selectedCalendarDate);
            };
            days.Controls.Add(dayButton);
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void LejantEkle(int left, int top)
    {
        NoktaliLejantEkle(left, top, _green, "MUSAIT");
        NoktaliLejantEkle(left + 120, top, Color.FromArgb(78, 101, 132), "DOLU");
        NoktaliLejantEkle(left + 220, top, _yellow, "TEMİZLİKTE");
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void NoktaliLejantEkle(int left, int top, Color renk, string text)
    {
        var dot = new Panel { Left = left + 6, Top = top + 5, Width = 14, Height = 14, BackColor = renk };
        var etiket = EtiketOlustur(text, left + 28, top, 10, FontStyle.Bold, _ink);
        _contentPanel.Controls.Add(dot);
        _contentPanel.Controls.Add(etiket);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private Panel IstatistikKartiOlustur(string baslikMetni, string deger, string detay, Color accent)
    {
        var pano = KartOlustur(0, 0, 245, 120, accent);
        pano.FillColor = RenkleriKaristir(Color.White, accent, 0.08);
        pano.BorderColor = RenkleriKaristir(Color.White, accent, 0.34);
        pano.Margin = new Padding(0, 0, 14, 0);
        pano.Controls.Add(new Panel { Left = 24, Top = 22, Width = 42, Height = 28, BackColor = RenkleriKaristir(Color.White, accent, 0.18) });
        pano.Controls.Add(EtiketOlustur(baslikMetni, 92, 24, 9, FontStyle.Bold, accent, 150));
        pano.Controls.Add(EtiketOlustur(deger, 24, 58, 26, FontStyle.Bold, _ink));
        pano.Controls.Add(EtiketOlustur(detay, 94, 72, 10, FontStyle.Regular, _muted));
        return pano;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void GelirCubuklariniCiz(Graphics graphics, Rectangle bounds)
    {
        using var brush = new SolidBrush(Color.FromArgb(236, 239, 244));
        using var activeBrush = new SolidBrush(_green);
        var values = new[] { 42, 60, 46, 76, 56, 100 };
        var labels = new[] { "OCA", "SUB", "MAR", "NIS", "MAY", "HAZ" };
        var baseY = bounds.Height - 30;
        for (var i = 0; i < values.Length; i++)
        {
            var x = 30 + i * 105;
            var h = values[i];
            graphics.FillRectangle(i == values.Length - 1 ? activeBrush : brush, x, baseY - h, 72, h);
            TextRenderer.DrawText(graphics, labels[i], Font, new Point(x, baseY + 7), _muted);
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private Button IslemDugmesiOlustur(string text, int left, int top, int width, Color backColor, Action islem, Color? foreColor = null)
    {
        if (backColor == Color.White)
        {
            backColor = _greenSoft;
        }

        if (backColor == _navy)
        {
            backColor = _green;
        }

        if (backColor == Color.FromArgb(249, 232, 232) || foreColor == Color.FromArgb(190, 45, 45))
        {
            backColor = _danger;
        }

        var dugme = new Button
        {
            Text = text,
            Left = left,
            Top = top,
            Width = width,
            Height = 42,
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        dugme.FlatAppearance.BorderSize = 0;
        dugme.FlatAppearance.BorderColor = _line;
        dugme.Click += (_, _) => islem();
        return dugme;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private Label RozetOlustur(string text, int left, int top, int width, Color back, Color fore)
    {
        var etiket = new Label
        {
            Text = text,
            Left = left,
            Top = top,
            Width = width,
            Height = 26,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = back,
            ForeColor = fore,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        return etiket;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private Label EtiketOlustur(string text, int left, int top, float size, FontStyle style, Color renk, int width = 260)
    {
        return new Label
        {
            Text = text,
            Left = left,
            Top = top,
            Width = width,
            Height = Math.Max(26, (int)Math.Ceiling(size * 2.05)),
            AutoEllipsis = true,
            Font = new Font("Segoe UI", size, style),
            ForeColor = renk,
            BackColor = Color.Transparent
        };
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private CardPanel KartOlustur(int left, int top, int width, int height, Color? accent = null)
    {
        return new CardPanel
        {
            Left = left,
            Top = top,
            Width = Math.Max(260, width),
            Height = height,
            BackColor = _card,
            FillColor = _card,
            BorderColor = _line,
            AccentColor = accent ?? Color.Transparent,
            CornerRadius = 8
        };
    }

    private DataGridView TabloOlustur<T>(int left, int top, int width, int height)
    {
        var tablo = new DataGridView
        {
            Left = left,
            Top = top,
            Width = Math.Max(360, width),
            Height = height,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoGenerateColumns = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            EnableHeadersVisualStyles = false
        };
        tablo.ColumnHeadersDefaultCellStyle.BackColor = _navy;
        tablo.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        tablo.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        tablo.ColumnHeadersHeight = 34;
        tablo.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
        tablo.DefaultCellStyle.BackColor = Color.White;
        tablo.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 251, 249);
        tablo.DefaultCellStyle.SelectionBackColor = _greenSoft;
        tablo.DefaultCellStyle.SelectionForeColor = _ink;
        tablo.GridColor = _line;
        tablo.RowTemplate.Height = 30;
        tablo.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        return tablo;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static Color RenkleriKaristir(Color baseColor, Color overlay, double overlayAmount)
    {
        overlayAmount = Math.Clamp(overlayAmount, 0, 1);
        var baseAmount = 1 - overlayAmount;
        return Color.FromArgb(
            (int)((baseColor.R * baseAmount) + (overlay.R * overlayAmount)),
            (int)((baseColor.G * baseAmount) + (overlay.G * overlayAmount)),
            (int)((baseColor.B * baseAmount) + (overlay.B * overlayAmount)));
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private TextBox MetinKutusuEkle(Control parent, string etiket, int left, int top, int width)
    {
        parent.Controls.Add(EtiketOlustur(etiket, left, top - 30, 9.5F, FontStyle.Bold, _ink, width));
        var textBox = new TextBox
        {
            Left = left,
            Top = top,
            Width = width,
            Height = 34,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10F)
        };
        parent.Controls.Add(textBox);
        return textBox;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private NumericUpDown SayisalAlanEkle(Control parent, string etiket, int left, int top, int width, decimal min, decimal max)
    {
        parent.Controls.Add(EtiketOlustur(etiket, left, top - 30, 9.5F, FontStyle.Bold, _ink, width));
        var sayiAlani = new NumericUpDown
        {
            Left = left,
            Top = top,
            Width = width,
            Height = 34,
            Minimum = min,
            Maximum = max,
            Value = min,
            DecimalPlaces = max > 100 ? 2 : 0,
            ThousandsSeparator = true,
            Font = new Font("Segoe UI", 10F)
        };
        parent.Controls.Add(sayiAlani);
        return sayiAlani;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private DateTimePicker TarihAlaniEkle(Control parent, string etiket, int left, int top, int width, DateTime deger)
    {
        parent.Controls.Add(EtiketOlustur(etiket, left, top - 30, 9.5F, FontStyle.Bold, _ink, width));
        var tarihSecici = new DateTimePicker
        {
            Left = left,
            Top = top,
            Width = width,
            Height = 34,
            Format = DateTimePickerFormat.Short,
            Value = deger,
            Font = new Font("Segoe UI", 10F)
        };
        parent.Controls.Add(tarihSecici);
        return tarihSecici;
    }

    private ComboBox SecimKutusuEkle<T>(Control parent, string etiket, int left, int top, int width, List<T> data, string? displayMember, string? valueMember)
    {
        parent.Controls.Add(EtiketOlustur(etiket, left, top - 30, 9.5F, FontStyle.Bold, _ink, width));
        var secimKutusu = new ComboBox
        {
            Left = left,
            Top = top,
            Width = width,
            Height = 34,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10F),
            DataSource = data
        };
        if (!string.IsNullOrWhiteSpace(displayMember))
        {
            secimKutusu.DisplayMember = displayMember;
        }

        if (!string.IsNullOrWhiteSpace(valueMember))
        {
            secimKutusu.ValueMember = valueMember;
        }

        parent.Controls.Add(secimKutusu);
        return secimKutusu;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void FinansalRaporuDisaAktar()
    {
        using var pencere = new SaveFileDialog
        {
            Title = "Finansal raporu kaydet",
            Filter = "Excel dosyası (*.xlsx)|*.xlsx",
            FileName = $"TinyTrack_Finansal_Rapor_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
            AddExtension = true,
            DefaultExt = "xlsx"
        };

        if (pencere.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var musteriler = _musteriManager.MusterileriGetir();
        var rezervasyonlar = _rezervasyonManager.RezervasyonlariGetir();
        var varliklar = _varlikManager.VarliklariGetir();
        var odemeler = _odemeManager.OdemeleriGetir();
        var reservationsByCustomer = rezervasyonlar
            .GroupBy(r => r.MusteriID)
            .ToDictionary(g => g.Key, g => g.OrderBy(r => r.BasTarih).ToList());
        var assetById = varliklar.ToDictionary(v => v.VarlikID);
        var paidByReservation = odemeler
            .GroupBy(o => o.RezervasyonID)
            .ToDictionary(g => g.Key, g => g.Sum(o => o.Ucret));
        var totalRevenue = odemeler.Sum(o => o.Ucret);

        using var calismaKitabi = new XLWorkbook();
        var detay = calismaKitabi.Worksheets.Add("Musteri Raporu");
        ExcelBasligiYaz(detay, "TinyTrack Finansal Müşteri Raporu", 8);
        detay.Cell(3, 1).Value = "Müşteri Adı";
        detay.Cell(3, 2).Value = "Müşteri Soyadı";
        detay.Cell(3, 3).Value = "T.C. Kimlik No";
        detay.Cell(3, 4).Value = "Rezervasyon Gün Sayısı";
        detay.Cell(3, 5).Value = "Varlık Birim Fiyatı";
        detay.Cell(3, 6).Value = "Kalınan Varlık";
        detay.Cell(3, 7).Value = "Varlık Tipi";
        detay.Cell(3, 8).Value = "Toplam Ödeme";

        var satir = 4;
        foreach (var musteri in musteriler.OrderBy(c => c.Ad).ThenBy(c => c.Soyad))
        {
            if (!reservationsByCustomer.TryGetValue(musteri.MusteriID, out var customerReservations) || customerReservations.Count == 0)
            {
                detay.Cell(satir, 1).Value = musteri.Ad;
                detay.Cell(satir, 2).Value = musteri.Soyad;
                detay.Cell(satir, 3).Value = musteri.KimlikNo;
                detay.Cell(satir, 8).Value = 0;
                satir++;
                continue;
            }

            foreach (var rezervasyon in customerReservations)
            {
                assetById.TryGetValue(rezervasyon.VarlikID, out var varlik);
                detay.Cell(satir, 1).Value = musteri.Ad;
                detay.Cell(satir, 2).Value = musteri.Soyad;
                detay.Cell(satir, 3).Value = musteri.KimlikNo;
                detay.Cell(satir, 4).Value = rezervasyon.GeceSayisi;
                detay.Cell(satir, 5).Value = varlik?.GunlukUcret ?? 0;
                detay.Cell(satir, 6).Value = varlik?.Ad ?? rezervasyon.VarlikAdi;
                detay.Cell(satir, 7).Value = varlik?.VarlikTipi ?? string.Empty;
                detay.Cell(satir, 8).Value = paidByReservation.GetValueOrDefault(rezervasyon.RezervasyonID);
                satir++;
            }
        }

        ExcelTablosunuBicimlendir(detay, 3, 1, Math.Max(3, satir - 1), 8);
        detay.Column(5).Style.NumberFormat.Format = "#,##0.00 ₺";
        detay.Column(8).Style.NumberFormat.Format = "#,##0.00 ₺";

        var summary = calismaKitabi.Worksheets.Add("Finans Ozeti");
        ExcelBasligiYaz(summary, "Finansal Özet", 2);
        summary.Cell(3, 1).Value = "Gösterge";
        summary.Cell(3, 2).Value = "Değer";
        summary.Cell(4, 1).Value = "Toplam Kazanç";
        summary.Cell(4, 2).Value = totalRevenue;
        summary.Cell(5, 1).Value = "Toplam Müşteri";
        summary.Cell(5, 2).Value = musteriler.Count;
        summary.Cell(6, 1).Value = "Toplam Rezervasyon";
        summary.Cell(6, 2).Value = rezervasyonlar.Count;
        summary.Cell(7, 1).Value = "Toplam Ödeme Kaydı";
        summary.Cell(7, 2).Value = odemeler.Count;
        ExcelTablosunuBicimlendir(summary, 3, 1, 7, 2);
        summary.Cell(4, 2).Style.NumberFormat.Format = "#,##0.00 ₺";

        var monthly = calismaKitabi.Worksheets.Add("Aylik Kazanc");
        ExcelBasligiYaz(monthly, "Aylık Kazanç", 2);
        monthly.Cell(3, 1).Value = "Ay";
        monthly.Cell(3, 2).Value = "Kazanç";
        satir = 4;
        foreach (var item in odemeler
            .GroupBy(o => new DateTime(o.OdemeTarihi.Year, o.OdemeTarihi.Month, 1))
            .OrderBy(g => g.Key))
        {
            monthly.Cell(satir, 1).Value = item.Key.ToString("MMMM yyyy", new CultureInfo("tr-TR"));
            monthly.Cell(satir, 2).Value = item.Sum(o => o.Ucret);
            satir++;
        }

        ExcelTablosunuBicimlendir(monthly, 3, 1, Math.Max(3, satir - 1), 2);
        monthly.Column(2).Style.NumberFormat.Format = "#,##0.00 ₺";

        var byAssetType = calismaKitabi.Worksheets.Add("Varlik Tipi Kazanc");
        ExcelBasligiYaz(byAssetType, "Varlık Tipine Göre Kazanç", 2);
        byAssetType.Cell(3, 1).Value = "Varlık Tipi";
        byAssetType.Cell(3, 2).Value = "Kazanç";
        satir = 4;
        var revenueByAssetType = odemeler
            .Join(rezervasyonlar, odeme => odeme.RezervasyonID, rezervasyon => rezervasyon.RezervasyonID, (odeme, rezervasyon) => new { odeme, rezervasyon })
            .Join(varliklar, x => x.rezervasyon.VarlikID, varlik => varlik.VarlikID, (x, varlik) => new { varlik.VarlikTipi, x.odeme.Ucret })
            .GroupBy(x => x.VarlikTipi)
            .OrderByDescending(g => g.Sum(x => x.Ucret));
        foreach (var item in revenueByAssetType)
        {
            byAssetType.Cell(satir, 1).Value = item.Key;
            byAssetType.Cell(satir, 2).Value = item.Sum(x => x.Ucret);
            satir++;
        }

        ExcelTablosunuBicimlendir(byAssetType, 3, 1, Math.Max(3, satir - 1), 2);
        byAssetType.Column(2).Style.NumberFormat.Format = "#,##0.00 ₺";

        foreach (var sayfa in calismaKitabi.Worksheets)
        {
            sayfa.Columns().AdjustToContents(12, 42);
            sayfa.SheetView.FreezeRows(3);
        }

        calismaKitabi.SaveAs(pencere.FileName);
        MessageBox.Show($"Finansal rapor oluşturuldu:\n{pencere.FileName}", "TinyTrack ERP", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void KullanimKosullariPenceresiniGoster()
    {
        const string terms = """
        TinyTrack ERP Kullanım Koşulları

        Bu yazılım ticari kullanım amacıyla geliştirilmiş özel bir işletme yönetim sistemidir. Yazılımın lisanssız, izinsiz, kopyalanmış, çoğaltılmış, dağıtılmış veya yetkisiz şekilde kullanıldığının tespiti halinde ilgili kişi ve kurumlar hakkında yasal işlem başlatılabilir.

        Kullanıcı; yazılımı yalnızca kendisine tanımlanan yetki, lisans ve kullanım kapsamı içinde kullanmayı kabul eder. Yazılım kaynak kodlarının, veritabanı yapısının, ekran tasarımlarının, raporlarının veya iş akışlarının izinsiz olarak kopyalanması, tersine mühendislik yoluyla incelenmesi, üçüncü kişilere devredilmesi ya da farklı bir üründe kullanılması yasaktır.

        Sisteme girilen müşteri, rezervasyon, ödeme ve işletme verilerinin doğruluğu ile kişisel verilerin mevzuata uygun şekilde işlenmesi kullanıcının sorumluluğundadır. TinyTrack ERP, kullanıcı tarafından hatalı girilen verilerden, yetkisiz erişimlerden, cihaz veya dosya kayıplarından, yedekleme yapılmamasından ya da hatalı kullanım nedeniyle oluşabilecek ticari zararlardan sorumlu tutulamaz.

        Yazılımda yer alan finansal raporlar, ödeme kayıtları ve operasyonel çıktılar bilgilendirme ve işletme takibi amacı taşır. Resmi muhasebe, vergi beyanı veya hukuki bildirim süreçlerinde nihai sorumluluk kullanıcıya aittir; gerektiğinde yetkili mali müşavir veya hukuk danışmanından destek alınmalıdır.

        Yazılımın kullanılması, bu koşulların okunduğu, anlaşıldığı ve kabul edildiği anlamına gelir. Koşulları kabul etmeyen kullanıcıların yazılımı kullanmaması gerekir.
        """;

        MessageBox.Show(
            terms,
            "Kullanım Koşulları ve Lisans Uyarısı",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static void ExcelBasligiYaz(IXLWorksheet sayfa, string baslikMetni, int columns)
    {
        var titleRange = sayfa.Range(1, 1, 1, columns);
        titleRange.Merge();
        titleRange.Value = baslikMetni;
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 18;
        titleRange.Style.Font.FontColor = XLColor.White;
        titleRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#6d47d5");
        titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        sayfa.Row(1).Height = 30;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static void ExcelTablosunuBicimlendir(IXLWorksheet sayfa, int firstRow, int firstColumn, int lastRow, int lastColumn)
    {
        var aralik = sayfa.Range(firstRow, firstColumn, lastRow, lastColumn);
        aralik.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        aralik.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        aralik.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        var baslik = sayfa.Range(firstRow, firstColumn, firstRow, lastColumn);
        baslik.Style.Font.Bold = true;
        baslik.Style.Font.FontColor = XLColor.White;
        baslik.Style.Fill.BackgroundColor = XLColor.FromHtml("#111827");
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void AyarSatiriEkle(Control parent, string baslikMetni, string detay, int top, Action? islem = null)
    {
        var satir = KartOlustur(32, top, parent.Width - 64, 62);
        parent.Controls.Add(satir);
        satir.Controls.Add(EtiketOlustur(baslikMetni, 20, 10, 13, FontStyle.Bold, _ink, satir.Width - 80));
        satir.Controls.Add(EtiketOlustur(detay, 20, 34, 9.5F, FontStyle.Regular, _muted, satir.Width - 80));
        satir.Controls.Add(EtiketOlustur(">", satir.Width - 45, 18, 16, FontStyle.Bold, _muted, 24));

        if (islem is null)
        {
            return;
        }

        satir.Cursor = Cursors.Hand;
        satir.Click += (_, _) => islem();
        foreach (Control child in satir.Controls)
        {
            child.Cursor = Cursors.Hand;
            child.Click += (_, _) => islem();
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void SutunlariGizle(DataGridView tablo, params string[] names)
    {
        foreach (var name in names)
        {
            if (tablo.Columns.Contains(name))
            {
                tablo.Columns[name]!.Visible = false;
            }
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void SutunBasliklariniAyarla(DataGridView tablo, params (string Column, string Header)[] headers)
    {
        foreach (var (sutun, baslik) in headers)
        {
            if (tablo.Columns.Contains(sutun))
            {
                var gridColumn = tablo.Columns[sutun]!;
                gridColumn.HeaderText = baslik;
                gridColumn.MinimumWidth = Math.Max(gridColumn.MinimumWidth, Math.Min(140, (baslik.Length * 9) + 28));
            }
        }

        foreach (DataGridViewColumn sutun in tablo.Columns)
        {
            if (!sutun.Visible)
            {
                continue;
            }

            sutun.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private int IcerikGenisligi()
    {
        return Math.Max(620, _contentPanel.ClientSize.Width - _contentPanel.Padding.Horizontal - 18);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static string SadeceRakamlar(string text)
    {
        return new string(text.Where(char.IsDigit).ToArray());
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static DateTime HaftaBaslangici(DateTime tarih)
    {
        var diff = ((int)tarih.DayOfWeek + 6) % 7;
        return tarih.Date.AddDays(-diff);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static bool RezervasyonTarihiKapsar(Rezervasyon rezervasyon, DateTime tarih)
    {
        return rezervasyon.BasTarih.Date <= tarih.Date && tarih.Date < rezervasyon.SonTarih.Date;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private static string KisaAd(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "Yönetici";
        }

        return text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Yönetici";
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private bool Onayla(string mesaj)
    {
        return MessageBox.Show(mesaj, "TinyTrack ERP", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void GuvenliCalistir(Action islem, string? successMessage = null)
    {
        try
        {
            islem();
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                MessageBox.Show(successMessage, "TinyTrack ERP", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (BusinessRuleException ex)
        {
            MessageBox.Show(ex.Message, "Is kurali", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    private void SayfaHatasiniGoster(Exception ex)
    {
        _contentPanel.Controls.Add(EtiketOlustur("Sayfa yuklenemedi", 0, 90, 22, FontStyle.Bold, _ink, 500));
        _contentPanel.Controls.Add(EtiketOlustur(ex.Message, 0, 132, 11, FontStyle.Regular, _muted, 760));
    }

    private void MainForm_Load(object sender, EventArgs e)
    {

    }
}

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class CardPanel : Panel
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 8;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color FillColor { get; set; } = Color.White;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = Color.FromArgb(229, 234, 242);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentColor { get; set; } = Color.Transparent;

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var path = RoundedRect(ClientRectangle with { Width = ClientRectangle.Width - 1, Height = ClientRectangle.Height - 1 }, CornerRadius);
        using var fill = new SolidBrush(FillColor);
        using var pen = new Pen(BorderColor);
        e.Graphics.FillPath(fill, path);
        if (AccentColor != Color.Transparent)
        {
            using var accent = new SolidBrush(AccentColor);
            var state = e.Graphics.Save();
            e.Graphics.SetClip(path);
            e.Graphics.FillRectangle(accent, 0, 0, Width, 5);
            e.Graphics.Restore(state);
        }
        e.Graphics.DrawPath(pen, path);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        using var path = RoundedRect(ClientRectangle, CornerRadius);
        Region = new Region(path);
    }

    private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        var diameter = radius * 2;
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
