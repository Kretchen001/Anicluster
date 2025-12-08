using Anicluster.Database.Services;
using Anicluster.Database;
using Modells.Anime;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Serilog;

namespace Anicluster.windows {

    /// <summary>
    /// Interaktionslogik für EditAnime.xaml
    /// </summary>
    public partial class EditAnime : Window {

        AnimeService animeService = new AnimeService(new DatabaseManager($"Data Source=db/data.sqlite"));

        public Anime AnimeToEdit { get; set; }

        private RatingGenerator window_RatingGenerator = new RatingGenerator();
        private SeasonGenerator window_SeasonGenerator = new SeasonGenerator();
        private AddTagToAnime window_AddTagToAnime = new AddTagToAnime();
        private PublishingTimeGenerator window_PublishingTime = new PublishingTimeGenerator();

        public EditAnime(int AnimeId) {
            InitializeComponent();
            Anime? animeNullableTemp = animeService.SelectAnimeByX(AnimeId);
            if (animeNullableTemp is null) {
                MessageBox.Show("Fehler beim Laden eines Animes\nBitte Datenbank sichern!");
                Log.Error($"EditAnime load with id: {AnimeId}");
                return;
            }
            AnimeToEdit = animeNullableTemp.DeepClone();
            AnimeToEdit.FirstChangeOccurred += (sender, e) => {
                BtnSaveAnimeChanges.Visibility = Visibility.Visible;
                BtnCancel.Content = "Änderungen verwerfen";
            };
            AnimeToEdit.ResetFirstChangeBool();
            this.DataContext = this;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void TBName_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                TBName.Visibility = Visibility.Collapsed;
                LbName.Visibility = Visibility.Visible;
                LbName.Content = TBName.Text;
            }
        }

        private void TBName_LostFocus(object sender, RoutedEventArgs e) {
            TBName.Visibility = Visibility.Collapsed;
            LbName.Visibility = Visibility.Visible;
            LbName.Content = TBName.Text;
        }

        private void LbName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            TBName.Visibility = Visibility.Visible;
            LbName.Visibility = Visibility.Collapsed;
            TBName.Focus();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void StarPath_MouseDown(object sender, MouseButtonEventArgs e) {
            if (AnimeToEdit.Favorite) {
                StarPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFAFAFAF"));
            }
            else {
                StarPath.Fill = Brushes.Yellow;
            }
            AnimeToEdit.Favorite = !AnimeToEdit.Favorite;
        }

        private void RBtn_Checked(object sender, RoutedEventArgs e) {
            if (sender is RadioButton radioBtn) {
                if (AnimeToEdit is null) {
                    return;
                }
                List<RadioButton> radioButtons = ((StackPanel)radioBtn.Parent).Children.OfType<RadioButton>().ToList();
                int selectedIndex = radioButtons.IndexOf(radioBtn);
                if (selectedIndex >= 0 && selectedIndex < Enum.GetValues(typeof(State)).Length) {
                    AnimeToEdit.Status.State = (State)selectedIndex;
                }
                if (AnimeToEdit.Status.State.Equals(State.Wishlist)) { // if Wishlist is selected, disable Rating
                    BtnRatingGenerator.IsEnabled = false;
                    BtnTierS.IsEnabled = false;
                    BtnTierA.IsEnabled = false;
                    BtnTierB.IsEnabled = false;
                    BtnTierC.IsEnabled = false;
                    BtnTierD.IsEnabled = false;
                    BtnTierE.IsEnabled = false;
                }
                else {
                    BtnRatingGenerator.IsEnabled = true;
                    BtnTierS.IsEnabled = true;
                    BtnTierA.IsEnabled = true;
                    BtnTierB.IsEnabled = true;
                    BtnTierC.IsEnabled = true;
                    BtnTierD.IsEnabled = true;
                    BtnTierE.IsEnabled = true;
                }
            }
        }

        private void ChangeTierButtonBorder(Tier oldTier, Tier newTier) {
            BtnTierS.BorderThickness = new Thickness(0.5);
            BtnTierA.BorderThickness = new Thickness(0.5);
            BtnTierB.BorderThickness = new Thickness(0.5);
            BtnTierC.BorderThickness = new Thickness(0.5);
            BtnTierD.BorderThickness = new Thickness(0.5);
            BtnTierE.BorderThickness = new Thickness(0.5);
            switch (newTier) {
                case Tier.S: {
                        BtnTierS.BorderThickness = new Thickness(5);
                        return;
                    }
                case Tier.A: {
                        BtnTierA.BorderThickness = new Thickness(5);
                        return;
                    }
                case Tier.B: {
                        BtnTierB.BorderThickness = new Thickness(5);
                        return;
                    }
                case Tier.C: {
                        BtnTierC.BorderThickness = new Thickness(5);
                        return;
                    }
                case Tier.D: {
                        BtnTierD.BorderThickness = new Thickness(5);
                        return;
                    }
                case Tier.E: {
                        BtnTierE.BorderThickness = new Thickness(5);
                        return;
                    }
            }
        }

