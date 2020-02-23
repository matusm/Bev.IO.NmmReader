﻿using System;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;

namespace Bev.IO.NmmReader.scan_mode
{
    public class Heydemann
    {
        private const double lambda2 = 316.4e-9; // Lambda/2

        #region Properties
        public CorrectionStatus Status { get; private set; } = CorrectionStatus.Unknown;
        public double CorrectionSpan { get; private set; } = 0.0;
        public double[] CorrectedData { get; private set; }
        // the 5 parameters characterizing an ellipse in the plane
        // the init values will result in a 0-correction
        public double OffsetX { get; private set; } = 0.0;
        public double OffsetY { get; private set; } = 0.0;
        public double Phase { get; private set; } = 0.0;
        public double Amplitude { get; private set; } = 1.0;
        public double AmplitudeRelation { get; private set; } = 1.0;
        #endregion

        public Heydemann(double[] rawData, double[] sinValues, double[] cosValues)
        {
            PerformCorrection(rawData, sinValues, cosValues);
        }

        #region Private stuff
        private void PerformCorrection(double[] rawData, double[] sinValues, double[] cosValues)
        {
            Status = CorrectionStatus.Uncorrected;
            CorrectedData = new double[rawData.Length];
            Array.Copy(rawData, CorrectedData, rawData.Length);
            if (rawData.Max() - rawData.Min() < lambda2)
            {
                Status = CorrectionStatus.UncorrectedRangeTooSmall;
                return;
            }
            if (sinValues.Length != cosValues.Length)
            {
                Status = CorrectionStatus.UncorrectedInconsitentData;
                return;
            }
            if (sinValues.Length != rawData.Length)
            {
                Status = CorrectionStatus.UncorrectedInconsitentData;
                return;
            }
            if (sinValues.Length < 5) // need more than 5 data points to perform fit
            {
                Status = CorrectionStatus.UncorrectedTooFewData;
                return;
            }
            FitEllipse(sinValues, cosValues);
            // now the ellipse parameters are valid
            double deviation;
            double maxDeviation = double.MinValue;
            double minDeviation = double.MaxValue;
            for (int i = 0; i < rawData.Length; i++)
            {
                deviation = HeydemannDeviationForPoint(sinValues[i], cosValues[i]);
                CorrectedData[i] = rawData[i] - deviation;
                if (deviation > maxDeviation) maxDeviation = deviation;
                if (deviation < minDeviation) minDeviation = deviation;
            }
            CorrectionSpan = maxDeviation - minDeviation;
            Status = CorrectionStatus.Corrected;
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
            Phase = Math.Asin(sC / Math.Sqrt(4.0 * sA * sB));
            // % Amplitude relation r
            // r=(B/A)^0.5;
            AmplitudeRelation = Math.Sqrt(sB / sA);
            // % Offset x
            // p=(2*B*D-E*C)/(C^2-4*A*B);
            OffsetX = (2.0 * sB * sD - sE * sC) / (sC * sC - 4.0 * sA * sB);
            // % Offset y
            // q=(2*A*E-D*C)/(C^2-4*A*B);
            OffsetY = (2.0 * sA * sE - sD * sC) / (sC * sC - 4.0 * sA * sB);
            // % Amplitude R
            // R=((p^2+r^2*q^2+2*r*p*q*sin(alpha))/cos(alpha)^2+1/(A*cos(alpha)^2))^0.5;
            double x1 = (OffsetX * OffsetX + AmplitudeRelation * AmplitudeRelation * OffsetY * OffsetY + 2.0 * AmplitudeRelation * OffsetX * OffsetY * Math.Sin(Phase)) / (Math.Cos(Phase) * Math.Cos(Phase));
            double x2 = 1.0 / (sA * Math.Cos(Phase) * Math.Cos(Phase));
            Amplitude = Math.Sqrt(x1 + x2);
        }

        private double HeydemannDeviationForPoint(double sin, double cos)
        {
            double sinc = sin - OffsetX;
            double cosc = sinc * Math.Sin(Phase) + AmplitudeRelation * (cos - OffsetY) / Math.Cos(Phase);
            double deviation = (lambda2 / (2.0 * Math.PI)) * (Math.Atan2(cos, sin) - Math.Atan2(cosc, sinc));
            if (deviation > 300e-9) deviation -= lambda2;
            if (deviation < -300e-9) deviation += lambda2;
            return deviation;
        }
        #endregion
    }

    public enum CorrectionStatus
    {
        Unknown,
        Uncorrected,
        UncorrectedInconsitentData,
        UncorrectedTooFewData,
        UncorrectedRangeTooSmall,
        Corrected
    }
}
