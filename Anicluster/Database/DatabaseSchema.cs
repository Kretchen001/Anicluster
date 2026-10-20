using Microsoft.Data.Sqlite;
using Serilog;
using System.IO;

namespace Anicluster.Database {

    /// <summary>
    /// Verwaltet die Struktur der Datenbank: legt sie bei Bedarf an und überführt ältere Stände in die aktuelle Version.
    /// Die Version steht in <c>PRAGMA user_version</c> (0 = neue oder noch nicht versionierte Datenbank).
    /// </summary>
    public static class DatabaseSchema {

        /// <summary>Aktuelle Version der Datenbankstruktur</summary>
        public const int CurrentVersion = 2;

        /// <summary>Struktur nach der Umstellung auf Fremdschlüssel, noch ohne Anime-Bilder</summary>
        private const int VersionWithoutImages = 1;

        /// <summary>
        /// Stellt sicher, dass die Datenbank die aktuelle Struktur hat.
        /// Eine ältere Datenbank wird gesichert (Kopie neben der Datenbankdatei) und migriert, Version 0 in eine neue Datei,
        /// Version 1 direkt in der vorhandenen Datei. Schlägt die Migration fehl, wird der alte Stand wiederhergestellt
        /// und die Exception weitergereicht.
        /// </summary>
        public static void EnsureCurrent(DatabaseManager dbManager) {
            int version;
            bool hasAnimeTable;
            using (SqliteConnection connection = dbManager.GetConnection()) {
                connection.Open();
                version = Convert.ToInt32(ExecuteScalar(connection, "PRAGMA user_version;"));
                hasAnimeTable = ExecuteScalar(connection, "SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'Anime';") is not null;
            }
            if (version > CurrentVersion) {
                throw new InvalidOperationException($"Die Datenbank hat die Version {version}, diese Anwendung kennt nur bis Version {CurrentVersion}.");
            }
            if (version == CurrentVersion) {
                return;
            }
            if (version == 0 && !hasAnimeTable) {
                Log.Information("Neue Datenbank wird mit Version {Version} angelegt.", CurrentVersion);
                CreateNewDatabase(dbManager);
                return;
            }
            // Die Migrationen bauen aufeinander auf: Version 0 -> 1 -> 2
            if (version == 0) {
                Log.Information("Ältere Datenbank gefunden, Migration auf Version {Version} startet.", VersionWithoutImages);
                MigrateFromVersion0(dbManager);
                version = VersionWithoutImages;
            }
            if (version == VersionWithoutImages) {
                Log.Information("Migration auf Version {Version} startet (Anime-Bilder).", CurrentVersion);
                MigrateFromVersion1(dbManager);
            }
        }

        private static void CreateNewDatabase(DatabaseManager dbManager) {
            using (SqliteConnection connection = dbManager.GetConnection()) {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction()) {
                    Execute(connection, transaction, SchemaV1);
                    Execute(connection, transaction, SchemaV2);
                    transaction.Commit();
                }
                Execute(connection, null, $"PRAGMA user_version = {CurrentVersion};");
            }
        }

        /// <summary>
        /// Migration von Version 1 auf Version 2: ergänzt die Tabelle für die Anime-Bilder. Vorhandene Daten bleiben unverändert.
        /// Vorher wird die Datenbankdatei gesichert, bei einem Fehler wird diese Sicherung zurückgespielt.
        /// </summary>
        private static void MigrateFromVersion1(DatabaseManager dbManager) {
            string databasePath = dbManager.DatabasePath;
            string backupPath = Path.Combine(
                Path.GetDirectoryName(databasePath)!,
                $"{Path.GetFileNameWithoutExtension(databasePath)}.v1-{DateTime.Now:yyyyMMdd-HHmmss}{Path.GetExtension(databasePath)}"
            );
            SqliteConnection.ClearAllPools(); // gepoolte Verbindungen halten die Datei sonst gesperrt
            File.Copy(databasePath, backupPath);
            Log.Information("Sicherung der Datenbank vor der Migration: {BackupPath}", backupPath);
            try {
                using (SqliteConnection connection = dbManager.GetConnection()) {
                    connection.Open();
                    using (SqliteTransaction transaction = connection.BeginTransaction()) {
                        Execute(connection, transaction, SchemaV2);
                        transaction.Commit();
                    }
                    Execute(connection, null, $"PRAGMA user_version = {CurrentVersion};");
                }
                SqliteConnection.ClearAllPools();
                Log.Information("Migration auf Version {Version} abgeschlossen.", CurrentVersion);
            }
            catch (Exception ex) {
                Log.Error(ex, "Migration fehlgeschlagen, der alte Stand wird wiederhergestellt.");
                SqliteConnection.ClearAllPools();
                File.Copy(backupPath, databasePath, true);
                throw;
            }
        }

