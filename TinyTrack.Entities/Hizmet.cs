namespace TinyTrack.Entities;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class Hizmet
{
    public string HizmetID { get; set; } = string.Empty;
    public string RezervasyonID { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public decimal Ucret { get; set; }
}
