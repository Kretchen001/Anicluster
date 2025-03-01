using Microsoft.Data.Sqlite;
using Serilog;
using System.Data;

namespace Anicluster.Database.Services.AssociativeEntities {

    public class AcousticSyncroAssociativeEntityService : IDatabaseService {
        
        private readonly DatabaseManager _databaseManager;

        public AcousticSyncroAssociativeEntityService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS AcousticSyncroAssociativeEntity (" +
                    "    AcousticId INTEGER," +
                    "    SyncroId INTEGER," +
                    "    FOREIGN KEY(AcousticId) REFERENCES Acoustic(Id)," +
                    "    FOREIGN KEY(SyncroId) REFERENCES Syncro(Id)," +
                    "    PRIMARY KEY(AcousticId, SyncroId)" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- AcousticSyncroAssociativeEntity table not created!");
                        return false;
                    }
                }
            }
            Log.Information("AcousticSyncroAssociativeEntity table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "AcousticSyncroAssociativeEntity");
                    object? result = cmd.ExecuteScalar();
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'AcousticSyncroAssociativeEntity' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'AcousticSyncroAssociativeEntity' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public bool Insert(int acousticId, int syncroId) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                    try {
                        string insertAcousticSyncroAssociativeEntity = "" +
                            "INSERT INTO AcousticSyncroAssociativeEntity (AcousticId, SyncroId) " +
                            "    VALUES (@acousticId, @syncroId)";
                        using (SqliteCommand cmd = new SqliteCommand(insertAcousticSyncroAssociativeEntity, connection)) {
                            cmd.Parameters.AddWithValue("@acousticId", acousticId);
                            cmd.Parameters.AddWithValue("@syncroId", syncroId);
                            cmd.ExecuteScalar();
                        }
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error while inserting Acoustic-Syncro data");
                        return false;
                    }
                connection.Close();
            }
            return true;
        }

        public List<int> GetSyncroIdsById(int id = -1) {
            string query = $"" +
                $"SELECT SyncroId\n" +
                $"FROM AcousticMusicPieceAssociativeEntities\n" +
                $"WHERE\n" +
                $"    AcousticId={id}";

            DataTable resDt = new DataTable();
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    resDt.Load(result);
                    connection.Close();
                }
            }
            return resDt.AsEnumerable().Select(row => Convert.ToInt32(row.Field<long>("SyncroId"))).ToList();
        }
    }
}
