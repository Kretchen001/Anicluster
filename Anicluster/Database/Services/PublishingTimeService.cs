using Anicluster.Database.Services.AssociativeEntities;
using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;

namespace Anicluster.Database.Services {

    public class PublishingTimeService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public PublishingTimeService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS PublishingTime (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    StartDate DATETIME," +
                    "    EndDate DATETIME" +
                    ");";
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
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "PublishingTime");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }

        /// <summary></summary>
        /// <param name="publishingTime"></param>
        /// <returns>The (new, correct) id of the inserted PublishingTime</returns>
        public int Insert(PublishingTime publishingTime) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string queryInsert = "" +
                    "INSERT INTO PublishingTime (StartDate, EndDate) \n" +
                    "VALUES (@StartDate, @EndDate);";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        cmd.Parameters.AddWithValue("@StartDate", publishingTime.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", publishingTime.EndDate);
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "SELECT last_insert_rowid();";
                        object? insertedId = cmd.ExecuteScalar();
                        connection.Close();
                        int insertedIdInt = (int)((long)insertedId!);
                        List<int> insertedInterruptionIds = [];
                        InterruptionService interruptionService = new InterruptionService(_databaseManager);
                        foreach (Interruption x in publishingTime.Interruptions) {
                            insertedInterruptionIds.Add(interruptionService.Insert(x));
                        }
                        PublishingTimeInterruptionAssociativeEntityService publishingTimeInterruptionAssociativeEntityService = new PublishingTimeInterruptionAssociativeEntityService(_databaseManager);
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
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
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
                                StartDate = (DateTime)reader["StartDate"],
                                EndDate = (DateTime)reader["EndDate"],
                            };
                            reader.Close();
                        }
                        connection.Close();
                        // Request Interruption(s)
                        PublishingTimeInterruptionAssociativeEntityService publishingTimeInterruptionAssociativeEntityService = new PublishingTimeInterruptionAssociativeEntityService(_databaseManager);
                        List<int> piIds = publishingTimeInterruptionAssociativeEntityService.SelectInterruptionsFromPublishing(id);
                        InterruptionService interruptionService = new InterruptionService(_databaseManager);
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

        ///// <summary></summary>
        ///// <param name="tagToDelete"></param>
        ///// <returns></returns>
        //public bool DeleteTagById(Tag tagToDelete) {
        //    using (SqliteConnection connection = _databaseManager.GetConnection()) {
        //        string queryInsert = "" +
        //            "DELETE FROM Tag\n" +
        //            $"WHERE Id LIKE {tagToDelete.Id}";
        //        using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
        //            try {
        //                connection.Open();
        //                cmd.ExecuteNonQuery();
        //                connection.Close();
        //                return true;
        //            }
        //            catch (Exception ex) {
        //                Log.Error(ex.StackTrace ?? "Error without stacktrace... <- DeleteTagById!");
        //                return false;
        //            }
        //        }
        //    }
        //}
    }
}
