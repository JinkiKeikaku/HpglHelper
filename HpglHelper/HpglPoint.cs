namespace HpglHelper
{
    public class HpglPoint
    {
        public double X;
        public double Y;
        public HpglPoint(double x = 0, double y = 0)
        {
            X = x;
            Y = y;
        }
        public void Set(double x, double y)
        {
            X = x; 
            Y = y;
        }
        public void Set(HpglPoint p)
        {
            X = p.X;
            Y = p.Y;
        }
        public void Offset(double dx, double dy)
        {
            X += dx;
            Y += dy;
        }
        public void Offset(HpglPoint dp)
        {
            Offset(dp.X, dp.Y);
        }

        public double Hypot() => Math.Sqrt(X * X + Y * Y);

        public override string ToString() => $"{X:0.#####},{Y:0.#####}";


        public static HpglPoint operator *(HpglPoint p, double a)
        {
            return new HpglPoint(p.X * a, p.Y * a);
        }
        public static HpglPoint operator /(HpglPoint p, double a)
        {
            return new HpglPoint(p.X / a, p.Y / a);
        }
        public static HpglPoint operator -(HpglPoint p1, HpglPoint p2)
        {
            return new HpglPoint(p1.X - p2.X, p1.Y - p2.Y);
        }
        public static HpglPoint operator +(HpglPoint p1, HpglPoint p2)
        {
            return new HpglPoint(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static double Epsilon { get; set; } = 0.000001;

        public static bool FloatEQ(double a, double b) => Math.Abs(a - b) < Epsilon;

        public static bool PointEQ(HpglPoint p1, HpglPoint p2) 
        {
            return FloatEQ(p1.X, p2.X) && FloatEQ(p1.Y, p2.Y);
        }

    }
}
