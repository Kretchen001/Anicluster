using Anicluster.Database;
using Anicluster.Database.Services;
using Anicluster.Database.Services.AssociativeEntities;
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

                    AcousticService acousticService = new AcousticService(dbManager);
                    if (acousticService.TableExist() == false) { acousticService.InitializeAcousticTable(); }
                    AnimeService animeService = new AnimeService(dbManager);
                    if (animeService.TableExist() == false) { animeService.InitializeAnimeTable(); }
                    InterruptionService interruptionService = new InterruptionService(dbManager);
                    if (interruptionService.TableExist() == false) { interruptionService.InitializeInterruptionTable(); }
                    MediaInfoService mediaInfoService = new MediaInfoService(dbManager);
                    if (mediaInfoService.TableExist() == false) { mediaInfoService.InitializeMediaInfoTable(); }
                    MusicPieceService musicPieceService = new MusicPieceService(dbManager);
                    if (musicPieceService.TableExist() == false) { musicPieceService.InitializeMusicPieceTable(); }
                    PublishingTimeService publishingTimeService = new PublishingTimeService(dbManager);
                    if (publishingTimeService.TableExist() == false) { publishingTimeService.InitializePublishingTimeTable(); }
                    RatingService ratingService = new RatingService(dbManager);
                    if (ratingService.TableExist() == false) { ratingService.InitializeRatingTable(); }
                    SeasonService seasonService = new SeasonService(dbManager);
                    if (seasonService.TableExist() == false) { seasonService.InitializeSeasonTable(); }
                    SyncroService syncroService = new SyncroService(dbManager);
                    if (syncroService.TableExist() == false) { syncroService.InitializeSyncroTable(); }
                    TagService tagService = new TagService(dbManager);
                    if (tagService.TableExist() == false) { tagService.InitializeTagTable(); }
                    VideoAnimationService videoAnimationService = new VideoAnimationService(dbManager);
                    if (videoAnimationService.TableExist() == false) { videoAnimationService.InitializeVideoAnimationTable(); }
                    AnimeTagAssociativeEntityService animeTagAssociativeEntityService = new AnimeTagAssociativeEntityService(dbManager);
                    if (animeTagAssociativeEntityService.TableExist() == false) { animeTagAssociativeEntityService.InitializeAnimeTagAssociativeEntityTable(); }

                    connection.Close();
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
        }

        protected override void OnExit(ExitEventArgs e) {
            Log.Information("Application shutdown.");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
