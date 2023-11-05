//*******************************************************************************************
//
// Class to compensate specific periodic errors in signals of homodyne laser interferometers
// 
// To have an ideal circular Lissajous trajectory, the interferometer signals (sin, cos)
// should have zero offset, identical amplitudes, and 90° phase difference.
// Deviations from the ideal case leads to an eliptical Lissajous trajectory which can
// be corrected by the NLcorrectionHeydemann (2th-order nonlinearity).
//
// In the NMM-1 additional nonlinearity contributions are present (4th-order).
// This specific nonlinearity is corrected by this class.
//
//
// Usage:
// 1.) create an instance of NLcorrectionDai with three arrays as parameters:
//     - rawData: the actual length values in m to be corrected
//     - sinValues: the respective sin-signal of the interferometer
//     - cosValues: the respective cos-signal of the interferometer
// 2.) consume the corrected data via the CorrectedData property
// 
// A constructor with an additional parameter (empirical correction amplitude)
// is also provided.
//
// There are no user accessible methods for this class.
// However some properties (getters) provide details on the correction status,
// most important is CorrectionSpan (in m) the maximum correction applied,
// and Status which gives information if there was an correction at all.
// in cases where there is no correction possible, CorrectionData just equals rawData.
// 
// The whole calculation is performed in the constructor only.
// 
// Author: Michael Matus, 2023
//
//*******************************************************************************************


using System;
using At.Matus.StatisticPod;

namespace Bev.IO.NmmReader.scan_mode
{
    public class NLcorrectionDai
    {
        public CorrectionStatus Status { get; private set; } = CorrectionStatus.Unknown;
        public double CorrectionAmplitude { get; private set; }
        public double CorrectionSpan => CorrectionAmplitude * 2;
        public double[] CorrectedData { get; private set; }
        public double[] CorrectedSinValues { get; private set; }
        public double[] CorrectedCosValues { get; private set; }

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
            CorrectedSinValues = new double[rawData.Length];
            CorrectedCosValues = new double[rawData.Length];
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
                // write corrected quadrature signals
                CorrectedSinValues[i] = SinCor(sinValues[i], cosValues[i]);
                CorrectedCosValues[i] = CosCor(sinValues[i], cosValues[i]);
            }
        }

        private double Phi(double x, double y) => Math.Atan2(y, x);

        private double GetCorrection(double sin, double cos) => -CorrectionAmplitude * Math.Sin(4 * Phi(sin, cos));

        private double EstimateCircleDeformation(double[] sinValues, double[] cosValues)
        {
            StatisticPod allRadii = new StatisticPod();
            StatisticPod axisRadii = new StatisticPod();
            StatisticPod medianRadii = new StatisticPod();
            for (int i = 0; i < sinValues.Length; i++)
            {
                double s = sinValues[i];
                double c = cosValues[i];
                double r = Radius(s, c);
                double phi = PhiDeg(s, c);
                allRadii.Update(r);
                if (IsNearToAxis(phi)) axisRadii.Update(r);
                if (IsNearToMedian(phi)) medianRadii.Update(r);
            }
            absoluteDeviation = medianRadii.AverageValue - axisRadii.AverageValue;
            double relativeDeviation = absoluteDeviation / allRadii.AverageValue;
            return relativeDeviation;
        }

        private double EstimateCorrectionAmplitude(double[] sinValues, double[] cosValues)
        {
            return NLconstants.empiricalNLfactor * EstimateCircleDeformation(sinValues, cosValues);
        }

        private double Radius(double x, double y) => Math.Sqrt(x * x + y * y);

        private double PhiDeg(double x, double y) => Phi(x, y) * 180 / Math.PI;

        private bool IsNearToAxis(double phi)
        {
            if (IsNear(phi, 0)) return true;
            if (IsNear(phi, 90)) return true;
            if (IsNear(phi, 180)) return true;
            if (IsNear(phi, -90)) return true;
            if (IsNear(phi, -180)) return true;
            return false;
        }

        private bool IsNearToMedian(double phi)
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

        private double SinCor(double sin, double cos)
        {
            return sin + DeltaRadius(sin, cos) * Math.Sin(Phi(sin, cos));
        }

        private double CosCor(double sin, double cos)
        {
            return cos + DeltaRadius(sin,cos) * Math.Cos(Phi(sin, cos));
        }

        private double DeltaRadius(double sin, double cos) => absoluteDeviation * Math.Sin(4 * Phi(sin, cos)+Math.PI/4);

        private double absoluteDeviation;

    }
}
