using System.ComponentModel.DataAnnotations;

namespace SmartBaby.Core.DTOs;

// Analysis Request DTOs
public class ImageAnalysisRequestDto
{
    public byte[]? ImageBytes { get; set; }
    public string? ImageBase64 { get; set; }
    public string? ImagePath { get; set; }
    public AnalysisOptionsDto? Options { get; set; }
    public string? RequestId { get; set; }
    public int? BabyId { get; set; }
}

public class AudioAnalysisRequestDto
{
    public byte[]? AudioBytes { get; set; }
    public string? AudioBase64 { get; set; }
    public string? AudioPath { get; set; }
    public AudioFormatDto? AudioFormat { get; set; }
    public AnalysisOptionsDto? Options { get; set; }
    public string? RequestId { get; set; }
    public int? BabyId { get; set; }
}

public class VideoAnalysisRequestDto
{
    public byte[]? VideoBytes { get; set; }
    public string? VideoPath { get; set; }
    public VideoAnalysisOptionsDto? Options { get; set; }
    public string? RequestId { get; set; }
    public int? BabyId { get; set; }
}

public class MultimodalAnalysisRequestDto
{
    public ImageAnalysisRequestDto? ImageRequest { get; set; }
    public AudioAnalysisRequestDto? AudioRequest { get; set; }
    public AnalysisOptionsDto? Options { get; set; }
    public string? RequestId { get; set; }
    public int? BabyId { get; set; }
}

public class RealtimeAnalysisRequestDto
{
    public RealtimeSettingsDto Settings { get; set; } = new();
    public string SessionId { get; set; } = string.Empty;
    public int? BabyId { get; set; }
}

// Analysis Response DTOs
public class ImageAnalysisResponseDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public EmotionAnalysisDto? EmotionAnalysis { get; set; }
    public string? RequestId { get; set; }
    public DateTime Timestamp { get; set; }
    public AnalysisMetadataDto? Metadata { get; set; }
}

public class AudioAnalysisResponseDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public CryAnalysisDto? CryAnalysis { get; set; }
    public string? RequestId { get; set; }
    public DateTime Timestamp { get; set; }
    public AnalysisMetadataDto? Metadata { get; set; }
}

public class VideoAnalysisResponseDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public VideoAnalysisResultDto? AnalysisResult { get; set; }
    public string? RequestId { get; set; }
    public DateTime Timestamp { get; set; }
    public AnalysisMetadataDto? Metadata { get; set; }
}

public class MultimodalAnalysisResponseDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public ImageAnalysisResponseDto? ImageResponse { get; set; }
    public AudioAnalysisResponseDto? AudioResponse { get; set; }
    public FusionAnalysisDto? FusionAnalysis { get; set; }
    public string? RequestId { get; set; }
    public DateTime Timestamp { get; set; }
}

