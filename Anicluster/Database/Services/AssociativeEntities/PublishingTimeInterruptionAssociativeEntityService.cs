using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services.AssociativeEntities {

    public class PublishingTimeInterruptionAssociativeEntityService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public PublishingTimeInterruptionAssociativeEntityService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS PublishingTimeInterruptionAssociativeEntity (\n" +
                    "    PublishingId INTEGER,\n" +
                    "    InterruptionId INTEGER,\n" +
                    "    FOREIGN KEY(PublishingId) REFERENCES Publishing(Id),\n" +
                    "    FOREIGN KEY(InterruptionId) REFERENCES Interruption(Id),\n" +
                    "    PRIMARY KEY(PublishingId, InterruptionId)\n" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- PublishingTimeInterruptionAssociativeEntity table not created!");
                        return false;
                    }
                }
            }
            Log.Information("PublishingTimeInterruptionAssociativeEntity table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "PublishingTimeInterruptionAssociativeEntity");
                    object? result = cmd.ExecuteScalar();

                    connection.Close();
                    return result != null;
                }
            }
        }

        public bool InsertPublishingInterruption(int interId, int pubId) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                try {
                    string query = "" +
                        "INSERT INTO PublishingTimeInterruptionAssociativeEntity (PublishingId, InterruptionId)\n " +
                        "    VALUES (@PublishingId, @InterruptionId);";
                    connection.Open();
                    using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                        cmd.Parameters.AddWithValue("@InterruptionId", interId);
                        cmd.Parameters.AddWithValue("@PublishingId", pubId);
                        cmd.ExecuteNonQuery();
                    }
                    connection.Close();
                    return true;
                }
                catch (Exception ex) {
                    Log.Error(ex.StackTrace ?? "Error while inserting Interruption-Publishing data");
                    return false;
                }
            }
        }

        public List<int> SelectInterruptionsFromPublishing(int pubId) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                try {
                    string query = "" +
                        "SELECT InterruptionId\n" +
                        "FROM PublishingTimeInterruptionAssociativeEntity\n" +
                        $"WHERE PublishingId LIKE '{pubId}'";
                    connection.Open();
                    using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                        object scalar = cmd.ExecuteScalar() ?? new object();
                        connection.Close();
                        List<int> res = [];
                        using (SqliteDataReader reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                res.Add((int)((long)reader["InterruptionId"]));
                            }
                            reader.Close();
                        }
                        return res;
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex.StackTrace ?? "Error while inserting Interruption-Publishing data");
                    return [];
                }
            }
        }
    }
}
