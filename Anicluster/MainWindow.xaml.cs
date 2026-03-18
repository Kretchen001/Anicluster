using Anicluster.Database;
using Anicluster.userControls;
using Anicluster.windows;
using Microsoft.Data.Sqlite;
using Modells.Anime;
using Modells.ViewModel;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
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
            this.InitializeComponent();
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, this.OpenAddNewAnime)); // bound Strg + N

            this.RequestAnimesPerViewAndAddToStackPnl();

            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version!;
            this.AppNameAndVersion = $"{assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Anicluster"} - Version: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            this.Title = this.AppNameAndVersion;
        }

        private UserControl? currentEditAnimeUC = null;

        public void ShowEditAnime(UserControl editControl) {
            // 1. Anime-Stack ausblenden
            this.StackPnlAnimes.Visibility = Visibility.Collapsed;

            // 2. Falls ein EditAnime schon angezeigt wird, entfernen und Event abmelden
            if (this.currentEditAnimeUC != null) {
                if (this.currentEditAnimeUC is EditAnime oldEA) {
                    oldEA.RequestClose -= (s, e) => this.HideEditAnime(); // Achtung: Lambda funktioniert hier nicht zum Abmelden
                }
                this.GrdMain.Children.Remove(this.currentEditAnimeUC);
            }

            // 3. Neues EditAnime UC einfügen
            this.currentEditAnimeUC = editControl;

            // Event abonnieren
            if (editControl is EditAnime ea) {
                ea.RequestClose += this.EditAnime_RequestClose;
            }

            this.GrdMain.Children.Add(editControl);
        }

        private void EditAnime_RequestClose(object? sender, EventArgs e) {
            this.HideEditAnime();
        }

        public void HideEditAnime() {
            if (this.currentEditAnimeUC != null) {
                // Event abmelden
                if (this.currentEditAnimeUC is EditAnime ea) {
                    ea.RequestClose -= this.EditAnime_RequestClose;
                }

                this.GrdMain.Children.Remove(this.currentEditAnimeUC);
                this.currentEditAnimeUC = null;
            }

            this.StackPnlAnimes.Visibility = Visibility.Visible;
        }

        private void RequestAnimesPerViewAndAddToStackPnl() {
            using (SqliteConnection connection = new DatabaseManager($"Data Source=db/data.sqlite").GetConnection()) {
                string queryRequestView = @"
                    SELECT 
                        AnimeId, AnimeName, Favorite, Tier, StatusState 
                    FROM
                        vw_Anime;
                ";
                this.Animes = new DataTable();
                using (SqliteCommand cmd = new SqliteCommand(queryRequestView, connection)) {
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    this.Animes.Load(result);
                    connection.Close();
                }
                this.StackPnlAnimes.Children.Clear();
                foreach (DataRow row in this.Animes.Rows) {
                    this.StackPnlAnimes.Children.Add(
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
            this.RequestAnimesPerViewAndAddToStackPnl();
        }

        private void MenuItemNewAnime_Click(object sender, RoutedEventArgs e) {
            this.OpenAddNewAnime(sender, null);
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
            this.StackPnlAnimes.Children.Clear();
            this.Animes.DefaultView.Sort = $"{col} {direction}";
            this.Animes = this.Animes.DefaultView.ToTable();
            foreach (DataRow row in this.Animes.Rows) {
                this.StackPnlAnimes.Children.Add(
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
            if (this.SortDictionaryASC["AnimeId"]) {
                this.SortDataTableByGivenCol("AnimeId", "ASC");
            }
            else {
                this.SortDataTableByGivenCol("AnimeId", "DESC");
            }
            this.SortDictionaryASC["AnimeId"] = !this.SortDictionaryASC["AnimeId"];
        }

        private void SortName_Click(object sender, RoutedEventArgs e) {
            if (this.SortDictionaryASC["AnimeName"]) {
                this.SortDataTableByGivenCol("AnimeName", "ASC");
            }
            else {
                this.SortDataTableByGivenCol("AnimeName", "DESC");
            }
            this.SortDictionaryASC["AnimeName"] = !this.SortDictionaryASC["AnimeName"];
        }

        private void SortStatus_Click(object sender, RoutedEventArgs e) {
            if (this.SortDictionaryASC["StatusState"]) {
                this.SortDataTableByGivenCol("StatusState", "ASC");
            }
            else {
                this.SortDataTableByGivenCol("StatusState", "DESC");
            }
            this.SortDictionaryASC["StatusState"] = !this.SortDictionaryASC["StatusState"];
        }

        private void SortFav_Click(object sender, RoutedEventArgs e) {
            if (this.SortDictionaryASC["Favorite"]) {
                this.SortDataTableByGivenCol("Favorite", "ASC");
            }
            else {
                this.SortDataTableByGivenCol("Favorite", "DESC");
            }
            this.SortDictionaryASC["Favorite"] = !this.SortDictionaryASC["Favorite"];
        }

        private void SortTier_Click(object sender, RoutedEventArgs e) {
            if (this.SortDictionaryASC["Tier"]) {
                this.SortDataTableByGivenCol("Tier", "ASC");
            }
            else {
                this.SortDataTableByGivenCol("Tier", "DESC");
            }
            this.SortDictionaryASC["Tier"] = !this.SortDictionaryASC["Tier"];
        }
    }
}