public class RealtimeAnalysisResponseDto
{
    public AnalysisUpdateDto Update { get; set; } = new();
    public string SessionId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

// Supporting DTOs
public class AnalysisOptionsDto
{
    public float ConfidenceThreshold { get; set; } = 0.5f;
    public bool IncludeDebugInfo { get; set; }
    public List<string> EnabledModels { get; set; } = new();
    public Dictionary<string, string> CustomParameters { get; set; } = new();
}

public class AudioFormatDto
{
    public int SampleRate { get; set; } = 44100;
    public int Channels { get; set; } = 1;
    public string Encoding { get; set; } = "LINEAR_PCM";
    public int BitDepth { get; set; } = 16;
}

public class VideoAnalysisOptionsDto
{
    public float FrameInterval { get; set; } = 1.0f;
    public float AudioSegmentDuration { get; set; } = 2.0f;
    public bool SaveResults { get; set; }
    public string? OutputDirectory { get; set; }
    public bool EnableFusion { get; set; } = true;
}

public class RealtimeSettingsDto
{
    public int VideoDeviceId { get; set; }
    public AudioFormatDto AudioFormat { get; set; } = new();
    public float FrameAnalysisInterval { get; set; } = 1.0f;
    public float AudioAnalysisDuration { get; set; } = 2.0f;
    public bool EnableVideoDisplay { get; set; }
    public bool EnableOverlay { get; set; }
}

public class EmotionAnalysisDto
{
    public string DetectedMood { get; set; } = string.Empty;
    public string DominantEmotion { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public Dictionary<string, float> AllEmotions { get; set; } = new();
    public string MoodCategory { get; set; } = string.Empty;
}

public class CryAnalysisDto
{
    public bool CryDetected { get; set; }
    public string CryReason { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public Dictionary<string, float> AllPredictions { get; set; } = new();
    public string ModelUsed { get; set; } = string.Empty;
    public AudioFeaturesDto? AudioFeatures { get; set; }
}

public class FusionAnalysisDto
{
    public string OverallState { get; set; } = string.Empty;
    public AlertLevel AlertLevel { get; set; }
    public float Confidence { get; set; }
    public string PrimaryIndicator { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
    public FusionMethod MethodUsed { get; set; }
}

public class VideoAnalysisResultDto
{
    public VideoInfoDto? VideoInfo { get; set; }
    public List<EmotionAnalysisDto> VisualAnalysis { get; set; } = new();
    public List<CryAnalysisDto> AudioAnalysis { get; set; } = new();
    public List<FusionAnalysisDto> FusionAnalysis { get; set; } = new();
    public AnalysisSummaryDto? Summary { get; set; }
}

public class VideoInfoDto
{
    public string FilePath { get; set; } = string.Empty;
    public float Duration { get; set; }
    public float Fps { get; set; }
    public int TotalFrames { get; set; }
    public VideoResolutionDto? Resolution { get; set; }
}

public class VideoResolutionDto
{
    public int Width { get; set; }
    public int Height { get; set; }
}

public class AnalysisSummaryDto
{
    public VisualStatisticsDto? VisualStats { get; set; }
    public AudioStatisticsDto? AudioStats { get; set; }
    public FusionStatisticsDto? FusionStats { get; set; }
    public List<string> KeyFindings { get; set; } = new();
    public string OverallAssessment { get; set; } = string.Empty;
}

public class VisualStatisticsDto
{
    public int TotalFrames { get; set; }
    public int SuccessfulFrames { get; set; }
    public Dictionary<string, int> MoodDistribution { get; set; } = new();
    public string MostCommonMood { get; set; } = string.Empty;
    public float AverageConfidence { get; set; }
}

public class AudioStatisticsDto
{
    public int TotalSegments { get; set; }
    public int SuccessfulSegments { get; set; }
    public float CryPercentage { get; set; }
    public Dictionary<string, int> CryReasonDistribution { get; set; } = new();
    public string MostCommonCryReason { get; set; } = string.Empty;
}

public class FusionStatisticsDto
{
    public int TotalWindows { get; set; }
    public Dictionary<string, int> AlertDistribution { get; set; } = new();
    public float HighAlertPercentage { get; set; }
}

public class AudioFeaturesDto
{
    public float RmsEnergy { get; set; }
    public float DominantFrequency { get; set; }
    public List<float> MfccFeatures { get; set; } = new();
    public float SpectralCentroid { get; set; }
    public float ZeroCrossingRate { get; set; }
}

public class AnalysisMetadataDto
{
    public string AnalyzerVersion { get; set; } = string.Empty;
    public float ProcessingTime { get; set; }
    public string HardwareInfo { get; set; } = string.Empty;
    public List<string> ModelsUsed { get; set; } = new();
}

public class AnalysisUpdateDto
{
    public UpdateType UpdateType { get; set; }
    public EmotionAnalysisDto? EmotionData { get; set; }
    public CryAnalysisDto? CryData { get; set; }
    public FusionAnalysisDto? FusionData { get; set; }
    public SystemStatusDto? SystemStatus { get; set; }
}

public class SystemStatusDto
{
    public float CpuUsage { get; set; }
    public float MemoryUsage { get; set; }
    public int ActiveSessions { get; set; }
    public float UptimeSeconds { get; set; }
}

public class HealthStatusResponseDto
{
    public ServiceHealth OverallHealth { get; set; }
    public List<ComponentHealthDto> ComponentHealth { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
}

public class ComponentHealthDto
{
    public string ComponentName { get; set; } = string.Empty;
    public ServiceHealth HealthStatus { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime LastCheck { get; set; }
}

public class ModelStatusResponseDto
{
    public List<ModelInfoDto> Models { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class ModelInfoDto
{
    public string ModelName { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public ModelStatus Status { get; set; }
    public string Version { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime LoadedAt { get; set; }
}

// Enums
public enum UpdateType
{
    Unspecified = 0,
    EmotionUpdate = 1,
    CryUpdate = 2,
    FusionUpdate = 3,
    ProgressUpdate = 4,
    ErrorUpdate = 5,
    StatusUpdate = 6
}

public enum AlertLevel
{
    Unspecified = 0,
    Normal = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum FusionMethod
{
    Unspecified = 0,
    TemporalFusion = 1,
    WeightedFusion = 2,
    RuleBasedFusion = 3,
    MlFusion = 4
}

public enum ServiceHealth
{
    Unspecified = 0,
    Healthy = 1,
    Degraded = 2,
    Unhealthy = 3,
    Unknown = 4
}

public enum ModelStatus
{
    Unspecified = 0,
    Loaded = 1,
    Loading = 2,
    Failed = 3,
    NotFound = 4,
    Unloaded = 5
}

public enum BatchAnalysisStatus
{
    Unspecified = 0,
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}
