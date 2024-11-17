namespace Anicluster.Database.Services {
    internal interface IDatabaseService {

        public bool InitializeTable();
        public bool TableExist();
    }
}
