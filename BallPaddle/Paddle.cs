using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace BallPaddle
{
    // The paddle players control to bounce balls
    public class Paddle : Widget
    {
        // Physical properties
        public double m_dWidth;
        public double m_dAccel;
        public double m_dDecel;
        public double m_dAngle;
        public double m_dFriction;

        // Icon is a simple line
        public Line m_IconLine
        {
            get
            {
                return m_Icon as Line;
            }

            set
            {
                m_Icon = value;
            }
        }
        
        // Current physical state
        public double m_dMinX;
        public double m_dMaxX;

        public double m_dVelX;

        // Init default properties, some depend on the size of the canvas and must be specified
        public Paddle (double dStartX, double dStartY, double dMinX, double dMaxX,
            double dWidth = 100.0, double dAccel = 0.5, double dDecel = 0.2, double dAngle = 0.05, double dFriction = 0.3)
        {
            m_dStartX = dStartX;
            m_dStartY = dStartY;
            m_dMinX = dMinX + dWidth * 0.5;
            m_dMaxX = dMaxX - dWidth * 0.5;

            m_dWidth = dWidth;
            m_dAccel = dAccel;
            m_dDecel = dDecel;
            m_dAngle = dAngle;
            m_dFriction = dFriction;

            Reset();
        }

        // Set to start
        public override void Reset()
        {
            m_dPosX = m_dStartX;
            m_dPosY = m_dStartY;
            m_dVelX = 0.0;
        }

        // Adjust position/velocity based on whether left key and/or right inputs are given
        public void Update(bool left, bool right)
        {
            if (right)
                m_dVelX += m_dAccel;

            if (left)
                m_dVelX -= m_dAccel;
            
            if (m_dVelX > m_dDecel)
            {
                m_dVelX -= m_dDecel;
            }
            else if (m_dVelX < -m_dDecel)
            {
                m_dVelX += m_dDecel;
            }
            else
                m_dVelX = 0.0;

            m_dPosX += m_dVelX;

            if (m_dPosX > m_dMaxX)
            {
                m_dPosX = m_dMaxX;
                if (m_dVelX > 0)
                    m_dVelX = 0;
            }
            else if (m_dPosX < m_dMinX)
            {
                m_dPosX = m_dMinX;
                if (m_dVelX < 0)
                    m_dVelX = 0;
            }            
        }

        // Create line and add to canvas
        public override void InitIcon(Canvas canvas)
        {
            m_IconLine = new Line();
            m_IconLine.Stroke = System.Windows.Media.Brushes.Black;
            m_IconLine.StrokeThickness = 2.0;

            canvas.Children.Add(m_Icon);
            Draw();
        }

        // Update line location
        public override void Draw()
        {
            m_IconLine.X1 = m_dPosX - m_dWidth * 0.5;
            m_IconLine.X2 = m_dPosX + m_dWidth * 0.5;

            m_IconLine.Y1 = m_IconLine.Y2 = m_dStartY;
        }


    }
}
