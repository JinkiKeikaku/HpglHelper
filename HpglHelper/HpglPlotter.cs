
namespace HpglHelper
{
    class HpglPlotter
    {
        bool isPenDown = false;
        bool isRelative = false;
        HpglPoint mCurrent = new HpglPoint();
        public HpglPoint P1 { get; private set; } = new HpglPoint();
        public HpglPoint P2 { get; private set; } = new HpglPoint();
        public double XMin;
        public double XMax;
        public double YMin;
        public double YMax;
        const double DefaultFontHeight = 0.375;
        const double DefaultFontWidth = 0.285;
        double mPaperWidth;
        double mPaperHeight;

        const string DefaultFontName = "Arial";

        public HpglPlotter() { }
        double mMillimeterPerUnit = 0.025;//0.025mm
        public int SelectedPen { get; set; } = 0;
        public double FontHeight { get; set; } = DefaultFontHeight;
        public double FontWidth { get; set; } = DefaultFontWidth;
        public string FontName { get; set; } = DefaultFontName;
        public double TextAngle { get; set; } = 0.0;
        /// <summary>
        /// 1:左下、２:左中、3:左上、4:中下、5:中中、6:中上、7:右下、8:右中、9:右上
        /// </summary>
        public int LabelOrigin { get; set; } = 1;

        public List<HpglShape> Shapes { get; set; } = new();
        public int ChordToleranceMode { get; set; } = 0;


        public void InitPlotter(double paperWidth, double paperHeight, double millimeterPerUnit)
        {
            mPaperWidth = paperWidth;
            mPaperHeight = paperHeight;
            mMillimeterPerUnit = millimeterPerUnit;
            Reset();
        }

        public void InitFontSize()
        {
            FontHeight = DefaultFontHeight;
            FontWidth = DefaultFontWidth;
        }

        public void Reset()
        {
            Shapes.Clear();
            SetIP();
            SetSC();
            mCurrent = new HpglPoint(0, 0);
            isPenDown = false;
            isRelative = false;
            InitFontSize();
            ChordToleranceMode = 0;
        }

        public void SetIP()
        {
            var w = mPaperWidth / mMillimeterPerUnit;
            var h = mPaperHeight / mMillimeterPerUnit;
            SetIP(new HpglPoint(0, 0), new HpglPoint(w, h));
        }
        public void SetIP(HpglPoint p1)
        {
            var dp = p1 - P1;
            var p2 = P2 + dp;
            SetIP(p1, p2);
        }

        public void SetIP(HpglPoint p1, HpglPoint p2)
        {
            var s = new HpglIPShape()
            {
                P1 = p1,
                P2 = p2,
            };
            P1.Set(p1);
            P2.Set(p2);
            AddShape(s);
        }

        public void SetSC()
        {
            SetSC(P1.X, P2.X, P1.Y, P2.Y);
        }

        public void SetSC(double xMin, double xMax, double yMin, double yMax)
        {
            var s = new HpglSCShape()
            {
                XMin = xMin,
                XMax = xMax,
                YMin = yMin,
                YMax = yMax
            };
            XMin = xMin;
            XMax = xMax;
            YMin = yMin;
            YMax = yMax;
            AddShape(s);
        }

        public void PenUp() => isPenDown = false;
        public void PenDown() => isPenDown = true;

        public void Move(double x, double y)
        {
            if (isRelative)
            {
                MoveRelative(x, y);
            }
            else
            {
                MoveAbsolute(x, y);
            }
        }

        public void MoveRelative(double x, double y)
        {
            if (isPenDown)
            {
                var line = new HpglLineShape();
                line.P0.Set(mCurrent);
                mCurrent.Offset(x, y);
                line.P1.Set(mCurrent);
                AddShape(line);
            }
            else
            {
                mCurrent.Offset(x, y);
            }
            isRelative = true;
        }
        public void MoveAbsolute(double x, double y)
        {
            if (isPenDown)
            {
                var line = new HpglLineShape();
                line.P0.Set(mCurrent);
                mCurrent.Set(x, y);
                line.P1.Set(mCurrent);
                AddShape(line);
            }
            else
            {
                mCurrent.Set(x, y);
            }
            isRelative = false;
        }
        public void Circle(double radius, double tolerance)
        {
            var s = new HpglCircleShape();
            s.Center.Set(mCurrent);
            s.Radius = radius;
            s.ChordToleranceMode = ChordToleranceMode;
            s.Tolerance = tolerance;
            AddShape(s);
        }
        public void Arc(double cx, double cy, double sweepDeg, bool isRelative, double tolerance)
        {
            var p0 = isRelative ? new HpglPoint(mCurrent.X + cx, mCurrent.Y + cy) : new HpglPoint(cx, cy);
            var s = new HpglArcShape();
            s.Center.Set(p0);
            s.SweepAngleDeg = sweepDeg;
            s.StartPoint.Set(mCurrent);
            if (isPenDown)
            {
                AddShape(s);
            }
            s.ChordToleranceMode = ChordToleranceMode;
            s.Tolerance = tolerance;
            mCurrent.Set(s.EndPoint);
        }
        public void Label(string label)
        {
            var text = new HpglLabelShape();
            text.Text = label;
            text.P0.Set(mCurrent);
            text.FontHeight = FontHeight;
            text.FontWidth = FontWidth;
            text.AngleDeg = TextAngle;
            text.Origin = LabelOrigin;
            AddShape(text);
        }

        public void CharacterMove(double cx, double cy)
        {
            var sx = (P2.X - P1.X) / (XMax - XMin) * mMillimeterPerUnit;
            var sy = (P2.Y - P1.Y) / (YMax - YMin) * mMillimeterPerUnit;

            mCurrent.Offset(10 * cx * FontWidth * sx, 10 * cy * FontHeight * sy);
        }

        //HpglPoint Convert(HpglPoint p)
        //{
        //    return p;
        //}
        //double Convert(double x)
        //{
        //    return x;
        //}


        void AddShape(HpglShape shape)
        {
            Shapes.Add(shape);
        }
    }
}
