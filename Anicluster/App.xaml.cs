using Anicluster.Database;
using Anicluster.Database.Services;
using Anicluster.Dialogs;
using Anicluster.ViewModels;
using Serilog;
using System.IO;
using System.Windows;

namespace Anicluster {

    /// <summary>
    /// Einstiegspunkt der Anwendung und Composition Root:
    /// Hier werden Logging, Datenbank, Services und das Hauptfenster zusammengesteckt.
    /// </summary>
    public partial class App : Application {

        private const string ConnectionString = "Data Source=db/data.sqlite";

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            this.ConfigureLogging();

            Log.Information("Application started.");
            DatabaseManager dbManager;
            try {
                Log.Information("Test SQLite-DB connection");
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db"));
                dbManager = new DatabaseManager(ConnectionString);
                DatabaseInitializer.CheckAndInitialize(dbManager);
                Log.Information("Connection to SQLite-DB available");
            }
            catch (Exception ex) {
                Log.Error(ex.StackTrace ?? "Error without stacktrace...");
                MessageBox.Show("Fehler beim Start der Anwendung!");
                this.Shutdown();
                return;
            }

            IDialogService dialogService = new DialogService();
            MainViewModel mainViewModel = new MainViewModel(new AnimeService(dbManager), new TagService(dbManager), dialogService);
            this.MainWindow = new MainWindow { DataContext = mainViewModel };
            this.MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e) {
            Log.Information("Application shutdown.");
            Log.CloseAndFlush();
            base.OnExit(e);
        }

        private void ConfigureLogging() {
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"));
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/log_.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
}
