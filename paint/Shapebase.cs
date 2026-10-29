using System;
using System.Drawing;

namespace ShapeApp
{
    // Бүх дүрсийн суурь класс
    public abstract class Shape
    {
        protected static readonly Random Rnd = new Random();

        public int X, Y, Width, Height;
        public Color Color;

        protected Shape(Rectangle b, Color c)
        {
            X = b.X; Y = b.Y; Width = b.Width; Height = b.Height;
            Color = c;
        }

        public virtual string Name { get { return "Shape"; } }

        // Дэд класс төгсгөлд нь base.Draw(g) дуудаж дүрсийн нэрийг бичүүлнэ
        public virtual void Draw(Graphics g)
        {
            using (var font = new Font("Segoe UI", 9))
            using (var brush = new SolidBrush(Color))
            {
                g.DrawString(Name, font, brush, X, Y + Height + 4);
            }
        }

        protected Pen MakePen() { return new Pen(Color, 2); }
        protected Brush MakeFill() { return new SolidBrush(Color.FromArgb(70, Color)); }
    }
}