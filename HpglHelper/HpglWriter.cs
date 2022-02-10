using HpglHelper.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HpglHelper
{
    public class HpglWriter
    {
        readonly double mmPerUnit = 0.025;//0.025mm
        HpglPoint mCurrent = new(0, 0);
        List<HpglPoint> mLinePoints = new(); //Line用の点バッファ
        bool mIsPenDown = false;
        int mSelectedPen = 1;

        public List<HpglCommand> Shapes { get; }=new();
        public void Write(TextWriter w)
        {
            w.WriteLine("IN;PU0,0;SP1;");//初期化とペンアップ
            w.WriteLine($"IP0,0,4000,4000; SC0,100,0,100");
            mCurrent.Set(0, 0);
            mIsPenDown = false;


            foreach (var shape in Shapes)
            {
                w.WriteLine(shape.ToString());
            }

        }

    }
}
