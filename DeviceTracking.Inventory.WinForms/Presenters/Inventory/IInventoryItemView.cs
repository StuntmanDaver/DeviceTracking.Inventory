using System;
using System.Collections.Generic;
using DeviceTracking.Inventory.Shared.DTOs;
using DeviceTracking.Inventory.WinForms.Presenters.Base;

namespace DeviceTracking.Inventory.WinForms.Presenters.Inventory;

/// <summary>
/// Interface for inventory item management view
/// </summary>
public interface IInventoryItemView : ICrudView
{
    // Data display properties
    string PartNumber { get; set; }
    string Description { get; set; }
    string Barcode { get; set; }
    string Category { get; set; }
    string SubCategory { get; set; }
    string UnitOfMeasure { get; set; }
    int CurrentStock { get; set; }
    int ReservedStock { get; set; }
    int MinimumStock { get; set; }
    int MaximumStock { get; set; }
    decimal StandardCost { get; set; }
    decimal SellingPrice { get; set; }
    Guid? LocationId { get; set; }
    Guid? SupplierId { get; set; }
    string Notes { get; set; }
    bool IsActive { get; set; }

    // Read-only computed properties
    int AvailableStock { get; }
    decimal TotalValue { get; }
    string StockStatus { get; }
    DateTime? LastMovement { get; set; }

    // Data binding
    IEnumerable<InventoryItemSummaryDto> InventoryItems { get; set; }
    IEnumerable<LocationSummaryDto> Locations { get; set; }
    IEnumerable<SupplierSummaryDto> Suppliers { get; set; }
    IEnumerable<LowStockAlertDto> LowStockAlerts { get; set; }

    // Selection
    InventoryItemSummaryDto? SelectedItem { get; set; }
    Guid? SelectedItemId { get; }

    // Filtering and search
    string SearchText { get; set; }
    string SelectedCategory { get; set; }
    Guid? SelectedLocationFilter { get; set; }
    Guid? SelectedSupplierFilter { get; set; }
    bool ShowInactiveItems { get; set; }

    // Events
    event EventHandler? LoadDataRequested;
    event EventHandler? NewItemRequested;
    event EventHandler? EditItemRequested;
    event EventHandler? DeleteItemRequested;
    event EventHandler? SaveItemRequested;
    event EventHandler? CancelEditRequested;
    event EventHandler? RefreshRequested;
    event EventHandler? SearchTextChanged;
    event EventHandler? FilterChanged;
    event EventHandler? ItemSelectionChanged;
    event EventHandler? ScanBarcodeRequested;
    event EventHandler? UpdateStockRequested;
    event EventHandler? LowStockAlertsRequested;

    // Validation
    void SetFieldError(string fieldName, string errorMessage);
    void ClearFieldErrors();
    bool ValidateForm();

    // UI state management
    void SetFormReadOnly(bool readOnly);
    void SetDataEntryMode(bool isDataEntry);
    void FocusField(string fieldName);
    void HighlightChangedFields();

    // Additional UI features
    void ShowBarcodeScanner();
    void UpdateStockDisplay();
    void ShowLowStockWarning();
    void RefreshItemList();
    void UpdateItemDetails();

    // Operation mode
    OperationMode GetOperationMode();
}
