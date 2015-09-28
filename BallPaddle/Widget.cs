using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BallPaddle
{
    // A drawable item on the canvas
    public abstract class Widget
    {
        // Starting position
        public double m_dStartX;
        public double m_dStartY;

        // Current position
        public double m_dPosX;
        public double m_dPosY;

        // The representation of the widget
        public FrameworkElement m_Icon;

        // Reset starting position/properties
        public abstract void Reset();

        // Update icon on its parent canvas
        public abstract void Draw();

        // Add the icon to the canvas
        public abstract void InitIcon(Canvas canvas);

        // Remove from canvas
        public virtual void Destroy()
        {
            if (m_Icon != null)
            {
                if (m_Icon.Parent as Canvas != null)
                    (m_Icon.Parent as Canvas).Children.Remove(m_Icon);

                m_Icon = null;
            }
        }
    }
}
