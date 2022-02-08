using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using static System.MathF;

namespace HpglViewer
{
    static class Helpers
    {
        /// <summary>
        /// RadianからDegreeに変換。
        /// </summary>
        public static double RadToDeg(double angleRad)
        {
            return 180.0 * angleRad / Math.PI;
        }

        public static double DegToRad(double angleDeg)
        {
            return angleDeg * Math.PI / 180.0;
        }

        /// <summary>
        /// 点の引き算
        /// </summary>
        public static PointF Sub (this PointF p1, PointF p2)
        {
            return new PointF(p1.X - p2.X, p1.Y - p2.Y);
        }

        /// <summary>
        /// 点の足し算
        /// </summary>
        public static PointF Add (this PointF p1, PointF p2)
        {
            return new PointF(p1.X + p2.X, p1.Y + p2.Y);
        }

        /// <summary>
        /// 点の回転。角度単位はRadian。
        /// </summary>
        public static PointF Rotate(this PointF p, float rad)
        {
            var c = Cos(rad);
            var s = Sin(rad);
            return new PointF(p.X * c - p.Y * s, p.X * s + p.Y * c);
        }

        /// <summary>
        /// 誤差を含めた比較。ABS([x]-[y])が誤差より小さければtrue。
        /// </summary>
        public static bool FloatEQ(float x, float y)
        {
            return Abs(x - y) < 0.00001f;
        }

        /// <summary>
        /// 直線[p11]-[p12]と[p21]-[p22]の交点を返す。交点がない場合はタプルの[flag]がfalse。
        /// </summary>
        public static (PointF p, bool flag) GetCrossPoint(PointF p11, PointF p12, PointF p21, PointF p22)
        {
            var dp1 = Sub(p12, p11);
            var dp2 = Sub(p22, p21);
            var dp3 = Sub(p11, p21);
            var a = dp1.X * dp2.Y - dp2.X * dp1.Y;
            if (FloatEQ(a, 0.0f)) return (new PointF(), false);
            var t = (dp2.X * dp3.Y - dp3.X * dp2.Y) / a;
            var cp = new PointF(dp1.X * t + p11.X, dp1.Y * t + p11.Y);
            return (cp, true);
        }
    }
}
