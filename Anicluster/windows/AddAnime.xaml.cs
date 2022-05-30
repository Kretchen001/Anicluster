using System.Windows;
using System.Windows.Input;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für AddAnime.xaml
    /// </summary>
    public partial class AddAnime : Window {

        private int ratingGeneral = 0;
        private int ratingStory = 0;
        private int ratingAnimation = 0;
        private int ratingSpecialEffects = 0;
        private int ratingSound = 0;
        private int ratingGermanDub = 0;

        public AddAnime() {
            InitializeComponent();

            initLabel();
        }

        private void mouseWheelSrollStory(object sender, MouseWheelEventArgs e) {
            if ((e.Delta > 0) && (ratingStory <= 100)) {
                if (ratingStory != 100) {
                    ratingStory += 1;
                }
            }
            else {
                if (ratingStory >= 0) {
                    if (ratingStory != 0) {
                        ratingStory -= 1;
                    }
                }
            }
            labelStoryRating.Content = ratingStory.ToString();
            progressBarStoryRating.Value = ratingStory;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void mouseWheelScrollAnimation(object sender, MouseWheelEventArgs e) {
            if ((e.Delta > 0) && (ratingAnimation <= 100)) {
                if (ratingAnimation != 100) {
                    ratingAnimation += 1;
                }
            }
            else {
                if (ratingAnimation >= 0) {
                    if (ratingAnimation != 0) {
                        ratingAnimation -= 1;
                    }
                }
            }
            labelAnimationRating.Content = ratingAnimation.ToString();
            progressBarAnimationRating.Value = ratingAnimation;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }
        private void mouseWheelScrollSpecialEffects(object sender, MouseWheelEventArgs e) {
            if ((e.Delta > 0) && (ratingSpecialEffects <= 100)) {
                if (ratingSpecialEffects != 100) {
                    ratingSpecialEffects += 1;
                }
            }
            else {
                if (ratingSpecialEffects >= 0) {
                    if (ratingSpecialEffects != 0) {
                        ratingSpecialEffects -= 1;
                    }
                }
            }
            labelSpecialEffectsRating.Content = ratingSpecialEffects.ToString();
            progressBarSpecialEffectsRating.Value = ratingSpecialEffects;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }
        
        private void mouseWheelScrollSound(object sender, MouseWheelEventArgs e) {
            if ((e.Delta > 0) && (ratingSound <= 100)) {
                if (ratingSound != 100) {
                    ratingSound += 1;
                }
            }
            else {
                if (ratingSound >= 0) {
                    if (ratingSound != 0) {
                        ratingSound -= 1;
                    }
                }
            }
            labelSoundRating.Content = ratingSound.ToString();
            progressBarSoundRating.Value = ratingSound;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void mouseWheelScrollGermanDub(object sender, MouseWheelEventArgs e) {
            if ((e.Delta > 0) && (ratingGermanDub <= 100)) {
                if (ratingGermanDub != 100) {
                    ratingGermanDub += 1;
                }
            }
            else {
                if (ratingGermanDub >= 0) {
                    if (ratingGermanDub != 0) {
                        ratingGermanDub -= 1;
                    }
                }
            }
            labelGermanDubRating.Content = ratingGermanDub.ToString();
            progressBarGermanDubRating.Value = ratingGermanDub;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void calculateAndPrintGeneralRating() {
            if (ratingGermanDub == 0) { 
                ratingGeneral = (int)((ratingStory * 0.4) + (ratingSound * 0.2) 
                    + (ratingAnimation * 0.3) + (ratingSpecialEffects* 0.1));
            }
            else {
                ratingGeneral = (int)((ratingStory * 0.4) + (ratingSound * 0.1) 
                    + (ratingAnimation * 0.3) + (ratingSpecialEffects * 0.1) 
                    + (ratingGermanDub* 0.1));
            }
            labelGeneralRating.Content = ratingGeneral;
            progressBarGeneralRating.Value = ratingGeneral;
        }

        private void initLabel() {
            labelGeneralRating.Content = ratingGeneral.ToString();
            labelStoryRating.Content = ratingStory.ToString();
            labelAnimationRating.Content = ratingAnimation.ToString();
            labelSpecialEffectsRating.Content = ratingSpecialEffects.ToString();
            labelSoundRating.Content = ratingSound.ToString();
            labelGermanDubRating.Content = ratingGermanDub.ToString();
        }
    }
}