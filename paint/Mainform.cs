using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ShapeApp
{
    // Цонхны хэмжээ өөрчлөгдөхөд анивчихгүй зурах Panel
    public class Canvas : Panel
    {
        public Canvas()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
        }
    }

    public class MainForm : Form
    {
        private readonly List<Shape> shapes = new List<Shape>();
        private readonly Random rnd = new Random();
        private readonly Canvas canvas = new Canvas();

        public MainForm()
        {
            Text = "Дасгал №2. Энгийн дүрсүүд зурах";
            ClientSize = new Size(820, 560);

            MenuStrip menu = new MenuStrip();
            ToolStripMenuItem root = new ToolStripMenuItem("Дүрс");
            menu.Items.Add(root);

            AddMenu(root, "Шулуун", (b, c) => new LineShape(b, c));
            AddMenu(root, "Тэгш өнцөгт", (b, c) => new RectangleShape(b, c));
            AddMenu(root, "Дугуй", (b, c) => new CircleShape(b, c));
            AddMenu(root, "Pie", (b, c) => new PieShape(b, c));
            AddMenu(root, "Polygon", (b, c) => new PolygonShape(b, c));
            AddMenu(root, "Curve", (b, c) => new CurveShape(b, c));

            root.DropDownItems.Add(new ToolStripSeparator());
            ToolStripMenuItem clear = new ToolStripMenuItem("Цэвэрлэх");   // хамгийн доор
            clear.Click += (s, e) => { shapes.Clear(); canvas.Invalidate(); };
            root.DropDownItems.Add(clear);

            canvas.Dock = DockStyle.Fill;
            canvas.Paint += canvas_Paint;

            Controls.Add(canvas);
            Controls.Add(menu);
            MainMenuStrip = menu;
        }

        private void AddMenu(ToolStripMenuItem parent, string text, Func<Rectangle, Color, Shape> make)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.Click += (s, e) => AddShape(make);
            parent.DropDownItems.Add(item);
        }

        // Санамсаргүй байрлал, хэмжээ, өнгө үүсгээд дүрсийг жагсаалтад нэмнэ
        private void AddShape(Func<Rectangle, Color, Shape> make)
        {
            int w = rnd.Next(70, 170);
            int h = rnd.Next(60, 140);
            int maxX = Math.Max(11, canvas.ClientSize.Width - w - 10);
            int maxY = Math.Max(11, canvas.ClientSize.Height - h - 30);
            Rectangle b = new Rectangle(rnd.Next(10, maxX), rnd.Next(10, maxY), w, h);
            Color c = Color.FromArgb(rnd.Next(40, 220), rnd.Next(40, 220), rnd.Next(40, 220));

            shapes.Add(make(b, c));
            canvas.Invalidate();
        }

        // Полиморфизм: ямар дүрс байсан ч нэг foreach-ээр зурна
        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            foreach (Shape sh in shapes)
            {
                sh.Draw(e.Graphics);
            }
        }
    }
}