using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace BallPaddle.Widget
{
    public delegate void BallEventDelegate(object sender, WidgetBall ball);

    public class WidgetBall : Widget
    {
        // Physical properties
        public double m_dRadius;
        public double m_dGravity;

        // Icon is an ellipse (circular)
        public Ellipse IconBall
        {
            get
            {
                return icon as Ellipse;
            }

            set
            {
                icon = value;
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
        public WidgetBall(double dStartX, double dStartY, double dMinX, double dMaxX, double dMinY, double dMaxY,
             BallEventDelegate onReset = null, BallEventDelegate onCollide = null, BallEventDelegate onFall = null,
             double dRadius = 20.0, double dGravity = 0.1)
        {
            this.dStartX = dStartX;
            this.dStartY = dStartY;
            this.m_dMinX = dMinX + dRadius;
            this.m_dMaxX = dMaxX - dRadius;
            this.m_dMinY = dMinY;
            this.m_dMaxY = dMaxY;

            this.m_dRadius = dRadius;
            this.m_dGravity = dGravity;

            this.m_OnReset = onReset;
            this.m_OnCollide = onCollide;
            this.m_OnFall = onFall;

            Reset();
        }

        // Update circle location
        public override void Draw()
        {
            if (IconBall != null)
               IconBall.Margin = new System.Windows.Thickness(dPosX - m_dRadius*.5, dPosY - m_dRadius*.5, 0, 0);
        }
        
        public override void Reset()
        {
            dPosX = dStartX;
            dPosY = dStartY;
            m_dVelX = 0;
            m_dVelY = 0;

            if (m_OnReset != null)
                m_OnReset(this, this);
        }

        // Shift position and velocity by a random amount, ranging from no change to maxDist and maxVel
        public void RandomOffsetAndVelocity(double maxDist, double maxVel)
        {
            dPosX += (MainWindow.random.NextDouble() - 0.5) * maxDist * 2.0;

            m_dVelX += (MainWindow.random.NextDouble() - 0.5) * maxVel * 2.0;
        }

        // Update ball's position and velocity, checking collision with the current paddle
        public void Update(WidgetPaddle paddle)
        {
            // Apply gravity
            m_dVelY += m_dGravity;

            // Update position
            dPosX += m_dVelX;
            dPosY += m_dVelY;

            if (dPosY > m_dMaxY)
            {
                // Check and handle falling below play area
                Reset();

                if (m_OnFall != null)
                    m_OnFall(this, this);
            }
            else if (Collide(paddle))
            {
                // Check and handle collision with the paddle

                // Reverse vertical velocity
                m_dVelY *= -1;
                dPosY = paddle.dPosY - m_dRadius;

                // Make the ball bounce off a bit based on distance from centre of paddle to make things interesting
                m_dVelX += (dPosX - paddle.dPosX) * paddle.m_dAngle;

                // Bring the ball's velocity closer to the paddle's
                //m_dVelX = paddle.m_dVelX * paddle.m_dFriction + m_dVelX * (paddle.m_dFriction - 1);

                if (m_OnCollide != null)
                    m_OnCollide(this, this);
            }

            // Enforce horizontal bounds, bouncing off if violated
            if (dPosX > m_dMaxX)
            {
                dPosX = m_dMaxX;
                m_dVelX *= -1;
            }
            else if (dPosX < m_dMinX)
            {
                dPosX = m_dMinX;
                m_dVelX *= -1;
            }
        }

        // Check collision with paddle
        public bool Collide (WidgetPaddle paddle)
        {
            bool result = false;

            // This isn't normally how you check if a circle intersects with a line, I'm cheating for now
            if (dPosY + m_dRadius >= paddle.dPosY && dPosY <= paddle.dPosY)
            {
                if (dPosX >= paddle.dPosX - paddle.m_dWidth * 0.5 && dPosX <= paddle.dPosX + paddle.m_dWidth * 0.5)
                    result = true;
                else if (Math.Sqrt(Math.Pow(dPosX - (paddle.dPosX - paddle.m_dWidth * 0.5), 2) + Math.Pow(0, 2)) <= m_dRadius)
                    result = true;
                else if (Math.Sqrt(Math.Pow(dPosX - (paddle.dPosX + paddle.m_dWidth * 0.5), 2) + Math.Pow(0, 2)) <= m_dRadius)
                    result = true;
            }
            return result;
        }

        // Create a circle and add it to the canvas
        public override void InitIcon(Canvas canvas)
        {
            IconBall = new Ellipse();
            IconBall.Stroke = System.Windows.Media.Brushes.Black;
            IconBall.StrokeThickness = 1.0;
            IconBall.Width = m_dRadius;
            IconBall.Height = m_dRadius;
            IconBall.Fill = System.Windows.Media.Brushes.White;

            canvas.Children.Add(IconBall);
            Draw();
        }
    }
}
