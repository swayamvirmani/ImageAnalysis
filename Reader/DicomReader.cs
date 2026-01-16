using FellowOakDicom.Imaging;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageAnalysis.Readers
{
    public static class DicomImageReader
    {
        public static BitmapSource LoadImage(string filePath)
        {
            var dicomImage = new DicomImage(filePath);

            
            dicomImage.WindowWidth = 400;
            dicomImage.WindowCenter = 40;

            var image = dicomImage.RenderImage();

            int width = image.Width;
            int height = image.Height;

            byte[] pixels = image.As<byte[]>();

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
