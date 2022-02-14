namespace HpglHelper.Commands
{
    /// <summary>
    /// hpglのポリゴンモードの図形を表現するクラス。
    /// </summary>
    public class HpglPolygonShape : HpglShape
    {
        /// <summary>
        /// 外周のペン番号。-1の場合は外周描画なし。
        /// </summary>
        public int EdgePen = -1;

        /// <summary>
        /// 塗りのペン番号。-1の場合は塗りなし。
        /// </summary>
        public int FillPen = -1;

        /// <summary>
        /// 図形が入ったポリゴンバッファを追加する。
        /// </summary>
        public void Add(List<HpglShape> buffer)
        {
            PolygonBufferList.Add(buffer);
        }
        /// <summary>
        /// ポリゴンモードで描画する図形のリストのリスト。線と円と円弧のみ。
        /// </summary>
        public List<List<HpglShape>> PolygonBufferList { get; } = new();
    }
}
