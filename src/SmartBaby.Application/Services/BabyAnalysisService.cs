using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartBaby.Core.DTOs;
using SmartBaby.Core.Interfaces;
using BabyAnalyzer.Grpc;
using System.Text.Json;
using Google.Protobuf;
using Grpc.Core;

namespace SmartBaby.Application.Services;

/// <summary>
/// Service implementation for Baby Analysis gRPC integration
/// </summary>
public class BabyAnalysisService : IBabyAnalysisService, IDisposable
{
    private readonly ILogger<BabyAnalysisService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAnalysisHistoryService _historyService;
    private readonly GrpcChannel _channel;
    private readonly BabyAnalyzerService.BabyAnalyzerServiceClient _grpcClient;
    private readonly string _grpcServerAddress;
    private bool _disposed;
    private readonly IPreviewGenerationService _previewService;

    public BabyAnalysisService(
        ILogger<BabyAnalysisService> logger,
        IConfiguration configuration,
        IAnalysisHistoryService historyService,
        IPreviewGenerationService previewService)
    {
        _logger = logger;
        _configuration = configuration;
        _historyService = historyService;
        _previewService = previewService;
        
        _grpcServerAddress = _configuration.GetValue<string>("BabyAnalyzer:GrpcAddress") 
            ?? "http://localhost:50051";
            
        _logger.LogInformation("Initializing Baby Analysis Service with gRPC address: {Address}", _grpcServerAddress);
        
        // Create gRPC channel
        _channel = GrpcChannel.ForAddress(_grpcServerAddress, new GrpcChannelOptions
        {
            HttpHandler = CreateHttpHandler(),
            MaxReceiveMessageSize = 100 * 1024 * 1024, // 100MB
            MaxSendMessageSize = 100 * 1024 * 1024, // 100MB
        });
        
        _grpcClient = new BabyAnalyzerService.BabyAnalyzerServiceClient(_channel);
    }

    #region Image Analysis

