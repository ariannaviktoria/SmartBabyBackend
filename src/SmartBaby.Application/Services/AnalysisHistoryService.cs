using Microsoft.Extensions.Logging;
using SmartBaby.Core.DTOs;
using SmartBaby.Core.Entities;
using SmartBaby.Core.Interfaces;
using System.Text.Json;
using AutoMapper;

namespace SmartBaby.Application.Services;

/// <summary>
/// Service for managing analysis history and persistence
/// </summary>
public class AnalysisHistoryService : IAnalysisHistoryService
{
    private readonly IRepository<BabyAnalysis> _analysisRepository;
    private readonly IRepository<AnalysisTag> _tagRepository;
    private readonly IRepository<AnalysisAlert> _alertRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AnalysisHistoryService> _logger;

    public AnalysisHistoryService(
        IRepository<BabyAnalysis> analysisRepository,
        IRepository<AnalysisTag> tagRepository,
        IRepository<AnalysisAlert> alertRepository,
        IMapper mapper,
        ILogger<AnalysisHistoryService> logger)
    {
        _analysisRepository = analysisRepository;
        _tagRepository = tagRepository;
        _alertRepository = alertRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<int> SaveAnalysisAsync(AnalysisHistoryDto analysis, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new BabyAnalysis
            {
                BabyId = analysis.BabyId,
                AnalysisType = analysis.AnalysisType,
                RequestId = analysis.RequestId,
                CreatedAt = analysis.CreatedAt,
                Success = analysis.Success,
                ErrorMessage = analysis.ErrorMessage,
                ResultData = analysis.ResultData,
                Confidence = analysis.Confidence,
                PrimaryResult = analysis.PrimaryResult,
                Metadata = JsonSerializer.Serialize(analysis.Metadata)
            };

            await _analysisRepository.AddAsync(entity);
            var saved = entity;
            
            _logger.LogInformation("Saved analysis result: ID={Id}, BabyId={BabyId}, Type={Type}", 
                saved.Id, saved.BabyId, saved.AnalysisType);

            // Check if we need to create any alerts
            await CheckAndCreateAlertsAsync(saved, cancellationToken);

            return saved.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving analysis for baby {BabyId}", analysis.BabyId);
            throw;
        }
    }

    public async Task<AnalysisHistoryDto?> GetAnalysisAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _analysisRepository.GetByIdAsync(id);
            if (entity == null) return null;

