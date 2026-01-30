using ImageAnalysis.Helpers;
using ImageAnalysis.Readers;
using Microsoft.Win32;
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

        private void UploadFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();


            if (dialog.ShowDialog() == true)
            {
                dicomFiles.Clear();
                MetadataList.ItemsSource = null;

                var fileType = FileTypeDetector.Detect(dialog.FileName);
                StatusText.Text = $"Detected: {fileType}";

                if (fileType == MedicalFileType.Dicom)
                {
                    try
                    {
                        ImageViewer.Source = DicomImageReader.LoadImage(dialog.FileName);
                        MetadataList.ItemsSource = DicomMetadataReader.ReadMetadata(dialog.FileName);
                    }
                    catch (FellowOakDicom.Imaging.Codec.DicomCodecException)
                    {
                        string decompressed =
                            DicomDecompressor.Decompress(dialog.FileName);

                        ImageViewer.Source =
                            DicomImageReader.LoadImage(decompressed);

                        MetadataList.ItemsSource =
                            DicomMetadataReader.ReadMetadata(decompressed);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }
                }
                else if (fileType == MedicalFileType.Hdf5)
                {
                    ImageViewer.Source = Hdf5ImageReader.LoadImage(dialog.FileName);
                }
                else
                {
                    System.Windows.MessageBox.Show("Unsupported file format.");
                }
            }
        }

        private void UploadFolder_Click(object sender, RoutedEventArgs e)
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

            try
            {
                ImageViewer.Source = DicomImageReader.LoadImage(dicomFiles[index]);
                MetadataList.ItemsSource = DicomMetadataReader.ReadMetadata(dicomFiles[index]);
                StatusText.Text = $"Slice {index + 1} / {dicomFiles.Count}";
            }
            catch (FellowOakDicom.Imaging.Codec.DicomCodecException)
            {
                string decompressed =
                    DicomDecompressor.Decompress(dicomFiles[index]);

                ImageViewer.Source =
                    DicomImageReader.LoadImage(decompressed);

                MetadataList.ItemsSource =
                    DicomMetadataReader.ReadMetadata(decompressed);

                StatusText.Text = $"Slice {index + 1} / {dicomFiles.Count}";
            }
        }

        private void ImageViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (dicomFiles.Count == 0)
                return;

            if ((DateTime.Now - lastScrollTime).TotalMilliseconds < ScrollDelayMs)
                return;

            lastScrollTime = DateTime.Now;

            currentIndex += e.Delta > 0 ? -1 : 1;
            currentIndex = Math.Max(0, Math.Min(currentIndex, dicomFiles.Count - 1));

            LoadDicomSlice(currentIndex);
        }
    }
}
