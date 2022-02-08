using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HpglHelper
{
    public class HpglLineShape : HpglShape
    {
        /// <summary>
        /// 始点
        /// </summary>
        public HpglPoint P0 { get; set; } =new();
        /// <summary>
        /// 終点
        /// </summary>
        public HpglPoint P1 { get; set; } = new();

    }
}
