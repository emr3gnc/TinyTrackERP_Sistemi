using System.ComponentModel;
using System.Globalization;
using TinyTrack.Business;
using TinyTrack.Entities;

namespace TinyTrack.WinForms;

public class MainForm : Form
{
    private const string CurrentUserId = "USR-001";

    private readonly Color _background = Color.FromArgb(241, 246, 250);
    private readonly Color _card = Color.White;
    private readonly Color _ink = Color.FromArgb(16, 24, 40);
    private readonly Color _muted = Color.FromArgb(102, 112, 133);
    private readonly Color _line = Color.FromArgb(229, 234, 242);
    private readonly Color _green = Color.FromArgb(0, 153, 112);
    private readonly Color _greenSoft = Color.FromArgb(228, 248, 241);
    private readonly Color _navy = Color.FromArgb(17, 24, 39);
    private readonly Color _yellow = Color.FromArgb(249, 181, 38);
    private readonly Color _blue = Color.FromArgb(37, 99, 235);
    private readonly Color _coral = Color.FromArgb(239, 68, 68);
    private readonly Color _violet = Color.FromArgb(124, 58, 237);
    private readonly Color _orange = Color.FromArgb(245, 158, 11);

    private readonly Panel _headerPanel = new();
    private readonly Panel _contentPanel = new();
    private readonly Label _headerUserLabel = new();
    private readonly Dictionary<string, Button> _navButtons = [];
    private DateTime _selectedCalendarDate = DateTime.Today;

    private readonly MusteriManager _musteriManager = new();
    private readonly VarlikManager _varlikManager = new();
    private readonly RezervasyonManager _rezervasyonManager = new();
    private readonly OdemeManager _odemeManager = new();
    private readonly OperasyonManager _operasyonManager = new();
    private readonly HizmetManager _hizmetManager = new();
    private readonly ProfilManager _profilManager = new();
    private readonly YoneticiManager _yoneticiManager = new();

    public MainForm()
    {
        Text = "TinyTrack ERP";
        MinimumSize = new Size(1180, 760);
        Size = new Size(1180, 820);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = _background;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");

        BuildShell();
        ShowDashboard();
    }

