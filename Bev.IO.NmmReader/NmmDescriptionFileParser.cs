using System.IO;
using System.Globalization;
using System.Collections.Generic;
using Bev.IO.NmmReader._3d_mode;

namespace Bev.IO.NmmReader
{
    /// <summary>
    /// Class consumes description files (*.dsc) produced during either a scan or a 3d measurement on the SIOS NMM.
    /// Backtrace description files are not used by this class.
    /// </summary>
    public class NmmDescriptionFileParser
    {
        static readonly NumberFormatInfo numFormat = new NumberFormatInfo() { NumberDecimalSeparator = "." };

        #region Ctor

        /// <summary>
        /// Reads a description file and analyzes all relevant information.
        /// Parameters are provided as properties.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        public NmmDescriptionFileParser(NmmFileName fileName)
        {
            // try to load files
            LoadDescriptionFile(fileName.GetDscFileName());
            // determine the measurement procedure
            DetermineMeasurementProcedure();
            // analyze data origin in more detail depending on procedure
            switch (Procedure)
            {
                case MeasurementProcedure.Unknown:
                    break;
                case MeasurementProcedure.Scan:
                    ParseScanDescription();
                    break;
                case MeasurementProcedure.Point3D:
                    Parse3DPointDescription();
                    break;
                case MeasurementProcedure.Object3D:
                    Parse3DObjectDescription();
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Properties

        // common (scan and 3d) properties
        public MeasurementProcedure Procedure { get; private set; } = MeasurementProcedure.Unknown;

        // 3d properties
        public string SpecimenIdentifier { get; private set; }
        public double ProbeDiameter { get; private set; }
        public int NumberOfDataPoints { get; private set; }

        // scan properties
        public List<string> ScanComments { get { return scanComments; } }
        public string SpmTechnique { get; private set; } = "unknown SPM technique";
        public double ScanFieldDeltaX { get; private set; }
        public double ScanFieldDeltaY { get; private set; }
        public double ScanFieldDimensionX { get; private set; }
        public double ScanFieldDimensionY { get; private set; }
        public double ScanFieldRotation { get; private set; }
        public double ScanSpeed { get; private set; }
        public int NumberOfProfiles { get; private set; }
        public int NumberOfScans { get; private set; } = 0;
        public long DataMask { get; private set; }

        #endregion

        #region Private stuff

        /// <summary>
        /// Loads the description text file in a list of strings. Empty lines are discarded, remaining lines are trimmed.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        private void LoadDescriptionFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }
            string line;
            StreamReader hFile = File.OpenText(fileName);
            while ((line = hFile.ReadLine()) != null)
                if (!string.IsNullOrWhiteSpace(line))
                    fileContent.Add(line.Trim());
            if (hFile != null) hFile.Close();
        }

        /// <summary>
        /// Determines the type of measurement procedure (scan, 3D, ...)
        /// </summary>
        private void DetermineMeasurementProcedure()
        {
            if (fileContent.Count == 0) return;
            foreach (string s in fileContent)
            {
                if (s.Contains("NMM Control 3D Point cloud description file"))
                {
                    Procedure = MeasurementProcedure.Point3D;
                    return;
                }
                if (s.Contains("3D Object measurement description file"))
                {
                    Procedure = MeasurementProcedure.Object3D;
                    return;
                }
                if (s.Contains("Scan procedure description file"))
                {
                    Procedure = MeasurementProcedure.Scan;
                    return;
                }
            }
        }

        /// <summary>
        /// Analyzes number of data points, sample designation, probe sphere diameter and object type.
        /// </summary>
        private void Parse3DObjectDescription()
        {
            string sTemp;
            foreach (string s in fileContent)
            {
                if (s.Contains("Number of data points in file :"))
                {
                    sTemp = s.Replace("Number of data points in file :", " ");
                    NumberOfDataPoints = int.Parse(sTemp, numFormat);
                }
                if (s.Contains("Object name :"))
                {
                    sTemp = s.Replace("Object name :", " ");
                    SpecimenIdentifier = sTemp.Trim();
                }
                if (s.Contains("Measuring object : Plane"))
                {
                    objectType = ObjectType.Plane;
                }
                if (s.Contains("Measuring object : Sphere"))
                {
                    objectType = ObjectType.Sphere;
                }
                if (s.Contains("2. Probe diameter : "))
                {
                    sTemp = s.Replace("2. Probe diameter : ", " ");
                    sTemp = sTemp.Replace("mm", " ");
                    ProbeDiameter = double.Parse(sTemp, numFormat);
                }
                if (s.Contains("Current probe sphere diameter : "))
                {
                    sTemp = s.Replace("Current probe sphere diameter : ", " ");
                    sTemp = sTemp.Replace("mm", " ");
                    ProbeDiameter = double.Parse(sTemp, numFormat);
                }
            }
        }

        /// <summary>
        /// Analyzes generic 3D files (nothing to analyze, unfortunately).
        /// </summary>
        private void Parse3DPointDescription()
        {
            // not much one can extract from this type of file
            objectType = ObjectType.Free;
            return;
        }

        /// <summary>
        /// Analyzes a multitude of different parameters for a topographic scan file.
        /// </summary>
        private void ParseScanDescription()
        {
            objectType = ObjectType.None;
            // first extract the respective chapter, also for later use
            scanSaveProcedure = ExtractChapterFromFile("1. Save procedure");
            scanProbeSystem = ExtractChapterFromFile("2. Probe system");
            scanFieldParameters = ExtractChapterFromFile("3. Scan field");
            scanComments = ExtractChapterFromFile("5. Additional comments");
            // and now parse the first three chapters
            ParseScanDescriptionForProcedure();
            ParseScanDescriptionForProbe();
            ParseScanDescriptionForFieldParameters();
        }

        /// <summary>
        /// Extracts contents of a specific chapter as a list of strings.
        /// </summary>
        /// <param name="chapter">Title of chapter.</param>
        /// <returns>Contents of the chapter.</returns>
        private List<string> ExtractChapterFromFile(string chapter)
        {
            List<string> temp = new List<string>();
            bool inChapter = false;
            foreach (string s in fileContent)
            {
                if (inChapter == true)
                {
                    // magic string!
                    if (s.Contains("-----------------------------------------"))
                        return temp;
                    if (!string.IsNullOrWhiteSpace(s))
                        temp.Add(s); // no Trim() needed as fileContent is already trimmed.
                }
                if (s.Contains(chapter)) inChapter = true;
            }
            return temp;
        }

        /// <summary>
        /// This method determines the SPM probe type (AFM or LFS)
        /// </summary>
        private void ParseScanDescriptionForProbe()
        {
            SpmTechnique = "unknown SPM technique";
            foreach (string s in scanProbeSystem)
            {
                if (s.Contains("AFM"))
                    SpmTechnique = "NC-AFM";
                if (s.Contains("Laser Focus"))
                    SpmTechnique = "LFS";
            }
        }

        /// <summary>
        /// This method extracts the data mask.
        /// </summary>
        private void ParseScanDescriptionForProcedure()
        {
            string sTemp;
            foreach (string s in scanSaveProcedure)
            {
                if (s.Contains("Current data mask       : "))
                {
                    sTemp = s.Replace("Current data mask       : ", " ");
                    DataMask = long.Parse(sTemp, numFormat);
                }
            }
        }

        /// <summary>
        /// This method determines the scan field parameters.
        /// </summary>
        private void ParseScanDescriptionForFieldParameters()
        {
            string sTemp;
            foreach (string s in scanFieldParameters)
            {
                // read the dx and dy values to check the scan field dimensions.
                // a premature stop of the scan can cause an inconsistency.
                // dsc files of older versions do not contain dy
                if (s.Contains("Distance beetween points  : ")) // sic!
                {
                    sTemp = s.Replace("Distance beetween points  : ", " ");
                    sTemp = sTemp.Replace("[um]", " ");
                    sTemp = sTemp.Replace("[�m]", " ");
                    ScanFieldDeltaX = double.Parse(sTemp, numFormat);
                    ScanFieldDeltaX *= 1.0E-6; // convert to m
                }
                if (s.Contains("Distance beetween point  : ")) // sic!
                {
                    sTemp = s.Replace("Distance beetween point  : ", " ");
                    sTemp = sTemp.Replace("[um]", " ");
                    sTemp = sTemp.Replace("[�m]", " ");
                    ScanFieldDeltaX = double.Parse(sTemp, numFormat);
                    ScanFieldDeltaX *= 1.0E-6; // convert to m
                }
                if (s.Contains("Distance beetween lines   : ")) // sic!
                {
                    sTemp = s.Replace("Distance beetween lines   : ", " ");
                    sTemp = sTemp.Replace("[um]", " ");
                    sTemp = sTemp.Replace("[�m]", " ");
                    ScanFieldDeltaY = double.Parse(sTemp, numFormat);
                    ScanFieldDeltaY *= 1.0E-6; // convert to m
                }
                if (s.Contains("Scan line length :"))
                {
                    sTemp = s.Replace("Scan line length :", " ");
                    sTemp = sTemp.Replace("[um]", " ");
                    sTemp = sTemp.Replace("[�m]", " ");
                    ScanFieldDimensionX = double.Parse(sTemp, numFormat); // might be negative!
                    ScanFieldDimensionX *= 1.0E-6; // convert to m
                }
                if (s.Contains("Scan field width :"))
                {
                    sTemp = s.Replace("Scan field width :", " ");
                    sTemp = sTemp.Replace("[um]", " ");
                    sTemp = sTemp.Replace("[�m]", " ");
                    ScanFieldDimensionY = double.Parse(sTemp, numFormat); // might be negative!
                    ScanFieldDimensionY *= 1.0E-6; // convert to m
                }
                if (s.Contains("Scan angle  :"))
                {
                    sTemp = s.Replace("Scan angle  :", " ");
                    sTemp = sTemp.Replace("rad", " ");
                    ScanFieldRotation = double.Parse(sTemp, numFormat);
                    ScanFieldRotation *= 57.29577951308232087; // convert from rad to °
                }
                if (s.Contains("Scan speed  :"))
                {
                    sTemp = s.Replace("Scan speed  :", " ");
                    sTemp = sTemp.Replace("[um/s]", " ");
                    sTemp = sTemp.Replace("[�m/s]", " ");
                    ScanSpeed = double.Parse(sTemp, numFormat);
                    ScanSpeed *= 1.0e-6; // convert to m/s
                }
                if (s.Contains("Number of lines  :"))
                {
                    sTemp = s.Replace("Number of lines  :", " ");
                    NumberOfProfiles = int.Parse(sTemp, numFormat);
                }
                if (s.Contains("Number of scans  : "))
                {
                    sTemp = s.Replace("Number of scans  : ", " ");
                    NumberOfScans = int.Parse(sTemp, numFormat);
                }
            }
        }

        // for housekeeping
        private ObjectType objectType = ObjectType.None;
        // the file is loaded in this bunch of strings
        private readonly List<string> fileContent = new List<string>();
        // chapters of scan description files
        private List<string> scanSaveProcedure = new List<string>(); // text lines following "1. Save procedure"
        private List<string> scanProbeSystem = new List<string>(); // text lines following "2. Probe system"
        private List<string> scanFieldParameters = new List<string>(); // text lines following "3. Scan field"
        private List<string> scanComments = new List<string>(); // text lines following "5. Additional comments"
        #endregion

    }
}
