using HpglHelper;
using HpglHelper.Commands;
using System.IO;

namespace HpglViewer
{
    public partial class Form1 : Form
    {
        const double mPaperWidth = 841;
        const double mPaperHeight = 594;
        const double mMillimeterPerUnit = 0.025;//0.025mm
        DrawContext DrawContext = new((float)mPaperWidth, (float)mPaperHeight);
        List<HpglCommand> mShapes = new();
        public Form1()
        {
            InitializeComponent();
            panel1.AutoScroll = true;
            this.panel1.MouseWheel += panel1_MouseWheel;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = new OpenFileDialog();
            d.Filter = "Hpgl files|*.hpgl;*.hgl;*.plt;|All files|*.*";
            if (d.ShowDialog() != DialogResult.OK) return;
            OpenFile(d.FileName);
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = new SaveFileDialog();
            d.Filter = "Hpgl files|*.hpgl|All files|*.*";
            if (d.ShowDialog() != DialogResult.OK) return;
            SaveFile(d.FileName);
        }

        private void OpenFile(String path)
        {
            var reader = new HpglReader();
            using var r = new StreamReader(path);
            reader.Read(r, mPaperWidth, mPaperHeight, mMillimeterPerUnit);

            mShapes = reader.Shapes;
            //スクロールバーなんかの設定。
            CalcSize();
            //panel1を無効化してpanel1のpaintが呼ばれる。
            panel1.Invalidate();
        }

        private void SaveFile(String path)
        {
            var writer = new HpglWriter();

            using var w = new StreamWriter(path);
            foreach (var s in mShapes)
            {
                if (s is HpglShape ss)
                    writer.AddShape(ss);
            }
            //writer.Shapes.Add(new HpglCircleShape()
            //{
            //    Center = new HpglPoint(50, 50),
            //    Radius = 50,
            //});
            //writer.Shapes.Add(new HpglCircleShape()
            //{
            //    Center = new HpglPoint(50, 50),
            //    Radius = 50,
            //    Flatness =2,
            //});

            writer.Write(w);
        }





        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);
            if (DrawContext == null) return;
            var saved = g.Save();
            g.TranslateTransform(
                DrawContext.Scale * 0 + panel1.AutoScrollPosition.X,
                DrawContext.Scale * DrawContext.PaperSize.Height + panel1.AutoScrollPosition.Y
            //DrawContext.Scale * DrawContext.PaperSize.Width / 2 + panel1.AutoScrollPosition.X,
            //DrawContext.Scale * DrawContext.PaperSize.Height / 2 + panel1.AutoScrollPosition.Y
            );
            g.ScaleTransform(DrawContext.Scale, DrawContext.Scale);
            var drawer = new HpglDrawer(mShapes, mPaperWidth, mPaperHeight, mMillimeterPerUnit);
            drawer.OnDraw(g, DrawContext);
            g.Restore(saved);
        }

        private void panel1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (DrawContext == null) return;
            /// マウスホイールでは拡大縮小のみ行う。
            if (e.Delta < 0)
            {
                DrawContext.Scale *= 2 / 3.0f;
            }
            else
            {
                DrawContext.Scale *= 1.5f;
            }
            CalcSize();
            panel1.Invalidate();
        }

        /// <summary>
        /// スクロールの設定
        /// </summary>
        private void CalcSize()
        {
            if (DrawContext == null) return;
            var ps = new Size((int)(DrawContext.PaperSize.Width * DrawContext.Scale), (int)(DrawContext.PaperSize.Height * DrawContext.Scale));
            panel1.AutoScrollMinSize = new Size((int)ps.Width, (int)ps.Height);
            panel1.AutoScrollPosition = new Point(
                Math.Max(0, (int)ps.Width / 2 - Width / 2),
                Math.Max(0, (int)ps.Height / 2 - Height / 2)
            );
        }

    }
}