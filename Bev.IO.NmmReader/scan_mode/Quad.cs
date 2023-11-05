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

        public Quad FromPolar(double radius, double angle)
        {
            double s = radius * Math.Cos(angle);
            double c = radius * Math.Sin(angle);
            return new Quad(s, c);
        }

    }
}
