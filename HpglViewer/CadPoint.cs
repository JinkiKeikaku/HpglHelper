using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HpglViewer
{
    class CadPoint
    {
        /// 座標X
        /// </summary>
        public double X { get; set; }
        /// <summary>
        /// 座標Y
        /// </summary>
        public double Y { get; set; }
        /// <summary>
        /// コンストラクター
        /// </summary>
        public CadPoint() { }
        /// <summary>
        /// コンストラクター
        /// </summary>
        public CadPoint(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
        public PointF ToPointF()
        {
            return new PointF((float)X, (float)Y);
        }
        public void Set(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
        public void Set(CadPoint p)
        {
            this.X = p.X;
            this.Y = p.Y;
        }
        /// <summary>
        /// 座標を(0, 0)基準で回転。角度はradian。
        /// </summary>
        public void Rotate(double rad)
        {
            if (Helpers.FloatEQ((float)rad, 0.0f)) return;
            var c = Math.Cos(rad);
            var s = Math.Sin(rad);
            var xx = X * c - Y * s;
            var yy = X * s + Y * c;
            X = xx;
            Y = yy;
        }

        public double GetAngle() => Math.Atan2(Y, X);

        public double Hypot() => Math.Sqrt(X * X + Y * Y);

        public CadPoint UnitPoint()
        {
            var r = Hypot();
            if(r == 0.00001)
            {
                //手抜きで０ベクトルを返す
                return new CadPoint(0,0);
            }
            return new CadPoint(X / r, Y / r);
        }
        public static CadPoint operator* (CadPoint p, double a)
        {
            return new CadPoint(p.X * a, p.Y * a);
        }
        public static CadPoint operator -(CadPoint p1, CadPoint p2)
        {
            return new CadPoint(p1.X - p2.X, p1.Y - p2.Y);
        }
        public static CadPoint operator +(CadPoint p1, CadPoint p2)
        {
            return new CadPoint(p1.X + p2.X, p1.Y + p2.Y);
        }
        public static CadPoint CreateFromAngle(double rad)
        {
            return new CadPoint(Math.Cos(rad), Math.Sin(rad));
        }
        public static CadPoint CreateFromPolar(double radius, double rad)
        {
            return new CadPoint(radius * Math.Cos(rad), radius * Math.Sin(rad));
        }


    }
}
