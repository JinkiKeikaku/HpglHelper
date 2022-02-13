using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HpglHelper;
using HpglHelper.Commands;

namespace HpglViewer
{
    /// <summary>
    /// Hpgl図形描画クラス。
    /// 中央を原点にした。
    /// </summary>
    internal class HpglDrawer
    {
        List<HpglCommand> mShapes;
        //CadPoint mP1 = new();
        //CadPoint mP2 = new();
        //double mXMin;
        //double mXMax;
        //double mYMin;
        //double mYMax;
        //double mMillimeterPerUnit;
        CadPoint mOrigin = new(0, 0);

        Color[] mPenColors = new Color[] {
            Color.Black,Color.Blue,Color.Red,Color.Magenta,
            Color.Green, Color.Cyan, Color.Yellow, Color.Black};



        public HpglDrawer(List<HpglCommand> shapes, double width, double height, double millimeterPerUnit)
        {
            mOrigin.X = width;
            mOrigin.Y = height;

            mShapes = shapes;
            //mMillimeterPerUnit = millimeterPerUnit;
        }

        public void OnDraw(Graphics g, DrawContext d)
        {
            foreach (var shape in mShapes)
            {
                OnDrawShape(g, d, shape);
            }
        }
        void OnDrawShape(Graphics g, DrawContext d, HpglCommand shape)
        {
            switch (shape)
            {
                //case HpglIPCommand s: OnSetIP(s); break;
                //case HpglSCCommand s: OnSetSC(s); break;
                case HpglLineShape s: OnDrawLine(g, d, s); break;
                case HpglCircleShape s: OnDrawCircle(g, d, s); break;
                case HpglArcShape s: OnDrawArc(g, d, s); break;
                case HpglLabelShape s: OnDrawLabel(g, d, s); break;
                case HpglEdgeWedgeShape s: OnDrawEdgeWedge(g, d, s); break;
                case HpglEdgeRectangleShape s: OnDrawRectangle(g, d, s); break;
                case HpglFillWedgeShape s: OnDrawFillWedge(g, d, s); break;
                case HpglFillRectangleShape s: OnDrawFillRectangle(g, d, s); break;
                case HpglPolygonShape s: OnDrawPolygon(g, d, s); break;

            }
        }

        void OnDrawLine(Graphics g, DrawContext d, HpglLineShape shape)
        {
            var p0 = d.DocToCanvas(ConvertPoint(shape.P0));
            var p1 = d.DocToCanvas(ConvertPoint(shape.P1));
            d.Pen.Color = ConvertPenColor(shape.PenNumber);
            g.DrawLine(d.Pen, p0, p1);
        }
        void OnDrawCircle(Graphics g, DrawContext d, HpglCircleShape shape)
        {
            var pa = new List<PointF>();
            double a = 0;
            while (a < 360)
            {
                pa.Add(ConvertArcPoint(d, shape.Center, shape.Radius, shape.Flatness, a));
                //手抜きでshape.ChordToleranceModeは無視します。
                a += shape.Tolerance;

            }
            d.Pen.Color = ConvertPenColor(shape.PenNumber);
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
                pa.Add(ConvertArcPoint(d, shape.Center, shape.Radius, shape.Flatness, a + sa));
                //手抜きでshape.ChordToleranceModeは無視します。
                a += shape.Tolerance;
            }
            pa.Add(ConvertArcPoint(d, shape.Center, shape.Radius, shape.Flatness, sw + sa));
            if (pa.Count > 1)
            {
                d.Pen.Color = ConvertPenColor(shape.PenNumber);
                g.DrawLines(d.Pen, pa.ToArray());
            }
        }
        void OnDrawRectangle(Graphics g, DrawContext d, HpglEdgeRectangleShape shape)
        {
            var p0 = d.DocToCanvas(ConvertPoint(shape.P0));
            var p2 = d.DocToCanvas(ConvertPoint(shape.P1));
            var p1 = new PointF(p2.X, p0.Y);
            var p3 = new PointF(p0.X, p2.Y);
            d.Pen.Color = ConvertPenColor(shape.PenNumber);
            g.DrawPolygon(d.Pen, new PointF[] { p0, p1, p2, p3 });
        }

        void OnDrawLabel(Graphics g, DrawContext d, HpglLabelShape shape)
        {
            //左下
            var p0 = d.DocToCanvas(ConvertPoint(shape.P0));
            var angle = d.DocToCanvasAngle(shape.AngleDeg);
            var saved = g.Save();
            g.TranslateTransform(p0.X, p0.Y);
            g.RotateTransform(angle);
            var h = shape.FontHeight;
            var w = shape.FontWidth;
            //手抜きでフォント幅は省略
            using var font = new Font("Arial", (float)h, GraphicsUnit.Pixel);
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
            using var brush = new SolidBrush(ConvertPenColor(shape.PenNumber));
            g.DrawString(shape.Text, font, brush, (float)x, (float)y);
            g.Restore(saved);
        }

