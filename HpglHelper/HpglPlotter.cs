
using HpglHelper.Commands;

namespace HpglHelper
{
    class HpglPlotter
    {
        bool isPenDown = false;
        bool isRelative = false;
        HpglPoint mCurrent = new HpglPoint();
        public int P1X;
        public int P1Y;
        public int P2X;
        public int P2Y;
        public int XMin;
        public int XMax;
        public int YMin;
        public int YMax;
        const double DefaultFontHeight = 3.75;
        const double DefaultFontWidth = 2.85;
        double mPaperWidth;
        double mPaperHeight;

        const string DefaultFontName = "Arial";

        public HpglPlotter() { }
        double mMillimeterPerUnit = 0.025;//0.025mm
        public int SelectedPen { get; private set; } = 0;
        public double FontHeight { get; set; } = DefaultFontHeight;
        public double FontWidth { get; set; } = DefaultFontWidth;
        public string FontName { get; set; } = DefaultFontName;
        public double TextAngle { get; set; } = 0.0;
        /// <summary>
        /// 1:左下、２:左中、3:左上、4:中下、5:中中、6:中上、7:右下、8:右中、9:右上
        /// </summary>
        public int LabelOrigin { get; set; } = 1;
        public List<HpglCommand> Shapes { get; set; } = new();
        public int ChordToleranceMode { get; set; } = 0;

        /// <summary>
        /// 塗りつぶしタイプ
        /// </summary>
        public HpglFillType FillType { get; set; } = new();
        /// <summary>
        /// ペン番号に対応する塗りつぶしのペン幅（単位はmm）。デフォルトは0.3mm
        /// </summary>
        Dictionary<int, double> mFillPenThicknessMap { get; } = new();
        /// <summary>
        /// 現在のペン幅。単位はｍｍ。
        /// </summary>
        /// <returns></returns>
        public double GetPenThickness()
        {
            return mFillPenThicknessMap.GetValueOrDefault(SelectedPen, 0.3);
        }
        public void SetPenThickness(double w)
        {
            mFillPenThicknessMap[SelectedPen] = w;
        }

        public void SetPen(int pen)
        {
            SelectedPen = pen;
        }

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
            mFillPenThicknessMap.Clear();
            FillType = new();
        }

        public void SetIP()
        {
            var w = mPaperWidth / mMillimeterPerUnit;
            var h = mPaperHeight / mMillimeterPerUnit;
            SetIP(0, 0, (int)w, (int)h);
        }
        public void SetIP(int p1x, int p1y)
        {
            var dx = p1x - P1X;
            var dy = p1y - P1Y;
            var p2x = P2X + dx;
            var p2y = P2Y + dy;
            SetIP(p1x, p1y, p2x, p2y);
        }

        public void SetIP(int p1x, int p1y, int p2x, int p2y)
        {
            P1X = p1x;
            P1Y = p1y;
            P2X = p2x;
            P2Y = p2y;
        }

        public void SetSC()
        {
            SetSC(P1X, P2X, P1Y, P2Y);
        }

