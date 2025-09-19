using System;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Application.Common;
using DeviceTracking.Inventory.Application.Services;
using DeviceTracking.Inventory.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeviceTracking.Inventory.Api.Controllers;

/// <summary>
/// Controller for inventory item operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class InventoryItemsController : ControllerBase
{
    private readonly IInventoryItemService _inventoryItemService;
    private readonly ILogger<InventoryItemsController> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public InventoryItemsController(
        IInventoryItemService inventoryItemService,
        ILogger<InventoryItemsController> logger)
    {
        _inventoryItemService = inventoryItemService ?? throw new ArgumentNullException(nameof(inventoryItemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get inventory items with pagination and filtering
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of inventory items</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<InventoryItemSummaryDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<PagedResponse<InventoryItemSummaryDto>>>> Get(
        [FromQuery] InventoryItemQueryDto query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting inventory items with query: {@Query}", query);

            var result = await _inventoryItemService.GetPagedAsync(query, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            _logger.LogWarning("Failed to get inventory items: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory items");
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving inventory items"));
        }
    }

    /// <summary>
    /// Get inventory item by ID
    /// </summary>
    /// <param name="id">Inventory item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Inventory item details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<InventoryItemDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting inventory item by ID: {Id}", id);

            var result = await _inventoryItemService.GetByIdAsync(id, cancellationToken);

            if (result.IsSuccess)
            {
                if (result.Data == null)
                {
                    return NotFound(ApiResponse.Fail("Inventory item not found"));
                }
                return Ok(result);
            }

            _logger.LogWarning("Failed to get inventory item {Id}: {Error}", id, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory item {Id}", id);
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving the inventory item"));
        }
    }

    /// <summary>
    /// Get inventory item by barcode
    /// </summary>
    /// <param name="barcode">Barcode value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Inventory item details</returns>
    [HttpGet("by-barcode/{barcode}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<InventoryItemDto>>> GetByBarcode(
        string barcode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting inventory item by barcode: {Barcode}", barcode);

            var result = await _inventoryItemService.GetByBarcodeAsync(barcode, cancellationToken);

            if (result.IsSuccess)
            {
                if (result.Data == null)
                {
                    return NotFound(ApiResponse.Fail("Inventory item not found for the specified barcode"));
                }
                return Ok(result);
            }

            _logger.LogWarning("Failed to get inventory item by barcode {Barcode}: {Error}", barcode, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory item by barcode {Barcode}", barcode);
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving the inventory item"));
        }
    }

    /// <summary>
    /// Create a new inventory item
    /// </summary>
    /// <param name="dto">Inventory item creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created inventory item</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<InventoryItemDto>>> Create(
        [FromBody] CreateInventoryItemDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating inventory item: {@Dto}", dto);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _inventoryItemService.CreateAsync(dto, userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created inventory item with ID: {Id}", result.Data?.Id);
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Data?.Id },
                    result);
            }

            _logger.LogWarning("Failed to create inventory item: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inventory item");
            return StatusCode(500, ApiResponse.Fail("An error occurred while creating the inventory item"));
        }
    }

    /// <summary>
    /// Update an existing inventory item
    /// </summary>
    /// <param name="id">Inventory item ID</param>
    /// <param name="dto">Inventory item update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated inventory item</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<InventoryItemDto>>> Update(
        Guid id,
        [FromBody] UpdateInventoryItemDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating inventory item {Id}: {@Dto}", id, dto);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _inventoryItemService.UpdateAsync(id, dto, userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated inventory item {Id}", id);
                return Ok(result);
            }

            _logger.LogWarning("Failed to update inventory item {Id}: {Error}", id, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inventory item {Id}", id);
            return StatusCode(500, ApiResponse.Fail("An error occurred while updating the inventory item"));
        }
    }

    /// <summary>
    /// Delete an inventory item
    /// </summary>
    /// <param name="id">Inventory item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Delete result</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), 204)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse>> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting inventory item {Id}", id);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _inventoryItemService.DeleteAsync(id, userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted inventory item {Id}", id);
                return NoContent();
            }

            _logger.LogWarning("Failed to delete inventory item {Id}: {Error}", id, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inventory item {Id}", id);
            return StatusCode(500, ApiResponse.Fail("An error occurred while deleting the inventory item"));
        }
    }

    /// <summary>
    /// Record barcode scan for inventory item
    /// </summary>
    /// <param name="id">Inventory item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Scan result</returns>
    [HttpPost("{id}/scan")]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<InventoryItemDto>>> RecordScan(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Recording barcode scan for inventory item {Id}", id);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _inventoryItemService.RecordBarcodeScanAsync(id.ToString(), userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully recorded barcode scan for item {Id}", id);
                return Ok(result);
            }

            _logger.LogWarning("Failed to record barcode scan for item {Id}: {Error}", id, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording barcode scan for item {Id}", id);
            return StatusCode(500, ApiResponse.Fail("An error occurred while recording the barcode scan"));
        }
    }

    /// <summary>
    /// Get low stock alerts
    /// </summary>
    /// <param name="threshold">Custom threshold for low stock (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of low stock alerts</returns>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LowStockAlertDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LowStockAlertDto>>>> GetLowStockAlerts(
        [FromQuery] int? threshold = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting low stock alerts with threshold: {Threshold}", threshold);

            var result = await _inventoryItemService.GetLowStockAlertsAsync(threshold ?? 10, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            _logger.LogWarning("Failed to get low stock alerts: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock alerts");
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving low stock alerts"));
        }
    }

    /// <summary>
    /// Update stock levels for an inventory item
    /// </summary>
    /// <param name="id">Inventory item ID</param>
    /// <param name="quantityChange">Quantity change (positive for increase, negative for decrease)</param>
    /// <param name="reason">Reason for the stock change</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated inventory item</returns>
    [HttpPatch("{id}/stock")]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<InventoryItemDto>>> UpdateStock(
        Guid id,
        [FromBody] UpdateStockRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating stock for inventory item {Id}: {QuantityChange}", id, request.QuantityChange);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _inventoryItemService.UpdateStockAsync(id, request.QuantityChange, request.Reason, userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated stock for item {Id}", id);
                return Ok(result);
            }

            _logger.LogWarning("Failed to update stock for item {Id}: {Error}", id, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for item {Id}", id);
            return StatusCode(500, ApiResponse.Fail("An error occurred while updating the stock level"));
        }
    }

    /// <summary>
    /// Get current user ID from claims
    /// </summary>
    private string? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return userIdClaim?.Value;
    }
}

/// <summary>
/// Request model for stock updates
/// </summary>
public class UpdateStockRequest
{
    /// <summary>
    /// Quantity change (positive for increase, negative for decrease)
    /// </summary>
    public int QuantityChange { get; set; }

    /// <summary>
    /// Reason for the stock change
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
