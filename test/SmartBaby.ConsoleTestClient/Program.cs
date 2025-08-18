using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using SmartBaby.Core.DTOs;
using System.Text;
using System.Text.Json;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SmartBaby.ConsoleTestClient;

/// <summary>
/// Console application to test SignalR real-time analysis with webcam
/// </summary>
public class Program
{
    private static HubConnection? _connection;
    private static VideoCapture? _capture;
    private static bool _isAnalyzing = false;
    private static string? _currentSessionId;
    private static string? _jwtToken;
    private static string? _currentUserEmail;
    private static readonly ILogger _logger = CreateLogger();
    private static HttpClient _httpClient = new HttpClient();
    private const string API_BASE_URL = "https://localhost:55362/api/";

    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== SmartBaby SignalR Real-Time Analysis Test Client ===");
        Console.WriteLine();

        try
        {
            // Configure HttpClient for development with SSL bypass
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            
            _httpClient?.Dispose();
            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = new Uri(API_BASE_URL);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            Console.WriteLine($"HttpClient configured for: {_httpClient.BaseAddress}");

            // Show login menu
            await ShowAuthenticationMenu();

            // Only proceed if authenticated
            if (string.IsNullOrEmpty(_jwtToken))
            {
                Console.WriteLine("Authentication required to continue. Exiting...");
                return;
            }

            // Initialize SignalR connection
            await InitializeSignalRConnection();

            // Initialize camera
            InitializeCamera();

            // Start interactive menu
            await RunInteractiveMenu();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error: {ex.Message}");
            _logger.LogError(ex, "Critical error in main application");
        }
        finally
        {
            await Cleanup();
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static async Task ShowAuthenticationMenu()
    {
        while (string.IsNullOrEmpty(_jwtToken))
        {
            Console.WriteLine();
            Console.WriteLine("=== Authentication Required ===");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine("3. Use Mock Token (for testing)");
            Console.WriteLine("0. Exit");
            Console.Write("Select option: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await LoginUser();
                    break;
                case "2":
                    await RegisterUser();
                    break;
                case "3":
                    _jwtToken = GenerateMockJwtToken();
                    _currentUserEmail = "test@example.com";
                    Console.WriteLine("✓ Using mock token for testing");
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }

        Console.WriteLine($"✓ Authenticated as: {_currentUserEmail}");
    }

    private static async Task LoginUser()
    {
        Console.WriteLine();
        Console.WriteLine("=== User Login ===");
        
        Console.Write("Email: ");
        var email = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("❌ Email is required");
            return;
        }

        Console.Write("Password: ");
        var password = ReadPassword();
        
        if (string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("❌ Password is required");
            return;
        }

        var loginDto = new LoginDto
        {
            Email = email.Trim(),
            Password = password
        };

        try
        {
            var json = JsonSerializer.Serialize(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            Console.WriteLine($"Sending login request to: {_httpClient.BaseAddress}auth/login");
            var response = await _httpClient.PostAsync("auth/login", content);
            
            Console.WriteLine($"Response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {responseContent}");
                
                var tokenDto = JsonSerializer.Deserialize<TokenDto>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
                
                if (tokenDto != null && !string.IsNullOrEmpty(tokenDto.Token))
                {
                    _jwtToken = tokenDto.Token;
                    _currentUserEmail = email.Trim();
                    Console.WriteLine("✓ Login successful!");
                    Console.WriteLine($"Token expires: {tokenDto.Expiration:yyyy-MM-dd HH:mm:ss}");
                }
                else
                {
                    Console.WriteLine("❌ Invalid response from server");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Login failed: {response.StatusCode}");
                Console.WriteLine($"Error: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Login error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Console.WriteLine("Make sure the SmartBaby API is running on https://localhost:55362");
        }
    }

    private static async Task RegisterUser()
    {
        Console.WriteLine();
        Console.WriteLine("=== User Registration ===");
        
        Console.Write("Full Name: ");
        var fullName = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(fullName))
        {
            Console.WriteLine("❌ Full name is required");
            return;
        }

        Console.Write("Email: ");
        var email = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("❌ Email is required");
            return;
        }

        Console.Write("Password (minimum 6 characters): ");
        var password = ReadPassword();
        
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            Console.WriteLine("❌ Password must be at least 6 characters");
            return;
        }

        Console.Write("Confirm Password: ");
        var confirmPassword = ReadPassword();
        
        if (password != confirmPassword)
        {
            Console.WriteLine("❌ Passwords do not match");
            return;
        }

        var userDto = new UserDto
        {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            Password = password
        };

        try
        {
            var json = JsonSerializer.Serialize(userDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            Console.WriteLine($"Sending registration request to: {_httpClient.BaseAddress}auth/register");
            var response = await _httpClient.PostAsync("auth/register", content);
            
            Console.WriteLine($"Response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {responseContent}");
                Console.WriteLine("✓ Registration successful!");
                Console.WriteLine("You can now login with your credentials.");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Registration failed: {response.StatusCode}");
                Console.WriteLine($"Error: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Registration error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Console.WriteLine("Make sure the SmartBaby API is running on https://localhost:55362");
        }
    }

    private static string ReadPassword()
    {
        var password = new StringBuilder();
        ConsoleKeyInfo keyInfo;

        do
        {
            keyInfo = Console.ReadKey(true);
            
            if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                Console.Write("\b \b");
            }
            else if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Backspace)
            {
                password.Append(keyInfo.KeyChar);
                Console.Write("*");
            }
        }
        while (keyInfo.Key != ConsoleKey.Enter);
        
        Console.WriteLine();
        return password.ToString();
    }

    private static async Task InitializeSignalRConnection()
    {
        Console.WriteLine("Initializing SignalR connection...");

        if (string.IsNullOrEmpty(_jwtToken))
        {
            throw new InvalidOperationException("JWT token is required for SignalR connection");
        }

        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:55362/hubs/babyanalysis", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(_jwtToken);
                // Skip SSL validation for development
                options.HttpMessageHandlerFactory = _ => new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                };
            })
            .WithAutomaticReconnect()
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(MsLogLevel.Information);
                logging.AddConsole();
            })
            .Build();

        // Set up event handlers
        SetupSignalREventHandlers();

        try
        {
            await _connection.StartAsync();
            Console.WriteLine("✓ SignalR connection established!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to connect to SignalR: {ex.Message}");
            Console.WriteLine("Make sure the SmartBaby API is running on https://localhost:55362");
            throw;
        }
    }

    private static void SetupSignalREventHandlers()
    {
        if (_connection == null) return;

        _connection.On("SessionStarted", (object data) =>
        {
            var json = JsonSerializer.Serialize(data);
            var sessionData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            
            if (sessionData?.TryGetValue("SessionId", out var sessionIdObj) == true)
            {
                _currentSessionId = sessionIdObj.ToString();
                Console.WriteLine($"🎯 Session started: {_currentSessionId}");
            }
        });

        _connection.On("AnalysisUpdate", (object update) =>
        {
            try
            {
                var json = JsonSerializer.Serialize(update);
                var analysisUpdate = JsonSerializer.Deserialize<RealtimeAnalysisResponseDto>(json);
                
                if (analysisUpdate != null)
                {
                    DisplayAnalysisUpdate(analysisUpdate);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing analysis update: {ex.Message}");
            }
        });

        _connection.On("SessionStopped", (object data) =>
        {
            Console.WriteLine("🛑 Session stopped");
            _currentSessionId = null;
            _isAnalyzing = false;
        });

        _connection.On("Error", (string error) =>
        {
            Console.WriteLine($"❌ SignalR Error: {error}");
        });

        _connection.On("ActiveSessions", (object sessions) =>
        {
            var json = JsonSerializer.Serialize(sessions);
            Console.WriteLine($"📋 Active sessions: {json}");
        });

        _connection.Closed += (error) =>
        {
            Console.WriteLine("🔌 SignalR connection closed");
            if (error != null)
            {
                Console.WriteLine($"Connection closed with error: {error.Message}");
            }
            return Task.CompletedTask;
        };

        _connection.Reconnected += (connectionId) =>
        {
            Console.WriteLine($"🔄 SignalR reconnected with ID: {connectionId}");
            return Task.CompletedTask;
        };

        _connection.Reconnecting += (error) =>
        {
            Console.WriteLine("🔄 SignalR reconnecting...");
            return Task.CompletedTask;
        };
    }

    private static void InitializeCamera()
    {
        Console.WriteLine("Initializing camera...");

        try
        {
            _capture = new VideoCapture(0);
            
            if (!_capture.IsOpened())
            {
                throw new InvalidOperationException("Cannot open camera");
            }

            // Set camera properties for better performance
            _capture.Set(VideoCaptureProperties.FrameWidth, 640);
            _capture.Set(VideoCaptureProperties.FrameHeight, 480);
            _capture.Set(VideoCaptureProperties.Fps, 15);

            Console.WriteLine("✓ Camera initialized successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to initialize camera: {ex.Message}");
            Console.WriteLine("Make sure you have a webcam connected and accessible.");
            throw;
        }
    }

    private static async Task RunInteractiveMenu()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Menu ===");
            Console.WriteLine($"Logged in as: {_currentUserEmail}");
            Console.WriteLine("1. Start Real-Time Analysis");
            Console.WriteLine("2. Stop Analysis");
            Console.WriteLine("3. Get Active Sessions");
            Console.WriteLine("4. Show Camera Preview");
            Console.WriteLine("5. Test Single Frame Analysis");
            Console.WriteLine("6. Connection Status");
            Console.WriteLine("7. Logout");
            Console.WriteLine("0. Exit");
            Console.Write("Select option: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await StartRealtimeAnalysis();
                        break;
                    case "2":
                        await StopAnalysis();
                        break;
                    case "3":
                        await GetActiveSessions();
                        break;
                    case "4":
                        ShowCameraPreview();
                        break;
                    case "5":
                        await TestSingleFrameAnalysis();
                        break;
                    case "6":
                        ShowConnectionStatus();
                        break;
                    case "7":
                        await Logout();
                        return;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command: {ex.Message}");
                _logger.LogError(ex, "Error in menu command");
            }
        }
    }

    private static async Task StartRealtimeAnalysis()
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            Console.WriteLine("❌ SignalR not connected");
            return;
        }

        if (_isAnalyzing)
        {
            Console.WriteLine("⚠️ Analysis already running");
            return;
        }

        Console.WriteLine("🚀 Starting real-time analysis...");

        var request = new RealtimeAnalysisRequestDto
        {
            BabyId = 1, // Mock baby ID for testing
            Settings = new RealtimeSettingsDto
            {
                VideoDeviceId = 0,
                AudioFormat = new AudioFormatDto
                {
                    SampleRate = 44100,
                    Channels = 1,
                    Encoding = "LINEAR_PCM",
                    BitDepth = 16
                },
                FrameAnalysisInterval = 2.0f,
                AudioAnalysisDuration = 2.0f,
                EnableVideoDisplay = true,
                EnableOverlay = true
            }
        };

        try
        {
            await _connection.InvokeAsync("StartRealtimeAnalysis", request);
            _isAnalyzing = true;

            // Start camera capture loop in background
            _ = Task.Run(CameraCaptureLoop);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to start analysis: {ex.Message}");
        }
    }

    private static async Task StopAnalysis()
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            Console.WriteLine("❌ SignalR not connected");
            return;
        }

        if (string.IsNullOrEmpty(_currentSessionId))
        {
            Console.WriteLine("⚠️ No active session to stop");
            return;
        }

        Console.WriteLine("🛑 Stopping analysis...");

        try
        {
            await _connection.InvokeAsync("StopRealtimeAnalysis", _currentSessionId);
            _isAnalyzing = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to stop analysis: {ex.Message}");
        }
    }

    private static async Task GetActiveSessions()
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            Console.WriteLine("❌ SignalR not connected");
            return;
        }

        Console.WriteLine("📋 Getting active sessions...");

        try
        {
            await _connection.InvokeAsync("GetActiveSessions", (int?)null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to get active sessions: {ex.Message}");
        }
    }

    private static void ShowCameraPreview()
    {
        if (_capture?.IsOpened() != true)
        {
            Console.WriteLine("❌ Camera not available");
            return;
        }

        Console.WriteLine("📹 Camera preview - Press 'q' to stop");

        using var window = new Window("Camera Preview");
        var frame = new Mat();

        while (true)
        {
            _capture.Read(frame);
            if (frame.Empty()) break;

            window.ShowImage(frame);
            
            if (Cv2.WaitKey(30) == 'q') break;
        }

        window.Close();
    }

    private static Task TestSingleFrameAnalysis()
    {
        if (_capture?.IsOpened() != true)
        {
            Console.WriteLine("❌ Camera not available");
            return Task.CompletedTask;
        }

        Console.WriteLine("📸 Capturing single frame for analysis...");

        using var frame = new Mat();
        _capture.Read(frame);

        if (frame.Empty())
        {
            Console.WriteLine("❌ Failed to capture frame");
            return Task.CompletedTask;
        }

        // Convert frame to base64 for API call
        var imageBytes = frame.ToBytes(".jpg");
        var base64Image = Convert.ToBase64String(imageBytes);

        Console.WriteLine($"✓ Captured frame ({imageBytes.Length} bytes)");
        Console.WriteLine("This would be sent to the analysis service in a real scenario.");
        
        // Here you could make a direct HTTP call to test the analysis API
        // without SignalR if needed
        return Task.CompletedTask;
    }

    private static void ShowConnectionStatus()
    {
        Console.WriteLine($"SignalR State: {_connection?.State ?? HubConnectionState.Disconnected}");
        Console.WriteLine($"Current User: {_currentUserEmail ?? "Not authenticated"}");
        Console.WriteLine($"Current Session: {_currentSessionId ?? "None"}");
        Console.WriteLine($"Analyzing: {_isAnalyzing}");
        Console.WriteLine($"Camera Available: {_capture?.IsOpened() ?? false}");
        Console.WriteLine($"JWT Token: {(_jwtToken != null ? "Present" : "Missing")}");
    }

    private static async Task Logout()
    {
        Console.WriteLine("Logging out...");
        
        // Stop any ongoing analysis
        if (_isAnalyzing)
        {
            await StopAnalysis();
        }

        // Close SignalR connection
        if (_connection != null)
        {
            try
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing SignalR connection: {ex.Message}");
            }
        }

        // Clear authentication data
        _jwtToken = null;
        _currentUserEmail = null;
        _currentSessionId = null;
        _isAnalyzing = false;

        Console.WriteLine("✓ Logged out successfully");
    }

    private static async Task CameraCaptureLoop()
    {
        if (_capture?.IsOpened() != true) return;

        Console.WriteLine("📹 Camera capture loop started");
        using var frame = new Mat();
        var frameCount = 0;

        while (_isAnalyzing && _capture.IsOpened())
        {
            try
            {
                _capture.Read(frame);
                if (frame.Empty()) continue;

                frameCount++;

                // Send frame every 2 seconds (adjust as needed)
                if (frameCount % 30 == 0)
                {
                    await ProcessFrame(frame);
                }

                await Task.Delay(33); // ~30 FPS
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in camera loop: {ex.Message}");
                await Task.Delay(1000);
            }
        }

        Console.WriteLine("📹 Camera capture loop stopped");
    }

    private static async Task ProcessFrame(Mat frame)
    {
        try
        {
            // Convert frame to base64
            var imageBytes = frame.ToBytes(".jpg");
            var base64Image = Convert.ToBase64String(imageBytes);

            // Here you would send the frame to your analysis service
            // For now, we'll just simulate the process
            Console.WriteLine($"📸 Processing frame ({imageBytes.Length} bytes) - Session: {_currentSessionId}");
            
            // In a real implementation, you would:
            // 1. Send the frame to your gRPC analysis service
            // 2. The service would return analysis results
            // 3. Those results would be automatically sent via SignalR to connected clients
            
            await Task.Delay(100); // Simulate processing time
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing frame: {ex.Message}");
        }
    }

    private static void DisplayAnalysisUpdate(RealtimeAnalysisResponseDto update)
    {
        Console.WriteLine();
        Console.WriteLine("🔍 === Analysis Update ===");
        Console.WriteLine($"Session ID: {update.SessionId}");
        Console.WriteLine($"Timestamp: {update.Timestamp:HH:mm:ss.fff}");
        Console.WriteLine($"Update Type: {update.Update.UpdateType}");

        switch (update.Update.UpdateType)
        {
            case UpdateType.EmotionUpdate when update.Update.EmotionData != null:
                var emotion = update.Update.EmotionData;
                Console.WriteLine($"😊 Emotion: {emotion.DetectedMood} ({emotion.Confidence:P1} confidence)");
                if (emotion.AllEmotions?.Any() == true)
                {
                    Console.WriteLine("   Emotion Scores:");
                    foreach (var score in emotion.AllEmotions.Take(3))
                    {
                        Console.WriteLine($"     {score.Key}: {score.Value:P1}");
                    }
                }
                break;

            case UpdateType.CryUpdate when update.Update.CryData != null:
                var cry = update.Update.CryData;
                Console.WriteLine($"👶 Crying: {(cry.CryDetected ? "Yes" : "No")}");
                if (cry.CryDetected)
                {
                    Console.WriteLine($"   Reason: {cry.CryReason}");
                    Console.WriteLine($"   Confidence: {cry.Confidence:P1}");
                }
                break;

            case UpdateType.FusionUpdate when update.Update.FusionData != null:
                var fusion = update.Update.FusionData;
                Console.WriteLine($"🔄 Fusion: {fusion.OverallState}");
                Console.WriteLine($"   Confidence: {fusion.Confidence:P1}");
                if (fusion.AlertLevel != AlertLevel.Normal)
                {
                    Console.WriteLine($"   Alert Level: {fusion.AlertLevel}");
                }
                break;
        }

        if (update.Update.FusionData?.Recommendations?.Any() == true)
        {
            Console.WriteLine("💡 Recommendations:");
            foreach (var rec in update.Update.FusionData.Recommendations)
            {
                Console.WriteLine($"   • {rec}");
            }
        }

        Console.WriteLine("========================");
    }

    private static string GenerateMockJwtToken()
    {
        // This is a mock token for testing purposes
        // In production, you would authenticate with the API and get a real JWT
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("""{"alg":"HS256","typ":"JWT"}"""));
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes($$"""
        {
            "sub": "test-user-123",
            "name": "Test User",
            "iat": {{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}},
            "exp": {{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}}
        }
        """));
        
        return $"{header}.{payload}.mock-signature";
    }

    private static ILogger CreateLogger()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole().SetMinimumLevel(MsLogLevel.Information);
        });
        
        return loggerFactory.CreateLogger<Program>();
    }

    private static async Task Cleanup()
    {
        Console.WriteLine("Cleaning up resources...");

        _isAnalyzing = false;

        if (_connection != null)
        {
            try
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing SignalR connection: {ex.Message}");
            }
        }

        _capture?.Dispose();
        _httpClient?.Dispose();
        
        Console.WriteLine("✓ Cleanup completed");
    }
}
