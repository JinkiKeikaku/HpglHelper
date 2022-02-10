namespace HpglHelper.Commands
{
    /// <summary>
    /// IPコマンド（スケーリングポイントP1、P2を設定）
    /// </summary>
    public class HpglIPCommand : HpglCommand
    {
        /// <summary>
        /// スケーリングポイントP1 X
        /// </summary>
        public int P1X { get; set; }
        /// <summary>
        /// スケーリングポイントP1 Y
        /// </summary>
        public int P1Y { get; set; }
        /// <summary>
        /// スケーリングポイントP2 X
        /// </summary>
        public int P2X { get; set; }
        /// <summary>
        /// スケーリングポイントP2 Y
        /// </summary>
        public int P2Y { get; set; }
    }
}
