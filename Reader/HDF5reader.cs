using HDF.PInvoke;
using System;
using System.Collections.Generic;
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
            if (fileId < 0) throw new Exception("Cannot open HDF5 file.");

            List<string> datasets = new List<string>();
            ulong idx = 0;

            H5L.iterate(
                fileId,
                H5.index_t.NAME,
                H5.iter_order_t.NATIVE,
                ref idx,
                (long g, IntPtr name, ref H5L.info_t info, IntPtr op) =>
                {
                    string n = Marshal.PtrToStringAnsi(name);
                    datasets.Add(n);
                    return 0;
                },
                IntPtr.Zero);

            if (datasets.Count == 0)
                throw new Exception("No datasets found.");

            string selected = null;
            int bestPixels = 0;

            foreach (string d in datasets)
            {
                long did = H5D.open(fileId, d);
                if (did < 0) continue;

                long sid = H5D.get_space(did);
                int rank = H5S.get_simple_extent_ndims(sid);
                if (rank < 2 || rank > 3)
                {
                    H5S.close(sid);
                    H5D.close(did);
                    continue;
                }

                ulong[] dims = new ulong[rank];
                H5S.get_simple_extent_dims(sid, dims, null);

                int pixels = rank == 2
                    ? (int)(dims[0] * dims[1])
                    : (int)(dims[1] * dims[2]);

                if (pixels > bestPixels)
                {
                    bestPixels = pixels;
                    selected = d;
                }

                H5S.close(sid);
                H5D.close(did);
            }

            if (selected == null)
                throw new Exception("No image dataset found.");

            long datasetId = H5D.open(fileId, selected);
            long spaceId = H5D.get_space(datasetId);

            int r = H5S.get_simple_extent_ndims(spaceId);
            ulong[] shape = new ulong[r];
            H5S.get_simple_extent_dims(spaceId, shape, null);

            int height = r == 2 ? (int)shape[0] : (int)shape[1];
            int width = r == 2 ? (int)shape[1] : (int)shape[2];

            float[] data = new float[width * height];

            long typeId = H5D.get_type(datasetId);
            H5T.class_t cls = H5T.get_class(typeId);

            if (cls == H5T.class_t.INTEGER)
            {
                ushort[] raw = new ushort[width * height];

                long memSpace = H5S.create_simple(2, new ulong[] { (ulong)height, (ulong)width }, null);
                ulong[] start = new ulong[] { 0, 0, 0 };
                ulong[] count = new ulong[] { 1, (ulong)height, (ulong)width };

                H5S.select_hyperslab(
                    spaceId,
                    H5S.seloper_t.SET,
                    start,
                    null,
                    count,
                    null);

                GCHandle h = GCHandle.Alloc(raw, GCHandleType.Pinned);

                H5D.read(
                    datasetId,
                    H5T.NATIVE_USHORT,
                    memSpace,
                    spaceId,
                    H5P.DEFAULT,
                    h.AddrOfPinnedObject());

                h.Free();
                H5S.close(memSpace);

                for (int i = 0; i < raw.Length; i++)
                    data[i] = raw[i];
            }

            else
            {
                GCHandle h = GCHandle.Alloc(data, GCHandleType.Pinned);

                H5D.read(
                    datasetId,
                    H5T.NATIVE_FLOAT,
                    H5S.ALL,
                    H5S.ALL,
                    H5P.DEFAULT,
                    h.AddrOfPinnedObject());

                h.Free();
            }

            float min = float.MaxValue;
            float max = float.MinValue;

            foreach (float v in data)
            {
                if (v < min) min = v;
                if (v > max) max = v;
            }

            if (max == min) max = min + 1f;

            byte[] pixelsOut = new byte[width * height * 4];

            for (int i = 0; i < data.Length; i++)
            {
                byte v = (byte)(255f * (data[i] - min) / (max - min));
                int p = i * 4;
                pixelsOut[p] = v;
                pixelsOut[p + 1] = v;
                pixelsOut[p + 2] = v;
                pixelsOut[p + 3] = 255;
            }

            H5D.close(datasetId);
            H5S.close(spaceId);
            H5F.close(fileId);

            WriteableBitmap bmp = new WriteableBitmap(
                width,
                height,
                96,
                96,
                PixelFormats.Bgra32,
                null);

            bmp.WritePixels(
                new System.Windows.Int32Rect(0, 0, width, height),
                pixelsOut,
                width * 4,
                0);

            return bmp;
        }
    }
}
