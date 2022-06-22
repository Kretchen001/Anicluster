using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Anicluster.windows.filter {
    /// <summary>
    /// Interaktionslogik für FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window {

        List<tagFilter> tagSelectionFilter = new List<tagFilter>();

        private int ratingGeneral = 0;
        private int ratingStory = 0;
        private int ratingAnimation = 0;
        private int ratingSpecialEffects = 0;
        private int ratingSound = 0;
        private int ratingGermanDub = 0;

        private bool isShiftPressed = false;

        public FilterWindow() {
            InitializeComponent();
            tagSelectionFilter.Add(new tagFilter() {
                isChecked = false,
                tagDesignator = "Hallo"
            });
            tagSelectionFilter.Add(new tagFilter() {
                isChecked = false,
                tagDesignator = "Hallo2"
            });
            tagSelectionFilter.Add(new tagFilter() {
                isChecked = true,
                tagDesignator = "Hallo3"
            });
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
            AddHandler(Keyboard.KeyUpEvent, (KeyEventHandler)HandleKeyUpEvent);
        }
        
        private void HandleKeyDownEvent(object sender, KeyEventArgs e) {
            if ((e.Key == Key.LeftShift) && (isShiftPressed == false)) {
                isShiftPressed = true;
            }
        }

        private void HandleKeyUpEvent(object sender, KeyEventArgs e) {
            if ((e.Key == Key.LeftShift) && (isShiftPressed == true)) {
                isShiftPressed = false;
            }
        }

        private void mouseWheelSrollStory(object sender, MouseWheelEventArgs e) {
            ratingStoryIncDec(e.Delta);
        }

        private void mouseWheelScrollAnimation(object sender, MouseWheelEventArgs e) {
            ratingAnimationIncDec(e.Delta);
        }

        private void mouseWheelScrollSpecialEffects(object sender, MouseWheelEventArgs e) {
            ratingSpecialEffectsIncDec(e.Delta);
        }

        private void mouseWheelScrollSound(object sender, MouseWheelEventArgs e) {
            ratingSoundIncDec(e.Delta);
        }

        private void mouseWheelScrollGermanDub(object sender, MouseWheelEventArgs e) {
            ratingGermanDubIncDec(e.Delta);
        }

        private void ratingStoryIncDec(int e) {
            if ((e > 0) && (ratingStory <= 100)) {
                if (ratingStory != 100) {
                    if (isShiftPressed && ratingStory < 90) {
                        ratingStory += 10;
                    }
                    else if (isShiftPressed && ratingStory >= 90) {
                        ratingStory = 100;
                    }
                    else {
                        ratingStory += 1;
                    }
                }
            }
            else {
                if (ratingStory > 0) {
                    if (isShiftPressed && ratingStory > 10) {
                        ratingStory -= 10;
                    }
                    else if (isShiftPressed && ratingStory <= 10) {
                        ratingStory = 0;
                    }
                    else {
                        ratingStory -= 1;
                    }
                }
            }
            labelStoryRating.Content = ratingStory.ToString();
            progressBarStoryRating.Value = ratingStory;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingAnimationIncDec(int e) {
            if ((e > 0) && (ratingAnimation <= 100)) {
                if (ratingAnimation != 100) {
                    if (isShiftPressed && ratingAnimation < 90) {
                        ratingAnimation += 10;
                    }
                    else if (isShiftPressed && ratingAnimation >= 90) {
                        ratingAnimation = 100;
                    }
                    else {
                        ratingAnimation += 1;
                    }
                }
            }
            else {
                if (ratingAnimation > 0) {
                    if (isShiftPressed && ratingAnimation > 10) {
                        ratingAnimation -= 10;
                    }
                    else if (isShiftPressed && ratingAnimation <= 10) {
                        ratingAnimation = 0;
                    }
                    else {
                        ratingAnimation -= 1;
                    }
                }
            }
            labelAnimationRating.Content = ratingAnimation.ToString();
            progressBarAnimationRating.Value = ratingAnimation;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingSpecialEffectsIncDec(int e) {
            if ((e > 0) && (ratingSpecialEffects <= 100)) {
                if (ratingSpecialEffects != 100) {
                    if (isShiftPressed && ratingSpecialEffects < 90) {
                        ratingSpecialEffects += 10;
                    }
                    else if (isShiftPressed && ratingSpecialEffects >= 90) {
                        ratingSpecialEffects = 100;
                    }
                    else {
                        ratingSpecialEffects += 1;
                    }
                }
            }
            else {
                if (ratingSpecialEffects > 0) {
                    if (isShiftPressed && ratingSpecialEffects > 10) {
                        ratingSpecialEffects -= 10;
                    }
                    else if (isShiftPressed && ratingSpecialEffects <= 10) {
                        ratingSpecialEffects = 0;
                    }
                    else {
                        ratingSpecialEffects -= 1;
                    }
                }
            }
            labelSpecialEffectsRating.Content = ratingSpecialEffects.ToString();
            progressBarSpecialEffectsRating.Value = ratingSpecialEffects;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingSoundIncDec(int e) {
            if ((e > 0) && (ratingSound <= 100)) {
                if (ratingSound != 100) {
                    if (isShiftPressed && ratingSound < 90) {
                        ratingSound += 10;
                    }
                    else if (isShiftPressed && ratingSound >= 90) {
                        ratingSound = 100;
                    }
                    else {
                        ratingSound += 1;
                    }
                }
            }
            else {
                if (ratingSound > 0) {
                    if (isShiftPressed && ratingSound > 10) {
                        ratingSound -= 10;
                    }
                    else if (isShiftPressed && ratingSound <= 10) {
                        ratingSound = 0;
                    }
                    else {
                        ratingSound -= 1;
                    }
                }
            }
            labelSoundRating.Content = ratingSound.ToString();
            progressBarSoundRating.Value = ratingSound;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingGermanDubIncDec(int e) {
            if ((e > 0) && (ratingGermanDub <= 100)) {
                if (ratingGermanDub != 100) {
                    if (isShiftPressed && ratingGermanDub < 90) {
                        ratingGermanDub += 10;
                    }
                    else if (isShiftPressed && ratingGermanDub >= 90) {
                        ratingGermanDub = 100;
                    }
                    else {
                        ratingGermanDub += 1;
                    }
                }
            }
            else {
                if (ratingGermanDub > 0) {
                    if (isShiftPressed && ratingGermanDub > 10) {
                        ratingGermanDub -= 10;
                    }
                    else if (isShiftPressed && ratingGermanDub <= 10) {
                        ratingGermanDub = 0;
                    }
                    else {
                        ratingGermanDub -= 1;
                    }
                }
            }
            labelGermanDubRating.Content = ratingGermanDub.ToString();
            progressBarGermanDubRating.Value = ratingGermanDub;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }
        private void clickRatingGeneralPlus(object sender, RoutedEventArgs e) {
            // TODO: 
        }

        private void clickRatingGeneralMinus(object sender, RoutedEventArgs e) {
            // TODO:
        }

        private void clickRatingStoryPlus(object sender, RoutedEventArgs e) {
            ratingStoryIncDec(1);
        }

        private void clickRatingStoryMinus(object sender, RoutedEventArgs e) {
            ratingStoryIncDec(-1);
        }

        private void clickRatingAnimationPlus(object sender, RoutedEventArgs e) {
            ratingAnimationIncDec(1);
        }

        private void clickRatingAnimationMinus(object sender, RoutedEventArgs e) {
            ratingAnimationIncDec(-1);
        }

        private void clickRatingSpecialEffectsPlus(object sender, RoutedEventArgs e) {
            ratingSpecialEffectsIncDec(1);
        }

        private void clickRatingSpecialEffectsMinus(object sender, RoutedEventArgs e) {
            ratingSpecialEffectsIncDec(-1);
        }

        private void clickRatingSoundPlus(object sender, RoutedEventArgs e) {
            ratingSoundIncDec(1);
        }

        private void clickRatingSoundMinus(object sender, RoutedEventArgs e) {
            ratingSoundIncDec(-1);
        }

        private void clickRatingGermanDubPlus(object sender, RoutedEventArgs e) {
            ratingGermanDubIncDec(1);
        }

        private void clickRatingGermanDubMinus(object sender, RoutedEventArgs e) {
            ratingGermanDubIncDec(-1);
        }

        private void calculateAndPrintGeneralRating() {
            if (ratingGermanDub == 0) {
                ratingGeneral = (int)((ratingStory * 0.4) + (ratingSound * 0.2)
                    + (ratingAnimation * 0.3) + (ratingSpecialEffects * 0.1));
            }
            else {
                ratingGeneral = (int)((ratingStory * 0.4) + (ratingSound * 0.1)
                    + (ratingAnimation * 0.3) + (ratingSpecialEffects * 0.1)
                    + (ratingGermanDub * 0.1));
            }
            labelGeneralRating.Content = ratingGeneral;
            progressBarGeneralRating.Value = ratingGeneral;
        }
    }

    internal class tagFilter {
        public bool isChecked = false;
        public string tagDesignator { get; set; } = "";
    }
}
