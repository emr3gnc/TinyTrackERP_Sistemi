namespace TinyTrack.Entities;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class Musteri
{
    public string MusteriID { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Adres { get; set; } = string.Empty;
    public string Il { get; set; } = string.Empty;
    public string Ilce { get; set; } = string.Empty;
    public string AcikAdres { get; set; } = string.Empty;
    public string PostaKodu { get; set; } = string.Empty;
    public string KimlikNo { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; } = DateTime.Now;

    public string AdSoyad => $"{Ad} {Soyad}".Trim();
    public string TamAdres => string.Join(" ", new[] { AcikAdres, Ilce, Il, PostaKodu }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
}
