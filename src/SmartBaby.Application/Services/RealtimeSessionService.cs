using Microsoft.Extensions.Logging;
using SmartBaby.Core.DTOs;
using SmartBaby.Core.Entities;
using SmartBaby.Core.Interfaces;
using System.Text.Json;
using RealtimeSessionStatus = SmartBaby.Core.Entities.RealtimeSessionStatus;

namespace SmartBaby.Application.Services;

/// <summary>
/// Service for managing real-time analysis sessions
/// </summary>
public class RealtimeSessionService : IRealtimeSessionService
{
    private readonly IRepository<RealtimeAnalysisSession> _sessionRepository;
    private readonly IRepository<RealtimeAnalysisUpdate> _updateRepository;
    private readonly ILogger<RealtimeSessionService> _logger;

    public RealtimeSessionService(
        IRepository<RealtimeAnalysisSession> sessionRepository,
        IRepository<RealtimeAnalysisUpdate> updateRepository,
        ILogger<RealtimeSessionService> logger)
    {
        _sessionRepository = sessionRepository;
        _updateRepository = updateRepository;
        _logger = logger;
    }

    public async Task<string> CreateSessionAsync(int babyId, RealtimeSettingsDto settings, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionId = Guid.NewGuid().ToString();
            
            var session = new RealtimeAnalysisSession
            {
                SessionId = sessionId,
                BabyId = babyId,
                Status = RealtimeSessionStatus.Created,
                Settings = JsonSerializer.Serialize(settings),
                CreatedAt = DateTime.UtcNow,
                UpdateCount = 0,
                Statistics = JsonSerializer.Serialize(new Dictionary<string, object>())
            };

            await _sessionRepository.AddAsync(session);
            
            _logger.LogInformation("Created real-time analysis session {SessionId} for baby {BabyId}", 
                sessionId, babyId);

            return sessionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating real-time session for baby {BabyId}", babyId);
            throw;
        }
    }

    public async Task<bool> StopSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await _sessionRepository.GetAllAsync();
            var session = sessions.FirstOrDefault(s => s.SessionId == sessionId);
            
            if (session == null)
            {
                _logger.LogWarning("Session {SessionId} not found for stopping", sessionId);
                return false;
            }

            session.Status = RealtimeSessionStatus.Stopped;
            session.StoppedAt = DateTime.UtcNow;

            _sessionRepository.Update(session);
            
            _logger.LogInformation("Stopped real-time analysis session {SessionId}", sessionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping real-time session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<RealtimeSessionDto?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await _sessionRepository.GetAllAsync();
            var session = sessions.FirstOrDefault(s => s.SessionId == sessionId);
            
            if (session == null) return null;

            return MapToDto(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting real-time session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<IEnumerable<RealtimeSessionDto>> GetActiveSessionsAsync(int? babyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await _sessionRepository.GetAllAsync();
            
            var query = sessions.Where(s => s.Status == RealtimeSessionStatus.Active || 
                                          s.Status == RealtimeSessionStatus.Starting);

            if (babyId.HasValue)
            {
                query = query.Where(s => s.BabyId == babyId.Value);
            }

            return query.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions for baby {BabyId}", babyId);
            throw;
        }
    }

    public async Task<bool> UpdateSessionStatusAsync(string sessionId, RealtimeSessionStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await _sessionRepository.GetAllAsync();
            var session = sessions.FirstOrDefault(s => s.SessionId == sessionId);
            
            if (session == null)
            {
                _logger.LogWarning("Session {SessionId} not found for status update", sessionId);
                return false;
            }

            var oldStatus = session.Status;
            session.Status = status;

            if (status == RealtimeSessionStatus.Active && session.StartedAt == null)
            {
                session.StartedAt = DateTime.UtcNow;
            }
            else if (status == RealtimeSessionStatus.Stopped && session.StoppedAt == null)
            {
                session.StoppedAt = DateTime.UtcNow;
            }

            _sessionRepository.Update(session);
            
            _logger.LogInformation("Updated session {SessionId} status from {OldStatus} to {NewStatus}", 
                sessionId, oldStatus, status);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session {SessionId} status to {Status}", sessionId, status);
            throw;
        }
    }

    public async Task<bool> AddSessionUpdateAsync(string sessionId, RealtimeAnalysisResponseDto update, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await _sessionRepository.GetAllAsync();
            var session = sessions.FirstOrDefault(s => s.SessionId == sessionId);
            
            if (session == null)
            {
                _logger.LogWarning("Session {SessionId} not found for adding update", sessionId);
                return false;
            }

            var updateEntity = new RealtimeAnalysisUpdate
            {
                SessionId = session.Id,
                Timestamp = update.Timestamp,
                UpdateType = update.Update.UpdateType.ToString(),
                UpdateData = JsonSerializer.Serialize(update.Update),
                Confidence = GetUpdateConfidence(update.Update),
                PrimaryResult = GetUpdatePrimaryResult(update.Update)
            };

            await _updateRepository.AddAsync(updateEntity);

            // Update session statistics
            session.UpdateCount++;
            session.LastUpdateAt = DateTime.UtcNow;
            _sessionRepository.Update(session);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding update to session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<IEnumerable<RealtimeAnalysisUpdate>> GetSessionUpdatesAsync(string sessionId, int? limit = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await _sessionRepository.GetAllAsync();
            var session = sessions.FirstOrDefault(s => s.SessionId == sessionId);
            
            if (session == null) return new List<RealtimeAnalysisUpdate>();

            var updates = await _updateRepository.GetAllAsync();
            var query = updates.Where(u => u.SessionId == session.Id)
                              .OrderByDescending(u => u.Timestamp).AsEnumerable();

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return query.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting updates for session {SessionId}", sessionId);
            throw;
        }
    }

    private RealtimeSessionDto MapToDto(RealtimeAnalysisSession session)
    {
        var settings = new RealtimeSettingsDto();
        if (!string.IsNullOrEmpty(session.Settings))
        {
            try
            {
                settings = JsonSerializer.Deserialize<RealtimeSettingsDto>(session.Settings) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deserializing session settings for {SessionId}", session.SessionId);
            }
        }

        var statistics = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(session.Statistics))
        {
            try
            {
                statistics = JsonSerializer.Deserialize<Dictionary<string, object>>(session.Statistics) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deserializing session statistics for {SessionId}", session.SessionId);
            }
        }

        return new RealtimeSessionDto
        {
            SessionId = session.SessionId,
            BabyId = session.BabyId,
            Status = session.Status,
            Settings = settings,
            CreatedAt = session.CreatedAt,
            StartedAt = session.StartedAt,
            StoppedAt = session.StoppedAt,
            UpdateCount = session.UpdateCount,
            LastUpdateAt = session.LastUpdateAt,
            Statistics = statistics
        };
    }

    private float? GetUpdateConfidence(AnalysisUpdateDto update)
    {
        return update.UpdateType switch
        {
            UpdateType.EmotionUpdate => update.EmotionData?.Confidence,
            UpdateType.CryUpdate => update.CryData?.Confidence,
            UpdateType.FusionUpdate => update.FusionData?.Confidence,
            _ => null
        };
    }

    private string? GetUpdatePrimaryResult(AnalysisUpdateDto update)
    {
        return update.UpdateType switch
        {
            UpdateType.EmotionUpdate => update.EmotionData?.DetectedMood,
            UpdateType.CryUpdate => update.CryData?.CryReason,
            UpdateType.FusionUpdate => update.FusionData?.OverallState,
            _ => null
        };
    }
}
