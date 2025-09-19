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
/// Controller for inventory transaction operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IInventoryTransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public TransactionsController(
        IInventoryTransactionService transactionService,
        ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get transactions with pagination and filtering
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of transactions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<InventoryTransactionDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<PagedResponse<InventoryTransactionDto>>>> Get(
        [FromQuery] InventoryTransactionQueryDto query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting transactions with query: {@Query}", query);

            var result = await _transactionService.GetPagedAsync(query, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            _logger.LogWarning("Failed to get transactions: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions");
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving transactions"));
        }
    }

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryTransactionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<InventoryTransactionDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting transaction by ID: {Id}", id);

            var result = await _transactionService.GetByIdAsync(id, cancellationToken);

            if (result.IsSuccess)
            {
                if (result.Data == null)
                {
                    return NotFound(ApiResponse.Fail("Transaction not found"));
                }
                return Ok(result);
            }

            _logger.LogWarning("Failed to get transaction {Id}: {Error}", id, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction {Id}", id);
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving the transaction"));
        }
    }

    /// <summary>
    /// Record a receipt transaction
    /// </summary>
    /// <param name="dto">Receipt transaction data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created transaction</returns>
    [HttpPost("receipt")]
    [ProducesResponseType(typeof(ApiResponse<InventoryTransactionDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<InventoryTransactionDto>>> RecordReceipt(
        [FromBody] CreateReceiptDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Recording receipt transaction: {@Dto}", dto);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _transactionService.RecordReceiptAsync(dto, userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully recorded receipt transaction with ID: {Id}", result.Data?.Id);
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Data?.Id },
                    result);
            }

            _logger.LogWarning("Failed to record receipt transaction: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording receipt transaction");
            return StatusCode(500, ApiResponse.Fail("An error occurred while recording the receipt transaction"));
        }
    }

    /// <summary>
    /// Record an issue transaction
    /// </summary>
    /// <param name="dto">Issue transaction data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created transaction</returns>
    [HttpPost("issue")]
    [ProducesResponseType(typeof(ApiResponse<InventoryTransactionDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<InventoryTransactionDto>>> RecordIssue(
        [FromBody] CreateIssueDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Recording issue transaction: {@Dto}", dto);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _transactionService.RecordIssueAsync(dto, userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully recorded issue transaction with ID: {Id}", result.Data?.Id);
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Data?.Id },
                    result);
            }

            _logger.LogWarning("Failed to record issue transaction: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording issue transaction");
            return StatusCode(500, ApiResponse.Fail("An error occurred while recording the issue transaction"));
        }
    }

    /// <summary>
    /// Record a transfer transaction
    /// </summary>
    /// <param name="dto">Transfer transaction data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created transaction</returns>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(ApiResponse<InventoryTransactionDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<InventoryTransactionDto>>> RecordTransfer(
        [FromBody] CreateTransferDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Recording transfer transaction: {@Dto}", dto);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _transactionService.RecordTransferAsync(dto, userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully recorded transfer transaction with ID: {Id}", result.Data?.Id);
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Data?.Id },
                    result);
            }

            _logger.LogWarning("Failed to record transfer transaction: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording transfer transaction");
            return StatusCode(500, ApiResponse.Fail("An error occurred while recording the transfer transaction"));
        }
    }

    /// <summary>
    /// Record an adjustment transaction
    /// </summary>
    /// <param name="dto">Adjustment transaction data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created transaction</returns>
    [HttpPost("adjustment")]
    [ProducesResponseType(typeof(ApiResponse<InventoryTransactionDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<InventoryTransactionDto>>> RecordAdjustment(
        [FromBody] CreateAdjustmentDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Recording adjustment transaction: {@Dto}", dto);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _transactionService.RecordAdjustmentAsync(dto, userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully recorded adjustment transaction with ID: {Id}", result.Data?.Id);
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Data?.Id },
                    result);
            }

            _logger.LogWarning("Failed to record adjustment transaction: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording adjustment transaction");
            return StatusCode(500, ApiResponse.Fail("An error occurred while recording the adjustment transaction"));
        }
    }

    /// <summary>
    /// Approve a pending transaction
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Approved transaction</returns>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(ApiResponse<InventoryTransactionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<InventoryTransactionDto>>> Approve(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Approving transaction {Id}", id);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _transactionService.ApproveTransactionAsync(id, userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully approved transaction {Id}", id);
                return Ok(result);
            }

            _logger.LogWarning("Failed to approve transaction {Id}: {Error}", id, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving transaction {Id}", id);
            return StatusCode(500, ApiResponse.Fail("An error occurred while approving the transaction"));
        }
    }

    /// <summary>
    /// Process a transaction
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processed transaction</returns>
    [HttpPost("{id}/process")]
    [ProducesResponseType(typeof(ApiResponse<InventoryTransactionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<InventoryTransactionDto>>> Process(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing transaction {Id}", id);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _transactionService.ProcessTransactionAsync(id, userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully processed transaction {Id}", id);
                return Ok(result);
            }

            _logger.LogWarning("Failed to process transaction {Id}: {Error}", id, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transaction {Id}", id);
            return StatusCode(500, ApiResponse.Fail("An error occurred while processing the transaction"));
        }
    }

    /// <summary>
    /// Cancel a transaction
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="request">Cancellation request with reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancelled transaction</returns>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<InventoryTransactionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<InventoryTransactionDto>>> Cancel(
        Guid id,
        [FromBody] CancelTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Cancelling transaction {Id} with reason: {Reason}", id, request.Reason);

            // Get current user ID from claims
            var userId = GetCurrentUserId();

            var result = await _transactionService.CancelTransactionAsync(id, request.Reason, userId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully cancelled transaction {Id}", id);
                return Ok(result);
            }

            _logger.LogWarning("Failed to cancel transaction {Id}: {Error}", id, result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling transaction {Id}", id);
            return StatusCode(500, ApiResponse.Fail("An error occurred while cancelling the transaction"));
        }
    }

    /// <summary>
    /// Get transaction summary statistics
    /// </summary>
    /// <param name="startDate">Start date for the summary</param>
    /// <param name="endDate">End date for the summary</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction summary statistics</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<TransactionSummaryDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<TransactionSummaryDto>>> GetSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting transaction summary from {StartDate} to {EndDate}", startDate, endDate);

            var result = await _transactionService.GetTransactionSummaryAsync(startDate, endDate, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            _logger.LogWarning("Failed to get transaction summary: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction summary");
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving transaction summary"));
        }
    }

    /// <summary>
    /// Get pending transactions for approval
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of pending transactions</returns>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InventoryTransactionDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryTransactionDto>>>> GetPending(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting pending transactions");

            var result = await _transactionService.GetPendingTransactionsAsync(cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            _logger.LogWarning("Failed to get pending transactions: {Error}", result.Error);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending transactions");
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving pending transactions"));
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
/// Request model for transaction cancellation
/// </summary>
public class CancelTransactionRequest
{
    /// <summary>
    /// Reason for cancellation
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
