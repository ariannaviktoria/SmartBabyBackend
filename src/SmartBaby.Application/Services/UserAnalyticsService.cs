using Microsoft.Extensions.Logging;
using SmartBaby.Core.DTOs;
using SmartBaby.Core.Entities;
using SmartBaby.Core.Interfaces;
using SmartBaby.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace SmartBaby.Application.Services;

/// <summary>
/// Service for comprehensive user analytics and statistics
/// </summary>
public class UserAnalyticsService : IUserAnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;
    private readonly ILogger<UserAnalyticsService> _logger;

    public UserAnalyticsService(
        ApplicationDbContext context,
        IUserService userService,
        ILogger<UserAnalyticsService> logger)
    {
        _context = context;
        _userService = userService;
        _logger = logger;
    }

    public async Task<UserAnalyticsComparisonDto> CompareUserAnalyticsAsync(
        string userId, 
        DateTime currentFrom, 
        DateTime currentTo, 
        DateTime previousFrom, 
        DateTime previousTo, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentPeriod = await GetAnalyticsForPeriodAsync(userId, currentFrom, currentTo, cancellationToken);
            var previousPeriod = await GetAnalyticsForPeriodAsync(userId, previousFrom, previousTo, cancellationToken);

            var changes = new UserAnalyticsChangesDto
            {
                AnalysesChange = currentPeriod.TotalAnalyses - previousPeriod.TotalAnalyses,
                SuccessRateChange = currentPeriod.SuccessRate - previousPeriod.SuccessRate,
                ConfidenceChange = currentPeriod.AverageConfidence - previousPeriod.AverageConfidence,
                RealtimeSessionsChange = currentPeriod.RealtimeSessions - previousPeriod.RealtimeSessions,
                AlertsChange = currentPeriod.AlertsGenerated - previousPeriod.AlertsGenerated
            };

            // Calculate percentage changes
            if (previousPeriod.TotalAnalyses > 0)
            {
                changes.AnalysesChangePercent = (float)changes.AnalysesChange / previousPeriod.TotalAnalyses * 100;
            }

            if (previousPeriod.RealtimeSessions > 0)
            {
                changes.RealtimeSessionsChangePercent = (float)changes.RealtimeSessionsChange / previousPeriod.RealtimeSessions * 100;
            }

            if (previousPeriod.AlertsGenerated > 0)
            {
                changes.AlertsChangePercent = (float)changes.AlertsChange / previousPeriod.AlertsGenerated * 100;
            }

            return new UserAnalyticsComparisonDto
            {
                CurrentPeriod = currentPeriod,
                PreviousPeriod = previousPeriod,
                Changes = changes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing analytics for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<BabyAnalyticsSummaryDto>> GetUserBabiesAnalyticsAsync(string userId, UserAnalyticsFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var babies = await _context.Babies
                .Where(b => b.UserId == userId)
                .ToListAsync(cancellationToken);

            filter ??= new UserAnalyticsFilterDto();
            var from = filter.From ?? DateTime.UtcNow.AddYears(-1);
            var to = filter.To ?? DateTime.UtcNow;

            return await GetBabyAnalyticsSummariesAsync(babies, from, to, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting babies analytics for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<RecentActivityDto>> GetUserRecentActivitiesAsync(string userId, int limit = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var babies = await _context.Babies
                .Where(b => b.UserId == userId)
                .ToListAsync(cancellationToken);

            var babyIds = babies.Select(b => b.Id).ToList();
            var activities = new List<RecentActivityDto>();

            // Get recent analyses
            var recentAnalyses = await _context.BabyAnalyses
                .Where(a => babyIds.Contains(a.BabyId))
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .Include(a => a.Baby)
                .ToListAsync(cancellationToken);

            foreach (var analysis in recentAnalyses)
            {
                activities.Add(new RecentActivityDto
                {
                    Timestamp = analysis.CreatedAt,
                    ActivityType = "analysis",
                    Description = $"{analysis.AnalysisType} analysis completed",
                    BabyId = analysis.BabyId,
                    BabyName = analysis.Baby.Name,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["success"] = analysis.Success,
                        ["confidence"] = analysis.Confidence ?? 0,
                        ["primaryResult"] = analysis.PrimaryResult ?? string.Empty
                    }
                });
            }

            // Get recent realtime sessions
            var recentSessions = await _context.RealtimeAnalysisSessions
                .Where(rs => babyIds.Contains(rs.BabyId))
                .OrderByDescending(rs => rs.CreatedAt)
                .Take(10)
                .Include(rs => rs.Baby)
                .ToListAsync(cancellationToken);

            foreach (var session in recentSessions)
            {
                activities.Add(new RecentActivityDto
                {
                    Timestamp = session.CreatedAt,
                    ActivityType = "realtime_session",
                    Description = $"Realtime session {session.Status.ToString().ToLower()}",
                    BabyId = session.BabyId,
                    BabyName = session.Baby.Name,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["status"] = session.Status.ToString(),
                        ["sessionId"] = session.SessionId,
                        ["updateCount"] = session.UpdateCount
                    }
                });
            }

            // Get recent alerts
            var recentAlerts = await _context.AnalysisAlerts
                .Where(a => babyIds.Contains(a.BabyId))
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Include(a => a.Baby)
                .ToListAsync(cancellationToken);

            foreach (var alert in recentAlerts)
            {
                activities.Add(new RecentActivityDto
                {
                    Timestamp = alert.CreatedAt,
                    ActivityType = "alert",
                    Description = alert.Message,
                    BabyId = alert.BabyId,
                    BabyName = alert.Baby.Name,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["alertType"] = alert.AlertType,
                        ["severity"] = alert.Severity.ToString(),
                        ["isRead"] = alert.IsRead
                    }
                });
            }

            return activities.OrderByDescending(a => a.Timestamp).Take(limit).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent activities for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserTrendsDto> GetUserTrendsAsync(string userId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        try
        {
            from ??= DateTime.UtcNow.AddYears(-1);
            to ??= DateTime.UtcNow;

            var babies = await _context.Babies
                .Where(b => b.UserId == userId)
                .ToListAsync(cancellationToken);

            var babyIds = babies.Select(b => b.Id).ToList();

            var analyses = await _context.BabyAnalyses
                .Where(a => babyIds.Contains(a.BabyId) && a.CreatedAt >= from && a.CreatedAt <= to)
                .ToListAsync(cancellationToken);

            var trends = new UserTrendsDto();

            // Analysis volume by month
            trends.AnalysisVolumeByMonth = analyses
                .GroupBy(a => a.CreatedAt.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => (float)g.Count());

            // Success rate by month
            trends.SuccessRateByMonth = analyses
                .GroupBy(a => a.CreatedAt.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Count(a => a.Success) / (float)g.Count() * 100);

            // Confidence by month
            trends.ConfidenceByMonth = analyses
                .Where(a => a.Success && a.Confidence.HasValue)
                .GroupBy(a => a.CreatedAt.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Average(a => a.Confidence!.Value));

            // Most active times
            var hourlyDistribution = analyses.GroupBy(a => a.CreatedAt.Hour)
                .ToDictionary(g => g.Key, g => g.Count());
            trends.MostActiveHour = hourlyDistribution.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;

            var dailyDistribution = analyses.GroupBy(a => a.CreatedAt.DayOfWeek.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
            trends.MostActiveDay = dailyDistribution.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key ?? "Monday";

            // Engagement metrics
            var totalDays = (to.Value - from.Value).TotalDays;
            trends.AverageAnalysesPerDay = (float)(analyses.Count / Math.Max(totalDays, 1));
            trends.AverageAnalysesPerWeek = trends.AverageAnalysesPerDay * 7;

            var activeDays = analyses.GroupBy(a => a.CreatedAt.Date).Count();
            trends.TotalActiveDays = activeDays;

            // Calculate consecutive active days
            trends.ConsecutiveActiveDays = CalculateConsecutiveActiveDays(analyses);

            // Feature usage
            trends.FeatureUsageFrequency = CalculateFeatureUsage(analyses);
            trends.MostUsedFeatures = trends.FeatureUsageFrequency
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .Select(kvp => kvp.Key)
                .ToList();

            // Baby mood trends
            trends.BabyMoodTrends = await CalculateBabyMoodTrends(babyIds, from.Value, to.Value, cancellationToken);

            // Common cry reasons
            trends.CommonCryReasons = await CalculateCommonCryReasons(babyIds, from.Value, to.Value, cancellationToken);

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trends for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<BabyCurrentStateDto>> GetBabiesCurrentStateAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var babies = await _context.Babies
                .Where(b => b.UserId == userId)
                .ToListAsync(cancellationToken);

            var result = new List<BabyCurrentStateDto>();

            foreach (var baby in babies)
            {
                // Get last analysis
                var lastAnalysis = await _context.BabyAnalyses
                    .Where(a => a.BabyId == baby.Id && a.Success)
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                // Check for unread alerts
                var hasUnreadAlerts = await _context.AnalysisAlerts
                    .AnyAsync(a => a.BabyId == baby.Id && !a.IsRead, cancellationToken);

                // Check for active realtime session
                var activeSession = await _context.RealtimeAnalysisSessions
                    .Where(rs => rs.BabyId == baby.Id && rs.Status == RealtimeSessionStatus.Active)
                    .FirstOrDefaultAsync(cancellationToken);

                result.Add(new BabyCurrentStateDto
                {
                    BabyId = baby.Id,
                    BabyName = baby.Name,
                    CurrentMood = ExtractMoodFromAnalysis(lastAnalysis),
                    LastAnalysisAt = lastAnalysis?.CreatedAt,
                    HasUnreadAlerts = hasUnreadAlerts,
                    IsInRealtimeSession = activeSession != null,
                    RealtimeSessionStatus = activeSession?.Status.ToString()
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting babies current state for user {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetUnreadAlertsCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var babyIds = await _context.Babies
                .Where(b => b.UserId == userId)
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);

            return await _context.AnalysisAlerts
                .CountAsync(a => babyIds.Contains(a.BabyId) && !a.IsRead, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread alerts count for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<string>> GetActiveRealtimeSessionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var babyIds = await _context.Babies
                .Where(b => b.UserId == userId)
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);

            return await _context.RealtimeAnalysisSessions
                .Where(rs => babyIds.Contains(rs.BabyId) && rs.Status == RealtimeSessionStatus.Active)
                .Select(rs => rs.SessionId)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active realtime sessions for user {UserId}", userId);
            throw;
        }
    }

    public async Task<float> CalculateUserEngagementScoreAsync(string userId, int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var from = DateTime.UtcNow.AddDays(-days);
            var babies = await _context.Babies
                .Where(b => b.UserId == userId)
                .ToListAsync(cancellationToken);

            var babyIds = babies.Select(b => b.Id).ToList();

            // Get activity counts
            var analysesCount = await _context.BabyAnalyses
                .CountAsync(a => babyIds.Contains(a.BabyId) && a.CreatedAt >= from, cancellationToken);

            var realtimeSessionsCount = await _context.RealtimeAnalysisSessions
                .CountAsync(rs => babyIds.Contains(rs.BabyId) && rs.CreatedAt >= from, cancellationToken);

            var activeDays = await _context.BabyAnalyses
                .Where(a => babyIds.Contains(a.BabyId) && a.CreatedAt >= from)
                .Select(a => a.CreatedAt.Date)
                .Distinct()
                .CountAsync(cancellationToken);

            // Calculate engagement score (0-100)
            var dailyAnalysisScore = Math.Min((float)analysesCount / days * 10, 40); // Max 40 points
            var sessionScore = Math.Min(realtimeSessionsCount * 5, 30); // Max 30 points
            var consistencyScore = Math.Min((float)activeDays / days * 30, 30); // Max 30 points

            var totalScore = dailyAnalysisScore + sessionScore + consistencyScore;
            return Math.Min(totalScore, 100);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating engagement score for user {UserId}", userId);
            throw;
        }
    }

    #region Private Helper Methods

    private async Task<List<BabyAnalyticsSummaryDto>> GetBabyAnalyticsSummariesAsync(List<Baby> babies, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var summaries = new List<BabyAnalyticsSummaryDto>();

        foreach (var baby in babies)
        {
            var analyses = await _context.BabyAnalyses
                .Where(a => a.BabyId == baby.Id && a.CreatedAt >= from && a.CreatedAt <= to)
                .ToListAsync(cancellationToken);

            var successfulAnalyses = analyses.Where(a => a.Success).ToList();

            // Get last mood analysis
            var lastMoodAnalysis = analyses
                .Where(a => a.Success && (a.AnalysisType == "image" || a.AnalysisType == "video") && !string.IsNullOrEmpty(a.PrimaryResult))
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault();

            // Get activity counts
            var sleepPeriods = await _context.SleepPeriods
                .CountAsync(s => s.BabyId == baby.Id && s.StartTime >= from && s.StartTime <= to, cancellationToken);

            var feedings = await _context.Feedings
                .CountAsync(f => f.BabyId == baby.Id && f.Time >= from && f.Time <= to, cancellationToken);

            var cryingEpisodes = await _context.Cryings
                .CountAsync(c => c.BabyId == baby.Id && c.StartTime >= from && c.StartTime <= to, cancellationToken);

            var notes = await _context.Notes
                .CountAsync(n => n.BabyId == baby.Id && n.CreatedAt >= from && n.CreatedAt <= to, cancellationToken);

            // Get realtime sessions
            var realtimeSessions = await _context.RealtimeAnalysisSessions
                .Where(rs => rs.BabyId == baby.Id && rs.CreatedAt >= from && rs.CreatedAt <= to)
                .ToListAsync(cancellationToken);

            // Get alerts
            var alerts = await _context.AnalysisAlerts
                .Where(a => a.BabyId == baby.Id && a.CreatedAt >= from && a.CreatedAt <= to)
                .ToListAsync(cancellationToken);

            summaries.Add(new BabyAnalyticsSummaryDto
            {
                BabyId = baby.Id,
                BabyName = baby.Name,
                BabyDateOfBirth = baby.DateOfBirth,
                AgeInDays = (int)(DateTime.UtcNow - baby.DateOfBirth).TotalDays,
                TotalAnalyses = analyses.Count,
                SuccessfulAnalyses = successfulAnalyses.Count,
                SuccessRate = analyses.Count > 0 ? (float)successfulAnalyses.Count / analyses.Count * 100 : 0,
                AnalysisTypeDistribution = analyses.GroupBy(a => a.AnalysisType).ToDictionary(g => g.Key, g => g.Count()),
                AverageConfidence = successfulAnalyses.Where(a => a.Confidence.HasValue)
                    .Select(a => a.Confidence!.Value)
                    .DefaultIfEmpty(0)
                    .Average(),
                LastDetectedMood = lastMoodAnalysis?.PrimaryResult,
                LastMoodAnalysisAt = lastMoodAnalysis?.CreatedAt,
                LastMoodConfidence = lastMoodAnalysis?.Confidence,
                TotalSleepPeriods = sleepPeriods,
                TotalFeedings = feedings,
                TotalCryingEpisodes = cryingEpisodes,
                TotalNotes = notes,
                RealtimeSessions = realtimeSessions.Count,
                LastRealtimeSessionAt = realtimeSessions.OrderByDescending(rs => rs.CreatedAt).FirstOrDefault()?.CreatedAt,
                TotalAlerts = alerts.Count,
                UnreadAlerts = alerts.Count(a => !a.IsRead),
                FirstAnalysisAt = analyses.MinBy(a => a.CreatedAt)?.CreatedAt,
                LastAnalysisAt = analyses.MaxBy(a => a.CreatedAt)?.CreatedAt
            });
        }

        return summaries;
    }

    private async Task<UserAnalyticsPeriodDto> GetAnalyticsForPeriodAsync(string userId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var babyIds = await _context.Babies
            .Where(b => b.UserId == userId)
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);

        var analyses = await _context.BabyAnalyses
            .Where(a => babyIds.Contains(a.BabyId) && a.CreatedAt >= from && a.CreatedAt <= to)
            .ToListAsync(cancellationToken);

        var realtimeSessions = await _context.RealtimeAnalysisSessions
            .CountAsync(rs => babyIds.Contains(rs.BabyId) && rs.CreatedAt >= from && rs.CreatedAt <= to, cancellationToken);

        var alerts = await _context.AnalysisAlerts
            .CountAsync(a => babyIds.Contains(a.BabyId) && a.CreatedAt >= from && a.CreatedAt <= to, cancellationToken);

        var successfulAnalyses = analyses.Where(a => a.Success).ToList();

        return new UserAnalyticsPeriodDto
        {
            From = from,
            To = to,
            TotalAnalyses = analyses.Count,
            SuccessRate = analyses.Count > 0 ? (float)successfulAnalyses.Count / analyses.Count * 100 : 0,
            AverageConfidence = successfulAnalyses.Where(a => a.Confidence.HasValue)
                .Select(a => a.Confidence!.Value)
                .DefaultIfEmpty(0)
                .Average(),
            RealtimeSessions = realtimeSessions,
            AlertsGenerated = alerts,
            AnalysisTypeDistribution = analyses.GroupBy(a => a.AnalysisType).ToDictionary(g => g.Key, g => g.Count())
        };
    }

    private TimeSpan CalculateTotalSessionDuration(List<RealtimeAnalysisSession> sessions)
    {
        var totalDuration = TimeSpan.Zero;
        foreach (var session in sessions)
        {
            if (session.StartedAt.HasValue)
            {
                var endTime = session.StoppedAt ?? DateTime.UtcNow;
                var duration = endTime - session.StartedAt.Value;
                if (duration > TimeSpan.Zero)
                {
                    totalDuration = totalDuration.Add(duration);
                }
            }
        }
        return totalDuration;
    }

    private Dictionary<string, int> CalculateDailyUsagePattern(List<BabyAnalysis> analyses)
    {
        return analyses.GroupBy(a => a.CreatedAt.Hour.ToString("D2"))
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private Dictionary<string, int> CalculateWeeklyUsagePattern(List<BabyAnalysis> analyses)
    {
        return analyses.GroupBy(a => a.CreatedAt.DayOfWeek.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private Dictionary<string, int> CalculateMonthlyUsagePattern(List<BabyAnalysis> analyses)
    {
        return analyses.GroupBy(a => a.CreatedAt.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private int CalculateConsecutiveActiveDays(List<BabyAnalysis> analyses)
    {
        if (!analyses.Any()) return 0;

        var activeDays = analyses.GroupBy(a => a.CreatedAt.Date)
            .Select(g => g.Key)
            .OrderByDescending(d => d)
            .ToList();

        int consecutive = 1;
        for (int i = 1; i < activeDays.Count; i++)
        {
            if (activeDays[i-1].AddDays(-1) == activeDays[i])
            {
                consecutive++;
            }
            else
            {
                break;
            }
        }

        return consecutive;
    }

    private Dictionary<string, float> CalculateFeatureUsage(List<BabyAnalysis> analyses)
    {
        var total = analyses.Count;
        if (total == 0) return new Dictionary<string, float>();

        return analyses.GroupBy(a => a.AnalysisType)
            .ToDictionary(g => g.Key, g => (float)g.Count() / total * 100);
    }

    private async Task<Dictionary<string, List<string>>> CalculateBabyMoodTrends(List<int> babyIds, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var trends = new Dictionary<string, List<string>>();

        foreach (var babyId in babyIds)
        {
            var moodAnalyses = await _context.BabyAnalyses
                .Where(a => a.BabyId == babyId && 
                           a.CreatedAt >= from && 
                           a.CreatedAt <= to &&
                           a.Success &&
                           (a.AnalysisType == "image" || a.AnalysisType == "video") &&
                           !string.IsNullOrEmpty(a.PrimaryResult))
                .OrderBy(a => a.CreatedAt)
                .Select(a => a.PrimaryResult!)
                .ToListAsync(cancellationToken);

            trends[babyId.ToString()] = moodAnalyses;
        }

        return trends;
    }

    private async Task<Dictionary<string, int>> CalculateCommonCryReasons(List<int> babyIds, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var cryAnalyses = await _context.BabyAnalyses
            .Where(a => babyIds.Contains(a.BabyId) && 
                       a.CreatedAt >= from && 
                       a.CreatedAt <= to &&
                       a.Success &&
                       a.AnalysisType == "audio" &&
                       !string.IsNullOrEmpty(a.PrimaryResult))
            .Select(a => a.PrimaryResult!)
            .ToListAsync(cancellationToken);

        return cryAnalyses.GroupBy(r => r)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private string? ExtractMoodFromAnalysis(BabyAnalysis? analysis)
    {
        if (analysis == null || !analysis.Success || string.IsNullOrEmpty(analysis.ResultData))
            return null;

        try
        {
            var resultData = JsonDocument.Parse(analysis.ResultData);
            if (resultData.RootElement.TryGetProperty("EmotionAnalysis", out var emotionAnalysis) &&
                emotionAnalysis.TryGetProperty("DetectedMood", out var mood))
            {
                return mood.GetString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting mood from analysis {AnalysisId}", analysis.Id);
        }

        return analysis.PrimaryResult;
    }

    #endregion
}
