using System.Linq;

namespace Bev.IO.NmmReader
{
    public class DataLeveling
    {
        #region Ctor
        public DataLeveling(double[] rawData, int numPoints, int numProfiles)
        {
            this.rawData = rawData;
            this.numPoints = numPoints;
            this.numProfiles = numProfiles;
            Mode = ReferenceTo.None;
        }

        public DataLeveling(double[] rawData, int numPoints) : this(rawData, numPoints, 1) { }
        #endregion

        #region Properties
        // misc
        public ReferenceTo Mode { get; private set; }
        public string LevelModeDescription => ModeToString(Mode);
        // z
        public double BiasValue { get; private set; }
        public double MaximumValue => rawData.Max();
        public double MinimumValue => rawData.Min();
        public double AverageValue => rawData.Average();
        public double CentralValue => (MaximumValue + MinimumValue) / 2.0;
        // x
        public double FirstValue => rawData[0];
        public double LastValue => rawData.Last();
        public double CenterValue => rawData[rawData.Length / 2];
        #endregion

        #region Methods

        public double[] LevelData(ReferenceTo mode)
        {
            switch(GetDataType())
            {
                case DataType.Unknown:
                    Mode = ReferenceTo.None;
                    return rawData;
                case DataType.Profile:
                    Mode = mode;
                    return LevelProfileData(mode);
                case DataType.Raster:
                    Mode = mode;
                    return LevelRasterData(mode);
                default:
                    Mode = ReferenceTo.None;
                    return rawData;
            }
        }

        #endregion

        #region Private stuff

        private double[] LevelProfileData( ReferenceTo mode )
        {
            // prepare return field
            double[] leveledData = new double[rawData.Length];
            // calculate mode dependend parameters
            switch(mode)
            {
                case ReferenceTo.None:
                    break;
                case ReferenceTo.First:
                    intercept = FirstValue;
                    break;
                case ReferenceTo.Last:
                    intercept = LastValue;
                    break;
                case ReferenceTo.Center:
                    intercept = CenterValue;
                    break;
                case ReferenceTo.Minimum:
                    intercept = MinimumValue;
                    break;
                case ReferenceTo.Maximum:
                    intercept = MaximumValue;
                    break;
                case ReferenceTo.Average:
                    intercept = AverageValue;
                    break;
                case ReferenceTo.Central:
                    intercept = CentralValue;
                    break;
                case ReferenceTo.Bias:
                    intercept = BiasValue;
                    break;
                case ReferenceTo.Line:
                    intercept = FirstValue;
                    slopeX = (LastValue - FirstValue) / (rawData.Length - 1);
                    break;
                case ReferenceTo.Lsq:
                    FitLsqLine();
                    break;
            }
            // now level the data
            for (int i = 0; i < rawData.Length; i++)
            {
                leveledData[i] = sign * (rawData[i] - (intercept + (slopeX * (double)i)));
            }
            return leveledData;
        }

        private double[] LevelRasterData(ReferenceTo mode)
        {
            // prepare return field
            double[] leveledData = new double[rawData.Length];
            // calculate mode dependend parameters
            switch (mode)
            {
                case ReferenceTo.None:
                    break;
                case ReferenceTo.First:
                    intercept = FirstValue;
                    break;
                case ReferenceTo.Last:
                    intercept = LastValue;
                    break;
                case ReferenceTo.Center:
                    intercept = CenterValue;
                    break;
                case ReferenceTo.Minimum:
                    intercept = MinimumValue;
                    break;
                case ReferenceTo.Maximum:
                    intercept = MaximumValue;
                    break;
                case ReferenceTo.Average:
                    intercept = AverageValue;
                    break;
                case ReferenceTo.Central:
                    intercept = CentralValue;
                    break;
                case ReferenceTo.Bias:
                    intercept = BiasValue;
                    break;
                case ReferenceTo.Line:
                    intercept = FirstValue;
                    slopeX = (rawData[numPoints - 1] - FirstValue) / (numPoints - 1);
                    slopeY = (rawData[rawData.Length - numPoints] - FirstValue) / (numProfiles - 1);
                    break;
                case ReferenceTo.Lsq:
                    FitLsqPlane();
                    break;
            }
            // now level the data
            for (int i = 0; i < numPoints; i++)
                for (int j = 0; j < numProfiles; j++)
                    leveledData[i + j * numPoints] = sign * (rawData[i + (j * numPoints)] - (intercept + (slopeX * (double)i) + (slopeY * (double)j)));
            return leveledData;
        }

        // Fits a least square line to raster data
        // works with equidistant spacing only
        private void FitLsqLine()
        {
            int n = rawData.Length;
            double sigmaXY = 0.0;
            for (int i = 0; i < n; i++)
                sigmaXY += i * rawData[i]; // Sum x*y
            slopeX = (12.0 * sigmaXY - 6.0 * (n - 1.0) * rawData.Sum()) / (n * (n * n + 1.0));
            intercept = AverageValue - 0.5 * slopeX * (n * (n - 1.0));
            //TODO !!
            //slopeX = (rawData.Sum() * (n * n - n) - 2.0 * n * sigmaXY) / ((-n * n / 6) * (n*n+4*n+1));
            //intercept = AverageValue - 0.5 * slopeX * (n - 1.0);
        }

