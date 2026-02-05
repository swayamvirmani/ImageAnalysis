using FellowOakDicom.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageAnalysis.Readers
{
    public static class DicomImageReader
    {
        public static BitmapSource LoadImage(string filePath)
        {
            var dicomImage = new DicomImage(filePath);
            return Render(dicomImage, 0);
        }

        public static BitmapSource LoadImage(string filePath, int frameIndex)
        {
            var dicomImage = new DicomImage(filePath);
            dicomImage.WindowWidth = 300;
            dicomImage.WindowCenter = 100;
            return Render(dicomImage, frameIndex);
        }

        private static BitmapSource Render(DicomImage image, int frameIndex)
        {
            var img = image.RenderImage(frameIndex);

            int width = img.Width;
            int height = img.Height;

            byte[] pixels = img.As<byte[]>();

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
