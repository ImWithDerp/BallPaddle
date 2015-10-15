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
    // A game mode including all the elements required to respresent it
    public abstract class Mode
    {        
        protected DispatcherTimer updateTimer; // Widget update timer
        protected Widget.WidgetPaddle paddle; // One paddle per game mode
        protected List<Widget.WidgetBall> lBalls; // Current active balls
        protected MainWindow mainWindow; // Window this mode is being run in

        public Mode(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        // Start playing the mode, adding all relevant elements to the canvas
        public virtual void StartMode()
        {
            paddle = new Widget.WidgetPaddle(mainWindow.Border.Width * 0.5, mainWindow.Border.Height - 50, 0, mainWindow.Border.Width);
            paddle.InitIcon(mainWindow.CanvasPlayArea);

            lBalls = new List<Widget.WidgetBall>();

            mainWindow.KeyDown += Mode_KeyDown;

            updateTimer = new DispatcherTimer();
            updateTimer.Interval = new TimeSpan(100000);
            updateTimer.Tick += Timer_Tick;
            updateTimer.Start();

            mainWindow.TextBlockLeft.Text = "Press B to add a ball, R to rese the field or ESC to return to main menu";

            ResetMode();
        }

        // Reset the mode do its starting state
        public virtual void ResetMode()
        {
            mainWindow.TextBlockGameOver.Visibility = System.Windows.Visibility.Collapsed;

            paddle.Reset();
            foreach (Widget.WidgetBall ball in lBalls)
                ball.Destroy();

            lBalls.Clear();
        }

        // Stop running the mode and clean up all canvas elements
        public virtual void EndMode()
        {
            updateTimer.Stop();
            mainWindow.KeyDown -= Mode_KeyDown;
            mainWindow.TextBlockLeft.Text = "";
            mainWindow.TextBlockRight.Text = "";

            foreach (Widget.WidgetBall ball in lBalls)
                ball.Destroy();

            lBalls.Clear();

            if (paddle != null)
            {
                paddle.Destroy();

                paddle = null;
            }
        }

        // Show game over text
        public virtual void GameOver()
        {
            foreach (Widget.WidgetBall ball in lBalls)
                ball.Destroy();

            mainWindow.TextBlockGameOver.Visibility = System.Windows.Visibility.Visible;
        }

        // Update widgets
        protected virtual void Timer_Tick(object sender, EventArgs e)
        {
            if (paddle != null)
            {
                paddle.Update(Keyboard.IsKeyDown(Key.Left), Keyboard.IsKeyDown(Key.Right));
                paddle.Draw();
            }

            List<Widget.WidgetBall> lBallsToRemove = new List<Widget.WidgetBall>();

            foreach (Widget.WidgetBall ball in lBalls)
            {
                if (ball.icon == null)
                {
                    lBallsToRemove.Add(ball);
                }
                else
                {
                    ball.Update(paddle);
                    ball.Draw();
                }
            }

            foreach (Widget.WidgetBall ball in lBallsToRemove)
            {
                lBalls.Remove(ball);
            }
        }

        protected abstract void Mode_KeyDown(object sender, KeyEventArgs e);

    }
}
