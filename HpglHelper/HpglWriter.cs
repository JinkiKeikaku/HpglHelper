using HpglHelper.Commands;
using static HpglHelper.HpglPoint;
namespace HpglHelper
{
    /// <summary>
    /// Hpgl保存クラス
    /// 図形要素を追加する場合、ConvertItem()とWriteItem()の両方に追加してください。
    /// </summary>
    public class HpglWriter
    {
        bool mIsPolygonBuffer = false;
        int mPolygonMode = 0;
        bool mIsFirstPolygonPoint = true;
        readonly double mmPerUnit = 0.025;//0.025mm
        HpglPoint mCurrent = new(0, 0);
        List<HpglPoint> mLinePoints = new(); //Line用の点バッファ
        bool mIsPenDown = false;
        int mSelectedPen = 1;
        HpglLabelShape mLabelShape = new();
        List<HpglShape> Shapes { get; } = new();

        /// <summary>
        /// 保存する図形の登録
        /// </summary>
        public void AddShape(HpglShape shape) => Shapes.Add(shape);

        /// <summary>
        /// 保存処理。AddShape()した図形を保存。
        /// </summary>
        /// <param name="w"></param>
        /// <param name="returnToHome">最後に（０,０）にペンを戻すときtrue</param>
        public void Write(TextWriter w, bool returnToHome = false)
        {
            if (!mIsPolygonBuffer)
            {
                w.WriteLine($"IN;PU0,0;SP{mSelectedPen};");//初期化とペンアップで原点、ペン選択
                                                           //                w.WriteLine($"IP0,0,4000,4000; SC0,100,0,100;");//座標単位をmmにする。
                mCurrent.Set(0, 0);
                mIsPenDown = false;
            }
            List<Item?> items = new();
            foreach (var shape in Shapes)
            {
                var item = ConvertItem(shape);
                if (item != null) items.Add(item);
            }
            items = GetSortedItems(items);
            while (true)
            {
                var (pos, ip) = GetNearPoint(mCurrent, items);
                if (pos == -1) break;
                WriteItem(w, items[pos]!, ip);
                items[pos] = null;
            }
            UpdateLines(w);
            if (!mIsPolygonBuffer && returnToHome)
            {
                w.WriteLine($"PU0,0;");//終わったら原点に戻す。
            }
        }

        class Item
        {
            public HpglShape Shape;
            public HpglPoint[] Terminal = new HpglPoint[2];
            public Item(HpglShape shape, HpglPoint p0, HpglPoint p1)
            {
                Shape = shape;
                Terminal[0] = p0;
                Terminal[1] = p1;
            }
            public bool IsClosed => PointEQ(Terminal[0], Terminal[1]);
        }
        List<Item?> GetSortedItems(List<Item?> src)
        {
            var dst = new List<Item?>();
            for (var i = 0; i < src.Count; i++)
            {
                var d = GetSortedItems(src, i);
                dst.AddRange(d);
            }
            return dst;
        }
        List<Item> GetSortedItems(List<Item?> src, int start)
        {
            var dst = new List<Item>();
            var startItem = src[start];
            if (startItem == null) return dst;
            dst.Add(startItem);
            src[start] = null;
            if (startItem.IsClosed) return dst;
            var p0 = new HpglPoint[2] { startItem.Terminal[0], startItem.Terminal[1] };
            int c0, c1;
            do
            {
                c0 = 0;
                c1 = 0;
                var i0 = -1;
                for (var i = 0; i < src.Count; i++)
                {
                    var s = src[i];
                    if (s == null) continue;
                    if (PointEQ(p0[0], s.Terminal[0]))
                    {
                        c0++;
                        i0 = i;
                        break;
                    }
                    if (PointEQ(p0[0], s.Terminal[1]))
                    {
                        c0++;
                        i0 = i;
                        break;
                    }
                    if (PointEQ(p0[1], s.Terminal[0]))
                    {
                        c1++;
                        i0 = i;
                        break;
                    }
                    if (PointEQ(p0[1], s.Terminal[1]))
                    {
                        c1++;
                        i0 = i;
                        break;
                    }
                }
                if (i0 >= 0 && c0 == 1)
                {
                    if (PointEQ(p0[0], src[i0]!.Terminal[0]))
                    {
                        p0[0] = src[i0]!.Terminal[1];
                    }
                    else
                    {
                        p0[0] = src[i0]!.Terminal[0];
                    }
                    dst.Insert(0, src[i0]!);
                }
                if (i0 >= 0 && c1 == 1)
                {
                    if (PointEQ(p0[1], src[i0]!.Terminal[0]))
                    {
                        p0[1] = src[i0]!.Terminal[1];
                    }
                    else
                    {
                        p0[1] = src[i0]!.Terminal[0];
                    }
                    dst.Add(src[i0]!);
                }
                if (i0 >= 0)
                {
                    src[i0] = null;
                }
            } while ((c0 == 1 || c1 == 1) && !PointEQ(p0[0], p0[1]));
            return dst;
        }

