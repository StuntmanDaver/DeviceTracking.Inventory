using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Application.Services;
using DeviceTracking.Inventory.Shared.DTOs;
using DeviceTracking.Inventory.WinForms.Presenters.Base;

namespace DeviceTracking.Inventory.WinForms.Presenters.Inventory;

/// <summary>
/// Presenter for inventory item management
/// </summary>
public class InventoryItemPresenter : BasePresenter<IInventoryItemView>
{
    private readonly IInventoryItemService _inventoryItemService;
    private readonly ILocationService _locationService;

    private InventoryItemDto? _currentItem;
    private bool _isDirty;

    public InventoryItemPresenter(
        IInventoryItemService inventoryItemService,
        ILocationService locationService)
    {
        _inventoryItemService = inventoryItemService ?? throw new ArgumentNullException(nameof(inventoryItemService));
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
    }

    public override async Task InitializeAsync(IInventoryItemView view)
    {
        await base.InitializeAsync(view);

        // Subscribe to view events
        View.LoadDataRequested += OnLoadDataRequested;
        View.NewItemRequested += OnNewItemRequested;
        View.EditItemRequested += OnEditItemRequested;
        View.DeleteItemRequested += OnDeleteItemRequested;
        View.SaveItemRequested += OnSaveItemRequested;
        View.CancelEditRequested += OnCancelEditRequested;
        View.RefreshRequested += OnRefreshRequested;
        View.SearchTextChanged += OnSearchTextChanged;
        View.FilterChanged += OnFilterChanged;
        View.ItemSelectionChanged += OnItemSelectionChanged;
        View.ScanBarcodeRequested += OnScanBarcodeRequested;
        View.UpdateStockRequested += OnUpdateStockRequested;
        View.LowStockAlertsRequested += OnLowStockAlertsRequested;

        // Load initial data
        await LoadReferenceDataAsync();
        await LoadInventoryItemsAsync();
    }

    public override void Cleanup()
    {
        // Unsubscribe from view events
        if (View != null)
        {
            View.LoadDataRequested -= OnLoadDataRequested;
            View.NewItemRequested -= OnNewItemRequested;
            View.EditItemRequested -= OnEditItemRequested;
            View.DeleteItemRequested -= OnDeleteItemRequested;
            View.SaveItemRequested -= OnSaveItemRequested;
            View.CancelEditRequested -= OnCancelEditRequested;
            View.RefreshRequested -= OnRefreshRequested;
            View.SearchTextChanged -= OnSearchTextChanged;
            View.FilterChanged -= OnFilterChanged;
            View.ItemSelectionChanged -= OnItemSelectionChanged;
            View.ScanBarcodeRequested -= OnScanBarcodeRequested;
            View.UpdateStockRequested -= OnUpdateStockRequested;
            View.LowStockAlertsRequested -= OnLowStockAlertsRequested;
        }

        base.Cleanup();
    }

    private async void OnLoadDataRequested(object? sender, EventArgs e)
    {
        await ExecuteSafelyAsync(async () =>
        {
            await LoadReferenceDataAsync();
            await LoadInventoryItemsAsync();
        }, "Failed to load data");
    }

    private async void OnNewItemRequested(object? sender, EventArgs e)
    {
        await ExecuteSafelyAsync(async () =>
        {
            _currentItem = null;
            _isDirty = false;

            // Clear the form
            ClearForm();

            // Set view to new mode
            View.SetOperationMode(OperationMode.New);
            View.SetFormReadOnly(false);
            View.SetDataEntryMode(true);

            // Focus on first field
            View.FocusField("PartNumber");

            View.ShowInformation("Enter details for the new inventory item");
        }, "Failed to start new item creation");
    }

