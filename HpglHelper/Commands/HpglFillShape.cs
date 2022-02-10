namespace HpglHelper.Commands
{
    public class HpglFillShape : HpglShape
    {
        public HpglFillType FillType { get; set; } = new();
        /// <summary>
        /// 塗りつぶしペン幅。単位はｍｍ。デフォルトは0.3mm
        /// </summary>
        public double PenThickness { get; set; } = 0.3;


    }
}
