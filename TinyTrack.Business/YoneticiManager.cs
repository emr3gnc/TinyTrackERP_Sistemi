using TinyTrack.DataAccess;
using TinyTrack.Entities;

namespace TinyTrack.Business;

public class YoneticiManager
{
    private readonly YoneticiDal _yoneticiDal = new();

    public DashboardOzet DashboardVeriOzetVerileriniGetir()
    {
        return _yoneticiDal.GetDashboardData();
    }

    public decimal AylikGelirHesapla(int ay, int yil)
    {
        return _yoneticiDal.GetAylikToplamGelir(ay, yil);
    }

    public double DolulukOraniHesapla()
    {
        return _yoneticiDal.GetDolulukOrani();
    }

    public int GunlukGirisCikisSayisi(DateTime tarih)
    {
        return _yoneticiDal.GetGunlukGirisCikisSayisi(tarih);
    }

    public bool SistemGirisKontrol(string kAdi, string sifre)
    {
        return _yoneticiDal.CheckAdminLogin(kAdi, sifre);
    }

    public int AktifRezervasyonSayisi()
    {
        return _yoneticiDal.GetAktifRezervasyonSayisi();
    }

    public int TemizliktekiVarlikSayisi()
    {
        return _yoneticiDal.GetTemizliktekiVarlikSayisi();
    }
}
