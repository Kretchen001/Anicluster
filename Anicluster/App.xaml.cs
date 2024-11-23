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
                }
            }
            catch (Exception ex) {
                Log.Error(ex.StackTrace ?? "Error without stacktrace...");
                MessageBox.Show("Fehler beim Start der Anwendung!");
                base.Shutdown();
                return;
            }
            Log.Information("Connection to SQLite-DB available");

            TestInsert();

            base.OnStartup(e);
        }

        private void TestInsert() {
            Anime a = new Anime {
                Id = 1,
                Name = "test",
                OriginalName = "testo",
                Url = "",
                Favorite = true,
                Rating = new Rating() {
                    Story = 85,
                    Animation = 10,
                    SpecialEffects = 90,
                    Acoustic = new Acoustic() {
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
                },
                Tier = Tier.S,
                Seasons = [new Season() {
                    Episodes = [
                        new VideoAnimation(videoType: VideoType.Episode, comment: "demo"),
                        new VideoAnimation(videoType: VideoType.Episode, comment: "2"),
                        new VideoAnimation(videoType: VideoType.Episode, comment: "54"),
                        new VideoAnimation(videoType: VideoType.Episode, comment: "lorem"),
                        new VideoAnimation(videoType: VideoType.Episode, comment: "as"),
                        new VideoAnimation(videoType: VideoType.Episode, comment: "dao"),
                        new VideoAnimation(videoType: VideoType.Episode, comment: "de"),
                        new VideoAnimation(videoType: VideoType.Episode, comment: "mo"),
                        new VideoAnimation(videoType: VideoType.Episode, comment: "o"),
                        new VideoAnimation(videoType: VideoType.Episode, comment: "64543"),
                        new VideoAnimation(videoType: VideoType.Episode, comment: "aaegew"),
                        new VideoAnimation(videoType: VideoType.Episode, comment: "2t4 bw"),
                        new VideoAnimation(videoType: VideoType.Episode, comment: ""),
                        new VideoAnimation(videoType: VideoType.Episode, comment: "q23 b"),
                        new VideoAnimation(videoType: VideoType.Episode, comment: "kkopn45"),
                        new VideoAnimation(videoType: VideoType.Episode, comment: "564415654156h34135456n413654n4156"),
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
                }],
                Ovas = [
                    new VideoAnimation(videoType: VideoType.Ova, comment: "demo"),
                    new VideoAnimation(videoType: VideoType.Movie, comment: "demo"),
                    new VideoAnimation(videoType: VideoType.Ova, comment: "demo")
                ],
                Status = new Status() {
                    State = State.Started,
                    Comment = "Danke Silberbaron"
                },
                Tags = [
                    //new Tag("a"),
                    //new Tag("b"),
                ],
                MediaInfo = new MediaInfo() {
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
                },
                RecommendedFrom = "Silberbaron höchst persönlich",
                Predecessor = -1,
                Successor = -1,
                Comment = "Bittö"
            };
            AnimeService animeService = new AnimeService(new DatabaseManager($"Data Source=db/data.sqlite"));
            animeService.Insert(a);
        }

        protected override void OnExit(ExitEventArgs e) {
            Log.Information("Application shutdown.");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
