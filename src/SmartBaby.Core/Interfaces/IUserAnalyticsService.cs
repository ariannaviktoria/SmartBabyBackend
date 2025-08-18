using SmartBaby.Core.DTOs;

namespace SmartBaby.Core.Interfaces;

/// <summary>
/// Service interface for user analytics and comprehensive user data analysis
/// </summary>
public interface IUserAnalyticsService
{
    /// <summary>
    /// Compare user analytics between two time periods
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="currentFrom">Current period start</param>
    /// <param name="currentTo">Current period end</param>
    /// <param name="previousFrom">Previous period start</param>
    /// <param name="previousTo">Previous period end</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analytics comparison</returns>
    Task<UserAnalyticsComparisonDto> CompareUserAnalyticsAsync(
        string userId, 
        DateTime currentFrom, 
        DateTime currentTo, 
        DateTime previousFrom, 
        DateTime previousTo, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get analytics for all babies of a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="filter">Analytics filter options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analytics for all user's babies</returns>
    Task<List<BabyAnalyticsSummaryDto>> GetUserBabiesAnalyticsAsync(string userId, UserAnalyticsFilterDto? filter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's recent activities
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="limit">Maximum number of activities to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recent user activities</returns>
    Task<List<RecentActivityDto>> GetUserRecentActivitiesAsync(string userId, int limit = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user trends and insights
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="from">Start date for trend analysis</param>
    /// <param name="to">End date for trend analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User trends and insights</returns>
    Task<UserTrendsDto> GetUserTrendsAsync(string userId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current state of all user's babies
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current state of all babies</returns>
    Task<List<BabyCurrentStateDto>> GetBabiesCurrentStateAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's unread alerts count
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of unread alerts</returns>
    Task<int> GetUnreadAlertsCountAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's active realtime sessions
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active realtime session IDs</returns>
    Task<List<string>> GetActiveRealtimeSessionsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate user engagement score
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="days">Number of days to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Engagement score (0-100)</returns>
    Task<float> CalculateUserEngagementScoreAsync(string userId, int days = 30, CancellationToken cancellationToken = default);
}
