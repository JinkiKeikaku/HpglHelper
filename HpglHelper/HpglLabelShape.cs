namespace HpglHelper
{
    public class HpglLabelShape : HpglShape
    {
        public HpglPoint P0 { get; set; } = new();
        public double AngleDeg { get; set; } = 0;
        public string Text { get; set; } = "";
        /// <summary>
        /// 文字幅。単位はセンチ。
        /// </summary>
        public double FontWidth { get; set; } = 0.285;
        /// <summary>
        /// 文字高さ。単位はセンチ。
        /// </summary>
        public double FontHeight { get; set; } = 0.375;
        /// <summary>
        /// 文字の傾き（tanΘ）。
        /// </summary>
        public double Slant { get; set; } = 0;
        /// <summary>
        /// 文字原点
        /// 1:左下、２:左中、3:左上、4:中下、5:中中、6:中上、7:右下、8:右中、9:右上
        /// ＋10で中以外の方向は文字幅文字高さの半分移動する。
        /// </summary>
        public int Origin { get; set; } = 1;


    }
}
