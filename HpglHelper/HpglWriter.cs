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
        readonly double mmPerUnit = 0.025;//0.025mm
        HpglPoint mCurrent = new(0, 0);
        List<HpglPoint> mLinePoints = new(); //Line用の点バッファ
        bool mIsPneDown = false;
        int mSelectedPen = 1;
        public List<HpglShape> Shapes { get; } = new();

        public void Write(TextWriter w)
        {
            w.WriteLine($"IN;PU0,0;SP{mSelectedPen};");//初期化とペンアップで原点、ペン選択
            w.WriteLine($"IP0,0,4000,4000; SC0,100,0,100;");//座標単位をmmにする。
            mCurrent.Set(0, 0);
            mIsPneDown = false;

            var items = new List<Item?>();
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

            w.WriteLine($"PU0,0;");//終わったら原点に戻す。

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
        List<Item> GetSortedItems(List<Item> src)
        {
            var dst = new List<Item>();
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
                        p0[0]=src[i0]!.Terminal[1];
                    }
                    else
                    {
                        p0[0]=src[i0]!.Terminal[0];
                    }
                    dst.Insert(0, src[i0]!);
                }
                if (i0 >= 0 && c1 == 1)
                {
                    if (PointEQ(p0[1], src[i0]!.Terminal[0]))
                    {
                        p0[1]=src[i0]!.Terminal[1];
                    }
                    else
                    {
                        p0[1]=src[i0]!.Terminal[0];
                    }
                    dst.Add(src[i0]!);
                }
                if(i0 >= 0)
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
                _ => null,
            };
        }

        (int pos, int ip) GetNearPoint(HpglPoint p, List<Item> items)
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
            }
        }

        void CheckPen(TextWriter w, HpglShape s)
        {
            if (mSelectedPen != s.PenNumber)
            {
                mSelectedPen = s.PenNumber;
                w.Write($"SP{mSelectedPen};");
            }
        }


        void WriteLine(TextWriter w, HpglLineShape s, int iTerminal)
        {
            var (p1, p2) = iTerminal == 0 ? (s.P0, s.P1) : (s.P1, s.P0);
            if (!PointEQ(mCurrent, p1) || mSelectedPen != s.PenNumber)
            {
                UpdateLines(w);
                mIsPneDown = false;
            }
            CheckPen(w, s);
            if (mLinePoints.Count == 0) mLinePoints.Add(p1);
            mLinePoints.Add(p2);
            mCurrent.Set(p2);
        }
        void WriteCircle(TextWriter w, HpglCircleShape s)
        {
            CheckPen(w, s);
            var p1 = s.Center;
            if (!PointEQ(mCurrent, p1))
            {
                w.WriteLine($"PU{p1.X:0.#####},{p1.Y:0.#####};");
                mCurrent.Set(p1);
            }
            if (s.Tolerance != 5)
            {
                w.WriteLine($"CI{s.Radius:0.#####},{s.Tolerance:0.#####};");
            }
            else
            {
                w.WriteLine($"CI{s.Radius:0.#####};");
            }
        }
        void WriteArc(TextWriter w, HpglArcShape s, int iTerminal)
        {
            CheckPen(w, s);
            var (p1, p2) = iTerminal == 0 ? (s.StartPoint, s.EndPoint) : (s.EndPoint, s.StartPoint);
            var a = iTerminal == 0 ? s.SweepAngleDeg : -s.SweepAngleDeg;
            var p0 = s.Center;
            if (!PointEQ(mCurrent, p1))
            {
                w.WriteLine($"PU{p1.X:0.#####},{p1.Y:0.#####};");
                mIsPneDown = false;
            }
            if (!mIsPneDown)
            {
                w.Write("PD;");
                mIsPneDown = true;
            }
            if (s.Tolerance != 5)
            {
                w.WriteLine($"PD;AA{p0.X:0.#####},{p0.Y:0.#####},{a:0.#####},{s.Tolerance:0.#####};");
            }
            else
            {
                w.WriteLine($"PD;AA{p0.X:0.#####},{p0.Y:0.#####},{a:0.#####};");
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
                w.WriteLine($"PU{p1.X:0.#####},{p1.Y:0.#####};");
            }
            if (s.Tolerance != 5)
            {
                w.WriteLine($"EW{s.Radius:0.#####},{s.StartAngleDeg:0.#####},{s.SweepAngleDeg:0.#####},{s.Tolerance:0.#####};");
            }
            else
            {
                w.WriteLine($"EW{s.Radius:0.#####},{s.StartAngleDeg:0.#####},{s.SweepAngleDeg:0.#####};");
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
                w.WriteLine($"PU{p1.X:0.#####},{p1.Y:0.#####};");
                mIsPneDown = false;
                mCurrent.Set(p1);
            }
            var p2 = pa[(iMin + 2) % 4];
            w.WriteLine($"EA{p2.X:0.#####},{p2.Y:0.#####};");
        }

        void WriteLabel(TextWriter w, HpglLabelShape s)
        {
            CheckPen(w, s);
            var p1 = s.P0;
            if (!PointEQ(mCurrent, p1))
            {
                w.WriteLine($"PU{p1.X:0.#####},{p1.Y:0.#####};");
                mCurrent.Set(p1);
            }
            var a = s.AngleDeg * Math.PI / 180;
            var dix = Math.Cos(a);
            var diy = Math.Sin(a);
            w.WriteLine($"SI{s.FontWidth/10},{s.FontHeight/10};SL{s.Slant};DI{dix},{diy};LO{s.Origin};LB{s.Text}\x03;");
        }

        void UpdateLines(TextWriter w)
        {
            if (mLinePoints.Count == 0) return;
            if (!mIsPneDown)
            {
                w.Write($"PU{mLinePoints[0].X:0.#####},{mLinePoints[0].Y:0.#####};");
                w.Write("PD");
                if (mLinePoints.Count == 1)
                {
                    w.WriteLine(";");
                }
                else
                {
                    for (var i = 1; i < mLinePoints.Count; i++)
                    {
                        w.Write($"{mLinePoints[i].X:0.#####},{mLinePoints[i].Y:0.#####}");
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
                    w.Write($"{mLinePoints[i].X:0.#####},{mLinePoints[i].Y:0.#####}");
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
            mIsPneDown = true;
            mLinePoints.Clear();
        }

        HpglPoint ConvertPoint(HpglPoint p) => p * mmPerUnit;
        double ConvertLength(double a) => a * mmPerUnit;
    }
}
