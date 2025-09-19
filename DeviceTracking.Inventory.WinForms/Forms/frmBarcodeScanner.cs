using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeviceTracking.Inventory.WinForms.CameraDeviceWrapper;
using DeviceTracking.Inventory.WinForms.BarcodeConfiguration;

namespace DeviceTracking.Inventory.WinForms.Forms;

/// <summary>
/// Barcode Scanner Form - Integrates with CameraDevice for real-time scanning
/// </summary>
public partial class frmBarcodeScanner : Form
{
    private CameraDeviceWrapper? _camera;
    private BarcodeReader? _barcodeReader;
    private bool _isScanning;
    private System.Windows.Forms.Timer? _scanTimer;

    public event EventHandler<string>? BarcodeScanned;
    public event EventHandler? ScanCancelled;

    public frmBarcodeScanner()
    {
        InitializeComponent();
        InitializeCamera();
        SetupScanTimer();
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();

        this.lblInstructions = new System.Windows.Forms.Label();
        this.btnStartScan = new System.Windows.Forms.Button();
        this.btnStopScan = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        this.lblLastScan = new System.Windows.Forms.Label();
        this.txtManualBarcode = new System.Windows.Forms.TextBox();
        this.btnManualEntry = new System.Windows.Forms.Button();
        this.chkAutoSubmit = new System.Windows.Forms.CheckBox();
        this.cmbScanMode = new System.Windows.Forms.ComboBox();

        // Form setup
        this.Text = "Barcode Scanner";
        this.Size = new System.Drawing.Size(600, 400);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // Instructions label
        lblInstructions.Text = "Position the barcode in front of the camera and click 'Start Scan'.\nThe scanner will automatically detect and process barcodes.";
        lblInstructions.Location = new Point(20, 20);
        lblInstructions.Size = new Size(550, 40);
        lblInstructions.Font = new Font("Arial", 9);

        // Scan mode combo box
        var lblScanMode = new Label();
        lblScanMode.Text = "Scan Mode:";
        lblScanMode.Location = new Point(20, 70);
        lblScanMode.Size = new Size(80, 20);

        cmbScanMode.Items.AddRange(new string[] { "Single Item", "Bulk Scan", "Continuous" });
        cmbScanMode.SelectedIndex = 0;
        cmbScanMode.Location = new Point(100, 70);
        cmbScanMode.Size = new Size(120, 20);

        // Buttons
        btnStartScan.Text = "&Start Scan";
        btnStartScan.Location = new Point(20, 110);
        btnStartScan.Size = new Size(100, 30);
        btnStartScan.Click += BtnStartScan_Click;

        btnStopScan.Text = "S&top Scan";
        btnStopScan.Location = new Point(130, 110);
        btnStopScan.Size = new Size(100, 30);
        btnStopScan.Enabled = false;
        btnStopScan.Click += BtnStopScan_Click;

        btnCancel.Text = "&Cancel";
        btnCancel.Location = new Point(240, 110);
        btnCancel.Size = new Size(100, 30);
        btnCancel.Click += BtnCancel_Click;

        // Status labels
        lblStatus.Text = "Ready to scan";
        lblStatus.Location = new Point(20, 160);
        lblStatus.Size = new Size(550, 20);
        lblStatus.Font = new Font("Arial", 9, FontStyle.Bold);

        lblLastScan.Text = "Last scan: None";
        lblLastScan.Location = new Point(20, 180);
        lblLastScan.Size = new Size(550, 20);

        // Manual entry section
        var lblManualEntry = new Label();
        lblManualEntry.Text = "Manual Entry (if camera not available):";
        lblManualEntry.Location = new Point(20, 220);
        lblManualEntry.Size = new Size(250, 20);

        txtManualBarcode.Location = new Point(20, 240);
        txtManualBarcode.Size = new Size(200, 20);

        btnManualEntry.Text = "&Enter";
        btnManualEntry.Location = new Point(230, 238);
        btnManualEntry.Size = new Size(60, 25);
        btnManualEntry.Click += BtnManualEntry_Click;

        // Options
        chkAutoSubmit.Text = "Auto-submit on successful scan";
        chkAutoSubmit.Location = new Point(20, 280);
        chkAutoSubmit.Checked = true;

        // Add controls
        this.Controls.AddRange(new Control[] {
            lblInstructions, lblScanMode, cmbScanMode,
            btnStartScan, btnStopScan, btnCancel,
            lblStatus, lblLastScan,
            lblManualEntry, txtManualBarcode, btnManualEntry,
            chkAutoSubmit
        });
    }

