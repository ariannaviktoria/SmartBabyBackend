using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Text;
using FFMpegCore;
using FFMpegCore.Pipes;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SmartBaby.Core.Interfaces;
using OpenCvSharp;

namespace SmartBaby.Application.Services;

/// <summary>
/// Generates and resizes preview images for image/video/audio inputs.
/// </summary>
public class PreviewGenerationService : IPreviewGenerationService
{
    private readonly ILogger<PreviewGenerationService> _logger;
    private const int DefaultMaxBytes = 1_000_000; // 1MB
    private const int VideoGrabSecond = 1; // grab first-second frame

    public PreviewGenerationService(ILogger<PreviewGenerationService> logger)
    {
        _logger = logger;
    }

    public async Task<PreviewResult?> ResizeImageAsync(string base64Image, string? contentType = null, int? maxWidth = null, int? maxHeight = null, int maxBytes = DefaultMaxBytes, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(base64Image)) return null;
            var clean = CleanBase64(base64Image);
            var bytes = Convert.FromBase64String(clean);
            using var msIn = new MemoryStream(bytes);
            using var image = await Image.LoadAsync(msIn, ct);

            // Maintain aspect ratio
            var targetWidth = maxWidth ?? 480;
            var targetHeight = maxHeight ?? 480;
            if (image.Width > targetWidth || image.Height > targetHeight)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new SixLabors.ImageSharp.Size(targetWidth, targetHeight)
                }));
            }

            // Encode with quality reduction loop until under size
            var quality = 85;
            byte[]? outBytes = null;
            IImageEncoder encoder = new JpegEncoder { Quality = quality };
            if ((contentType ?? InferContentType(bytes)) == "image/png")
            {
                encoder = new PngEncoder { CompressionLevel = PngCompressionLevel.Level6 };
            }

            while (quality >= 40)
            {
                using var ms = new MemoryStream();
                if (encoder is JpegEncoder) encoder = new JpegEncoder { Quality = quality };
                await image.SaveAsync(ms, encoder, ct);
                outBytes = ms.ToArray();
                if (outBytes.Length <= maxBytes) break;
                quality -= 10;
                if (encoder is not JpegEncoder)
                {
                    // Switch to jpeg for heavier compression
                    encoder = new JpegEncoder { Quality = quality };
                }
            }

            if (outBytes == null) return null;
            return new PreviewResult
            {
                Base64 = Convert.ToBase64String(outBytes),
                ContentType = encoder is PngEncoder ? "image/png" : "image/jpeg",
                Width = image.Width,
                Height = image.Height,
                ByteSize = outBytes.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resize image preview");
            return null;
        }
    }

    public async Task<PreviewResult?> GenerateVideoPreviewAsync(byte[] videoBytes, CancellationToken ct = default)
    {
        try
        {
            if (videoBytes == null || videoBytes.Length == 0) return null;
            var tmpVideo = Path.GetTempFileName() + ".mp4";
            await File.WriteAllBytesAsync(tmpVideo, videoBytes, ct);

            byte[]? imageBytes = null;
            var tmpOut = Path.GetTempFileName() + ".jpg";
            try
            {
                try
                {
                    await EnsureFFMpegConfiguredAsync();
                    // FFMpegCore Snapshot: output path, size?, position
                    var snapshotSuccess = await FFMpeg.SnapshotAsync(tmpVideo, tmpOut, null, TimeSpan.FromSeconds(VideoGrabSecond));
                    if (snapshotSuccess && File.Exists(tmpOut))
                    {
                        imageBytes = await File.ReadAllBytesAsync(tmpOut, ct);
                    }
                }
                catch (Exception ffEx)
                {
                    _logger.LogDebug(ffEx, "FFMpegCore snapshot failed; attempting OpenCvSharp frame extraction.");
                }
                // Fallback: OpenCvSharp first frame extraction
                if (imageBytes == null)
                {
                    try
                    {
                        using var capture = new VideoCapture(tmpVideo);
                        if (capture.IsOpened())
                        {
                            if (VideoGrabSecond > 0 && capture.Fps > 0)
                            {
                                var targetFrameIndex = (int)(VideoGrabSecond * capture.Fps);
                                capture.Set(VideoCaptureProperties.PosFrames, targetFrameIndex);
                            }
                            using var mat = new Mat();
                            if (capture.Read(mat) && !mat.Empty())
                            {
                                Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2RGB);
                                imageBytes = OpenCvSharpHelpers.MatToJpeg(mat, 80);
                            }
                        }
                    }
                    catch (Exception ocvEx)
                    {
                        _logger.LogDebug(ocvEx, "OpenCvSharp frame extraction failed; will use placeholder.");
                    }
                }
            }
            finally
            {
                TryDelete(tmpOut);
            }

            if (imageBytes == null)
            {
                _logger.LogWarning("Video preview generation failed with both FFmpeg and OpenCvSharp; using minimal placeholder.");
                using var placeholder = new Image<Rgba32>(160, 90, new Rgba32(20, 20, 25));
                using var msPh = new MemoryStream();
                await placeholder.SaveAsJpegAsync(msPh, new JpegEncoder { Quality = 60 }, ct);
                imageBytes = msPh.ToArray();
            }

            var resized = await ResizeImageAsync(Convert.ToBase64String(imageBytes), "image/jpeg", 480, 480, DefaultMaxBytes, ct);
            TryDelete(tmpVideo);
            return resized;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate video preview");
            return null;
        }
    }

    public Task<PreviewResult?> GenerateAudioPreviewAsync(byte[] audioBytes, CancellationToken ct = default)
    {
        try
        {
            if (audioBytes == null || audioBytes.Length < 16) return Task.FromResult<PreviewResult?>(null);

            // First attempt: native lightweight WAV waveform (fast path)
            bool isWav = audioBytes.Length >= 44 && Encoding.ASCII.GetString(audioBytes, 0, 4) == "RIFF" && Encoding.ASCII.GetString(audioBytes, 8, 4) == "WAVE";
            if (!isWav)
            {
                // Try FFmpeg-based generation for MP3 / other formats
                // Quick MP3 / ID3 detection
                bool looksMp3 = (audioBytes[0] == 0x49 && audioBytes[1] == 0x44 && audioBytes[2] == 0x33) // ID3 tag
                                 || (audioBytes[0] == 0xFF && (audioBytes[1] & 0xE0) == 0xE0); // Frame sync
                // We will still attempt FFmpeg for any non-WAV; FFmpeg will decide
                try
                {
                    var ff = GenerateAudioPreviewWithFfmpeg(audioBytes, ct);
                    return Task.FromResult(ff);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "FFmpeg audio preview generation failed (isMp3={IsMp3})", looksMp3);
                    return Task.FromResult<PreviewResult?>(null);
                }
            }

            // WAV path (16-bit PCM only)
            if (Encoding.ASCII.GetString(audioBytes, 0, 4) != "RIFF" || Encoding.ASCII.GetString(audioBytes, 8, 4) != "WAVE")
                return Task.FromResult<PreviewResult?>(null);

            int channels = BitConverter.ToInt16(audioBytes, 22);
            int sampleRate = BitConverter.ToInt32(audioBytes, 24);
            short bitsPerSample = BitConverter.ToInt16(audioBytes, 34);
            if (bitsPerSample != 16) return Task.FromResult<PreviewResult?>(null);

            // Find data chunk
            int idx = 12;
            int dataOffset = -1;
            int dataLength = 0;
            while (idx + 8 < audioBytes.Length)
            {
                var chunkId = Encoding.ASCII.GetString(audioBytes, idx, 4);
                var chunkSize = BitConverter.ToInt32(audioBytes, idx + 4);
                if (chunkId == "data")
                {
                    dataOffset = idx + 8;
                    dataLength = chunkSize;
                    break;
                }
                idx += 8 + chunkSize;
            }
            if (dataOffset < 0) return Task.FromResult<PreviewResult?>(null);

            var sampleCount = dataLength / (bitsPerSample / 8);
            var step = Math.Max(1, sampleCount / 2000); // downsample for waveform width
            var points = new List<float>(2000);
            for (int i = 0; i < sampleCount; i += step * channels)
            {
                short sample = BitConverter.ToInt16(audioBytes, dataOffset + i * 2);
                points.Add(sample / 32768f);
                if (points.Count >= 2000) break;
            }

            int width = points.Count;
            int height = 120;
            using var img = new Image<Rgba32>(width, height, new Rgba32(10, 10, 30));
            var mid = height / 2f;
            img.ProcessPixelRows(accessor =>
            {
                for (int x = 0; x < width; x++)
                {
                    var v = points[x];
                    int y1 = (int)Math.Clamp(mid - v * (mid - 2), 0, height - 1);
                    int y2 = (int)Math.Clamp(mid + v * (mid - 2), 0, height - 1);
                    for (int y = y1; y <= y2; y++)
                    {
                        accessor.GetRowSpan(y)[x] = new Rgba32(0, 200, 255);
                    }
                }
            });

            using var ms = new MemoryStream();
            img.SaveAsPng(ms);
            var pngBytes = ms.ToArray();
            // Potentially resize/compress (already small)
            return Task.FromResult<PreviewResult?>(new PreviewResult
            {
                Base64 = Convert.ToBase64String(pngBytes),
                ContentType = "image/png",
                Width = width,
                Height = height,
                ByteSize = pngBytes.Length
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate audio waveform preview");
            return Task.FromResult<PreviewResult?>(null);
        }
    }

    private PreviewResult? GenerateAudioPreviewWithFfmpeg(byte[] audioBytes, CancellationToken ct)
    {
        EnsureFFMpegConfiguredAsync().GetAwaiter().GetResult();
        var tmpIn = Path.GetTempFileName() + ".bin"; // extension guessed by ffmpeg using probing
        var tmpOut = Path.GetTempFileName() + ".png";
        File.WriteAllBytes(tmpIn, audioBytes);
        try
        {
            // Use showwavespic (more universally enabled than showspectrumpic).
            // 640x120 for consistency with WAV fallback.
            var args = FFMpegArguments
                .FromFileInput(tmpIn)
                .OutputToFile(tmpOut, true, opt => opt
                    .WithCustomArgument("-filter_complex showwavespic=s=640x120 -frames:v 1"));
            args.ProcessSynchronously();
            if (!File.Exists(tmpOut)) return null;
            var png = File.ReadAllBytes(tmpOut);
            return new PreviewResult
            {
                Base64 = Convert.ToBase64String(png),
                ContentType = "image/png",
                Width = 640,
                Height = 120,
                ByteSize = png.Length
            };
        }
        finally
        {
            TryDelete(tmpIn);
            TryDelete(tmpOut);
        }
    }

    private static string CleanBase64(string data)
    {
        var idx = data.IndexOf(',');
        if (data.StartsWith("data:") && idx > -1) return data[(idx + 1)..];
        return data.Trim();
    }

    private static string InferContentType(byte[] bytes)
    {
        if (bytes.Length >= 4)
        {
            if (bytes[0] == 0x89 && bytes[1] == 0x50) return "image/png";
            if (bytes[0] == 0xFF && bytes[1] == 0xD8) return "image/jpeg";
            if (bytes[0] == 0x47 && bytes[1] == 0x49) return "image/gif";
        }
        return "image/png";
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    private static bool _ffmpegConfigured;
    private static Task EnsureFFMpegConfiguredAsync()
    {
        if (_ffmpegConfigured) return Task.CompletedTask;
        var folder = FFmpegFinder.TryLocate();
        try
        {
            // If folder is null, FFMpegCore will use PATH
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = folder ?? string.Empty, TemporaryFilesFolder = Path.GetTempPath() });
        }
        catch (Exception)
        {
            // Leave _ffmpegConfigured false so another attempt may be made later.
            return Task.CompletedTask;
        }
        _ffmpegConfigured = true;
        return Task.CompletedTask;
    }
}

internal static class FFmpegFinder
{
    public static string? TryLocate()
    {
        // Respect existing PATH if ffmpeg is installed
        return null; // FFMpegCore will attempt to use PATH
    }
}

internal static class OpenCvSharpHelpers
{
    internal static byte[] MatToJpeg(Mat mat, int quality)
    {
        Cv2.ImEncode(".jpg", mat, out var buf, new int[] { (int)ImwriteFlags.JpegQuality, quality });
        return buf; // ImEncode already returns a managed byte[]
    }
}
