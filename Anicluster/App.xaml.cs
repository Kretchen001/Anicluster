using Anicluster.Database;
using Anicluster.Database.Services;
using Anicluster.Database.Services.AssociativeEntities;
using Microsoft.Data.Sqlite;
using Modells.Anime;
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
            try {
                Log.Information("Test SQLite-DB connection");
                CheckAndInitializeDb();
                Log.Information("Connection to SQLite-DB available");
            }
            catch (Exception ex) {
                Log.Error(ex.StackTrace ?? "Error without stacktrace...");
                MessageBox.Show("Fehler beim Start der Anwendung!");
                base.Shutdown();
                return;
            }
            base.OnStartup(e);
            Anime x = new AnimeService(new DatabaseManager($"Data Source=db/data.sqlite")).SelectAnimeByX(1);
        }

        protected override void OnExit(ExitEventArgs e) {
            Log.Information("Application shutdown.");
            Log.CloseAndFlush();
            base.OnExit(e);
        }

        private static void CheckAndInitializeDb() {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "db")) {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "db");
            }
            DatabaseManager dbManager = new DatabaseManager($"Data Source=db/data.sqlite");
            using (SqliteConnection connection = dbManager.GetConnection()) {
                connection.Open();
                connection.Close();

                AcousticService acousticService = new AcousticService(dbManager);
                if (acousticService.TableExist() == false) { acousticService.InitializeTable(); }
                AcousticMusicPieceAssociativeEntityService ampaes = new AcousticMusicPieceAssociativeEntityService(dbManager);
                if (ampaes.TableExist() == false) { ampaes.InitializeTable(); }
                AcousticSyncroAssociativeEntityService acousticSyncroAssociativeEntitiesService = new AcousticSyncroAssociativeEntityService(dbManager);
                if (acousticSyncroAssociativeEntitiesService.TableExist() == false) { acousticSyncroAssociativeEntitiesService.InitializeTable(); }
                AnimeService animeService = new AnimeService(dbManager);
                if (animeService.TableExist() == false) { animeService.InitializeTable(); }
                AnimeTagAssociativeEntityService animeTagAssociativeEntity = new AnimeTagAssociativeEntityService(dbManager);
                if (animeTagAssociativeEntity.TableExist() == false) { animeTagAssociativeEntity.InitializeTable(); }
                AnimeSeasonAssociativeEntityService animeSeasonAssociativeEntityService = new AnimeSeasonAssociativeEntityService(dbManager);
                if (animeSeasonAssociativeEntityService.TableExist() == false) { animeSeasonAssociativeEntityService.InitializeTable(); }
                AnimeOvaAssociativeEntityService aoaes = new AnimeOvaAssociativeEntityService(dbManager);
                if (aoaes.TableExist() == false) { aoaes.InitializeTable(); }
                InterruptionService interruptionService = new InterruptionService(dbManager);
                if (interruptionService.TableExist() == false) { interruptionService.InitializeTable(); }
                MediaInfoService mediaInfoService = new MediaInfoService(dbManager);
                if (mediaInfoService.TableExist() == false) { mediaInfoService.InitializeTable(); }
                MusicPieceService musicPieceService = new MusicPieceService(dbManager);
                if (musicPieceService.TableExist() == false) { musicPieceService.InitializeTable(); }
                PublishingTimeService publishingTimeService = new PublishingTimeService(dbManager);
                if (publishingTimeService.TableExist() == false) { publishingTimeService.InitializeTable(); }
                PublishingTimeInterruptionAssociativeEntityService ptiaes = new PublishingTimeInterruptionAssociativeEntityService(dbManager);
                if (ptiaes.TableExist() == false) { ptiaes.InitializeTable(); }
                RatingService ratingService = new RatingService(dbManager);
                if (ratingService.TableExist() == false) { ratingService.InitializeTable(); }
                SeasonService seasonService = new SeasonService(dbManager);
                if (seasonService.TableExist() == false) { seasonService.InitializeTable(); }
                SeasonVideoAnimationAssociativeEntityService svaaes = new SeasonVideoAnimationAssociativeEntityService(dbManager);
                if (svaaes.TableExist() == false) { svaaes.InitializeTable(); }
                SyncroService syncroService = new SyncroService(dbManager);
                if (syncroService.TableExist() == false) { syncroService.InitializeTable(); }
                StatusService statusService = new StatusService(dbManager);
                if (statusService.TableExist() == false) { statusService.InitializeTable(); }
                TagService tagService = new TagService(dbManager);
                if (tagService.TableExist() == false) { tagService.InitializeTable(); }
                VideoAnimationService videoAnimationService = new VideoAnimationService(dbManager);
                if (videoAnimationService.TableExist() == false) { videoAnimationService.InitializeTable(); }

                if (animeService.CreateViewAnimeComplete()) {
                    Log.Information("vw_Anime created");
                }
            }
        }
    }
}
