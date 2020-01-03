using System;
using System.Collections.Generic;
using System.Text;

namespace Bev.IO.NmmReader.scan_mode
{
    /// <summary>
    /// A container class for metadata of a NMM scan file collection.
    /// </summary>
    public class ScanMetaData
    {

        #region Properties
        // geometry parameters
        public long DataMask { get; private set; }
        public int NumberOfProfiles { get; private set; }
        public int NumberOfSpuriousProfiles { get; private set; }
        public int NumberOfDataPoints { get; private set; }
        public int NumberOfGlitchedDataPoints { get; private set; }
        public int NumberOfScans { get; private set; }
        public int NumberOfColumnsInFile { get; private set; }
        public int[] ForwardProfileLengths { get; private set; }
        public int[] BackwardProfileLengths { get; private set; }
        public int SpuriousDataLines { get; private set; }
        // metric parameters
        public double ScanFieldDeltaX { get; private set; }
        public double ScanFieldDeltaY { get; private set; }
        public double ScanFieldDimensionX { get; private set; }
        public double ScanFieldDimensionY { get; private set; }
        public double ScanSpeed { get; private set; }
        public double ScanFieldRotation { get; private set; }
        // additional metadata
        public string BaseFileName { get; private set; }
        public int ScanIndex { get; private set; }
        public DateTime CreationDate { get; private set; }
        public TimeSpan ScanDuration { get; private set; }
        public MeasurementProcedure Procedure { get; private set; }
        public ScanDirectionStatus ScanStatus { get; private set; }
        public ScanColumnPredicate[] ColumnPredicates { get; private set; }
        public string SpmTechnique { get; private set; }
        public List<string> ScanComments { get; private set; }
        // environmental data
        public double AirTemperature { get; private set; }
        public double SampleTemperature { get; private set; }
        public double RelativeHumidity { get; private set; }
        public double BarometricPressure { get; private set; }
        public double AirTemperatureDrift { get; private set; }
        public double SampleTemperatureDrift { get; private set; }
        public double RelativeHumidityDrift { get; private set; }
        public double BarometricPressureDrift { get; private set; }
        public double AirTemperatureGradient { get; private set; }
        public int NumberOfAirSamples { get; private set; }
        public string AirSampleSourceText { get; private set; }
        // Instrument characteristics
        public string InstrumentIdentifier { get; private set; }
        public string User { get; private set; }
        public string OrganisationLong { get; private set; }
        public string Organisation { get; private set; }
        public string InstrumentManufacturer { get; private set; }
        public string InstrumentModel { get; private set; }
        public string InstrumentSerial { get; private set; }
        public string InstrumentVersion { get; private set; }
        public string EnvironmentMode { get; private set; }
        public string Institute { get; private set; }
        #endregion

        #region Methods

        /// <summary>
        /// Add relevant properties from the files read by the respective classes.
        /// </summary>
        /// <param name="obj">The object of respective file reader.</param>
        public void AddDataFrom(object obj)
        {
            if (obj is NmmEnvironmentData)
                FillEnvironmentalData(obj as NmmEnvironmentData);
            if (obj is NmmIndFileParser)
                FillIndexData(obj as NmmIndFileParser);
            if (obj is NmmDescriptionFileParser)
                FillDescriptionData(obj as NmmDescriptionFileParser);
            if (obj is NmmInstrumentCharacteristcs)
                FillInstrumentData(obj as NmmInstrumentCharacteristcs);
            if (obj is NmmFileName)
                FillFileData(obj as NmmFileName);
        }

        public string ToDebugString()
        {
            StringBuilder sb = new StringBuilder();
            _ = sb.AppendLine(string.Format("InstrumentIdentifier:       {0}", InstrumentIdentifier));
            _ = sb.AppendLine(string.Format("InstrumentManufacturer:     {0}", InstrumentManufacturer));
            _ = sb.AppendLine(string.Format("InstrumentModel:            {0}", InstrumentModel));
            _ = sb.AppendLine(string.Format("InstrumentSerial:           {0}", InstrumentSerial));
            _ = sb.AppendLine(string.Format("InstrumentVersion:          {0}", InstrumentVersion));
            _ = sb.AppendLine(string.Format("Organisation:               {0}", Organisation));
            _ = sb.AppendLine(string.Format("OrganisationLong:           {0}", OrganisationLong));
            _ = sb.AppendLine(string.Format("Institute:                  {0}", Institute));
            _ = sb.AppendLine(string.Format("User:                       {0}", User));
            _ = sb.AppendLine(string.Format("NumberOfProfiles:           {0}", NumberOfProfiles));
            _ = sb.AppendLine(string.Format("NumberOfSpuriousProfiles:   {0}", NumberOfSpuriousProfiles));
            _ = sb.AppendLine(string.Format("NumberOfDataPoints:         {0}", NumberOfDataPoints));
            _ = sb.AppendLine(string.Format("NumberOfGlitchedDataPoints: {0}", NumberOfGlitchedDataPoints));
            _ = sb.AppendLine(string.Format("NumberOfScans:              {0}", NumberOfScans));
            _ = sb.AppendLine(string.Format("SpuriousDataLines:          {0}", SpuriousDataLines));
            _ = sb.AppendLine(string.Format("ScanFieldDeltaX:            {0}", ScanFieldDeltaX));
            _ = sb.AppendLine(string.Format("ScanFieldDeltaY:            {0}", ScanFieldDeltaY));
            _ = sb.AppendLine(string.Format("ScanFieldDimensionX:        {0}", ScanFieldDimensionX));
            _ = sb.AppendLine(string.Format("ScanFieldDimensionY:        {0}", ScanFieldDimensionY));
            _ = sb.AppendLine(string.Format("ScanSpeed:                  {0}", ScanSpeed));
            _ = sb.AppendLine(string.Format("ScanFieldRotation:          {0}", ScanFieldRotation));
            _ = sb.AppendLine(string.Format("SpmTechnique:               {0}", SpmTechnique));
            _ = sb.AppendLine(string.Format("AirTemperature:             {0}", AirTemperature));
            _ = sb.AppendLine(string.Format("SampleTemperature:          {0}", SampleTemperature));
            _ = sb.AppendLine(string.Format("RelativeHumidity:           {0}", RelativeHumidity));
            _ = sb.AppendLine(string.Format("BarometricPressure:         {0}", BarometricPressure));
            _ = sb.AppendLine(string.Format("Environment:                {0}", EnvironmentMode));
            _ = sb.AppendLine(string.Format("AirSampleSourceText:        {0}", AirSampleSourceText));
            _ = sb.AppendLine(string.Format("MeasurementProcedure:       {0}", Procedure));
            _ = sb.AppendLine(string.Format("ScanDirectionStatus:        {0}", ScanStatus));
            _ = sb.AppendLine(string.Format("Filename:                   {0}", BaseFileName));
            _ = sb.AppendLine(string.Format("ScanIndex:                  {0}", ScanIndex));
            _ = sb.AppendLine("----------------");
            foreach (var s in ScanComments)
            {
                _ = sb.AppendLine(string.Format("> {0}", s));
            }
            return sb.ToString();
        }

