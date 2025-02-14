using Anicluster.Database;
using Anicluster.userControls;
using Anicluster.windows;
using Microsoft.Data.Sqlite;
using Modells.Anime;
using Modells.ViewModel;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Anicluster {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public DataTable Animes { get; set; }

        public MainWindow() {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, OpenAddNewAnime)); // bound Strg + N

            RequestAnimesPerViewAndAddToStackPnl();
        }

        private void RequestAnimesPerViewAndAddToStackPnl() {
            using (SqliteConnection connection = new DatabaseManager($"Data Source=db/data.sqlite").GetConnection()) {
                string queryRequestView = @"
                    SELECT 
                        AnimeId, AnimeName, Favorite, Tier, StatusState 
                    FROM
                        vw_Anime;
                ";
                Animes = new DataTable();
                using (SqliteCommand cmd = new SqliteCommand(queryRequestView, connection)) {
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    Animes.Load(result);
                    connection.Close();
                }
                foreach (DataRow row in Animes.Rows) {
                    StackPnlAnimes.Children.Add(
                        new AnimeMainWindowDisplay(new AnimeViewModel(
                            id: int.Parse(row["AnimeId"].ToString() ?? "-1"),
                            name: row["AnimeName"].ToString() ?? "",
                            isFavorite: bool.Parse((row["Favorite"].ToString()!.Equals("0") ? "false" : "true") ?? "false"),
                            tier: (Tier)Enum.Parse(typeof(Tier), row["Tier"].ToString() ?? "NotDefinied"),
                            state: (State)Enum.Parse(typeof(State), row["StatusState"].ToString() ?? "0")
                        ))
                    );
                }
            }
        }

        private void OpenAddNewAnime(object sender, ExecutedRoutedEventArgs? e) {
            AddNewAnime addNewAnime = new AddNewAnime();
            addNewAnime.ShowDialog();
        }

        private void MenuItemNewAnime_Click(object sender, RoutedEventArgs e) {
            OpenAddNewAnime(sender, null);
        }

        private void MenuItemTags_Click(object sender, RoutedEventArgs e) {
            TagOrganisator tagOrga = new TagOrganisator();
            tagOrga.ShowDialog();
        }

        private void MenuItemNewAnimeSmall_Click(object sender, RoutedEventArgs e) {
            AddNewAnimeSmall addNewAnimeSmall = new AddNewAnimeSmall();
            addNewAnimeSmall.ShowDialog();
        }

        private void SortDataTableByGivenCol(string col, string direction) {
            StackPnlAnimes.Children.Clear();
            Animes.DefaultView.Sort = $"{col} {direction}";
            Animes = Animes.DefaultView.ToTable();
            foreach (DataRow row in Animes.Rows) {
                StackPnlAnimes.Children.Add(
                    new AnimeMainWindowDisplay(new AnimeViewModel(
                        id: int.Parse(row["AnimeId"].ToString() ?? "-1"),
                        name: row["AnimeName"].ToString() ?? "",
                        isFavorite: bool.Parse((row["Favorite"].ToString()!.Equals("0") ? "false" : "true") ?? "false"),
                        tier: (Tier)Enum.Parse(typeof(Tier), row["Tier"].ToString() ?? "NotDefinied"),
                        state: (State)Enum.Parse(typeof(State), row["StatusState"].ToString() ?? "0")
                    ))
                );
            }
        }

        private void SortId_Click(object sender, RoutedEventArgs e) {
            SortDataTableByGivenCol("AnimeId", "ASC");
        }

        private void SortName_Click(object sender, RoutedEventArgs e) {
            SortDataTableByGivenCol("AnimeName", "ASC");
        }

        private void SortStatus_Click(object sender, RoutedEventArgs e) {
            SortDataTableByGivenCol("StatusState", "ASC");
        }

        private void SortFav_Click(object sender, RoutedEventArgs e) {
            SortDataTableByGivenCol("Favorite", "ASC");
        }

        private void SortTier_Click(object sender, RoutedEventArgs e) {
            SortDataTableByGivenCol("Tier", "ASC");
        }
    }
}
