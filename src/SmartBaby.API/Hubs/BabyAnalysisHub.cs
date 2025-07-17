using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartBaby.Core.DTOs;
using SmartBaby.Core.Entities;
using SmartBaby.Core.Interfaces;
using System.Security.Claims;

namespace SmartBaby.API.Hubs;

/// <summary>
/// SignalR Hub for real-time baby analysis updates
/// </summary>
[Authorize]
public class BabyAnalysisHub : Hub
{
    private readonly IBabyAnalysisService _analysisService;
    private readonly IRealtimeSessionService _sessionService;
    private readonly IBabyService _babyService;
    private readonly ILogger<BabyAnalysisHub> _logger;

    public BabyAnalysisHub(
        IBabyAnalysisService analysisService,
        IRealtimeSessionService sessionService,
        IBabyService babyService,
        ILogger<BabyAnalysisHub> logger)
    {
        _analysisService = analysisService;
        _sessionService = sessionService;
        _babyService = babyService;
        _logger = logger;
    }

    /// <summary>
    /// Start real-time analysis session for a baby
    /// </summary>
    public async Task StartRealtimeAnalysis(RealtimeAnalysisRequestDto request)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Validate baby ownership if BabyId is provided
            if (request.BabyId.HasValue)
            {
                var baby = await _babyService.GetBabyByIdAsync(request.BabyId.Value);
                if (baby == null || baby.UserId != userId)
                {
                    await Clients.Caller.SendAsync("Error", "You don't have access to this baby's data");
                    return;
                }
            }

            // Create session in database
            var sessionId = await _sessionService.CreateSessionAsync(
                request.BabyId ?? 0, 
                request.Settings);

            request.SessionId = sessionId;

            // Join SignalR group for this session
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");

            _logger.LogInformation("Starting real-time analysis session {SessionId} for user {UserId}", 
                sessionId, userId);

            // Send session started confirmation
            await Clients.Caller.SendAsync("SessionStarted", new { SessionId = sessionId });

            // Start the real-time analysis and stream updates
            await foreach (var update in _analysisService.StartRealtimeAnalysisAsync(request))
            {
                // Send updates to all clients in this session group
                await Clients.Group($"session_{sessionId}").SendAsync("AnalysisUpdate", update);
                
                // Update session status
                await _sessionService.UpdateSessionStatusAsync(sessionId, RealtimeSessionStatus.Active);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting real-time analysis session");
            await Clients.Caller.SendAsync("Error", "Failed to start real-time analysis session");
        }
    }

    /// <summary>
    /// Stop real-time analysis session
    /// </summary>
    public async Task StopRealtimeAnalysis(string sessionId)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Validate session ownership
            var session = await _sessionService.GetSessionAsync(sessionId);
            if (session != null && session.BabyId > 0)
            {
                var baby = await _babyService.GetBabyByIdAsync(session.BabyId);
                if (baby == null || baby.UserId != userId)
                {
                    await Clients.Caller.SendAsync("Error", "You don't have access to this session");
                    return;
                }
            }

            _logger.LogInformation("Stopping real-time analysis session {SessionId} for user {UserId}", 
                sessionId, userId);

            // Stop the analysis service
            await _analysisService.StopRealtimeAnalysisAsync(sessionId);

            // Update session status
            await _sessionService.UpdateSessionStatusAsync(sessionId, RealtimeSessionStatus.Stopped);

            // Remove from SignalR group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");

            // Notify clients that session has stopped
            await Clients.Group($"session_{sessionId}").SendAsync("SessionStopped", new { SessionId = sessionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping real-time analysis session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", "Failed to stop real-time analysis session");
        }
    }

    /// <summary>
    /// Join an existing real-time analysis session
    /// </summary>
    public async Task JoinSession(string sessionId)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Validate session ownership
            var session = await _sessionService.GetSessionAsync(sessionId);
            if (session == null)
            {
                await Clients.Caller.SendAsync("Error", "Session not found");
                return;
            }

            if (session.BabyId > 0)
            {
                var baby = await _babyService.GetBabyByIdAsync(session.BabyId);
                if (baby == null || baby.UserId != userId)
                {
                    await Clients.Caller.SendAsync("Error", "You don't have access to this session");
                    return;
                }
            }

            // Join SignalR group for this session
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");

            _logger.LogInformation("User {UserId} joined real-time analysis session {SessionId}", 
                userId, sessionId);

            await Clients.Caller.SendAsync("SessionJoined", new { SessionId = sessionId, Session = session });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining real-time analysis session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", "Failed to join real-time analysis session");
        }
    }

    /// <summary>
    /// Leave a real-time analysis session
    /// </summary>
    public async Task LeaveSession(string sessionId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");
            await Clients.Caller.SendAsync("SessionLeft", new { SessionId = sessionId });
            
            _logger.LogInformation("Connection {ConnectionId} left session {SessionId}", 
                Context.ConnectionId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving session {SessionId}", sessionId);
        }
    }

    /// <summary>
    /// Get active sessions for the user
    /// </summary>
    public async Task GetActiveSessions(int? babyId = null)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // If babyId is provided, validate ownership
            if (babyId.HasValue)
            {
                var baby = await _babyService.GetBabyByIdAsync(babyId.Value);
                if (baby == null || baby.UserId != userId)
                {
                    await Clients.Caller.SendAsync("Error", "You don't have access to this baby's data");
                    return;
                }
            }

            var sessions = await _sessionService.GetActiveSessionsAsync(babyId);
            
            // Filter sessions to only those owned by the user
            var userSessions = new List<RealtimeSessionDto>();
            foreach (var session in sessions)
            {
                if (session.BabyId > 0)
                {
                    var baby = await _babyService.GetBabyByIdAsync(session.BabyId);
                    if (baby?.UserId == userId)
                    {
                        userSessions.Add(session);
                    }
                }
                else
                {
                    userSessions.Add(session);
                }
            }

            await Clients.Caller.SendAsync("ActiveSessions", userSessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions");
            await Clients.Caller.SendAsync("Error", "Failed to retrieve active sessions");
        }
    }

    /// <summary>
    /// Send a custom message to a session group
    /// </summary>
    public async Task SendMessageToSession(string sessionId, object message)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            // Validate session ownership
            var session = await _sessionService.GetSessionAsync(sessionId);
            if (session != null && session.BabyId > 0)
            {
                var baby = await _babyService.GetBabyByIdAsync(session.BabyId);
                if (baby == null || baby.UserId != userId)
                {
                    return;
                }
            }

            await Clients.Group($"session_{sessionId}").SendAsync("SessionMessage", new 
            { 
                SessionId = sessionId, 
                Message = message, 
                UserId = userId,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to session {SessionId}", sessionId);
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} connected to BabyAnalysisHub with connection {ConnectionId}", 
            userId, Context.ConnectionId);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} disconnected from BabyAnalysisHub with connection {ConnectionId}", 
            userId, Context.ConnectionId);
        
        if (exception != null)
        {
            _logger.LogError(exception, "User {UserId} disconnected with error", userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
