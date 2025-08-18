namespace SmartBaby.Core.DTOs;

/// <summary>
/// Summary analytics for each baby
/// </summary>
public class BabyAnalyticsSummaryDto
{
    public int BabyId { get; set; }
    public string BabyName { get; set; } = string.Empty;
    public DateTime BabyDateOfBirth { get; set; }
    public int AgeInDays { get; set; }
    
    // Analysis statistics for this baby
    public int TotalAnalyses { get; set; }
    public int SuccessfulAnalyses { get; set; }
    public float SuccessRate { get; set; }
    public Dictionary<string, int> AnalysisTypeDistribution { get; set; } = new();
    public float AverageConfidence { get; set; }
    
    // Recent mood and state
    public string? LastDetectedMood { get; set; }
    public DateTime? LastMoodAnalysisAt { get; set; }
    public float? LastMoodConfidence { get; set; }
    
    // Activity tracking
    public int TotalSleepPeriods { get; set; }
    public int TotalFeedings { get; set; }
    public int TotalCryingEpisodes { get; set; }
    public int TotalNotes { get; set; }
    
    // Real-time sessions for this baby
    public int RealtimeSessions { get; set; }
    public DateTime? LastRealtimeSessionAt { get; set; }
    
    // Alerts for this baby
    public int TotalAlerts { get; set; }
    public int UnreadAlerts { get; set; }
    
    // Time periods
    public DateTime? FirstAnalysisAt { get; set; }
    public DateTime? LastAnalysisAt { get; set; }
}

/// <summary>
/// Recent user activity summary
/// </summary>
public class RecentActivityDto
{
    public DateTime Timestamp { get; set; }
    public string ActivityType { get; set; } = string.Empty; // "analysis", "realtime_session", "baby_created", etc.
    public string Description { get; set; } = string.Empty;
    public int? BabyId { get; set; }
    public string? BabyName { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

/// <summary>
/// User behavior trends and insights
/// </summary>
public class UserTrendsDto
{
    // Analysis trends over time
    public Dictionary<string, float> AnalysisVolumeByMonth { get; set; } = new(); // Month -> analysis count
    public Dictionary<string, float> SuccessRateByMonth { get; set; } = new(); // Month -> success rate
    public Dictionary<string, float> ConfidenceByMonth { get; set; } = new(); // Month -> avg confidence
    
    // Most active times
    public int MostActiveHour { get; set; }
    public string MostActiveDay { get; set; } = string.Empty;
    
    // Engagement metrics
    public float AverageAnalysesPerDay { get; set; }
    public float AverageAnalysesPerWeek { get; set; }
    public int ConsecutiveActiveDays { get; set; }
    public int TotalActiveDays { get; set; }
    
    // Feature usage
    public Dictionary<string, float> FeatureUsageFrequency { get; set; } = new(); // Feature -> usage %
    public List<string> MostUsedFeatures { get; set; } = new();
    public List<string> UnusedFeatures { get; set; } = new();
    
    // Baby development insights
    public Dictionary<string, List<string>> BabyMoodTrends { get; set; } = new(); // BabyId -> mood trend
    public Dictionary<string, int> CommonCryReasons { get; set; } = new(); // Cry reason -> count
}

/// <summary>
/// Filter for user analytics queries
/// </summary>
public class UserAnalyticsFilterDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? BabyId { get; set; } // Filter for specific baby
    public bool IncludeRecentActivities { get; set; } = true;
    public int RecentActivitiesLimit { get; set; } = 10;
    public bool IncludeTrends { get; set; } = true;
    public bool IncludeBabyDetails { get; set; } = true;
}

/// <summary>
/// Quick user dashboard statistics
/// </summary>
public class UserDashboardDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    
    // Quick stats
    public int TotalBabies { get; set; }
    public int TotalAnalysesToday { get; set; }
    public int TotalAnalysesThisWeek { get; set; }
    public int TotalAnalysesThisMonth { get; set; }
    public int UnreadAlerts { get; set; }
    public int ActiveRealtimeSessions { get; set; }
    
    // Recent activity summary
    public DateTime? LastActivityAt { get; set; }
    public string? LastActivityDescription { get; set; }
    
    // Current baby states
    public List<BabyCurrentStateDto> BabyStates { get; set; } = new();
}

/// <summary>
/// Current state of a baby
/// </summary>
public class BabyCurrentStateDto
{
    public int BabyId { get; set; }
    public string BabyName { get; set; } = string.Empty;
    public string? CurrentMood { get; set; }
    public DateTime? LastAnalysisAt { get; set; }
    public bool HasUnreadAlerts { get; set; }
    public bool IsInRealtimeSession { get; set; }
    public string? RealtimeSessionStatus { get; set; }
}

/// <summary>
/// User analytics comparison between periods
/// </summary>
public class UserAnalyticsComparisonDto
{
    public UserAnalyticsPeriodDto CurrentPeriod { get; set; } = new();
    public UserAnalyticsPeriodDto PreviousPeriod { get; set; } = new();
    public UserAnalyticsChangesDto Changes { get; set; } = new();
}

/// <summary>
/// Analytics for a specific time period
/// </summary>
public class UserAnalyticsPeriodDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int TotalAnalyses { get; set; }
    public float SuccessRate { get; set; }
    public float AverageConfidence { get; set; }
    public int RealtimeSessions { get; set; }
    public int AlertsGenerated { get; set; }
    public Dictionary<string, int> AnalysisTypeDistribution { get; set; } = new();
}

/// <summary>
/// Changes between analytics periods
/// </summary>
public class UserAnalyticsChangesDto
{
    public int AnalysesChange { get; set; }
    public float AnalysesChangePercent { get; set; }
    public float SuccessRateChange { get; set; }
    public float ConfidenceChange { get; set; }
    public int RealtimeSessionsChange { get; set; }
    public float RealtimeSessionsChangePercent { get; set; }
    public int AlertsChange { get; set; }
    public float AlertsChangePercent { get; set; }
}
