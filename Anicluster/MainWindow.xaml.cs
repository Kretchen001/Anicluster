using Anicluster.Database;
using Anicluster.userControls;
using Anicluster.windows;
using Microsoft.Data.Sqlite;
using Modells.Anime;
using Modells.ViewModel;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Anicluster {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public DataTable Animes { get; set; } = new DataTable();
        private Dictionary<string, bool> SortDictionaryASC { get; set; } = new Dictionary<string, bool>() {
            { "AnimeId", true },
            { "AnimeName", true },
            { "StatusState", true },
            { "Favorite", true },
            { "Tier", true },
        };
        private string AppNameAndVersion { get; set; } = "Anicluster";

        public MainWindow() {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, OpenAddNewAnime)); // bound Strg + N

            RequestAnimesPerViewAndAddToStackPnl();

            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version!;
            AppNameAndVersion = $"{assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Anicluster"} - Version: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            this.Title = AppNameAndVersion;
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
                StackPnlAnimes.Children.Clear();
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
            RequestAnimesPerViewAndAddToStackPnl();
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
            if (SortDictionaryASC["AnimeId"]) {
                SortDataTableByGivenCol("AnimeId", "ASC");
            }
            else {
                SortDataTableByGivenCol("AnimeId", "DESC");
            }
            SortDictionaryASC["AnimeId"] = !SortDictionaryASC["AnimeId"];
        }

        private void SortName_Click(object sender, RoutedEventArgs e) {
            if (SortDictionaryASC["AnimeName"]) {
                SortDataTableByGivenCol("AnimeName", "ASC");
            }
            else {
                SortDataTableByGivenCol("AnimeName", "DESC");
            }
            SortDictionaryASC["AnimeName"] = !SortDictionaryASC["AnimeName"];
        }

        private void SortStatus_Click(object sender, RoutedEventArgs e) {
            if (SortDictionaryASC["StatusState"]) {
                SortDataTableByGivenCol("StatusState", "ASC");
            }
            else {
                SortDataTableByGivenCol("StatusState", "DESC");
            }
            SortDictionaryASC["StatusState"] = !SortDictionaryASC["StatusState"];
        }

        private void SortFav_Click(object sender, RoutedEventArgs e) {
            if (SortDictionaryASC["Favorite"]) {
                SortDataTableByGivenCol("Favorite", "ASC");
            }
            else {
                SortDataTableByGivenCol("Favorite", "DESC");
            }
            SortDictionaryASC["Favorite"] = !SortDictionaryASC["Favorite"];
        }

        private void SortTier_Click(object sender, RoutedEventArgs e) {
            if (SortDictionaryASC["Tier"]) {
                SortDataTableByGivenCol("Tier", "ASC");
            }
            else {
                SortDataTableByGivenCol("Tier", "DESC");
            }
            SortDictionaryASC["Tier"] = !SortDictionaryASC["Tier"];
        }
    }
}
