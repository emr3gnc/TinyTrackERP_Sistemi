namespace TinyTrack.Entities;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class Operasyon
{
    public string OperasyonID { get; set; } = string.Empty;
    public string VarlikID { get; set; } = string.Empty;
    public OperasyonTipi OperasyonTipi { get; set; } = OperasyonTipi.Temizlik;
    public bool Durum { get; set; }
    public DateTime Tarih { get; set; } = DateTime.Today;
    public string Notlar { get; set; } = string.Empty;

    public string VarlikAdi { get; set; } = string.Empty;
    public string DurumMetni => Durum ? "Tamamlandi" : "Devam ediyor";
}
