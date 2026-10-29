using System.Drawing;

namespace ShapeApp
{
    public class PieShape : Shape
    {
        private readonly float _startAngle;   // эхлэх өнцөг (градус)
        private readonly float _sweepAngle;   // хамрах өнцөг (градус)

        public PieShape(Rectangle b, Color c) : base(b, c)
        {
            // санамсаргүй утгыг байгуулагчид НЭГ удаа үүсгэнэ
            _startAngle = Rnd.Next(0, 360);
            _sweepAngle = Rnd.Next(90, 271);
        }

        public override string Name { get { return "Pie"; } }

        public override void Draw(Graphics g)
        {
            using (var fill = MakeFill())
            using (var pen = MakePen())
            {
                g.FillPie(fill, X, Y, Width, Height, _startAngle, _sweepAngle);
                g.DrawPie(pen, X, Y, Width, Height, _startAngle, _sweepAngle);
            }
            base.Draw(g);
        }
    }
}