        /// <summary>
        /// Migration der alten Struktur (jede Beziehung über eigene Zuordnungstabellen) in Version 1.
        /// Die alte Datei wird gesichert, die neue Datei entsteht daneben und übernimmt die Daten per ATTACH.
        /// </summary>
        private static void MigrateFromVersion0(DatabaseManager dbManager) {
            string databasePath = dbManager.DatabasePath;
            string backupPath = Path.Combine(
                Path.GetDirectoryName(databasePath)!,
                $"{Path.GetFileNameWithoutExtension(databasePath)}.legacy-{DateTime.Now:yyyyMMdd-HHmmss}{Path.GetExtension(databasePath)}"
            );
            SqliteConnection.ClearAllPools(); // gepoolte Verbindungen halten die Datei sonst gesperrt
            File.Move(databasePath, backupPath);
            Log.Information("Sicherung der alten Datenbank: {BackupPath}", backupPath);
            try {
                using (SqliteConnection connection = dbManager.GetConnection()) {
                    connection.Open();
                    // ATTACH ist innerhalb einer Transaktion nicht erlaubt, die Fremdschlüssel werden nach dem Kopieren geprüft
                    Execute(connection, null, "PRAGMA foreign_keys = OFF;");
                    using (SqliteCommand attach = connection.CreateCommand()) {
                        attach.CommandText = "ATTACH DATABASE @path AS old;";
                        attach.Parameters.AddWithValue("@path", backupPath);
                        attach.ExecuteNonQuery();
                    }
                    using (SqliteTransaction transaction = connection.BeginTransaction()) {
                        Execute(connection, transaction, SchemaV1);
                        Execute(connection, transaction, MigrationV0ToV1);
                        transaction.Commit();
                    }
                    int violations = 0;
                    using (SqliteCommand check = connection.CreateCommand()) {
                        check.CommandText = "PRAGMA main.foreign_key_check;";
                        using (SqliteDataReader reader = check.ExecuteReader()) {
                            while (reader.Read()) {
                                violations += 1;
                            }
                        }
                    }
                    if (violations > 0) {
                        throw new InvalidOperationException($"Die Migration hat {violations} ungültige Verweise erzeugt.");
                    }
                    Execute(connection, null, "DETACH DATABASE old;");
                    Execute(connection, null, $"PRAGMA user_version = {VersionWithoutImages};");
                }
                SqliteConnection.ClearAllPools();
                Log.Information("Migration auf Version {Version} abgeschlossen.", VersionWithoutImages);
            }
            catch (Exception ex) {
                Log.Error(ex, "Migration fehlgeschlagen, der alte Stand wird wiederhergestellt.");
                SqliteConnection.ClearAllPools();
                if (File.Exists(databasePath)) {
                    File.Delete(databasePath);
                }
                File.Move(backupPath, databasePath);
                throw;
            }
        }

        private static object? ExecuteScalar(SqliteConnection connection, string sql) {
            using (SqliteCommand cmd = connection.CreateCommand()) {
                cmd.CommandText = sql;
                return cmd.ExecuteScalar();
            }
        }