        #endregion

        #region Private stuff

        private void FillFileData(NmmFileName obj)
        {
            BaseFileName = obj.BaseFileName;
            ScanIndex = obj.ScanIndex;
        }

        private void FillInstrumentData(NmmInstrumentCharacteristcs obj)
        {
            InstrumentIdentifier = obj.InstrumentIdentifier;
            User = obj.User;
            OrganisationLong = obj.OrganisationLong;
            Organisation = obj.Organisation;
            InstrumentManufacturer = obj.InstrumentManufacturer;
            InstrumentModel = obj.InstrumentModel;
            InstrumentSerial = obj.InstrumentSerial;
            InstrumentVersion = obj.InstrumentVersion;
            EnvironmentMode = obj.EnvironmentMode;
            Institute = obj.Institute;
        }

        private void FillIndexData(NmmIndFileParser obj)
        {
            if (DataMask == 0)
            {
                DataMask = obj.DataMask;
                ColumnPredicates = Scan.ColumnPredicatesFor(DataMask);
                NumberOfColumnsInFile = ColumnPredicates.Length;
            }
            // prefer NumberOfProfiles infered from the *.ind file
            NumberOfProfiles = obj.NumberOfProfiles;
            // end NumberOfProfiles
            NumberOfSpuriousProfiles = obj.SpuriousProfiles;
            NumberOfDataPoints = obj.NominalDataPoints;
            NumberOfGlitchedDataPoints = obj.DataPointsGlitch;
            SpuriousDataLines = obj.SpuriousDataLines;
            ScanStatus = obj.ScanStatus;
            CreationDate = obj.CreationDate;
            ScanDuration = obj.ScanDuration;
            ForwardProfileLengths = obj.ForwardProfileLengths.ToArray();
            BackwardProfileLengths = obj.BackwardProfileLengths.ToArray();

        }

        private void FillDescriptionData(NmmDescriptionFileParser obj)
        {
            // prefer DataMask from the *.dsc file
            DataMask = obj.DataMask;
            ColumnPredicates = Scan.ColumnPredicatesFor(DataMask);
            NumberOfColumnsInFile = ColumnPredicates.Length;
            // end DataMask
            if (NumberOfProfiles == 0)
            {
                NumberOfProfiles = obj.NumberOfProfiles;
            }
            NumberOfScans = obj.NumberOfScans;
            Procedure = obj.Procedure;
            ScanFieldDeltaX = obj.ScanFieldDeltaX;
            ScanFieldDeltaY = obj.ScanFieldDeltaY;
            ScanFieldDimensionX = obj.ScanFieldDimensionX;
            ScanFieldDimensionY = obj.ScanFieldDimensionY;
            ScanSpeed = obj.ScanSpeed;
            ScanFieldRotation = obj.ScanFieldRotation;
            SpmTechnique = obj.SpmTechnique;
            ScanComments = obj.ScanComments;
        }

        private void FillEnvironmentalData(NmmEnvironmentData obj)
        {
            AirTemperature = obj.AirTemperature;
            SampleTemperature = obj.SampleTemperature;
            RelativeHumidity = obj.RelativeHumidity;
            BarometricPressure = obj.BarometricPressure;
            AirTemperatureDrift = obj.AirTemperatureDrift;
            SampleTemperatureDrift = obj.SampleTemperatureDrift;
            RelativeHumidityDrift = obj.RelativeHumidityDrift;
            BarometricPressureDrift = obj.BarometricPressureDrift;
            AirTemperatureGradient = obj.AirTemparatureGradient;
            NumberOfAirSamples = obj.NumberOfAirSamples;
            AirSampleSourceText = obj.AirSampleSourceText;
        }

        #endregion

    }
}
