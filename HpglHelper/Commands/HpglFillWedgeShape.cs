using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HpglHelper.Commands
{
    public class HpglFillWedgeShape : HpglFillShape
    {
        /// <summary>
        /// 中心
        /// </summary>
        public HpglPoint Center { get; set; } = new();

        /// <summary>
        /// 半径
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// 扁平率。保存時は無視されます（保存時は常に1.0として処理）。
        /// </summary>
        public double Flatness { get; set; } = 1.0;

        /// <summary>
        /// 開始角。
        /// </summary>
        public double StartAngleDeg { get; set; }

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
    }
}


