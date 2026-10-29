using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace paint
{
    public abstract class PaintShape
    {
        public Color Color;
        public float Width;
        public Point Start;
        public Point End;

        protected PaintShape(Color color, float width, Point start)
        {
            Color = color;
            Width = width;
            Start = start;
            End = start;
        }

        public abstract void Draw(Graphics g);

        protected Rectangle Box
        {
            get
            {
                return new Rectangle(
                    Math.Min(Start.X, End.X), Math.Min(Start.Y, End.Y),
                    Math.Abs(Start.X - End.X), Math.Abs(Start.Y - End.Y));
            }
        }

        protected Pen MakePen()
        {
            Pen p = new Pen(Color, Width);
            p.StartCap = LineCap.Round;
            p.EndCap = LineCap.Round;
            p.LineJoin = LineJoin.Round;
            return p;
        }
    }

    // Шулуун
    public class PLine : PaintShape
    {
        public PLine(Color c, float w, Point s) : base(c, w, s) { }

        public override void Draw(Graphics g)
        {
            using (Pen p = MakePen())
                g.DrawLine(p, Start, End);
        }
    }

    // Тэгш өнцөгт
    public class PRect : PaintShape
    {
        public PRect(Color c, float w, Point s) : base(c, w, s) { }

        public override void Draw(Graphics g)
        {
            using (Pen p = MakePen())
                g.DrawRectangle(p, Box);
        }
    }

    // Дугуй (эллипс)
    public class PEllipse : PaintShape
    {
        public PEllipse(Color c, float w, Point s) : base(c, w, s) { }

        public override void Draw(Graphics g)
        {
            Rectangle r = Box;
            if (r.Width < 1 || r.Height < 1) return;
            using (Pen p = MakePen())
            using (SolidBrush b = new SolidBrush(Color.FromArgb(60, Color)))
            {
                g.FillEllipse(b, r);
                g.DrawEllipse(p, r);
            }
        }
    }

    // Нум
    public class PArc : PaintShape
    {
        public PArc(Color c, float w, Point s) : base(c, w, s) { }

        public override void Draw(Graphics g)
        {
            Rectangle r = Box;
            if (r.Width < 1 || r.Height < 1) return;
            using (Pen p = MakePen())
                g.DrawArc(p, r, 180, 180);   // дээд талын хагас нум
        }
    }

    // Дугуй график (pie chart)
    public class PPie : PaintShape
    {
        private static readonly float[] angles = { 100, 140, 70, 50 };   // нийт 360
        private static readonly Color[] colors =
        {
            Color.FromArgb(120, 70, 130, 220),
            Color.FromArgb(120, 230, 90, 90),
            Color.FromArgb(120, 90, 190, 110),
            Color.FromArgb(120, 240, 200, 60)
        };

        public PPie(Color c, float w, Point s) : base(c, w, s) { }

        public override void Draw(Graphics g)
        {
            Rectangle r = Box;
            if (r.Width < 1 || r.Height < 1) return;

            using (Pen p = MakePen())
            {
                float start = 0;
                for (int i = 0; i < angles.Length; i++)
                {
                    using (SolidBrush b = new SolidBrush(colors[i]))
                        g.FillPie(b, r, start, angles[i]);
                    g.DrawPie(p, r, start, angles[i]);
                    start += angles[i];
                }
            }
        }
    }

    // Олон талт дүрс
    public class PPolygon : PaintShape
    {
        public List<Point> Points = new List<Point>();
        public bool Closed = false;

        public PPolygon(Color c, float w, Point s) : base(c, w, s)
        {
            Points.Add(s);
        }

        public override void Draw(Graphics g)
        {
            List<Point> pts = new List<Point>(Points);
            if (!Closed) pts.Add(End);          // дуусаагүй үед хулганыг дагана

            using (Pen p = MakePen())
            {
                if (pts.Count >= 3)
                {
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(60, Color)))
                        g.FillPolygon(b, pts.ToArray());
                    g.DrawPolygon(p, pts.ToArray());
                }
                else if (pts.Count == 2)
                {
                    g.DrawLine(p, pts[0], pts[1]);
                }
            }
        }
    }
}