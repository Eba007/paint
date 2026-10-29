using System.Drawing;

namespace ShapeApp
{
    public class CurveShape : Shape
    {
        // Bounds-ийн дотор хэмжээсийн хувиар (0..1) хадгална.
        // Ингэснээр цонх дахин зурагдахад муруй хэлбэрээ хадгална.
        private readonly float[] _ty;

        public CurveShape(Rectangle b, Color c) : base(b, c)
        {
            _ty = new float[5];
            for (int i = 0; i < _ty.Length; i++)
                _ty[i] = (float)Rnd.NextDouble();      // санамсаргүй өндөр
        }

        public override string Name { get { return "Curve"; } }

        public override void Draw(Graphics g)
        {
            PointF[] pts = new PointF[_ty.Length];
            for (int i = 0; i < pts.Length; i++)
            {
                pts[i] = new PointF(
                    X + Width * i / (float)(pts.Length - 1),
                    Y + Height * _ty[i]);
            }

            using (var pen = MakePen())
            {
                g.DrawCurve(pen, pts);     // битүү биш тул зөвхөн хүрээ
            }
            base.Draw(g);
        }
    }
}