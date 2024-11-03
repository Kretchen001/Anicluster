using Anicluster.Database;
using Anicluster.Database.Services;
using Microsoft.Data.Sqlite;
using Serilog;
using System.IO;
using System.Windows;

namespace Anicluster {

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        protected override void OnStartup(StartupEventArgs e) {
            // Serilog konfigurieren
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "logs")) {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "logs");
            }
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/log_.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Application started.");
            Log.Information("Test SQLite-DB connection");
            try {
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "db")) {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "db");
                }
                DatabaseManager dbManager = new DatabaseManager($"Data Source=db/data.sqlite");
                using (SqliteConnection connection = dbManager.GetConnection()) {
                    connection.Open();
                    connection.Close();
                }
                AnimeService animeService = new AnimeService(dbManager);
                if (animeService.TableExists() == false) {
                    animeService.InitializeAnimeTable();
                }
            }
            catch (Exception ex) {
                Log.Error(ex.StackTrace ?? "Error without stacktrace...");
                MessageBox.Show("Fehler beim Start der Anwendung!");
                base.Shutdown();
                return;
            }
            Log.Information("Connection to SQLite-DB available");

            base.OnStartup(e);

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e) {
            Log.Information("Application shutdown.");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
