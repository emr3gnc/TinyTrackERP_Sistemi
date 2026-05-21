namespace TinyTrack.Entities;

public class Musteri
{
    public string MusteriID { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Adres { get; set; } = string.Empty;
    public string KimlikNo { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; } = DateTime.Now;

    public string AdSoyad => $"{Ad} {Soyad}".Trim();
}
