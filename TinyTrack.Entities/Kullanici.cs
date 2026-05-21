namespace TinyTrack.Entities;

public class Kullanici
{
    public string KullaniciID { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Sifre { get; set; } = string.Empty;
    public string Rol { get; set; } = "Yonetici";
}
