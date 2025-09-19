using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeviceTracking.Inventory.Shared.DTOs;
using DeviceTracking.Inventory.WinForms.Presenters.Base;
using DeviceTracking.Inventory.WinForms.Presenters.Inventory;
using DeviceTracking.Inventory.WinForms.Forms;

namespace DeviceTracking.Inventory.WinForms.Forms;

/// <summary>
/// Inventory Items Management Form - Implements MVP Pattern
/// </summary>
public partial class frmInventoryItems : Form, IInventoryItemView
{
    private InventoryItemPresenter? _presenter;
    private OperationMode _currentMode = OperationMode.View;

    public frmInventoryItems()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    #region IView Implementation

    public void Show()
    {
        this.Show();
    }

    public void Hide()
    {
        this.Hide();
    }

    public void Close()
    {
        this.Close();
    }

    public event EventHandler? ViewLoaded;
    public event EventHandler? ViewClosing;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ViewLoaded?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        ViewClosing?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region IDataView Implementation

    public void SetLoadingState(bool isLoading)
    {
        UseWaitCursor = isLoading;
        foreach (Control control in Controls)
        {
            control.Enabled = !isLoading;
        }
    }

    public void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public void ShowSuccess(string message)
    {
        MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void ShowInformation(string message)
    {
        MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public bool ShowConfirmation(string message, string title = "Confirm")
    {
        return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }

    #endregion

    #region IRefreshableView Implementation

    public void RefreshData()
    {
        OnRefreshRequested();
    }

    public void ClearData()
    {
        ClearForm();
    }

    #endregion

    #region ICrudView Implementation

    public void SetNewEnabled(bool enabled)
    {
        btnNew.Enabled = enabled;
    }

    public void SetEditEnabled(bool enabled)
    {
        btnEdit.Enabled = enabled;
    }

    public void SetDeleteEnabled(bool enabled)
    {
        btnDelete.Enabled = enabled;
    }

    public void SetSaveEnabled(bool enabled)
    {
        btnSave.Enabled = enabled;
    }

    public void SetCancelEnabled(bool enabled)
    {
        btnCancel.Enabled = enabled;
    }

    public void SetOperationMode(OperationMode mode)
    {
        _currentMode = mode;

        // Update UI based on mode
        switch (mode)
        {
            case OperationMode.View:
                SetViewMode();
                break;
            case OperationMode.New:
                SetNewMode();
                break;
            case OperationMode.Edit:
                SetEditMode();
                break;
        }
    }

    #endregion

    #region IInventoryItemView Implementation

    // Data properties
    public string PartNumber
    {
        get => txtPartNumber.Text;
        set => txtPartNumber.Text = value;
    }

    public string Description
    {
        get => txtDescription.Text;
        set => txtDescription.Text = value;
    }

    public string Barcode
    {
        get => txtBarcode.Text;
        set => txtBarcode.Text = value;
    }

    public string Category
    {
        get => txtCategory.Text;
        set => txtCategory.Text = value;
    }

    public string SubCategory
    {
        get => txtSubCategory.Text;
        set => txtSubCategory.Text = value;
    }

    public string UnitOfMeasure
    {
        get => cmbUnitOfMeasure.Text;
        set => cmbUnitOfMeasure.Text = value;
    }

    public int CurrentStock
    {
        get => int.TryParse(txtCurrentStock.Text, out var value) ? value : 0;
        set => txtCurrentStock.Text = value.ToString();
    }

    public int ReservedStock
    {
        get => int.TryParse(txtReservedStock.Text, out var value) ? value : 0;
        set => txtReservedStock.Text = value.ToString();
    }

    public int MinimumStock
    {
        get => int.TryParse(txtMinimumStock.Text, out var value) ? value : 0;
        set => txtMinimumStock.Text = value.ToString();
    }

    public int MaximumStock
    {
        get => int.TryParse(txtMaximumStock.Text, out var value) ? value : 0;
        set => txtMaximumStock.Text = value.ToString();
    }

    public decimal StandardCost
    {
        get => decimal.TryParse(txtStandardCost.Text, out var value) ? value : 0;
        set => txtStandardCost.Text = value.ToString("F2");
    }

    public decimal SellingPrice
    {
        get => decimal.TryParse(txtSellingPrice.Text, out var value) ? value : 0;
        set => txtSellingPrice.Text = value.ToString("F2");
    }

    public Guid? LocationId
    {
        get => cmbLocation.SelectedValue as Guid?;
        set => cmbLocation.SelectedValue = value;
    }

    public Guid? SupplierId
    {
        get => cmbSupplier.SelectedValue as Guid?;
        set => cmbSupplier.SelectedValue = value;
    }

    public string Notes
    {
        get => txtNotes.Text;
        set => txtNotes.Text = value;
    }

    public bool IsActive
    {
        get => chkIsActive.Checked;
        set => chkIsActive.Checked = value;
    }

    // Computed properties
    public int AvailableStock => CurrentStock - ReservedStock;
    public decimal TotalValue => CurrentStock * StandardCost;
    public string StockStatus
    {
        get
        {
            if (CurrentStock == 0) return "Out of Stock";
            if (CurrentStock <= MinimumStock) return "Low Stock";
            if (CurrentStock >= MaximumStock) return "Overstocked";
            return "In Stock";
        }
    }

    public DateTime? LastMovement { get; set; }

    // Data binding
    public IEnumerable<InventoryItemSummaryDto> InventoryItems
    {
        get => (IEnumerable<InventoryItemSummaryDto>)dgvItems.DataSource ?? new List<InventoryItemSummaryDto>();
        set => dgvItems.DataSource = value.ToList();
    }

    public IEnumerable<LocationSummaryDto> Locations { get; set; } = new List<LocationSummaryDto>();
    public IEnumerable<SupplierSummaryDto> Suppliers { get; set; } = new List<SupplierSummaryDto>();
    public IEnumerable<LowStockAlertDto> LowStockAlerts { get; set; } = new List<LowStockAlertDto>();

    // Selection
    public InventoryItemSummaryDto? SelectedItem => GetSelectedItem();
    public Guid? SelectedItemId => SelectedItem?.Id;

    // Filtering
    public string SearchText
    {
        get => txtSearch.Text;
        set => txtSearch.Text = value;
    }

    public string SelectedCategory
    {
        get => cmbCategoryFilter.Text;
        set => cmbCategoryFilter.Text = value;
    }

    public Guid? SelectedLocationFilter
    {
        get => cmbLocationFilter.SelectedValue as Guid?;
        set => cmbLocationFilter.SelectedValue = value;
    }

    public Guid? SelectedSupplierFilter
    {
        get => cmbSupplierFilter.SelectedValue as Guid?;
        set => cmbSupplierFilter.SelectedValue = value;
    }

    public bool ShowInactiveItems
    {
        get => chkShowInactive.Checked;
        set => chkShowInactive.Checked = value;
    }

    // Events
    public event EventHandler? LoadDataRequested;
    public event EventHandler? NewItemRequested;
    public event EventHandler? EditItemRequested;
    public event EventHandler? DeleteItemRequested;
    public event EventHandler? SaveItemRequested;
    public event EventHandler? CancelEditRequested;
    public event EventHandler? RefreshRequested;
    public event EventHandler? SearchTextChanged;
    public event EventHandler? FilterChanged;
    public event EventHandler? ItemSelectionChanged;
    public event EventHandler? ScanBarcodeRequested;
    public event EventHandler? UpdateStockRequested;
    public event EventHandler? LowStockAlertsRequested;

    #endregion

    #region Form Event Handlers

    private void SetupEventHandlers()
    {
        // Button events
        btnNew.Click += (s, e) => NewItemRequested?.Invoke(this, EventArgs.Empty);
        btnEdit.Click += (s, e) => EditItemRequested?.Invoke(this, EventArgs.Empty);
        btnDelete.Click += (s, e) => DeleteItemRequested?.Invoke(this, EventArgs.Empty);
        btnSave.Click += (s, e) => SaveItemRequested?.Invoke(this, EventArgs.Empty);
        btnCancel.Click += (s, e) => CancelEditRequested?.Invoke(this, EventArgs.Empty);
        btnRefresh.Click += (s, e) => RefreshRequested?.Invoke(this, EventArgs.Empty);
        btnScanBarcode.Click += (s, e) => ScanBarcodeRequested?.Invoke(this, EventArgs.Empty);
        btnUpdateStock.Click += (s, e) => UpdateStockRequested?.Invoke(this, EventArgs.Empty);
        btnLowStockAlerts.Click += (s, e) => LowStockAlertsRequested?.Invoke(this, EventArgs.Empty);

        // Search and filter events
        txtSearch.TextChanged += (s, e) => SearchTextChanged?.Invoke(this, EventArgs.Empty);
        cmbCategoryFilter.SelectedIndexChanged += (s, e) => FilterChanged?.Invoke(this, EventArgs.Empty);
        cmbLocationFilter.SelectedIndexChanged += (s, e) => FilterChanged?.Invoke(this, EventArgs.Empty);
        cmbSupplierFilter.SelectedIndexChanged += (s, e) => FilterChanged?.Invoke(this, EventArgs.Empty);
        chkShowInactive.CheckedChanged += (s, e) => FilterChanged?.Invoke(this, EventArgs.Empty);

        // Grid events
        dgvItems.SelectionChanged += (s, e) => ItemSelectionChanged?.Invoke(this, EventArgs.Empty);

        // Form events
        Load += (s, e) => LoadDataRequested?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Private Methods

    private void SetViewMode()
    {
        // Enable view controls
        SetNewEnabled(true);
        SetEditEnabled(true);
        SetDeleteEnabled(true);
        SetSaveEnabled(false);
        SetCancelEnabled(false);

        // Set form to read-only
        SetFormReadOnly(true);
        SetDataEntryMode(false);

        // Update button texts
        btnNew.Text = "&New";
        btnEdit.Text = "&Edit";
        btnDelete.Text = "&Delete";
        btnSave.Text = "&Save";
        btnCancel.Text = "&Cancel";
    }

    private void SetNewMode()
    {
        // Enable new controls
        SetNewEnabled(false);
        SetEditEnabled(false);
        SetDeleteEnabled(false);
        SetSaveEnabled(true);
        SetCancelEnabled(true);

        // Set form to editable
        SetFormReadOnly(false);
        SetDataEntryMode(true);

        // Update button texts
        btnSave.Text = "&Create";
        btnCancel.Text = "&Cancel";
    }

    private void SetEditMode()
    {
        // Enable edit controls
        SetNewEnabled(false);
        SetEditEnabled(false);
        SetDeleteEnabled(false);
        SetSaveEnabled(true);
        SetCancelEnabled(true);

        // Set form to editable
        SetFormReadOnly(false);
        SetDataEntryMode(true);

        // Update button texts
        btnSave.Text = "&Update";
        btnCancel.Text = "&Cancel";
    }

    private InventoryItemSummaryDto? GetSelectedItem()
    {
        if (dgvItems.CurrentRow != null && dgvItems.CurrentRow.DataBoundItem is InventoryItemSummaryDto item)
        {
            return item;
        }
        return null;
    }

    private void ClearForm()
    {
        PartNumber = string.Empty;
        Description = string.Empty;
        Barcode = string.Empty;
        Category = string.Empty;
        SubCategory = string.Empty;
        UnitOfMeasure = "Each";
        CurrentStock = 0;
        ReservedStock = 0;
        MinimumStock = 0;
        MaximumStock = 0;
        StandardCost = 0;
        SellingPrice = 0;
        LocationId = null;
        SupplierId = null;
        Notes = string.Empty;
        IsActive = true;

        ClearFieldErrors();
    }

    private void OnRefreshRequested()
    {
        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region IInventoryItemView Implementation - Additional Methods

    public void SetFieldError(string fieldName, string errorMessage)
    {
        // This would highlight the field with an error
        // For now, just show the error
        ShowError($"{fieldName}: {errorMessage}");
    }

    public void ClearFieldErrors()
    {
        // Clear any field-level error indicators
    }

    public bool ValidateForm()
    {
        ClearFieldErrors();

        if (string.IsNullOrWhiteSpace(PartNumber))
        {
            SetFieldError("Part Number", "Part number is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            SetFieldError("Description", "Description is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Barcode))
        {
            SetFieldError("Barcode", "Barcode is required");
            return false;
        }

        if (!LocationId.HasValue)
        {
            SetFieldError("Location", "Location is required");
            return false;
        }

        if (MinimumStock > MaximumStock && MaximumStock > 0)
        {
            SetFieldError("Stock Levels", "Minimum stock cannot be greater than maximum stock");
            return false;
        }

        return true;
    }

    public void SetFormReadOnly(bool readOnly)
    {
        txtPartNumber.ReadOnly = readOnly || _currentMode == OperationMode.Edit;
        txtDescription.ReadOnly = readOnly;
        txtBarcode.ReadOnly = readOnly;
        txtCategory.ReadOnly = readOnly;
        txtSubCategory.ReadOnly = readOnly;
        cmbUnitOfMeasure.Enabled = !readOnly;
        txtCurrentStock.ReadOnly = readOnly;
        txtReservedStock.ReadOnly = readOnly;
        txtMinimumStock.ReadOnly = readOnly;
        txtMaximumStock.ReadOnly = readOnly;
        txtStandardCost.ReadOnly = readOnly;
        txtSellingPrice.ReadOnly = readOnly;
        cmbLocation.Enabled = !readOnly;
        cmbSupplier.Enabled = !readOnly;
        txtNotes.ReadOnly = readOnly;
        chkIsActive.Enabled = !readOnly;
    }

    public void SetDataEntryMode(bool isDataEntry)
    {
        // Adjust UI for data entry vs viewing
        if (isDataEntry)
        {
            txtPartNumber.BackColor = Color.White;
            txtDescription.BackColor = Color.White;
        }
        else
        {
            txtPartNumber.BackColor = SystemColors.Control;
            txtDescription.BackColor = SystemColors.Control;
        }
    }

    public void FocusField(string fieldName)
    {
        var control = GetControlByFieldName(fieldName);
        control?.Focus();
    }

    public void HighlightChangedFields()
    {
        // This would highlight fields that have been changed
        // Implementation depends on change tracking
    }

    public async void ShowBarcodeScanner()
    {
        try
        {
            var barcode = await frmBarcodeScanner.ShowScannerDialogAsync(this);
            if (!string.IsNullOrEmpty(barcode))
            {
                Barcode = barcode;
                ShowSuccess($"Barcode scanned: {barcode}");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Barcode scanner error: {ex.Message}");
        }
    }

    public void UpdateStockDisplay()
    {
        // Update stock-related display fields
        lblAvailableStock.Text = $"Available: {AvailableStock}";
        lblTotalValue.Text = $"Total Value: ${TotalValue:F2}";
        lblStockStatus.Text = $"Status: {StockStatus}";
    }

    public void ShowLowStockWarning()
    {
        if (LowStockAlerts.Any())
        {
            var count = LowStockAlerts.Count();
            ShowInformation($"Warning: {count} items are low on stock");
        }
    }

    public void RefreshItemList()
    {
        dgvItems.Refresh();
        UpdateItemCount();
    }

    public void UpdateItemDetails()
    {
        UpdateStockDisplay();

        if (LastMovement.HasValue)
        {
            lblLastMovement.Text = $"Last Movement: {LastMovement.Value:d}";
        }
        else
        {
            lblLastMovement.Text = "Last Movement: Never";
        }
    }

    public OperationMode GetOperationMode()
    {
        return _currentMode;
    }

    #endregion

    #region Helper Methods

    private Control? GetControlByFieldName(string fieldName)
    {
        return fieldName switch
        {
            "PartNumber" => txtPartNumber,
            "Description" => txtDescription,
            "Barcode" => txtBarcode,
            "Category" => txtCategory,
            "SubCategory" => txtSubCategory,
            "UnitOfMeasure" => cmbUnitOfMeasure,
            "CurrentStock" => txtCurrentStock,
            "MinimumStock" => txtMinimumStock,
            "MaximumStock" => txtMaximumStock,
            "StandardCost" => txtStandardCost,
            "SellingPrice" => txtSellingPrice,
            "Location" => cmbLocation,
            "Supplier" => cmbSupplier,
            "Notes" => txtNotes,
            _ => null
        };
    }

    private void UpdateItemCount()
    {
        var count = InventoryItems.Count();
        lblItemCount.Text = $"{count} item{(count != 1 ? "s" : "")}";
    }

    #endregion

    #region Designer Generated Code
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.btnNew = new System.Windows.Forms.Button();
        this.btnEdit = new System.Windows.Forms.Button();
        this.btnDelete = new System.Windows.Forms.Button();
        this.btnSave = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.btnRefresh = new System.Windows.Forms.Button();
        this.btnScanBarcode = new System.Windows.Forms.Button();
        this.btnUpdateStock = new System.Windows.Forms.Button();
        this.btnLowStockAlerts = new System.Windows.Forms.Button();

        this.txtPartNumber = new System.Windows.Forms.TextBox();
        this.txtDescription = new System.Windows.Forms.TextBox();
        this.txtBarcode = new System.Windows.Forms.TextBox();
        this.txtCategory = new System.Windows.Forms.TextBox();
        this.txtSubCategory = new System.Windows.Forms.TextBox();
        this.cmbUnitOfMeasure = new System.Windows.Forms.ComboBox();
        this.txtCurrentStock = new System.Windows.Forms.TextBox();
        this.txtReservedStock = new System.Windows.Forms.TextBox();
        this.txtMinimumStock = new System.Windows.Forms.TextBox();
        this.txtMaximumStock = new System.Windows.Forms.TextBox();
        this.txtStandardCost = new System.Windows.Forms.TextBox();
        this.txtSellingPrice = new System.Windows.Forms.TextBox();
        this.cmbLocation = new System.Windows.Forms.ComboBox();
        this.cmbSupplier = new System.Windows.Forms.ComboBox();
        this.txtNotes = new System.Windows.Forms.TextBox();
        this.chkIsActive = new System.Windows.Forms.CheckBox();

        this.txtSearch = new System.Windows.Forms.TextBox();
        this.cmbCategoryFilter = new System.Windows.Forms.ComboBox();
        this.cmbLocationFilter = new System.Windows.Forms.ComboBox();
        this.cmbSupplierFilter = new System.Windows.Forms.ComboBox();
        this.chkShowInactive = new System.Windows.Forms.CheckBox();

        this.dgvItems = new System.Windows.Forms.DataGridView();
        this.lblAvailableStock = new System.Windows.Forms.Label();
        this.lblTotalValue = new System.Windows.Forms.Label();
        this.lblStockStatus = new System.Windows.Forms.Label();
        this.lblLastMovement = new System.Windows.Forms.Label();
        this.lblItemCount = new System.Windows.Forms.Label();

        // Form setup
        this.Text = "Inventory Items Management";
        this.Size = new System.Drawing.Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterScreen;

        // Add controls to form
        this.Controls.AddRange(new Control[] {
            btnNew, btnEdit, btnDelete, btnSave, btnCancel, btnRefresh,
            btnScanBarcode, btnUpdateStock, btnLowStockAlerts,
            txtPartNumber, txtDescription, txtBarcode, txtCategory, txtSubCategory,
            cmbUnitOfMeasure, txtCurrentStock, txtReservedStock, txtMinimumStock, txtMaximumStock,
            txtStandardCost, txtSellingPrice, cmbLocation, cmbSupplier, txtNotes, chkIsActive,
            txtSearch, cmbCategoryFilter, cmbLocationFilter, cmbSupplierFilter, chkShowInactive,
            dgvItems, lblAvailableStock, lblTotalValue, lblStockStatus, lblLastMovement, lblItemCount
        });

        // Set up basic properties
        SetupControlProperties();
    }

    private void SetupControlProperties()
    {
        // Buttons
        btnNew.Text = "&New";
        btnNew.Location = new Point(10, 10);

        btnEdit.Text = "&Edit";
        btnEdit.Location = new Point(90, 10);

        btnDelete.Text = "&Delete";
        btnDelete.Location = new Point(170, 10);

        btnSave.Text = "&Save";
        btnSave.Location = new Point(250, 10);

        btnCancel.Text = "&Cancel";
        btnCancel.Location = new Point(330, 10);

        btnRefresh.Text = "&Refresh";
        btnRefresh.Location = new Point(410, 10);

        btnScanBarcode.Text = "Scan &Barcode";
        btnScanBarcode.Location = new Point(490, 10);

        btnUpdateStock.Text = "Update &Stock";
        btnUpdateStock.Location = new Point(570, 10);

        btnLowStockAlerts.Text = "&Low Stock Alerts";
        btnLowStockAlerts.Location = new Point(650, 10);

        // Text boxes
        txtPartNumber.Location = new Point(150, 60);
        txtDescription.Location = new Point(150, 90);
        txtBarcode.Location = new Point(150, 120);
        txtCategory.Location = new Point(150, 150);
        txtSubCategory.Location = new Point(150, 180);
        txtCurrentStock.Location = new Point(150, 210);
        txtReservedStock.Location = new Point(150, 240);
        txtMinimumStock.Location = new Point(150, 270);
        txtMaximumStock.Location = new Point(150, 300);
        txtStandardCost.Location = new Point(150, 330);
        txtSellingPrice.Location = new Point(150, 360);
        txtNotes.Location = new Point(150, 390);

        // Combo boxes
        cmbUnitOfMeasure.Location = new Point(150, 420);
        cmbLocation.Location = new Point(150, 450);
        cmbSupplier.Location = new Point(150, 480);

        // Check box
        chkIsActive.Location = new Point(150, 510);

        // Search controls
        txtSearch.Location = new Point(10, 550);
        cmbCategoryFilter.Location = new Point(200, 550);
        cmbLocationFilter.Location = new Point(350, 550);
        cmbSupplierFilter.Location = new Point(500, 550);
        chkShowInactive.Location = new Point(650, 550);

        // Data grid
        dgvItems.Location = new Point(10, 580);
        dgvItems.Size = new Size(780, 150);

        // Labels
        lblAvailableStock.Location = new Point(800, 60);
        lblTotalValue.Location = new Point(800, 90);
        lblStockStatus.Location = new Point(800, 120);
        lblLastMovement.Location = new Point(800, 150);
        lblItemCount.Location = new Point(800, 180);
    }

    #endregion

    #region Form Controls
    private System.ComponentModel.IContainer components;
    private Button btnNew;
    private Button btnEdit;
    private Button btnDelete;
    private Button btnSave;
    private Button btnCancel;
    private Button btnRefresh;
    private Button btnScanBarcode;
    private Button btnUpdateStock;
    private Button btnLowStockAlerts;

    private TextBox txtPartNumber;
    private TextBox txtDescription;
    private TextBox txtBarcode;
    private TextBox txtCategory;
    private TextBox txtSubCategory;
    private ComboBox cmbUnitOfMeasure;
    private TextBox txtCurrentStock;
    private TextBox txtReservedStock;
    private TextBox txtMinimumStock;
    private TextBox txtMaximumStock;
    private TextBox txtStandardCost;
    private TextBox txtSellingPrice;
    private ComboBox cmbLocation;
    private ComboBox cmbSupplier;
    private TextBox txtNotes;
    private CheckBox chkIsActive;

    private TextBox txtSearch;
    private ComboBox cmbCategoryFilter;
    private ComboBox cmbLocationFilter;
    private ComboBox cmbSupplierFilter;
    private CheckBox chkShowInactive;

    private DataGridView dgvItems;
    private Label lblAvailableStock;
    private Label lblTotalValue;
    private Label lblStockStatus;
    private Label lblLastMovement;
    private Label lblItemCount;
    #endregion
}
