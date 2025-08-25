using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBaby.Core.Entities;

/// <summary>
/// Entity for storing baby analysis history
/// </summary>
public class BabyAnalysis
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int BabyId { get; set; }

    [Required]
    [MaxLength(50)]
    public string AnalysisType { get; set; } = string.Empty; // "image", "audio", "video", "multimodal", "realtime"

    [Required]
    [MaxLength(100)]
    public string RequestId { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public bool Success { get; set; }

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    [Required]
    [Column(TypeName = "jsonb")]
    public string ResultData { get; set; } = string.Empty; // JSON serialized analysis result

    [Range(0.0, 1.0)]
    public float? Confidence { get; set; }

    [MaxLength(100)]
    public string? PrimaryResult { get; set; } // Main result (e.g., detected mood, cry reason)

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; } // Additional metadata as JSON

    public float? ProcessingTimeMs { get; set; }

    [MaxLength(50)]
    public string? ModelVersion { get; set; }

    // File reference fields
    [MaxLength(500)]
    public string? OriginalFilePath { get; set; }

    [MaxLength(500)]
    public string? StoredFilePath { get; set; }

    public long? FileSizeBytes { get; set; }

    [MaxLength(100)]
    public string? FileChecksum { get; set; }

    // Preview image (thumbnail) for quick recognition of the analysis entry
    // For image & multimodal we store the submitted image (possibly resized upstream)
    // For video we may later extract a representative frame (currently optional)
    // For audio we may later store a generated waveform/spectrogram image
    [Column(TypeName = "bytea")]
    public byte[]? PreviewImage { get; set; }

    [MaxLength(100)]
    public string? PreviewImageContentType { get; set; }

    // Navigation properties
    public virtual Baby Baby { get; set; } = null!;
    public virtual ICollection<AnalysisTag> Tags { get; set; } = new List<AnalysisTag>();
}

/// <summary>
/// Entity for storing batch analysis information
/// </summary>
public class BatchAnalysis
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string BatchId { get; set; } = string.Empty;

    [Required]
    public int BabyId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [Required]
    public SmartBaby.Core.DTOs.BatchAnalysisStatus Status { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int TotalItems { get; set; }

    public int ProcessedItems { get; set; }
    public int SuccessfulItems { get; set; }
    public int FailedItems { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Settings { get; set; } // Analysis options as JSON

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public virtual Baby Baby { get; set; } = null!;
    public virtual ICollection<BabyAnalysis> Analyses { get; set; } = new List<BabyAnalysis>();
}

/// <summary>
/// Entity for storing real-time analysis sessions
/// </summary>
public class RealtimeAnalysisSession
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = string.Empty;

    [Required]
    public int BabyId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }

    [Required]
    public RealtimeSessionStatus Status { get; set; }

    [Column(TypeName = "jsonb")]
    public string Settings { get; set; } = string.Empty; // RealtimeSettingsDto as JSON

    public int UpdateCount { get; set; }
    public DateTime? LastUpdateAt { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Statistics { get; set; } // Session statistics as JSON

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public virtual Baby Baby { get; set; } = null!;
    public virtual ICollection<RealtimeAnalysisUpdate> Updates { get; set; } = new List<RealtimeAnalysisUpdate>();
}

/// <summary>
/// Entity for storing real-time analysis updates
/// </summary>
public class RealtimeAnalysisUpdate
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int SessionId { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(50)]
    public string UpdateType { get; set; } = string.Empty; // "emotion", "cry", "fusion", "status", "error"

    [Required]
    [Column(TypeName = "jsonb")]
    public string UpdateData { get; set; } = string.Empty; // JSON serialized update data

    public float? Confidence { get; set; }

    [MaxLength(100)]
    public string? PrimaryResult { get; set; }

    // Navigation properties
    public virtual RealtimeAnalysisSession Session { get; set; } = null!;
}

/// <summary>
/// Entity for storing analysis tags (for categorization and search)
/// </summary>
public class AnalysisTag
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int AnalysisId { get; set; }

    [Required]
    [MaxLength(50)]
    public string TagName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TagValue { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? CreatedBy { get; set; } // User who added the tag

    // Navigation properties
    public virtual BabyAnalysis Analysis { get; set; } = null!;
}

/// <summary>
/// Entity for storing analysis alerts and notifications
/// </summary>
public class AnalysisAlert
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int BabyId { get; set; }

    [Required]
    public int AnalysisId { get; set; }

    [Required]
    [MaxLength(50)]
    public string AlertType { get; set; } = string.Empty; // "high_distress", "crying_detected", "abnormal_pattern"

    [Required]
    public AlertLevel Severity { get; set; }

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? AcknowledgedAt { get; set; }

    [MaxLength(100)]
    public string? AcknowledgedBy { get; set; }

    public bool IsRead { get; set; }

    [Column(TypeName = "jsonb")]
    public string? AdditionalData { get; set; } // Additional alert data as JSON

    // Navigation properties
    public virtual Baby Baby { get; set; } = null!;
    public virtual BabyAnalysis Analysis { get; set; } = null!;
}

public enum RealtimeSessionStatus
{
    Created = 0,
    Starting = 1,
    Active = 2,
    Paused = 3,
    Stopped = 4,
    Error = 5
}

public enum AlertLevel
{
    Info = 0,
    Warning = 1,
    High = 2,
    Critical = 3
}
