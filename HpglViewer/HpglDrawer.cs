using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HpglHelper;

namespace HpglViewer
{
    /// <summary>
    /// Hpgl図形描画クラス。
    /// 中央を原点にした。
    /// </summary>
    internal class HpglDrawer
    {
        List<HpglShape> mShapes;
        CadPoint mP1 = new();
        CadPoint mP2 = new();
        double mXMin;
        double mXMax;
        double mYMin;
        double mYMax;
        double mMillimeterPerUnit;
        CadPoint mOrigin = new(0, 0);

        public HpglDrawer(List<HpglShape> shapes, double width, double height, double millimeterPerUnit)
        {
            mOrigin.X = width;
            mOrigin.Y = height;

            mShapes = shapes;
            mMillimeterPerUnit = millimeterPerUnit;
        }

        public void OnDraw(Graphics g, DrawContext d)
        {
            foreach (var shape in mShapes)
            {
                OnDrawShape(g, d, shape);
            }
        }
        void OnDrawShape(Graphics g, DrawContext d, HpglShape shape)
        {
            switch (shape)
            {
                case HpglIPShape s: OnSetIP(s); break;
                case HpglSCShape s: OnSetSC(s); break;
                case HpglLineShape s: OnDrawLine(g, d, s); break;
                case HpglCircleShape s: OnDrawCircle(g, d, s); break;
                case HpglArcShape s: OnDrawArc(g, d, s); break;
                case HpglLabelShape s: OnDrawLabel(g, d, s); break;
            }
        }

        void OnSetIP(HpglIPShape s)
        {
            mP1.X = s.P1.X;
            mP1.Y = s.P1.Y;
            mP2.X = s.P2.X;
            mP2.Y = s.P2.Y;
        }
        void OnSetSC(HpglSCShape s)
        {
            mXMin = s.XMin;
            mYMin = s.YMin;
            mXMax = s.XMax;
            mYMax = s.YMax;
        }

        void OnDrawLine(Graphics g, DrawContext d, HpglLineShape shape)
        {
            var p0 = d.DocToCanvas(ConvertPoint(shape.P0));
            var p1 = d.DocToCanvas(ConvertPoint(shape.P1));
            g.DrawLine(d.Pen, p0, p1);
        }
        void OnDrawCircle(Graphics g, DrawContext d, HpglCircleShape shape)
        {
            var pa = new List<PointF>();
            double a = 0;
            while (a < 360)
            {
                pa.Add(ConvertArcPoint(d, shape.Center, shape.Radius, a));
                //手抜きでshape.ChordToleranceModeは無視します。
                a += shape.Tolerance;

            }
            g.DrawPolygon(d.Pen, pa.ToArray());
        }
        void OnDrawArc(Graphics g, DrawContext d, HpglArcShape shape)
        {
            var pa = new List<PointF>();
            double sa = shape.StartAngleDeg;
            double sw = shape.SweepAngleDeg;

            if (sw < 0)
            {
                sa += sw;
                sw = -sw;
            }
            var a = 0.0;
            while (a < sw)
            {
                pa.Add(ConvertArcPoint(d, shape.Center, shape.Radius, a + sa));
                //手抜きでshape.ChordToleranceModeは無視します。
                a += shape.Tolerance;
            }
            pa.Add(ConvertArcPoint(d, shape.Center, shape.Radius, sw + sa));
            if (pa.Count > 1)
            {
                g.DrawLines(d.Pen, pa.ToArray());
            }
        }
        void OnDrawLabel(Graphics g, DrawContext d, HpglLabelShape shape)
        {
            //左下
            var p0 = d.DocToCanvas(ConvertPoint(shape.P0));
            var angle = d.DocToCanvasAngle(shape.AngleDeg);
            var saved = g.Save();
            g.TranslateTransform(p0.X, p0.Y);
            g.RotateTransform(angle);
            var h = shape.FontHeight * 10.0f;
            var w = shape.FontWidth * 10.0f;
            //手抜きでフォント幅は省略
            using var font = new Font("Arial", (float)h, GraphicsUnit.Pixel);
            using var brush = new SolidBrush(Color.Black);
            var i = shape.Origin;
            var shift = false;
            if (i >= 10)
            {
                i -= 10;
                shift = true;
            }
            double[] dx = { 0.0, 0.0, 0.0, 0.0, -0.5, -0.5, -0.5, -1.0, -1.0, -1.0 };
            double[] dy = { 0.0, -1.0, -0.5, 0.0, -1.0, -0.5, 0.0, -1.0, -0.5, 0.0 };
            double[] dx2 = { 0.0, 1.0, 1.0, 1.0, 0.0, 0.0, 0.0, -1.0, -1.0, -1.0 };
            double[] dy2 = { 0.0, -1.0, 0.0, 1.0, -1.0, 0.0, 1.0, -1.0, 0.0, 1.0 };
            var size = g.MeasureString(shape.Text, font);
            var x = size.Width * dx[i];
            var y = size.Height * dy[i];
            if (shift)
            {
                x += dx2[i] * w * 0.5;
                y += dy2[i] * h * 0.5;
            }
            g.DrawString(shape.Text, font, brush, (float)x, (float)y);
            g.Restore(saved);
        }

        PointF ConvertArcPoint(DrawContext d, HpglPoint p0, double radius, double angle)
        {
            var a = angle * Math.PI / 180;
            var p = new HpglPoint(
                p0.X + Math.Cos(a) * radius,
                p0.Y + Math.Sin(a) * radius
            );
            return d.DocToCanvas(ConvertPoint(p));
        }

        /// <summary>
        /// 座標変換。SC,IPの値から返還。単位はmm。
        /// </summary>
        CadPoint ConvertPoint(HpglPoint p)
        {
            var p0 = (mP1 - new CadPoint(-mXMin, -mYMin)) * mMillimeterPerUnit;
            var sx = (mP2.X - mP1.X) / (mXMax - mXMin) * mMillimeterPerUnit;
            var sy = (mP2.Y - mP1.Y) / (mYMax - mYMin) * mMillimeterPerUnit;
            return new CadPoint(sx * p.X + p0.X, sy * p.Y + p0.Y) + mOrigin;
        }
    }
}