namespace TinyTrack.Entities;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class Odeme
{
    public string OdemeID { get; set; } = string.Empty;
    public string RezervasyonID { get; set; } = string.Empty;
    public decimal Ucret { get; set; }
    public DateTime OdemeTarihi { get; set; } = DateTime.Today;
    public OdemeTipi OdemeTipi { get; set; } = OdemeTipi.Nakit;
    public string Aciklama { get; set; } = string.Empty;

    public string MusteriAdSoyad { get; set; } = string.Empty;
}
