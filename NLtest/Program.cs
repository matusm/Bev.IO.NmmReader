using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bev.IO.NmmReader;
using Bev.IO.NmmReader.scan_mode;

namespace NLtest
{
    class Program
    {
        static void Main(string[] args)
        {
            string filename = args[0];

            Console.WriteLine($"Reading {filename}");

            NmmFileName nmmFileName = new NmmFileName(filename);
            NmmScanData nmmScanData = new NmmScanData(nmmFileName);

            double[] rawData = nmmScanData.ExtractProfile("-LZ+AZ", 0, TopographyProcessType.ForwardOnly);
            double[] rawSin = nmmScanData.ExtractProfile("F4", 0, TopographyProcessType.ForwardOnly);
            double[] rawCos = nmmScanData.ExtractProfile("F5", 0, TopographyProcessType.ForwardOnly);

            Console.WriteLine($"Number of data point {rawData.Length}");

            Console.WriteLine($"Correcting data");

            NLcorrectionHeydemann heydemann = new NLcorrectionHeydemann(rawData, rawSin, rawCos);
            double[] hData = heydemann.CorrectedData;
            double[] hSin = heydemann.CorrectedSinValues;
            double[] hCos = heydemann.CorrectedCosValues;

            NLcorrectionDai dai = new NLcorrectionDai(hData, hSin, hCos);
            double[] dData = dai.CorrectedData;
            double[] dSin = dai.CorrectedSinValues;
            double[] dCos = dai.CorrectedCosValues;

            int numberPoints = Math.Min(10_000, rawData.Length);
            using (StreamWriter writer = new StreamWriter(nmmFileName.BaseFileName+".csv", false))
            {
                Console.WriteLine($"Writing {nmmFileName.BaseFileName + ".csv"}");
                for (int i = 0; i < numberPoints; i++)
                {
                    double s0 = rawSin[i];
                    double c0 = rawCos[i];
                    double s1 = hSin[i];
                    double c1 = hCos[i];
                    double s2 = dSin[i];
                    double c2 = dCos[i];
                    string line = $"{s0}, {c0}, {PhiDeg(s0, c0)}, {Radius(s0, c0)},  {s1}, {c1}, {PhiDeg(s1, c1)}, {Radius(s1, c1)}, {s2}, {c2}, {PhiDeg(s2, c2)}, {Radius(s2, c2)}";
                    writer.WriteLine(line);
                }
            }

            Console.WriteLine("done.");
        }


        static double Radius(double x, double y) => Math.Sqrt(x * x + y * y);

        static double PhiDeg(double x, double y) => Phi(x, y) * 180 / Math.PI;

        static double Phi(double x, double y) => Math.Atan2(y, x);
    }
}
