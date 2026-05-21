using TinyTrack.DataAccess;

namespace TinyTrack.Business;

public static class TinyTrackBootstrapper
{
    public static void Initialize(string? connectionString)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            DbHelper.ConnectionString = connectionString;
        }

        DatabaseInitializer.EnsureCreatedAndSeeded();
    }
}
