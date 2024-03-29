﻿// this is a dirty test program to evaluate the nonlinearity correction routines of Bev.IO.NmmReader

using System;
using System.IO;
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
            if (args.Length == 2)
                nmmFileName.SetScanIndex(int.Parse(args[1]));
            NmmScanData nmmScanData = new NmmScanData(nmmFileName);

            double[] rawData = nmmScanData.ExtractProfile("-LZ+AZ", 0, TopographyProcessType.ForwardOnly);
            double[] rawSin = nmmScanData.ExtractProfile("F4", 0, TopographyProcessType.ForwardOnly);
            double[] rawCos = nmmScanData.ExtractProfile("F5", 0, TopographyProcessType.ForwardOnly);
            Quad[] rawSignal = CombineSignals(rawSin, rawCos);

            Console.WriteLine($"Number of data point {rawData.Length}");
            Console.WriteLine($"Correcting data");

            NLcorrectionHeydemann heydemann = new NLcorrectionHeydemann(rawData, rawSignal);
            double[] hData = heydemann.CorrectedData;
            Quad[] hSignal = heydemann.CorrectedQuadratureValues;

            Console.WriteLine($"Heydemann: {heydemann.Status} ({heydemann.CorrectionSpan * 1e9:F1} nm)");
            Console.WriteLine($"Signal: {hSignal.Length}");

            NLcorrectionDai dai = new NLcorrectionDai(hData, hSignal);
            Quad[] dSignal = dai.CorrectedQuadratureValues;

            int numberPoints = Math.Min(10_000, rawData.Length);
            using (StreamWriter writer = new StreamWriter(nmmFileName.BaseFileName + ".csv", false))
            {
                Console.WriteLine($"Writing {nmmFileName.BaseFileName + ".csv"}");
                for (int i = 0; i < rawSignal.Length; i++)
                {
                    Quad q0 = rawSignal[i];
                    Quad q1 = hSignal[i];
                    Quad q2 = dSignal[i];
                    string line = $"{q0.Sin}, {q0.Cos}, {q0.PhiDeg}, {q0.Radius}, {q1.Sin}, {q1.Cos}, {q1.PhiDeg}, {q1.Radius}, {q2.Sin}, {q2.Cos}, {q2.PhiDeg}, {q2.Radius}";
                    writer.WriteLine(line);
                }
            }

            Console.WriteLine("done.");
        }

        static Quad[] CombineSignals(double[] sinValues, double[] cosValues)
        {
            Quad[] quad = new Quad[sinValues.Length];
            for (int i = 0; i < quad.Length; i++)
            {
                quad[i] = new Quad(sinValues[i], cosValues[i]);
            }
            return quad;
        }
    }
}
