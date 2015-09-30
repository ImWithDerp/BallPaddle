using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace BallPaddle.Widget
{
    // The paddle players control to bounce balls
    public class WidgetPaddle : Widget
    {
        // Physical properties
        public double m_dWidth;
        public double m_dAccel;
        public double m_dDecel;
        public double m_dAngle;
        public double m_dFriction;

        // Icon is a simple line
        public Line IconLine
        {
            get
            {
                return icon as Line;
            }

            set
            {
                icon = value;
            }
        }
        
        // Current physical state
        public double m_dMinX;
        public double m_dMaxX;

        public double m_dVelX;

        // Init default properties, some depend on the size of the canvas and must be specified
        public WidgetPaddle (double dStartX, double dStartY, double dMinX, double dMaxX,
            double dWidth = 100.0, double dAccel = 0.5, double dDecel = 0.2, double dAngle = 0.05, double dFriction = 0.3)
        {
            this.dStartX = dStartX;
            this.dStartY = dStartY;
            this.m_dMinX = dMinX + dWidth * 0.5;
            this.m_dMaxX = dMaxX - dWidth * 0.5;

            this.m_dWidth = dWidth;
            this.m_dAccel = dAccel;
            this.m_dDecel = dDecel;
            this.m_dAngle = dAngle;
            this.m_dFriction = dFriction;

            Reset();
        }

        // Set to start
        public override void Reset()
        {
            dPosX = dStartX;
            dPosY = dStartY;
            m_dVelX = 0.0;
        }

        // Adjust position/velocity based on whether left key and/or right inputs are given
        public void Update(bool left, bool right)
        {
            // Accelerate to the right, if right is pressed
            if (right)
                m_dVelX += m_dAccel;

            // Accelerate to the left, if left is pressed
            if (left)
                m_dVelX -= m_dAccel;

            // Apply deceleration
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

            // Update position
            dPosX += m_dVelX;

            // Enforce min/max bounds
            if (dPosX > m_dMaxX)
            {
                dPosX = m_dMaxX;
                if (m_dVelX > 0)
                    m_dVelX = 0;
            }
            else if (dPosX < m_dMinX)
            {
                dPosX = m_dMinX;
                if (m_dVelX < 0)
                    m_dVelX = 0;
            }            
        }

        // Create line and add to canvas
        public override void InitIcon(Canvas canvas)
        {
            IconLine = new Line();
            IconLine.Stroke = System.Windows.Media.Brushes.Black;
            IconLine.StrokeThickness = 2.0;

            canvas.Children.Add(icon);
            Draw();
        }

        // Update line location
        public override void Draw()
        {
            IconLine.X1 = dPosX - m_dWidth * 0.5;
            IconLine.X2 = dPosX + m_dWidth * 0.5;

            IconLine.Y1 = IconLine.Y2 = dStartY;
        }


    }
}
