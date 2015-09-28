using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BallPaddle
{
    /// <summary>
    /// Interaction logic for HighScoreWindow.xaml
    /// </summary>
    public partial class HighScoreWindow : Window
    {
        public HighScoreWindow()
        {
            InitializeComponent();

            ShowScores();
        }

        private void ShowScores()
        {
            FreePlay.Text = "Free Play: " + Properties.Settings.Default.FreePlayHighScore;
            Survival.Text = "Survival: " + Properties.Settings.Default.SurvivalHighScore;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Set high scores to 0 and save to disk
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.FreePlayHighScore = 0;
            Properties.Settings.Default.SurvivalHighScore = 0;

            Properties.Settings.Default.Save();

            ShowScores();
        }
    }
}
