namespace TinyTrack.Entities;

public class Rezervasyon
{
    public string RezervasyonID { get; set; } = string.Empty;
    public string MusteriID { get; set; } = string.Empty;
    public string VarlikID { get; set; } = string.Empty;
    public DateTime BasTarih { get; set; } = DateTime.Today;
    public DateTime SonTarih { get; set; } = DateTime.Today.AddDays(1);
    public decimal ToplamUcret { get; set; }
    public RezervasyonDurumu Durum { get; set; } = RezervasyonDurumu.Aktif;
    public DateTime KayitTarihi { get; set; } = DateTime.Now;

    public string MusteriAdSoyad { get; set; } = string.Empty;
    public string VarlikAdi { get; set; } = string.Empty;
    public int GeceSayisi => Math.Max(1, (SonTarih.Date - BasTarih.Date).Days);
    public string SecimMetni => $"{MusteriAdSoyad} - {VarlikAdi} ({BasTarih:dd.MM.yyyy}-{SonTarih:dd.MM.yyyy})";
}
