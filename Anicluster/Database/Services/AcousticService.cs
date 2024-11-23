using Anicluster.Database.Services.AssociativeEntities;
using Microsoft.Data.Sqlite;
using Modells.Anime.SoundRating;
using Serilog;

namespace Anicluster.Database.Services {

    public class AcousticService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public AcousticService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = @"
                    CREATE TABLE IF NOT EXISTS Acoustic (
                        Id INTEGER PRIMARY KEY,
                        Soundtrack INTEGER,
                        Comment TEXT
                    );
                ";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Acoustic table not created!");
                        return false;
                    }
                }
            }
            Log.Information("Acoustic table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "Acoustic");
                    object? result = cmd.ExecuteScalar();
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'Acoustic' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'Acoustic' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public int Insert(Acoustic acousticToInsert) {
            int acousticId = 0;
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                try {
                    string insertAcoustic = "" +
                        "INSERT INTO Acoustic (Soundtrack, Comment)\n" +
                        "    VALUES (@soundtrack, @comment)";
                    connection.Open();
                    using (SqliteCommand cmd = new SqliteCommand(insertAcoustic, connection)) {
                        cmd.Parameters.AddWithValue("@soundtrack", acousticToInsert.Soundtrack);
                        cmd.Parameters.AddWithValue("@comment", acousticToInsert.Comment);
                        cmd.ExecuteScalar();
                    }
                    using (SqliteCommand cmd = new SqliteCommand("SELECT last_insert_rowid();", connection)) {
                        object? result = cmd.ExecuteScalar();
                        acousticId = (int)((long)result!);
                    }
                    connection.Close();
                }
                catch (Exception ex) {
                    Log.Error(ex.StackTrace ?? "Error while inserting acousticToInsert data");
                    return -1;
                }
            }
            // first insert and then map the opening and ending
            List<int> oList = new List<int>(acousticToInsert.Opening.Count);
            List<int> eList = new List<int>(acousticToInsert.Ending.Count);
            MusicPieceService mpService = new MusicPieceService(_databaseManager);
            foreach (MusicPiece x in acousticToInsert.Opening) {
                oList.Add(mpService.Insert(x));
            }
            foreach (MusicPiece x in acousticToInsert.Ending) {
                eList.Add(mpService.Insert(x));
            }
            AcousticMusicPieceAssociativeEntityService ampaes = new AcousticMusicPieceAssociativeEntityService(_databaseManager);
            foreach (int x in oList) {
                ampaes.Insert(acousticId, x);
            }
            foreach (int x in eList) {
                ampaes.Insert(acousticId, x);
            }
            // insert and map the syncros
            List<int> sList = new List<int>(acousticToInsert.Syncro.Count);
            SyncroService syncroService = new SyncroService(_databaseManager);
            foreach (Syncro x in acousticToInsert.Syncro) {
                sList.Add(syncroService.Insert(x));
            }
            AcousticSyncroAssociativeEntityService asaes = new AcousticSyncroAssociativeEntityService(_databaseManager);
            foreach (int x in sList) {
                asaes.Insert(acousticId, x);
            }

            return acousticId;
        }
    }
}
