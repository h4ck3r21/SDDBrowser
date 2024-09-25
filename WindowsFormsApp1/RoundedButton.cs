using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.UI.Xaml;

namespace SDDBrowser
{
    internal class RoundedButton : Button
    {
        readonly int Radius;
        readonly float Thickness;
        Color BorderColor;

        internal RoundedButton(int radius, float thickness)
        {
            Radius = radius;
            Thickness = thickness;
            BorderColor = ForeColor;
        }

        GraphicsPath GetRoundPath(RectangleF Rect, int radius)
        {
            float r2 = radius / 2f;
            GraphicsPath GraphPath = new GraphicsPath();
            GraphPath.AddArc(Rect.X, Rect.Y, radius, radius, 180, 90);
            GraphPath.AddLine(Rect.X + r2, Rect.Y, Rect.Width - r2, Rect.Y);
            GraphPath.AddArc(Rect.X + Rect.Width - radius, Rect.Y, radius, radius, 270, 90);
            GraphPath.AddLine(Rect.Width, Rect.Y + r2, Rect.Width, Rect.Height - r2);
            GraphPath.AddArc(Rect.X + Rect.Width - radius,
                             Rect.Y + Rect.Height - radius, radius, radius, 0, 90);
            GraphPath.AddLine(Rect.Width - r2, Rect.Height, Rect.X + r2, Rect.Height);
            GraphPath.AddArc(Rect.X, Rect.Y + Rect.Height - radius, radius, radius, 90, 90);
            GraphPath.AddLine(Rect.X, Rect.Height - r2, Rect.X, Rect.Y + r2);
            GraphPath.CloseFigure();
            return GraphPath;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Brush BackBrush = new SolidBrush(BackColor);
            e.Graphics.FillRectangle(BackBrush, new Rectangle(0, 0, Width, Height));
            BackBrush.Dispose();
            Brush brush = new SolidBrush(ForeColor);
            SizeF size = e.Graphics.MeasureString(Text, Font);
            float x = (Width - size.Width) / 2;
            float y = (Height - size.Height) / 2;
            e.Graphics.DrawString(Text, Font, brush, new PointF(x, y));
            brush.Dispose();


            RectangleF Rect = new RectangleF(0, 0, Width, Height);
            using (GraphicsPath GraphPath = GetRoundPath(Rect, Radius))
            {
                Region = new Region(GraphPath);
                using (Pen pen = new Pen(BorderColor, Thickness))
                {
                    pen.Alignment = PenAlignment.Inset;
                    e.Graphics.DrawPath(pen, GraphPath);
                }
            }
        }

        public void SetBorderColor(Color color)
        {
            BorderColor = color;
            Invalidate();
        }
    }
}
