using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Bev.IO.NmmReader.scan_mode
{
    /// <summary>
    /// This Class consumes the index files (*.ind) produced during a scan on the SIOS NMM.
    /// The data is provided by properties only, there are no public methods. 
    /// </summary>
    public class NmmIndFileParser
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Bev.IO.NmmReader.NmmIndFileParser"/> class.
        /// </summary>
        /// <param name="fileName">Base name for the data files.</param>
        public NmmIndFileParser(NmmFileName fileName)
        {
            // try to load data from file(s)
            LoadIndexData(fileName.GetIndFileNameForScanIndex(ScanDirection.Forward), ScanDirection.Forward);
            LoadIndexData(fileName.GetIndFileNameForScanIndex(ScanDirection.Backward), ScanDirection.Backward);
            // analyze loaded data
            // also justifies backward profile number if needed.
            AnalyzeDirectionStatus();
            // determine nominal number of data points
            DetermineNominalDataPointNumber();
            // determine profiles with to few data points
            // the largest number is deemed as the nominal length
            FindShortProfiles(NominalDataPoints);
            // Creation date and duration
            EvaluateMeasurementTime();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the scan status (backward, forward, ...)
        /// </summary>
        public ScanDirectionStatus ScanStatus { get; private set; }

        /// <summary>
        /// Gets the number of profiles of the scan as determined by the index files.
        /// </summary>
        public int NumberOfProfiles { get { return ForwardProfileLengths.Count(); } }

        /// <summary>
        /// Gets the number of spurious profiles in the backward scan (peculiar NMM file error).
        /// </summary>
        public int SpuriousProfiles { get; private set; }

        /// <summary>
        /// Gets the estimated nominal number of data points.
        /// </summary>
        public int NominalDataPoints { get; private set; }

        /// <summary>
        /// Gets the maximum data points glitch.
        /// </summary>
        public int DataPointsGlitch { get; private set; }

        /// <summary>
        /// Gets the data mask for the scan.
        /// </summary>
        public long DataMask { get; private set; }

        /// <summary>
        /// Gets the creation (or start) date for the scan.
        /// </summary>
        public DateTime CreationDate { get; private set; }

        /// <summary>
        /// Gets the duration for the scan.
        /// </summary>
        public TimeSpan ScanDuration { get; private set; }

        /// <summary>
        /// Gets the array of missing points for the forward profils.
        /// </summary>
        public int[] ProfilsDefectsForward { get; private set; }

        /// <summary>
        /// Gets the array of missing points for the backward profils.
        /// </summary>
        public int[] ProfilsDefectsBackward { get; private set; }
        
        public List<int> ForwardProfileLengths { get; private set; } = new List<int>();

        public List<int> BackwardProfileLengths { get; private set; } = new List<int>();

        // the number of sporious data lines in the backward scan file
        public int SpuriousDataLines { get; private set; }

        #endregion

        #region Private stuff

        /// <summary>
        /// Calculates nominal number of data points (and maximum glitch).
        /// </summary>
        private void DetermineNominalDataPointNumber()
        {
            NominalDataPoints = 0;
            DataPointsGlitch = 0;
            if (ScanStatus == ScanDirectionStatus.NoData) return;
            if (ScanStatus == ScanDirectionStatus.Unknown) return;
            if (NumberOfProfiles < 1) return;
            NominalDataPoints = ForwardProfileLengths.Max();
            DataPointsGlitch = NominalDataPoints - ForwardProfileLengths.Min();
            if (ScanStatus == ScanDirectionStatus.ForwardAndBackward || ScanStatus == ScanDirectionStatus.ForwardAndBackwardJustified)
            {
                NominalDataPoints = Math.Max(NominalDataPoints, BackwardProfileLengths.Max());
                DataPointsGlitch = Math.Max(DataPointsGlitch, NominalDataPoints - BackwardProfileLengths.Min());
            }
        }

        /// <summary>
        /// Extract creation date and scan duration.
        /// </summary>
        private void EvaluateMeasurementTime()
        {
            if (forwardTimeStamps.Count == 0)
            {
                ScanDuration = TimeSpan.Zero;
                return;
            }
            forwardTimeStamps.AddRange(backwardTimeStamps);
            CreationDate = forwardTimeStamps.First();
            ScanDuration = forwardTimeStamps.Last() - CreationDate;
        }

        /// <summary>
        /// List the profiles which are too short (according to a nominal profile length).
        /// </summary>
        /// <param name="nominalProfileLength">Nominal profile length.</param>
        private void FindShortProfiles(int nominalProfileLength)
        {
            if (ForwardProfileLengths.Count() > 0)
            {
                ProfilsDefectsForward = new int[ForwardProfileLengths.Count()];
                for (int i = 0; i < ForwardProfileLengths.Count(); i++)
                {
                    ProfilsDefectsForward[i] = nominalProfileLength - ForwardProfileLengths[i];
                }
            }
            if (BackwardProfileLengths.Count() > 0)
            {
                ProfilsDefectsBackward = new int[BackwardProfileLengths.Count()];
                for (int i = 0; i < BackwardProfileLengths.Count(); i++)
                {
                    ProfilsDefectsBackward[i] = nominalProfileLength - BackwardProfileLengths[i];
                }
            }
        }

        /// <summary>
        /// Determines the direction status (StatusFromIndex) from 
        /// the number of profiles for forward and backward scans.
        /// Also justifies backward profile number if needed.
        /// </summary>
        private void AnalyzeDirectionStatus()
        {
            ScanStatus = ScanDirectionStatus.Unknown;
            SpuriousDataLines = 0;
            // no data at all
            if (ForwardProfileLengths.Count == 0)
            {
                ScanStatus = ScanDirectionStatus.NoData;
                return;
            }
            SpuriousProfiles = BackwardProfileLengths.Count - ForwardProfileLengths.Count;
            // backward scan data file invalid or not existing
            if (SpuriousProfiles < 0)
            {
                ScanStatus = ScanDirectionStatus.ForwardOnly;
                if (BackwardProfileLengths.Count != 0)
                    BackwardProfileLengths.Clear();
                SpuriousProfiles = 0;
                return;
            }
            // peculiar NMM file error
            if (SpuriousProfiles > 0)
            {
                ScanStatus = ScanDirectionStatus.ForwardAndBackwardJustified;
                for (int i = 0; i < SpuriousProfiles; i++)
                {
                    SpuriousDataLines += BackwardProfileLengths[i];
                }
                BackwardProfileLengths.RemoveRange(0, SpuriousProfiles);
                backwardTimeStamps.RemoveRange(0, SpuriousProfiles);
                return;
            }
            if (SpuriousProfiles == 0)
            {
                ScanStatus = ScanDirectionStatus.ForwardAndBackward;
                return;
            }
        }

        /// <summary>
        /// Loads data from an index file.
        /// </summary>
        /// <param name="fileName">The full file name.</param>
        /// <param name="direction">The scan direction.</param>
        private void LoadIndexData(string fileName, ScanDirection direction)
        {
            if (!File.Exists(fileName))
            {
                return;
            }
            string line;
            StreamReader hFile = File.OpenText(fileName);
            // file creation date as a first guess, will be overwritten.
            if (direction == ScanDirection.Forward)
            {
                CreationDate = File.GetCreationTime(fileName);
            }
            while ((line = hFile.ReadLine()) != null)
                ParseDataLine(line, direction);
            if (hFile != null) hFile.Close();
        }

        /// <summary>
        /// Parses a line of text for index data and add them on success.
        /// </summary>
        /// <param name="line">The line of text to be parsed.</param>
        /// <param name="direction">The scan direction.</param>
        private void ParseDataLine(string line, ScanDirection direction)
        {
            if (string.IsNullOrWhiteSpace(line)) return;
            char[] charSeparators = { ';' }; // fields are separated by semicolons
            string[] tokens = line.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 5) return;
            DataMask = long.Parse(tokens[2]);   // the data mask is hopefully always the same
            int scanLineLength = int.Parse(tokens[1]); 
            DateTime timeStamp = DateTime.ParseExact(tokens[0].Trim(), "dd-MMM-yyyy HH:mm:ss", new CultureInfo("de-DE")); // 01-Dez-2014 13:15:10
            if (direction == ScanDirection.Forward)
            {
                ForwardProfileLengths.Add(scanLineLength);
                forwardTimeStamps.Add(timeStamp);
            }
            if (direction == ScanDirection.Backward)
            {
                BackwardProfileLengths.Add(scanLineLength);
                backwardTimeStamps.Add(timeStamp);
            }
        }

        private readonly List<DateTime> forwardTimeStamps = new List<DateTime>();
        private readonly List<DateTime> backwardTimeStamps = new List<DateTime>();
        #endregion
    }
}
