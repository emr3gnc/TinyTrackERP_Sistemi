namespace TinyTrack.Business;

// Bu sınıfta ilgili sorumluluğu birlikte topluyoruz.
public class BusinessRuleException : Exception
{
    // Bu blokta ilgili işlemi birlikte yürütüyoruz.
    public BusinessRuleException(string mesaj) : base(mesaj)
    {
    }
}
