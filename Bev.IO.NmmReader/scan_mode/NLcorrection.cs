namespace Bev.IO.NmmReader.scan_mode
{
    public class NLcorrection
    {
        private const double daiCorrection = 1e-9;

        public CorrectionStatus Status { get; private set; } = CorrectionStatus.Unknown;
        public double[] CorrectedData { get; }
        public double CorrectionSpan { get; private set; } = 0.0;

        public NLcorrection(double[] rawData, double[] sinValues, double[] cosValues, double empiricalCorrection)
        {
            NLcorrectionHeydemann heydemann = new NLcorrectionHeydemann(rawData, sinValues, cosValues);
            NLcorrectionDai gaoliang = new NLcorrectionDai(heydemann.CorrectedData, sinValues, cosValues, empiricalCorrection);
            CorrectedData = gaoliang.CorrectedData;
            CorrectionSpan = heydemann.CorrectionSpan + gaoliang.CorrectionSpan;
            Status = heydemann.Status;
        }

        public NLcorrection(double[] rawData, double[] sinValues, double[] cosValues) : this(rawData, sinValues, cosValues, daiCorrection) { }
    
    }
}