        public void SetSC(int xMin, int xMax, int yMin, int yMax)
        {
            XMin = xMin;
            XMax = xMax;
            YMin = yMin;
            YMax = yMax;
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
                mCurrent= ConvertRelativePoint(x, y);
                line.P1.Set(mCurrent);
                AddCommand(line);
            }
            else
            {
                mCurrent = ConvertRelativePoint(x, y);
            }
            isRelative = true;
        }
        public void MoveAbsolute(double x, double y)
        {
            var p = ConvertPoint(x, y);
            if (isPenDown)
            {
                var line = new HpglLineShape();
                line.P0.Set(mCurrent);
                mCurrent.Set(p);
                line.P1.Set(mCurrent);
                AddCommand(line);
            }
            else
            {
                mCurrent.Set(p);
            }
            isRelative = false;
        }

        public void Circle(double radius, double tolerance)
        {
            var s = new HpglCircleSahpe();
            s.Center.Set(mCurrent);
            s.Radius = ConvertLength(radius);
            s.Flatness = GetFlatness();
            s.ChordToleranceMode = ChordToleranceMode;
            s.Tolerance = tolerance;
            AddCommand(s);
        }

        public void Arc(double cx, double cy, double sweepDeg, bool isRelative, double tolerance)
        {
            var p0 = isRelative? ConvertRelativePoint(cx, cy):ConvertPoint(cx, cy);
            var s = new HpglArcShape();
            var dp = mCurrent - p0;
            dp.Y /= GetFlatness();
            s.Center.Set(p0);
            s.Radius = dp.HYpot();
            s.Flatness = GetFlatness();
            s.StartAngleDeg = 180 * Math.Atan2(dp.Y, dp.X) / Math.PI;
            s.SweepAngleDeg = sweepDeg;
//            s.StartPoint.Set(mCurrent);
            if (isPenDown)
            {
                AddCommand(s);
            }
            s.ChordToleranceMode = ChordToleranceMode;
            s.Tolerance = tolerance;
            mCurrent.Set(s.EndPoint);
        }

        public void EdgeWedge(double radius, double startDeg, double sweepDeg, double tolerance)
        {
            var s = new HpglEdgeWedgeShape();
            s.Center.Set(mCurrent);
            s.Radius = ConvertLength(radius);
            s.Flatness = GetFlatness();
            s.StartAngleDeg = startDeg;
            s.SweepAngleDeg = sweepDeg;
            s.ChordToleranceMode = ChordToleranceMode;
            s.Tolerance = tolerance;
            AddCommand(s);
        }

        public void EdgeRectangle(double x, double y, bool isRelative)
        {
            var p1 = isRelative ? ConvertRelativePoint(x, y) : ConvertPoint(x, y);
            var s = new HpglEdgeRectangleShape();
            s.P0.Set(mCurrent);
            s.P1.Set(p1);
            AddCommand(s);
        }

        public void FillRectangle(double x, double y, bool isRelative)
        {
            var p1 = isRelative ? ConvertRelativePoint(x, y) : ConvertPoint(x, y);
            var s = new HpglFillRectangleShape();
            s.P0.Set(mCurrent);
            s.P1.Set(p1);
//            s.FillType = FillType;
            s.PenThickness = GetPenThickness();
            AddCommand(s);
        }

        public void FillWedge(double radius, double startDeg, double sweepDeg, double tolerance)
        {
            var s = new HpglFillWedgeShape();
            s.Center.Set(mCurrent);
            s.Radius = ConvertLength(radius);
            s.Flatness = GetFlatness();
            s.StartAngleDeg = startDeg;
            s.SweepAngleDeg = sweepDeg;
            s.ChordToleranceMode = ChordToleranceMode;
            s.Tolerance = tolerance;
            s.PenThickness = GetPenThickness();
            AddCommand(s);
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
            AddCommand(text);
        }

        /// <summary>
        /// 文字単位の移動
        /// </summary>
        public void CharacterMove(double cx, double cy)
        {
            //var sx = (P2X - P1X) / (XMax - XMin) * mMillimeterPerUnit;
            //var sy = (P2Y - P1Y) / (YMax - YMin) * mMillimeterPerUnit;
            mCurrent.Offset(cx * FontWidth, cy * FontHeight);
        }

        void AddCommand(HpglCommand cmd)
        {
            if (cmd is HpglShape s) { 
                s.PenNumber = SelectedPen; 
            }
            if(cmd is HpglFillShape fs)
            {
                fs.FillType.FillType = FillType.FillType;
                fs.FillType.FillAngle=FillType.FillAngle;
                switch (FillType.FillGap)
                {
                    case -1:
                        var dx = P2X - P1X;
                        var dy = P2Y - P1Y;
                        fs.FillType.FillGap  = Math.Sqrt(dx * dx + dy * dy) / 100 * mMillimeterPerUnit;
                        break;
                    case 0:
                        fs.FillType.FillGap = GetPenThickness();
                        break;
                    default:
                        fs.FillType.FillGap = FillType.FillGap * mMillimeterPerUnit;
                        break;
                }



            }
            Shapes.Add(cmd);
        }

        /// <summary>
        /// 座標変換。SC,IPの値からプロッタ座標(mm)に変換。
        /// </summary>
        HpglPoint ConvertPoint(HpglPoint p)=>ConvertPoint(p.X, p.Y);

        /// <summary>
        /// 座標変換。SC,IPの値からプロッタ座標(mm)に変換。
        /// </summary>
        HpglPoint ConvertPoint(double x, double y)
        {
            var sx = (double)(P2X - P1X) / (XMax - XMin);
            var sy = (double)(P2Y - P1Y) / (YMax - YMin);
            return new HpglPoint(
                (sx * (x - XMin) + P1X) * mMillimeterPerUnit,
                (sy * (y - YMin) + P1Y) * mMillimeterPerUnit);
        }

        /// <summary>
        /// 座標変換。SC,IPの値からプロッタ座標(mm)に変換。
        /// </summary>
        HpglPoint ConvertRelativePoint(double x, double y)
        {
            var sx = (double)(P2X - P1X) / (XMax - XMin);
            var sy = (double)(P2Y - P1Y) / (YMax - YMin);
            return new HpglPoint(sx * x * mMillimeterPerUnit +　mCurrent.X, sy * y * mMillimeterPerUnit + mCurrent.Y);
        }

        double ConvertLength(double a)
        {
            return a * (P2X - P1X) / (XMax - XMin) * mMillimeterPerUnit;
        }

        double GetFlatness()
        {
            //var sx = (double)(P2X - P1X) / (XMax - XMin);
            //var sy = (double)(P2Y - P1Y) / (YMax - YMin);
            //return sy / sx;
            var sx = (double)(P2X - P1X) / (XMax - XMin);
            return (double)(P2Y - P1Y) * (XMax - XMin) / ((YMax - YMin)* (P2X - P1X));
        }

    }
}
