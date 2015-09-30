using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BallPaddle.Widget
{
    // A drawable item on the canvas
    public abstract class Widget
    {
        // Starting position
        public double dStartX;
        public double dStartY;

        // Current position
        public double dPosX;
        public double dPosY;

        // The representation of the widget
        public FrameworkElement icon;

        // Reset starting position/properties
        public abstract void Reset();

        // Update icon on its parent canvas
        public abstract void Draw();

        // Add the icon to the canvas
        public abstract void InitIcon(Canvas canvas);

        // Remove from canvas
        public virtual void Destroy()
        {
            if (icon != null)
            {
                if (icon.Parent as Canvas != null)
                    (icon.Parent as Canvas).Children.Remove(icon);

                icon = null;
            }
        }
    }
}