    private async void OnEditItemRequested(object? sender, EventArgs e)
    {
        await ExecuteSafelyAsync(async () =>
        {
            if (View.SelectedItemId == null)
            {
                View.ShowError("Please select an item to edit");
                return;
            }

            var result = await _inventoryItemService.GetByIdAsync(View.SelectedItemId.Value);
            if (!result.IsSuccess || result.Data == null)
            {
                View.ShowError(result.Error ?? "Failed to load item for editing");
                return;
            }

            _currentItem = result.Data;
            _isDirty = false;

            // Load item data into form
            LoadItemIntoForm(_currentItem);

            // Set view to edit mode
            View.SetOperationMode(OperationMode.Edit);
            View.SetFormReadOnly(false);
            View.SetDataEntryMode(true);

            View.ShowInformation("Edit the item details and click Save");
        }, "Failed to start item editing");
    }

    private async void OnDeleteItemRequested(object? sender, EventArgs e)
    {
        await ExecuteSafelyAsync(async () =>
        {
            if (View.SelectedItemId == null)
            {
                View.ShowError("Please select an item to delete");
                return;
            }

            var item = View.SelectedItem;
            if (item == null)
            {
                View.ShowError("Selected item not found");
                return;
            }

            var confirmed = View.ShowConfirmation(
                $"Are you sure you want to delete '{item.PartNumber}'?",
                "Confirm Delete");

            if (!confirmed)
            {
                return;
            }

            var result = await _inventoryItemService.DeleteAsync(View.SelectedItemId.Value);
            if (result.IsSuccess)
            {
                View.ShowSuccess("Item deleted successfully");
                await LoadInventoryItemsAsync();
                ClearForm();
            }
            else
            {
                View.ShowError(result.Error ?? "Failed to delete item");
            }
        }, "Failed to delete item");
    }

    private async void OnSaveItemRequested(object? sender, EventArgs e)
    {
        await ExecuteSafelyAsync(async () =>
        {
            // Validate form
            if (!View.ValidateForm())
            {
                View.ShowError("Please correct the validation errors before saving");
                return;
            }

            if (View.GetOperationMode() == OperationMode.New)
            {
                await SaveNewItemAsync();
            }
            else if (View.GetOperationMode() == OperationMode.Edit)
            {
                await SaveEditedItemAsync();
            }
        }, "Failed to save item");
    }

    private async void OnCancelEditRequested(object? sender, EventArgs e)
    {
        await ExecuteSafelyAsync(async () =>
        {
            if (_isDirty)
            {
                var confirmed = View.ShowConfirmation(
                    "You have unsaved changes. Are you sure you want to cancel?",
                    "Confirm Cancel");

                if (!confirmed)
                {
                    return;
                }
            }

            // Reset form
            if (_currentItem != null)
            {
                LoadItemIntoForm(_currentItem);
            }
            else
            {
                ClearForm();
            }

            View.SetOperationMode(OperationMode.View);
            View.SetFormReadOnly(true);
            View.SetDataEntryMode(false);
            _isDirty = false;

            View.ShowInformation("Changes cancelled");
        }, "Failed to cancel edit");
    }

    private async void OnRefreshRequested(object? sender, EventArgs e)
    {
        await ExecuteSafelyAsync(async () =>
        {
            await LoadInventoryItemsAsync();
            View.ShowInformation("Data refreshed");
        }, "Failed to refresh data");
    }

    private async void OnSearchTextChanged(object? sender, EventArgs e)
    {
        await ExecuteSafelyAsync(async () =>
        {
            await LoadInventoryItemsAsync();
        }, "Failed to filter data");
    }

    private async void OnFilterChanged(object? sender, EventArgs e)
    {
        await ExecuteSafelyAsync(async () =>
        {
            await LoadInventoryItemsAsync();
        }, "Failed to apply filter");
    }

    private async void OnItemSelectionChanged(object? sender, EventArgs e)
    {
        await ExecuteSafelyAsync(async () =>
        {
            if (View.SelectedItemId != null)
            {
                var result = await _inventoryItemService.GetByIdAsync(View.SelectedItemId.Value);
                if (result.IsSuccess && result.Data != null)
                {
                    _currentItem = result.Data;
                    LoadItemIntoForm(_currentItem);
                    View.SetOperationMode(OperationMode.View);
                    View.SetFormReadOnly(true);
                    View.SetDataEntryMode(false);
                }
            }
        }, "Failed to load selected item");
    }