        // Fits a least square plane to raster data
        // works only with rectangular ordered data with equidistant spacing
        // spacing in x and y can be different, however
        // implementation according to EUNA 15178 ENC eq. (9.7)
        // the quantity symbols are 
        private void FitLsqPlane()
        {
            double u = 0.0, v = 0.0, w = 0.0;
            int M = numPoints;
            int N = numProfiles;
            // the casting from int to double is essential - M⁴ 
            double fM = (double)M;
            double fN = (double)N;
            // u
            for (int l = 0; l < N; l++)
                for (int k = 0; k < M; k++)
                    u += k * rawData[k + l * M];
            // v
            for (int l = 0; l < N; l++)
                for (int k = 0; k < M; k++)
                    v += l * rawData[k + l * M];
            // w
            w = rawData.Sum();
            // (9-7)
            double a = ((7 * fM * fN + fM + fN - 5) * w - 6 * u * (fN + 1) - 6 * v * (fM + 1)) / (fM * fN * (fM + 1) * (fN + 1));
            double b = (12 * u - 6 * w * (fM - 1)) / (fM * fN * (fM - 1) * (fM + 1));
            double c = (12 * v - 6 * w * (fN - 1)) / (fM * fN * (fN - 1) * (fN + 1));
            // dx und dy = 1 !
            // translate parameter names
            intercept = a;
            slopeX = b;
            slopeY = c;
        }

        private string ModeToString(ReferenceTo mode)
        {
            switch (GetDataType())
            {
                case DataType.Unknown:
                    return "Data not leveled";
                case DataType.Profile:
                    return ModeProfileToString(mode);
                case DataType.Raster:
                    return ModeRasterToString(mode);
                default:
                    return "Data not leveled"; ;
            }

        }

        private string ModeProfileToString(ReferenceTo mode)
        {
            switch (mode)
            {
                case ReferenceTo.None:
                    return "Profile not referenced";
                case ReferenceTo.Average:
                    return $"Profile referenced to avarage height value ({intercept})";
                case ReferenceTo.Bias:
                    return $"Profile referenced to user supplied value ({intercept})";
                case ReferenceTo.Center:
                    return $"Profile referenced to central value of trace ({intercept})";
                case ReferenceTo.Central:
                    return $"Profile referenced to mid height value ({intercept})";
                case ReferenceTo.First:
                    return $"Profile referenced to first value of trace ({intercept})";
                case ReferenceTo.Last:
                    return $"Profile referenced to last value of trace ({intercept})";
                case ReferenceTo.Maximum:
                    return $"Profile referenced to maximum height value ({intercept})";
                case ReferenceTo.Minimum:
                    return $"Profile referenced to minimum height value ({intercept})";
                case ReferenceTo.Line:
                    return "Profile leveled to line connecting boundary points";
                case ReferenceTo.Lsq:
                    return "Profile leveled to least square line";
                default:
                    return "this should not happen!";
            }
        }

        private string ModeRasterToString(ReferenceTo mode)
        {
            switch (mode)
            {
                case ReferenceTo.None:
                    return "Surface not referenced";
                case ReferenceTo.Average:
                    return $"Surface referenced to avarage height value ({intercept})";
                case ReferenceTo.Bias:
                    return $"Surface referenced to user supplied value ({intercept})";
                case ReferenceTo.Center:
                    return $"Surface referenced to central value of array ({intercept})";
                case ReferenceTo.Central:
                    return $"Surface referenced to mid height value ({intercept})";
                case ReferenceTo.First:
                    return $"Surface referenced to first value of array ({intercept})";
                case ReferenceTo.Last:
                    return $"Surface referenced to last value of array ({intercept})";
                case ReferenceTo.Maximum:
                    return $"Surface referenced to maximum height value ({intercept})";
                case ReferenceTo.Minimum:
                    return $"Surface referenced to minimum height value ({intercept})";
                case ReferenceTo.Line:
                    return "Three point surface leveling";
                case ReferenceTo.Lsq:
                    return "Surface leveled to least square plane";
                default:
                    return "this should not happen!";
            }
        }

        private DataType GetDataType()
        {
            if (rawData.Length != numPoints * numProfiles) return DataType.Unknown;
            if (numPoints <= 0) return DataType.Unknown;
            if (numProfiles <= 0) return DataType.Unknown;
            if (numProfiles == 1) return DataType.Profile;
            return DataType.Raster;
        }

        private readonly double[] rawData;
        private readonly int numPoints;
        private readonly int numProfiles;
        // fields for leveling methods
        private double intercept; // constant part to be subtracted
        private double slopeX; // X-length dependend part to be subtracted
        private double slopeY; // Y-length dependend part to be subtracted
        private double sign = 1.0; // sign factor (+1/-1);

        #endregion

    }

    // some handy enums

    public enum ReferenceTo
    {
        None = 0,       // do not change the height data
        Minimum = 1,    // reference height data to minimal z-value
        Maximum = 2,    // reference height data to maximal z-value
        Average = 3,    // reference height data to arithmetic mean z-value
        Central = 4,    // reference height data to mid of span z-value
        Bias = 5,       // reference height data to user defined bias z-value
        First = 6,      // reference height data to first value
        Last = 7,       // reference height data to last value
        Center = 8,     // reference height data to center value
        Line = 9,       // subtract linear line (first to last point) / three point plane
        Lsq = 10        // subtract least square linear line / plane
    }

    public enum DataType
    {
        Unknown,    // neither a single profile nor raster data
        Profile,    // data represents a single profile
        Raster      // data represents a recangular raster
    }

}
