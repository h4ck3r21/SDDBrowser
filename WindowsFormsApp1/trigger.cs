using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public Point getPointOnScreen()
        {
            Point point = new Point();
            point.X = rectangle.Left + form.Left;
            point.Y = rectangle.Top + form.Top;
            return point;
        }

        public Rectangle getRectangleOnScreen()
        {
            Rectangle r = new Rectangle();
            r.X = rectangle.Left + form.Left;
            r.Y = rectangle.Top + form.Top;
            r.Width = rectangle.Width;
            r.Height = rectangle.Height;
            return r;
        }
    }
}
