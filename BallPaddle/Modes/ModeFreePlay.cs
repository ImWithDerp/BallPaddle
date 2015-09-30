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
    // No lose condition, players can add balls arbitrarily and play as long as they like
    public class ModeFreePlay : Mode
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

            mainWindow.TextBlockLeft.Text = "Press B to add a ball, R to reset the field or ESC to return to main menu";

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

        private void UpdateScoreText()
        {
            mainWindow.TextBlockRight.Text = "Score: " + m_iBounces + ", High Score: " + m_iHighScore;
        }

        private void AddBall()
        {
            Widget.WidgetBall ball = new Widget.WidgetBall(mainWindow.Border.Width * 0.5, 50, 0, mainWindow.Border.Width, 0, mainWindow.Border.Height,
                BallOnReset, BallOnCollide);

            ball.InitIcon(mainWindow.CanvasPlayArea);

            lBalls.Add(ball);
        }
    }
}
