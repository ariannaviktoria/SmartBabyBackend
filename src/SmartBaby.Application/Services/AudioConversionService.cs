using System.Text;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;
using SmartBaby.Core.Interfaces;

namespace SmartBaby.Application.Services;

/// <summary>
/// Uses FFmpeg (via FFMpegCore) to normalize arbitrary input audio (e.g. m4a/aac/caf, mp3) into WAV PCM16 mono 44100Hz.
/// Provides lightweight validation to skip conversion when already suitable WAV.
/// </summary>
public class AudioConversionService : IAudioConversionService
{
    private readonly ILogger<AudioConversionService> _logger;
    private static bool _ffmpegConfigured;

    public AudioConversionService(ILogger<AudioConversionService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]?> ConvertToPcm16WavAsync(byte[] inputBytes, CancellationToken cancellationToken = default)
    {
        try
        {
            if (inputBytes == null || inputBytes.Length < 16) return null;

            // Check if already PCM16 mono 44100 WAV
            if (IsAcceptableWav(inputBytes))
            {
                return inputBytes; // no conversion needed
            }

            await EnsureFfmpegAsync();

            var tmpIn = Path.GetTempFileName();
            var tmpOut = Path.ChangeExtension(Path.GetTempFileName(), ".wav");
            await File.WriteAllBytesAsync(tmpIn, inputBytes, cancellationToken);
            try
            {
                var args = FFMpegArguments
                    .FromFileInput(tmpIn, verifyExists: true)
                    .OutputToFile(tmpOut, overwrite: true, opt => opt
                        .WithAudioCodec("pcm_s16le")
                        .WithCustomArgument("-ac 1") // mono
                        .WithCustomArgument("-ar 44100") // 44.1k
                        .DisableChannel(Channel.Video) // ensure no video
                    );
                await args.ProcessAsynchronously();

                if (!File.Exists(tmpOut))
                {
                    _logger.LogWarning("FFmpeg conversion did not produce output file.");
                    return null;
                }
                var converted = await File.ReadAllBytesAsync(tmpOut, cancellationToken);
                if (!IsAcceptableWav(converted))
                {
                    _logger.LogWarning("Converted file is not in expected PCM16 mono 44100 WAV format.");
                }
                return converted;
            }
            finally
            {
                TryDelete(tmpIn);
                TryDelete(tmpOut);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Audio conversion failed");
            return null;
        }
    }

    private static bool IsAcceptableWav(byte[] bytes)
    {
        try
        {
            if (bytes.Length < 44) return false;
            if (Encoding.ASCII.GetString(bytes, 0, 4) != "RIFF" || Encoding.ASCII.GetString(bytes, 8, 4) != "WAVE") return false;
            short audioFormat = BitConverter.ToInt16(bytes, 20); // PCM = 1
            short channels = BitConverter.ToInt16(bytes, 22);
            int sampleRate = BitConverter.ToInt32(bytes, 24);
            short bitsPerSample = BitConverter.ToInt16(bytes, 34);
            return audioFormat == 1 && channels == 1 && sampleRate == 44100 && bitsPerSample == 16;
        }
        catch { return false; }
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    private static Task EnsureFfmpegAsync()
    {
        if (_ffmpegConfigured) return Task.CompletedTask;
        try
        {
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = null, TemporaryFilesFolder = Path.GetTempPath() });
            _ffmpegConfigured = true;
        }
        catch { }
        return Task.CompletedTask;
    }
}
