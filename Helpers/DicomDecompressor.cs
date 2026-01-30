using System;
using System.Diagnostics;
using System.IO;

namespace ImageAnalysis.Helpers
{
    public static class DicomDecompressor
    {
        private static string DcmtkPath =
@"C:\ProgramData\chocolatey\lib\dcmtk\tools\dcmtk-3.7.0-win64-chocolatey\bin\dcmdjpeg.exe";

        public static string Decompress(string inputPath)
        {
            string folder = Path.GetDirectoryName(inputPath);
            string outputPath = Path.Combine(folder, "decompressed.dcm");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = DcmtkPath,
                Arguments = $"\"{inputPath}\" \"{outputPath}\"",
                WorkingDirectory = folder,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Process p = Process.Start(psi);
            p.WaitForExit();

            if (p.ExitCode != 0 || !File.Exists(outputPath))
                throw new Exception("DICOM decompression failed");

            return outputPath;
        }
    }
}
