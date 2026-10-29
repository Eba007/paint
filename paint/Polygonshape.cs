using System;
using System.Drawing;

namespace ShapeApp
{
    public class PolygonShape : Shape
    {
        private readonly int _sides;           // талын тоо
        private readonly double _rotation;     // эхний оройн өнцөг (радиан)

        public PolygonShape(Rectangle b, Color c) : base(b, c)
        {
            _sides = Rnd.Next(3, 9);                       // 3-аас 8 тал
            _rotation = Rnd.NextDouble() * Math.PI * 2;
        }

        public override string Name { get { return "Polygon"; } }

        // Bounds-ийн дотор багтсан эллипс дээрх орой бүрийн цэг
        private PointF[] GetPoints()
        {
            PointF[] pts = new PointF[_sides];
            float cx = X + Width / 2f;
            float cy = Y + Height / 2f;
            float rx = Width / 2f;
            float ry = Height / 2f;
            for (int i = 0; i < _sides; i++)
            {
                double a = _rotation + 2 * Math.PI * i / _sides;
                pts[i] = new PointF(cx + (float)(rx * Math.Cos(a)),
                                    cy + (float)(ry * Math.Sin(a)));
            }
            return pts;
        }

        public override void Draw(Graphics g)
        {
            PointF[] pts = GetPoints();
            using (var fill = MakeFill())
            using (var pen = MakePen())
            {
                g.FillPolygon(fill, pts);
                g.DrawPolygon(pen, pts);
            }
            base.Draw(g);
        }
    }
}