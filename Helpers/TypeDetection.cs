using System.Collections;
using System.IO;
using System.Text;

namespace ImageAnalysis.Helpers
{
    public enum MedicalFileType
    {
        Dicom,
        Hdf5,
        Unknown
    }

    public static class FileTypeDetector
    {
        public static MedicalFileType Detect(string filePath)
        {
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            
            byte[] hdf5Signature = new byte[8];
            fs.Read(hdf5Signature, 0, 8);

            byte[] expectedHdf5 =
            {
                0x89, 0x48, 0x44, 0x46,
                0x0D, 0x0A, 0x1A, 0x0A
            };

            if (StructuralComparisons.StructuralEqualityComparer
                .Equals(hdf5Signature, expectedHdf5))
            {
                return MedicalFileType.Hdf5;
            }

            
            fs.Seek(128, SeekOrigin.Begin);
            byte[] dicm = new byte[4];
            fs.Read(dicm, 0, 4);

            if (Encoding.ASCII.GetString(dicm) == "DICM")
            {
                return MedicalFileType.Dicom;
            }

            return MedicalFileType.Unknown;
        }
    }
}