            return MapToDto(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analysis {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<AnalysisHistoryDto>> GetAnalysisHistoryAsync(
        int babyId, 
        AnalysisHistoryFilter filter, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _analysisRepository.GetAllAsync();
            
            var query = entities.Where(a => a.BabyId == babyId);

            if (!string.IsNullOrEmpty(filter.AnalysisType))
            {
                query = query.Where(a => a.AnalysisType == filter.AnalysisType);
            }

            if (filter.From.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= filter.From.Value);
            }

            if (filter.To.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= filter.To.Value);
            }

            if (filter.SuccessOnly.HasValue)
            {
                query = query.Where(a => a.Success == filter.SuccessOnly.Value);
            }

            if (filter.MinConfidence.HasValue)
            {
                query = query.Where(a => a.Confidence >= filter.MinConfidence.Value);
            }

            query = query.OrderByDescending(a => a.CreatedAt);

            if (filter.Offset.HasValue)
            {
                query = query.Skip(filter.Offset.Value);
            }

            if (filter.Limit.HasValue)
            {
                query = query.Take(filter.Limit.Value);
            }

            return query.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analysis history for baby {BabyId}", babyId);
            throw;
        }
    }

    public async Task<bool> DeleteAnalysisAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _analysisRepository.GetByIdAsync(id);
            if (entity == null) return false;

            _analysisRepository.Remove(entity);
            
            _logger.LogInformation("Deleted analysis {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting analysis {Id}", id);
            throw;
        }
    }

    public async Task<AnalysisStatisticsDto> GetAnalysisStatisticsAsync(
        int babyId, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _analysisRepository.GetAllAsync();
            var analyses = entities.Where(a => a.BabyId == babyId && 
                                             a.CreatedAt >= from && 
                                             a.CreatedAt <= to).ToList();

            if (!analyses.Any())
            {
                return new AnalysisStatisticsDto
                {
                    TotalAnalyses = 0,
                    SuccessfulAnalyses = 0,
                    SuccessRate = 0,
                    FirstAnalysis = DateTime.MinValue,
                    LastAnalysis = DateTime.MinValue
                };
            }

            var successful = analyses.Where(a => a.Success).ToList();
            var analysisTypeDistribution = analyses.GroupBy(a => a.AnalysisType)
                .ToDictionary(g => g.Key, g => g.Count());
            var resultDistribution = successful.Where(a => !string.IsNullOrEmpty(a.PrimaryResult))
                .GroupBy(a => a.PrimaryResult!)
                .ToDictionary(g => g.Key, g => g.Count());

            return new AnalysisStatisticsDto
            {
                TotalAnalyses = analyses.Count,
                SuccessfulAnalyses = successful.Count,
                SuccessRate = (float)successful.Count / analyses.Count * 100,
                AnalysisTypeDistribution = analysisTypeDistribution,
                ResultDistribution = resultDistribution,
                AverageConfidence = successful.Where(a => a.Confidence.HasValue)
                    .Select(a => a.Confidence!.Value)
                    .DefaultIfEmpty(0)
                    .Average(),
                FirstAnalysis = analyses.Min(a => a.CreatedAt),
                LastAnalysis = analyses.Max(a => a.CreatedAt),
                TrendData = CalculateTrendData(analyses, from, to)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating analysis statistics for baby {BabyId}", babyId);
            throw;
        }
    }

    private async Task CheckAndCreateAlertsAsync(BabyAnalysis analysis, CancellationToken cancellationToken)
    {
        try
        {
            var alerts = new List<AnalysisAlert>();

            // Parse the result data to check for alert conditions
            if (analysis.Success && !string.IsNullOrEmpty(analysis.ResultData))
            {
                var alertLevel = DetermineAlertLevel(analysis);
                
                if (alertLevel > Core.Entities.AlertLevel.Info)
                {
                    var alert = new AnalysisAlert
                    {
                        BabyId = analysis.BabyId,
                        AnalysisId = analysis.Id,
                        AlertType = DetermineAlertType(analysis),
                        Severity = alertLevel,
                        Message = GenerateAlertMessage(analysis),
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };

                    alerts.Add(alert);
                }
            }

            // Save alerts
            foreach (var alert in alerts)
            {
                await _alertRepository.AddAsync(alert);
                _logger.LogInformation("Created alert: Type={Type}, Severity={Severity}, BabyId={BabyId}", 
                    alert.AlertType, alert.Severity, alert.BabyId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating alerts for analysis {AnalysisId}", analysis.Id);
            // Don't throw - alert creation shouldn't fail the main operation
        }
    }

    private Core.Entities.AlertLevel DetermineAlertLevel(BabyAnalysis analysis)
    {
        try
        {
            var resultData = JsonSerializer.Deserialize<JsonElement>(analysis.ResultData);

            return analysis.AnalysisType switch
            {
                "audio" => DetermineAudioAlertLevel(resultData),
                "image" => DetermineImageAlertLevel(resultData),
                "video" => DetermineVideoAlertLevel(resultData),
                "multimodal" => DetermineMultimodalAlertLevel(resultData),
                _ => Core.Entities.AlertLevel.Info
            };
        }
        catch
        {
            return Core.Entities.AlertLevel.Info;
        }
    }

    private Core.Entities.AlertLevel DetermineAudioAlertLevel(JsonElement resultData)
    {
        if (resultData.TryGetProperty("CryAnalysis", out var cryAnalysis) &&
            cryAnalysis.TryGetProperty("CryDetected", out var cryDetected) &&
            cryDetected.GetBoolean())
        {
            if (cryAnalysis.TryGetProperty("Confidence", out var confidence) &&
                confidence.GetSingle() > 0.8f)
            {
                return Core.Entities.AlertLevel.High;
            }
            return Core.Entities.AlertLevel.Warning;
        }
        return Core.Entities.AlertLevel.Info;
    }

    private Core.Entities.AlertLevel DetermineImageAlertLevel(JsonElement resultData)
    {
        if (resultData.TryGetProperty("EmotionAnalysis", out var emotionAnalysis) &&
            emotionAnalysis.TryGetProperty("DetectedMood", out var mood))
        {
            var moodStr = mood.GetString();
            if (moodStr == "distressed" || moodStr == "uncomfortable")
            {
                return Core.Entities.AlertLevel.Warning;
            }
        }
        return Core.Entities.AlertLevel.Info;
    }

    private Core.Entities.AlertLevel DetermineVideoAlertLevel(JsonElement resultData)
    {
        // Combine audio and visual analysis for video alerts
        var audioLevel = DetermineAudioAlertLevel(resultData);
        var imageLevel = DetermineImageAlertLevel(resultData);
        
        return (Core.Entities.AlertLevel)Math.Max((int)audioLevel, (int)imageLevel);
    }

    private Core.Entities.AlertLevel DetermineMultimodalAlertLevel(JsonElement resultData)
    {
        if (resultData.TryGetProperty("FusionAnalysis", out var fusionAnalysis) &&
            fusionAnalysis.TryGetProperty("AlertLevel", out var alertLevel))
        {
            return alertLevel.GetInt32() switch
            {
                3 => Core.Entities.AlertLevel.High,
                4 => Core.Entities.AlertLevel.Critical,
                2 => Core.Entities.AlertLevel.Warning,
                _ => Core.Entities.AlertLevel.Info
            };
        }
        return Core.Entities.AlertLevel.Info;
    }

    private string DetermineAlertType(BabyAnalysis analysis)
    {
        return analysis.AnalysisType switch
        {
            "audio" => "crying_detected",
            "image" => "mood_change",
            "video" => "behavioral_pattern",
            "multimodal" => "fusion_alert",
            _ => "general_alert"
        };
    }

    private string GenerateAlertMessage(BabyAnalysis analysis)
    {
        try
        {
            var resultData = JsonSerializer.Deserialize<JsonElement>(analysis.ResultData);
            
            return analysis.AnalysisType switch
            {
                "audio" when resultData.TryGetProperty("CryAnalysis", out var cryAnalysis) => 
                    GenerateAudioAlertMessage(cryAnalysis),
                "image" when resultData.TryGetProperty("EmotionAnalysis", out var emotionAnalysis) => 
                    GenerateImageAlertMessage(emotionAnalysis),
                "multimodal" when resultData.TryGetProperty("FusionAnalysis", out var fusionAnalysis) => 
                    GenerateMultimodalAlertMessage(fusionAnalysis),
                _ => $"Analysis completed: {analysis.PrimaryResult}"
            };
        }
        catch
        {
            return $"Alert for {analysis.AnalysisType} analysis";
        }
    }

    private string GenerateAudioAlertMessage(JsonElement cryAnalysis)
    {
        if (cryAnalysis.TryGetProperty("CryReason", out var reason))
        {
            return $"Baby crying detected - Reason: {reason.GetString()}";
        }
        return "Baby crying detected";
    }

    private string GenerateImageAlertMessage(JsonElement emotionAnalysis)
    {
        if (emotionAnalysis.TryGetProperty("DetectedMood", out var mood))
        {
            return $"Baby mood detected: {mood.GetString()}";
        }
        return "Baby mood analysis completed";
    }

    private string GenerateMultimodalAlertMessage(JsonElement fusionAnalysis)
    {
        if (fusionAnalysis.TryGetProperty("OverallState", out var state))
        {
            return $"Baby state: {state.GetString()}";
        }
        return "Combined analysis completed";
    }

    private Dictionary<string, object> CalculateTrendData(List<BabyAnalysis> analyses, DateTime from, DateTime to)
    {
        var trendData = new Dictionary<string, object>();
        
        try
        {
            // Group by day
            var dailyStats = analyses.GroupBy(a => a.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => new
                {
                    Count = g.Count(),
                    SuccessRate = g.Count(a => a.Success) / (float)g.Count() * 100,
                    AverageConfidence = g.Where(a => a.Confidence.HasValue)
                        .Select(a => a.Confidence!.Value)
                        .DefaultIfEmpty(0)
                        .Average()
                });

            trendData["daily_stats"] = dailyStats;

            // Mood trends for image/video analyses
            var moodTrends = analyses.Where(a => a.AnalysisType == "image" || a.AnalysisType == "video")
                .Where(a => !string.IsNullOrEmpty(a.PrimaryResult))
                .GroupBy(a => a.PrimaryResult!)
                .ToDictionary(g => g.Key, g => g.Count());

            trendData["mood_trends"] = moodTrends;

            // Cry reason trends for audio analyses
            var cryTrends = analyses.Where(a => a.AnalysisType == "audio")
                .Where(a => !string.IsNullOrEmpty(a.PrimaryResult))
                .GroupBy(a => a.PrimaryResult!)
                .ToDictionary(g => g.Key, g => g.Count());

            trendData["cry_trends"] = cryTrends;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calculating trend data");
        }

        return trendData;
    }

    private AnalysisHistoryDto MapToDto(BabyAnalysis entity)
    {
        var metadata = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(entity.Metadata))
        {
            try
            {
                metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Metadata) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deserializing metadata for analysis {Id}", entity.Id);
            }
        }

        return new AnalysisHistoryDto
        {
            Id = entity.Id,
            BabyId = entity.BabyId,
            AnalysisType = entity.AnalysisType,
            RequestId = entity.RequestId,
            CreatedAt = entity.CreatedAt,
            Success = entity.Success,
            ErrorMessage = entity.ErrorMessage,
            ResultData = entity.ResultData,
            Confidence = entity.Confidence,
            PrimaryResult = entity.PrimaryResult,
            Metadata = metadata
        };
    }
}
