using System;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;

namespace Bev.IO.NmmReader.scan_mode
{
    public class Heydemann
    {

        public CorrectionStatus Status { get; private set; } = CorrectionStatus.Unknown;
        private double[] correctedData;

        public Heydemann()
        {
        }

        public void PerformCorrection(double[] rawData, double[] sinValues, double[] cosValues)
        {
            Status = CorrectionStatus.Uncorrected;
            correctedData = new double[rawData.Length];
            Array.Copy(rawData, correctedData, rawData.Length);
            if (sinValues.Length != cosValues.Length) return;
            if (sinValues.Length != rawData.Length) return;
            if (sinValues.Length < 5) return; // need more than 5 data points to perform fit
            FitEllipse(sinValues, cosValues);
        }

        private void FitEllipse(double[] sin, double[] cos)
        {
            var M = Matrix<double>.Build;
            var V = Vector<double>.Build;
            double[,] matMtemp = new double[5, sin.Length];
            // M=[ks.*ks,kc.*kc,ks.*kc,ks,kc]
            for (int i = 0; i < sin.Length; i++)
            {
                matMtemp[0, i] = sin[i] * sin[i];
                matMtemp[1, i] = cos[i] * cos[i];
                matMtemp[2, i] = sin[i] * cos[i];
                matMtemp[3, i] = sin[i];
                matMtemp[4, i] = cos[i];
            }
            var matM = M.DenseOfArray(matMtemp);
            // P=inv(M'*M)
            var matQ = matM * matM.Transpose();
            var matP = matQ.Inverse();
            // s=P*M'*X;
            var matX = V.Dense(sin.Length, 1.0);
            // var matS = matP * matM * matX;
            var matT = matM * matX;
            var matS = matP * matT;
            double sA = matS[0];
            double sB = matS[1];
            double sC = matS[2];
            double sD = matS[3];
            double sE = matS[4];
            // % Phase deviation
            // alpha=asin((C/(4*A*B)^0.5));
            // % alphagrd=alpha*180/pi;
            double phi = Math.Asin(sC / Math.Sqrt(4.0 * sA * sB));
            // % Amplitude relation r
            // r=(B/A)^0.5;
            double r_rel = Math.Sqrt(sB / sA);
            // % Offset x
            // p=(2*B*D-E*C)/(C^2-4*A*B);
            double p = (2.0 * sB * sD - sE * sC) / (sC * sC - 4.0 * sA * sB);
            // % Offset y
            // q=(2*A*E-D*C)/(C^2-4*A*B);
            double q = (2.0 * sA * sE - sD * sC) / (sC * sC - 4.0 * sA * sB);
            // % Amplitude R
            // R=((p^2+r^2*q^2+2*r*p*q*sin(alpha))/cos(alpha)^2+1/(A*cos(alpha)^2))^0.5;
            double x1 = (p * p + r_rel * r_rel * q * q + 2.0 * r_rel * p * q * Math.Sin(phi)) / (Math.Cos(phi) * Math.Cos(phi));
            double x2 = 1.0 / (sA * Math.Cos(phi) * Math.Cos(phi));
            double r = Math.Sqrt(x1 + x2);
        }

    }

    public enum CorrectionStatus
    {
        Unknown,
        Uncorrected,
        Corrected
    }
}
