using TinyTrack.DataAccess;

namespace TinyTrack.Business;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public static class TinyTrackBootstrapper
{
    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public static void Initialize(string? connectionString)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            DbHelper.ConnectionString = connectionString;
        }

        DatabaseInitializer.OlusturuldugunuVeOrnekVerininHazirlandiginiGarantiEt();
    }
}
