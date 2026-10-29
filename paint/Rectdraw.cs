using System;
using System.Drawing;

namespace paint
{
    
    public class RectDraw : Shape
    {
        public RectDraw(Color color, float width, Point start)
            : base(color, width, start) { }

        public override void Draw(Graphics g)
        {
            
            int x = Math.Min(Start.X, End.X);
            int y = Math.Min(Start.Y, End.Y);
            int w = Math.Abs(End.X - Start.X);
            int h = Math.Abs(End.Y - Start.Y);

            using (Pen pen = new Pen(Color, Width))
            {
                g.DrawRectangle(pen, x, y, w, h);
            }
        }
    }
}