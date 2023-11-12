using System;

namespace Bev.IO.NmmReader.scan_mode
{
    public class Quad
    {
        public double Sin { get; }
        public double Cos { get; }
        public double Radius => Math.Sqrt(Sin * Sin + Cos * Cos);
        public double Phi => Math.Atan2(Cos, Sin);
        public double PhiDeg => Phi * 180 / Math.PI;

        public Quad(double sin, double cos)
        {
            Sin = sin;
            Cos = cos;
        }

        public Quad(double radius, double angle, AngleUnit unit)
        {
            if (unit == AngleUnit.Degree)
                angle = angle * Math.PI / 180;
            Sin = radius * Math.Cos(angle);
            Cos = radius * Math.Sin(angle);
        }
    }

    public enum AngleUnit
    {
        Radian,
        Degree
    }
}
