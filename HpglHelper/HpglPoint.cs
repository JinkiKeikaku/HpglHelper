using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HpglHelper
{
    public class HpglPoint
    {
        public double X;
        public double Y;
        public HpglPoint(double x = 0, double y = 0)
        {
            X = x;
            Y = y;
        }
        public void Set(double x, double y)
        {
            X = x; 
            Y = y;
        }
        public void Set(HpglPoint p)
        {
            X = p.X;
            Y = p.Y;
        }
        public void Offset(double dx, double dy)
        {
            X += dx;
            Y += dy;
        }
        public override string ToString() => $"{X} {Y}";


        public static HpglPoint operator *(HpglPoint p, double a)
        {
            return new HpglPoint(p.X * a, p.Y * a);
        }
        public static HpglPoint operator -(HpglPoint p1, HpglPoint p2)
        {
            return new HpglPoint(p1.X - p2.X, p1.Y - p2.Y);
        }
        public static HpglPoint operator +(HpglPoint p1, HpglPoint p2)
        {
            return new HpglPoint(p1.X + p2.X, p1.Y + p2.Y);
        }



    }
}
