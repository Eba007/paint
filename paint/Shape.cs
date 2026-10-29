using System.Drawing;

namespace paint
{
   
    public abstract class Shape
    {
        public Color Color;     
        public float Width;     
        public Point Start;     
        public Point End;       

        public Shape(Color color, float width, Point start)
        {
            Color = color;
            Width = width;
            Start = start;
            End = start;
        }

        
        public abstract void Draw(Graphics g);
    }
}