namespace BiboAnime.databaseLiteDB {

    public static class databaseConfigs {

        public static string LocationOfDataBase {
            set { }
            get {
                if(!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/database.liteDB")) {
                    if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB")) {
                        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB");
                    }
                    File.Create(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/database.liteDB").Close();
                }
                return (AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/database.liteDB");
            }
        }
    }
}
