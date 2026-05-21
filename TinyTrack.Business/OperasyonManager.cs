using TinyTrack.DataAccess;
using TinyTrack.Entities;

namespace TinyTrack.Business;

public class OperasyonManager
{
    private readonly OperasyonDal _operasyonDal = new();
    private readonly VarlikDal _varlikDal = new();

    public List<Operasyon> OperasyonlariGetir()
    {
        return _operasyonDal.GetAll();
    }

    public Operasyon? OperasyonGetir(string operasyonID)
    {
        return _operasyonDal.GetById(operasyonID);
    }

    public bool OperasyonEkle(Operasyon operasyon)
    {
        Validate(operasyon);
        operasyon.OperasyonID = string.IsNullOrWhiteSpace(operasyon.OperasyonID)
            ? IdGenerator.NewId("OPR")
            : operasyon.OperasyonID;
        var result = _operasyonDal.Insert(operasyon);
        if (result)
        {
            VarlikDurumunuOperasyonaGoreAyarla(operasyon);
        }
        return result;
    }

    public bool OperasyonGuncelle(Operasyon operasyon)
    {
        Validate(operasyon);
        if (string.IsNullOrWhiteSpace(operasyon.OperasyonID))
        {
            throw new BusinessRuleException("Guncellenecek operasyon secilmelidir.");
        }

        var result = _operasyonDal.Update(operasyon);
        if (result)
        {
            VarlikDurumunuOperasyonaGoreAyarla(operasyon);
        }

        return result;
    }

    public bool OperasyonTamamla(string operasyonID)
    {
        var operasyon = _operasyonDal.GetById(operasyonID)
            ?? throw new BusinessRuleException("Operasyon bulunamadi.");
        operasyon.Durum = true;
        return OperasyonGuncelle(operasyon);
    }

    public bool OperasyonSil(string operasyonID)
    {
        if (string.IsNullOrWhiteSpace(operasyonID))
        {
            throw new BusinessRuleException("Silinecek operasyon secilmelidir.");
        }

        var operasyon = _operasyonDal.GetById(operasyonID)
            ?? throw new BusinessRuleException("Operasyon bulunamadi.");
        var result = _operasyonDal.Delete(operasyonID);
        if (result && !operasyon.Durum && _operasyonDal.CountOpenByVarlik(operasyon.VarlikID) == 0)
        {
            _varlikDal.UpdateDurum(operasyon.VarlikID, VarlikDurumu.Musait);
        }

        return result;
    }

    private static void Validate(Operasyon operasyon)
    {
        if (string.IsNullOrWhiteSpace(operasyon.VarlikID))
        {
            throw new BusinessRuleException("Operasyon icin varlik secilmelidir.");
        }
    }

    private void VarlikDurumunuOperasyonaGoreAyarla(Operasyon operasyon)
    {
        if (operasyon.Durum)
        {
            _varlikDal.UpdateDurum(operasyon.VarlikID, VarlikDurumu.Musait);
            return;
        }

        _varlikDal.UpdateDurum(
            operasyon.VarlikID,
            operasyon.OperasyonTipi == OperasyonTipi.Bakim ? VarlikDurumu.Bakimda : VarlikDurumu.Temizlikte);
    }
}