        Item? ConvertItem(HpglShape shape)
        {
            return shape switch
            {
                HpglLineShape s => new Item(s, s.P0, s.P1),
                HpglArcShape s => new Item(s, s.StartPoint, s.EndPoint),
                HpglCircleShape s => new Item(s, s.Center, s.Center),
                HpglEdgeRectangleShape s => new Item(s, s.P0, s.P0),
                HpglEdgeWedgeShape s => new Item(s, s.Center, s.Center),
                HpglLabelShape s => new Item(s, s.P0, s.P0),
                HpglPolygonShape s => new Item(s, new HpglPoint(), new HpglPoint()),
                _ => null,
            };
        }

        (int pos, int ip) GetNearPoint(HpglPoint p, List<Item?> items)
        {
            var kMin = -1;
            var iMin = -1;
            var rMin = double.MaxValue;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null) continue;
                for (var k = 0; k < 2; k++)
                {
                    if (rMin > (item.Terminal[k] - p).Hypot())
                    {
                        rMin = (item.Terminal[k] - p).Hypot();
                        iMin = i;
                        kMin = k;
                        if (FloatEQ(rMin, 0)) break;
                    }
                }
            }
            return (iMin, kMin);
        }

        void WriteItem(TextWriter w, Item item, int i)
        {
            switch (item.Shape)
            {
                case HpglLineShape s:
                    WriteLine(w, s, i);
                    break;
                case HpglArcShape s:
                    UpdateLines(w);
                    WriteArc(w, s, i);
                    break;
                case HpglCircleShape s:
                    UpdateLines(w);
                    WriteCircle(w, s);
                    break;
                case HpglEdgeRectangleShape s:
                    UpdateLines(w);
                    WriteEdgeRectangle(w, s);
                    break;
                case HpglEdgeWedgeShape s:
                    UpdateLines(w);
                    WriteEdgeWedge(w, s);
                    break;
                case HpglLabelShape s:
                    UpdateLines(w);
                    WriteLabel(w, s);
                    break;
                case HpglPolygonShape s:
                    UpdateLines(w);
                    WritePolygon(w, s);
                    break;

            }
        }

        void CheckPen(TextWriter w, HpglShape s)
        {
            CheckPen(w, s.PenNumber);
        }

        void CheckPen(TextWriter w, int pen)
        {
            if (mSelectedPen != pen)
            {
                mSelectedPen = pen;
                w.Write($"SP{mSelectedPen};");
            }
        }

        void CheckPolygonFirstPoint(TextWriter w, HpglPoint p1)
        {
            if (mIsPolygonBuffer && mIsFirstPolygonPoint)
            {
                mIsFirstPolygonPoint = false;
                if (mPolygonMode == 0)
                {
                    if(!PointEQ(mCurrent, p1))
                    {
                        w.Write($"PU{ConvertPoint(p1)};");
                        mIsPenDown = false;
                        mCurrent.Set(p1);
                    }
                    w.WriteLine($"PM{mPolygonMode};");
                    return;
                }
            }
            if (!PointEQ(mCurrent, p1))
            {
                w.Write($"PU{ConvertPoint(p1)};");
                mIsPenDown = false;
                mCurrent.Set(p1);
            }
        }

        void WriteLine(TextWriter w, HpglLineShape s, int iTerminal)
        {
            var (p1, p2) = iTerminal == 0 ? (s.P0, s.P1) : (s.P1, s.P0);
            if (!PointEQ(mCurrent, p1) || mSelectedPen != s.PenNumber)
            {
                UpdateLines(w);
                mIsPenDown = false;
            }
            CheckPen(w, s);
            if (mLinePoints.Count == 0)
            {
                CheckPolygonFirstPoint(w, p1);
                mLinePoints.Add(p1);
            }
            mLinePoints.Add(p2);
            mCurrent.Set(p2);
        }
        void WriteCircle(TextWriter w, HpglCircleShape s)
        {
            CheckPen(w, s);
            var p1 = s.Center;
            CheckPolygonFirstPoint(w, p1);
            if (!PointEQ(mCurrent, p1))
            {
                w.WriteLine($"PU{ConvertPoint(p1)};");
                mCurrent.Set(p1);
            }
            var r = ConvertLength(s.Radius);
            if (s.Tolerance != 5)
            {
                w.WriteLine($"CI{r:0.#####},{s.Tolerance:0.#####};");
            }
            else
            {
                w.WriteLine($"CI{r:0.#####};");
            }
        }
        void WriteArc(TextWriter w, HpglArcShape s, int iTerminal)
        {
            CheckPen(w, s);
            var (p1, p2) = iTerminal == 0 ? (s.StartPoint, s.EndPoint) : (s.EndPoint, s.StartPoint);
            var a = iTerminal == 0 ? s.SweepAngleDeg : -s.SweepAngleDeg;
            var p0 = s.Center;
            CheckPolygonFirstPoint(w, p1);
            if (!PointEQ(mCurrent, p1))
            {
                w.WriteLine($"PU{ConvertPoint(p1)};");
                mIsPenDown = false;
            }
            if (!mIsPenDown)
            {
                w.Write("PD;");
                mIsPenDown = true;
            }
            if (s.Tolerance != 5)
            {
                w.WriteLine($"PD;AA{ConvertPoint(p0)},{a:0.#####},{s.Tolerance:0.#####};");
            }
            else
            {
                w.WriteLine($"PD;AA{ConvertPoint(p0)},{a:0.#####};");
            }
            mCurrent.Set(p2);
        }

        void WriteEdgeWedge(TextWriter w, HpglEdgeWedgeShape s)
        {
            CheckPen(w, s);
            var p1 = s.Center;
            if (!PointEQ(mCurrent, p1))
            {
                mCurrent.Set(p1);
                w.WriteLine($"PU{ConvertPoint(p1)};");
            }
            var r = ConvertLength(s.Radius);
            if (s.Tolerance != 5)
            {
                w.WriteLine($"EW{r:0.#####},{s.StartAngleDeg:0.#####},{s.SweepAngleDeg:0.#####},{s.Tolerance:0.#####};");
            }
            else
            {
                w.WriteLine($"EW{r:0.#####},{s.StartAngleDeg:0.#####},{s.SweepAngleDeg:0.#####};");
            }
        }

        void WriteEdgeRectangle(TextWriter w, HpglEdgeRectangleShape s)
        {
            CheckPen(w, s);
            var pa = new HpglPoint[]
            {
                s.P0, new HpglPoint(s.P0.X, s.P1.Y),
                s.P1, new HpglPoint(s.P1.X, s.P0.Y),
            };
            var rMin = double.MaxValue;
            var iMin = 0;
            for (var i = 0; i < 4; i++)
            {
                if (rMin > (pa[i] - mCurrent).Hypot())
                {
                    rMin = (pa[i] - mCurrent).Hypot();
                    iMin = i;
                }
            }
            var p1 = pa[iMin];
            if (!FloatEQ(rMin, 0))
            {
                w.WriteLine($"PU{ConvertPoint(p1)};");
                mIsPenDown = false;
                mCurrent.Set(p1);
            }
            {
                var p2 = pa[(iMin + 2) % 4];
                w.WriteLine($"EA{ConvertPoint(p2)};");
            }
        }
        void WritePolygon(TextWriter w, HpglPolygonShape s)
        {
            if (s.PolygonBufferList.Count == 0) return;
            //            w.Write($"PM0;");
            int polygonMode = 0;

            for (var i = 0; i < s.PolygonBufferList.Count; i++)
            {
                var pb = s.PolygonBufferList[i];
                if (pb.Count == 0) continue;
                var writer = new HpglWriter();
                writer.mIsPolygonBuffer = true;
                writer.mPolygonMode = polygonMode;

                foreach (var ss in pb)
                {
                    if (ss is HpglLineShape || ss is HpglCircleShape || ss is HpglArcShape)
                    {
                        writer.Shapes.Add(ss);
                    }
                }
                writer.Write(w);
                polygonMode = 1;
            }
            if (polygonMode == 0) return;
            w.Write($"PM2;");
            if (s.FillPen >= 0)
            {
                CheckPen(w, s.FillPen);
                w.Write("FP;");
            }
            if (s.EdgePen >= 0)
            {
                CheckPen(w, s.EdgePen);
                w.Write("EP;");
            }
            w.WriteLine();
        }

        void WriteLabel(TextWriter w, HpglLabelShape s)
        {
            CheckPen(w, s);
            var p1 = s.P0;
            if (!PointEQ(mCurrent, p1))
            {
                w.WriteLine($"PU{ConvertPoint(p1)};");
                mCurrent.Set(p1);
            }
            if (!FloatEQ(mLabelShape.Slant, s.Slant))
            {
                w.Write($"SL{s.Slant:0.#####};");
                mLabelShape.Slant = s.Slant;
            }
            if (!FloatEQ(mLabelShape.AngleDeg, s.AngleDeg))
            {
                var a = s.AngleDeg * Math.PI / 180;
                var dix = Math.Cos(a);
                var diy = Math.Sin(a);
                w.Write($"DI{dix:0.#####},{diy:0.#####};");
                mLabelShape.AngleDeg = s.AngleDeg;
            }
            if (!FloatEQ(mLabelShape.FontWidth, s.FontWidth) || !FloatEQ(mLabelShape.FontHeight, s.FontHeight))
            {
                w.Write($"SI{s.FontWidth / 10:0.#####},{s.FontHeight / 10:0.#####};");
                mLabelShape.FontWidth = s.FontWidth;
                mLabelShape.FontHeight = s.FontHeight;
            }
            if (!FloatEQ(mLabelShape.LetterSpace, s.LetterSpace) || !FloatEQ(mLabelShape.LineSpace, s.LineSpace))
            {
                var letterSpacing = s.LetterSpace / (1.5 * s.FontWidth) - 1.0;
                var lineSpacing = s.LineSpace / (2 * s.FontHeight) - 1.0;
                w.Write($"ES{letterSpacing:0.#####},{lineSpacing:0.#####};");
                mLabelShape.LetterSpace = s.LetterSpace;
                mLabelShape.LineSpace = s.LineSpace;
            }
            if (mLabelShape.Origin != s.Origin)
            {
                w.Write($"LO{s.Origin};");
                mLabelShape.Origin = s.Origin;
            }
            w.WriteLine($"LB{s.Text}\x03;");
            {
                var n = s.Text.Length;
                var dx = (s.FontWidth + s.LetterSpace) * n;
                var dy = (s.FontHeight + s.LineSpace) * 0;
                var a = s.AngleDeg * Math.PI / 180;
                mCurrent.X += dx * Math.Cos(a) - dy * Math.Sin(a);
                mCurrent.Y += dx * Math.Sin(a) + dy * Math.Cos(a);
            }
        }

        void UpdateLines(TextWriter w)
        {
            if (mLinePoints.Count == 0) return;
            if (!mIsPenDown)
            {
                w.Write($"PU{ConvertPoint(mLinePoints[0])};");
                w.Write("PD");
                if (mLinePoints.Count == 1)
                {
                    w.WriteLine(";");
                }
                else
                {
                    for (var i = 1; i < mLinePoints.Count; i++)
                    {
                        w.Write($"{ConvertPoint(mLinePoints[i])}");
                        if (i == mLinePoints.Count - 1)
                        {
                            w.Write(";");
                        }
                        else
                        {
                            w.Write(",");
                        }
                    }
                    w.WriteLine();
                }
            }
            else
            {
                w.Write("PA");
                for (var i = 0; i < mLinePoints.Count; i++)
                {
                    w.Write($"{ConvertPoint(mLinePoints[i])}");
                    if (i == mLinePoints.Count - 1)
                    {
                        w.Write(";");
                    }
                    else
                    {
                        w.Write(",");
                    }
                }
                w.WriteLine();
            }
            mIsPenDown = true;
            mLinePoints.Clear();
        }

        HpglPoint ConvertPoint(HpglPoint p) => p / mmPerUnit;
        double ConvertLength(double a) => a / mmPerUnit;

    }
}
