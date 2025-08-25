using SmartBaby.Core.DTOs;

namespace SmartBaby.Core.Interfaces;

/// <summary>
/// Result container for a generated or resized preview image.
/// </summary>
public sealed class PreviewResult
{
    public string Base64 { get; init; } = string.Empty; // Raw base64 (no data URI)
    public string ContentType { get; init; } = "image/png";
    public int Width { get; init; }
    public int Height { get; init; }
    public long ByteSize { get; init; }
}

/// <summary>
/// Service responsible for generating (video/audio) and resizing (any) preview images.
/// Ensures previews are below a configurable maximum size (default 1MB) suitable for DB storage.
/// </summary>
public interface IPreviewGenerationService
{
    /// <summary>
    /// Resize / recompress an existing image (base64 or with a content type) to be <= maxBytes.
    /// </summary>
    Task<PreviewResult?> ResizeImageAsync(string base64Image, string? contentType = null, int? maxWidth = null, int? maxHeight = null, int maxBytes = 1_000_000, CancellationToken ct = default);

    /// <summary>
    /// Generate a preview (thumbnail) from raw video bytes. Returns null if generation fails.
    /// </summary>
    Task<PreviewResult?> GenerateVideoPreviewAsync(byte[] videoBytes, CancellationToken ct = default);

    /// <summary>
    /// Generate a preview (simple waveform or spectrogram substitute) from raw audio bytes.
    /// Supports uncompressed PCM WAV. For unsupported encodings returns null.
    /// </summary>
    Task<PreviewResult?> GenerateAudioPreviewAsync(byte[] audioBytes, CancellationToken ct = default);
}
