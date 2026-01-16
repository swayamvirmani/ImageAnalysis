# Medical Image Viewer (DICOM & HDF5)

This project is a C# WPF application designed to read and display medical images stored in **DICOM** and **HDF5** file formats.  
It focuses on understanding file structures, extracting image data, and visualizing it correctly rather than treating files as standard image formats.

---

## Features

- Detects file type using binary signatures (not file extensions)
- Displays medical images from **DICOM** files with proper windowing
- Displays images reconstructed from **HDF5 numerical datasets**
- Extracts and displays key **DICOM metadata** such as:
  - Patient Name
  - Patient ID
  - Study Date
  - Modality
  - Slice Thickness
  - Pixel Spacing
- Clean and simple WPF-based user interface

---

## Supported File Formats

### DICOM
- Reads uncompressed DICOM files
- Applies window width and window center for correct visualization
- Extracts metadata using standard DICOM tags

### HDF5

- Normalizes data values to grayscale
- Reconstructs and displays the image in the viewer

---

## Technologies Used

- **C#**
- **WPF (.NET)**
- **fo-dicom** – for DICOM file parsing and rendering
- **HDF.PInvoke.NETStandard** – for reading HDF5 files
- **Visual Studio**

---

## Project Structure

- `Helpers`
  - File type detection logic
- `Readers`
  - DICOM image reader
  - HDF5 image reader
  - DICOM metadata reader
- `MainWindow`
  - Handles UI interaction and workflow

The project follows separation of concerns to keep detection, reading, and UI logic independent.

---

## How It Works

1. User uploads a file using the UI
2. The application detects whether the file is DICOM or HDF5
3. Based on the file type:
   - DICOM images are rendered and metadata is extracted
   - HDF5 datasets are normalized and converted into images
4. The image is displayed in the viewer
5. Metadata is shown for DICOM files

---