    private void BuildShell()
    {
        _headerPanel.Dock = DockStyle.Top;
        _headerPanel.Height = 74;
        _headerPanel.BackColor = Color.FromArgb(252, 254, 255);
        _headerPanel.Padding = new Padding(22, 12, 22, 12);
        _headerPanel.Paint += (_, e) =>
        {
            using var pen = new Pen(_line);
            e.Graphics.DrawLine(pen, 0, _headerPanel.Height - 1, _headerPanel.Width, _headerPanel.Height - 1);
        };

        var logo = new PictureBox
        {
            Left = 22,
            Top = 13,
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

        _headerPanel.Controls.Add(logo);

        var brand = new Label
        {
            Text = "TinyTrack",
            AutoSize = false,
            Width = 155,
            Height = 50,
            Left = 78,
            Top = 12,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = _ink
        };
        _headerPanel.Controls.Add(brand);

        _headerUserLabel.AutoSize = false;
        _headerUserLabel.Width = 148;
        _headerUserLabel.Height = 50;
        _headerUserLabel.Left = 1280;
        _headerUserLabel.Top = 12;
        _headerUserLabel.TextAlign = ContentAlignment.MiddleRight;
        _headerUserLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _headerUserLabel.ForeColor = _muted;
        _headerUserLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _headerUserLabel.AutoEllipsis = true;
        _headerPanel.Controls.Add(_headerUserLabel);
        RefreshHeaderUser();

        var nav = new FlowLayoutPanel
        {
            Left = 214,
            Top = 16,
            Width = 880,
            Height = 46,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 2, 0, 0),
            BackColor = Color.White,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        _headerPanel.Controls.Add(nav);

        void LayoutHeader()
        {
            var width = Math.Max(1180, _headerPanel.ClientSize.Width);
            _headerUserLabel.Left = width - _headerUserLabel.Width - 22;
            nav.Left = brand.Right + 12;
            nav.Width = Math.Max(710, _headerUserLabel.Left - nav.Left - 18);
        }

        _headerPanel.Resize += (_, _) => LayoutHeader();
        LayoutHeader();

        AddNavButton(nav, "Dashboard", "PANEL", 90, ShowDashboard);
        AddNavButton(nav, "Rezervasyonlar", "REZERVASYON", 124, ShowReservations);
        AddNavButton(nav, "Varliklar", "VARLIKLAR", 106, ShowAssets);
        AddNavButton(nav, "Musteriler", "MUSTERILER", 116, ShowCustomers);
        AddNavButton(nav, "Operasyon", "OPERASYON", 114, ShowOperations);
        AddNavButton(nav, "Finans", "FINANS", 80, ShowFinance);
        AddNavButton(nav, "Ayarlar", "AYARLAR", 92, ShowSettings);

        _contentPanel.Dock = DockStyle.Fill;
        _contentPanel.AutoScroll = true;
        _contentPanel.BackColor = _background;
        _contentPanel.Padding = new Padding(36, 28, 36, 36);
        Controls.Add(_contentPanel);
        Controls.Add(_headerPanel);
    }

    private Kullanici GetCurrentUser()
    {
        return _profilManager.KullaniciProfilBilgileriniGetir(CurrentUserId) ?? new Kullanici
        {
            KullaniciID = CurrentUserId,
            AdSoyad = "Yonetici",
            Email = "admin@tinytrack.local",
            Rol = "Yonetici"
        };
    }

    private void RefreshHeaderUser()
    {
        try
        {
            var user = GetCurrentUser();
            _headerUserLabel.Text = string.IsNullOrWhiteSpace(user.AdSoyad) ? "Yonetici" : user.AdSoyad;
        }
        catch
        {
            _headerUserLabel.Text = "Yonetici";
        }
    }

    private void AddNavButton(FlowLayoutPanel nav, string key, string text, int width, Action action)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = 42,
            Margin = new Padding(1, 0, 1, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = _muted,
            Font = new Font("Segoe UI", 8.4F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => action();
        _navButtons[key] = button;
        nav.Controls.Add(button);
    }

    private void SetActiveNav(string key)
    {
        foreach (var item in _navButtons)
        {
            item.Value.BackColor = item.Key == key ? _greenSoft : Color.White;
            item.Value.ForeColor = item.Key == key ? _green : _muted;
        }
    }

    private void PreparePage(string key)
    {
        SetActiveNav(key);
        _contentPanel.SuspendLayout();
        _contentPanel.Controls.Clear();
        _contentPanel.AutoScrollPosition = Point.Empty;
    }

    private void FinishPage()
    {
        _contentPanel.ResumeLayout();
    }

    private void ShowDashboard()
    {
        PreparePage("Dashboard");
        try
        {
            var dashboard = _yoneticiManager.DashboardVeriOzetVerileriniGetir();
            var user = GetCurrentUser();
            AddHeader($"Gunaydin, {ShortName(user.AdSoyad)}", DateTime.Today.ToString("dd MMMM dddd, yyyy"), null, null);

            var stats = new FlowLayoutPanel
            {
                Left = 0,
                Top = 126,
                Width = ContentWidth(),
                Height = 145,
                BackColor = _background,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            _contentPanel.Controls.Add(stats);

            stats.Controls.Add(CreateStatCard("GIRISLER", dashboard.BugunkuGiris.ToString("00"), "Bugun", _green));
            stats.Controls.Add(CreateStatCard("CIKISLAR", dashboard.BugunkuCikis.ToString("00"), "Bugun", _blue));
            stats.Controls.Add(CreateStatCard("TEMIZLIKTE", dashboard.TemizliktekiVarlik.ToString("00"), "Birim", _coral));
            stats.Controls.Add(CreateStatCard("AKTIF REZ.", dashboard.AktifRezervasyon.ToString(), "Kayit", _navy));

            var chart = CreateCard(0, 300, Math.Min(720, ContentWidth()), 215, _blue);
            _contentPanel.Controls.Add(chart);
            chart.Controls.Add(CreateLabel("AYLIK GELIR", 18, 22, 14, FontStyle.Bold, _muted));
            chart.Controls.Add(CreateLabel(dashboard.AylikGelir.ToString("C0"), 18, 48, 25, FontStyle.Bold, _ink));
            chart.Controls.Add(CreateBadge($"%{Math.Max(0, dashboard.DolulukOrani):0.0} doluluk", chart.Width - 145, 24, 120, _greenSoft, _green));
            chart.Paint += (_, e) => DrawRevenueBars(e.Graphics, chart.ClientRectangle);

            var quick = CreateCard(0, 540, Math.Min(720, ContentWidth()), 160, _navy);
            _contentPanel.Controls.Add(quick);
            quick.Controls.Add(CreateLabel("HIZLI ISLEMLER", 22, 18, 14, FontStyle.Bold, _ink));
            quick.Controls.Add(CreateActionButton("Yeni Rezervasyon", 22, 58, 280, _navy, ShowReservations));
            quick.Controls.Add(CreateActionButton("Varlik Durumu", 322, 58, 280, Color.White, ShowAssets, _ink));

            var wideLayout = ContentWidth() >= 1180;
            var nextLeft = wideLayout ? 760 : 0;
            var nextTop = wideLayout ? 300 : 725;
            var nextWidth = wideLayout ? Math.Max(360, ContentWidth() - 760) : Math.Min(720, ContentWidth());
            var next = CreateCard(nextLeft, nextTop, nextWidth, 400, _green);
            _contentPanel.Controls.Add(next);
            next.Controls.Add(CreateLabel("SIRADAKI GIRIS", 22, 22, 14, FontStyle.Bold, _green));
            next.Controls.Add(CreateLabel(dashboard.SiradakiGiris, 22, 58, 18, FontStyle.Bold, _ink, next.Width - 44));
            next.Controls.Add(CreateLabel("Takvim ve operasyon ekranlarindan bugunku sureci kontrol edebilirsiniz.", 22, 100, 10.5F, FontStyle.Regular, _muted, next.Width - 44));
            next.Controls.Add(CreateActionButton("Rezervasyonlari Ac", 22, 155, 230, _green, ShowReservations));
        }
        catch (Exception ex)
        {
            ShowPageError(ex);
        }
        finally
        {
            FinishPage();
        }
    }

    private void ShowReservations()
    {
        PreparePage("Rezervasyonlar");
        try
        {
            AddHeader("Takvim / Rezervasyonlar", "Musaitlik durumunu ve konaklamalari yonetin.", "YENI REZERVASYON", () => MessageBox.Show("Formu doldurup Kaydet'e basin.", "TinyTrack"));
            var form = CreateCard(0, 342, 430, 465, _green);
            _contentPanel.Controls.Add(form);
            form.Controls.Add(CreateLabel("Hizli Rezervasyon Formu", 24, 24, 18, FontStyle.Bold, _ink, 390));

            var cmbMusteri = AddCombo<Musteri>(form, "Musteri", 24, 100, 370, _musteriManager.MusterileriGetir(), "AdSoyad", "MusteriID");
            var cmbVarlik = AddCombo<Varlik>(form, "Varlik", 24, 168, 370, _varlikManager.VarliklariGetir(), "Ad", "VarlikID");
            var dtBas = AddDate(form, "Giris Tarihi", 24, 236, 175, DateTime.Today);
            var dtSon = AddDate(form, "Cikis Tarihi", 219, 236, 175, DateTime.Today.AddDays(1));
            var totalLabel = CreateLabel("Toplam: 0 TL", 24, 302, 13, FontStyle.Bold, _green, 370);
            form.Controls.Add(totalLabel);

            var grid = CreateGrid<Rezervasyon>(450, 342, ContentWidth() - 450, 465);
            _contentPanel.Controls.Add(grid);

            void RefreshGrid()
            {
                grid.DataSource = new BindingList<Rezervasyon>(_rezervasyonManager.RezervasyonlariGetir());
                HideColumns(grid, "MusteriID", "VarlikID", "KayitTarihi");
                SetColumnHeaders(grid,
                    ("RezervasyonID", "Rezervasyon"),
                    ("BasTarih", "Giris"),
                    ("SonTarih", "Cikis"),
                    ("ToplamUcret", "Toplam"),
                    ("MusteriAdSoyad", "Musteri"),
                    ("VarlikAdi", "Varlik"),
                    ("GeceSayisi", "Gece"));
            }

            void RefreshTotal()
            {
                if (cmbVarlik.SelectedValue is string varlikID)
                {
                    Safe(() =>
                    {
                        totalLabel.Text = $"Toplam: {_rezervasyonManager.ToplamUcretHesapla(varlikID, dtBas.Value, dtSon.Value, 0):C0}";
                    });
                }
            }

            dtBas.ValueChanged += (_, _) => RefreshTotal();
            dtSon.ValueChanged += (_, _) => RefreshTotal();
            cmbVarlik.SelectedIndexChanged += (_, _) => RefreshTotal();

            AddCalendarStrip(0, 126, selectedDate =>
            {
                dtBas.Value = selectedDate;
                if (dtSon.Value.Date <= selectedDate.Date)
                {
                    dtSon.Value = selectedDate.AddDays(1);
                }
                RefreshTotal();
            });
            AddLegend(0, 292);

            var selectedID = string.Empty;
            var selectedStatus = RezervasyonDurumu.Aktif;
            grid.SelectionChanged += (_, _) =>
            {
                if (grid.CurrentRow?.DataBoundItem is not Rezervasyon r)
                {
                    return;
                }

                selectedID = r.RezervasyonID;
                cmbMusteri.SelectedValue = r.MusteriID;
                cmbVarlik.SelectedValue = r.VarlikID;
                dtBas.Value = r.BasTarih;
                dtSon.Value = r.SonTarih;
                selectedStatus = r.Durum;
                totalLabel.Text = $"Toplam: {r.ToplamUcret:C0}";
            };

            form.Controls.Add(CreateActionButton("Kaydet", 24, 346, 170, _green, () => Safe(() =>
            {
                _rezervasyonManager.RezervasyonEkle(new Rezervasyon
                {
                    MusteriID = cmbMusteri.SelectedValue?.ToString() ?? string.Empty,
                    VarlikID = cmbVarlik.SelectedValue?.ToString() ?? string.Empty,
                    BasTarih = dtBas.Value,
                    SonTarih = dtSon.Value
                });
                RefreshGrid();
                RefreshTotal();
            }, "Rezervasyon kaydedildi.")));

            form.Controls.Add(CreateActionButton("Guncelle", 214, 346, 170, Color.White, () => Safe(() =>
            {
                _rezervasyonManager.RezervasyonGuncelle(new Rezervasyon
                {
                    RezervasyonID = selectedID,
                    MusteriID = cmbMusteri.SelectedValue?.ToString() ?? string.Empty,
                    VarlikID = cmbVarlik.SelectedValue?.ToString() ?? string.Empty,
                    BasTarih = dtBas.Value,
                    SonTarih = dtSon.Value,
                    Durum = selectedStatus
                });
                RefreshGrid();
            }, "Rezervasyon guncellendi."), _ink));

            form.Controls.Add(CreateActionButton("Iptal Et", 24, 392, 115, Color.FromArgb(249, 232, 232), () => Safe(() =>
            {
                _rezervasyonManager.RezervasyonIptal(selectedID);
                RefreshGrid();
            }, "Rezervasyon iptal edildi."), Color.FromArgb(190, 45, 45)));

            form.Controls.Add(CreateActionButton("Cikis", 155, 392, 115, _yellow, () => Safe(() =>
            {
                _rezervasyonManager.CikisIslemiBaslat(selectedID);
                RefreshGrid();
            }, "Cikis islemi tamamlandi, temizlik operasyonu basladi."), _ink));

            form.Controls.Add(CreateActionButton("Sil", 285, 392, 100, Color.White, () =>
            {
                if (Confirm("Secili rezervasyon silinsin mi?"))
                {
                    Safe(() =>
                    {
                        _rezervasyonManager.RezervasyonSil(selectedID);
                        RefreshGrid();
                    }, "Rezervasyon silindi.");
                }
            }, _ink));

            RefreshGrid();
            RefreshTotal();
        }
        catch (Exception ex)
        {
            ShowPageError(ex);
        }
        finally
        {
            FinishPage();
        }
    }

    private void ShowCustomers()
    {
        PreparePage("Musteriler");
        try
        {
            AddHeader("Musteriler", "Kimlik, iletisim ve konaklama musteri kayitlari.", null, null);

            var form = CreateCard(0, 126, 420, 500, _blue);
            _contentPanel.Controls.Add(form);
            form.Controls.Add(CreateLabel("Musteri Bilgileri", 24, 24, 20, FontStyle.Bold, _ink, 364));
            var txtAd = AddTextBox(form, "Ad", 24, 100, 170);
            var txtSoyad = AddTextBox(form, "Soyad", 218, 100, 170);
            var txtKimlik = AddTextBox(form, "T.C. Kimlik No", 24, 170, 364);
            var txtTelefon = AddTextBox(form, "Telefon", 24, 240, 364);
            var txtAdres = AddTextBox(form, "Adres", 24, 310, 364);
            var grid = CreateGrid<Musteri>(440, 126, ContentWidth() - 440, 500);
            _contentPanel.Controls.Add(grid);
            var selectedID = string.Empty;

            void RefreshGrid()
            {
                grid.DataSource = new BindingList<Musteri>(_musteriManager.MusterileriGetir());
                HideColumns(grid, "KayitTarihi", "AdSoyad");
                SetColumnHeaders(grid,
                    ("MusteriID", "Musteri No"),
                    ("KimlikNo", "T.C. Kimlik"),
                    ("Telefon", "Telefon"),
                    ("Adres", "Adres"));
            }

            grid.SelectionChanged += (_, _) =>
            {
                if (grid.CurrentRow?.DataBoundItem is not Musteri m)
                {
                    return;
                }

                selectedID = m.MusteriID;
                txtAd.Text = m.Ad;
                txtSoyad.Text = m.Soyad;
                txtKimlik.Text = m.KimlikNo;
                txtTelefon.Text = m.Telefon;
                txtAdres.Text = m.Adres;
            };

            form.Controls.Add(CreateActionButton("Kaydet", 24, 390, 170, _green, () => Safe(() =>
            {
                _musteriManager.MusteriEkle(new Musteri
                {
                    Ad = txtAd.Text,
                    Soyad = txtSoyad.Text,
                    KimlikNo = txtKimlik.Text,
                    Telefon = DigitsOnly(txtTelefon.Text),
                    Adres = txtAdres.Text
                });
                RefreshGrid();
            }, "Musteri kaydedildi.")));

            form.Controls.Add(CreateActionButton("Guncelle", 218, 390, 170, Color.White, () => Safe(() =>
            {
                _musteriManager.MusteriGuncelle(new Musteri
                {
                    MusteriID = selectedID,
                    Ad = txtAd.Text,
                    Soyad = txtSoyad.Text,
                    KimlikNo = txtKimlik.Text,
                    Telefon = DigitsOnly(txtTelefon.Text),
                    Adres = txtAdres.Text
                });
                RefreshGrid();
            }, "Musteri guncellendi."), _ink));

            form.Controls.Add(CreateActionButton("Sil", 24, 442, 364, Color.FromArgb(249, 232, 232), () =>
            {
                if (Confirm("Secili musteri silinsin mi?"))
                {
                    Safe(() =>
                    {
                        _musteriManager.MusteriSil(selectedID);
                        RefreshGrid();
                    }, "Musteri silindi.");
                }
            }, Color.FromArgb(190, 45, 45)));

            RefreshGrid();
        }
        catch (Exception ex)
        {
            ShowPageError(ex);
        }
        finally
        {
            FinishPage();
        }
    }

    private void ShowAssets()
    {
        PreparePage("Varliklar");
        try
        {
            AddHeader("Varliklar", "Tiny house, karavan ve bungalov durumlarini yonetin.", null, null);

            var form = CreateCard(0, 126, 420, 500, _green);
            _contentPanel.Controls.Add(form);
            form.Controls.Add(CreateLabel("Varlik Kaydi", 24, 24, 20, FontStyle.Bold, _ink, 364));
            var cmbTip = AddCombo<string>(form, "Tip", 24, 100, 170, ["Tiny House", "Karavan", "Bungalov"], null, null);
            var cmbDurum = AddCombo<VarlikDurumu>(form, "Durum", 218, 100, 170, Enum.GetValues<VarlikDurumu>().ToList(), null, null);
            var txtAd = AddTextBox(form, "Ad", 24, 170, 364);
            var txtKonum = AddTextBox(form, "Konum", 24, 240, 364);
            var numKapasite = AddNumeric(form, "Kapasite", 24, 310, 170, 1, 20);
            var numUcret = AddNumeric(form, "Gunluk Ucret", 218, 310, 170, 100, 100000);
            var grid = CreateGrid<Varlik>(440, 126, ContentWidth() - 440, 500);
            _contentPanel.Controls.Add(grid);
            var selectedID = string.Empty;

            void RefreshGrid()
            {
                grid.DataSource = new BindingList<Varlik>(_varlikManager.VarliklariGetir());
                SetColumnHeaders(grid,
                    ("VarlikID", "Varlik No"),
                    ("VarlikTipi", "Tip"),
                    ("GunlukUcret", "Gunluk Ucret"),
                    ("Kapasite", "Kapasite"),
                    ("Durum", "Durum"),
                    ("Konum", "Konum"));
            }

            grid.SelectionChanged += (_, _) =>
            {
                if (grid.CurrentRow?.DataBoundItem is not Varlik v)
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

            form.Controls.Add(CreateActionButton("Kaydet", 24, 390, 170, _green, () => Safe(() =>
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
            }, "Varlik kaydedildi.")));

            form.Controls.Add(CreateActionButton("Guncelle", 218, 390, 170, Color.White, () => Safe(() =>
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
            }, "Varlik guncellendi."), _ink));

            form.Controls.Add(CreateActionButton("Sil", 24, 442, 364, Color.FromArgb(249, 232, 232), () =>
            {
                if (Confirm("Secili varlik silinsin mi?"))
                {
                    Safe(() =>
                    {
                        _varlikManager.VarlikSil(selectedID);
                        RefreshGrid();
                    }, "Varlik silindi.");
                }
            }, Color.FromArgb(190, 45, 45)));

            RefreshGrid();
        }
        catch (Exception ex)
        {
            ShowPageError(ex);
        }
        finally
        {
            FinishPage();
        }
    }

    private void ShowOperations()
    {
        PreparePage("Operasyon");
        try
        {
            AddHeader("Operasyon", "Temizlik ve bakim sureclerini takip edin.", null, null);
            var form = CreateCard(0, 126, 420, 430, _orange);
            _contentPanel.Controls.Add(form);
            form.Controls.Add(CreateLabel("Operasyon Kaydi", 24, 24, 20, FontStyle.Bold, _ink, 364));
            var cmbVarlik = AddCombo<Varlik>(form, "Varlik", 24, 100, 364, _varlikManager.VarliklariGetir(), "Ad", "VarlikID");
            var cmbTip = AddCombo<OperasyonTipi>(form, "Tip", 24, 168, 170, Enum.GetValues<OperasyonTipi>().ToList(), null, null);
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
            var txtNot = AddTextBox(form, "Not", 24, 240, 364);
            var grid = CreateGrid<Operasyon>(440, 126, ContentWidth() - 440, 430);
            _contentPanel.Controls.Add(grid);
            var selectedID = string.Empty;

            void RefreshGrid()
            {
                grid.DataSource = new BindingList<Operasyon>(_operasyonManager.OperasyonlariGetir());
                HideColumns(grid, "VarlikID");
                SetColumnHeaders(grid,
                    ("OperasyonID", "Operasyon No"),
                    ("OperasyonTipi", "Tip"),
                    ("Durum", "Tamamlandi"),
                    ("Tarih", "Tarih"),
                    ("Notlar", "Not"),
                    ("VarlikAdi", "Varlik"),
                    ("DurumMetni", "Durum"));
            }

            grid.SelectionChanged += (_, _) =>
            {
                if (grid.CurrentRow?.DataBoundItem is not Operasyon o)
                {
                    return;
                }

                selectedID = o.OperasyonID;
                cmbVarlik.SelectedValue = o.VarlikID;
                cmbTip.SelectedItem = o.OperasyonTipi;
                chkDurum.Checked = o.Durum;
                txtNot.Text = o.Notlar;
            };

            form.Controls.Add(CreateActionButton("Kaydet", 24, 320, 112, _green, () => Safe(() =>
            {
                _operasyonManager.OperasyonEkle(new Operasyon
                {
                    VarlikID = cmbVarlik.SelectedValue?.ToString() ?? string.Empty,
                    OperasyonTipi = cmbTip.SelectedItem is OperasyonTipi t ? t : OperasyonTipi.Temizlik,
                    Durum = chkDurum.Checked,
                    Tarih = DateTime.Today,
                    Notlar = txtNot.Text
                });
                RefreshGrid();
            }, "Operasyon kaydedildi.")));

            form.Controls.Add(CreateActionButton("Guncelle", 148, 320, 112, Color.White, () => Safe(() =>
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
            }, "Operasyon guncellendi."), _ink));

            form.Controls.Add(CreateActionButton("Tamamla", 272, 320, 116, _greenSoft, () => Safe(() =>
            {
                _operasyonManager.OperasyonTamamla(selectedID);
                RefreshGrid();
            }, "Operasyon tamamlandi."), _green));

            form.Controls.Add(CreateActionButton("Sil", 24, 364, 364, Color.White, () =>
            {
                if (Confirm("Secili operasyon silinsin mi?"))
                {
                    Safe(() =>
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
            ShowPageError(ex);
        }
        finally
        {
            FinishPage();
        }
    }

    private void ShowFinance()
    {
        PreparePage("Finans");
        try
        {
            AddHeader("Finans / Kasa", "Odeme, ekstra hizmet ve gelir akisini yonetin.", null, null);
            var reservations = _rezervasyonManager.RezervasyonlariGetir();

            var paymentForm = CreateCard(0, 126, 420, 390, _blue);
            _contentPanel.Controls.Add(paymentForm);
            paymentForm.Controls.Add(CreateLabel("Odeme Al", 24, 24, 20, FontStyle.Bold, _ink, 364));
            var cmbRez = AddCombo<Rezervasyon>(paymentForm, "Rezervasyon", 24, 100, 364, reservations, "SecimMetni", "RezervasyonID");
            var numTutar = AddNumeric(paymentForm, "Tutar", 24, 170, 170, 1, 1000000);
            var cmbTip = AddCombo<OdemeTipi>(paymentForm, "Odeme Tipi", 218, 170, 170, Enum.GetValues<OdemeTipi>().ToList(), null, null);
            var txtAciklama = AddTextBox(paymentForm, "Aciklama", 24, 240, 364);
            var paymentInfo = CreateLabel("Rezervasyon secildiginde tutar otomatik hesaplanir.", 24, 286, 10.5F, FontStyle.Bold, _green, 364);
            paymentForm.Controls.Add(paymentInfo);
            var paymentGrid = CreateGrid<Odeme>(440, 126, ContentWidth() - 440, 390);
            _contentPanel.Controls.Add(paymentGrid);
            var selectedPaymentID = string.Empty;

            void RefreshPayments()
            {
                paymentGrid.DataSource = new BindingList<Odeme>(_odemeManager.OdemeleriGetir());
                HideColumns(paymentGrid, "RezervasyonID");
                SetColumnHeaders(paymentGrid,
                    ("OdemeID", "Odeme No"),
                    ("Ucret", "Tutar"),
                    ("OdemeTarihi", "Tarih"),
                    ("OdemeTipi", "Tip"),
                    ("Aciklama", "Aciklama"),
                    ("MusteriAdSoyad", "Musteri"));
            }

            void FillPaymentAmount()
            {
                if (cmbRez.SelectedItem is not Rezervasyon rezervasyon)
                {
                    paymentInfo.Text = "Rezervasyon secildiginde tutar otomatik hesaplanir.";
                    return;
                }

                Safe(() =>
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

            paymentForm.Controls.Add(CreateActionButton("Kaydet", 24, 330, 112, _green, () => Safe(() =>
            {
                _odemeManager.OdemeEkle(new Odeme
                {
                    RezervasyonID = cmbRez.SelectedValue?.ToString() ?? string.Empty,
                    Ucret = numTutar.Value,
                    OdemeTarihi = DateTime.Today,
                    OdemeTipi = cmbTip.SelectedItem is OdemeTipi t ? t : OdemeTipi.Nakit,
                    Aciklama = txtAciklama.Text
                });
                RefreshPayments();
                FillPaymentAmount();
            }, "Odeme kaydedildi.")));

            paymentForm.Controls.Add(CreateActionButton("Guncelle", 148, 330, 112, Color.White, () => Safe(() =>
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
                RefreshPayments();
                FillPaymentAmount();
            }, "Odeme guncellendi."), _ink));

            paymentForm.Controls.Add(CreateActionButton("Sil", 272, 330, 116, Color.FromArgb(249, 232, 232), () =>
            {
                if (Confirm("Secili odeme silinsin mi?"))
                {
                    Safe(() =>
                    {
                        _odemeManager.OdemeSil(selectedPaymentID);
                        RefreshPayments();
                        FillPaymentAmount();
                    }, "Odeme silindi.");
                }
            }, Color.FromArgb(190, 45, 45)));

            var serviceForm = CreateCard(0, 550, 420, 320, _violet);
            _contentPanel.Controls.Add(serviceForm);
            serviceForm.Controls.Add(CreateLabel("Ekstra Hizmet", 24, 24, 20, FontStyle.Bold, _ink, 364));
            var cmbRezHizmet = AddCombo<Rezervasyon>(serviceForm, "Rezervasyon", 24, 100, 364, reservations, "SecimMetni", "RezervasyonID");
            var txtHizmet = AddTextBox(serviceForm, "Hizmet Adi", 24, 170, 170);
            var numHizmet = AddNumeric(serviceForm, "Ucret", 218, 170, 170, 0, 100000);
            var serviceGrid = CreateGrid<Hizmet>(440, 550, ContentWidth() - 440, 320);
            _contentPanel.Controls.Add(serviceGrid);
            var selectedServiceID = string.Empty;

            void RefreshServices()
            {
                serviceGrid.DataSource = new BindingList<Hizmet>(_hizmetManager.HizmetleriGetir());
                HideColumns(serviceGrid, "RezervasyonID");
                SetColumnHeaders(serviceGrid,
                    ("HizmetID", "Hizmet No"),
                    ("Ad", "Hizmet"),
                    ("Ucret", "Ucret"));
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

            serviceForm.Controls.Add(CreateActionButton("Ekle", 24, 240, 112, _green, () => Safe(() =>
            {
                _hizmetManager.HizmetEkle(new Hizmet
                {
                    RezervasyonID = cmbRezHizmet.SelectedValue?.ToString() ?? string.Empty,
                    Ad = txtHizmet.Text,
                    Ucret = numHizmet.Value
                });
                RefreshServices();
                ShowFinance();
            }, "Hizmet eklendi ve rezervasyon toplami yenilendi.")));

            serviceForm.Controls.Add(CreateActionButton("Guncelle", 148, 240, 112, Color.White, () => Safe(() =>
            {
                _hizmetManager.HizmetGuncelle(new Hizmet
                {
                    HizmetID = selectedServiceID,
                    RezervasyonID = cmbRezHizmet.SelectedValue?.ToString() ?? string.Empty,
                    Ad = txtHizmet.Text,
                    Ucret = numHizmet.Value
                });
                RefreshServices();
                ShowFinance();
            }, "Hizmet guncellendi ve rezervasyon toplami yenilendi."), _ink));

            serviceForm.Controls.Add(CreateActionButton("Sil", 272, 240, 116, Color.FromArgb(249, 232, 232), () =>
            {
                if (Confirm("Secili hizmet silinsin mi?"))
                {
                    Safe(() =>
                    {
                        _hizmetManager.HizmetSil(selectedServiceID);
                        RefreshServices();
                        ShowFinance();
                    }, "Hizmet silindi ve rezervasyon toplami yenilendi.");
                }
            }, Color.FromArgb(190, 45, 45)));

            RefreshPayments();
            FillPaymentAmount();
            RefreshServices();
        }
        catch (Exception ex)
        {
            ShowPageError(ex);
        }
        finally
        {
            FinishPage();
        }
    }

    private void ShowSettings()
    {
        PreparePage("Ayarlar");
        try
        {
            AddHeader("Ayarlar", "Profil, isletme ve bildirim tercihleri.", null, null);
            var user = GetCurrentUser();
            var settings = _profilManager.IsletmeAyarlariGetir();

            var profile = CreateCard(0, 126, 520, 230, _green);
            _contentPanel.Controls.Add(profile);
            var profileName = CreateLabel(user.AdSoyad, 32, 28, 22, FontStyle.Bold, _ink, 440);
            var profileRole = CreateLabel(user.Rol, 34, 68, 12, FontStyle.Regular, _muted, 240);
            profile.Controls.Add(profileName);
            profile.Controls.Add(profileRole);
            var txtAdSoyad = AddTextBox(profile, "Ad Soyad", 32, 124, 205);
            var txtEmail = AddTextBox(profile, "E-posta", 260, 124, 220);
            txtAdSoyad.Text = user.AdSoyad;
            txtEmail.Text = user.Email;
            profile.Controls.Add(CreateActionButton("Profili Guncelle", 32, 182, 205, _green, () => Safe(() =>
            {
                user.KullaniciID = CurrentUserId;
                user.AdSoyad = txtAdSoyad.Text;
                user.Email = txtEmail.Text;
                _profilManager.ProfilGuncelle(user);
                profileName.Text = user.AdSoyad;
                profileRole.Text = user.Rol;
                RefreshHeaderUser();
            }, "Profil guncellendi.")));

            var business = CreateCard(0, 390, 520, 360, _blue);
            _contentPanel.Controls.Add(business);
            business.Controls.Add(CreateLabel("ISLETME AYARLARI", 32, 24, 13, FontStyle.Bold, _muted));
            var txtIsletme = AddTextBox(business, "Isletme Bilgileri", 32, 92, 448);
            var txtPara = AddTextBox(business, "Para Birimi", 32, 162, 205);
            var txtDil = AddTextBox(business, "Dil", 260, 162, 220);
            var chkRez = new CheckBox { Text = "Yeni rezervasyon bildirimleri", Left = 32, Top = 232, Width = 260, Checked = settings.RezervasyonBildirimleri, BackColor = _card, ForeColor = _ink };
            var chkTem = new CheckBox { Text = "Temizlik uyarilari", Left = 32, Top = 264, Width = 260, Checked = settings.TemizlikUyarilari, BackColor = _card, ForeColor = _ink };
            business.Controls.Add(chkRez);
            business.Controls.Add(chkTem);
            txtIsletme.Text = settings.IsletmeAdi;
            txtPara.Text = settings.ParaBirimi;
            txtDil.Text = settings.Dil;

            business.Controls.Add(CreateActionButton("Ayarlari Kaydet", 32, 306, 448, _green, () => Safe(() =>
            {
                settings.IsletmeAdi = txtIsletme.Text;
                settings.ParaBirimi = txtPara.Text;
                settings.Dil = txtDil.Text;
                settings.RezervasyonBildirimleri = chkRez.Checked;
                settings.TemizlikUyarilari = chkTem.Checked;
                _profilManager.IsletmeAyarlariGuncelle(settings);
            }, "Ayarlar kaydedildi.")));

            var list = CreateCard(560, 126, Math.Max(420, ContentWidth() - 560), 624, _coral);
            _contentPanel.Controls.Add(list);
            list.Controls.Add(CreateLabel("SISTEM VE GUVENLIK", 32, 24, 13, FontStyle.Bold, _muted));
            list.Controls.Add(CreateLabel("SIFRE DEGISTIR", 32, 78, 13, FontStyle.Bold, _ink, 260));
            var txtMevcutSifre = AddTextBox(list, "Mevcut Sifre", 32, 142, 200);
            var txtYeniSifre = AddTextBox(list, "Yeni Sifre", 256, 142, 200);
            var txtYeniSifreTekrar = AddTextBox(list, "Yeni Sifre Tekrar", 32, 212, 200);
            txtMevcutSifre.UseSystemPasswordChar = true;
            txtYeniSifre.UseSystemPasswordChar = true;
            txtYeniSifreTekrar.UseSystemPasswordChar = true;
            list.Controls.Add(CreateActionButton("Sifreyi Kaydet", 256, 212, 200, _green, () => Safe(() =>
            {
                _profilManager.SifreDegistir(CurrentUserId, txtMevcutSifre.Text, txtYeniSifre.Text, txtYeniSifreTekrar.Text);
                txtMevcutSifre.Clear();
                txtYeniSifre.Clear();
                txtYeniSifreTekrar.Clear();
            }, "Sifre guncellendi.")));
            AddSettingsRow(list, "Veri Yedekleme", "SQL scripti ve LocalDB veritabani ile teslim edilir.", 304);
            AddSettingsRow(list, "Kullanim Kosullari", "TinyTrack ERP v1.0 - Gorsel Programlama II final projesi.", 384);
            list.Controls.Add(CreateActionButton("Cikis Yap", 32, 540, Math.Min(440, list.Width - 64), Color.FromArgb(249, 232, 232), () => Close(), Color.FromArgb(190, 45, 45)));
        }
        catch (Exception ex)
        {
            ShowPageError(ex);
        }
        finally
        {
            FinishPage();
        }
    }

    private void AddHeader(string title, string subtitle, string? buttonText, Action? buttonAction)
    {
        var titleLabel = CreateLabel(title, 0, 0, 28, FontStyle.Bold, _ink, ContentWidth() - 280);
        _contentPanel.Controls.Add(titleLabel);
        var subtitleLabel = CreateLabel(subtitle, 2, 62, 13, FontStyle.Regular, _muted, ContentWidth() - 280);
        _contentPanel.Controls.Add(subtitleLabel);

        if (!string.IsNullOrWhiteSpace(buttonText) && buttonAction is not null)
        {
            _contentPanel.Controls.Add(CreateActionButton(buttonText, ContentWidth() - 230, 18, 220, _navy, buttonAction));
        }
    }

    private void AddCalendarStrip(int left, int top, Action<DateTime>? onDateSelected = null)
    {
        var strip = CreateCard(left, top, ContentWidth(), 130);
        _contentPanel.Controls.Add(strip);
        strip.Controls.Add(CreateLabel(DateTime.Today.ToString("MMMM yyyy"), 28, 22, 15, FontStyle.Bold, _ink));

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
            var date = DateTime.Today.AddDays(i);
            var active = date.Date == _selectedCalendarDate.Date;
            var dayButton = new Button
            {
                Width = 78,
                Height = 60,
                Margin = new Padding(0, 0, 14, 0),
                BackColor = active ? Color.Black : Color.FromArgb(241, 244, 248),
                ForeColor = active ? Color.White : _ink,
                FlatStyle = FlatStyle.Flat,
                Text = $"{date:ddd}\r\n{date:dd}",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Tag = date,
                TabStop = false,
                TextAlign = ContentAlignment.MiddleCenter
            };
            dayButton.FlatAppearance.BorderSize = 0;
            dayButton.Click += (_, _) =>
            {
                _selectedCalendarDate = date.Date;
                foreach (Button button in days.Controls.OfType<Button>())
                {
                    var isSelected = button.Tag is DateTime buttonDate && buttonDate.Date == _selectedCalendarDate.Date;
                    button.BackColor = isSelected ? Color.Black : Color.FromArgb(241, 244, 248);
                    button.ForeColor = isSelected ? Color.White : _ink;
                }

                onDateSelected?.Invoke(_selectedCalendarDate);
            };
            days.Controls.Add(dayButton);
        }
    }

    private void AddLegend(int left, int top)
    {
        AddDotLegend(left, top, _green, "MUSAIT");
        AddDotLegend(left + 120, top, Color.FromArgb(78, 101, 132), "DOLU");
        AddDotLegend(left + 220, top, _yellow, "TEMIZLIKTE");
    }

    private void AddDotLegend(int left, int top, Color color, string text)
    {
        var dot = new Panel { Left = left + 6, Top = top + 5, Width = 14, Height = 14, BackColor = color };
        var label = CreateLabel(text, left + 28, top, 10, FontStyle.Bold, _ink);
        _contentPanel.Controls.Add(dot);
        _contentPanel.Controls.Add(label);
    }

    private Panel CreateStatCard(string title, string value, string detail, Color accent)
    {
        var panel = CreateCard(0, 0, 245, 120, accent);
        panel.FillColor = Mix(Color.White, accent, 0.08);
        panel.BorderColor = Mix(Color.White, accent, 0.34);
        panel.Margin = new Padding(0, 0, 14, 0);
        panel.Controls.Add(new Panel { Left = 24, Top = 22, Width = 42, Height = 28, BackColor = Mix(Color.White, accent, 0.18) });
        panel.Controls.Add(CreateLabel(title, 92, 24, 9, FontStyle.Bold, accent, 150));
        panel.Controls.Add(CreateLabel(value, 24, 58, 26, FontStyle.Bold, _ink));
        panel.Controls.Add(CreateLabel(detail, 94, 72, 10, FontStyle.Regular, _muted));
        return panel;
    }

    private void DrawRevenueBars(Graphics graphics, Rectangle bounds)
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

    private Button CreateActionButton(string text, int left, int top, int width, Color backColor, Action action, Color? foreColor = null)
    {
        var button = new Button
        {
            Text = text,
            Left = left,
            Top = top,
            Width = width,
            Height = 42,
            BackColor = backColor,
            ForeColor = foreColor ?? Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        button.FlatAppearance.BorderSize = backColor == Color.White ? 1 : 0;
        button.FlatAppearance.BorderColor = _line;
        button.Click += (_, _) => action();
        return button;
    }

    private Label CreateBadge(string text, int left, int top, int width, Color back, Color fore)
    {
        var label = new Label
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
        return label;
    }

    private Label CreateLabel(string text, int left, int top, float size, FontStyle style, Color color, int width = 260)
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
            ForeColor = color,
            BackColor = Color.Transparent
        };
    }

    private CardPanel CreateCard(int left, int top, int width, int height, Color? accent = null)
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

    private DataGridView CreateGrid<T>(int left, int top, int width, int height)
    {
        var grid = new DataGridView
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
        grid.ColumnHeadersDefaultCellStyle.BackColor = _navy;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grid.ColumnHeadersHeight = 34;
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 251, 249);
        grid.DefaultCellStyle.SelectionBackColor = _greenSoft;
        grid.DefaultCellStyle.SelectionForeColor = _ink;
        grid.GridColor = _line;
        grid.RowTemplate.Height = 30;
        return grid;
    }

    private static Color Mix(Color baseColor, Color overlay, double overlayAmount)
    {
        overlayAmount = Math.Clamp(overlayAmount, 0, 1);
        var baseAmount = 1 - overlayAmount;
        return Color.FromArgb(
            (int)((baseColor.R * baseAmount) + (overlay.R * overlayAmount)),
            (int)((baseColor.G * baseAmount) + (overlay.G * overlayAmount)),
            (int)((baseColor.B * baseAmount) + (overlay.B * overlayAmount)));
    }

    private TextBox AddTextBox(Control parent, string label, int left, int top, int width)
    {
        parent.Controls.Add(CreateLabel(label, left, top - 30, 9.5F, FontStyle.Bold, _ink, width));
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

    private NumericUpDown AddNumeric(Control parent, string label, int left, int top, int width, decimal min, decimal max)
    {
        parent.Controls.Add(CreateLabel(label, left, top - 30, 9.5F, FontStyle.Bold, _ink, width));
        var number = new NumericUpDown
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
        parent.Controls.Add(number);
        return number;
    }

    private DateTimePicker AddDate(Control parent, string label, int left, int top, int width, DateTime value)
    {
        parent.Controls.Add(CreateLabel(label, left, top - 30, 9.5F, FontStyle.Bold, _ink, width));
        var picker = new DateTimePicker
        {
            Left = left,
            Top = top,
            Width = width,
            Height = 34,
            Format = DateTimePickerFormat.Short,
            Value = value,
            Font = new Font("Segoe UI", 10F)
        };
        parent.Controls.Add(picker);
        return picker;
    }

    private ComboBox AddCombo<T>(Control parent, string label, int left, int top, int width, List<T> data, string? displayMember, string? valueMember)
    {
        parent.Controls.Add(CreateLabel(label, left, top - 30, 9.5F, FontStyle.Bold, _ink, width));
        var combo = new ComboBox
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
            combo.DisplayMember = displayMember;
        }

        if (!string.IsNullOrWhiteSpace(valueMember))
        {
            combo.ValueMember = valueMember;
        }

        parent.Controls.Add(combo);
        return combo;
    }

    private void AddSettingsRow(Control parent, string title, string detail, int top)
    {
        var row = CreateCard(32, top, parent.Width - 64, 62);
        parent.Controls.Add(row);
        row.Controls.Add(CreateLabel(title, 20, 10, 13, FontStyle.Bold, _ink, row.Width - 80));
        row.Controls.Add(CreateLabel(detail, 20, 34, 9.5F, FontStyle.Regular, _muted, row.Width - 80));
        row.Controls.Add(CreateLabel(">", row.Width - 45, 18, 16, FontStyle.Bold, _muted, 24));
    }

    private void HideColumns(DataGridView grid, params string[] names)
    {
        foreach (var name in names)
        {
            if (grid.Columns.Contains(name))
            {
                grid.Columns[name]!.Visible = false;
            }
        }
    }

    private void SetColumnHeaders(DataGridView grid, params (string Column, string Header)[] headers)
    {
        foreach (var (column, header) in headers)
        {
            if (grid.Columns.Contains(column))
            {
                grid.Columns[column]!.HeaderText = header;
            }
        }
    }

    private int ContentWidth()
    {
        return Math.Max(1040, _contentPanel.ClientSize.Width - _contentPanel.Padding.Horizontal - 18);
    }

    private static string DigitsOnly(string text)
    {
        return new string(text.Where(char.IsDigit).ToArray());
    }

    private static string ShortName(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "Yonetici";
        }

        return text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Yonetici";
    }

    private bool Confirm(string message)
    {
        return MessageBox.Show(message, "TinyTrack ERP", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }

    private void Safe(Action action, string? successMessage = null)
    {
        try
        {
            action();
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

    private void ShowPageError(Exception ex)
    {
        _contentPanel.Controls.Add(CreateLabel("Sayfa yuklenemedi", 0, 90, 22, FontStyle.Bold, _ink, 500));
        _contentPanel.Controls.Add(CreateLabel(ex.Message, 0, 132, 11, FontStyle.Regular, _muted, 760));
    }
}

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
