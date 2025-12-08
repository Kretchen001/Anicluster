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
            this.InitializeComponent();
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

        private void StarPath_MouseDown(object sender, MouseButtonEventArgs e) {
            if (this.Favorite) {
                this.StarPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFAFAFAF"));
            }
            else {
                this.StarPath.Fill = Brushes.Yellow;
            }
            this.Favorite = !this.Favorite;
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
            this.ChangeTierButtonBorder(oldTier: this.SelectedTier, newTier: Tier.S);
            this.SelectedTier = Tier.S;
        }

        private void BtnTierA_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.SelectedTier, newTier: Tier.A);
            this.SelectedTier = Tier.A;
        }

        private void BtnTierB_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.SelectedTier, newTier: Tier.B);
            this.SelectedTier = Tier.B;
        }

        private void BtnTierC_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.SelectedTier, newTier: Tier.C);
            this.SelectedTier = Tier.C;
        }

        private void BtnTierD_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.SelectedTier, newTier: Tier.D);
            this.SelectedTier = Tier.D;
        }

        private void BtnTierE_Click(object sender, RoutedEventArgs e) {
            this.ChangeTierButtonBorder(oldTier: this.SelectedTier, newTier: Tier.E);
            this.SelectedTier = Tier.E;
        }

        private void BtnAddAnimeAsNew_Click(object sender, RoutedEventArgs e) {
            if (this.Name != "") {
                new AnimeService(new DatabaseManager($"Data Source=db/data.sqlite")).InsertFast(this.Name, this.Favorite, this.SelectedTier);
                this.Close();
            }
        }
    }
}
