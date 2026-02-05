using ImageAnalysis.Helpers;
using ImageAnalysis.Readers;
using Microsoft.Win32;
using FellowOakDicom.Imaging;
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

        private string multiFrameFile = null;
        private int totalFrames = 1;

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
                multiFrameFile = null;
                currentIndex = 0;

                var fileType = FileTypeDetector.Detect(dialog.FileName);
                StatusText.Text = $"Detected: {fileType}";

                if (fileType == MedicalFileType.Dicom)
                {
                    try
                    {
                        var dicom = new DicomImage(dialog.FileName);
                        totalFrames = dicom.NumberOfFrames;

                        if (totalFrames > 1)
                        {
                            multiFrameFile = dialog.FileName;
                            ImageViewer.Source =
                                DicomImageReader.LoadImage(multiFrameFile, 0);
                            StatusText.Text = $"Frame 1 / {totalFrames}";
                        }
                        else
                        {
                            ImageViewer.Source =
                                DicomImageReader.LoadImage(dialog.FileName);
                        }

                        MetadataList.ItemsSource =
                            DicomMetadataReader.ReadMetadata(dialog.FileName);
                    }
                    catch (FellowOakDicom.Imaging.Codec.DicomCodecException)
                    {
                        string decompressed =
                            DicomDecompressor.Decompress(dialog.FileName);

                        var dicom = new DicomImage(decompressed);
                        totalFrames = dicom.NumberOfFrames;

                        if (totalFrames > 1)
                        {
                            
                            multiFrameFile = decompressed;
                            currentIndex = 0;
                            ImageViewer.Source =
                                DicomImageReader.LoadImage(decompressed, 0);
                            StatusText.Text = $"Frame 1 / {totalFrames}";
                        }
                        else
                        {
                            ImageViewer.Source =
                                DicomImageReader.LoadImage(decompressed);
                        }

                        MetadataList.ItemsSource =
                            DicomMetadataReader.ReadMetadata(decompressed);
                    }
                }
                else if (fileType == MedicalFileType.Hdf5)
                {
                    ImageViewer.Source =
                        Hdf5ImageReader.LoadImage(dialog.FileName);
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
                    System.Windows.MessageBox.Show("No DICOM files found.");
                    return;
                }

                multiFrameFile = null;
                currentIndex = 0;

                LoadFolderSlice(0);
            }
        }

        private void LoadFolderSlice(int index)
        {
            ImageViewer.Source =
                DicomImageReader.LoadImage(dicomFiles[index]);

            MetadataList.ItemsSource =
                DicomMetadataReader.ReadMetadata(dicomFiles[index]);

            StatusText.Text =
                $"Slice {index + 1} / {dicomFiles.Count}";
        }

        private void LoadMultiFrameSlice(int index)
        {
            ImageViewer.Source =
                DicomImageReader.LoadImage(multiFrameFile, index);

            StatusText.Text =
                $"Frame {index + 1} / {totalFrames}";
        }

        private void ImageViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((DateTime.Now - lastScrollTime).TotalMilliseconds < ScrollDelayMs)
                return;

            lastScrollTime = DateTime.Now;

            currentIndex += e.Delta > 0 ? -1 : 1;

            if (multiFrameFile != null)
            {
                currentIndex = Math.Max(0, Math.Min(currentIndex, totalFrames - 1));
                LoadMultiFrameSlice(currentIndex);
            }
            else if (dicomFiles.Count > 0)
            {
                currentIndex = Math.Max(0, Math.Min(currentIndex, dicomFiles.Count - 1));
                LoadFolderSlice(currentIndex);
            }
        }
    }
}
