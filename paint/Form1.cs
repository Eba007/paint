using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
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

        private string tool = "free";      // free | line | rect | ellipse | arc | polygon | pie
        private PaintShape current;             // одоо чирж буй дүрс

        private ToolStripMenuItem freeItem, lineItem, rectItem, ellipseItem, arcItem, pieItem, polyItem;

        private List<Bitmap> undoList = new List<Bitmap>();
        private List<Bitmap> redoList = new List<Bitmap>();
        private string currentFile = null;
        private bool selectAll = false;

        public Form1()
        {
            InitializeComponent();
            Resize -= Form1_Resize;
            Resize += Form1_Resize;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;

            int w = Math.Max(1, this.ClientRectangle.Width);
            int h = Math.Max(1, this.ClientRectangle.Height);

            bmp = new Bitmap(w, h);
            g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            p.StartCap = LineCap.Round;
            p.EndCap = LineCap.Round;

            if (numericUpDown1 != null)
            {
                numericUpDown1.Minimum = 1;
                numericUpDown1.Maximum = 10;
                p.Width = (float)numericUpDown1.Value;
            }

            BuildToolMenus();
            WireStandardMenus();
            SetTool("free");
        }

        // =====================================================
        //  ЦЭС ХОЛБОХ
        // =====================================================

        private void BuildToolMenus()
        {
            freeItem = GetOrCreate("freeToolStripMenuItem", "Чөлөөт", "Tools", (s, e) => SetTool("free"));
            lineItem = GetOrCreate("customizeToolStripMenuItem", "Шулуун", "Tools", (s, e) => SetTool("line"));
            rectItem = GetOrCreate("optionsToolStripMenuItem", "Тэгш өнцөгт", "Tools", (s, e) => SetTool("rect"));
            ellipseItem = GetOrCreate("ellipseToolStripMenuItem", "Дугуй", "Tools", (s, e) => SetTool("ellipse"));
            arcItem = GetOrCreate("arcToolStripMenuItem", "Нум", "Tools", (s, e) => SetTool("arc"));
            polyItem = GetOrCreate("polygonToolStripMenuItem", "Олон талт дүрс", "Tools", (s, e) => SetTool("polygon"));
            pieItem = GetOrCreate("pieToolStripMenuItem", "Дугуй график", "Tools", (s, e) => SetTool("pie"));
            GetOrCreate("clearToolStripMenuItem", "Clear", "Graphic", (s, e) => ClearCanvas());

            // Дасгал №2 (ShapeApp) цонхыг нээнэ
            GetOrCreate("shapeLabToolStripMenuItem", "Дасгал №2", "Tools", (s, e) => new ShapeApp.MainForm().Show());
        }

        private void WireStandardMenus()
        {
            // File
            Wire(NewFile, "newToolStripMenuItem", "New");
            Wire(OpenFile, "openToolStripMenuItem", "Open");
            Wire(SaveFile, "saveToolStripMenuItem", "Save");
            Wire(SaveFileAs, "saveAsToolStripMenuItem", "Save As");
            Wire(PrintCanvas, "printToolStripMenuItem", "Print");
            Wire(PrintPreviewCanvas, "printPreviewToolStripMenuItem", "Print Preview");
            Wire(Close, "exitToolStripMenuItem", "Exit", "Close");

            // Edit
            Wire(Undo, "undoToolStripMenuItem", "Undo");
            Wire(Redo, "redoToolStripMenuItem", "Redo");
            Wire(CutCanvas, "cutToolStripMenuItem", "Cut");
            Wire(CopyCanvas, "copyToolStripMenuItem", "Copy");
            Wire(PasteCanvas, "pasteToolStripMenuItem", "Paste");
            Wire(SelectAllCanvas, "selectAllToolStripMenuItem", "Select All");

            // Help
            Wire(ShowHelp, "contentsToolStripMenuItem", "Contents");
            Wire(ShowHelp, "indexToolStripMenuItem", "Index");
            Wire(ShowHelp, "searchToolStripMenuItem", "Search");
            Wire(ShowAbout, "aboutToolStripMenuItem", "About");
        }

        private IEnumerable<ToolStripItem> AllItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem it in items)
            {
                yield return it;
                if (it is ToolStripDropDownItem d && d.HasDropDownItems)
                {
                    foreach (ToolStripItem sub in AllItems(d.DropDownItems))
                        yield return sub;
                }
            }
        }

        private static string Norm(string s)
        {
            return (s ?? "").Replace("&", "").Trim().TrimEnd('.').Trim();
        }

        private static bool IsParent(ToolStripItem it)
        {
            return it is ToolStripDropDownItem d && d.HasDropDownItems;
        }

        private void Wire(Action action, string name, params string[] texts)
        {
            foreach (ToolStripItem it in AllItems(menuStrip1.Items))
            {
                bool match = (it.Name == name);
                if (!match && !IsParent(it))
                {
                    foreach (string t in texts)
                    {
                        if (string.Equals(Norm(it.Text), t, StringComparison.OrdinalIgnoreCase))
                            match = true;
                    }
                }
                if (match)
                    it.Click += (s, e) => action();
            }
        }

        private ToolStripMenuItem GetOrCreate(string name, string text, string parentText, EventHandler click)
        {
            ToolStripMenuItem item = null;
            ToolStripItem[] found = menuStrip1.Items.Find(name, true);
            if (found.Length > 0) item = found[0] as ToolStripMenuItem;

            if (item == null)
            {
                ToolStripMenuItem parent = null;
                foreach (ToolStripItem t in menuStrip1.Items)
                {
                    if (t is ToolStripMenuItem m && Norm(m.Text) == parentText)
                        parent = m;
                }
                if (parent == null)
                {
                    parent = new ToolStripMenuItem(parentText);
                    menuStrip1.Items.Add(parent);
                }
                item = new ToolStripMenuItem();
                item.Name = name;
                parent.DropDownItems.Add(item);
            }

            item.Text = text;
            item.Click -= click;
            item.Click += click;
            return item;
        }

        private void SetTool(string t)
        {
            FinishPolygon();            // дуусаагүй олон талт дүрс байвал дуусгана
            tool = t;
            freeItem.Checked = (t == "free");
            lineItem.Checked = (t == "line");
            rectItem.Checked = (t == "rect");
            ellipseItem.Checked = (t == "ellipse");
            arcItem.Checked = (t == "arc");
            polyItem.Checked = (t == "polygon");
            pieItem.Checked = (t == "pie");
        }

        private void FinishPolygon()
        {
            PPolygon poly = current as PPolygon;
            if (poly == null) return;

            poly.Closed = true;
            if (poly.Points.Count >= 2) poly.Draw(g);
            current = null;
            this.Invalidate();
        }

        // =====================================================
        //  Bitmap, Undo / Redo туслахууд
        // =====================================================

        private void EnsureSize(int w, int h)
        {
            w = Math.Max(bmp.Width, w);
            h = Math.Max(bmp.Height, h);
            if (w == bmp.Width && h == bmp.Height) return;

            Bitmap nb = new Bitmap(w, h);
            using (Graphics ng = Graphics.FromImage(nb))
            {
                ng.Clear(Color.White);
                ng.DrawImageUnscaled(bmp, 0, 0);
            }
            g.Dispose();
            bmp.Dispose();
            bmp = nb;
            g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (bmp == null || this.WindowState == FormWindowState.Minimized) return;
            EnsureSize(this.ClientSize.Width, this.ClientSize.Height);
        }

        private Bitmap GetCanvasImage()
        {
            int top = toolStrip1.Bottom;
            int w = Math.Max(1, Math.Min(this.ClientSize.Width, bmp.Width));
            int h = Math.Max(1, Math.Min(this.ClientSize.Height, bmp.Height) - top);
            Rectangle r = new Rectangle(0, top, w, h);

            Bitmap img = new Bitmap(w, h);
            using (Graphics ig = Graphics.FromImage(img))
            {
                ig.DrawImage(bmp, new Rectangle(0, 0, w, h), r, GraphicsUnit.Pixel);
            }
            return img;
        }

        private void PushUndo()
        {
            undoList.Add((Bitmap)bmp.Clone());
            if (undoList.Count > 20)
            {
                undoList[0].Dispose();
                undoList.RemoveAt(0);
            }
            foreach (Bitmap b in redoList) b.Dispose();
            redoList.Clear();
        }

        private void Restore(Bitmap snap)
        {
            g.Clear(Color.White);
            g.DrawImageUnscaled(snap, 0, 0);
            this.Invalidate();
        }

        // =====================================================
        //  ЦЭСНИЙ ҮЙЛДЛҮҮД
        // =====================================================

        private void ClearCanvas()
        {
            if (g == null) return;
            PushUndo();
            g.Clear(Color.White);
            current = null;
            selectAll = false;
            this.Invalidate();
        }

        private void NewFile()
        {
            ClearCanvas();
            currentFile = null;
        }

        private void OpenFile()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Зураг|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    using (Image tmp = Image.FromFile(ofd.FileName))
                    {
                        int top = toolStrip1.Bottom;
                        PushUndo();
                        EnsureSize(tmp.Width, tmp.Height + top);
                        g.Clear(Color.White);
                        g.DrawImage(tmp, 0, top, tmp.Width, tmp.Height);
                    }
                    currentFile = null;
                    this.Invalidate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Зургийг нээж чадсангүй: " + ex.Message);
                }
            }
        }

        private void SaveFile()
        {
            if (currentFile == null) SaveFileAs();
            else SaveTo(currentFile);
        }

        private void SaveFileAs()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG|*.png|JPEG|*.jpg|BMP|*.bmp";
                sfd.DefaultExt = "png";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    SaveTo(sfd.FileName);
                    currentFile = sfd.FileName;
                }
            }
        }

        private void SaveTo(string path)
        {
            ImageFormat fmt = ImageFormat.Png;
            string ext = Path.GetExtension(path).ToLower();
            if (ext == ".jpg" || ext == ".jpeg") fmt = ImageFormat.Jpeg;
            else if (ext == ".bmp") fmt = ImageFormat.Bmp;

            try
            {
                using (Bitmap img = GetCanvasImage())
                {
                    img.Save(path, fmt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Хадгалж чадсангүй: " + ex.Message);
            }
        }

        private PrintDocument MakePrintDoc()
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (s, ev) =>
            {
                using (Bitmap img = GetCanvasImage())
                {
                    Rectangle mb = ev.MarginBounds;
                    float scale = Math.Min((float)mb.Width / img.Width, (float)mb.Height / img.Height);
                    ev.Graphics.DrawImage(img, mb.Left, mb.Top, img.Width * scale, img.Height * scale);
                }
            };
            return pd;
        }

        private void PrintCanvas()
        {
            try
            {
                using (PrintDialog dlg = new PrintDialog())
                {
                    dlg.Document = MakePrintDoc();
                    if (dlg.ShowDialog() == DialogResult.OK)
                        dlg.Document.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Хэвлэх боломжгүй байна: " + ex.Message);
            }
        }

        private void PrintPreviewCanvas()
        {
            try
            {
                using (PrintPreviewDialog pv = new PrintPreviewDialog())
                {
                    pv.Document = MakePrintDoc();
                    pv.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Урьдчилан харах боломжгүй байна: " + ex.Message);
            }
        }

        private void Undo()
        {
            if (undoList.Count == 0) return;
            redoList.Add((Bitmap)bmp.Clone());
            Bitmap snap = undoList[undoList.Count - 1];
            undoList.RemoveAt(undoList.Count - 1);
            Restore(snap);
            snap.Dispose();
        }

        private void Redo()
        {
            if (redoList.Count == 0) return;
            undoList.Add((Bitmap)bmp.Clone());
            Bitmap snap = redoList[redoList.Count - 1];
            redoList.RemoveAt(redoList.Count - 1);
            Restore(snap);
            snap.Dispose();
        }

        private void CopyCanvas()
        {
            try
            {
                using (Bitmap img = GetCanvasImage())
                {
                    Clipboard.SetImage(img);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Хуулж чадсангүй: " + ex.Message);
            }
        }

        private void CutCanvas()
        {
            CopyCanvas();
            ClearCanvas();
        }

        private void PasteCanvas()
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("Clipboard-д зураг алга байна.");
                return;
            }
            using (Image img = Clipboard.GetImage())
            {
                int top = toolStrip1.Bottom + 10;
                PushUndo();
                EnsureSize(10 + img.Width, top + img.Height);
                g.DrawImage(img, 10, top, img.Width, img.Height);
            }
            this.Invalidate();
        }

        private void SelectAllCanvas()
        {
            selectAll = true;
            this.Invalidate();
        }

        private void ShowHelp()
        {
            MessageBox.Show(
                "Tools цэс: Чөлөөт, Шулуун, Тэгш өнцөгт, Дугуй, Нум, Олон талт дүрс, Дугуй график.\n" +
                "Олон талт дүрс: зүүн click = орой нэмэх, баруун click = дуусгах.\n" +
                "Өнгө цэс: үзгийн өнгө сонгоно.\n" +
                "Баруун дээд тоо: үзгийн өргөн (1-10).\n" +
                "Graphic -> Clear: зургийг цэвэрлэнэ.\n" +
                "Edit -> Undo/Redo: сүүлийн зурсныг буцаана/дахин хийнэ.\n" +
                "File -> Save: зургийг PNG, JPG, BMP болгон хадгална.",
                "Тусламж");
        }

        private void ShowAbout()
        {
            MessageBox.Show("Paint программ\nХулганаар зурах лабораторийн ажил", "Тухай");
        }

        // =====================================================
        //  ЗУРАХ
        // =====================================================

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (bmp != null)
            {
                e.Graphics.DrawImageUnscaled(bmp, 0, 0);
            }
            if (current != null)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                current.Draw(e.Graphics);      // чирж буй дүрсийн урьдчилсан харагдац
            }
            if (selectAll)
            {
                using (Pen sp = new Pen(Color.Blue))
                {
                    sp.DashStyle = DashStyle.Dash;
                    int top = toolStrip1.Bottom;
                    e.Graphics.DrawRectangle(sp, 0, top,
                        this.ClientSize.Width - 1, this.ClientSize.Height - top - 1);
                }
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            // Олон талт дүрс: зүүн click = орой нэмэх, баруун click = дуусгах
            if (tool == "polygon")
            {
                if (e.Button == MouseButtons.Right)
                {
                    FinishPolygon();
                    return;
                }
                if (e.Button != MouseButtons.Left) return;

                selectAll = false;
                PPolygon poly = current as PPolygon;
                if (poly == null)
                {
                    PushUndo();
                    poly = new PPolygon(p.Color, p.Width, e.Location);
                    current = poly;
                }
                else
                {
                    poly.Points.Add(e.Location);
                }
                poly.End = e.Location;
                this.Invalidate();
                return;
            }

            if (e.Button != MouseButtons.Left) return;

            selectAll = false;
            PushUndo();     // Undo-д зориулж зурахаас өмнөх байдлыг хадгална

            if (tool == "free")
            {
                draw = true;
                x = e.X;
                y = e.Y;
            }
            else if (tool == "line") current = new PLine(p.Color, p.Width, e.Location);
            else if (tool == "rect") current = new PRect(p.Color, p.Width, e.Location);
            else if (tool == "ellipse") current = new PEllipse(p.Color, p.Width, e.Location);
            else if (tool == "arc") current = new PArc(p.Color, p.Width, e.Location);
            else if (tool == "pie") current = new PPie(p.Color, p.Width, e.Location);
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
            else if (current != null)
            {
                current.End = e.Location;
                this.Invalidate();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            draw = false;

            if (current != null && !(current is PPolygon))
            {
                current.End = e.Location;
                current.Draw(g);               // Bitmap дээр бүрмөсөн зурна
                current = null;
                this.Invalidate();
            }
        }

        // =====================================================
        //  Designer-тэй холбогдсон функцүүд (устгаж болохгүй)
        // =====================================================

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

        // Хэрэгслийн мөрийн товчнууд
        private void newToolStripButton_Click(object sender, EventArgs e) { NewFile(); }
        private void openToolStripButton_Click(object sender, EventArgs e) { OpenFile(); }
        private void saveToolStripButton_Click(object sender, EventArgs e) { SaveFile(); }
        private void printToolStripButton_Click(object sender, EventArgs e) { PrintCanvas(); }
        private void cutToolStripButton_Click(object sender, EventArgs e) { CutCanvas(); }
        private void copyToolStripButton_Click(object sender, EventArgs e) { CopyCanvas(); }
        private void pasteToolStripButton_Click(object sender, EventArgs e) { PasteCanvas(); }
        private void helpToolStripButton_Click(object sender, EventArgs e) { ShowHelp(); }
        private void toolStripButton1_Click(object sender, EventArgs e) { ClearCanvas(); }
    }
}