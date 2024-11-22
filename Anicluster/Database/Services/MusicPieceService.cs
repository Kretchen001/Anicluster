using Microsoft.Data.Sqlite;
using Modells.Anime.SoundRating;
using Serilog;

namespace Anicluster.Database.Services {

    public class MusicPieceService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public MusicPieceService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS MusicPiece (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    Type INTEGER," +
                    "    Name TEXT," +
                    "    Comment TEXT," +
                    "    General INTEGER" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- MusicPiece table not created!");
                        return false;
                    }
                }
            }
            Log.Information("MusicPiece table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "MusicPiece");
                    object? result = cmd.ExecuteScalar();
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'MusicPiece' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'MusicPiece' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        /// <summary></summary>
        /// <param name="musicPiece"></param>
        /// <returns></returns>
        public int Insert(MusicPiece musicPiece) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string queryInsert = "" +
                    "INSERT INTO MusicPiece (Type, Name, Comment, General) \n" +
                    "VALUES (@Type, @Name, @Comment, @General);";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        cmd.Parameters.AddWithValue("@Type", musicPiece.Type);
                        cmd.Parameters.AddWithValue("@Name", musicPiece.Name);
                        cmd.Parameters.AddWithValue("@Comment", musicPiece.Comment);
                        cmd.Parameters.AddWithValue("@General", musicPiece.General);
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "SELECT last_insert_rowid();";
                        object? insertedId = cmd.ExecuteScalar();
                        connection.Close();
                        return insertedId is not null ? (int)((long)insertedId) : -1;
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- MusicPiece not inserted!");
                        return -1;
                    }
                }
            }
        }

        /// <summary><b>NOT IMPLEMENTED</b><br></br>NOT to use</summary>
        /// <param name="musicPieceList"></param>
        /// <returns></returns>
        public List<int> InsertMusicPieceList(List<MusicPiece> musicPieceList) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                //string queryInsert = "" +
                //    "INSERT INTO Tag (Designation) \n" +
                //    "VALUES ";
                //for (int i = 0; i < musicPieceList.Count; i += 1) {
                //    queryInsert += $"(@Designation{i})";
                //    queryInsert += i != musicPieceList.Count - 1 ? ", \n" : ";";
                //}
                //string querySelectIds = "" +
                //            "SELECT Id \n" +
                //            "FROM Tag \n" +
                //            "WHERE Designation LIKE @Designation0";
                //for (int i = 1; i < musicPieceList.Count; i += 1) { // start by 1 because of string before
                //    querySelectIds += $"\n\tOR Designation LIKE @Designation{i}";
                //    if (i == musicPieceList.Count - 1) { querySelectIds += ";"; }
                //}
                //using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                //    try {
                //        for (int i = 0; i < musicPieceList.Count; i += 1) {
                //            cmd.Parameters.AddWithValue($"@Designation{i}", musicPieceList[i].Designation);
                //        }
                //        connection.Open();
                //        cmd.ExecuteNonQuery();
                //        // Request the id's
                //        cmd.CommandText = querySelectIds;
                //        // use the same Parameters from insert
                //        List<int> ids = [];
                //        using (SqliteDataReader reader = cmd.ExecuteReader()) {
                //            while (reader.Read()) {
                //                ids.Add((int)((long)reader["Id"]));
                //            }
                //            reader.Close();
                //        }
                //        connection.Close();
                //        return ids;
                //    }
                //    catch (Exception ex) {
                //        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Tags not inserted or queryable!");
                //        return [];
                //    }
                //}
                return [];
            }
        }

        /// <summary></summary>
        /// <returns></returns>
        public List<MusicPiece> SelectAllMusicPieces() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string queryInsert = "" +
                    "SELECT *\n" +
                    "FROM MusicPiece;";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        List<MusicPiece> musicPieces = [];
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        using (SqliteDataReader reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                musicPieces.Add(new MusicPiece() {
                                    Id = (int)((long)reader["Id"]),
                                    Type = (MusicPieceType)Enum.ToObject(typeof(MusicPieceType), (long)reader["Type"]),
                                    Name = (string)reader["Name"],
                                    Comment = (string)reader["Comment"],
                                    General = (int)((long)reader["General"])
                                });
                            }
                            reader.Close();
                        }
                        connection.Close();
                        return musicPieces;
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- MusicPiece Selection (all)!");
                        return [];
                    }
                }
            }
        }

        /// <summary></summary>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public List<MusicPiece> SelectAmountOfMusicPieces(int count = 50, int offset = 0) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string queryInsert = "" +
                    "SELECT *\n" +
                    "FROM MusicPiece\n" +
                    $"LIMIT {count} OFFSET {offset}";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        List<MusicPiece> musicPieces = new List<MusicPiece>(count + 1);
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        using (SqliteDataReader reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                musicPieces.Add(new MusicPiece() {
                                    Id = (int)((long)reader["Id"]),
                                    Type = (MusicPieceType)Enum.ToObject(typeof(MusicPieceType), (long)reader["Type"]),
                                    Name = (string)reader["Name"],
                                    Comment = (string)reader["Comment"],
                                    General = (int)((long)reader["General"])
                                });
                            }
                            reader.Close();
                        }
                        connection.Close();
                        return musicPieces;
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- MusicPiece Selection (Amount with Offset)!");
                        return [];
                    }
                }
            }
        }

        /// <summary></summary>
        /// <param name="musicPieceToDelete"></param>
        /// <returns></returns>
        public bool DeleteMusicPieceById(MusicPiece musicPieceToDelete) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string queryInsert = "" +
                    "DELETE FROM MusicPiece\n" +
                    $"WHERE Id LIKE {musicPieceToDelete.Id}";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();
                        return true;
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- DeleteMusicPieceById!");
                        return false;
                    }
                }
            }
        }
    }
}