    public async Task<ImageAnalysisResponseDto> AnalyzeImageAsync(ImageAnalysisRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting image analysis for request: {RequestId}", request.RequestId);

            var grpcRequest = MapToGrpcImageRequest(request);
            var grpcResponse = await _grpcClient.AnalyzeImageAsync(grpcRequest, cancellationToken: cancellationToken);
            var response = MapFromGrpcImageResponse(grpcResponse);

            // Save analysis result if BabyId is provided
            if (request.BabyId.HasValue && response.Success)
            {
                // Attach preview (generate from bytes if not provided)
                var previewBase64 = request.PreviewImageBase64 ?? request.ImageBase64 ?? (request.ImageBytes != null ? Convert.ToBase64String(request.ImageBytes) : null);
                string? contentType = request.PreviewImageContentType ?? InferImageContentType(previewBase64);
                if (previewBase64 != null)
                {
                    var resized = await _previewService.ResizeImageAsync(previewBase64, contentType, maxWidth: 480, maxHeight: 480, ct: cancellationToken);
                    if (resized != null)
                    {
                        previewBase64 = resized.Base64;
                        contentType = resized.ContentType;
                    }
                }
                await SaveAnalysisResultInternalAsync(request.BabyId.Value, "image", response, previewBase64, contentType, cancellationToken, request.RequestId);
            }

            _logger.LogInformation("Image analysis completed: Success={Success}, Mood={Mood}", 
                response.Success, response.EmotionAnalysis?.DetectedMood);

            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error during image analysis: {Status} - {Detail}", ex.Status.StatusCode, ex.Status.Detail);
            return CreateErrorImageResponse(request.RequestId, $"gRPC Error: {ex.Status.Detail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during image analysis");
            return CreateErrorImageResponse(request.RequestId, ex.Message);
        }
    }

    public async IAsyncEnumerable<ImageAnalysisResponseDto> AnalyzeImageStreamAsync(
        IAsyncEnumerable<ImageAnalysisRequestDto> requests, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var call = _grpcClient.AnalyzeImageStream(cancellationToken: cancellationToken);
        
        // Start sending requests
        var sendTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var request in requests.WithCancellation(cancellationToken))
                {
                    var grpcRequest = MapToGrpcImageRequest(request);
                    await call.RequestStream.WriteAsync(grpcRequest);
                }
                await call.RequestStream.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending image stream requests");
            }
        }, cancellationToken);

        // Receive responses
        await foreach (var grpcResponse in call.ResponseStream.ReadAllAsync(cancellationToken))
        {
            yield return MapFromGrpcImageResponse(grpcResponse);
        }

        await sendTask;
    }

    #endregion

    #region Audio Analysis

    public async Task<AudioAnalysisResponseDto> AnalyzeAudioAsync(AudioAnalysisRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting audio analysis for request: {RequestId}", request.RequestId);

            var grpcRequest = MapToGrpcAudioRequest(request);
            var grpcResponse = await _grpcClient.AnalyzeAudioAsync(grpcRequest, cancellationToken: cancellationToken);
            var response = MapFromGrpcAudioResponse(grpcResponse);

            // Save analysis result if BabyId is provided
            if (request.BabyId.HasValue && response.Success)
            {
                string? preview = request.PreviewImageBase64;
                string? contentType = request.PreviewImageContentType;
                if (preview == null && request.AudioBytes != null)
                {
                    var generated = await _previewService.GenerateAudioPreviewAsync(request.AudioBytes, cancellationToken);
                    if (generated != null)
                    {
                        preview = generated.Base64;
                        contentType = generated.ContentType;
                    }
                }
                if (preview != null)
                {
                    await SaveAnalysisResultInternalAsync(request.BabyId.Value, "audio", response, preview, contentType, cancellationToken, request.RequestId);
                }
            }

            _logger.LogInformation("Audio analysis completed: Success={Success}, CryDetected={CryDetected}, Reason={Reason}", 
                response.Success, response.CryAnalysis?.CryDetected, response.CryAnalysis?.CryReason);

            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error during audio analysis: {Status} - {Detail}", ex.Status.StatusCode, ex.Status.Detail);
            return CreateErrorAudioResponse(request.RequestId, $"gRPC Error: {ex.Status.Detail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during audio analysis");
            return CreateErrorAudioResponse(request.RequestId, ex.Message);
        }
    }

    public async IAsyncEnumerable<AudioAnalysisResponseDto> AnalyzeAudioStreamAsync(
        IAsyncEnumerable<AudioAnalysisRequestDto> requests, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var call = _grpcClient.AnalyzeAudioStream(cancellationToken: cancellationToken);
        
        // Start sending requests
        var sendTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var request in requests.WithCancellation(cancellationToken))
                {
                    var grpcRequest = MapToGrpcAudioRequest(request);
                    await call.RequestStream.WriteAsync(grpcRequest);
                }
                await call.RequestStream.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending audio stream requests");
            }
        }, cancellationToken);

        // Receive responses
        await foreach (var grpcResponse in call.ResponseStream.ReadAllAsync(cancellationToken))
        {
            yield return MapFromGrpcAudioResponse(grpcResponse);
        }

        await sendTask;
    }

    #endregion

    #region Video Analysis

    public async Task<VideoAnalysisResponseDto> AnalyzeVideoAsync(VideoAnalysisRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting video analysis for request: {RequestId}", request.RequestId);

            var grpcRequest = MapToGrpcVideoRequest(request);
            var grpcResponse = await _grpcClient.AnalyzeVideoAsync(grpcRequest, cancellationToken: cancellationToken);
            var response = MapFromGrpcVideoResponse(grpcResponse);

            // Save analysis result if BabyId is provided and SaveResults (default true when not specified)
            if (request.BabyId.HasValue && response.Success)
            {
                var shouldSave = request.Options?.SaveResults != false; // treat null as true
                if (!shouldSave)
                {
                    _logger.LogDebug("Video analysis result not saved because SaveResults=false (RequestId={RequestId})", response.RequestId);
                }
                else
                {
                    string? preview = request.PreviewImageBase64;
                    string? contentType = request.PreviewImageContentType ?? InferImageContentType(preview);

                    if (preview == null && request.VideoBytes != null)
                    {
                        try
                        {
                            var generated = await _previewService.GenerateVideoPreviewAsync(request.VideoBytes, cancellationToken);
                            if (generated != null)
                            {
                                preview = generated.Base64;
                                contentType = generated.ContentType;
                            }
                            else
                            {
                                _logger.LogInformation("No video preview generated (possibly missing FFmpeg). Saving without preview. RequestId={RequestId}", response.RequestId);
                            }
                        }
                        catch (Exception genEx)
                        {
                            _logger.LogWarning(genEx, "Error generating video preview. Proceeding to save analysis without preview. RequestId={RequestId}", response.RequestId);
                        }
                    }

                    var saved = await SaveAnalysisResultInternalAsync(request.BabyId.Value, "video", response, preview, contentType, cancellationToken, request.RequestId);
                    if (!saved)
                    {
                        _logger.LogWarning("Failed to persist video analysis result (RequestId={RequestId})", response.RequestId);
                    }
                }
            }

            _logger.LogInformation("Video analysis completed: Success={Success}, Duration={Duration}s", 
                response.Success, response.AnalysisResult?.VideoInfo?.Duration);

            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error during video analysis: {Status} - {Detail}", ex.Status.StatusCode, ex.Status.Detail);
            return CreateErrorVideoResponse(request.RequestId, $"gRPC Error: {ex.Status.Detail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during video analysis");
            return CreateErrorVideoResponse(request.RequestId, ex.Message);
        }
    }

    public async IAsyncEnumerable<VideoAnalysisResponseDto> AnalyzeVideoStreamAsync(
        IAsyncEnumerable<VideoAnalysisRequestDto> requests, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Note: This would require implementing video streaming in chunks
        // For now, process each request individually
        await foreach (var request in requests.WithCancellation(cancellationToken))
        {
            yield return await AnalyzeVideoAsync(request, cancellationToken);
        }
    }

    #endregion

    #region Multimodal Analysis

    public async Task<MultimodalAnalysisResponseDto> AnalyzeMultimodalAsync(MultimodalAnalysisRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting multimodal analysis for request: {RequestId}", request.RequestId);

            var grpcRequest = MapToGrpcMultimodalRequest(request);
            var grpcResponse = await _grpcClient.AnalyzeMultimodalAsync(grpcRequest, cancellationToken: cancellationToken);
            var response = MapFromGrpcMultimodalResponse(grpcResponse);

            // Save analysis result if BabyId is provided
            if (request.BabyId.HasValue && response.Success)
            {
                string? preview = request.PreviewImageBase64 
                               ?? request.ImageRequest?.PreviewImageBase64 
                               ?? request.ImageRequest?.ImageBase64;
                string? contentType = request.PreviewImageContentType 
                                  ?? request.ImageRequest?.PreviewImageContentType 
                                  ?? InferImageContentType(preview);
                if (preview != null)
                {
                    var resized = await _previewService.ResizeImageAsync(preview, contentType, 480, 480, ct: cancellationToken);
                    if (resized != null)
                    {
                        preview = resized.Base64;
                        contentType = resized.ContentType;
                    }
                }
                await SaveAnalysisResultInternalAsync(request.BabyId.Value, "multimodal", response, preview, contentType, cancellationToken, request.RequestId);
            }

            _logger.LogInformation("Multimodal analysis completed: Success={Success}", response.Success);

            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error during multimodal analysis: {Status} - {Detail}", ex.Status.StatusCode, ex.Status.Detail);
            return CreateErrorMultimodalResponse(request.RequestId, $"gRPC Error: {ex.Status.Detail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during multimodal analysis");
            return CreateErrorMultimodalResponse(request.RequestId, ex.Message);
        }
    }

    #endregion

    #region Real-time Analysis

    public async IAsyncEnumerable<RealtimeAnalysisResponseDto> StartRealtimeAnalysisAsync(
        RealtimeAnalysisRequestDto request, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var call = _grpcClient.StartRealtimeAnalysis(MapToGrpcRealtimeRequest(request), cancellationToken: cancellationToken);
        
        await foreach (var grpcResponse in call.ResponseStream.ReadAllAsync(cancellationToken))
        {
            var response = MapFromGrpcRealtimeResponse(grpcResponse);
            
            // Save real-time updates if BabyId is provided
            if (request.BabyId.HasValue)
            {
                // This would be handled by IRealtimeSessionService
                _logger.LogDebug("Real-time update received for session: {SessionId}", response.SessionId);
            }
            
            yield return response;
        }
    }

    public Task<bool> StopRealtimeAnalysisAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            // The gRPC service handles session stopping automatically when the stream is cancelled
            // Additional cleanup logic can be added here
            _logger.LogInformation("Stopping real-time analysis session: {SessionId}", sessionId);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping real-time analysis session: {SessionId}", sessionId);
            return Task.FromResult(false);
        }
    }

    #endregion

    #region Health and Status

    public async Task<HealthStatusResponseDto> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var grpcResponse = await _grpcClient.GetHealthStatusAsync(new Google.Protobuf.WellKnownTypes.Empty(), cancellationToken: cancellationToken);
            return MapFromGrpcHealthResponse(grpcResponse);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting health status: {Status} - {Detail}", ex.Status.StatusCode, ex.Status.Detail);
            return new HealthStatusResponseDto
            {
                OverallHealth = SmartBaby.Core.DTOs.ServiceHealth.Unhealthy,
                Timestamp = DateTime.UtcNow,
                ComponentHealth = new List<ComponentHealthDto>
                {
                    new ComponentHealthDto
                    {
                        ComponentName = "gRPC Connection",
                        HealthStatus = SmartBaby.Core.DTOs.ServiceHealth.Unhealthy,
                        ErrorMessage = ex.Status.Detail,
                        LastCheck = DateTime.UtcNow
                    }
                }
            };
        }
    }

    public async Task<ModelStatusResponseDto> GetModelStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var grpcResponse = await _grpcClient.GetModelStatusAsync(new Google.Protobuf.WellKnownTypes.Empty(), cancellationToken: cancellationToken);
            return MapFromGrpcModelStatusResponse(grpcResponse);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting model status: {Status} - {Detail}", ex.Status.StatusCode, ex.Status.Detail);
            return new ModelStatusResponseDto
            {
                Models = new List<ModelInfoDto>(),
                Timestamp = DateTime.UtcNow
            };
        }
    }

    #endregion

    #region Analysis History Management

    public async Task<bool> SaveAnalysisResultAsync(int babyId, string analysisType, object analysisResult, CancellationToken cancellationToken = default)
    {
        try
        {
            var analysisHistory = new AnalysisHistoryDto
            {
                BabyId = babyId,
                AnalysisType = analysisType,
                CreatedAt = DateTime.UtcNow,
                Success = GetAnalysisSuccess(analysisResult),
                ResultData = JsonSerializer.Serialize(analysisResult),
                Confidence = GetAnalysisConfidence(analysisResult),
                PrimaryResult = GetPrimaryResult(analysisResult),
                RequestId = GetAnalysisRequestId(analysisResult) ?? string.Empty
            };

            await _historyService.SaveAnalysisAsync(analysisHistory, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving analysis result for baby {BabyId}", babyId);
            return false;
        }
    }

    private async Task<bool> SaveAnalysisResultInternalAsync(int babyId, string analysisType, object analysisResult, string? previewBase64, string? previewContentType, CancellationToken cancellationToken, string? requestId = null)
    {
        try
        {
            var analysisHistory = new AnalysisHistoryDto
            {
                BabyId = babyId,
                AnalysisType = analysisType,
                CreatedAt = DateTime.UtcNow,
                Success = GetAnalysisSuccess(analysisResult),
                ResultData = JsonSerializer.Serialize(analysisResult),
                Confidence = GetAnalysisConfidence(analysisResult),
                PrimaryResult = GetPrimaryResult(analysisResult),
                RequestId = requestId ?? GetAnalysisRequestId(analysisResult) ?? string.Empty,
                PreviewImageBase64 = CleanBase64(previewBase64),
                PreviewImageContentType = previewContentType
            };

            await _historyService.SaveAnalysisAsync(analysisHistory, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving analysis result (internal) for baby {BabyId}", babyId);
            return false;
        }
    }

    public async Task<IEnumerable<AnalysisHistoryDto>> GetAnalysisHistoryAsync(
        int babyId, 
        string? analysisType = null, 
        DateTime? from = null, 
        DateTime? to = null, 
        CancellationToken cancellationToken = default)
    {
        var filter = new AnalysisHistoryFilter
        {
            AnalysisType = analysisType,
            From = from,
            To = to
        };

        return await _historyService.GetAnalysisHistoryAsync(babyId, filter, cancellationToken);
    }

    public async Task<AnalysisHistoryDto?> GetAnalysisResultAsync(int analysisId, CancellationToken cancellationToken = default)
    {
        return await _historyService.GetAnalysisAsync(analysisId, cancellationToken);
    }

    #endregion

    #region Batch Analysis

    public Task<BatchAnalysisResponseDto> SubmitBatchAnalysisAsync(BatchAnalysisRequestDto request, CancellationToken cancellationToken = default)
    {
        // This would be implemented to handle batch processing
        // For now, return a placeholder response
        return Task.FromResult(new BatchAnalysisResponseDto
        {
            BatchId = request.BatchId,
            Success = true,
            Status = SmartBaby.Core.DTOs.BatchAnalysisStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            TotalItems = request.AnalysisItems.Count,
            ProcessedItems = 0
        });
    }

    public Task<BatchAnalysisStatusDto> GetBatchAnalysisStatusAsync(string batchId, CancellationToken cancellationToken = default)
    {
        // This would query the batch analysis status from the database
        // For now, return a placeholder response
        return Task.FromResult(new BatchAnalysisStatusDto
        {
            BatchId = batchId,
            Status = SmartBaby.Core.DTOs.BatchAnalysisStatus.Processing,
            TotalItems = 0,
            ProcessedItems = 0,
            SuccessfulItems = 0,
            FailedItems = 0,
            ProgressPercentage = 0,
            CreatedAt = DateTime.UtcNow,
            Results = new List<AnalysisItemResultDto>()
        });
    }

    #endregion

    #region Private Helper Methods

    private HttpMessageHandler CreateHttpHandler()
    {
        var handler = new HttpClientHandler();
        
        // Add any custom configuration for the HTTP handler
        // For development, you might want to disable SSL validation
        if (_configuration.GetValue<bool>("BabyAnalyzer:DisableSslValidation"))
        {
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        }
        
        return handler;
    }

    private static bool GetAnalysisSuccess(object analysisResult)
    {
        return analysisResult switch
        {
            ImageAnalysisResponseDto image => image.Success,
            AudioAnalysisResponseDto audio => audio.Success,
            VideoAnalysisResponseDto video => video.Success,
            MultimodalAnalysisResponseDto multimodal => multimodal.Success,
            _ => false
        };
    }

    private static float? GetAnalysisConfidence(object analysisResult)
    {
        return analysisResult switch
        {
            ImageAnalysisResponseDto image => image.EmotionAnalysis?.Confidence,
            AudioAnalysisResponseDto audio => audio.CryAnalysis?.Confidence,
            VideoAnalysisResponseDto video => video.AnalysisResult?.Summary?.VisualStats?.AverageConfidence,
            MultimodalAnalysisResponseDto multimodal => multimodal.FusionAnalysis?.Confidence,
            _ => null
        };
    }

    private static string? GetPrimaryResult(object analysisResult)
    {
        return analysisResult switch
        {
            ImageAnalysisResponseDto image => image.EmotionAnalysis?.DetectedMood,
            AudioAnalysisResponseDto audio => audio.CryAnalysis?.CryReason,
            VideoAnalysisResponseDto video => video.AnalysisResult?.Summary?.OverallAssessment,
            MultimodalAnalysisResponseDto multimodal => multimodal.FusionAnalysis?.OverallState,
            _ => null
        };
    }

    private static string? GetAnalysisRequestId(object analysisResult)
    {
        return analysisResult switch
        {
            ImageAnalysisResponseDto image => image.RequestId,
            AudioAnalysisResponseDto audio => audio.RequestId,
            VideoAnalysisResponseDto video => video.RequestId,
            MultimodalAnalysisResponseDto multi => multi.RequestId,
            _ => null
        };
    }

    private static ImageAnalysisResponseDto CreateErrorImageResponse(string? requestId, string errorMessage)
    {
        return new ImageAnalysisResponseDto
        {
            Success = false,
            ErrorMessage = errorMessage,
            RequestId = requestId,
            Timestamp = DateTime.UtcNow
        };
    }

    private static AudioAnalysisResponseDto CreateErrorAudioResponse(string? requestId, string errorMessage)
    {
        return new AudioAnalysisResponseDto
        {
            Success = false,
            ErrorMessage = errorMessage,
            RequestId = requestId,
            Timestamp = DateTime.UtcNow
        };
    }

    private static VideoAnalysisResponseDto CreateErrorVideoResponse(string? requestId, string errorMessage)
    {
        return new VideoAnalysisResponseDto
        {
            Success = false,
            ErrorMessage = errorMessage,
            RequestId = requestId,
            Timestamp = DateTime.UtcNow
        };
    }

    private static MultimodalAnalysisResponseDto CreateErrorMultimodalResponse(string? requestId, string errorMessage)
    {
        return new MultimodalAnalysisResponseDto
        {
            Success = false,
            ErrorMessage = errorMessage,
            RequestId = requestId,
            Timestamp = DateTime.UtcNow
        };
    }

    private static string? CleanBase64(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var commaIdx = input.IndexOf(',');
        if (input.StartsWith("data:") && commaIdx > -1)
        {
            return input[(commaIdx + 1)..];
        }
        return input.Trim();
    }

    private static string? InferImageContentType(string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return null;
        // Basic magic number inspection (after cleaning potential data URI)
        var data = CleanBase64(base64);
        try
        {
            var bytes = Convert.FromBase64String(data!);
            if (bytes.Length >= 4)
            {
                // PNG
                if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) return "image/png";
                // JPEG
                if (bytes[0] == 0xFF && bytes[1] == 0xD8) return "image/jpeg";
                // GIF
                if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46) return "image/gif";
            }
        }
        catch { }
        return "image/png"; // default
    }

    #endregion

    #region Mapping Methods - To be implemented

    // These methods would map between DTOs and gRPC messages
    // Due to space constraints, I'm showing the structure but not the full implementation
    
    private BabyAnalyzer.Grpc.ImageAnalysisRequest MapToGrpcImageRequest(ImageAnalysisRequestDto request)
    {
        var grpcRequest = new BabyAnalyzer.Grpc.ImageAnalysisRequest
        {
            RequestId = request.RequestId ?? Guid.NewGuid().ToString()
        };

        if (request.ImageBytes != null)
        {
            grpcRequest.ImageBytes = ByteString.CopyFrom(request.ImageBytes);
        }
        else if (!string.IsNullOrEmpty(request.ImageBase64))
        {
            grpcRequest.ImageBase64 = request.ImageBase64;
        }
        else if (!string.IsNullOrEmpty(request.ImagePath))
        {
            grpcRequest.ImagePath = request.ImagePath;
        }

        // Map analysis options
        if (request.Options != null)
        {
            grpcRequest.Options = new BabyAnalyzer.Grpc.AnalysisOptions
            {
                ConfidenceThreshold = request.Options.ConfidenceThreshold,
                IncludeDebugInfo = request.Options.IncludeDebugInfo
            };
            grpcRequest.Options.EnabledModels.AddRange(request.Options.EnabledModels);
        }

        return grpcRequest;
    }

    private ImageAnalysisResponseDto MapFromGrpcImageResponse(BabyAnalyzer.Grpc.ImageAnalysisResponse grpcResponse)
    {
        var response = new ImageAnalysisResponseDto
        {
            Success = grpcResponse.Success,
            ErrorMessage = grpcResponse.ErrorMessage,
            RequestId = grpcResponse.RequestId,
            Timestamp = grpcResponse.Timestamp?.ToDateTime() ?? DateTime.UtcNow
        };

        if (grpcResponse.EmotionAnalysis != null)
        {
            response.EmotionAnalysis = new EmotionAnalysisDto
            {
                DetectedMood = grpcResponse.EmotionAnalysis.DetectedMood,
                DominantEmotion = grpcResponse.EmotionAnalysis.DominantEmotion,
                Confidence = grpcResponse.EmotionAnalysis.Confidence,
                MoodCategory = grpcResponse.EmotionAnalysis.MoodCategory,
                AllEmotions = grpcResponse.EmotionAnalysis.AllEmotions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }

        if (grpcResponse.Metadata != null)
        {
            response.Metadata = new AnalysisMetadataDto
            {
                AnalyzerVersion = grpcResponse.Metadata.AnalyzerVersion,
                ProcessingTime = grpcResponse.Metadata.ProcessingTime,
                HardwareInfo = grpcResponse.Metadata.HardwareInfo,
                ModelsUsed = grpcResponse.Metadata.ModelsUsed.ToList()
            };
        }

        return response;
    }

    private BabyAnalyzer.Grpc.AudioAnalysisRequest MapToGrpcAudioRequest(AudioAnalysisRequestDto request)
    {
        var grpcRequest = new BabyAnalyzer.Grpc.AudioAnalysisRequest
        {
            RequestId = request.RequestId ?? Guid.NewGuid().ToString()
        };

        if (request.AudioBytes != null)
        {
            grpcRequest.AudioBytes = ByteString.CopyFrom(request.AudioBytes);
        }
        else if (!string.IsNullOrEmpty(request.AudioBase64))
        {
            grpcRequest.AudioBase64 = request.AudioBase64;
        }
        else if (!string.IsNullOrEmpty(request.AudioPath))
        {
            grpcRequest.AudioPath = request.AudioPath;
        }

        // Map audio format
        if (request.AudioFormat != null)
        {
            grpcRequest.AudioFormat = new BabyAnalyzer.Grpc.AudioFormat
            {
                SampleRate = request.AudioFormat.SampleRate,
                Channels = request.AudioFormat.Channels,
                BitDepth = request.AudioFormat.BitDepth,
                Encoding = request.AudioFormat.Encoding switch
                {
                    "LINEAR_PCM" => BabyAnalyzer.Grpc.AudioEncoding.LinearPcm,
                    "FLAC" => BabyAnalyzer.Grpc.AudioEncoding.Flac,
                    "MP3" => BabyAnalyzer.Grpc.AudioEncoding.Mp3,
                    "WAV" => BabyAnalyzer.Grpc.AudioEncoding.Wav,
                    "OGG" => BabyAnalyzer.Grpc.AudioEncoding.Ogg,
                    _ => BabyAnalyzer.Grpc.AudioEncoding.LinearPcm
                }
            };
        }

        // Map analysis options
        if (request.Options != null)
        {
            grpcRequest.Options = new BabyAnalyzer.Grpc.AnalysisOptions
            {
                ConfidenceThreshold = request.Options.ConfidenceThreshold,
                IncludeDebugInfo = request.Options.IncludeDebugInfo
            };
            grpcRequest.Options.EnabledModels.AddRange(request.Options.EnabledModels);
            
            foreach (var param in request.Options.CustomParameters)
            {
                grpcRequest.Options.CustomParameters.Add(param.Key, param.Value);
            }
        }

        return grpcRequest;
    }

    private AudioAnalysisResponseDto MapFromGrpcAudioResponse(BabyAnalyzer.Grpc.AudioAnalysisResponse grpcResponse)
    {
        var response = new AudioAnalysisResponseDto
        {
            Success = grpcResponse.Success,
            ErrorMessage = grpcResponse.ErrorMessage,
            RequestId = grpcResponse.RequestId,
            Timestamp = grpcResponse.Timestamp?.ToDateTime() ?? DateTime.UtcNow
        };

        if (grpcResponse.CryAnalysis != null)
        {
            response.CryAnalysis = new CryAnalysisDto
            {
                CryDetected = grpcResponse.CryAnalysis.CryDetected,
                CryReason = grpcResponse.CryAnalysis.CryReason,
                Confidence = grpcResponse.CryAnalysis.Confidence,
                ModelUsed = grpcResponse.CryAnalysis.ModelUsed,
                AllPredictions = grpcResponse.CryAnalysis.AllPredictions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };

            if (grpcResponse.CryAnalysis.AudioFeatures != null)
            {
                response.CryAnalysis.AudioFeatures = new AudioFeaturesDto
                {
                    RmsEnergy = grpcResponse.CryAnalysis.AudioFeatures.RmsEnergy,
                    DominantFrequency = grpcResponse.CryAnalysis.AudioFeatures.DominantFrequency,
                    SpectralCentroid = grpcResponse.CryAnalysis.AudioFeatures.SpectralCentroid,
                    ZeroCrossingRate = grpcResponse.CryAnalysis.AudioFeatures.ZeroCrossingRate,
                    MfccFeatures = grpcResponse.CryAnalysis.AudioFeatures.MfccFeatures.ToList()
                };
            }
        }

        if (grpcResponse.Metadata != null)
        {
            response.Metadata = new AnalysisMetadataDto
            {
                AnalyzerVersion = grpcResponse.Metadata.AnalyzerVersion,
                ProcessingTime = grpcResponse.Metadata.ProcessingTime,
                HardwareInfo = grpcResponse.Metadata.HardwareInfo,
                ModelsUsed = grpcResponse.Metadata.ModelsUsed.ToList()
            };
        }

        return response;
    }
    private BabyAnalyzer.Grpc.VideoAnalysisRequest MapToGrpcVideoRequest(VideoAnalysisRequestDto request)
    {
        var grpcRequest = new BabyAnalyzer.Grpc.VideoAnalysisRequest
        {
            RequestId = request.RequestId ?? Guid.NewGuid().ToString()
        };

        if (request.VideoBytes != null)
        {
            grpcRequest.VideoBytes = ByteString.CopyFrom(request.VideoBytes);
        }
        else if (!string.IsNullOrEmpty(request.VideoPath))
        {
            grpcRequest.VideoPath = request.VideoPath;
        }

        // Map video analysis options
        if (request.Options != null)
        {
            grpcRequest.Options = new BabyAnalyzer.Grpc.VideoAnalysisOptions
            {
                FrameInterval = request.Options.FrameInterval,
                AudioSegmentDuration = request.Options.AudioSegmentDuration,
                SaveResults = request.Options.SaveResults,
                EnableFusion = request.Options.EnableFusion,
                OutputDirectory = request.Options.OutputDirectory ?? ""
            };
        }

        return grpcRequest;
    }

    private VideoAnalysisResponseDto MapFromGrpcVideoResponse(BabyAnalyzer.Grpc.VideoAnalysisResponse grpcResponse)
    {
        var response = new VideoAnalysisResponseDto
        {
            Success = grpcResponse.Success,
            ErrorMessage = grpcResponse.ErrorMessage,
            RequestId = grpcResponse.RequestId,
            Timestamp = grpcResponse.Timestamp?.ToDateTime() ?? DateTime.UtcNow
        };

        if (grpcResponse.AnalysisResult != null)
        {
            response.AnalysisResult = new VideoAnalysisResultDto
            {
                VideoInfo = grpcResponse.AnalysisResult.VideoInfo != null ? new VideoInfoDto
                {
                    FilePath = grpcResponse.AnalysisResult.VideoInfo.FilePath,
                    Duration = grpcResponse.AnalysisResult.VideoInfo.Duration,
                    Fps = grpcResponse.AnalysisResult.VideoInfo.Fps,
                    TotalFrames = grpcResponse.AnalysisResult.VideoInfo.TotalFrames,
                    Resolution = grpcResponse.AnalysisResult.VideoInfo.Resolution != null ? new VideoResolutionDto
                    {
                        Width = grpcResponse.AnalysisResult.VideoInfo.Resolution.Width,
                        Height = grpcResponse.AnalysisResult.VideoInfo.Resolution.Height
                    } : null
                } : null,
                VisualAnalysis = grpcResponse.AnalysisResult.VisualAnalysis.Select(va => new EmotionAnalysisDto
                {
                    DetectedMood = va.DetectedMood,
                    DominantEmotion = va.DominantEmotion,
                    Confidence = va.Confidence,
                    MoodCategory = va.MoodCategory,
                    AllEmotions = va.AllEmotions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                }).ToList(),
                AudioAnalysis = grpcResponse.AnalysisResult.AudioAnalysis.Select(aa => new CryAnalysisDto
                {
                    CryDetected = aa.CryDetected,
                    CryReason = aa.CryReason,
                    Confidence = aa.Confidence,
                    ModelUsed = aa.ModelUsed,
                    AllPredictions = aa.AllPredictions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    AudioFeatures = aa.AudioFeatures != null ? new AudioFeaturesDto
                    {
                        RmsEnergy = aa.AudioFeatures.RmsEnergy,
                        DominantFrequency = aa.AudioFeatures.DominantFrequency,
                        SpectralCentroid = aa.AudioFeatures.SpectralCentroid,
                        ZeroCrossingRate = aa.AudioFeatures.ZeroCrossingRate,
                        MfccFeatures = aa.AudioFeatures.MfccFeatures.ToList()
                    } : null
                }).ToList()
            };

            if (grpcResponse.AnalysisResult.Summary != null)
            {
                response.AnalysisResult.Summary = new AnalysisSummaryDto
                {
                    OverallAssessment = grpcResponse.AnalysisResult.Summary.OverallAssessment,
                    KeyFindings = grpcResponse.AnalysisResult.Summary.KeyFindings.ToList(),
                    VisualStats = grpcResponse.AnalysisResult.Summary.VisualStats != null ? new VisualStatisticsDto
                    {
                        TotalFrames = grpcResponse.AnalysisResult.Summary.VisualStats.TotalFrames,
                        SuccessfulFrames = grpcResponse.AnalysisResult.Summary.VisualStats.SuccessfulFrames,
                        AverageConfidence = grpcResponse.AnalysisResult.Summary.VisualStats.AverageConfidence,
                        MostCommonMood = grpcResponse.AnalysisResult.Summary.VisualStats.MostCommonMood,
                        MoodDistribution = grpcResponse.AnalysisResult.Summary.VisualStats.MoodDistribution.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                    } : null,
                    AudioStats = grpcResponse.AnalysisResult.Summary.AudioStats != null ? new AudioStatisticsDto
                    {
                        TotalSegments = grpcResponse.AnalysisResult.Summary.AudioStats.TotalSegments,
                        SuccessfulSegments = grpcResponse.AnalysisResult.Summary.AudioStats.SuccessfulSegments,
                        CryPercentage = grpcResponse.AnalysisResult.Summary.AudioStats.CryPercentage,
                        MostCommonCryReason = grpcResponse.AnalysisResult.Summary.AudioStats.MostCommonCryReason,
                        CryReasonDistribution = grpcResponse.AnalysisResult.Summary.AudioStats.CryReasonDistribution.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                    } : null
                };
            }
        }

        if (grpcResponse.Metadata != null)
        {
            response.Metadata = new AnalysisMetadataDto
            {
                AnalyzerVersion = grpcResponse.Metadata.AnalyzerVersion,
                ProcessingTime = grpcResponse.Metadata.ProcessingTime,
                HardwareInfo = grpcResponse.Metadata.HardwareInfo,
                ModelsUsed = grpcResponse.Metadata.ModelsUsed.ToList()
            };
        }

        return response;
    }
    private BabyAnalyzer.Grpc.MultimodalAnalysisRequest MapToGrpcMultimodalRequest(MultimodalAnalysisRequestDto request)
    {
        var grpcRequest = new BabyAnalyzer.Grpc.MultimodalAnalysisRequest
        {
            RequestId = request.RequestId ?? Guid.NewGuid().ToString()
        };

        if (request.ImageRequest != null)
        {
            grpcRequest.ImageRequest = MapToGrpcImageRequest(request.ImageRequest);
        }

        if (request.AudioRequest != null)
        {
            grpcRequest.AudioRequest = MapToGrpcAudioRequest(request.AudioRequest);
        }

        if (request.Options != null)
        {
            grpcRequest.Options = new BabyAnalyzer.Grpc.AnalysisOptions
            {
                ConfidenceThreshold = request.Options.ConfidenceThreshold,
                IncludeDebugInfo = request.Options.IncludeDebugInfo
            };
            grpcRequest.Options.EnabledModels.AddRange(request.Options.EnabledModels);
            
            foreach (var param in request.Options.CustomParameters)
            {
                grpcRequest.Options.CustomParameters.Add(param.Key, param.Value);
            }
        }

        return grpcRequest;
    }

    private MultimodalAnalysisResponseDto MapFromGrpcMultimodalResponse(BabyAnalyzer.Grpc.MultimodalAnalysisResponse grpcResponse)
    {
        var response = new MultimodalAnalysisResponseDto
        {
            Success = grpcResponse.Success,
            ErrorMessage = grpcResponse.ErrorMessage,
            RequestId = grpcResponse.RequestId,
            Timestamp = grpcResponse.Timestamp?.ToDateTime() ?? DateTime.UtcNow
        };

        if (grpcResponse.ImageResponse != null)
        {
            response.ImageResponse = MapFromGrpcImageResponse(grpcResponse.ImageResponse);
        }

        if (grpcResponse.AudioResponse != null)
        {
            response.AudioResponse = MapFromGrpcAudioResponse(grpcResponse.AudioResponse);
        }

        if (grpcResponse.FusionAnalysis != null)
        {
            response.FusionAnalysis = new FusionAnalysisDto
            {
                OverallState = grpcResponse.FusionAnalysis.OverallState,
                AlertLevel = (SmartBaby.Core.DTOs.AlertLevel)grpcResponse.FusionAnalysis.AlertLevel,
                Confidence = grpcResponse.FusionAnalysis.Confidence,
                PrimaryIndicator = grpcResponse.FusionAnalysis.PrimaryIndicator,
                Recommendations = grpcResponse.FusionAnalysis.Recommendations.ToList(),
                MethodUsed = (SmartBaby.Core.DTOs.FusionMethod)grpcResponse.FusionAnalysis.MethodUsed
            };
        }

        return response;
    }

    private BabyAnalyzer.Grpc.RealtimeAnalysisRequest MapToGrpcRealtimeRequest(RealtimeAnalysisRequestDto request)
    {
        var grpcRequest = new BabyAnalyzer.Grpc.RealtimeAnalysisRequest
        {
            SessionId = request.SessionId
        };

        if (request.Settings != null)
        {
            grpcRequest.Settings = new BabyAnalyzer.Grpc.RealtimeSettings
            {
                VideoDeviceId = request.Settings.VideoDeviceId,
                FrameAnalysisInterval = request.Settings.FrameAnalysisInterval,
                AudioAnalysisDuration = request.Settings.AudioAnalysisDuration,
                EnableVideoDisplay = request.Settings.EnableVideoDisplay,
                EnableOverlay = request.Settings.EnableOverlay
            };

            if (request.Settings.AudioFormat != null)
            {
                grpcRequest.Settings.AudioFormat = new BabyAnalyzer.Grpc.AudioFormat
                {
                    SampleRate = request.Settings.AudioFormat.SampleRate,
                    Channels = request.Settings.AudioFormat.Channels,
                    BitDepth = request.Settings.AudioFormat.BitDepth,
                    Encoding = request.Settings.AudioFormat.Encoding switch
                    {
                        "LINEAR_PCM" => BabyAnalyzer.Grpc.AudioEncoding.LinearPcm,
                        "FLAC" => BabyAnalyzer.Grpc.AudioEncoding.Flac,
                        "MP3" => BabyAnalyzer.Grpc.AudioEncoding.Mp3,
                        "WAV" => BabyAnalyzer.Grpc.AudioEncoding.Wav,
                        "OGG" => BabyAnalyzer.Grpc.AudioEncoding.Ogg,
                        _ => BabyAnalyzer.Grpc.AudioEncoding.LinearPcm
                    }
                };
            }
        }

        return grpcRequest;
    }

    private RealtimeAnalysisResponseDto MapFromGrpcRealtimeResponse(BabyAnalyzer.Grpc.RealtimeAnalysisResponse grpcResponse)
    {
        var response = new RealtimeAnalysisResponseDto
        {
            SessionId = grpcResponse.SessionId,
            Timestamp = grpcResponse.Timestamp?.ToDateTime() ?? DateTime.UtcNow
        };

        if (grpcResponse.Update != null)
        {
            response.Update = new AnalysisUpdateDto
            {
                UpdateType = (SmartBaby.Core.DTOs.UpdateType)grpcResponse.Update.UpdateType
            };

            switch (grpcResponse.Update.UpdateDataCase)
            {
                case BabyAnalyzer.Grpc.AnalysisUpdate.UpdateDataOneofCase.EmotionData:
                    response.Update.EmotionData = new EmotionAnalysisDto
                    {
                        DetectedMood = grpcResponse.Update.EmotionData.DetectedMood,
                        DominantEmotion = grpcResponse.Update.EmotionData.DominantEmotion,
                        Confidence = grpcResponse.Update.EmotionData.Confidence,
                        MoodCategory = grpcResponse.Update.EmotionData.MoodCategory,
                        AllEmotions = grpcResponse.Update.EmotionData.AllEmotions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                    };
                    break;
                case BabyAnalyzer.Grpc.AnalysisUpdate.UpdateDataOneofCase.CryData:
                    response.Update.CryData = new CryAnalysisDto
                    {
                        CryDetected = grpcResponse.Update.CryData.CryDetected,
                        CryReason = grpcResponse.Update.CryData.CryReason,
                        Confidence = grpcResponse.Update.CryData.Confidence,
                        ModelUsed = grpcResponse.Update.CryData.ModelUsed,
                        AllPredictions = grpcResponse.Update.CryData.AllPredictions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                    };
                    break;
                case BabyAnalyzer.Grpc.AnalysisUpdate.UpdateDataOneofCase.FusionData:
                    response.Update.FusionData = new FusionAnalysisDto
                    {
                        OverallState = grpcResponse.Update.FusionData.OverallState,
                        AlertLevel = (SmartBaby.Core.DTOs.AlertLevel)grpcResponse.Update.FusionData.AlertLevel,
                        Confidence = grpcResponse.Update.FusionData.Confidence,
                        PrimaryIndicator = grpcResponse.Update.FusionData.PrimaryIndicator,
                        Recommendations = grpcResponse.Update.FusionData.Recommendations.ToList(),
                        MethodUsed = (SmartBaby.Core.DTOs.FusionMethod)grpcResponse.Update.FusionData.MethodUsed
                    };
                    break;
                case BabyAnalyzer.Grpc.AnalysisUpdate.UpdateDataOneofCase.SystemStatus:
                    response.Update.SystemStatus = new SystemStatusDto
                    {
                        CpuUsage = grpcResponse.Update.SystemStatus.CpuUsage,
                        MemoryUsage = grpcResponse.Update.SystemStatus.MemoryUsage,
                        ActiveSessions = grpcResponse.Update.SystemStatus.ActiveSessions,
                        UptimeSeconds = grpcResponse.Update.SystemStatus.UptimeSeconds
                    };
                    break;
            }
        }

        return response;
    }

    private HealthStatusResponseDto MapFromGrpcHealthResponse(BabyAnalyzer.Grpc.HealthStatusResponse grpcResponse)
    {
        return new HealthStatusResponseDto
        {
            OverallHealth = (SmartBaby.Core.DTOs.ServiceHealth)grpcResponse.OverallHealth,
            Timestamp = grpcResponse.Timestamp?.ToDateTime() ?? DateTime.UtcNow,
            Version = grpcResponse.Version,
            ComponentHealth = grpcResponse.ComponentHealth.Select(ch => new ComponentHealthDto
            {
                ComponentName = ch.ComponentName,
                HealthStatus = (SmartBaby.Core.DTOs.ServiceHealth)ch.HealthStatus,
                ErrorMessage = ch.ErrorMessage,
                LastCheck = ch.LastCheck?.ToDateTime() ?? DateTime.UtcNow
            }).ToList()
        };
    }

    private ModelStatusResponseDto MapFromGrpcModelStatusResponse(BabyAnalyzer.Grpc.ModelStatusResponse grpcResponse)
    {
        return new ModelStatusResponseDto
        {
            Timestamp = grpcResponse.Timestamp?.ToDateTime() ?? DateTime.UtcNow,
            Models = grpcResponse.Models.Select(m => new ModelInfoDto
            {
                ModelName = m.ModelName,
                ModelType = m.ModelType,
                Status = (SmartBaby.Core.DTOs.ModelStatus)m.Status,
                Version = m.Version,
                FilePath = m.FilePath,
                LoadedAt = m.LoadedAt?.ToDateTime() ?? DateTime.UtcNow
            }).ToList()
        };
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (!_disposed)
        {
            _channel?.Dispose();
            _disposed = true;
        }
    }

    #endregion
}
