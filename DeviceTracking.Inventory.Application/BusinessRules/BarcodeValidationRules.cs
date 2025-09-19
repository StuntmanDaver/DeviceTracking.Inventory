using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Application.Common;
using DeviceTracking.Inventory.Application.Repositories;

namespace DeviceTracking.Inventory.Application.BusinessRules;

/// <summary>
/// Business rules for barcode validation and uniqueness
/// </summary>
public class BarcodeValidationRules
{
    private readonly IInventoryItemRepository _inventoryItemRepository;

    /// <summary>
    /// Constructor
    /// </summary>
    public BarcodeValidationRules(IInventoryItemRepository inventoryItemRepository)
    {
        _inventoryItemRepository = inventoryItemRepository ?? throw new ArgumentNullException(nameof(inventoryItemRepository));
    }

    /// <summary>
    /// Supported barcode formats and their validation patterns
    /// </summary>
    private static readonly Dictionary<string, BarcodeFormatInfo> _barcodeFormats = new()
    {
        ["CODE_128"] = new BarcodeFormatInfo
        {
            Name = "Code 128",
            MinLength = 1,
            MaxLength = 128,
            Pattern = @"^[A-Za-z0-9\-\.\s/]+$",
            CheckDigitRequired = false,
            Description = "Alphanumeric code with full ASCII character set"
        },
        ["QR_CODE"] = new BarcodeFormatInfo
        {
            Name = "QR Code",
            MinLength = 1,
            MaxLength = 2048,
            Pattern = null, // QR codes can contain any data
            CheckDigitRequired = false,
            Description = "2D barcode that can store various types of data"
        },
        ["CODE_39"] = new BarcodeFormatInfo
        {
            Name = "Code 39",
            MinLength = 1,
            MaxLength = 43,
            Pattern = @"^[A-Z0-9\-\.\s/]+$",
            CheckDigitRequired = false,
            Description = "Alphanumeric code (uppercase only)"
        },
        ["EAN_13"] = new BarcodeFormatInfo
        {
            Name = "EAN-13",
            MinLength = 13,
            MaxLength = 13,
            Pattern = @"^\d{13}$",
            CheckDigitRequired = true,
            Description = "13-digit commercial product code with check digit"
        },
        ["EAN_8"] = new BarcodeFormatInfo
        {
            Name = "EAN-8",
            MinLength = 8,
            MaxLength = 8,
            Pattern = @"^\d{8}$",
            CheckDigitRequired = true,
            Description = "8-digit commercial product code with check digit"
        },
        ["UPC_A"] = new BarcodeFormatInfo
        {
            Name = "UPC-A",
            MinLength = 12,
            MaxLength = 12,
            Pattern = @"^\d{12}$",
            CheckDigitRequired = true,
            Description = "12-digit Universal Product Code with check digit"
        },
        ["UPC_E"] = new BarcodeFormatInfo
        {
            Name = "UPC-E",
            MinLength = 6,
            MaxLength = 8,
            Pattern = @"^\d{6,8}$",
            CheckDigitRequired = true,
            Description = "Compressed 6-8 digit UPC code"
        }
    };

