using SmartBaby.Core.DTOs;
using SmartBaby.Core.Entities;

namespace SmartBaby.Core.Interfaces;

/// <summary>
/// Interface for Baby Analysis gRPC Service integration
/// </summary>
public interface IBabyAnalysisService
{
    // Image Analysis
    Task<ImageAnalysisResponseDto> AnalyzeImageAsync(ImageAnalysisRequestDto request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ImageAnalysisResponseDto> AnalyzeImageStreamAsync(IAsyncEnumerable<ImageAnalysisRequestDto> requests, CancellationToken cancellationToken = default);
    
    // Audio Analysis
    Task<AudioAnalysisResponseDto> AnalyzeAudioAsync(AudioAnalysisRequestDto request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<AudioAnalysisResponseDto> AnalyzeAudioStreamAsync(IAsyncEnumerable<AudioAnalysisRequestDto> requests, CancellationToken cancellationToken = default);
    
    // Video Analysis
    Task<VideoAnalysisResponseDto> AnalyzeVideoAsync(VideoAnalysisRequestDto request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<VideoAnalysisResponseDto> AnalyzeVideoStreamAsync(IAsyncEnumerable<VideoAnalysisRequestDto> requests, CancellationToken cancellationToken = default);
    
    // Multimodal Analysis
    Task<MultimodalAnalysisResponseDto> AnalyzeMultimodalAsync(MultimodalAnalysisRequestDto request, CancellationToken cancellationToken = default);
    
    // Real-time Analysis
    IAsyncEnumerable<RealtimeAnalysisResponseDto> StartRealtimeAnalysisAsync(RealtimeAnalysisRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> StopRealtimeAnalysisAsync(string sessionId, CancellationToken cancellationToken = default);
    
    // Health and Status
    Task<HealthStatusResponseDto> GetHealthStatusAsync(CancellationToken cancellationToken = default);
    Task<ModelStatusResponseDto> GetModelStatusAsync(CancellationToken cancellationToken = default);
    
    // Analysis History Management
    Task<bool> SaveAnalysisResultAsync(int babyId, string analysisType, object analysisResult, CancellationToken cancellationToken = default);
    Task<IEnumerable<AnalysisHistoryDto>> GetAnalysisHistoryAsync(int babyId, string? analysisType = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<AnalysisHistoryDto?> GetAnalysisResultAsync(int analysisId, CancellationToken cancellationToken = default);
    
    // Batch Analysis
    Task<BatchAnalysisResponseDto> SubmitBatchAnalysisAsync(BatchAnalysisRequestDto request, CancellationToken cancellationToken = default);
    Task<BatchAnalysisStatusDto> GetBatchAnalysisStatusAsync(string batchId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for managing analysis history and persistence
/// </summary>
public interface IAnalysisHistoryService
{
    Task<int> SaveAnalysisAsync(AnalysisHistoryDto analysis, CancellationToken cancellationToken = default);
    Task<AnalysisHistoryDto?> GetAnalysisAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AnalysisHistoryDto>> GetAnalysisHistoryAsync(int babyId, AnalysisHistoryFilter filter, CancellationToken cancellationToken = default);
    Task<bool> DeleteAnalysisAsync(int id, CancellationToken cancellationToken = default);
    Task<AnalysisStatisticsDto> GetAnalysisStatisticsAsync(int babyId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for real-time analysis session management
/// </summary>
public interface IRealtimeSessionService
{
    Task<string> CreateSessionAsync(int babyId, RealtimeSettingsDto settings, CancellationToken cancellationToken = default);
    Task<bool> StopSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<RealtimeSessionDto?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RealtimeSessionDto>> GetActiveSessionsAsync(int? babyId = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateSessionStatusAsync(string sessionId, RealtimeSessionStatus status, CancellationToken cancellationToken = default);
    Task<bool> AddSessionUpdateAsync(string sessionId, RealtimeAnalysisResponseDto update, CancellationToken cancellationToken = default);
    Task<IEnumerable<RealtimeAnalysisUpdate>> GetSessionUpdatesAsync(string sessionId, int? limit = null, CancellationToken cancellationToken = default);
}

// Additional DTOs for extended functionality
public class AnalysisHistoryDto
{
    public int Id { get; set; }
    public int BabyId { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string ResultData { get; set; } = string.Empty; // JSON serialized result
    public float? Confidence { get; set; }
    public string? PrimaryResult { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class AnalysisHistoryFilter
{
    public string? AnalysisType { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool? SuccessOnly { get; set; }
    public float? MinConfidence { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}

public class AnalysisStatisticsDto
{
    public int TotalAnalyses { get; set; }
    public int SuccessfulAnalyses { get; set; }
    public float SuccessRate { get; set; }
    public Dictionary<string, int> AnalysisTypeDistribution { get; set; } = new();
    public Dictionary<string, int> ResultDistribution { get; set; } = new();
    public float AverageConfidence { get; set; }
    public DateTime FirstAnalysis { get; set; }
    public DateTime LastAnalysis { get; set; }
    public Dictionary<string, object> TrendData { get; set; } = new();
}

public class BatchAnalysisRequestDto
{
    public string BatchId { get; set; } = Guid.NewGuid().ToString();
    public int BabyId { get; set; }
    public List<AnalysisItemDto> AnalysisItems { get; set; } = new();
    public string? Description { get; set; }
    public AnalysisOptionsDto? Options { get; set; }
    public bool SaveResults { get; set; } = true;
}

public class AnalysisItemDto
{
    public string ItemId { get; set; } = string.Empty;
    public string AnalysisType { get; set; } = string.Empty; // "image", "audio", "video", "multimodal"
    public byte[]? Data { get; set; }
    public string? FilePath { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class BatchAnalysisResponseDto
{
    public string BatchId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public SmartBaby.Core.DTOs.BatchAnalysisStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public string? EstimatedCompletionTime { get; set; }
}

public class BatchAnalysisStatusDto
{
    public string BatchId { get; set; } = string.Empty;
    public SmartBaby.Core.DTOs.BatchAnalysisStatus Status { get; set; }
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public int SuccessfulItems { get; set; }
    public int FailedItems { get; set; }
    public float ProgressPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<AnalysisItemResultDto> Results { get; set; } = new();
}

public class AnalysisItemResultDto
{
    public string ItemId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string ResultData { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}

public class RealtimeSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public int BabyId { get; set; }
    public RealtimeSessionStatus Status { get; set; }
    public RealtimeSettingsDto Settings { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }
    public int UpdateCount { get; set; }
    public DateTime? LastUpdateAt { get; set; }
    public Dictionary<string, object> Statistics { get; set; } = new();
}

// Enums are defined in SmartBaby.Core.Entities.BabyAnalysisEntities.cs
