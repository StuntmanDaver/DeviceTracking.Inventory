using System;
using System.Drawing;
using System.Threading.Tasks;
using OpenCvSharp;
using ZXing;
using ZXing.Common;

namespace DeviceTracking.Inventory.WinForms;

/// <summary>
/// Camera device wrapper for barcode scanning integration
/// </summary>
public class CameraDeviceWrapper : IDisposable
{
    private VideoCapture? _capture;
    private BarcodeReader? _barcodeReader;
    private bool _isInitialized;

    /// <summary>
    /// Supported barcode formats
    /// </summary>
    private readonly BarcodeFormat[] _supportedFormats = new[]
    {
        BarcodeFormat.CODE_128,
        BarcodeFormat.QR_CODE,
        BarcodeFormat.CODE_39,
        BarcodeFormat.EAN_13,
        BarcodeFormat.EAN_8,
        BarcodeFormat.UPC_A,
        BarcodeFormat.UPC_E
    };

    /// <summary>
    /// Constructor
    /// </summary>
    public CameraDeviceWrapper()
    {
        Initialize();
    }

    /// <summary>
    /// Initialize camera and barcode reader
    /// </summary>
    private void Initialize()
    {
        try
        {
            // Initialize video capture (camera)
            _capture = new VideoCapture(0); // Use default camera

            if (!_capture.IsOpened())
            {
                throw new InvalidOperationException("Unable to open camera device");
            }

            // Set camera properties for optimal scanning
            _capture.Set(VideoCaptureProperties.FrameWidth, 640);
            _capture.Set(VideoCaptureProperties.FrameHeight, 480);
            _capture.Set(VideoCaptureProperties.Fps, 30);

            // Initialize barcode reader with supported formats
            _barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                TryInverted = true,
                Options = new DecodingOptions
                {
                    PossibleFormats = _supportedFormats,
                    TryHarder = true,
                    PureBarcode = false,
                    ReturnCodabarStartEnd = false
                }
            };

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize camera device", ex);
        }
    }

    /// <summary>
    /// Check if camera is available and initialized
    /// </summary>
    public bool IsAvailable => _isInitialized && _capture?.IsOpened() == true;

    /// <summary>
    /// Capture a single frame and scan for barcodes
    /// </summary>
    public async Task<BarcodeResult?> ScanBarcodeAsync()
    {
        if (!IsAvailable || _capture == null || _barcodeReader == null)
        {
            throw new InvalidOperationException("Camera device is not available");
        }

        try
        {
            using var frame = new Mat();
            _capture.Read(frame);

            if (frame.Empty())
            {
                return null;
            }

            // Convert OpenCV Mat to Bitmap for ZXing
            using var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);

            // Scan for barcodes
            var result = _barcodeReader.Decode(bitmap);

            if (result != null)
            {
                return new BarcodeResult
                {
                    Text = result.Text,
                    Format = result.BarcodeFormat,
                    Timestamp = DateTime.UtcNow,
                    Confidence = CalculateConfidence(result)
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to scan barcode", ex);
        }
    }

    /// <summary>
    /// Capture multiple frames and scan for barcodes with retry logic
    /// </summary>
    public async Task<BarcodeResult?> ScanBarcodeWithRetryAsync(int maxRetries = 5, int delayMs = 200)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            var result = await ScanBarcodeAsync();
            if (result != null)
            {
                return result;
            }

            if (i < maxRetries - 1)
            {
                await Task.Delay(delayMs);
            }
        }

        return null;
    }

    /// <summary>
    /// Calculate confidence score for barcode result
    /// </summary>
    private int CalculateConfidence(Result result)
    {
        // Simple confidence calculation based on format reliability
        return result.BarcodeFormat switch
        {
            BarcodeFormat.QR_CODE => 95,
            BarcodeFormat.CODE_128 => 90,
            BarcodeFormat.EAN_13 => 85,
            BarcodeFormat.CODE_39 => 80,
            _ => 75
        };
    }

    /// <summary>
    /// Get list of available cameras
    /// </summary>
    public static string[] GetAvailableCameras()
    {
        var cameras = new System.Collections.Generic.List<string>();

        for (int i = 0; i < 10; i++) // Check first 10 camera indices
        {
            try
            {
                using var testCapture = new VideoCapture(i);
                if (testCapture.IsOpened())
                {
                    cameras.Add($"Camera {i}");
                }
            }
            catch
            {
                // Camera not available, continue
            }
        }

        return cameras.ToArray();
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        _capture?.Dispose();
        _barcodeReader = null;
        _isInitialized = false;
    }
}

/// <summary>
/// Barcode scan result
/// </summary>
public class BarcodeResult
{
    /// <summary>
    /// The decoded barcode text
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The barcode format
    /// </summary>
    public BarcodeFormat Format { get; set; }

    /// <summary>
    /// Timestamp of the scan
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Confidence score (0-100)
    /// </summary>
    public int Confidence { get; set; }

    /// <summary>
    /// Check if the result is valid
    /// </summary>
    public bool IsValid => !string.IsNullOrEmpty(Text) && Confidence > 0;
}
