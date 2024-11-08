using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;
using System.Data;
using System.Security.Cryptography.Pkcs;

namespace Anicluster.Database.Services {

    public class TagService {

        private readonly DatabaseManager _databaseManager;

        public TagService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTagTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS Tag (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    Designation TEXT NOT NULL" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Tag table not created!");
                        return false;
                    }
                }
            }
            Log.Information("Tag table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    connection.Open();
                    cmd.Parameters.AddWithValue("@tableName", "Tag");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }

        /// <summary>
        /// Insert the Tag-Designation. The id from param can not be correct after this!
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>The (new, correct) id of the inserted Anime</returns>
        public int Insert(Tag tag) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string queryInsert = "" +
                    "INSERT INTO Tag (Designation) \n" +
                    "VALUES (@Designation);";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        cmd.Parameters.AddWithValue("@Designation", tag.Designation);
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = $"SELECT Id FROM Tag WHERE Designation Like '{tag.Designation}'";
                        object? insertedId = cmd.ExecuteScalar();
                        connection.Close();
                        return insertedId is not null ? (int)insertedId : -1;
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Tag not inserted or queryable!");
                        return -1;
                    }
                }
            }
        }

        public List<int> InsertTagList(List<Tag> tagList) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string queryInsert = "" +
                    "INSERT INTO Tag (Designation) \n" +
                    "VALUES ";
                for (int i=0; i < tagList.Count; i += 1) {
                    queryInsert += $"(@Designation{i})";
                    queryInsert += i != tagList.Count - 1 ? ", \n" : ";";
                }
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        for (int i = 0; i < tagList.Count; i += 1) {
                            cmd.Parameters.AddWithValue($"@Designation{i}", tagList[i].Designation);
                        }
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        // Request the id's
                        // DemoString; TODO: Zusammensetzen
                        // SELECT Designation FROM Tag WHERE Designation LIKE @designation_Design1 OR Designation LIKE @designation_Design2 OR Designation LIKE @designation_Design3;
                        string querySelectIds = "";
                        cmd.CommandText = querySelectIds;
                        List<int> ids = [];
                        using (SqliteDataReader reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                ids.Add((int)reader["Designation"]);
                            }
                        }
                        connection.Close();
                        return ids;
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Tags not inserted or queryable!");
                        return [];
                    }
                }
            }
        }
    }
}
