using Anicluster.Database;
using Anicluster.Database.Services;
using Modells.Anime;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für AddNewAnime.xaml
    /// </summary>
    public partial class AddNewAnime : Window {

        public Anime NewAnime { get; set; }

        private RatingGenerator window_RatingGenerator = new RatingGenerator();
        private SeasonGenerator window_SeasonGenerator = new SeasonGenerator();
        private AddTagToAnime window_AddTagToAnime = new AddTagToAnime();
        private PublishingTimeGenerator window_PublishingTime = new PublishingTimeGenerator();

        public AddNewAnime() {
            this.InitializeComponent();
            this.NewAnime = new Anime();
            this.DataContext = this;
            this.TBName.Visibility = Visibility.Visible;
            this.TBName.Focus();
            if (!this.NewAnime.Rating.IsRated) {
                this.LbGeneral.Content = "n/a";
            }
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
            if (this.NewAnime.Favorite) {
                this.StarPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFAFAFAF"));
            }
            else {
                this.StarPath.Fill = Brushes.Yellow;
            }
            this.NewAnime.Favorite = !this.NewAnime.Favorite;
        }

        private void RBtn_Checked(object sender, RoutedEventArgs e) {
            if (sender is RadioButton radioBtn) {
                if (this.NewAnime is null) {
                    return;
                }
                List<RadioButton> radioButtons = ((StackPanel)radioBtn.Parent).Children.OfType<RadioButton>().ToList();
                int selectedIndex = radioButtons.IndexOf(radioBtn);
                if (selectedIndex >= 0 && selectedIndex < Enum.GetValues(typeof(State)).Length) {
                    this.NewAnime.Status.State = (State)selectedIndex;
                }
                if (this.NewAnime.Status.State.Equals(State.Wishlist)) { // if Wishlist is selected, disable Rating
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
            this.ChangeTierButtonBorder(oldTier: this.NewAnime.Tier, newTier: Tier.S);
            this.NewAnime.Tier = Tier.S;
        }

        private void BtnTierA_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.NewAnime.Tier, newTier: Tier.A);
            this.NewAnime.Tier = Tier.A;
        }

        private void BtnTierB_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.NewAnime.Tier, newTier: Tier.B);
            this.NewAnime.Tier = Tier.B;
        }

        private void BtnTierC_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.NewAnime.Tier, newTier: Tier.C);
            this.NewAnime.Tier = Tier.C;
        }

        private void BtnTierD_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.NewAnime.Tier, newTier: Tier.D);
            this.NewAnime.Tier = Tier.D;
        }

        private void BtnTierE_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.NewAnime.Tier, newTier: Tier.E);
            this.NewAnime.Tier = Tier.E;
        }

        private void BtnRatingGenerator_Click(object sender, RoutedEventArgs e) {
            if (!this.window_RatingGenerator.IsVisible) {
                this.window_RatingGenerator.Closed += this.BtnRatingGeneratorClosed!;
                this.window_RatingGenerator.Show();
            }
        }

        private void BtnRatingGeneratorClosed(object sender, EventArgs e) {
            this.NewAnime.Rating = this.window_RatingGenerator.GenerateRating();
            if (this.NewAnime.Rating.IsRated) {
                this.ProgBarGeneral.Value = this.NewAnime.Rating.General;
                this.LbGeneral.Content = this.NewAnime.Rating.General;
            }
            this.window_RatingGenerator.Closed -= this.BtnRatingGeneratorClosed!;
            this.window_RatingGenerator = new RatingGenerator(this.NewAnime.Rating);
        }

        private void BtnSeasonGenerator_Click(object sender, RoutedEventArgs e) {
            if (!this.window_SeasonGenerator.IsVisible) {
                this.window_SeasonGenerator.Closed += this.SeasonGeneratorClosed!;
                this.window_SeasonGenerator.Show();
            }
        }

        private void SeasonGeneratorClosed(object sender, EventArgs e) {
            this.NewAnime.Seasons = new List<Season>(this.window_SeasonGenerator.Seasons);
            this.window_SeasonGenerator.Closed -= this.SeasonGeneratorClosed!;
            this.window_SeasonGenerator = new SeasonGenerator(this.NewAnime.Seasons);
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
            this.NewAnime.Tags = new List<Tag>(this.window_AddTagToAnime.SelectedTags);
            this.ListViewTags.ItemsSource = this.NewAnime.Tags;
            this.window_AddTagToAnime.Closed -= this.TagsSelectorClosed!;
            this.window_AddTagToAnime = new AddTagToAnime(this.NewAnime.Tags);
        }

        private void BtnPublishingTime_Click(object sender, RoutedEventArgs e) {
            if (!this.window_PublishingTime.IsVisible) {
                this.window_PublishingTime.Closed += this.PublishingTimeClosed!;
                this.window_PublishingTime.Owner = this;
                this.window_PublishingTime.Show();
            }
        }

        private void PublishingTimeClosed(object sender, EventArgs e) {
            this.NewAnime.MediaInfo.PublishingTime = this.window_PublishingTime.PublishingTime;
            this.window_PublishingTime.Closed -= this.PublishingTimeClosed!;
            this.window_PublishingTime = new PublishingTimeGenerator(this.NewAnime.MediaInfo.PublishingTime);
        }

        private void BtnAddAnimeAsNew_Click(object sender, RoutedEventArgs e) {
            if (this.NewAnime.Name != "") {
                this.NewAnime.Comment = this.TxtBxComment.Text.Equals("") ? null : this.TxtBxComment.Text;
                new AnimeService(new DatabaseManager($"Data Source=db/data.sqlite")).Insert(this.NewAnime);
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
            this.NewAnime.Url = $"https://www.anime-planet.com/anime/{WebUtility.UrlEncode(this.NewAnime.Name.Replace(" ", "-"))}";
            this.TBxUrl.Text = this.NewAnime.Url;
            //HttpClient client = new HttpClient();
            //client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            //HttpResponseMessage response = client.SendAsync(new HttpRequestMessage(HttpMethod.Get, NewAnime.Url)).Result;
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
