﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BallPaddle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Random random = new Random();

        public Modes.Mode m_CurrentMode;

        public MainWindow()
        {
            InitializeComponent();

            this.KeyDown += MainWindow_KeyDown;
        }

        // Return to main menu
        public void OpenMainMenu()
        {
            if (m_CurrentMode != null)
                m_CurrentMode.EndMode();

            GridGameScreen.Visibility = Visibility.Collapsed;
            GridMainMenu.Visibility = Visibility.Visible;
        }

        // Start the specified game mode
        public void StartMode(Modes.Mode mode)
        {
            m_CurrentMode = mode;

            GridMainMenu.Visibility = Visibility.Collapsed;
            GridGameScreen.Visibility = Visibility.Visible;

            mode.StartMode();
        }

        // Let user press escape to return to main menu
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    OpenMainMenu();
                    break;
            }
             
        }

        // Start free play mode
        private void FreePlay_Click(object sender, RoutedEventArgs e)
        {
            StartMode(new Modes.ModeFreePlay(this));
        }

        // Start survival mode
        private void Survival_Click(object sender, RoutedEventArgs e)
        {
            StartMode(new Modes.ModeSurvival(this));
        }

        // Show high scores window
        private void HighScores_Click(object sender, RoutedEventArgs e)
        {
            HighScoreWindow highScoreWindow = new HighScoreWindow();

            highScoreWindow.ShowDialog();
        }
    }
}
