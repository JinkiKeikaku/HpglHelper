using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HpglHelper.Commands
{
    public class HpglPolygonShape : HpglShape
    {
        public int EdgePen = -1;
        public int FillPen = -1;
        public List<List<HpglShape>> PolygonBufferList { get; } = new();
        public void Add(List<HpglShape> buffer)
        {
            PolygonBufferList.Add(buffer);
        }
    }
}