    private async void OnScanBarcodeRequested(object? sender, EventArgs e)
    {
        await ExecuteSafelyAsync(async () =>
        {
            View.ShowBarcodeScanner();

            // The barcode scanner will handle the scanning and call back
            // The result will be set in the Barcode property of the view
            // For now, we'll wait and check if a barcode was set

            // In a real implementation, the barcode scanner form would be modal
            // and would return the scanned barcode directly
        }, "Failed to scan barcode");
    }

    private async void OnUpdateStockRequested(object? sender, EventArgs e)
    {
        await ExecuteSafelyAsync(async () =>
        {
            if (View.SelectedItemId == null)
            {
                View.ShowError("Please select an item to update stock");
                return;
            }

            // This would typically show a dialog for stock adjustment
            // For now, we'll simulate a stock increase
            var quantityChange = 10; // This would come from user input
            var reason = "Manual adjustment"; // This would come from user input

            var result = await _inventoryItemService.UpdateStockAsync(
                View.SelectedItemId.Value, quantityChange, reason);

            if (result.IsSuccess)
            {
                View.ShowSuccess($"Stock updated by {quantityChange}");
                await LoadInventoryItemsAsync();
                if (_currentItem != null)
                {
                    var updatedResult = await _inventoryItemService.GetByIdAsync(_currentItem.Id);
                    if (updatedResult.IsSuccess && updatedResult.Data != null)
                    {
                        _currentItem = updatedResult.Data;
                        LoadItemIntoForm(_currentItem);
                    }
                }
            }
            else
            {
                View.ShowError(result.Error ?? "Failed to update stock");
            }
        }, "Failed to update stock");
    }

    private async void OnLowStockAlertsRequested(object? sender, EventArgs e)
    {
        await ExecuteSafelyAsync(async () =>
        {
            var result = await _inventoryItemService.GetLowStockAlertsAsync();
            if (result.IsSuccess)
            {
                View.LowStockAlerts = result.Data ?? new List<LowStockAlertDto>();
                View.ShowLowStockWarning();
            }
            else
            {
                View.ShowError(result.Error ?? "Failed to load low stock alerts");
            }
        }, "Failed to load low stock alerts");
    }

    private async Task LoadReferenceDataAsync()
    {
        // Load locations
        var locationsResult = await _locationService.GetPagedAsync(new LocationQueryDto { PageSize = 1000 });
        if (locationsResult.IsSuccess)
        {
            View.Locations = locationsResult.Data?.Items ?? new List<LocationDto>();
        }

        // Suppliers will be loaded when supplier service is implemented
        View.Suppliers = new List<SupplierSummaryDto>();
    }

    private async Task LoadInventoryItemsAsync()
    {
        var query = new InventoryItemQueryDto
        {
            SearchTerm = View.SearchText,
            Category = View.SelectedCategory,
            LocationId = View.SelectedLocationFilter,
            SupplierId = View.SelectedSupplierFilter,
            IncludeInactive = View.ShowInactiveItems,
            PageSize = 100 // Load more items for the grid
        };

        var result = await _inventoryItemService.GetPagedAsync(query);
        if (result.IsSuccess)
        {
            View.InventoryItems = result.Data?.Items ?? new List<InventoryItemSummaryDto>();
            View.RefreshItemList();
        }
        else
        {
            View.ShowError(result.Error ?? "Failed to load inventory items");
        }
    }

    private async Task SaveNewItemAsync()
    {
        var dto = CreateDtoFromForm();

        var result = await _inventoryItemService.CreateAsync(dto);
        if (result.IsSuccess)
        {
            View.ShowSuccess("Item created successfully");
            await LoadInventoryItemsAsync();
            _currentItem = result.Data;
            View.SetOperationMode(OperationMode.View);
            View.SetFormReadOnly(true);
            View.SetDataEntryMode(false);
            _isDirty = false;
        }
        else
        {
            View.ShowError(result.Error ?? "Failed to create item");
        }
    }

