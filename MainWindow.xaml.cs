using ImageAnalysis.Helpers;
using ImageAnalysis.Readers;
using Microsoft.Win32;
using System.Windows;


namespace ImageAnalysis
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                var fileType = FileTypeDetector.Detect(dialog.FileName);

                StatusText.Text = $"Detected format: {fileType}";
                MetadataList.ItemsSource = null;

                if (fileType == MedicalFileType.Dicom)
                {
                    ImageViewer.Source = DicomImageReader.LoadImage(dialog.FileName);

                    
                    var metadata = DicomMetadataReader.ReadMetadata(dialog.FileName);
                    MetadataList.ItemsSource = metadata;
                }
                else if (fileType == MedicalFileType.Hdf5)
                {
                    ImageViewer.Source = Hdf5ImageReader.LoadImage(dialog.FileName);
                }
                else
                {
                    MessageBox.Show("Unsupported file format.");
                }
            }
        }




    }
}
