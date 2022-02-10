using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HpglHelper.Commands
{
    public class HpglFillType
    {
        /// <summary>
        /// 塗りつぶし方法　
        /// 1：双方向（デフォルト、角度無視）　2：単方向（角度無視）3：ハッチング
        /// 4：クロスハッチング
        /// </summary>
        public int FillType { get; set; } = 1;
        /// <summary>
        /// 塗りつぶしの間隔。デフォルト(-1)はP1-P2間の距離の1%、0の時はペン幅になるので注意。
        /// </summary>
        public double FillGap { get; set; } = -1;
        public double FillAngle { get; set; } = 0;

    }
}
