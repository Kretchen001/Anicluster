using Microsoft.Data.Sqlite;
using Serilog;
using System.Data;

namespace Anicluster.Database.Services.AssociativeEntities {

    public class AcousticMusicPieceAssociativeEntityService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public AcousticMusicPieceAssociativeEntityService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = @"
                    CREATE TABLE IF NOT EXISTS AcousticMusicPieceAssociativeEntities (
                        AcousticId INTEGER,
                        MusicPieceId INTEGER,
                        FOREIGN KEY(AcousticId) REFERENCES Acoustic(Id),
                        FOREIGN KEY(MusicPieceId) REFERENCES MusicPiece(Id),
                        PRIMARY KEY(AcousticId, MusicPieceId)
                    );
                ";
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
            Log.Information("AcousticMusicPieceAssociativeEntities table created.");
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
                    if (result is not null) {
                        Log.Information("Tabelle 'AcousticMusicPieceAssociativeEntities' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'AcousticMusicPieceAssociativeEntities' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public bool Insert(int acousticId, int musicPieceId) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                try {
                    string insertAcousticMusicPieceAssociativeEntities = "" +
                        "INSERT INTO AcousticMusicPieceAssociativeEntities (AcousticId, MusicPieceId) " +
                        "    VALUES (@acousticId, @musicPieceId)";
                    using (SqliteCommand cmd = new SqliteCommand(insertAcousticMusicPieceAssociativeEntities, connection)) {
                        cmd.Parameters.AddWithValue("@acousticId", acousticId);
                        cmd.Parameters.AddWithValue("@musicPieceId", musicPieceId);
                        cmd.ExecuteScalar();
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex.StackTrace ?? "Error while inserting AcousticMusicPieceAssociativeEntity data");
                    return false;
                }
                connection.Close();
            }
            return true;
        }

        public List<int> GetMusicPieceIdsById(int id = -1) {
            string query = $"" +
                $"SELECT MusicPieceId\n" +
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
            return resDt.AsEnumerable().Select(row => Convert.ToInt32(row.Field<long>("MusicPieceId"))).ToList();
        }
    }
}
