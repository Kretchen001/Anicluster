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
            this.InitializeComponent();
            Anime? animeNullableTemp = this.animeService.SelectAnimeByX(AnimeId);
            if (animeNullableTemp is null) {
                MessageBox.Show("Fehler beim Laden eines Animes\nBitte Datenbank sichern!");
                Log.Error($"EditAnime load with id: {AnimeId}");
                return;
            }
            this.AnimeToEdit = animeNullableTemp.DeepClone();
            this.AnimeToEdit.FirstChangeOccurred += (sender, e) => {
                this.BtnSaveAnimeChanges.Visibility = Visibility.Visible;
                this.BtnCancel.Content = "Änderungen verwerfen";
            };
            this.AnimeToEdit.ResetFirstChangeBool();
            this.DataContext = this;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void TBName_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                this.TBName.Visibility = Visibility.Collapsed;
                this.LbName.Visibility = Visibility.Visible;
                this.LbName.Content = this.TBName.Text;
            }
        }

        private void TBName_LostFocus(object sender, RoutedEventArgs e) {
            this.TBName.Visibility = Visibility.Collapsed;
            this.LbName.Visibility = Visibility.Visible;
            this.LbName.Content = this.TBName.Text;
        }

        private void LbName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            this.TBName.Visibility = Visibility.Visible;
            this.LbName.Visibility = Visibility.Collapsed;
            this.TBName.Focus();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void StarPath_MouseDown(object sender, MouseButtonEventArgs e) {
            if (this.AnimeToEdit.Favorite) {
                this.StarPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFAFAFAF"));
            }
            else {
                this.StarPath.Fill = Brushes.Yellow;
            }
            this.AnimeToEdit.Favorite = !this.AnimeToEdit.Favorite;
        }

        private void RBtn_Checked(object sender, RoutedEventArgs e) {
            if (sender is RadioButton radioBtn) {
                if (this.AnimeToEdit is null) {
                    return;
                }
                List<RadioButton> radioButtons = ((StackPanel)radioBtn.Parent).Children.OfType<RadioButton>().ToList();
                int selectedIndex = radioButtons.IndexOf(radioBtn);
                if (selectedIndex >= 0 && selectedIndex < Enum.GetValues(typeof(State)).Length) {
                    this.AnimeToEdit.Status.State = (State)selectedIndex;
                }
                if (this.AnimeToEdit.Status.State.Equals(State.Wishlist)) { // if Wishlist is selected, disable Rating
                    this.BtnRatingGenerator.IsEnabled = false;
                    this.BtnTierS.IsEnabled = false;
                    this.BtnTierA.IsEnabled = false;
                    this.BtnTierB.IsEnabled = false;
                    this.BtnTierC.IsEnabled = false;
                    this.BtnTierD.IsEnabled = false;
                    this.BtnTierE.IsEnabled = false;
                }
                else {
                    this.BtnRatingGenerator.IsEnabled = true;
                    this.BtnTierS.IsEnabled = true;
                    this.BtnTierA.IsEnabled = true;
                    this.BtnTierB.IsEnabled = true;
                    this.BtnTierC.IsEnabled = true;
                    this.BtnTierD.IsEnabled = true;
                    this.BtnTierE.IsEnabled = true;
                }
            }
        }

        private void ChangeTierButtonBorder(Tier oldTier, Tier newTier) {
            this.BtnTierS.BorderThickness = new Thickness(0.5);
            this.BtnTierA.BorderThickness = new Thickness(0.5);
            this.BtnTierB.BorderThickness = new Thickness(0.5);
            this.BtnTierC.BorderThickness = new Thickness(0.5);
            this.BtnTierD.BorderThickness = new Thickness(0.5);
            this.BtnTierE.BorderThickness = new Thickness(0.5);
            switch (newTier) {
                case Tier.S: {
                        this.BtnTierS.BorderThickness = new Thickness(5);
                        return;
                    }
                case Tier.A: {
                        this.BtnTierA.BorderThickness = new Thickness(5);
                        return;
                    }
                case Tier.B: {
                        this.BtnTierB.BorderThickness = new Thickness(5);
                        return;
                    }
                case Tier.C: {
                        this.BtnTierC.BorderThickness = new Thickness(5);
                        return;
                    }
                case Tier.D: {
                        this.BtnTierD.BorderThickness = new Thickness(5);
                        return;
                    }
                case Tier.E: {
                        this.BtnTierE.BorderThickness = new Thickness(5);
                        return;
                    }
            }
        }

        private void BtnTierS_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.AnimeToEdit.Tier, newTier: Tier.S);
            this.AnimeToEdit.Tier = Tier.S;
        }

        private void BtnTierA_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.AnimeToEdit.Tier, newTier: Tier.A);
            this.AnimeToEdit.Tier = Tier.A;
        }

        private void BtnTierB_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.AnimeToEdit.Tier, newTier: Tier.B);
            this.AnimeToEdit.Tier = Tier.B;
        }

        private void BtnTierC_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.AnimeToEdit.Tier, newTier: Tier.C);
            this.AnimeToEdit.Tier = Tier.C;
        }

        private void BtnTierD_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.AnimeToEdit.Tier, newTier: Tier.D);
            this.AnimeToEdit.Tier = Tier.D;
        }

        private void BtnTierE_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.AnimeToEdit.Tier, newTier: Tier.E);
            this.AnimeToEdit.Tier = Tier.E;
        }

        private void BtnRatingGenerator_Click(object sender, RoutedEventArgs e) {
            if (!this.window_RatingGenerator.IsVisible) {
                this.window_RatingGenerator.Closed += this.BtnRatingGeneratorClosed!;
                this.window_RatingGenerator.Show();
            }
        }

        private void BtnRatingGeneratorClosed(object sender, EventArgs e) {
            this.AnimeToEdit.Rating = this.window_RatingGenerator.GenerateRating();
            if (this.AnimeToEdit.Rating.IsRated) {
                this.ProgBarGeneral.Value = this.AnimeToEdit.Rating.General;
                this.LbGeneral.Content = this.AnimeToEdit.Rating.General;
            }
            this.window_RatingGenerator.Closed -= this.BtnRatingGeneratorClosed!;
            this.window_RatingGenerator = new RatingGenerator(this.AnimeToEdit.Rating);
        }

        private void BtnSeasonGenerator_Click(object sender, RoutedEventArgs e) {
            if (!this.window_SeasonGenerator.IsVisible) {
                this.window_SeasonGenerator.Closed += this.SeasonGeneratorClosed!;
                this.window_SeasonGenerator.Show();
            }
        }

        private void SeasonGeneratorClosed(object sender, EventArgs e) {
            this.AnimeToEdit.Seasons = new List<Season>(this.window_SeasonGenerator.Seasons);
            this.window_SeasonGenerator.Closed -= this.SeasonGeneratorClosed!;
            this.window_SeasonGenerator = new SeasonGenerator(this.AnimeToEdit.Seasons);
        }

        private void BtnOvaGenerator_Click(object sender, RoutedEventArgs e) {
        }

        private void OvaGeneratorClosed(object sender, EventArgs e) {
        }

        private void BtnTagsSelector_Click(object sender, RoutedEventArgs e) {
            if (!this.window_AddTagToAnime.IsVisible) {
                this.window_AddTagToAnime.Closed += this.TagsSelectorClosed!;
                this.window_AddTagToAnime.Owner = this;
                this.window_AddTagToAnime.Show();
            }
        }

        private void TagsSelectorClosed(object sender, EventArgs e) {
            this.AnimeToEdit.Tags = new List<Tag>(this.window_AddTagToAnime.SelectedTags);
            this.ListViewTags.ItemsSource = this.AnimeToEdit.Tags;
            this.window_AddTagToAnime.Closed -= this.TagsSelectorClosed!;
            this.window_AddTagToAnime = new AddTagToAnime(this.AnimeToEdit.Tags);
        }

        private void BtnPublishingTime_Click(object sender, RoutedEventArgs e) {
            if (!this.window_PublishingTime.IsVisible) {
                this.window_PublishingTime.Closed += this.PublishingTimeClosed!;
                this.window_PublishingTime.Owner = this;
                this.window_PublishingTime.Show();
            }
        }

        private void PublishingTimeClosed(object sender, EventArgs e) {
            this.AnimeToEdit.MediaInfo.PublishingTime = this.window_PublishingTime.PublishingTime;
            this.window_PublishingTime.Closed -= this.PublishingTimeClosed!;
            this.window_PublishingTime = new PublishingTimeGenerator(this.AnimeToEdit.MediaInfo.PublishingTime);
        }

        private void BtnSaveAnimeChanges_Click(object sender, RoutedEventArgs e) {
            if (this.AnimeToEdit.Name != "") {
                this.AnimeToEdit.Comment = this.TxtBxComment.Text.Equals("") ? null : this.TxtBxComment.Text;
                new AnimeService(new DatabaseManager($"Data Source=db/data.sqlite")).Insert(this.AnimeToEdit);
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
            this.AnimeToEdit.Url = $"https://www.anime-planet.com/anime/{WebUtility.UrlEncode(this.AnimeToEdit.Name.Replace(" ", "-"))}";
            this.TBxUrl.Text = this.AnimeToEdit.Url;
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
