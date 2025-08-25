using System.Threading;
using System.Threading.Tasks;

namespace SmartBaby.Core.Interfaces;

/// <summary>
/// Converts incoming audio bytes from various container/codecs (e.g. AAC/M4A, CAF/Apple PCM, MP3, etc.)
/// into a normalized WAV (PCM 16-bit, mono, 44100 Hz) byte array suitable for analysis backend.
/// Returns null if conversion fails or input is invalid.
/// </summary>
public interface IAudioConversionService
{
    /// <summary>
    /// Attempts to determine if the audio bytes are already acceptable (PCM16 mono 44100 WAV) and if not, converts them.
    /// </summary>
    /// <param name="inputBytes">Raw audio file bytes in any supported format.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Converted WAV bytes or null if conversion failed.</returns>
    Task<byte[]?> ConvertToPcm16WavAsync(byte[] inputBytes, CancellationToken cancellationToken = default);
}
