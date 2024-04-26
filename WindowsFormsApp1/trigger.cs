using System.Drawing;
using System.Windows.Forms;

namespace SDDBrowser
{
    public class Trigger
    {
        public Rectangle rectangle;
        public Size Size;
        public Point Location;
        public string Name;
        public Form form;

        public Trigger(string name, Rectangle r, Form form)
        {
            Name = name;
            rectangle = r;
            Size = new Size(r.Width, r.Height);
            Location = new Point(r.Left, r.Top);
            this.form = form;
        }

        public Point GetPointOnScreen()
        {
            Point point = new Point
            {
                X = rectangle.Left + form.Left,
                Y = rectangle.Top + form.Top
            };
            return point;
        }

        public Rectangle GetRectangleOnScreen()
        {
            Rectangle r = new Rectangle
            {
                X = rectangle.Left + form.Left,
                Y = rectangle.Top + form.Top,
                Width = rectangle.Width,
                Height = rectangle.Height
            };
            return r;
        }
    }
}
