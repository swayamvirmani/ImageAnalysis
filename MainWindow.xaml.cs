using ImageAnalysis.Helpers;
using ImageAnalysis.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace ImageAnalysis
{
    public partial class MainWindow : Window
    {
        private List<string> dicomFiles = new List<string>();
        private int currentIndex = 0;

       
        private DateTime lastScrollTime = DateTime.MinValue;
        private const int ScrollDelayMs = 120;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                dicomFiles = Directory
                    .GetFiles(dialog.SelectedPath, "*.dcm")
                    .OrderBy(f => f)
                    .ToList();

                if (dicomFiles.Count == 0)
                {
                    System.Windows.MessageBox.Show("No DICOM files found in the selected folder.");
                    return;
                }

                currentIndex = 0;
                LoadDicomSlice(currentIndex);
            }
        }

        private void LoadDicomSlice(int index)
        {
            if (index < 0 || index >= dicomFiles.Count)
                return;

            ImageViewer.Source = DicomImageReader.LoadImage(dicomFiles[index]);

            var metadata = DicomMetadataReader.ReadMetadata(dicomFiles[index]);
            MetadataList.ItemsSource = metadata;

            StatusText.Text = $"Slice {index + 1} / {dicomFiles.Count}";
        }

        private void ImageViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (dicomFiles == null || dicomFiles.Count == 0)
                return;
            if ((DateTime.Now - lastScrollTime).TotalMilliseconds < ScrollDelayMs)
                return;

            lastScrollTime = DateTime.Now;

            if (e.Delta > 0)
                currentIndex--;
            else
                currentIndex++;

            currentIndex = Math.Max(0, Math.Min(currentIndex, dicomFiles.Count - 1));

            LoadDicomSlice(currentIndex);
        }
    }
}
