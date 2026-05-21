namespace TinyTrack.Entities;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class DashboardOzet
{
    public int BugunkuGiris { get; set; }
    public int BugunkuCikis { get; set; }
    public int TemizliktekiVarlik { get; set; }
    public int AktifRezervasyon { get; set; }
    public decimal AylikGelir { get; set; }
    public double DolulukOrani { get; set; }
    public string SiradakiGiris { get; set; } = "Planli giris yok";
}
