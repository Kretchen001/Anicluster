using Anicluster.Database.Services;
using Anicluster.Database;
using Modells.Anime;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Anicluster.windows {

    /// <summary>
    /// Interaktionslogik für AddNewAnimeSmall.xaml
    /// </summary>
    public partial class AddNewAnimeSmall : Window {

        public Tier SelectedTier { get; set; } = Tier.NotDefined;
        public bool Favorite { get; set; } = false;
        public string Name { get; set; } = "";

        public AddNewAnimeSmall() {
            InitializeComponent();
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

        private void StarPath_MouseDown(object sender, MouseButtonEventArgs e) {
            if (Favorite) {
                StarPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFAFAFAF"));
            }
            else {
                StarPath.Fill = Brushes.Yellow;
            }
            Favorite = !Favorite;
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
            ChangeTierButtonBorder(oldTier: SelectedTier, newTier: Tier.S);
            SelectedTier = Tier.S;
        }

        private void BtnTierA_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: SelectedTier, newTier: Tier.A);
            SelectedTier = Tier.A;
        }

        private void BtnTierB_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: SelectedTier, newTier: Tier.B);
            SelectedTier = Tier.B;
        }

        private void BtnTierC_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: SelectedTier, newTier: Tier.C);
            SelectedTier = Tier.C;
        }

        private void BtnTierD_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: SelectedTier, newTier: Tier.D);
            SelectedTier = Tier.D;
        }

        private void BtnTierE_Click(object sender, RoutedEventArgs e) {
            ChangeTierButtonBorder(oldTier: SelectedTier, newTier: Tier.E);
            SelectedTier = Tier.E;
        }

        private void BtnAddAnimeAsNew_Click(object sender, RoutedEventArgs e) {
            if (Name != "") {
                new AnimeService(new DatabaseManager($"Data Source=db/data.sqlite")).InsertFast(Name, Favorite, SelectedTier);
                this.Close();
            }
        }
    }
}
