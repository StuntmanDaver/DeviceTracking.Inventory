using System.Collections.Generic;
using ZXing;

namespace DeviceTracking.Inventory.WinForms;

/// <summary>
/// Configuration for barcode scanning formats and settings
/// </summary>
public static class BarcodeConfiguration
{
    /// <summary>
    /// Supported barcode formats for inventory system
    /// </summary>
    public static readonly BarcodeFormat[] SupportedFormats = new[]
    {
        BarcodeFormat.CODE_128,    // Part numbers, serial numbers, batch codes
        BarcodeFormat.QR_CODE,     // Complex data with embedded information
        BarcodeFormat.CODE_39,     // Legacy part numbers
        BarcodeFormat.EAN_13,      // Commercial product codes
        BarcodeFormat.EAN_8,       // Shorter commercial codes
        BarcodeFormat.UPC_A,       // Universal Product Codes
        BarcodeFormat.UPC_E        // Compressed UPC codes
    };

    /// <summary>
    /// Get human-readable name for barcode format
    /// </summary>
    public static string GetFormatName(BarcodeFormat format)
    {
        return format switch
        {
            BarcodeFormat.CODE_128 => "Code 128",
            BarcodeFormat.QR_CODE => "QR Code",
            BarcodeFormat.CODE_39 => "Code 39",
            BarcodeFormat.EAN_13 => "EAN-13",
            BarcodeFormat.EAN_8 => "EAN-8",
            BarcodeFormat.UPC_A => "UPC-A",
            BarcodeFormat.UPC_E => "UPC-E",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Get description for barcode format usage
    /// </summary>
    public static string GetFormatDescription(BarcodeFormat format)
    {
        return format switch
        {
            BarcodeFormat.CODE_128 => "Alphanumeric codes, part numbers, serial numbers",
            BarcodeFormat.QR_CODE => "2D codes with complex data and URLs",
            BarcodeFormat.CODE_39 => "Legacy alphanumeric codes",
            BarcodeFormat.EAN_13 => "13-digit commercial product codes",
            BarcodeFormat.EAN_8 => "8-digit commercial product codes",
            BarcodeFormat.UPC_A => "12-digit Universal Product Codes",
            BarcodeFormat.UPC_E => "Compressed 6-digit UPC codes",
            _ => "Unknown barcode format"
        };
    }

    /// <summary>
    /// Check if format is supported
    /// </summary>
    public static bool IsFormatSupported(BarcodeFormat format)
    {
        return SupportedFormats.Contains(format);
    }

    /// <summary>
    /// Get all supported format names
    /// </summary>
    public static string[] GetSupportedFormatNames()
    {
        var names = new List<string>();
        foreach (var format in SupportedFormats)
        {
            names.Add(GetFormatName(format));
        }
        return names.ToArray();
    }

    /// <summary>
    /// Validate barcode text based on format
    /// </summary>
    public static bool ValidateBarcode(string barcode, BarcodeFormat format)
    {
        if (string.IsNullOrEmpty(barcode))
            return false;

        return format switch
        {
            BarcodeFormat.CODE_128 => barcode.Length >= 1 && barcode.Length <= 128,
            BarcodeFormat.QR_CODE => barcode.Length >= 1,
            BarcodeFormat.CODE_39 => barcode.Length >= 1 && barcode.Length <= 43,
            BarcodeFormat.EAN_13 => barcode.Length == 13 && IsNumeric(barcode),
            BarcodeFormat.EAN_8 => barcode.Length == 8 && IsNumeric(barcode),
            BarcodeFormat.UPC_A => barcode.Length == 12 && IsNumeric(barcode),
            BarcodeFormat.UPC_E => barcode.Length == 6 && IsNumeric(barcode),
            _ => false
        };
    }

    /// <summary>
    /// Check if string contains only numeric characters
    /// </summary>
    private static bool IsNumeric(string value)
    {
        foreach (char c in value)
        {
            if (!char.IsDigit(c))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Get recommended camera settings for barcode scanning
    /// </summary>
    public static CameraSettings GetRecommendedSettings()
    {
        return new CameraSettings
        {
            Width = 640,
            Height = 480,
            FrameRate = 30,
            FocusMode = FocusMode.Continuous,
            ExposureMode = ExposureMode.Auto,
            WhiteBalance = WhiteBalanceMode.Auto
        };
    }
}

/// <summary>
/// Camera settings for optimal barcode scanning
/// </summary>
public class CameraSettings
{
    /// <summary>
    /// Camera frame width
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Camera frame height
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Camera frame rate (FPS)
    /// </summary>
    public int FrameRate { get; set; }

    /// <summary>
    /// Camera focus mode
    /// </summary>
    public FocusMode FocusMode { get; set; }

    /// <summary>
    /// Camera exposure mode
    /// </summary>
    public ExposureMode ExposureMode { get; set; }

    /// <summary>
    /// Camera white balance mode
    /// </summary>
    public WhiteBalanceMode WhiteBalance { get; set; }
}

/// <summary>
/// Camera focus modes
/// </summary>
public enum FocusMode
{
    Auto,
    Manual,
    Continuous
}

/// <summary>
/// Camera exposure modes
/// </summary>
public enum ExposureMode
{
    Auto,
    Manual,
    Night
}

/// <summary>
/// Camera white balance modes
/// </summary>
public enum WhiteBalanceMode
{
    Auto,
    Daylight,
    Cloudy,
    Tungsten,
    Fluorescent
}
