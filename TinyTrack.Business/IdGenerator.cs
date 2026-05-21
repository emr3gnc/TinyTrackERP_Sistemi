namespace TinyTrack.Business;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
internal static class IdGenerator
{
    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public static string YeniId(string prefix)
    {
        return $"{prefix}-{DateTime.Now:yyMMddHHmmss}{Random.Shared.Next(10, 99)}";
    }
}
