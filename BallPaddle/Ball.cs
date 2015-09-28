using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace BallPaddle
{
    public delegate void BallEventDelegate(Ball ball);

    public class Ball : Widget
    {
        // Physical properties
        public double m_dRadius;
        public double m_dGravity;

        // Icon is an ellipse (circular)
        public Ellipse m_IconBall
        {
            get
            {
                return m_Icon as Ellipse;
            }

            set
            {
                m_Icon = value;
            }
        }

        // Min/max x bounds, ball will rebound off X bounds
        public double m_dMinX;
        public double m_dMaxX;

        // Min/max y bounds, ball will reset off max Y bound, min y bound currently unused
        public double m_dMinY;
        public double m_dMaxY;

        // Current physical state
        public double m_dVelX;
        public double m_dVelY;

        // Delegates called when certain events happen, can be null
        public BallEventDelegate m_OnReset;
        public BallEventDelegate m_OnCollide;
        public BallEventDelegate m_OnFall;

        // Init default properties, some depend on the size of the canvas and must be specified
        // Also specify delegates if needed
        public Ball(double dStartX, double dStartY, double dMinX, double dMaxX, double dMinY, double dMaxY,
             BallEventDelegate onReset = null, BallEventDelegate onCollide = null, BallEventDelegate onFall = null,
             double dRadius = 20.0, double dGravity = 0.1)
        {
            m_dStartX = dStartX;
            m_dStartY = dStartY;
            m_dMinX = dMinX + dRadius;
            m_dMaxX = dMaxX - dRadius;
            m_dMinY = dMinY;
            m_dMaxY = dMaxY;

            m_dRadius = dRadius;
            m_dGravity = dGravity;

            m_OnReset = onReset;
            m_OnCollide = onCollide;
            m_OnFall = onFall;

            Reset();
        }

        // Update circle location
        public override void Draw()
        {
            if (m_IconBall != null)
               m_IconBall.Margin = new System.Windows.Thickness(m_dPosX - m_dRadius*.5, m_dPosY - m_dRadius*.5, 0, 0);
        }
        
        public override void Reset()
        {
            m_dPosX = m_dStartX;
            m_dPosY = m_dStartY;
            m_dVelX = 0;
            m_dVelY = 0;

            if (m_OnReset != null)
                m_OnReset(this);
        }

        // Shift position and velocity by a random amount, ranging from no change to maxDist and maxVel
        public void RandomOffsetAndVelocity(double maxDist, double maxVel)
        {
            m_dPosX += (MainWindow.random.NextDouble() - 0.5) * maxDist * 2.0;

            m_dVelX += (MainWindow.random.NextDouble() - 0.5) * maxVel * 2.0;
        }

        // Update ball's position and velocity, checking collision with the current paddle
        public void Update(Paddle paddle)
        {
            m_dVelY += m_dGravity;

            m_dPosX += m_dVelX;
            m_dPosY += m_dVelY;

            if (m_dPosY > m_dMaxY)
            {
                Reset();

                if (m_OnFall != null)
                    m_OnFall(this);
            }
            else if (Collide(paddle))
            {
                m_dVelY *= -1;
                m_dPosY = paddle.m_dPosY - m_dRadius;

                // Make the ball bounce off a bit based on distance from centre of paddle to make things interesting
                m_dVelX += (m_dPosX - paddle.m_dPosX) * paddle.m_dAngle;

                // Bring the ball's velocity closer to the paddle's
                //m_dVelX = paddle.m_dVelX * paddle.m_dFriction + m_dVelX * (paddle.m_dFriction - 1);

                if (m_OnCollide != null)
                    m_OnCollide(this);
            }

            if (m_dPosX > m_dMaxX)
            {
                m_dPosX = m_dMaxX;
                m_dVelX *= -1;
            }
            else if (m_dPosX < m_dMinX)
            {
                m_dPosX = m_dMinX;
                m_dVelX *= -1;
            }
        }

        // Check collision with paddle
        public bool Collide (Paddle paddle)
        {
            bool result = false;

            // This isn't normally how you check if a circle intersects with a line, I'm cheating for now
            if (m_dPosY + m_dRadius >= paddle.m_dPosY && m_dPosY <= paddle.m_dPosY)
            {
                if (m_dPosX >= paddle.m_dPosX - paddle.m_dWidth * 0.5 && m_dPosX <= paddle.m_dPosX + paddle.m_dWidth * 0.5)
                    result = true;
                else if (Math.Sqrt(Math.Pow(m_dPosX - (paddle.m_dPosX - paddle.m_dWidth * 0.5), 2) + Math.Pow(0, 2)) <= m_dRadius)
                    result = true;
                else if (Math.Sqrt(Math.Pow(m_dPosX - (paddle.m_dPosX + paddle.m_dWidth * 0.5), 2) + Math.Pow(0, 2)) <= m_dRadius)
                    result = true;
            }
            return result;
        }

        // Create a circle and add it to the canvas
        public override void InitIcon(Canvas canvas)
        {
            m_IconBall = new Ellipse();
            m_IconBall.Stroke = System.Windows.Media.Brushes.Black;
            m_IconBall.StrokeThickness = 1.0;
            m_IconBall.Width = m_dRadius;
            m_IconBall.Height = m_dRadius;
            m_IconBall.Fill = System.Windows.Media.Brushes.White;

            canvas.Children.Add(m_IconBall);
            Draw();
        }
    }
}
