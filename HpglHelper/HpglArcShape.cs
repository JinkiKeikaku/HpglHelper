namespace HpglHelper
{
    public class HpglArcShape : HpglShape
    {
        /// <summary>
        /// 中心
        /// </summary>
        public HpglPoint Center { get; set; } = new();
        /// <summary>
        /// 円弧の開始点。半径はこの点と中心点の距離から求められる。
        /// 同様に、円弧の開始角度もこの点と中心点の関係から求められる。
        /// </summary>
        public HpglPoint StartPoint { get; set; } = new();
        /// <summary>
        /// 円弧角
        /// </summary>
        public double SweepAngleDeg { get; set; }
        /// <summary>
        /// 分解能モード。Toleranceの値は、
        /// 0：角度。 1:円弧上の2点を通る直線と円弧の間の最長垂線距離。
        /// </summary>
        public int ChordToleranceMode { get; set; } = 0;
        /// <summary>
        /// 分解能。ChordToleranceModeの値により角度もしくは偏倚距離。
        /// </summary>
        public double Tolerance { get; set; } = 5;
        /// <summary>
        /// 半径
        /// </summary>
        public double Radius
        {
            get
            {
                double dx = StartPoint.X - Center.X;
                double dy = StartPoint.Y - Center.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }
        }
        /// <summary>
        /// 開始角
        /// </summary>
        public double StartAngleDeg
        {
            get
            {
                var a = Math.Atan2(StartPoint.Y - Center.Y, StartPoint.X - Center.X);
                return a * 180 / Math.PI;
            }
        }

        public HpglPoint EndPoint
        {
            get
            {
                var a = Math.PI * (StartAngleDeg + SweepAngleDeg) / 180;
                return new HpglPoint(
                    Math.Cos(a)*Radius+Center.X, Math.Sin(a)*Radius+Center.Y);
            }
        }
    }
}
