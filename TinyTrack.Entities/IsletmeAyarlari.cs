namespace TinyTrack.Entities;

public class IsletmeAyarlari
{
    public string AyarID { get; set; } = "SET-001";
    public string IsletmeAdi { get; set; } = "TinyTrack";
    public string ParaBirimi { get; set; } = "Turk Lirasi";
    public string Dil { get; set; } = "Turkce";
    public bool RezervasyonBildirimleri { get; set; } = true;
    public bool TemizlikUyarilari { get; set; } = true;
}
