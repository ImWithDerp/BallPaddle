using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace BallPaddle
{
    // A game mode including all the elements required to respresent it
    public abstract class Mode
    {        
        protected DispatcherTimer m_Timer; // Widget update timer
        protected Paddle m_Paddle; // One paddle per game mode
        protected List<Ball> m_lBalls; // Current active balls
        protected MainWindow m_MainWindow; // Window this mode is being run in

        public Mode(MainWindow mainWindow)
        {
            m_MainWindow = mainWindow;
        }

        // Start playing the mode, adding all relevant elements to the canvas
        public virtual void StartMode()
        {
            m_Paddle = new Paddle(m_MainWindow.m_Border.Width * 0.5, m_MainWindow.m_Border.Height - 50, 0, m_MainWindow.m_Border.Width);
            m_Paddle.InitIcon(m_MainWindow.m_Canvas);

            m_lBalls = new List<Ball>();

            m_MainWindow.KeyDown += Mode_KeyDown;

            m_Timer = new DispatcherTimer();
            m_Timer.Interval = new TimeSpan(100000);
            m_Timer.Tick += Timer_Tick;
            m_Timer.Start();

            m_MainWindow.m_Text_Left.Text = "Press B to add a ball, R to rese the field or ESC to return to main menu";

            ResetMode();
        }

        // Reset the mode do its starting state
        public virtual void ResetMode()
        {
            m_MainWindow.m_Text_GameOver.Visibility = System.Windows.Visibility.Collapsed;

            m_Paddle.Reset();
            foreach (Ball ball in m_lBalls)
                ball.Destroy();

            m_lBalls.Clear();
        }

        // Stop running the mode and clean up all canvas elements
        public virtual void EndMode()
        {
            m_Timer.Stop();
            m_MainWindow.KeyDown -= Mode_KeyDown;
            m_MainWindow.m_Text_Left.Text = "";
            m_MainWindow.m_Text_Right.Text = "";

            foreach (Ball ball in m_lBalls)
                ball.Destroy();

            m_lBalls.Clear();

            if (m_Paddle != null)
            {
                m_Paddle.Destroy();

                m_Paddle = null;
            }
        }

        // Show game over text
        public virtual void GameOver()
        {
            foreach (Ball ball in m_lBalls)
                ball.Destroy();

            m_MainWindow.m_Text_GameOver.Visibility = System.Windows.Visibility.Visible;
        }

        // Update widgets
        protected virtual void Timer_Tick(object sender, EventArgs e)
        {
            if (m_Paddle != null)
            {
                m_Paddle.Update(Keyboard.IsKeyDown(Key.Left), Keyboard.IsKeyDown(Key.Right));
                m_Paddle.Draw();
            }

            List<Ball> lBallsToRemove = new List<Ball>();

            foreach (Ball ball in m_lBalls)
            {
                if (ball.m_Icon == null)
                {
                    lBallsToRemove.Add(ball);
                }
                else
                {
                    ball.Update(m_Paddle);
                    ball.Draw();
                }
            }

            foreach (Ball ball in lBallsToRemove)
            {
                m_lBalls.Remove(ball);
            }
        }

        protected abstract void Mode_KeyDown(object sender, KeyEventArgs e);

    }

    // No lose condition, players can add balls arbitrarily and play as long as they like
    class ModeFreePlay : Mode
    {
        int m_iBounces; // Number of bounces this session
        static int m_iHighScore = -1; // Max number of bounces ever for this mode

        public ModeFreePlay(MainWindow mainWindow) : base(mainWindow)
        {
            // Nothing extra to do for this mode
        }

        protected override void Mode_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.B:
                    AddBall();
                    break;
                case Key.R:
                    ResetMode();
                    break;
            }

        }

        // Set appropriate help text
        public override void StartMode()
        {
            base.StartMode();

            m_MainWindow.m_Text_Left.Text = "Press B to add a ball, R to reset the field or ESC to return to main menu";

            UpdateScoreText();
        }

        // Save current high score to disk
        private void SaveHighScore()
        {
            if (m_iHighScore > Properties.Settings.Default.SurvivalHighScore)
                Properties.Settings.Default.FreePlayHighScore = m_iHighScore;

            Properties.Settings.Default.Save();
        }

        public override void ResetMode()
        {
            base.ResetMode();

            SaveHighScore();

            m_iBounces = 0;
            m_iHighScore = Properties.Settings.Default.FreePlayHighScore;
        }

        public override void EndMode()
        {
            base.EndMode();

            SaveHighScore();
        }

        private void BallOnReset(Ball ball)
        {
            ball.RandomOffsetAndVelocity(m_MainWindow.m_Border.Width * 0.3, 2);
        }

        private void BallOnCollide(Ball ball)
        {
            m_iBounces++;

            if (m_iBounces > m_iHighScore)
                m_iHighScore = m_iBounces;

            UpdateScoreText();
        }

        private void UpdateScoreText()
        {
            m_MainWindow.m_Text_Right.Text = "Score: " + m_iBounces + ", High Score: " + m_iHighScore;
        }

        private void AddBall()
        {
            Ball ball = new Ball(m_MainWindow.m_Border.Width * 0.5, 50, 0, m_MainWindow.m_Border.Width, 0, m_MainWindow.m_Border.Height,
                BallOnReset, BallOnCollide);

            ball.InitIcon(m_MainWindow.m_Canvas);

            m_lBalls.Add(ball);
        }
    }

    // Players have limited lives consumed when a ball falls off the canvas
    // New balls are added periodically
    class ModeSurvival : Mode
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

            m_MainWindow.m_Text_Left.Text = "Press R to reset the field or ESC to return to main menu";

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

        private void BallOnReset(Ball ball)
        {
            ball.RandomOffsetAndVelocity(m_MainWindow.m_Border.Width * 0.3, 2);
        }

        private void BallOnCollide(Ball ball)
        {
            m_iBounces++;

            if (m_iBounces > m_iHighScore)
                m_iHighScore = m_iBounces;

            UpdateScoreText();
        }

        private void BallOnFall(Ball ball)
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
            m_MainWindow.m_Text_Right.Text = "Lives: " + m_iLives + ", Score: " + m_iBounces + ", High Score: " + m_iHighScore;
        }

        private void AddBall()
        {
            Ball ball = new Ball(m_MainWindow.m_Border.Width * 0.5, 50, 0, m_MainWindow.m_Border.Width, 0, m_MainWindow.m_Border.Height,
                BallOnReset, BallOnCollide, BallOnFall);

            ball.InitIcon(m_MainWindow.m_Canvas);

            m_lBalls.Add(ball);
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
