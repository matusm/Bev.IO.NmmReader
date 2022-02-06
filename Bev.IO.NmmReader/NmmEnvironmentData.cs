﻿using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace Bev.IO.NmmReader
{
    public class NmmEnvironmentData
    {
        private static readonly double referenceTemperature = 20;
        private static readonly double referencePressure = 101300;
        private static readonly double referenceHumidity = 50;
        private static readonly NumberFormatInfo numFormat = new NumberFormatInfo() { NumberDecimalSeparator = "." };

        public NmmEnvironmentData(NmmFileName fileName)
        {
            airTemperatureOrigin = AirTemperatureOrigin.Unknown;
            sampleTemperatureOrigin = SampleTemperatureOrigin.Unknown;
            LoadSensorData(fileName.GetPosFileNameForScanIndex(ScanDirection.Forward));
            LoadSensorData(fileName.GetPosFileNameForScanIndex(ScanDirection.Backward));
            AnalyzeSensorOrigin();
        }

        public string AirSampleSourceText => StatusDescription();
        public EnvironmentDataStatus AirSampleSource { get; private set; }
        public int NumberOfAirSamples => xTemperatureValues.Count;
        public double AirTemperature => (XTemperature + YTemperature + ZTemperature) / 3.0;
        public double AirTemperatureDrift => EstimateAirTemperatureDrift();
        public double AirTemparatureGradient => EstimateAirTemperatureHomogeneity();
        public double SampleTemperature => GetSampleTemperature();
        public double SampleTemperatureDrift => GetSampleTemperatureDrift();
        public double RelativeHumidity => EvaluateMean(humiditieValues, referenceHumidity);
        public double RelativeHumidityDrift => EvaluateSpan(humiditieValues);
        public double BarometricPressure => EvaluateMean(pressureValues, referencePressure);
        public double BarometricPressureDrift => EvaluateSpan(pressureValues);
        public double XTemperature => EvaluateMean(xTemperatureValues, referenceTemperature);
        public double YTemperature => EvaluateMean(yTemperatureValues, referenceTemperature);
        public double ZTemperature => EvaluateMean(zTemperatureValues, referenceTemperature);

        private double GetSampleTemperature()
        {
            if (sampleTemperatureOrigin == SampleTemperatureOrigin.EstimatedFromAir)
                return AirTemperature;
            return EvaluateMean(sTemperatureValues, referenceTemperature);
        }

        private double GetSampleTemperatureDrift()
        {
            if (sampleTemperatureOrigin == SampleTemperatureOrigin.EstimatedFromAir)
                return AirTemperatureDrift;
            return EvaluateSpan(sTemperatureValues);
        }

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

        private void LoadSensorData(string fileName)
        {
            try
            {
                string line;
                StreamReader hFile = File.OpenText(fileName);
                while ((line = hFile.ReadLine()) != null)
                    ParseDataLine(line);
                hFile.Close();
            }
            catch (Exception)
            {
                // file error, ignore
            }
        }

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

        private double EvaluateMean(List<double> values, double defaultValue)
        {
            if (values.Count == 0)
                return defaultValue;
            return values.Average();
        }

        private double EvaluateSpan(List<double> values)
        {
            if (values.Count <= 1)
                return 0.0;
            return values.Max() - values.Min();
        }

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

    }

    internal enum AirTemperatureOrigin
    {
        Unknown,
        MeasuredBySensor,
        DfaultValues
    }

    internal enum SampleTemperatureOrigin
    {
        Unknown,
        MeasuredBySensor,
        EstimatedFromAir,
        DefaultValues
    }

}
