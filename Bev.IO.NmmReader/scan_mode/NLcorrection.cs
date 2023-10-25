using System;
using System.IO;

namespace Bev.IO.NmmReader.scan_mode
{
    public class NLcorrection
    {
        private const double daiCorrection = 0.5e-9;

        public CorrectionStatus Status { get; private set; } = CorrectionStatus.Unknown;
        public double[] CorrectedData { get; }
        public double CorrectionSpan { get; private set; } = 0.0;

        public NLcorrection(double[] rawData, double[] sinValues, double[] cosValues, double empiricalCorrection)
        {
            NLcorrectionHeydemann heydemann = new NLcorrectionHeydemann(rawData, sinValues, cosValues);

            DebugLog(sinValues, cosValues, heydemann.CorrectedSinValues, heydemann.CorrectedCosValues);

            NLcorrectionDai gaoliang = new NLcorrectionDai(heydemann.CorrectedData, heydemann.CorrectedSinValues, heydemann.CorrectedCosValues, empiricalCorrection);
            //NLcorrectionDai gaoliang = new NLcorrectionDai(heydemann.CorrectedData, sinValues, cosValues, empiricalCorrection);
            CorrectedData = gaoliang.CorrectedData;
            CorrectionSpan = heydemann.CorrectionSpan + gaoliang.CorrectionSpan;
            Status = heydemann.Status;
        }

        public NLcorrection(double[] rawData, double[] sinValues, double[] cosValues) : this(rawData, sinValues, cosValues, daiCorrection) { }

        private void DebugLog(double[] sin, double[] cos, double[] sinCor, double[] cosCor)
        {
            using (StreamWriter writer = new StreamWriter("quadrature.csv"))
            {
                for (int i = 0; i < sin.Length; i += 97)
                {
                    double s = sin[i];
                    double c = cos[i];
                    double sc = sinCor[i];
                    double cc = cosCor[i];
                    writer.WriteLine($"{s}, {c}, {PhiDeg(s,c)}, {Radius(s,c)}, {sc}, {cc}, {PhiDeg(sc, cc)}, {Radius(sc, cc)}");
                }
            }
        }

        private double Radius(double x, double y) => Math.Sqrt(x * x + y * y);
        
        private double PhiDeg(double x, double y) => Math.Atan2(y, x)* 180/Math.PI;

    }
}
