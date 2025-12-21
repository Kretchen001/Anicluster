using Anicluster.Database.Services.AssociativeEntities;
using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;

namespace Anicluster.Database.Services {

    public class PublishingTimeService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public PublishingTimeService(DatabaseManager databaseManager) {
            this._databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                string creationString = @"
                    CREATE TABLE IF NOT EXISTS PublishingTime (
                        Id INTEGER PRIMARY KEY,
                        StartDate DATETIME,
                        EndDate DATETIME
                    );
                ";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- PublishingTime table not created!");
                        return false;
                    }
                }
            }
            Log.Information("PublishingTime table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "PublishingTime");
                    object? result = cmd.ExecuteScalar();
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'PublishingTime' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'PublishingTime' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        /// <summary></summary>
        /// <param name="publishingTime"></param>
        /// <returns>The (new, correct) id of the inserted PublishingTime</returns>
        public int Insert(PublishingTime publishingTime) {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                string queryInsert = "" +
                    "INSERT INTO PublishingTime (StartDate, EndDate) \n" +
                    "    VALUES (@StartDate, @EndDate);";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        cmd.Parameters.AddWithValue("@StartDate", publishingTime.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", publishingTime.EndDate);
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT last_insert_rowid();";
                        object? insertedId = cmd.ExecuteScalar();
                        connection.Close();
                        int insertedIdInt = (int)((long)insertedId!);
                        publishingTime.Id = insertedIdInt;
                        List<int> insertedInterruptionIds = [];
                        InterruptionService interruptionService = new InterruptionService(this._databaseManager);
                        foreach (Interruption x in publishingTime.Interruptions) {
                            insertedInterruptionIds.Add(interruptionService.Insert(x));
                        }
                        PublishingTimeInterruptionAssociativeEntityService publishingTimeInterruptionAssociativeEntityService = new PublishingTimeInterruptionAssociativeEntityService(this._databaseManager);
                        foreach (int x in insertedInterruptionIds) {
                            publishingTimeInterruptionAssociativeEntityService.InsertPublishingInterruption(insertedIdInt, x);
                        }
                        return insertedIdInt;
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- PublishingTime not inserted or queryable!");
                        return -1;
                    }
                }
            }
        }

        /// <summary></summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PublishingTime SelectById(int id) {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                string queryInsert = "" +
                    "SELECT *\n" +
                    "FROM PublishingTime\n" +
                    $"WHERE Id LIKE {id};";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        PublishingTime publishingTime = new PublishingTime();
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        using (SqliteDataReader reader = cmd.ExecuteReader()) {
                            publishingTime = new PublishingTime() {
                                Id = (int)((long)reader["Id"]),
                                StartDate = (DateTime)reader["StartDate"],
                                EndDate = (DateTime)reader["EndDate"],
                            };
                            reader.Close();
                        }
                        connection.Close();
                        // Request Interruption(s)
                        PublishingTimeInterruptionAssociativeEntityService publishingTimeInterruptionAssociativeEntityService = new PublishingTimeInterruptionAssociativeEntityService(this._databaseManager);
                        List<int> piIds = publishingTimeInterruptionAssociativeEntityService.SelectInterruptionsFromPublishing(id);
                        InterruptionService interruptionService = new InterruptionService(this._databaseManager);
                        publishingTime.Interruptions.AddRange(interruptionService.SelectInterruptionsById(piIds));

                        return publishingTime;
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Tags Selection (all)!");
                        return new PublishingTime();
                    }
                }
            }
        }

        public bool UpdatePublishingTime(PublishingTime publishingTimeToUpdate) {
            // update Interruptions
            if (!(new InterruptionService(this._databaseManager).UpdateInterruptions(publishingTimeToUpdate.Id ,publishingTimeToUpdate.Interruptions))) {
                return false;
            }

            // update Publishing Time
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                string queryInsert = @"
                    UPDATE PublishingTime 
                    SET 
                        StartDate = @StartDate
                        EndDate = @EndDate
                    WHERE Id = @Id; 
                ";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        PublishingTime publishingTime = new PublishingTime();
                        connection.Open();
                        cmd.Parameters.AddWithValue("@Id", publishingTimeToUpdate.Id);
                        cmd.Parameters.AddWithValue("@StartDate", publishingTimeToUpdate.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", publishingTimeToUpdate.EndDate);
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Tags Selection (all)!");
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
