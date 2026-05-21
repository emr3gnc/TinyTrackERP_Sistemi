namespace TinyTrack.Entities;

public class Hizmet
{
    public string HizmetID { get; set; } = string.Empty;
    public string RezervasyonID { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public decimal Ucret { get; set; }
}
