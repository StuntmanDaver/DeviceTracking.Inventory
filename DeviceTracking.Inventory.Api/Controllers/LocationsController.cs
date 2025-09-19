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
/// Controller for location operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly ILogger<LocationsController> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public LocationsController(
        ILocationService locationService,
        ILogger<LocationsController> logger)
    {
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get locations with pagination and filtering
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of locations</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<LocationDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<PagedResponse<LocationDto>>>> Get(
        [FromQuery] LocationQueryDto query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting locations with query: {@Query}", query);

            var result = await _locationService.GetPagedAsync(query, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            _logger.LogWarning("Failed to get locations: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting locations");
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving locations"));
        }
    }

    /// <summary>
    /// Get location by ID
    /// </summary>
    /// <param name="id">Location ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Location details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<LocationDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<LocationDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting location by ID: {Id}", id);

            var result = await _locationService.GetByIdAsync(id, cancellationToken);

            if (result.IsSuccess)
            {
                if (result.Data == null)
                {
                    return NotFound(ApiResponse.Fail("Location not found"));
                }
                return Ok(result);
            }

            _logger.LogWarning("Failed to get location {Id}: {Error}", id, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location {Id}", id);
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving the location"));
        }
    }

    /// <summary>
    /// Get location by code
    /// </summary>
    /// <param name="code">Location code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Location details</returns>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(ApiResponse<LocationDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<LocationDto>>> GetByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting location by code: {Code}", code);

            var result = await _locationService.GetByCodeAsync(code, cancellationToken);

            if (result.IsSuccess)
            {
                if (result.Data == null)
                {
                    return NotFound(ApiResponse.Fail("Location not found"));
                }
                return Ok(result);
            }

            _logger.LogWarning("Failed to get location by code {Code}: {Error}", code, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location by code {Code}", code);
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving the location"));
        }
    }

    /// <summary>
    /// Create a new location
    /// </summary>
    /// <param name="dto">Location creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created location</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LocationDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<LocationDto>>> Create(
        [FromBody] CreateLocationDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating location: {@Dto}", dto);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _locationService.CreateAsync(dto, userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created location with ID: {Id}", result.Data?.Id);
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Data?.Id },
                    result);
            }

            _logger.LogWarning("Failed to create location: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating location");
            return StatusCode(500, ApiResponse.Fail("An error occurred while creating the location"));
        }
    }

    /// <summary>
    /// Update an existing location
    /// </summary>
    /// <param name="id">Location ID</param>
    /// <param name="dto">Location update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated location</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<LocationDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<LocationDto>>> Update(
        Guid id,
        [FromBody] UpdateLocationDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating location {Id}: {@Dto}", id, dto);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _locationService.UpdateAsync(id, dto, userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated location {Id}", id);
                return Ok(result);
            }

            _logger.LogWarning("Failed to update location {Id}: {Error}", id, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating location {Id}", id);
            return StatusCode(500, ApiResponse.Fail("An error occurred while updating the location"));
        }
    }

    /// <summary>
    /// Delete a location
    /// </summary>
    /// <param name="id">Location ID</param>
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
            _logger.LogInformation("Deleting location {Id}", id);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _locationService.DeleteAsync(id, userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted location {Id}", id);
                return NoContent();
            }

            _logger.LogWarning("Failed to delete location {Id}: {Error}", id, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting location {Id}", id);
            return StatusCode(500, ApiResponse.Fail("An error occurred while deleting the location"));
        }
    }

    /// <summary>
    /// Get location hierarchy
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Location hierarchy</returns>
    [HttpGet("hierarchy")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LocationHierarchyDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LocationHierarchyDto>>>> GetHierarchy(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting location hierarchy");

            var result = await _locationService.GetHierarchyAsync(cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            _logger.LogWarning("Failed to get location hierarchy: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location hierarchy");
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving the location hierarchy"));
        }
    }

    /// <summary>
    /// Get locations by type
    /// </summary>
    /// <param name="type">Location type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of locations by type</returns>
    [HttpGet("by-type/{type}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LocationDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LocationDto>>>> GetByType(
        string type,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting locations by type: {Type}", type);

            var result = await _locationService.GetByTypeAsync(type, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            _logger.LogWarning("Failed to get locations by type {Type}: {Error}", type, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting locations by type {Type}", type);
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving locations by type"));
        }
    }

    /// <summary>
    /// Get capacity utilization for all locations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Capacity utilization data</returns>
    [HttpGet("capacity-utilization")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<(LocationDto Location, int ItemCount, decimal UtilizationPercent)>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<IEnumerable<(LocationDto Location, int ItemCount, decimal UtilizationPercent)>>>> GetCapacityUtilization(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting capacity utilization for all locations");

            var result = await _locationService.GetCapacityUtilizationAsync(cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            _logger.LogWarning("Failed to get capacity utilization: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting capacity utilization");
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving capacity utilization data"));
        }
    }

    /// <summary>
    /// Transfer items between locations
    /// </summary>
    /// <param name="fromLocationId">Source location ID</param>
    /// <param name="toLocationId">Destination location ID</param>
    /// <param name="items">Items to transfer with quantities</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transfer result</returns>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse>> TransferItems(
        [FromBody] TransferRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Transferring items from location {FromLocationId} to {ToLocationId}",
                request.FromLocationId, request.ToLocationId);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _locationService.TransferItemsAsync(
                request.FromLocationId,
                request.ToLocationId,
                request.Items,
                request.Reason,
                userId,
                cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully transferred items between locations");
                return Ok(result);
            }

            _logger.LogWarning("Failed to transfer items: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring items between locations");
            return StatusCode(500, ApiResponse.Fail("An error occurred while transferring items"));
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
/// Request model for item transfers
/// </summary>
public class TransferRequest
{
    /// <summary>
    /// Source location ID
    /// </summary>
    public Guid FromLocationId { get; set; }

    /// <summary>
    /// Destination location ID
    /// </summary>
    public Guid ToLocationId { get; set; }

    /// <summary>
    /// Items to transfer with quantities
    /// </summary>
    public IEnumerable<(Guid ItemId, int Quantity)> Items { get; set; } = new List<(Guid, int)>();

    /// <summary>
    /// Reason for the transfer
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
