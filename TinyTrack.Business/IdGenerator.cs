namespace TinyTrack.Business;

internal static class IdGenerator
{
    public static string NewId(string prefix)
    {
        return $"{prefix}-{DateTime.Now:yyMMddHHmmss}{Random.Shared.Next(10, 99)}";
    }
}
