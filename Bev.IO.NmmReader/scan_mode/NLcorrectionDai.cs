using System;

namespace Bev.IO.NmmReader.scan_mode
{
    public class NLcorrectionDai
    {

        public CorrectionStatus Status { get; private set; } = CorrectionStatus.Unknown;
        public double CorrectionAmplitude { get; private set; }
        public double CorrectionSpan => CorrectionAmplitude * 2;
        public double[] CorrectedData { get; private set; }

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

        private double GetPhi(double sin, double cos) => Math.Atan2(cos, sin);

        private double GetCorrection(double sin, double cos) => - CorrectionAmplitude * Math.Sin(4 * GetPhi(sin, cos)); // or Sin ?

    }
}
