using System.Drawing;

namespace ShapeApp
{
    public class LineShape : Shape
    {
        private readonly bool _down;   // аль диагоналиар татах

        public LineShape(Rectangle b, Color c) : base(b, c)
        {
            _down = Rnd.Next(2) == 0;  // санамсаргүй утгыг энд НЭГ удаа
        }

        public override string Name { get { return "Шулуун"; } }

        public override void Draw(Graphics g)
        {
            using (var pen = MakePen())
            {
                if (_down)
                    g.DrawLine(pen, X, Y, X + Width, Y + Height);
                else
                    g.DrawLine(pen, X, Y + Height, X + Width, Y);
            }
            base.Draw(g);
        }
    }
}