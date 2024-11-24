using Modells.Anime;
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

        public AddNewAnime() {
            InitializeComponent();
            NewAnime = new Anime();
            this.DataContext = this;
            TBName.Visibility = Visibility.Visible;
            TBName.Focus();
            if (!NewAnime.Rating.IsRated) {
                LbGeneral.Content = "n/a";
            }
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
            if (NewAnime.Favorite) {
                StarPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFAFAFAF")); ;
            }
            else {
                StarPath.Fill = Brushes.Yellow;
            }
            NewAnime.Favorite = !NewAnime.Favorite;
        }

        private void RBtn_Checked(object sender, RoutedEventArgs e) {
            if (sender is RadioButton radioBtn) {
                if (NewAnime is null) {
                    return;
                }
                List<RadioButton> radioButtons = ((StackPanel)radioBtn.Parent).Children.OfType<RadioButton>().ToList();
                int selectedIndex = radioButtons.IndexOf(radioBtn);
                if (selectedIndex >= 0 && selectedIndex < Enum.GetValues(typeof(State)).Length) {
                    NewAnime.Status.State = (State)selectedIndex;
                }
                if (NewAnime.Status.State.Equals(State.Wishlist)) { // if Wishlist is selected, disable Rating
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
            ChangeTierButtonBorder(oldTier: NewAnime.Tier, newTier: Tier.S);
            NewAnime.Tier = Tier.S;
        }

        private void BtnTierA_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: NewAnime.Tier, newTier: Tier.A);
            NewAnime.Tier = Tier.A;
        }

        private void BtnTierB_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: NewAnime.Tier, newTier: Tier.B);
            NewAnime.Tier = Tier.B;
        }

        private void BtnTierC_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: NewAnime.Tier, newTier: Tier.C);
            NewAnime.Tier = Tier.C;
        }

        private void BtnTierD_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: NewAnime.Tier, newTier: Tier.D);
            NewAnime.Tier = Tier.D;
        }

        private void BtnTierE_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: NewAnime.Tier, newTier: Tier.E);
            NewAnime.Tier = Tier.E;
        }

        private void BtnRatingGenerator_Click(object sender, RoutedEventArgs e) {
            if (!window_RatingGenerator.IsVisible) {
                window_RatingGenerator.Closed += BtnRatingGeneratorClosed!;
                window_RatingGenerator.Show();
            }
        }

        private void BtnRatingGeneratorClosed(object sender, EventArgs e) {
            NewAnime.Rating = window_RatingGenerator.GenerateRating();
            if (NewAnime.Rating.IsRated) {
                ProgBarGeneral.Value = NewAnime.Rating.General;
                LbGeneral.Content = NewAnime.Rating.General;
            }
            window_RatingGenerator.Closed -= BtnRatingGeneratorClosed!;
            window_RatingGenerator = new RatingGenerator(NewAnime.Rating);
        }

        private void BtnSeasonGenerator_Click(object sender, RoutedEventArgs e) {
            if (!window_SeasonGenerator.IsVisible) {
                window_SeasonGenerator.Closed += SeasonGeneratorClosed!;
                window_SeasonGenerator.Show();
            }
        }

        private void SeasonGeneratorClosed(object sender, EventArgs e) {
            NewAnime.Seasons = new List<Season>(window_SeasonGenerator.Seasons);
            window_SeasonGenerator.Closed -= SeasonGeneratorClosed!;
            window_SeasonGenerator = new SeasonGenerator(NewAnime.Seasons);
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
            NewAnime.Tags = new List<Tag>(window_AddTagToAnime.SelectedTags);
            window_AddTagToAnime.Closed -= TagsSelectorClosed!;
            window_AddTagToAnime = new AddTagToAnime(NewAnime.Tags);
        }

        private void BtnAddAnimeAsNew_Click(object sender, RoutedEventArgs e) {
            if (NewAnime.Name != "" &&
                NewAnime.Seasons.Count != 0) {
                
            }
            //zw string json = JsonSerializer.Serialize(myObject, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
        }
    }
}
