using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services.AssociativeEntities {

    public class AcousticMusicPieceAssociativeEntityService {

        private readonly DatabaseManager _databaseManager;

        public AcousticMusicPieceAssociativeEntityService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeAcousticMusicPieceAssociativeEntitiesTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS AcousticMusicPieceAssociativeEntities (" +
                    "    AcousticId INTEGER," +
                    "    MusicPieceId INTEGER," +
                    "    FOREIGN KEY(AcousticId) REFERENCES Acoustic(Id)," +
                    "    FOREIGN KEY(MusicPieceId) REFERENCES MusicPiece(Id)," +
                    "    PRIMARY KEY(AcousticId, MusicPieceId)" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- AcousticMusicPieceAssociativeEntities table not created!");
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
                    cmd.Parameters.AddWithValue("@tableName", "AcousticMusicPieceAssociativeEntities");
                    object? result = cmd.ExecuteScalar();

                    connection.Close();
                    return result != null;
                }
            }
        }

        public bool InsertAcousticMusicPieceAssociativeEntity(int acousticId, int musicPieceId) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction()) {
                    try {
                        string insertAcousticMusicPieceAssociativeEntities = "" +
                            "INSERT INTO AcousticMusicPieceAssociativeEntities (AcousticId, MusicPieceId) " +
                            "    VALUES (@acousticId, @musicPieceId)";
                        using (SqliteCommand cmd = new SqliteCommand(insertAcousticMusicPieceAssociativeEntities, connection)) {
                            cmd.Parameters.AddWithValue("@acousticId", acousticId);
                            cmd.Parameters.AddWithValue("@musicPieceId", musicPieceId);
                            cmd.ExecuteScalar();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex) {
                        transaction.Rollback();
                        Log.Error(ex.StackTrace ?? "Error while inserting AcousticMusicPieceAssociativeEntity data");
                        return false;
                    }
                }
                connection.Close();
            }
            return true;
        }
    }
}
