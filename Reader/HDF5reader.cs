using HDF.PInvoke;
using System;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageAnalysis.Readers
{
    public static class Hdf5ImageReader
    {
        public static BitmapSource LoadImage(string filePath)
        {
            
            long fileId = H5F.open(filePath, H5F.ACC_RDONLY);
            if (fileId < 0) throw new Exception("Failed to open HDF5 file.");

            
            long datasetId = H5D.open(fileId, "image");
            long spaceId = H5D.get_space(datasetId);

            ulong[] dims = new ulong[2];
            H5S.get_simple_extent_dims(spaceId, dims, null);

            int height = (int)dims[0];
            int width = (int)dims[1];

            float[] data = new float[width * height];

            GCHandle hnd = GCHandle.Alloc(data, GCHandleType.Pinned);
            H5D.read(
                datasetId,
                H5T.NATIVE_FLOAT,
                H5S.ALL,
                H5S.ALL,
                H5P.DEFAULT,
                hnd.AddrOfPinnedObject());
            hnd.Free();

            
            float min = float.MaxValue;
            float max = float.MinValue;
            foreach (var v in data)
            {
                if (v < min) min = v;
                if (v > max) max = v;
            }

            byte[] pixels = new byte[width * height * 4];
            for (int i = 0; i < width * height; i++)
            {
                byte val = (byte)(255 * (data[i] - min) / (max - min));
                pixels[i * 4 + 0] = val;
                pixels[i * 4 + 1] = val;
                pixels[i * 4 + 2] = val;
                pixels[i * 4 + 3] = 255;
            }

            
            H5D.close(datasetId);
            H5S.close(spaceId);
            H5F.close(fileId);

            WriteableBitmap bitmap = new WriteableBitmap(
                width,
                height,
                96,
                96,
                PixelFormats.Bgra32,
                null);

            bitmap.WritePixels(
                new System.Windows.Int32Rect(0, 0, width, height),
                pixels,
                width * 4,
                0);

            return bitmap;
        }
    }
}