    private void InitializeCamera()
    {
        try
        {
            _camera = new CameraDeviceWrapper();
            _barcodeReader = new BarcodeReader();

            if (_camera.IsAvailable)
            {
                UpdateStatus("Camera initialized successfully");
            }
            else
            {
                UpdateStatus("Camera not available - use manual entry");
                btnStartScan.Enabled = false;
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Camera initialization failed: {ex.Message}");
            btnStartScan.Enabled = false;
        }
    }

    private void SetupScanTimer()
    {
        _scanTimer = new System.Windows.Forms.Timer();
        _scanTimer.Interval = 500; // Scan every 500ms
        _scanTimer.Tick += ScanTimer_Tick;
    }

    private async void BtnStartScan_Click(object? sender, EventArgs e)
    {
        if (_camera == null || !_camera.IsAvailable)
        {
            UpdateStatus("Camera not available");
            return;
        }

        _isScanning = true;
        btnStartScan.Enabled = false;
        btnStopScan.Enabled = true;
        UpdateStatus("Scanning... Position barcode in front of camera");

        _scanTimer?.Start();
    }

    private void BtnStopScan_Click(object? sender, EventArgs e)
    {
        StopScanning();
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        StopScanning();
        ScanCancelled?.Invoke(this, EventArgs.Empty);
        this.Close();
    }

    private async void ScanTimer_Tick(object? sender, EventArgs e)
    {
        if (!_isScanning) return;

        try
        {
            var result = await _camera!.ScanBarcodeAsync();

            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                await ProcessScanResult(result);
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Scan error: {ex.Message}");
        }
    }

    private async Task ProcessScanResult(BarcodeResult result)
    {
        StopScanning();

        UpdateStatus($"Barcode detected: {result.Text} ({result.Format})");
        lblLastScan.Text = $"Last scan: {result.Text} (Confidence: {result.Confidence}%)";

        // Validate barcode
        var validationResult = BarcodeConfiguration.ValidateBarcodeFormat(result.Text);
        if (!validationResult.IsSuccess)
        {
            UpdateStatus($"Invalid barcode format: {validationResult.Error}");
            return;
        }

        // Check scanning quality
        var qualityResult = BarcodeConfiguration.ValidateScanningQuality(result.Confidence, result.Text.Length);
        if (!qualityResult.IsSuccess)
        {
            UpdateStatus(qualityResult.Error);
            // Continue anyway as it might still be usable
        }

        // Auto-submit if enabled
        if (chkAutoSubmit.Checked)
        {
            BarcodeScanned?.Invoke(this, result.Text);
            this.Close();
        }
        else
        {
            UpdateStatus("Barcode scanned successfully - click OK to use this barcode");
        }
    }

    private void BtnManualEntry_Click(object? sender, EventArgs e)
    {
        var barcode = txtManualBarcode.Text.Trim();
        if (string.IsNullOrEmpty(barcode))
        {
            UpdateStatus("Please enter a barcode");
            return;
        }

        // Validate manual entry
        var validationResult = BarcodeConfiguration.ValidateBarcodeFormat(barcode);
        if (!validationResult.IsSuccess)
        {
            UpdateStatus($"Invalid barcode format: {validationResult.Error}");
            return;
        }

        UpdateStatus($"Manual barcode entered: {barcode}");
        lblLastScan.Text = $"Last scan: {barcode} (Manual entry)";

        BarcodeScanned?.Invoke(this, barcode);
        this.Close();
    }

    private void StopScanning()
    {
        _isScanning = false;
        _scanTimer?.Stop();
        btnStartScan.Enabled = true;
        btnStopScan.Enabled = false;
        UpdateStatus("Scan stopped");
    }

    private void UpdateStatus(string message)
    {
        lblStatus.Text = message;
        lblStatus.Refresh();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        StopScanning();
        _camera?.Dispose();
        base.OnFormClosing(e);
    }

    /// <summary>
    /// Gets available cameras for selection
    /// </summary>
    public static string[] GetAvailableCameras()
    {
        return CameraDeviceWrapper.GetAvailableCameras();
    }

    /// <summary>
    /// Shows the barcode scanner dialog and returns the scanned barcode
    /// </summary>
    public static async Task<string?> ShowScannerDialogAsync(IWin32Window owner)
    {
        using var scanner = new frmBarcodeScanner();
        var completionSource = new TaskCompletionSource<string?>();

        scanner.BarcodeScanned += (s, barcode) => completionSource.SetResult(barcode);
        scanner.ScanCancelled += (s, e) => completionSource.SetResult(null);

        scanner.ShowDialog(owner);
        return await completionSource.Task;
    }
}

/// <summary>
/// Result of a barcode scan operation
/// </summary>
public class BarcodeResult
{
    public string Text { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public int Confidence { get; set; }
    public DateTime Timestamp { get; set; }
}
