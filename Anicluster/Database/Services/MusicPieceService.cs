using Microsoft.Data.Sqlite;
using Modells.Anime;
using Modells.Anime.SoundRating;
using Serilog;
using System.Data;

namespace Anicluster.Database.Services {

    public class MusicPieceService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public MusicPieceService(DatabaseManager databaseManager) {
            this._databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                string creationString = @"
                   CREATE TABLE IF NOT EXISTS MusicPiece (
                        Id INTEGER PRIMARY KEY,
                        Type INTEGER,
                        Name TEXT,
                        Comment TEXT,
                        General INTEGER
                    );
                ";
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
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
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
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                string queryInsert = @"
                    INSERT INTO MusicPiece (Type, Name, Comment, General)
                        VALUES (@Type, @Name, @Comment, @General);
                ";
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

        /// <summary></summary>
        /// <returns></returns>
        public List<MusicPiece> SelectAllMusicPieces() {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
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
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
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

        public List<MusicPiece> GetMusicPiecesByIds(List<int> ids) {
            List<MusicPiece> mps = [];
            DataTable resDt = new DataTable();
            string query = $"SELECT Id, Type, Name, Comment, General FROM MusicPiece WHERE Id IN ({String.Join(", ", ids)})";
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    resDt.Load(result);
                    connection.Close();
                }
            }
            foreach (DataRow row in resDt.Rows) {
                mps.Add(
                    new MusicPiece(
                        id: (int)((long)row["Id"]),
                        type: (MusicPieceType)Enum.Parse(typeof(MusicPieceType), row["Type"].ToString() ?? MusicPieceType.Other.ToString()),
                        name: row["Name"].ToString(),
                        comment: row["Comment"].ToString(),
                        general: (int)((long)row["General"])
                ));
            }
            return mps;
        }

        /// <summary></summary>
        /// <param name="musicPieceToDelete"></param>
        /// <returns></returns>
        public bool DeleteMusicPieceById(MusicPiece musicPieceToDelete) {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
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