        private void BtnTierS_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: AnimeToEdit.Tier, newTier: Tier.S);
            AnimeToEdit.Tier = Tier.S;
        }

        private void BtnTierA_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: AnimeToEdit.Tier, newTier: Tier.A);
            AnimeToEdit.Tier = Tier.A;
        }

        private void BtnTierB_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: AnimeToEdit.Tier, newTier: Tier.B);
            AnimeToEdit.Tier = Tier.B;
        }

        private void BtnTierC_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: AnimeToEdit.Tier, newTier: Tier.C);
            AnimeToEdit.Tier = Tier.C;
        }

        private void BtnTierD_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: AnimeToEdit.Tier, newTier: Tier.D);
            AnimeToEdit.Tier = Tier.D;
        }

        private void BtnTierE_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: AnimeToEdit.Tier, newTier: Tier.E);
            AnimeToEdit.Tier = Tier.E;
        }

        private void BtnRatingGenerator_Click(object sender, RoutedEventArgs e) {
            if (!window_RatingGenerator.IsVisible) {
                window_RatingGenerator.Closed += BtnRatingGeneratorClosed!;
                window_RatingGenerator.Show();
            }
        }

        private void BtnRatingGeneratorClosed(object sender, EventArgs e) {
            AnimeToEdit.Rating = window_RatingGenerator.GenerateRating();
            if (AnimeToEdit.Rating.IsRated) {
                ProgBarGeneral.Value = AnimeToEdit.Rating.General;
                LbGeneral.Content = AnimeToEdit.Rating.General;
            }
            window_RatingGenerator.Closed -= BtnRatingGeneratorClosed!;
            window_RatingGenerator = new RatingGenerator(AnimeToEdit.Rating);
        }

        private void BtnSeasonGenerator_Click(object sender, RoutedEventArgs e) {
            if (!window_SeasonGenerator.IsVisible) {
                window_SeasonGenerator.Closed += SeasonGeneratorClosed!;
                window_SeasonGenerator.Show();
            }
        }

        private void SeasonGeneratorClosed(object sender, EventArgs e) {
            AnimeToEdit.Seasons = new List<Season>(window_SeasonGenerator.Seasons);
            window_SeasonGenerator.Closed -= SeasonGeneratorClosed!;
            window_SeasonGenerator = new SeasonGenerator(AnimeToEdit.Seasons);
        }

        private void BtnOvaGenerator_Click(object sender, RoutedEventArgs e) {
        }

        private void OvaGeneratorClosed(object sender, EventArgs e) {
        }

        private void BtnTagsSelector_Click(object sender, RoutedEventArgs e) {
            if (!window_AddTagToAnime.IsVisible) {
                window_AddTagToAnime.Closed += TagsSelectorClosed!;
                window_AddTagToAnime.Owner = this;
                window_AddTagToAnime.Show();
            }
        }

        private void TagsSelectorClosed(object sender, EventArgs e) {
            AnimeToEdit.Tags = new List<Tag>(window_AddTagToAnime.SelectedTags);
            ListViewTags.ItemsSource = AnimeToEdit.Tags;
            window_AddTagToAnime.Closed -= TagsSelectorClosed!;
            window_AddTagToAnime = new AddTagToAnime(AnimeToEdit.Tags);
        }

        private void BtnPublishingTime_Click(object sender, RoutedEventArgs e) {
            if (!window_PublishingTime.IsVisible) {
                window_PublishingTime.Closed += PublishingTimeClosed!;
                window_PublishingTime.Owner = this;
                window_PublishingTime.Show();
            }
        }

        private void PublishingTimeClosed(object sender, EventArgs e) {
            AnimeToEdit.MediaInfo.PublishingTime = window_PublishingTime.PublishingTime;
            window_PublishingTime.Closed -= PublishingTimeClosed!;
            window_PublishingTime = new PublishingTimeGenerator(AnimeToEdit.MediaInfo.PublishingTime);
        }

        private void BtnSaveAnimeChanges_Click(object sender, RoutedEventArgs e) {
            if (AnimeToEdit.Name != "") {
                AnimeToEdit.Comment = TxtBxComment.Text.Equals("") ? null : TxtBxComment.Text;
                new AnimeService(new DatabaseManager($"Data Source=db/data.sqlite")).Insert(AnimeToEdit);
                this.Close();
            }
            else {
                MessageBox.Show(
                    "Pflichtfelder sind leider nicht ausgefüllt!",
                    "",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            //zw string json = JsonSerializer.Serialize(myObject, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
        }

        private void BtnAutoLinkGeneration_Click(object sender, RoutedEventArgs e) {
            AnimeToEdit.Url = $"https://www.anime-planet.com/anime/{WebUtility.UrlEncode(AnimeToEdit.Name.Replace(" ", "-"))}";
            TBxUrl.Text = AnimeToEdit.Url;
            //HttpClient client = new HttpClient();
            //client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            //HttpResponseMessage response = client.SendAsync(new HttpRequestMessage(HttpMethod.Get, AnimeToEdit.Url)).Result;
            //if (!response.IsSuccessStatusCode)  {
            //    MessageBox.Show(
            //        "Link nicht pingbar!",
            //        "",
            //        MessageBoxButton.OK,
            //        MessageBoxImage.Warning
            //    );
            //}
        }
    }
}
