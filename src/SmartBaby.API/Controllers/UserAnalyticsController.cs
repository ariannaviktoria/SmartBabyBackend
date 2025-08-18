using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBaby.Core.DTOs;
using SmartBaby.Core.Interfaces;
using System.Security.Claims;

namespace SmartBaby.API.Controllers;

/// <summary>
/// User analytics controller providing comprehensive user analysis and statistics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserAnalyticsController : ControllerBase
{
    private readonly IUserAnalyticsService _userAnalyticsService;
    private readonly ILogger<UserAnalyticsController> _logger;

    public UserAnalyticsController(
        IUserAnalyticsService userAnalyticsService,
        ILogger<UserAnalyticsController> logger)
    {
        _userAnalyticsService = userAnalyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Compare user analytics between two time periods
    /// </summary>
    /// <param name="request">Comparison request with time periods</param>
    /// <returns>Analytics comparison data</returns>
    /// <response code="200">Returns analytics comparison</response>
    /// <response code="400">Invalid time periods</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("compare")]
    [ProducesResponseType(typeof(UserAnalyticsComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CompareAnalytics([FromBody] AnalyticsComparisonRequestDto request)
    {
        try
        {
            if (request.CurrentFrom >= request.CurrentTo || request.PreviousFrom >= request.PreviousTo)
            {
                return BadRequest("Invalid time periods: 'From' date must be before 'To' date");
            }

            var userId = GetCurrentUserId();
            var comparison = await _userAnalyticsService.CompareUserAnalyticsAsync(
                userId, 
                request.CurrentFrom, 
                request.CurrentTo, 
                request.PreviousFrom, 
                request.PreviousTo);
            
            _logger.LogInformation("Compared analytics for user {UserId} between {CurrentPeriod} and {PreviousPeriod}", 
                userId, 
                $"{request.CurrentFrom:yyyy-MM-dd} to {request.CurrentTo:yyyy-MM-dd}",
                $"{request.PreviousFrom:yyyy-MM-dd} to {request.PreviousTo:yyyy-MM-dd}");
            
            return Ok(comparison);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing analytics for user");
            return StatusCode(500, "An error occurred while comparing analytics");
        }
    }

    /// <summary>
    /// Get analytics for all user's babies
    /// </summary>
    /// <param name="filter">Optional filter parameters</param>
    /// <returns>Analytics for all babies</returns>
    /// <response code="200">Returns babies analytics</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("babies")]
    [ProducesResponseType(typeof(List<BabyAnalyticsSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBabiesAnalytics([FromBody] UserAnalyticsFilterDto? filter = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var babiesAnalytics = await _userAnalyticsService.GetUserBabiesAnalyticsAsync(userId, filter);
            
            _logger.LogInformation("Retrieved analytics for {BabyCount} babies for user {UserId}", 
                babiesAnalytics.Count, userId);
            
            return Ok(babiesAnalytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting babies analytics for user");
            return StatusCode(500, "An error occurred while retrieving babies analytics");
        }
    }

    /// <summary>
    /// Get user's recent activities
    /// </summary>
    /// <param name="limit">Maximum number of activities to return (default: 20, max: 100)</param>
    /// <returns>Recent user activities</returns>
    /// <response code="200">Returns recent activities</response>
    /// <response code="400">Invalid limit parameter</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("activities")]
    [ProducesResponseType(typeof(List<RecentActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecentActivities([FromQuery] int limit = 20)
    {
        try
        {
            if (limit <= 0 || limit > 100)
            {
                return BadRequest("Limit must be between 1 and 100");
            }

            var userId = GetCurrentUserId();
            var activities = await _userAnalyticsService.GetUserRecentActivitiesAsync(userId, limit);
            
            _logger.LogInformation("Retrieved {ActivityCount} recent activities for user {UserId}", 
                activities.Count, userId);
            
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent activities for user");
            return StatusCode(500, "An error occurred while retrieving recent activities");
        }
    }

    /// <summary>
    /// Get user trends and insights
    /// </summary>
    /// <param name="from">Start date for trend analysis (optional)</param>
    /// <param name="to">End date for trend analysis (optional)</param>
    /// <returns>User trends and insights</returns>
    /// <response code="200">Returns user trends</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(UserTrendsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTrends([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var trends = await _userAnalyticsService.GetUserTrendsAsync(userId, from, to);
            
            _logger.LogInformation("Retrieved trends for user {UserId} from {From} to {To}", 
                userId, from?.ToString("yyyy-MM-dd") ?? "beginning", to?.ToString("yyyy-MM-dd") ?? "now");
            
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trends for user");
            return StatusCode(500, "An error occurred while retrieving user trends");
        }
    }

    /// <summary>
    /// Get current state of all user's babies
    /// </summary>
    /// <returns>Current state of all babies</returns>
    /// <response code="200">Returns babies current state</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("babies/status")]
    [ProducesResponseType(typeof(List<BabyCurrentStateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBabiesCurrentState()
    {
        try
        {
            var userId = GetCurrentUserId();
            var babiesState = await _userAnalyticsService.GetBabiesCurrentStateAsync(userId);
            
            _logger.LogInformation("Retrieved current state for {BabyCount} babies for user {UserId}", 
                babiesState.Count, userId);
            
            return Ok(babiesState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting babies current state for user");
            return StatusCode(500, "An error occurred while retrieving babies current state");
        }
    }

    /// <summary>
    /// Get count of unread alerts for the user
    /// </summary>
    /// <returns>Number of unread alerts</returns>
    /// <response code="200">Returns unread alerts count</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("alerts/unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadAlertsCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _userAnalyticsService.GetUnreadAlertsCountAsync(userId);
            
            _logger.LogInformation("User {UserId} has {UnreadCount} unread alerts", userId, count);
            
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread alerts count for user");
            return StatusCode(500, "An error occurred while retrieving unread alerts count");
        }
    }

    /// <summary>
    /// Get list of active realtime session IDs for the user
    /// </summary>
    /// <returns>List of active session IDs</returns>
    /// <response code="200">Returns active session IDs</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("sessions/active")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActiveRealtimeSessions()
    {
        try
        {
            var userId = GetCurrentUserId();
            var sessions = await _userAnalyticsService.GetActiveRealtimeSessionsAsync(userId);
            
            _logger.LogInformation("User {UserId} has {SessionCount} active realtime sessions", userId, sessions.Count);
            
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active realtime sessions for user");
            return StatusCode(500, "An error occurred while retrieving active sessions");
        }
    }

    /// <summary>
    /// Calculate user engagement score based on activity over specified days
    /// </summary>
    /// <param name="days">Number of days to analyze (default: 30, max: 365)</param>
    /// <returns>Engagement score (0-100)</returns>
    /// <response code="200">Returns engagement score</response>
    /// <response code="400">Invalid days parameter</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("engagement-score")]
    [ProducesResponseType(typeof(float), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEngagementScore([FromQuery] int days = 30)
    {
        try
        {
            if (days <= 0 || days > 365)
            {
                return BadRequest("Days must be between 1 and 365");
            }

            var userId = GetCurrentUserId();
            var score = await _userAnalyticsService.CalculateUserEngagementScoreAsync(userId, days);
            
            _logger.LogInformation("User {UserId} has engagement score {Score} over {Days} days", userId, score, days);
            
            return Ok(score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating engagement score for user");
            return StatusCode(500, "An error occurred while calculating engagement score");
        }
    }

    /// <summary>
    /// Get analytics summary for a specific time period
    /// </summary>
    /// <param name="request">Time period request</param>
    /// <returns>Analytics summary for the specified period</returns>
    /// <response code="200">Returns period analytics</response>
    /// <response code="400">Invalid time period</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("period-summary")]
    [ProducesResponseType(typeof(UserAnalyticsPeriodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPeriodSummary([FromBody] PeriodSummaryRequestDto request)
    {
        try
        {
            if (request.From >= request.To)
            {
                return BadRequest("'From' date must be before 'To' date");
            }

            var userId = GetCurrentUserId();
            
            // Use the comparison method with the same period to get a single period summary
            var comparison = await _userAnalyticsService.CompareUserAnalyticsAsync(
                userId, request.From, request.To, request.From, request.To);
            
            _logger.LogInformation("Retrieved period summary for user {UserId} from {From} to {To}", 
                userId, request.From.ToString("yyyy-MM-dd"), request.To.ToString("yyyy-MM-dd"));
            
            return Ok(comparison.CurrentPeriod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting period summary for user");
            return StatusCode(500, "An error occurred while retrieving period summary");
        }
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}

/// <summary>
/// Request DTO for analytics comparison
/// </summary>
public class AnalyticsComparisonRequestDto
{
    public DateTime CurrentFrom { get; set; }
    public DateTime CurrentTo { get; set; }
    public DateTime PreviousFrom { get; set; }
    public DateTime PreviousTo { get; set; }
}

/// <summary>
/// Request DTO for period summary
/// </summary>
public class PeriodSummaryRequestDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}
