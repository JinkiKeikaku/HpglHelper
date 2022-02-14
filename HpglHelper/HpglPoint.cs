namespace HpglHelper
{
    /// <summary>
    /// 点クラス
    /// </summary>
    public class HpglPoint
    {
        /// <summary>
        /// Ｘ座標
        /// </summary>
        public double X;

        /// <summary>
        /// Ｙ座標
        /// </summary>
        public double Y;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HpglPoint(double x = 0, double y = 0)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// メンバへの代入。X=x, Y=y
        /// </summary>
        public void Set(double x, double y)
        {
            X = x; 
            Y = y;
        }

        /// <summary>
        /// メンバへの代入。X=p.X, Y=p.Y
        /// </summary>
        public void Set(HpglPoint p)
        {
            X = p.X;
            Y = p.Y;
        }

        /// <summary>
        /// オフセット。P.X += dx, P.Y+ = dy
        /// </summary>
        public void Offset(double dx, double dy)
        {
            X += dx;
            Y += dy;
        }

        /// <summary>
        /// オフセット。P＝P＋ｄｐ
        /// </summary>
        public void Offset(HpglPoint dp)
        {
            Offset(dp.X, dp.Y);
        }
        /// <summary>
        /// 点の距離、もしくはベクトル長。Sqrt(X^2 + Y^2)
        /// </summary>
        public double Hypot() => Math.Sqrt(X * X + Y * Y);

        /// <summary>
        /// 表示文字列を返す。この形式でhpglファイルの点を描きだすので注意。
        /// </summary>
        public override string ToString() => $"{X:0.#####},{Y:0.#####}";


        /// <summary>
        /// 定数の掛け算
        /// </summary>
        public static HpglPoint operator *(HpglPoint p, double a)
        {
            return new HpglPoint(p.X * a, p.Y * a);
        }
        /// <summary>
        /// 定数の割り算
        /// </summary>
        public static HpglPoint operator /(HpglPoint p, double a)
        {
            return new HpglPoint(p.X / a, p.Y / a);
        }
        /// <summary>
        /// 減算
        /// </summary>
        public static HpglPoint operator -(HpglPoint p1, HpglPoint p2)
        {
            return new HpglPoint(p1.X - p2.X, p1.Y - p2.Y);
        }
        /// <summary>
        /// 加算
        /// </summary>
        public static HpglPoint operator +(HpglPoint p1, HpglPoint p2)
        {
            return new HpglPoint(p1.X + p2.X, p1.Y + p2.Y);
        }
        /// <summary>
        /// FloatEQ()などに使う比較の誤差。この値より小さかれば等しい。
        /// </summary>
        public static double Epsilon { get; set; } = 0.000001;
        /// <summary>
        /// 誤差を考慮した比較。比較して誤差がEpsilonより小さければ等しい。
        /// </summary>
        public static bool FloatEQ(double a, double b) => Math.Abs(a - b) < Epsilon;

        /// <summary>
        /// 誤差を考慮した点の比較。Epsilonより誤差がＸ，Ｙ共に小さければ等しい。
        /// </summary>
        public static bool PointEQ(HpglPoint p1, HpglPoint p2) 
        {
            return FloatEQ(p1.X, p2.X) && FloatEQ(p1.Y, p2.Y);
        }

    }
}
