using TinyTrack.DataAccess;
using TinyTrack.Entities;

namespace TinyTrack.Business;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class YoneticiManager
{
    private readonly YoneticiDal _yoneticiDal = new();

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public DashboardOzet DashboardVeriOzetVerileriniGetir()
    {
        return _yoneticiDal.PanelVerisiniGetir();
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public decimal AylikGelirHesapla(int ay, int yil)
    {
        return _yoneticiDal.AylikToplamGeliriGetir(ay, yil);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public double DolulukOraniHesapla()
    {
        return _yoneticiDal.DolulukOraniniGetir();
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public int GunlukGirisCikisSayisi(DateTime tarih)
    {
        return _yoneticiDal.GunlukGirisCikisSayisiniGetir(tarih);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public bool SistemGirisKontrol(string kAdi, string sifre)
    {
        return _yoneticiDal.CheckAdminLogin(kAdi, sifre);
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public int AktifRezervasyonSayisi()
    {
        return _yoneticiDal.AktifRezervasyonSayisiniGetir();
    }

    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public int TemizliktekiVarlikSayisi()
    {
        return _yoneticiDal.TemizliktekiVarlikSayisiniGetir();
    }
}
