using System;
using At.Matus.StatisticPod;

namespace Bev.IO.NmmReader.scan_mode
{
    public class NLcorrectionDai
    {
        private const double empiricalNLfactor = 305.25e-10; // in m. This is an empirical constant, found to be usefull for 0.5 nm correction

        public CorrectionStatus Status { get; private set; } = CorrectionStatus.Unknown;
        public double CorrectionAmplitude { get; private set; }
        public double CorrectionSpan => CorrectionAmplitude * 2;
        public double[] CorrectedData { get; private set; }

        public NLcorrectionDai(double[] rawData, double[] sinValues, double[] cosValues)
        {
            CorrectionAmplitude = EstimateCorrectionAmplitude(sinValues, cosValues);
            PerformCorrection(rawData, sinValues, cosValues);
        }

        public NLcorrectionDai(double[] rawData, double[] sinValues, double[] cosValues, double empiricalCorrection)
        {
            CorrectionAmplitude = empiricalCorrection;
            PerformCorrection(rawData, sinValues, cosValues);
        }

        private void PerformCorrection(double[] rawData, double[] sinValues, double[] cosValues)
        {
            Status = CorrectionStatus.Uncorrected;
            CorrectedData = new double[rawData.Length];
            Array.Copy(rawData, CorrectedData, rawData.Length);
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

            for (int i = 0; i < rawData.Length; i++)
            {
                CorrectedData[i] += GetCorrection(sinValues[i], cosValues[i]);
            }
        }

        private double Phi(double x, double y) => Math.Atan2(y, x);

        private double GetCorrection(double sin, double cos) => - CorrectionAmplitude * Math.Sin(4 * Phi(sin, cos));

        private double EstimateCircleDeformation(double[] sinValues, double[] cosValues)
        {
            StatisticPod allRadii = new StatisticPod();
            StatisticPod axisRadii = new StatisticPod();
            StatisticPod medianRadii= new StatisticPod();
            for (int i = 0; i < sinValues.Length; i++)
            {
                double s = sinValues[i];
                double c = cosValues[i];
                double r = Radius(s, c);
                double phi = PhiDeg(s, c);
                allRadii.Update(r);
                if (IsAxis(phi)) axisRadii.Update(r);
                if (IsMedian(phi)) medianRadii.Update(r);
            }
            return (medianRadii.AverageValue - axisRadii.AverageValue) / allRadii.AverageValue;
        }

        private double EstimateCorrectionAmplitude(double[] sinValues, double[] cosValues)
        {
            return empiricalNLfactor * EstimateCircleDeformation(sinValues, cosValues);
        }

        private double Radius(double x, double y) => Math.Sqrt(x * x + y * y);

        private double PhiDeg(double x, double y) => Phi(x, y) * 180 / Math.PI;

        private bool IsAxis(double phi)
        {
            if (IsNear(phi, 0)) return true;
            if (IsNear(phi, 90)) return true;
            if (IsNear(phi, 180)) return true;
            if (IsNear(phi, -90)) return true;
            if (IsNear(phi, -180)) return true;
            return false;
        }

        private bool IsMedian(double phi)
        {
            if (IsNear(phi, 45)) return true;
            if (IsNear(phi, 135)) return true;
            if (IsNear(phi, -45)) return true;
            if (IsNear(phi, -135)) return true;
            return false;
        }

        private bool IsNear(double phi, double target)
        {
            const double eps = 2;
            if (Math.Abs(target - phi) < eps) return true;
            return false;
        }
    }
}
