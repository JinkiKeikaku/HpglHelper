namespace HpglHelper.Commands
{
    /// <summary>
    /// 円
    /// </summary>
    public class HpglCircleSahpe : HpglShape
    {
        /// <summary>
        /// 半径
        /// </summary>
        public double Radius { get; set; }
        /// <summary>
        /// 中心
        /// </summary>
        public HpglPoint Center { get; set; } = new();
        /// <summary>
        /// 分解能モード。Toleranceの値は、
        /// 0：角度。 1:円弧上の2点を通る直線と円弧の間の最長垂線距離。
        /// </summary>
        public int ChordToleranceMode { get; set; } = 0;
        /// <summary>
        /// 分解能。ChordToleranceModeの値により角度もしくは偏倚距離。
        /// </summary>
        public double Tolerance { get; set; } = 5;

    }
}
