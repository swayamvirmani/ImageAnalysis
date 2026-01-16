using FellowOakDicom;
using System.Collections.Generic;

namespace ImageAnalysis.Readers
{
    public static class DicomMetadataReader
    {
        public static Dictionary<string, string> ReadMetadata(string filePath)
        {
            var dicomFile = DicomFile.Open(filePath);
            var ds = dicomFile.Dataset;

            var metadata = new Dictionary<string, string>();

            metadata["Patient Name"] = ds.GetSingleValueOrDefault(DicomTag.PatientName, "N/A");
            metadata["Patient ID"] = ds.GetSingleValueOrDefault(DicomTag.PatientID, "N/A");
            metadata["Study Date"] = ds.GetSingleValueOrDefault(DicomTag.StudyDate, "N/A");
            metadata["Modality"] = ds.GetSingleValueOrDefault(DicomTag.Modality, "N/A");
            metadata["Slice Thickness"] = ds.GetSingleValueOrDefault(DicomTag.SliceThickness, "N/A");

           
            if (ds.TryGetValues(DicomTag.PixelSpacing, out double[] spacing))
            {
                metadata["Pixel Spacing"] = $"{spacing[0]} x {spacing[1]}";
            }
            else
            {
                metadata["Pixel Spacing"] = "N/A";
            }

            return metadata;
        }
    }
}
