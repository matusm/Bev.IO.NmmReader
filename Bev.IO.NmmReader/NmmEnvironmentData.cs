using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace Bev.IO.NmmReader
{
    /// <summary>
    /// Encapsulates the download of environmental sensor data recorded by a measurement on the SIOS NMM.
    /// Evaluated parameters are provided as properties.
    /// </summary>
    /// <remarks>
    /// All valid *.pos files are automaticaly consumed by this class.
    /// Scan files (forward and backward), 3d-files, with or without sample temperature sensor channel. 
    /// </remarks>
    public class NmmEnvironmentData
    {
        private static readonly double referenceTemperature = 20;
        private static readonly double referencePressure = 101300;
        private static readonly double referenceHumidity = 50;
        // Files produced by the NMM are usually generated with the "." as decimal separator
        private static readonly NumberFormatInfo numFormat = new NumberFormatInfo() { NumberDecimalSeparator = "." };

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Bev.IO.NmmReader.NmmEnvironmentData"/> class.
        /// </summary>
        /// <param name="fileName">Base name for the data files.</param>
        public NmmEnvironmentData(NmmFileName fileName)
        {
            // start with unknown status of sensor data
            airTemperatureOrigin = AirTemperatureOrigin.Unknown;
            sampleTemperatureOrigin = SampleTemperatureOrigin.Unknown;
            // try to load files
            LoadSensorData(fileName.GetPosFileNameForScanIndex(ScanDirection.Forward));
            LoadSensorData(fileName.GetPosFileNameForScanIndex(ScanDirection.Backward));
            // analyze data origin in more detail
            AnalyzeSensorOrigin();
        }

        #endregion

        #region Properties

        public string AirSampleSourceText { get { return StatusDescription(); } }

        public EnvironmentDataStatus AirSampleSource { get; private set; }

        /// <summary>
        /// Gets the number of data points.
        /// </summary>
        public int NumberOfAirSamples { get { return xTemperatureValues.Count(); } }

        /// <summary>
        /// Gets the air temperature.
        /// </summary>
        public double AirTemperature { get { return (XTemperature + YTemperature + ZTemperature) / 3.0; } }

        /// <summary>
        /// Gets the span of air temperatures.
        /// </summary>
        public double AirTemperatureDrift { get { return EstimateAirTemperatureDrift(); } }

        /// <summary>
        /// Gets the gradient estimated by the three air thermometers.
        /// </summary>
        public double AirTemparatureGradient { get { return EstimateAirTemperatureHomogeneity(); } }

        /// <summary>
        /// Gets the sample temperature.
        /// </summary>
        public double SampleTemperature
        {
            get
            {
                if (sampleTemperatureOrigin == SampleTemperatureOrigin.EstimatedFromAir)
                    return AirTemperature;
                return EvaluateMean(sTemperatureValues, referenceTemperature);
            }
        }

        /// <summary>
        /// Gets the span of sample temperatures.
        /// </summary>
        public double SampleTemperatureDrift
        {
            get
            {
                if (sampleTemperatureOrigin == SampleTemperatureOrigin.EstimatedFromAir)
                    return AirTemperatureDrift;
                return EvaluateSpan(sTemperatureValues);
            }
        }

        /// <summary>
        /// Gets the relative humidity.
        /// </summary>
        public double RelativeHumidity { get { return EvaluateMean(humiditieValues, referenceHumidity); } }

        /// <summary>
        /// Gets the span of relative humidity.
        /// </summary>
        public double RelativeHumidityDrift { get { return EvaluateSpan(humiditieValues); } }

        /// <summary>
        /// Gets the barometric pressure.
        /// </summary>
        public double BarometricPressure { get { return EvaluateMean(pressureValues, referencePressure); } }

        /// <summary>
        /// Gets the span of pressures.
        /// </summary>
        public double BarometricPressureDrift { get { return EvaluateSpan(pressureValues); } }

        /// <summary>
        /// Gets the air temperature for the X-axis interferometer.
        /// </summary>
        public double XTemperature { get { return EvaluateMean(xTemperatureValues, referenceTemperature); } }

        /// <summary>
        /// Gets the air temperature for the Y-axis interferometer.
        /// </summary>
        public double YTemperature { get { return EvaluateMean(yTemperatureValues, referenceTemperature); } }

        /// <summary>
        /// Gets the air temperature for the Z-axis interferometer.
        /// </summary>
        public double ZTemperature { get { return EvaluateMean(zTemperatureValues, referenceTemperature); } }
        #endregion

        #region Private stuff

        /// <summary>
        /// Transforms the <c>DataStatus</c> enumeration to a user friedly text.
        /// </summary>
        /// <returns>A verbatim description.</returns>
        private string StatusDescription()
        {
            switch (AirSampleSource)
            {
                case EnvironmentDataStatus.Unknown:
                    return "This should not happen!";
                case EnvironmentDataStatus.NoDataProvided:
                    return "File(s) not found or invalid, using default values.";
                case EnvironmentDataStatus.DefaultValues:
                    return "File(s) contained default values only (Instrument malfunction).";
                case EnvironmentDataStatus.MeasuredValues:
                    return "All parameters (including sample temperature) recorded.";
                case EnvironmentDataStatus.SampleEstimatedbyAir:
                    return "Air parameters recorded, sample temperature estimated by air temperature.";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Determines the source of the data for the standard and the sample temperature separately.
        /// </summary>
        private void AnalyzeSensorOrigin()
        {
            if (NumberOfAirSamples == 0)
            {
                airTemperatureOrigin = AirTemperatureOrigin.DfaultValues;
                sampleTemperatureOrigin = SampleTemperatureOrigin.DefaultValues;
            }
            else
            {
                if (AirTemperature == 20.0 && AirTemperatureDrift == 0.0)
                    airTemperatureOrigin = AirTemperatureOrigin.DfaultValues;
                if (SampleTemperature == 20.0 && SampleTemperatureDrift == 0.0)
                    sampleTemperatureOrigin = SampleTemperatureOrigin.DefaultValues;
            }
            SubsumizeStatus();
        }

        /// <summary>
        /// Determines a synopsis of the source status.
        /// </summary>
        private void SubsumizeStatus()
        {
            AirSampleSource = EnvironmentDataStatus.Unknown;

            if (NumberOfAirSamples == 0)
            {
                AirSampleSource = EnvironmentDataStatus.NoDataProvided;
                return;
            }

            if (airTemperatureOrigin == AirTemperatureOrigin.MeasuredBySensor)
            {
                switch (sampleTemperatureOrigin)
                {
                    case SampleTemperatureOrigin.MeasuredBySensor:
                        AirSampleSource = EnvironmentDataStatus.MeasuredValues;
                        return;
                    case SampleTemperatureOrigin.EstimatedFromAir:
                        AirSampleSource = EnvironmentDataStatus.SampleEstimatedbyAir;
                        return;
                    default:
                        return;
                }
            }

            if (airTemperatureOrigin == AirTemperatureOrigin.DfaultValues && sampleTemperatureOrigin == SampleTemperatureOrigin.DefaultValues)
            {
                AirSampleSource = EnvironmentDataStatus.DefaultValues;
                return;
            }
        }

        /// <summary>
        /// Loads the sensor data from the file.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        private void LoadSensorData(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }
            string line;
            StreamReader hFile = File.OpenText(fileName);
            while ((line = hFile.ReadLine()) != null)
                ParseDataLine(line);
            if (hFile != null) hFile.Close();
        }

        /// <summary>
        /// Parses a line of text for sensor data and update lists on success.
        /// </summary>
        /// <param name="line">The line of text to be parsed.</param>
        private void ParseDataLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return;
            char[] charSeparators = { ' ' };
            string[] tokens = line.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                switch (tokens.Length)
                {
                    case 5: // 3d file without sample sensor
                        xTemperatureValues.Add(double.Parse(tokens[0], numFormat));
                        yTemperatureValues.Add(double.Parse(tokens[1], numFormat));
                        zTemperatureValues.Add(double.Parse(tokens[2], numFormat));
                        pressureValues.Add(double.Parse(tokens[3], numFormat));
                        humiditieValues.Add(double.Parse(tokens[4], numFormat));
                        airTemperatureOrigin = AirTemperatureOrigin.MeasuredBySensor;
                        sampleTemperatureOrigin = SampleTemperatureOrigin.EstimatedFromAir;
                        break;
                    case 6: // 3d file with sample sensor
                        xTemperatureValues.Add(double.Parse(tokens[0], numFormat));
                        yTemperatureValues.Add(double.Parse(tokens[1], numFormat));
                        zTemperatureValues.Add(double.Parse(tokens[2], numFormat));
                        pressureValues.Add(double.Parse(tokens[3], numFormat));
                        humiditieValues.Add(double.Parse(tokens[4], numFormat));
                        sTemperatureValues.Add(double.Parse(tokens[5], numFormat));
                        airTemperatureOrigin = AirTemperatureOrigin.MeasuredBySensor;
                        sampleTemperatureOrigin = SampleTemperatureOrigin.MeasuredBySensor;
                        break;
                    case 8: // scan file without sample sensor
                        xTemperatureValues.Add(double.Parse(tokens[3], numFormat));
                        yTemperatureValues.Add(double.Parse(tokens[4], numFormat));
                        zTemperatureValues.Add(double.Parse(tokens[5], numFormat));
                        pressureValues.Add(double.Parse(tokens[6], numFormat));
                        humiditieValues.Add(double.Parse(tokens[7], numFormat));
                        airTemperatureOrigin = AirTemperatureOrigin.MeasuredBySensor;
                        sampleTemperatureOrigin = SampleTemperatureOrigin.EstimatedFromAir;
                        break;
                    case 9: // scan file with sample sensor
                        xTemperatureValues.Add(double.Parse(tokens[3], numFormat));
                        yTemperatureValues.Add(double.Parse(tokens[4], numFormat));
                        zTemperatureValues.Add(double.Parse(tokens[5], numFormat));
                        pressureValues.Add(double.Parse(tokens[6], numFormat));
                        humiditieValues.Add(double.Parse(tokens[7], numFormat));
                        sTemperatureValues.Add(double.Parse(tokens[8], numFormat));
                        airTemperatureOrigin = AirTemperatureOrigin.MeasuredBySensor;
                        sampleTemperatureOrigin = SampleTemperatureOrigin.MeasuredBySensor;
                        break;
                    default:
                        return;
                }
            }
            catch (Exception)
            {
                // most probably a System.FormatException occured
                // just ignore
            }
        }

        /// <summary>
        /// Evaluates the mean value of the given data list.
        /// </summary>
        /// <returns>The mean or the default value</returns>
        /// <param name="values">The data list.</param>
        /// <param name="defaultValue">The default value if the data list is empty.</param>
        private double EvaluateMean(List<double> values, double defaultValue)
        {
            if (values.Count == 0) return defaultValue;
            return values.Average();
        }

        /// <summary>
        /// Evaluates the span of all values in the given data list.
        /// </summary>
        /// <returns>The span.</returns>
        /// <param name="values">The data list.</param>
        private double EvaluateSpan(List<double> values)
        {
            if (values.Count <= 1) return 0.0;
            return values.Max() - values.Min();
        }

        /// <summary>
        /// Estimates the span of three air temperature values. Measure for the air temperature homogeniety.
        /// </summary>
        /// <returns>The homogeneity.</returns>
        private double EstimateAirTemperatureHomogeneity()
        {
            double t1 = XTemperature;
            double t2 = YTemperature;
            double t3 = ZTemperature;
            double tMax = t1;
            if (t2 > tMax) tMax = t2;
            if (t3 > tMax) tMax = t3;
            double tMin = t1;
            if (t2 < tMin) tMin = t2;
            if (t3 < tMin) tMin = t3;
            return tMax - tMin;
        }

        /// <summary>
        /// Estimates the mean air temperature drift as the mean of the three individual drifts.
        /// </summary>
        /// <returns>The air temperature drift.</returns>
        private double EstimateAirTemperatureDrift()
        {
            return (EvaluateSpan(xTemperatureValues) + EvaluateSpan(yTemperatureValues) + EvaluateSpan(zTemperatureValues)) / 3.0;
        }

        // fields for housekeeping purposes
        private AirTemperatureOrigin airTemperatureOrigin;
        private SampleTemperatureOrigin sampleTemperatureOrigin;
        // actual data as provided by file(s)
        private readonly List<double> xTemperatureValues = new List<double>();
        private readonly List<double> yTemperatureValues = new List<double>();
        private readonly List<double> zTemperatureValues = new List<double>();
        private readonly List<double> sTemperatureValues = new List<double>();
        private readonly List<double> humiditieValues = new List<double>();
        private readonly List<double> pressureValues = new List<double>();
        
        #endregion

    }

    #region Enums

    /// <summary>
    /// Local data source types for air parameters.
    /// </summary>
    internal enum AirTemperatureOrigin
    {
        Unknown,
        MeasuredBySensor,
        DfaultValues
    }

    /// <summary>
    /// Local data source types for sample temperature parameter.
    /// </summary>
    internal enum SampleTemperatureOrigin
    {
        Unknown,
        MeasuredBySensor,
        EstimatedFromAir,
        DefaultValues
    }

    #endregion
}
