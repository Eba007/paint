using System.Drawing;
using System.Drawing.Drawing2D;

namespace paint
{
    
    public class LineDraw : Shape
    {
        public LineDraw(Color color, float width, Point start)
            : base(color, width, start) { }

        public override void Draw(Graphics g)
        {
            using (Pen pen = new Pen(Color, Width))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawLine(pen, Start, End);
            }
        }
    }
}