        private static void Execute(SqliteConnection connection, SqliteTransaction? transaction, string sql) {
            using (SqliteCommand cmd = connection.CreateCommand()) {
                cmd.Transaction = transaction;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Struktur Version 1. Beziehungen 1:n und 1:1 laufen über Fremdschlüssel mit ON DELETE CASCADE:
        /// Wird ein Anime gelöscht, verschwinden automatisch alle seine Daten (Status, Bewertung, Staffeln, ...).
        /// Nur Anime und Tag sind über eine Zuordnungstabelle verbunden.
        /// </summary>
        private const string SchemaV1 = @"
CREATE TABLE Tag (
    Id INTEGER PRIMARY KEY,
    Designation TEXT NOT NULL
);

CREATE TABLE Anime (
    Id INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    OriginalName TEXT,
    Url TEXT,
    Favorite BOOLEAN NOT NULL DEFAULT 0,
    Tier INTEGER NOT NULL,
    RecommendedFrom TEXT,
    Predecessor INTEGER NOT NULL DEFAULT -1,
    Successor INTEGER NOT NULL DEFAULT -1,
    Related INTEGER NOT NULL DEFAULT -1,
    Comment TEXT
);

CREATE TABLE AnimeTagAssociativeEntity (
    AnimeId INTEGER NOT NULL REFERENCES Anime(Id) ON DELETE CASCADE,
    TagId INTEGER NOT NULL REFERENCES Tag(Id) ON DELETE CASCADE,
    PRIMARY KEY (AnimeId, TagId)
);
CREATE INDEX IX_AnimeTag_TagId ON AnimeTagAssociativeEntity (TagId);

CREATE TABLE Status (
    Id INTEGER PRIMARY KEY,
    AnimeId INTEGER NOT NULL UNIQUE REFERENCES Anime(Id) ON DELETE CASCADE,
    State INTEGER NOT NULL,
    Comment TEXT
);

CREATE TABLE Rating (
    Id INTEGER PRIMARY KEY,
    AnimeId INTEGER NOT NULL UNIQUE REFERENCES Anime(Id) ON DELETE CASCADE,
    Story INTEGER NOT NULL DEFAULT 0,
    Animation INTEGER NOT NULL DEFAULT 0,
    SpecialEffects INTEGER NOT NULL DEFAULT 0,
    IsRated BOOLEAN NOT NULL DEFAULT 0
);

CREATE TABLE Acoustic (
    Id INTEGER PRIMARY KEY,
    RatingId INTEGER NOT NULL UNIQUE REFERENCES Rating(Id) ON DELETE CASCADE,
    Soundtrack INTEGER NOT NULL DEFAULT -1,
    Comment TEXT
);

CREATE TABLE MusicPiece (
    Id INTEGER PRIMARY KEY,
    AcousticId INTEGER NOT NULL REFERENCES Acoustic(Id) ON DELETE CASCADE,
    Type INTEGER NOT NULL,
    Name TEXT,
    Comment TEXT,
    General INTEGER NOT NULL DEFAULT -1
);
CREATE INDEX IX_MusicPiece_AcousticId ON MusicPiece (AcousticId);

CREATE TABLE Syncro (
    Id INTEGER PRIMARY KEY,
    AcousticId INTEGER NOT NULL REFERENCES Acoustic(Id) ON DELETE CASCADE,
    General INTEGER NOT NULL DEFAULT -1,
    Language INTEGER NOT NULL,
    IsAssessed BOOLEAN NOT NULL DEFAULT 0,
    Comment TEXT
);
CREATE INDEX IX_Syncro_AcousticId ON Syncro (AcousticId);

CREATE TABLE MediaInfo (
    Id INTEGER PRIMARY KEY,
    AnimeId INTEGER NOT NULL UNIQUE REFERENCES Anime(Id) ON DELETE CASCADE,
    Author TEXT,
    Producer TEXT,
    Publisher TEXT
);

CREATE TABLE Season (
    Id INTEGER PRIMARY KEY,
    AnimeId INTEGER NOT NULL REFERENCES Anime(Id) ON DELETE CASCADE,
    Number INTEGER NOT NULL,
    Comment TEXT
);
CREATE INDEX IX_Season_AnimeId ON Season (AnimeId);

CREATE TABLE PublishingTime (
    Id INTEGER PRIMARY KEY,
    MediaInfoId INTEGER UNIQUE REFERENCES MediaInfo(Id) ON DELETE CASCADE,
    SeasonId INTEGER UNIQUE REFERENCES Season(Id) ON DELETE CASCADE,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    CHECK ((MediaInfoId IS NULL) <> (SeasonId IS NULL))
);

CREATE TABLE Interruption (
    Id INTEGER PRIMARY KEY,
    PublishingTimeId INTEGER NOT NULL REFERENCES PublishingTime(Id) ON DELETE CASCADE,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    Comment TEXT
);
CREATE INDEX IX_Interruption_PublishingTimeId ON Interruption (PublishingTimeId);

CREATE TABLE VideoAnimation (
    Id INTEGER PRIMARY KEY,
    AnimeId INTEGER REFERENCES Anime(Id) ON DELETE CASCADE,
    SeasonId INTEGER REFERENCES Season(Id) ON DELETE CASCADE,
    Position INTEGER NOT NULL DEFAULT 0,
    VideoType INTEGER NOT NULL,
    Comment TEXT,
    CHECK ((AnimeId IS NULL) <> (SeasonId IS NULL))
);
CREATE INDEX IX_VideoAnimation_AnimeId ON VideoAnimation (AnimeId);
CREATE INDEX IX_VideoAnimation_SeasonId ON VideoAnimation (SeasonId);
";

        /// <summary>
        /// Ergänzung in Version 2: ein Bild pro Anime (1:1, wird mit dem Anime gelöscht).
        /// Das Bild liegt in zwei Größen vor, damit die Bilder-Ansicht nur die kleinen Vorschauen laden muss.
        /// </summary>
        private const string SchemaV2 = @"
CREATE TABLE AnimeImage (
    Id INTEGER PRIMARY KEY,
    AnimeId INTEGER NOT NULL UNIQUE REFERENCES Anime(Id) ON DELETE CASCADE,
    Data BLOB NOT NULL,
    Thumbnail BLOB NOT NULL
);
";

        /// <summary>
        /// Kopiert die Daten der alten Struktur (als Schema "old" angehängt) in Version 1.
        /// Erst werden vorhandene Daten mit ihren Ids übernommen, danach fehlende Teile ergänzt, damit keine Ids kollidieren.
        /// </summary>
        private const string MigrationV0ToV1 = @"
-- 1. Vorhandene Daten übernehmen (Ids bleiben erhalten)
INSERT INTO Tag (Id, Designation)
    SELECT Id, Designation FROM old.Tag;

INSERT INTO Anime (Id, Name, OriginalName, Url, Favorite, Tier, RecommendedFrom, Predecessor, Successor, Related, Comment)
    SELECT Id, Name, NULLIF(OriginalName, ''), NULLIF(Url, ''), COALESCE(Favorite, 0), COALESCE(Tier, 6), NULLIF(RecommendedFrom, ''),
           COALESCE(Predecessor, -1), COALESCE(Successor, -1), COALESCE(Related, -1), NULLIF(Comment, '')
    FROM old.Anime;

INSERT OR IGNORE INTO AnimeTagAssociativeEntity (AnimeId, TagId)
    SELECT m.AnimeId, m.TagId
    FROM old.AnimeTagAssociativeEntity m
    WHERE m.AnimeId IN (SELECT Id FROM Anime) AND m.TagId IN (SELECT Id FROM Tag);

INSERT INTO Status (Id, AnimeId, State, Comment)
    SELECT s.Id, a.Id, s.State, NULLIF(s.Comment, '')
    FROM old.Anime a JOIN old.Status s ON s.Id = a.StatusId;

INSERT INTO Rating (Id, AnimeId, Story, Animation, SpecialEffects, IsRated)
    SELECT r.Id, a.Id, COALESCE(r.Story, 0), COALESCE(r.Animation, 0), COALESCE(r.SpecialEffects, 0), COALESCE(r.IsRated, 0)
    FROM old.Anime a JOIN old.Rating r ON r.Id = a.RatingId;

INSERT INTO Acoustic (Id, RatingId, Soundtrack, Comment)
    SELECT ac.Id, r.Id, COALESCE(ac.Soundtrack, -1), NULLIF(ac.Comment, '')
    FROM old.Rating r JOIN old.Acoustic ac ON ac.Id = r.AcousticId
    WHERE r.Id IN (SELECT Id FROM Rating);

INSERT INTO MusicPiece (Id, AcousticId, Type, Name, Comment, General)
    SELECT mp.Id, m.AcousticId, COALESCE(mp.Type, 2), mp.Name, NULLIF(mp.Comment, ''), COALESCE(mp.General, -1)
    FROM old.AcousticMusicPieceAssociativeEntities m JOIN old.MusicPiece mp ON mp.Id = m.MusicPieceId
    WHERE m.AcousticId IN (SELECT Id FROM Acoustic);

INSERT INTO Syncro (Id, AcousticId, General, Language, IsAssessed, Comment)
    SELECT sy.Id, m.AcousticId, COALESCE(sy.General, -1), COALESCE(sy.Language, 1), COALESCE(sy.IsAssessed, 0), NULLIF(sy.Comment, '')
    FROM old.AcousticSyncroAssociativeEntity m JOIN old.Syncro sy ON sy.Id = m.SyncroId
    WHERE m.AcousticId IN (SELECT Id FROM Acoustic);

INSERT INTO MediaInfo (Id, AnimeId, Author, Producer, Publisher)
    SELECT mi.Id, a.Id, NULLIF(mi.Author, ''), NULLIF(mi.Producer, ''), NULLIF(mi.Publisher, '')
    FROM old.Anime a JOIN old.MediaInfo mi ON mi.Id = a.MediaInfoId;

INSERT INTO PublishingTime (Id, MediaInfoId, SeasonId, StartDate, EndDate)
    SELECT pt.Id, mi.Id, NULL, COALESCE(pt.StartDate, '0001-01-01 00:00:00'), COALESCE(pt.EndDate, '0001-01-01 00:00:00')
    FROM old.MediaInfo mi JOIN old.PublishingTime pt ON pt.Id = mi.PublishingTimeId
    WHERE mi.Id IN (SELECT Id FROM MediaInfo);

INSERT INTO Season (Id, AnimeId, Number, Comment)
    SELECT s.Id, m.AnimeId, COALESCE(s.Number, 0), NULLIF(s.Comment, '')
    FROM old.AnimeSeasonAssociativeEntity m JOIN old.Season s ON s.Id = m.SeasonId
    WHERE m.AnimeId IN (SELECT Id FROM Anime);

INSERT INTO PublishingTime (Id, MediaInfoId, SeasonId, StartDate, EndDate)
    SELECT pt.Id, NULL, s.Id, COALESCE(pt.StartDate, '0001-01-01 00:00:00'), COALESCE(pt.EndDate, '0001-01-01 00:00:00')
    FROM old.Season s JOIN old.PublishingTime pt ON pt.Id = s.PublishingTimeId
    WHERE s.Id IN (SELECT Id FROM Season);

INSERT INTO Interruption (Id, PublishingTimeId, StartDate, EndDate, Comment)
    SELECT i.Id, m.PublishingTimeId, i.StartDate, i.EndDate, NULLIF(i.Comment, '')
    FROM old.PublishingTimeInterruptionAssociativeEntity m JOIN old.Interruption i ON i.Id = m.InterruptionId
    WHERE m.PublishingTimeId IN (SELECT Id FROM PublishingTime);

INSERT INTO VideoAnimation (Id, AnimeId, SeasonId, Position, VideoType, Comment)
    SELECT v.Id, NULL, m.SeasonId, ROW_NUMBER() OVER (PARTITION BY m.SeasonId ORDER BY v.Id), COALESCE(v.VideoType, 0), NULLIF(v.Comment, '')
    FROM old.SeasonVideoAnimationAssociativeEntityService m JOIN old.VideoAnimation v ON v.Id = m.VideoAnimationId
    WHERE m.SeasonId IN (SELECT Id FROM Season);

INSERT INTO VideoAnimation (Id, AnimeId, SeasonId, Position, VideoType, Comment)
    SELECT v.Id, m.AnimeId, NULL, ROW_NUMBER() OVER (PARTITION BY m.AnimeId ORDER BY v.Id), COALESCE(v.VideoType, 1), NULLIF(v.Comment, '')
    FROM old.AnimeOvaAssociativeEntityService m JOIN old.VideoAnimation v ON v.Id = m.VideoAnimationId
    WHERE m.AnimeId IN (SELECT Id FROM Anime);

-- 2. Fehlende Teile ergänzen (z. B. bei schnell angelegten Animes), erst jetzt, damit keine Ids kollidieren
INSERT INTO Status (AnimeId, State)
    SELECT Id, 0 FROM Anime WHERE Id NOT IN (SELECT AnimeId FROM Status);

INSERT INTO Rating (AnimeId)
    SELECT Id FROM Anime WHERE Id NOT IN (SELECT AnimeId FROM Rating);

INSERT INTO Acoustic (RatingId)
    SELECT Id FROM Rating WHERE Id NOT IN (SELECT RatingId FROM Acoustic);

INSERT INTO MediaInfo (AnimeId)
    SELECT Id FROM Anime WHERE Id NOT IN (SELECT AnimeId FROM MediaInfo);

INSERT INTO PublishingTime (MediaInfoId, StartDate, EndDate)
    SELECT Id, '0001-01-01 00:00:00', '0001-01-01 00:00:00'
    FROM MediaInfo WHERE Id NOT IN (SELECT MediaInfoId FROM PublishingTime WHERE MediaInfoId IS NOT NULL);

INSERT INTO PublishingTime (SeasonId, StartDate, EndDate)
    SELECT Id, '0001-01-01 00:00:00', '0001-01-01 00:00:00'
    FROM Season WHERE Id NOT IN (SELECT SeasonId FROM PublishingTime WHERE SeasonId IS NOT NULL);
";
    }
}