        void OnDrawEdgeWedge(Graphics g, DrawContext d, HpglEdgeWedgeShape shape)
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
                pa.Add(ConvertArcPoint(d, shape.Center, shape.Radius, shape.Flatness, a + sa));
                //手抜きでshape.ChordToleranceModeは無視します。
                a += shape.Tolerance;
            }
            pa.Add(ConvertArcPoint(d, shape.Center, shape.Radius, shape.Flatness, sw + sa));
            pa.Add(d.DocToCanvas(ConvertPoint(shape.Center)));
            if (pa.Count > 1)
            {
                d.Pen.Color = ConvertPenColor(shape.PenNumber);
                g.DrawPolygon(d.Pen, pa.ToArray());
            }
        }

        void OnDrawFillRectangle(Graphics g, DrawContext d, HpglFillRectangleShape shape)
        {
            var p0 = d.DocToCanvas(ConvertPoint(shape.P0));
            var p2 = d.DocToCanvas(ConvertPoint(shape.P1));
            var p1 = new PointF(p2.X, p0.Y);
            var p3 = new PointF(p0.X, p2.Y);
            using var b = new SolidBrush(ConvertPenColor(shape.PenNumber));
            g.FillPolygon(b, new PointF[] { p0, p1, p2, p3 });

        }


        void OnDrawFillWedge(Graphics g, DrawContext d, HpglFillWedgeShape shape)
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
                pa.Add(ConvertArcPoint(d, shape.Center, shape.Radius, shape.Flatness, a + sa));
                //手抜きでshape.ChordToleranceModeは無視します。
                a += shape.Tolerance;
            }
            pa.Add(ConvertArcPoint(d, shape.Center, shape.Radius, shape.Flatness, sw + sa));
            pa.Add(d.DocToCanvas(ConvertPoint(shape.Center)));
            if (pa.Count > 1)
            {
                using var b = new SolidBrush(ConvertPenColor(shape.PenNumber));
                g.FillPolygon(b, pa.ToArray());
            }
        }


        void OnDrawPolygon(Graphics g, DrawContext d, HpglPolygonShape shape)
        {
            var path = new GraphicsPath();
            foreach (var pb in shape.PolygonBufferList)
            {
                foreach (var ss in pb)
                {
                    switch (ss)
                    {
                        case HpglLineShape s:
                            {
                                path.AddLine(d.DocToCanvas(ConvertPoint(s.P0)), d.DocToCanvas(ConvertPoint(s.P1)));
                            }
                            break;
                        case HpglCircleShape s:
                            {
                                var p0 = d.DocToCanvas(ConvertPoint(s.Center));
                                var rx = d.DocToCanvas(s.Radius);
                                var ry = d.DocToCanvas(s.Radius * s.Flatness);
                                path.AddEllipse(p0.X - rx, p0.Y - ry, rx * 2, ry * 2);

                            }
                            break;
                        case HpglArcShape s:
                            {
                                var p0 = d.DocToCanvas(ConvertPoint(s.Center));
                                var rx = d.DocToCanvas(s.Radius);
                                var ry = d.DocToCanvas(s.Radius * s.Flatness);
                                path.AddArc(
                                    p0.X - rx, p0.Y - ry, rx * 2, ry * 2, 
                                    d.DocToCanvasAngle(s.StartAngleDeg), 
                                    d.DocToCanvasAngle(s.SweepAngleDeg));
                            }
                            break;
                    }
                }
            }

            if (shape.FillPen >= 0)
            {
                using var b = new SolidBrush(ConvertPenColor(shape.PenNumber));
                g.FillPath(b, path);
            }
            if(shape.EdgePen >= 0)
            {
                d.Pen.Color = ConvertPenColor(shape.PenNumber);
                g.DrawPath(d.Pen, path);
            }
        }


        PointF ConvertArcPoint(DrawContext d, HpglPoint p0, double radius, double flatness, double angle)
        {
            var a = angle * Math.PI / 180;
            var p = new HpglPoint(
                p0.X + Math.Cos(a) * radius,
                p0.Y + Math.Sin(a) * radius * flatness
            );
            return d.DocToCanvas(ConvertPoint(p));
        }

        CadPoint ConvertPoint(HpglPoint p)
        {
            //var p0 = (mP1 - new CadPoint(-mXMin, -mYMin)) * mMillimeterPerUnit;
            //var sx = (mP2.X - mP1.X) / (mXMax - mXMin) * mMillimeterPerUnit;
            //var sy = (mP2.Y - mP1.Y) / (mYMax - mYMin) * mMillimeterPerUnit;
            //return new CadPoint(sx * p.X + p0.X, sy * p.Y + p0.Y) + mOrigin;
            return new CadPoint(p.X + mOrigin.X, p.Y + mOrigin.Y);
        }

        Color ConvertPenColor(int pen)
        {
            if (pen < mPenColors.Length)
            {
                return mPenColors[pen];
            }
            return Color.Gray;
        }
    }
}