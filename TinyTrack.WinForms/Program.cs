using Microsoft.Extensions.Configuration;
using TinyTrack.Business;

namespace TinyTrack.WinForms;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            TinyTrackBootstrapper.Initialize(configuration.GetConnectionString("TinyTrackDb"));
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"TinyTrack baslatilirken hata olustu:\n{ex.Message}",
                "TinyTrack ERP",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }    
}
