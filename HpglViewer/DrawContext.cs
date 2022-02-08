using System.Collections.Generic;
using System.Drawing;

namespace HpglViewer
{
    /// <summary>
    /// 描画用の情報保持クラス
    /// </summary>
    class DrawContext
    {
        public Pen Pen = new(Color.Black, 0.0f);
        public float Scale = 4.0f;
        //用紙サイズは固定
        public SizeF PaperSize = new Size(1000, 1000);


        public DrawContext(float paperWidth, float paperHeight)
        {
            PaperSize = new SizeF(paperWidth*2, paperHeight*2);
        }

        /// <summary>
        /// DocumentとGDI+の半径などの変換。
        /// </summary>
        public float DocToCanvas(double radius)
        {
            return (float)radius;
        }

        /// <summary>
        /// DocumentとGDI+の座標変換。Jwwは上が正なのでｙ座標のみ符号を変える。
        /// </summary>
        public PointF DocToCanvas(double x, double y)
        {
            return new PointF((float)x, (float)-y);
        }

        /// <summary>
        /// DocumentとGDI+の座標変換。Jwwは上が正なのでｙ座標のみ符号を変える。
        /// </summary>
        public PointF DocToCanvas(CadPoint p)
        {
            return DocToCanvas(p.X, p.Y);
        }

        /// <summary>
        /// DocumentとGDI+の角度の変換。Jwwの角度は左回り。GDI+は右回り。
        /// 符号を変えるだけだが座標変換に合わせて間違えないようにあえてこれを使う。
        /// </summary>
        public float DocToCanvasAngle(double angle)
        {
            return -(float)angle;
        }
    }
}
