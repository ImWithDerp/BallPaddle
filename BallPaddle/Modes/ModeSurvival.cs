using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace BallPaddle.Modes
{
    // Players have limited lives consumed when a ball falls off the canvas
    // New balls are added periodically
    public class ModeSurvival : Mode
    {
        const int m_ciMaxLives = 3; // Starting lives 
        const int m_ciNewBallInterval = 100000000; // Nanoseconds between adding balls
        int m_iLives; // Current lives
        int m_iBounces; // Number of bounces this session
        static int m_iHighScore = -1; // Max number of bounces ever for this mode
        int m_iBallsToAdd = 0; // Incremented by survival timer, consumed by update timer, use mutex to manage access
        Mutex m_mBallsToAddMutex; // Mutex for accessing BallsToAdd

        protected DispatcherTimer m_SurvivalTimer; // Adds new balls over time

        public ModeSurvival(MainWindow mainWindow) : base(mainWindow)
        {
            m_mBallsToAddMutex = new Mutex();

            // Start survival timer
            m_SurvivalTimer = new DispatcherTimer();
            m_SurvivalTimer.Interval = new TimeSpan(m_ciNewBallInterval);
            m_SurvivalTimer.Tick += SurvivalTimer_Tick;
            m_SurvivalTimer.Start();
        }

        // Set appropriate help text
        public override void StartMode()
        {
            base.StartMode();

            mainWindow.TextBlockLeft.Text = "Press R to reset the field or ESC to return to main menu";

            UpdateScoreText();
        }

        // Save high score to disk
        private void SaveHighScore()
        {
            if (m_iHighScore > Properties.Settings.Default.SurvivalHighScore)
                Properties.Settings.Default.SurvivalHighScore = m_iHighScore;

            Properties.Settings.Default.Save();
        }

        // Make sure to reset survival timer
        public override void ResetMode()
        {
            base.ResetMode();

            SaveHighScore();

            m_iLives = m_ciMaxLives;
            m_iBounces = 0;
            m_iHighScore = Properties.Settings.Default.SurvivalHighScore;

            UpdateScoreText();

            m_SurvivalTimer.Stop();
            m_SurvivalTimer.Start();

            m_iBallsToAdd++;
        }

        public override void EndMode()
        {
            base.EndMode();

            m_SurvivalTimer.Stop();

            SaveHighScore();
        }

        protected override void Mode_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.R:
                    ResetMode();
                    break;
            }

        }

        private void BallOnReset(object sender, Widget.WidgetBall ball)
        {
            ball.RandomOffsetAndVelocity(mainWindow.Border.Width * 0.3, 2);
        }

        private void BallOnCollide(object sender, Widget.WidgetBall ball)
        {
            m_iBounces++;

            if (m_iBounces > m_iHighScore)
                m_iHighScore = m_iBounces;

            UpdateScoreText();
        }

        private void BallOnFall(object sender, Widget.WidgetBall ball)
        {
            m_iLives--;

            if (m_iLives < 0)
                GameOver();
            else
                UpdateScoreText();
        }

        public override void GameOver()
        {
            m_SurvivalTimer.Stop();

            base.GameOver();

            SaveHighScore();
        }

        private void UpdateScoreText()
        {
            mainWindow.TextBlockRight.Text = "Lives: " + m_iLives + ", Score: " + m_iBounces + ", High Score: " + m_iHighScore;
        }

        private void AddBall()
        {
            Widget.WidgetBall ball = new Widget.WidgetBall(mainWindow.Border.Width * 0.5, 50, 0, mainWindow.Border.Width, 0, mainWindow.Border.Height,
                BallOnReset, BallOnCollide, BallOnFall);

            ball.InitIcon(mainWindow.CanvasPlayArea);

            lBalls.Add(ball);
        }

        // Add all pending balls to the field and reset pending ball counter
        protected override void Timer_Tick(object sender, EventArgs e)
        {
            m_mBallsToAddMutex.WaitOne();
            for (int i = 0; i < m_iBallsToAdd; i++)
                AddBall();
            m_iBallsToAdd = 0;
            m_mBallsToAddMutex.ReleaseMutex();

            base.Timer_Tick(sender, e);
        }

        // Increment pending balls counter to be handled by next update timer tick
        protected virtual void SurvivalTimer_Tick(object sender, EventArgs e)
        {
            m_mBallsToAddMutex.WaitOne();
            m_iBallsToAdd++;
            m_mBallsToAddMutex.ReleaseMutex();
        }
    }
}
