using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;
using System.Data;

namespace Anicluster.Database.Services {

    public class TagService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public TagService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = @"
                    CREATE TABLE IF NOT EXISTS Tag (
                        Id INTEGER PRIMARY KEY,
                        Designation TEXT NOT NULL
                    );
                ";
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
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'Tag' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'Tag' existiert NICHT!");
                        return false;
                    }
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
                        cmd.CommandText = $"SELECT Id FROM Tag WHERE Designation LIKE '{tag.Designation}'";
                        object? insertedId = cmd.ExecuteScalar();
                        connection.Close();
                        return insertedId is not null ? (int)((long)insertedId) : -1;
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Tag not inserted or queryable!");
                        return -1;
                    }
                }
            }
        }

        /// <summary>
        /// Insert a List of Tags and return the ids from the Insert.<br></br>
        /// If Tag is already exist, it <b>added twice</b> by this function!
        /// </summary>
        /// <param name="tagList"></param>
        /// <returns></returns>
        public List<int> InsertTagList(List<Tag> tagList) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string queryInsert = "" +
                    "INSERT INTO Tag (Designation) \n" +
                    "VALUES ";
                for (int i = 0; i < tagList.Count; i += 1) {
                    queryInsert += $"(@Designation{i})";
                    queryInsert += i != tagList.Count - 1 ? ", \n" : ";";
                }
                string querySelectIds = "" +
                            "SELECT Id \n" +
                            "FROM Tag \n" +
                            "WHERE Designation LIKE @Designation0";
                for (int i = 1; i < tagList.Count; i += 1) { // start by 1 because of string before
                    querySelectIds += $"\n\tOR Designation LIKE @Designation{i}";
                    if (i == tagList.Count - 1) { querySelectIds += ";"; }
                }
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        for (int i = 0; i < tagList.Count; i += 1) {
                            cmd.Parameters.AddWithValue($"@Designation{i}", tagList[i].Designation);
                        }
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        // Request the id's
                        cmd.CommandText = querySelectIds;
                        // use the same Parameters from insert
                        List<int> ids = [];
                        using (SqliteDataReader reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                ids.Add((int)((long)reader["Id"]));
                            }
                            reader.Close();
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

        /// <summary></summary>
        /// <returns></returns>
        public List<Tag> SelectAllTags() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string queryInsert = "" +
                    "SELECT *\n" +
                    "FROM Tag;";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        List<Tag> tags = [];
                        connection.Open();
                        cmd.ExecuteScalar(); 
                        using (SqliteDataReader reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                tags.Add(new Tag () {
                                    Id = (int)((long)reader["Id"]),
                                    Designation = reader["Designation"].ToString() ?? "n/a"
                                });
                            }
                            reader.Close();
                        }
                        connection.Close();
                        return tags;
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Tags Selection (all)!");
                        return [];
                    }
                }
            }
        }

        /// <summary></summary>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public List<Tag> SelectAmountOfTags(int count = 50, int offset = 0) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string queryInsert = "" +
                    "SELECT *\n" +
                    "FROM Tag\n" +
                    $"LIMIT {count} OFFSET {offset}";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        List<Tag> tags = new List<Tag>(count + 1);
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        using (SqliteDataReader reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                tags.Add(new Tag() {
                                    Id = (int)((long)reader["Id"]),
                                    Designation = reader["Designation"].ToString() ?? "n/a"
                                });
                            }
                            reader.Close();
                        }
                        connection.Close();
                        return tags;
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Tags Selection (Amount with Offset)!");
                        return [];
                    }
                }
            }
        }

        public List<Tag> GetTagsByIds(List<int> ids) {
            List<Tag> tags = [];
            DataTable resDt = new DataTable();
            string query = $"SELECT Id, Designation FROM Tag WHERE Id IN ({String.Join(", ", ids)})";
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    resDt.Load(result);
                    connection.Close();
                }
            }
            foreach (DataRow row in resDt.Rows) {
                tags.Add(new Tag() {
                    Id = (int)((long)row["Id"]),
                    Designation = row["Designation"].ToString() ?? "n/a"
                });
            }
            return tags;
        }

        /// <summary></summary>
        /// <param name="tagToDelete"></param>
        /// <returns></returns>
        public bool DeleteTagById(Tag tagToDelete) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string queryInsert = "" +
                    "DELETE FROM Tag\n" +
                    $"WHERE Id LIKE {tagToDelete.Id}";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();                        
                        connection.Close();
                        return true;
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- DeleteTagById!");
                        return false;
                    }
                }
            }
        }
    }
}
