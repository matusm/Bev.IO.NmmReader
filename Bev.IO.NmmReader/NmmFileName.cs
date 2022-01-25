/*
 * 
 */

using System.IO;

namespace Bev.IO.NmmReader
{
    public class NmmFileName
    {
        public NmmFileName(string path)
        {
            BasePath = Path.ChangeExtension(path, null);
            ScanIndex = 0;
        }

        public string BaseFileName => Path.GetFileNameWithoutExtension(BasePath);
        public string BasePath { get; private set; }
        public int ScanIndex { get; private set; }

        // setting index to 0 is used for a single scan without special filenames
        public void SetScanIndex(int index)
        {
            if (index < 0) index = 0;
            ScanIndex = index;
        }

        public string GetDatFileNameForScanIndex(ScanDirection direction)
        {
            return GetFileName(direction, "dat", ScanIndex);
        }

        public string GetPosFileNameForScanIndex(ScanDirection direction)
        {
            return GetFileName(direction, "pos", ScanIndex);
        }

        public string GetIndFileNameForScanIndex(ScanDirection direction)
        {
            return GetFileName(direction, "ind", ScanIndex);
        }

        public string GetDscFileNameForScanIndex(ScanDirection direction)
        {
            return GetFileName(direction, "dsc", ScanIndex);
        }

        // overloads without parameters return the forward scan files
        public string GetDatFileName()
        {
            return GetFreeFileName("dat");
        }

        public string GetPosFileName()
        {
            return GetFreeFileName("pos");
        }

        public string GetIndFileName()
        {
            return GetFreeFileName("ind");
        }

        public string GetDscFileName()
        {
            return GetFreeFileName("dsc"); ;
        }

        // 3D-files have neither a direction nor multiple scans
        public string Get3DFileName()
        {
            return GetFreeFileName("3d");
        }

        public string GetTrgFileName()
        {
            return GetFreeFileName("trg");
        }

        // this may be used to generate names for output files.
        public string GetFreeFileName(string extension)
        {
            return GetFileName(ScanDirection.Forward, extension, 0);
        }

        public string GetFreeFileNameWithIndex(string extension)
        {
            return GetFileName(ScanDirection.Forward, extension, ScanIndex);
        }


        private string GetFileName(ScanDirection direction, string extension, int index)
        {
            switch (direction)
            {
                case ScanDirection.Forward:
                    return GetFwdFileName(extension, index);
                case ScanDirection.Backward:
                    return GetBwdFileName(extension, index);
                default:
                    return string.Empty;
            }
        }

        private string GetFwdFileName(string extension, int index)
        {
            if (index > 0)
            {
                return Path.ChangeExtension(
                    BasePath + string.Format("_{0}", index),
                    extension
                );
            }
            return Path.ChangeExtension(BasePath, extension);
        }

        private string GetBwdFileName(string extension, int index)
        {
            if (index > 0)
            {
                return Path.ChangeExtension(
                    BasePath + string.Format("b_{0}", index),
                    extension
                );
            }
            return Path.ChangeExtension(BasePath + "b", extension);
        }

        public override string ToString()
        {
            if (ScanIndex == 0)
                return $"[NmmFileName: {BaseFileName}]";
            return $"[NmmFileName: {BaseFileName} for scan: {ScanIndex}]";
        }

    }
}
