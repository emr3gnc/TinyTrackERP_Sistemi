namespace TinyTrack.Entities;

public class Varlik
{
    public string VarlikID { get; set; } = string.Empty;
    public string VarlikTipi { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public int Kapasite { get; set; }
    public decimal GunlukUcret { get; set; }
    public VarlikDurumu Durum { get; set; } = VarlikDurumu.Musait;
    public string Konum { get; set; } = string.Empty;
}
