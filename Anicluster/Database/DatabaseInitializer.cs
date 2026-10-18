using Anicluster.Database.Services;
using Anicluster.Database.Services.AssociativeEntities;
using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database {

    /// <summary>
    /// Prüft beim Start die Datenbankverbindung und legt fehlende Tabellen und Views an.
    /// </summary>
    public static class DatabaseInitializer {

        public static void CheckAndInitialize(DatabaseManager dbManager) {
            using (SqliteConnection connection = dbManager.GetConnection()) {
                connection.Open();
                connection.Close();

                SysService sysService = new SysService(dbManager);
                if (sysService.TableExist() == false) { sysService.InitializeTable(); }

                #region Create Tables if not exist
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
                #endregion

                sysService.UpdateTables();

                if (animeService.CreateViewAnimeComplete()) {
                    Log.Information("vw_Anime created");
                }
            }
        }
    }
}
