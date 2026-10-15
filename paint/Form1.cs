using System;
using System.Drawing;
using System.Windows.Forms;

namespace paint
{
    public partial class Form1 : Form
    {
        private Bitmap bmp;
        private Graphics g;
        private Pen p = new Pen(Color.Black, 2);
        private bool draw = false;
        private int x, y;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int w = Math.Max(1, this.ClientRectangle.Width);
            int h = Math.Max(1, this.ClientRectangle.Height);

            bmp = new Bitmap(w, h);
            g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            if (numericUpDown1 != null)
            {
                p.Width = (float)numericUpDown1.Value;
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (bmp != null)
            {
                e.Graphics.DrawImage(bmp, 0, 0);
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                draw = true;
                x = e.X;
                y = e.Y;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (draw && g != null && p != null)
            {
                g.DrawLine(p, x, y, e.X, e.Y);
                x = e.X;
                y = e.Y;
                this.Invalidate();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            draw = false;
        }

        
        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                p.Color = colorDialog1.Color;
            }
        }

       
        private void colorToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                p.Color = colorDialog1.Color;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (p != null && numericUpDown1 != null)
            {
                p.Width = (float)numericUpDown1.Value;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

       
        private void copyToolStripButton_Click(object sender, EventArgs e) { }
        private void newToolStripButton_Click(object sender, EventArgs e) { }
        private void openToolStripButton_Click(object sender, EventArgs e) { }
        private void saveToolStripButton_Click(object sender, EventArgs e) { }
        private void printToolStripButton_Click(object sender, EventArgs e) { }
        private void cutToolStripButton_Click(object sender, EventArgs e) { }
        private void pasteToolStripButton_Click(object sender, EventArgs e) { }
        private void helpToolStripButton_Click(object sender, EventArgs e) { }
        private void toolStripButton1_Click(object sender, EventArgs e) { }
    }
}