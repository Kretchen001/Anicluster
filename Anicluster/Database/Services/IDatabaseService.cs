namespace Anicluster.Database.Services {
    internal interface IDatabaseService/*<T>*/ {

        public bool InitializeTable();
        public bool TableExist();
    }
}
