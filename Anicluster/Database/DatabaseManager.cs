using Microsoft.Data.Sqlite;

namespace Anicluster.Database {

    public class DatabaseManager {

        private readonly string _connectionString;

        public DatabaseManager(string connectionString) {
            this._connectionString = connectionString;
        }

        public SqliteConnection GetConnection() {
            return new SqliteConnection(this._connectionString);
        }
    }
}