    /// <summary>
    /// Validate barcode format and structure
    /// </summary>
    public ServiceResult ValidateBarcodeFormat(string barcode, string format = "AUTO")
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return ServiceResult.Failure("Barcode cannot be empty");
        }

        if (format == "AUTO")
        {
            // Try to auto-detect format
            foreach (var formatInfo in _barcodeFormats.Values)
            {
                if (IsValidForFormat(barcode, formatInfo))
                {
                    return ServiceResult.Success($"Valid {formatInfo.Name} barcode");
                }
            }
            return ServiceResult.Failure("Barcode format not recognized");
        }

        if (!_barcodeFormats.TryGetValue(format.ToUpper(), out var formatInfo))
        {
            return ServiceResult.Failure($"Unsupported barcode format: {format}");
        }

        if (!IsValidForFormat(barcode, formatInfo))
        {
            return ServiceResult.Failure($"Invalid {formatInfo.Name} barcode format");
        }

        return ServiceResult.Success($"Valid {formatInfo.Name} barcode");
    }

    /// <summary>
    /// Check if barcode is valid for a specific format
    /// </summary>
    private bool IsValidForFormat(string barcode, BarcodeFormatInfo formatInfo)
    {
        // Check length
        if (barcode.Length < formatInfo.MinLength || barcode.Length > formatInfo.MaxLength)
        {
            return false;
        }

        // Check pattern
        if (formatInfo.Pattern != null && !Regex.IsMatch(barcode, formatInfo.Pattern))
        {
            return false;
        }

        // Check digit validation for formats that require it
        if (formatInfo.CheckDigitRequired)
        {
            return ValidateCheckDigit(barcode, formatInfo.Name);
        }

        return true;
    }

    /// <summary>
    /// Validate check digit for barcode formats that require it
    /// </summary>
    private bool ValidateCheckDigit(string barcode, string formatName)
    {
        switch (formatName)
        {
            case "EAN-13":
                return ValidateEan13CheckDigit(barcode);
            case "EAN-8":
                return ValidateEan8CheckDigit(barcode);
            case "UPC-A":
                return ValidateUpcCheckDigit(barcode);
            case "UPC-E":
                return ValidateUpcECheckDigit(barcode);
            default:
                return true; // No validation available
        }
    }

    /// <summary>
    /// Validate EAN-13 check digit
    /// </summary>
    private bool ValidateEan13CheckDigit(string barcode)
    {
        if (barcode.Length != 13) return false;

        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = barcode[i] - '0';
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == (barcode[12] - '0');
    }

    /// <summary>
    /// Validate EAN-8 check digit
    /// </summary>
    private bool ValidateEan8CheckDigit(string barcode)
    {
        if (barcode.Length != 8) return false;

        int sum = 0;
        for (int i = 0; i < 7; i++)
        {
            int digit = barcode[i] - '0';
            sum += (i % 2 == 0) ? digit * 3 : digit;
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == (barcode[7] - '0');
    }

    /// <summary>
    /// Validate UPC-A check digit
    /// </summary>
    private bool ValidateUpcCheckDigit(string barcode)
    {
        if (barcode.Length != 12) return false;

        int sum = 0;
        for (int i = 0; i < 11; i++)
        {
            int digit = barcode[i] - '0';
            sum += (i % 2 == 0) ? digit * 3 : digit;
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == (barcode[11] - '0');
    }

    /// <summary>
    /// Validate UPC-E check digit (simplified)
    /// </summary>
    private bool ValidateUpcECheckDigit(string barcode)
    {
        // UPC-E validation is more complex, simplified for now
        return barcode.All(char.IsDigit);
    }

    /// <summary>
    /// Validate barcode uniqueness in the system
    /// </summary>
    public async Task<ServiceResult> ValidateBarcodeUniquenessAsync(string barcode, Guid? excludeItemId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return ServiceResult.Failure("Barcode cannot be empty");
        }

        var existingItem = await _inventoryItemRepository.GetByBarcodeAsync(barcode, cancellationToken);
        if (existingItem != null && existingItem.Id != excludeItemId)
        {
            return ServiceResult.Failure($"Barcode '{barcode}' is already assigned to item '{existingItem.PartNumber}'");
        }

        return ServiceResult.Success("Barcode is available");
    }

    /// <summary>
    /// Generate a suggested barcode for a part number
    /// </summary>
    public ServiceResult<string> GenerateSuggestedBarcode(string partNumber, string format = "CODE_128")
    {
        if (string.IsNullOrWhiteSpace(partNumber))
        {
            return ServiceResult<string>.Failure("Part number cannot be empty");
        }

        string suggestedBarcode;

        // Clean and format the part number
        var cleanPartNumber = Regex.Replace(partNumber.ToUpper(), @"[^A-Z0-9]", "");

        switch (format.ToUpper())
        {
            case "CODE_128":
                // Use cleaned part number directly
                suggestedBarcode = cleanPartNumber;
                break;

            case "EAN_13":
                // Generate a basic EAN-13 style code
                var baseCode = cleanPartNumber.PadRight(12, '0').Substring(0, 12);
                suggestedBarcode = GenerateEan13WithCheckDigit(baseCode);
                break;

            default:
                suggestedBarcode = cleanPartNumber;
                break;
        }

        return ServiceResult<string>.Success(suggestedBarcode, $"Suggested barcode: {suggestedBarcode}");
    }

    /// <summary>
    /// Generate EAN-13 with check digit
    /// </summary>
    private string GenerateEan13WithCheckDigit(string baseCode)
    {
        if (baseCode.Length != 12) return baseCode;

        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = baseCode[i] - '0';
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return baseCode + checkDigit.ToString();
    }

    /// <summary>
    /// Get information about supported barcode formats
    /// </summary>
    public IEnumerable<BarcodeFormatInfo> GetSupportedFormats()
    {
        return _barcodeFormats.Values;
    }

    /// <summary>
    /// Detect the most likely barcode format for a given barcode
    /// </summary>
    public ServiceResult<string> DetectBarcodeFormat(string barcode)
    {
        foreach (var kvp in _barcodeFormats)
        {
            if (IsValidForFormat(barcode, kvp.Value))
            {
                return ServiceResult<string>.Success(kvp.Key, $"Detected format: {kvp.Value.Name}");
            }
        }

        return ServiceResult<string>.Failure("Unable to detect barcode format");
    }

    /// <summary>
    /// Validate barcode scanning quality
    /// </summary>
    public ServiceResult ValidateScanningQuality(int confidence, int length)
    {
        if (confidence < 50)
        {
            return ServiceResult.Failure("Low confidence scan - please try again");
        }

        if (length < 3)
        {
            return ServiceResult.Failure("Barcode too short - may be a scanning error");
        }

        if (length > 2048)
        {
            return ServiceResult.Failure("Barcode too long - may be a scanning error");
        }

        return ServiceResult.Success("Barcode scan quality is acceptable");
    }
}

/// <summary>
/// Information about a barcode format
/// </summary>
public class BarcodeFormatInfo
{
    /// <summary>
    /// Format name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Minimum length
    /// </summary>
    public int MinLength { get; set; }

    /// <summary>
    /// Maximum length
    /// </summary>
    public int MaxLength { get; set; }

    /// <summary>
    /// Validation pattern (regex)
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// Whether check digit is required
    /// </summary>
    public bool CheckDigitRequired { get; set; }

    /// <summary>
    /// Description of the format
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
