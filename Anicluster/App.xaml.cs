using Anicluster.Database;
using Anicluster.Database.Services;
using Anicluster.Database.Services.AssociativeEntities;
using Microsoft.Data.Sqlite;
using Modells.Anime.SoundRating;
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
            Log.Information("Test SQLite-DB connection");
            try {
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "db")) {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "db");
                }
                DatabaseManager dbManager = new DatabaseManager($"Data Source=db/data.sqlite");
                using (SqliteConnection connection = dbManager.GetConnection()) {
                    connection.Open();
                    connection.Close();

                    AcousticService acousticService = new AcousticService(dbManager);
                    if (acousticService.TableExist() == false) { acousticService.InitializeAcousticTable(); }
                    AcousticMusicPieceAssociativeEntityService acousticMusicPieceAssociativeEntitiesService = new AcousticMusicPieceAssociativeEntityService(dbManager);
                    if (acousticMusicPieceAssociativeEntitiesService.TableExist() == false) { acousticMusicPieceAssociativeEntitiesService.InitializeAcousticMusicPieceAssociativeEntitiesTable(); }
                    AcousticSyncroAssociativeEntityService acousticSyncroAssociativeEntitiesService = new AcousticSyncroAssociativeEntityService(dbManager);
                    if (acousticSyncroAssociativeEntitiesService.TableExist() == false) { acousticSyncroAssociativeEntitiesService.InitializeAcousticSyncroAssociativeEntityTable(); }
                    AnimeService animeService = new AnimeService(dbManager);
                    if (animeService.TableExist() == false) { animeService.InitializeAnimeTable(); }
                    AnimeTagAssociativeEntityService animeTagAssociativeEntity = new AnimeTagAssociativeEntityService(dbManager);
                    if (animeTagAssociativeEntity.TableExist() == false) { animeTagAssociativeEntity.InitializeAnimeTagAssociativeEntityTable(); }
                    InterruptionService interruptionService = new InterruptionService(dbManager);
                    if (interruptionService.TableExist() == false) { interruptionService.InitializeInterruptionTable(); }
                    MediaInfoService mediaInfoService = new MediaInfoService(dbManager);
                    if (mediaInfoService.TableExist() == false) { mediaInfoService.InitializeMediaInfoTable(); }
                    MusicPieceService musicPieceService = new MusicPieceService(dbManager);
                    if (musicPieceService.TableExist() == false) { musicPieceService.InitializeMusicPieceTable(); }
                    PublishingTimeService publishingTimeService = new PublishingTimeService(dbManager);
                    if (publishingTimeService.TableExist() == false) { publishingTimeService.InitializePublishingTimeTable(); }
                    PublishingTimeInterruptionAssociativeEntityService publishingTimeInterruptionAssociativeEntityService = new PublishingTimeInterruptionAssociativeEntityService(dbManager);
                    if (publishingTimeInterruptionAssociativeEntityService.TableExist() == false) { publishingTimeInterruptionAssociativeEntityService.InitializePublishingTimeTable(); }
                    RatingService ratingService = new RatingService(dbManager);
                    if (ratingService.TableExist() == false) { ratingService.InitializeRatingTable(); }
                    SeasonService seasonService = new SeasonService(dbManager);
                    if (seasonService.TableExist() == false) { seasonService.InitializeSeasonTable(); }
                    SyncroService syncroService = new SyncroService(dbManager);
                    if (syncroService.TableExist() == false) { syncroService.InitializeSyncroTable(); }
                    StatusService statusService = new StatusService(dbManager);
                    if (statusService.TableExist() == false) { statusService.InitializeStatusTable(); }
                    TagService tagService = new TagService(dbManager);
                    if (tagService.TableExist() == false) { tagService.InitializeTagTable(); }
                    VideoAnimationService videoAnimationService = new VideoAnimationService(dbManager);
                    if (videoAnimationService.TableExist() == false) { videoAnimationService.InitializeVideoAnimationTable(); }
                }
            }
            catch (Exception ex) {
                Log.Error(ex.StackTrace ?? "Error without stacktrace...");
                MessageBox.Show("Fehler beim Start der Anwendung!");
                base.Shutdown();
                return;
            }
            Log.Information("Connection to SQLite-DB available");

            //TestInsert();

            base.OnStartup(e);
        }

        private void TestInsert() {
            Anime a = new Anime();
            a.Id = 1;
            a.Name = "test";
            a.OriginalName = "testo";
            a.Url = "";
            a.Favorite = true;
            a.Rating = new Rating() {
                Story = 85,
                Animation = 10,
                SpecialEffects = 90,
                Acoustic = new Modells.Anime.SoundRating.Acoustic() {
                    Opening = [
                        new MusicPiece() {
                            Id = 1,
                            Name = "test",
                            Type = MusicPieceType.Opening,
                            Comment = "test",
                            General = 60
                        }
                    ],
                    Ending = [
                        new MusicPiece() {
                            Id = 1,
                            General = 67,
                            Name = "test",
                            Type = MusicPieceType.Ending,
                            Comment = "test",
                        }
                    ],
                    Soundtrack = 10,
                    Syncro = [new Syncro() {
                        General = 30,
                        IsAssessed = true,
                        Language = SyncroLanuage.jp
                    }],
                    Comment = "doll"
                }
            };
            a.Tier = Tier.S;
            a.Season = [new Season() {
                Episodes = [
                    new VideoAnimation(videoType: VideoType.Episode, comment: "demo")
                ],
                PublishingTime = new PublishingTime() {
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    Interruptions = [new Interruption() {
                        Start = DateTime.Now,
                        End = DateTime.Now,
                        Comment = "adfjlaskdjfölaskdjf"
                    }]
                },
                Comment = "hallo"
            }];
            a.Ovas = [
                new VideoAnimation(videoType: VideoType.Ova, comment: "demo"),
                new VideoAnimation(videoType: VideoType.Movie, comment: "demo"),
                new VideoAnimation(videoType: VideoType.Ova, comment: "demo")
            ];
            a.Status = new Status() {
                State = State.Started,
                Comment = "Danke Silberbaron"
            };
            a.Tags = [];
            a.MediaInfo = new MediaInfo() {
                Author = "David",
                Producer = "Tim",
                Publisher = "Konstantin",
                PublishingTime = new PublishingTime() {
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    Interruptions = [new Interruption() {
                        Start = DateTime.Now,
                        End = DateTime.Now,
                        Comment = "2341"
                    }]
                },
            };
            a.RecommendedFrom = "Silberbaron höchst persönlich";
            a.Predecessor = -1;
            a.Successor = -1;
            a.Comment = "Bittö";
            AnimeService animeService = new AnimeService(new DatabaseManager($"Data Source=db/data.sqlite"));
            animeService.InsertAnime(a);
        }

        protected override void OnExit(ExitEventArgs e) {
            Log.Information("Application shutdown.");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
