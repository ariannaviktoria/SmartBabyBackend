using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBaby.API.Models;
using SmartBaby.Core.DTOs;
using SmartBaby.Core.Interfaces;
using System.Security.Claims;

namespace SmartBaby.API.Controllers;

/// <summary>
/// Controller for Baby Analysis operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BabyAnalysisController : ControllerBase
{
    private readonly IBabyAnalysisService _analysisService;
    private readonly IBabyService _babyService;
    private readonly ILogger<BabyAnalysisController> _logger;

    public BabyAnalysisController(
        IBabyAnalysisService analysisService,
        IBabyService babyService,
        ILogger<BabyAnalysisController> logger)
    {
        _analysisService = analysisService;
        _babyService = babyService;
        _logger = logger;
    }

    #region Image Analysis

    /// <summary>
    /// Analyze an image for baby emotion detection
    /// </summary>
    [HttpPost("image")]
    public async Task<ActionResult<ImageAnalysisResponseDto>> AnalyzeImage([FromBody] ImageAnalysisRequestDto request)
    {
        try
        {
            // Validate baby ownership if BabyId is provided
            if (request.BabyId.HasValue)
            {
                if (!await ValidateBabyOwnership(request.BabyId.Value))
                {
                    return Forbid("You don't have access to this baby's data");
                }
            }

            var result = await _analysisService.AnalyzeImageAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image");
            return StatusCode(500, "An error occurred while analyzing the image");
        }
    }

    /// <summary>
    /// Analyze an image from uploaded file
    /// </summary>
    /// <param name="imageFile">Image file to analyze (JPEG, PNG, BMP)</param>
    /// <param name="babyId">Optional baby ID to associate with analysis</param>
    /// <param name="confidenceThreshold">Minimum confidence threshold for results</param>
    /// <param name="includeDebugInfo">Include debug information in response</param>
    /// <returns>Image analysis results</returns>
    [HttpPost("image/upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImageAnalysisResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    public async Task<ActionResult<ImageAnalysisResponseDto>> AnalyzeImageUpload(
        [FromForm] IFormFile imageFile,
        [FromForm] int? babyId,
        [FromForm] float confidenceThreshold = 0.5f,
        [FromForm] bool includeDebugInfo = false)
    {
        try
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("No image file provided");
            }

            // Validate file size (max 10MB)
            if (imageFile.Length > 10 * 1024 * 1024)
            {
                return BadRequest("File size too large. Maximum 10MB allowed.");
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/bmp" };
            if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
            {
                return BadRequest("Invalid file type. Only JPEG, PNG, and BMP files are allowed.");
            }

            // Validate baby ownership if BabyId is provided
            if (babyId.HasValue)
            {
                if (!await ValidateBabyOwnership(babyId.Value))
                {
                    return Forbid("You don't have access to this baby's data");
                }
            }

            // Convert file to bytes
            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            var request = new ImageAnalysisRequestDto
            {
                ImageBytes = imageBytes,
                BabyId = babyId,
                RequestId = Guid.NewGuid().ToString(),
                Options = new AnalysisOptionsDto
                {
                    ConfidenceThreshold = confidenceThreshold,
                    IncludeDebugInfo = includeDebugInfo
                },
                PreviewImageBase64 = Convert.ToBase64String(imageBytes),
                PreviewImageContentType = imageFile.ContentType
            };

            var result = await _analysisService.AnalyzeImageAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing uploaded image");
            return StatusCode(500, "An error occurred while analyzing the image");
        }
    }

    #endregion

    #region Audio Analysis

    /// <summary>
    /// Analyze audio for baby cry detection
    /// </summary>
    [HttpPost("audio")]
    public async Task<ActionResult<AudioAnalysisResponseDto>> AnalyzeAudio([FromBody] AudioAnalysisRequestDto request)
    {
        try
        {
            // Validate baby ownership if BabyId is provided
            if (request.BabyId.HasValue)
            {
                if (!await ValidateBabyOwnership(request.BabyId.Value))
                {
                    return Forbid("You don't have access to this baby's data");
                }
            }

            var result = await _analysisService.AnalyzeAudioAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing audio");
            return StatusCode(500, "An error occurred while analyzing the audio");
        }
    }

    /// <summary>
    /// Analyze audio from uploaded file
    /// </summary>
    /// <param name="audioFile">Audio file to analyze (WAV, MP3)</param>
    /// <param name="babyId">Optional baby ID to associate with analysis</param>
    /// <param name="confidenceThreshold">Minimum confidence threshold for results</param>
    /// <param name="sampleRate">Audio sample rate (default: 44100)</param>
    /// <param name="channels">Number of audio channels (default: 1)</param>
    /// <returns>Audio analysis results</returns>
    [HttpPost("audio/upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AudioAnalysisResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB limit
    public async Task<ActionResult<AudioAnalysisResponseDto>> AnalyzeAudioUpload(
        [FromForm] IFormFile audioFile,
        [FromForm] int? babyId,
        [FromForm] float confidenceThreshold = 0.5f,
        [FromForm] int sampleRate = 44100,
        [FromForm] int channels = 1)
    {
        try
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("No audio file provided");
            }

            // Validate file size (max 50MB)
            if (audioFile.Length > 50 * 1024 * 1024)
            {
                return BadRequest("File size too large. Maximum 50MB allowed.");
            }

            // Validate file type
            var allowedTypes = new[] { "audio/wav", "audio/wave", "audio/x-wav", "audio/mpeg", "audio/mp3" };
            if (!allowedTypes.Contains(audioFile.ContentType.ToLower()))
            {
                return BadRequest("Invalid file type. Only WAV and MP3 files are allowed.");
            }

            // Validate baby ownership if BabyId is provided
            if (babyId.HasValue)
            {
                if (!await ValidateBabyOwnership(babyId.Value))
                {
                    return Forbid("You don't have access to this baby's data");
                }
            }

            // Convert file to bytes
            byte[] audioBytes;
            using (var memoryStream = new MemoryStream())
            {
                await audioFile.CopyToAsync(memoryStream);
                audioBytes = memoryStream.ToArray();
            }

            var request = new AudioAnalysisRequestDto
            {
                AudioBytes = audioBytes,
                BabyId = babyId,
                RequestId = Guid.NewGuid().ToString(),
                AudioFormat = new AudioFormatDto
                {
                    SampleRate = sampleRate,
                    Channels = channels,
                    Encoding = "LINEAR_PCM",
                    BitDepth = 16
                },
                Options = new AnalysisOptionsDto
                {
                    ConfidenceThreshold = confidenceThreshold
                }
            };

            var result = await _analysisService.AnalyzeAudioAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing uploaded audio");
            return StatusCode(500, "An error occurred while analyzing the audio");
        }
    }

    #endregion

    #region Video Analysis

    /// <summary>
    /// Analyze video for combined audio-visual analysis
    /// </summary>
    [HttpPost("video")]
    public async Task<ActionResult<VideoAnalysisResponseDto>> AnalyzeVideo([FromBody] VideoAnalysisRequestDto request)
    {
        try
        {
            // Validate baby ownership if BabyId is provided
            if (request.BabyId.HasValue)
            {
                if (!await ValidateBabyOwnership(request.BabyId.Value))
                {
                    return Forbid("You don't have access to this baby's data");
                }
            }

            var result = await _analysisService.AnalyzeVideoAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing video");
            return StatusCode(500, "An error occurred while analyzing the video");
        }
    }

    /// <summary>
    /// Analyze video from uploaded file
    /// </summary>
    /// <param name="videoFile">Video file to analyze (MP4, AVI, MOV, WMV)</param>
    /// <param name="babyId">Optional baby ID to associate with analysis</param>
    /// <param name="frameInterval">Interval between frame analysis in seconds</param>
    /// <param name="audioSegmentDuration">Audio segment duration in seconds</param>
    /// <param name="enableFusion">Enable audio-visual fusion analysis</param>
    /// <param name="saveResults">Save results to storage</param>
    /// <returns>Video analysis results</returns>
    [HttpPost("video/upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(VideoAnalysisResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    [RequestSizeLimit(200 * 1024 * 1024)] // 200MB limit
    public async Task<ActionResult<VideoAnalysisResponseDto>> AnalyzeVideoUpload(
        [FromForm] IFormFile videoFile,
        [FromForm] int? babyId,
        [FromForm] float frameInterval = 1.0f,
        [FromForm] float audioSegmentDuration = 2.0f,
    [FromForm] bool enableFusion = true,
    [FromForm] bool saveResults = true)
    {
        try
        {
            if (videoFile == null || videoFile.Length == 0)
            {
                return BadRequest("No video file provided");
            }

            // Validate file size (max 200MB)
            if (videoFile.Length > 200 * 1024 * 1024)
            {
                return BadRequest("File size too large. Maximum 200MB allowed.");
            }

            // Validate file type
            var allowedTypes = new[] { "video/mp4", "video/avi", "video/mov", "video/wmv" };
            if (!allowedTypes.Contains(videoFile.ContentType.ToLower()))
            {
                return BadRequest("Invalid file type. Only MP4, AVI, MOV, and WMV files are allowed.");
            }

            // Validate baby ownership if BabyId is provided
            if (babyId.HasValue)
            {
                if (!await ValidateBabyOwnership(babyId.Value))
                {
                    return Forbid("You don't have access to this baby's data");
                }
            }

            // Convert file to bytes
            byte[] videoBytes;
            using (var memoryStream = new MemoryStream())
            {
                await videoFile.CopyToAsync(memoryStream);
                videoBytes = memoryStream.ToArray();
            }

            var request = new VideoAnalysisRequestDto
            {
                VideoBytes = videoBytes,
                BabyId = babyId,
                RequestId = Guid.NewGuid().ToString(),
                Options = new VideoAnalysisOptionsDto
                {
                    FrameInterval = frameInterval,
                    AudioSegmentDuration = audioSegmentDuration,
                    EnableFusion = enableFusion,
                    SaveResults = saveResults
                },
                // TODO: Optionally extract a thumbnail frame server-side; for now client doesn't send it
            };

            var result = await _analysisService.AnalyzeVideoAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing uploaded video");
            return StatusCode(500, "An error occurred while analyzing the video");
        }
    }

    #endregion

    #region Multimodal Analysis

    /// <summary>
    /// Perform combined image and audio analysis
    /// </summary>
    [HttpPost("multimodal")]
    public async Task<ActionResult<MultimodalAnalysisResponseDto>> AnalyzeMultimodal([FromBody] MultimodalAnalysisRequestDto request)
    {
        try
        {
            // Validate baby ownership if BabyId is provided
            if (request.BabyId.HasValue)
            {
                if (!await ValidateBabyOwnership(request.BabyId.Value))
                {
                    return Forbid("You don't have access to this baby's data");
                }
            }

            var result = await _analysisService.AnalyzeMultimodalAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing multimodal analysis");
            return StatusCode(500, "An error occurred while performing multimodal analysis");
        }
    }

    /// <summary>
    /// Perform multimodal analysis from uploaded files
    /// </summary>
    /// <param name="request">The multimodal upload request containing files and parameters</param>
    /// <returns>Multimodal analysis results</returns>
    [HttpPost("multimodal/upload")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 209715200)] // 200MB
    [RequestSizeLimit(200 * 1024 * 1024)] // 200MB limit
    [ProducesResponseType(typeof(MultimodalAnalysisResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<MultimodalAnalysisResponseDto>> AnalyzeMultimodalUpload(
        [FromForm] MultimodalUploadRequestDto request)
    {
        try
        {
            if (request.ImageFile == null && request.AudioFile == null)
            {
                return BadRequest("At least one file (image or audio) must be provided");
            }

            // Validate baby ownership if BabyId is provided
            if (request.BabyId.HasValue)
            {
                if (!await ValidateBabyOwnership(request.BabyId.Value))
                {
                    return Forbid("You don't have access to this baby's data");
                }
            }

            var analysisRequest = new MultimodalAnalysisRequestDto
            {
                BabyId = request.BabyId,
                RequestId = Guid.NewGuid().ToString(),
                Options = new AnalysisOptionsDto
                {
                    ConfidenceThreshold = request.ConfidenceThreshold
                }
            };

            // Process image file if provided
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                byte[] imageBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await request.ImageFile.CopyToAsync(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }

                analysisRequest.ImageRequest = new ImageAnalysisRequestDto
                {
                    ImageBytes = imageBytes,
                    RequestId = analysisRequest.RequestId,
                    Options = analysisRequest.Options
                };
            }

            // Process audio file if provided
            if (request.AudioFile != null && request.AudioFile.Length > 0)
            {
                byte[] audioBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await request.AudioFile.CopyToAsync(memoryStream);
                    audioBytes = memoryStream.ToArray();
                }

                analysisRequest.AudioRequest = new AudioAnalysisRequestDto
                {
                    AudioBytes = audioBytes,
                    RequestId = analysisRequest.RequestId,
                    Options = analysisRequest.Options,
                    AudioFormat = new AudioFormatDto
                    {
                        SampleRate = 44100,
                        Channels = 1,
                        Encoding = "LINEAR_PCM",
                        BitDepth = 16
                    }
                };
            }

            var result = await _analysisService.AnalyzeMultimodalAsync(analysisRequest);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing multimodal analysis upload");
            return StatusCode(500, "An error occurred while performing multimodal analysis");
        }
    }

    #endregion

    #region Analysis History

    /// <summary>
    /// Get analysis history for a baby
    /// </summary>
    [HttpGet("history/{babyId}")]
    public async Task<ActionResult<IEnumerable<AnalysisHistoryDto>>> GetAnalysisHistory(
        int babyId,
        [FromQuery] string? analysisType = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        try
        {
            if (!await ValidateBabyOwnership(babyId))
            {
                return Forbid("You don't have access to this baby's data");
            }

            var history = await _analysisService.GetAnalysisHistoryAsync(babyId, analysisType, from, to);
            return Ok(history.Skip(offset).Take(limit));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analysis history for baby {BabyId}", babyId);
            return StatusCode(500, "An error occurred while retrieving analysis history");
        }
    }

    /// <summary>
    /// Get specific analysis result
    /// </summary>
    [HttpGet("result/{analysisId}")]
    public async Task<ActionResult<AnalysisHistoryDto>> GetAnalysisResult(int analysisId)
    {
        try
        {
            var result = await _analysisService.GetAnalysisResultAsync(analysisId);
            if (result == null)
            {
                return NotFound("Analysis result not found");
            }

            // Validate baby ownership
            if (!await ValidateBabyOwnership(result.BabyId))
            {
                return Forbid("You don't have access to this analysis result");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analysis result {AnalysisId}", analysisId);
            return StatusCode(500, "An error occurred while retrieving the analysis result");
        }
    }

    #endregion

    #region Health and Status

    /// <summary>
    /// Get analyzer service health status
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult<HealthStatusResponseDto>> GetHealthStatus()
    {
        try
        {
            var status = await _analysisService.GetHealthStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health status");
            return StatusCode(500, "An error occurred while checking health status");
        }
    }

    /// <summary>
    /// Get analyzer model status
    /// </summary>
    [HttpGet("models")]
    public async Task<ActionResult<ModelStatusResponseDto>> GetModelStatus()
    {
        try
        {
            var status = await _analysisService.GetModelStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model status");
            return StatusCode(500, "An error occurred while checking model status");
        }
    }

    #endregion

    #region Batch Analysis

    /// <summary>
    /// Submit batch analysis request
    /// </summary>
    [HttpPost("batch")]
    public async Task<ActionResult<BatchAnalysisResponseDto>> SubmitBatchAnalysis([FromBody] BatchAnalysisRequestDto request)
    {
        try
        {
            // Validate baby ownership
            if (!await ValidateBabyOwnership(request.BabyId))
            {
                return Forbid("You don't have access to this baby's data");
            }

            var result = await _analysisService.SubmitBatchAnalysisAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting batch analysis");
            return StatusCode(500, "An error occurred while submitting batch analysis");
        }
    }

    /// <summary>
    /// Get batch analysis status
    /// </summary>
    [HttpGet("batch/{batchId}")]
    public async Task<ActionResult<BatchAnalysisStatusDto>> GetBatchAnalysisStatus(string batchId)
    {
        try
        {
            var status = await _analysisService.GetBatchAnalysisStatusAsync(batchId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batch analysis status for {BatchId}", batchId);
            return StatusCode(500, "An error occurred while retrieving batch analysis status");
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<bool> ValidateBabyOwnership(int babyId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var baby = await _babyService.GetBabyByIdAsync(babyId);
            return baby != null && baby.UserId == userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating baby ownership for baby {BabyId}", babyId);
            return false;
        }
    }

    #endregion
}
