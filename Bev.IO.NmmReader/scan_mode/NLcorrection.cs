namespace Bev.IO.NmmReader.scan_mode
{
    public class NLcorrection
    {
        private const double defaultDaiCorrection = 0.5e-9;

        public CorrectionStatus Status { get; private set; } = CorrectionStatus.Unknown;
        public double[] CorrectedData { get; }
        public double CorrectionSpan { get; private set; } = 0.0;

        // The Dai correction is performed in any case with the provided correction value
        public NLcorrection(double[] rawData, double[] sinValues, double[] cosValues, double empiricalCorrection)
        {
            NLcorrectionHeydemann heydemann = new NLcorrectionHeydemann(rawData, sinValues, cosValues);
            NLcorrectionDai gaoliang = new NLcorrectionDai(heydemann.CorrectedData, heydemann.CorrectedSinValues, heydemann.CorrectedCosValues, empiricalCorrection);
            CorrectedData = gaoliang.CorrectedData;
            CorrectionSpan = heydemann.CorrectionSpan + gaoliang.CorrectionSpan;
            Status = heydemann.Status;
        }

        // The Dai correction is performed with an estimated value or wit the default value, depending on the Heydemann success
        public NLcorrection(double[] rawData, double[] sinValues, double[] cosValues)
        {
            NLcorrectionHeydemann heydemann = new NLcorrectionHeydemann(rawData, sinValues, cosValues);
            NLcorrectionDai gaoliang;
            if (heydemann.Status==CorrectionStatus.Corrected)
            {
                gaoliang = new NLcorrectionDai(heydemann.CorrectedData, heydemann.CorrectedSinValues, heydemann.CorrectedCosValues);
            }
            else
            {
                gaoliang = new NLcorrectionDai(heydemann.CorrectedData, heydemann.CorrectedSinValues, heydemann.CorrectedCosValues, defaultDaiCorrection);
            }
            CorrectedData = gaoliang.CorrectedData;
            CorrectionSpan = heydemann.CorrectionSpan + gaoliang.CorrectionSpan;
            Status = heydemann.Status;
        }
    }
}