    private async Task SaveEditedItemAsync()
    {
        if (_currentItem == null)
        {
            View.ShowError("No item selected for editing");
            return;
        }

        var dto = CreateDtoFromForm();

        var result = await _inventoryItemService.UpdateAsync(_currentItem.Id, dto);
        if (result.IsSuccess)
        {
            View.ShowSuccess("Item updated successfully");
            await LoadInventoryItemsAsync();
            _currentItem = result.Data;
            View.SetOperationMode(OperationMode.View);
            View.SetFormReadOnly(true);
            View.SetDataEntryMode(false);
            _isDirty = false;
        }
        else
        {
            View.ShowError(result.Error ?? "Failed to update item");
        }
    }

    private CreateInventoryItemDto CreateDtoFromForm()
    {
        return new CreateInventoryItemDto
        {
            PartNumber = View.PartNumber,
            Description = View.Description,
            Barcode = View.Barcode,
            Category = View.Category,
            SubCategory = View.SubCategory,
            UnitOfMeasure = View.UnitOfMeasure,
            CurrentStock = View.CurrentStock,
            MinimumStock = View.MinimumStock,
            MaximumStock = View.MaximumStock,
            StandardCost = View.StandardCost,
            SellingPrice = View.SellingPrice,
            LocationId = View.LocationId ?? Guid.Empty,
            SupplierId = View.SupplierId,
            IsActive = View.IsActive,
            Notes = View.Notes
        };
    }

    private UpdateInventoryItemDto CreateUpdateDtoFromForm()
    {
        return new UpdateInventoryItemDto
        {
            Description = View.Description,
            Category = View.Category,
            SubCategory = View.SubCategory,
            UnitOfMeasure = View.UnitOfMeasure,
            MinimumStock = View.MinimumStock,
            MaximumStock = View.MaximumStock,
            StandardCost = View.StandardCost,
            SellingPrice = View.SellingPrice,
            LocationId = View.LocationId,
            SupplierId = View.SupplierId,
            IsActive = View.IsActive,
            Notes = View.Notes
        };
    }

    private void LoadItemIntoForm(InventoryItemDto item)
    {
        View.PartNumber = item.PartNumber;
        View.Description = item.Description;
        View.Barcode = item.Barcode;
        View.Category = item.Category ?? string.Empty;
        View.SubCategory = item.SubCategory ?? string.Empty;
        View.UnitOfMeasure = item.UnitOfMeasure;
        View.CurrentStock = item.CurrentStock;
        View.ReservedStock = item.ReservedStock;
        View.MinimumStock = item.MinimumStock;
        View.MaximumStock = item.MaximumStock;
        View.StandardCost = item.StandardCost;
        View.SellingPrice = item.SellingPrice;
        View.LocationId = item.Location?.Id;
        View.SupplierId = item.Supplier?.Id;
        View.Notes = item.Notes ?? string.Empty;
        View.IsActive = item.IsActive;
        View.LastMovement = item.LastMovement;

        View.ClearFieldErrors();
        View.UpdateItemDetails();
    }

    private void ClearForm()
    {
        View.PartNumber = string.Empty;
        View.Description = string.Empty;
        View.Barcode = string.Empty;
        View.Category = string.Empty;
        View.SubCategory = string.Empty;
        View.UnitOfMeasure = "Each";
        View.CurrentStock = 0;
        View.ReservedStock = 0;
        View.MinimumStock = 0;
        View.MaximumStock = 0;
        View.StandardCost = 0;
        View.SellingPrice = 0;
        View.LocationId = null;
        View.SupplierId = null;
        View.Notes = string.Empty;
        View.IsActive = true;
        View.LastMovement = null;

        View.ClearFieldErrors();
    }
}
