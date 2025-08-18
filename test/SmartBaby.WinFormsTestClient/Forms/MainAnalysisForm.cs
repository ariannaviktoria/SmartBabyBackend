using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SmartBaby.Core.DTOs;
using System.Drawing.Imaging;
using System.Text;
using System.Text.Json;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SmartBaby.WinFormsTestClient.Forms;

public partial class MainAnalysisForm : Form
{
    private readonly string _jwtToken;
    private readonly string _userEmail;
    private HubConnection? _connection;
    private VideoCapture? _capture;
    private bool _isAnalyzing = false;
    private string? _currentSessionId;
    private readonly System.Windows.Forms.Timer _frameTimer;
    private int _frameCount = 0;

    public MainAnalysisForm(string jwtToken, string userEmail)
    {
        InitializeComponent();
        _jwtToken = jwtToken;
        _userEmail = userEmail;
        
        // Initialize frame timer for camera preview
        _frameTimer = new System.Windows.Forms.Timer();
        _frameTimer.Interval = 33; // ~30 FPS
        _frameTimer.Tick += FrameTimer_Tick;
        
        lblUserInfo.Text = $"Logged in as: {_userEmail}";
        
        // Initialize components sequentially on the UI thread
        this.Load += MainAnalysisForm_Load;
    }

    private async void MainAnalysisForm_Load(object? sender, EventArgs e)
    {
        await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            // Step 1: Initialize camera first
            lblStatus.Text = "Initializing camera...";
            lblStatus.ForeColor = Color.Blue;
            
            if (!InitializeCamera())
            {
                lblStatus.Text = "Camera initialization failed - continuing without camera";
                lblStatus.ForeColor = Color.Orange;
                txtAnalysisLog.AppendText("⚠️ Camera not available, continuing without camera preview\r\n");
            }
            
            // Step 2: Initialize SignalR connection
            lblStatus.Text = "Connecting to SignalR...";
            lblStatus.ForeColor = Color.Blue;
            
            await InitializeSignalRConnection();
            
            // Step 3: Ready for analysis
            lblStatus.Text = "Ready for analysis";
            lblStatus.ForeColor = Color.Green;
            btnStartAnalysis.Enabled = true;
            
            txtAnalysisLog.AppendText("✅ Initialization completed successfully\r\n");
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Initialization failed: {ex.Message}";
            lblStatus.ForeColor = Color.Red;
            txtAnalysisLog.AppendText($"❌ Initialization failed: {ex.Message}\r\n");
            
            MessageBox.Show($"Failed to initialize application:\n{ex.Message}", 
                          "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task InitializeSignalRConnection()
    {
        lblStatus.Text = "Connecting to SignalR...";
        lblStatus.ForeColor = Color.Blue;
        txtAnalysisLog.AppendText("🔗 Connecting to SignalR hub...\r\n");

        try
        {
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

            SetupSignalREventHandlers();

            await _connection.StartAsync();
            
            lblConnectionStatus.Text = "Connected";
            lblConnectionStatus.ForeColor = Color.Green;
            txtAnalysisLog.AppendText("✅ SignalR connection established successfully\r\n");
        }
        catch (Exception ex)
        {
            lblConnectionStatus.Text = "Connection Failed";
            lblConnectionStatus.ForeColor = Color.Red;
            txtAnalysisLog.AppendText($"❌ SignalR connection failed: {ex.Message}\r\n");
            throw new InvalidOperationException($"Failed to connect to SignalR: {ex.Message}", ex);
        }
    }

    private void SetupSignalREventHandlers()
    {
        if (_connection == null) return;

        _connection.On<object>("SessionStarted", (data) =>
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                txtAnalysisLog.AppendText($"📥 Raw SessionStarted data: {json}\r\n");
                
                var sessionData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                
                if (sessionData?.TryGetValue("SessionId", out var sessionIdObj) == true)
                {
                    _currentSessionId = sessionIdObj.ToString();
                    this.Invoke(() =>
                    {
                        lblSessionId.Text = $"Session: {_currentSessionId}";
                        txtAnalysisLog.AppendText($"🎯 Session started: {_currentSessionId}\r\n");
                        btnStartAnalysis.Enabled = false;
                        btnStopAnalysis.Enabled = true;
                        _isAnalyzing = true;
                        lblStatus.Text = "Analysis running...";
                        lblStatus.ForeColor = Color.Green;
                    });
                }
                else
                {
                    this.Invoke(() =>
                    {
                        txtAnalysisLog.AppendText("⚠️ SessionStarted event received but no SessionId found\r\n");
                    });
                }
            }
            catch (Exception ex)
            {
                this.Invoke(() =>
                {
                    txtAnalysisLog.AppendText($"❌ Error processing SessionStarted: {ex.Message}\r\n");
                });
            }
        });

        _connection.On<RealtimeAnalysisResponseDto>("AnalysisUpdate", (analysisUpdate) =>
        {
            try
            {
                if (analysisUpdate != null)
                {
                    this.Invoke(() => DisplayAnalysisUpdate(analysisUpdate));
                }
                else
                {
                    this.Invoke(() =>
                    {
                        txtAnalysisLog.AppendText("⚠️ Received null analysis update\r\n");
                    });
                }
            }
            catch (Exception ex)
            {
                this.Invoke(() =>
                {
                    txtAnalysisLog.AppendText($"❌ Error processing analysis update: {ex.Message}\r\n");
                });
            }
        });

        _connection.On("SessionStopped", (object data) =>
        {
            this.Invoke(() =>
            {
                txtAnalysisLog.AppendText("🛑 Session stopped by server\r\n");
                _currentSessionId = null;
                _isAnalyzing = false;
                lblSessionId.Text = "Session: None";
                btnStartAnalysis.Enabled = true;
                btnStopAnalysis.Enabled = false;
            });
        });

        _connection.On("SessionStoppedConfirmation", (object data) =>
        {
            this.Invoke(() =>
            {
                txtAnalysisLog.AppendText("✅ Session stop confirmed\r\n");
                _currentSessionId = null;
                _isAnalyzing = false;
                lblSessionId.Text = "Session: None";
                btnStartAnalysis.Enabled = true;
                btnStopAnalysis.Enabled = false;
            });
        });

        _connection.On("SessionError", (object data) =>
        {
            this.Invoke(() =>
            {
                var json = JsonSerializer.Serialize(data);
                txtAnalysisLog.AppendText($"❌ Session error: {json}\r\n");
                _isAnalyzing = false;
                btnStartAnalysis.Enabled = true;
                btnStopAnalysis.Enabled = false;
            });
        });

        _connection.On("Error", (string error) =>
        {
            this.Invoke(() =>
            {
                txtAnalysisLog.AppendText($"❌ SignalR Error: {error}\r\n");
            });
        });

        _connection.Closed += (error) =>
        {
            this.Invoke(() =>
            {
                lblConnectionStatus.Text = "Disconnected";
                lblConnectionStatus.ForeColor = Color.Red;
                btnStartAnalysis.Enabled = false;
                btnStopAnalysis.Enabled = false;
                
                if (error != null)
                {
                    txtAnalysisLog.AppendText($"❌ Connection closed with error: {error.Message}\r\n");
                }
                else
                {
                    txtAnalysisLog.AppendText("🔌 Connection closed\r\n");
                }
            });
            return Task.CompletedTask;
        };

        _connection.Reconnected += (connectionId) =>
        {
            this.Invoke(() =>
            {
                lblConnectionStatus.Text = "Connected";
                lblConnectionStatus.ForeColor = Color.Green;
                btnStartAnalysis.Enabled = true;
                txtAnalysisLog.AppendText($"🔄 SignalR reconnected with ID: {connectionId}\r\n");
            });
            return Task.CompletedTask;
        };

        _connection.Reconnecting += (error) =>
        {
            this.Invoke(() =>
            {
                lblConnectionStatus.Text = "Reconnecting...";
                lblConnectionStatus.ForeColor = Color.Orange;
                btnStartAnalysis.Enabled = false;
                btnStopAnalysis.Enabled = false;
                txtAnalysisLog.AppendText("🔄 SignalR reconnecting...\r\n");
            });
            return Task.CompletedTask;
        };
    }

    private bool InitializeCamera()
    {
        lblCameraStatus.Text = "Initializing...";
        lblCameraStatus.ForeColor = Color.Blue;
        txtAnalysisLog.AppendText("📹 Initializing camera...\r\n");

        try
        {
            // Try different camera indices if the first one fails
            for (int cameraIndex = 0; cameraIndex < 3; cameraIndex++)
            {
                try
                {
                    _capture?.Dispose(); // Dispose previous attempt
                    _capture = new VideoCapture(cameraIndex);
                    
                    if (_capture.IsOpened())
                    {
                        // Test if we can actually read a frame
                        using var testFrame = new Mat();
                        bool frameRead = _capture.Read(testFrame);
                        
                        if (frameRead && !testFrame.Empty())
                        {
                            // Set camera properties
                            _capture.Set(VideoCaptureProperties.FrameWidth, 640);
                            _capture.Set(VideoCaptureProperties.FrameHeight, 480);
                            _capture.Set(VideoCaptureProperties.Fps, 30);

                            lblCameraStatus.Text = $"Camera Ready (Index: {cameraIndex})";
                            lblCameraStatus.ForeColor = Color.Green;
                            btnCameraPreview.Enabled = true;
                            txtAnalysisLog.AppendText($"✅ Camera initialized successfully on index {cameraIndex}\r\n");
                            
                            // Start camera preview automatically
                            _frameCount = 0;
                            _frameTimer.Start();
                            btnCameraPreview.Text = "Stop Preview";
                            txtAnalysisLog.AppendText("📹 Camera preview started automatically\r\n");
                            
                            return true; // Success
                        }
                    }
                    
                    txtAnalysisLog.AppendText($"⚠️ Camera index {cameraIndex} not available or no frames\r\n");
                }
                catch (Exception ex)
                {
                    txtAnalysisLog.AppendText($"⚠️ Camera index {cameraIndex} failed: {ex.Message}\r\n");
                }
            }
            
            // If we get here, no camera worked
            throw new InvalidOperationException("No working camera found on any index (0-2)");
        }
        catch (Exception ex)
        {
            _capture?.Dispose();
            _capture = null;
            
            lblCameraStatus.Text = "Camera Error";
            lblCameraStatus.ForeColor = Color.Red;
            btnCameraPreview.Enabled = false;
            txtAnalysisLog.AppendText($"❌ Camera initialization failed: {ex.Message}\r\n");
            return false;
        }
    }

    private async void btnStartAnalysis_Click(object sender, EventArgs e)
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            MessageBox.Show("SignalR connection is not active. Please wait for connection to be established.", 
                          "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtAnalysisLog.AppendText("❌ Cannot start analysis - SignalR not connected\r\n");
            return;
        }

        if (_isAnalyzing)
        {
            MessageBox.Show("Analysis is already running", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnStartAnalysis.Enabled = false;
        btnStopAnalysis.Enabled = true;
        txtAnalysisLog.AppendText("🚀 Starting real-time analysis...\r\n");
        lblStatus.Text = "Starting analysis...";
        lblStatus.ForeColor = Color.Blue;

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
            txtAnalysisLog.AppendText("📡 Sending StartRealtimeAnalysis request...\r\n");
            await _connection.InvokeAsync("StartRealtimeAnalysis", request);
            
            // Don't set _isAnalyzing here - wait for SessionStarted event
            lblStatus.Text = "Analysis request sent...";
            lblStatus.ForeColor = Color.Blue;
            txtAnalysisLog.AppendText("✅ Analysis request sent successfully - waiting for session confirmation\r\n");
        }
        catch (Exception ex)
        {
            txtAnalysisLog.AppendText($"❌ Failed to start analysis: {ex.Message}\r\n");
            lblStatus.Text = "Analysis start failed";
            lblStatus.ForeColor = Color.Red;
            
            btnStartAnalysis.Enabled = true;
            btnStopAnalysis.Enabled = false;
            _isAnalyzing = false;
            
            MessageBox.Show($"Failed to start analysis:\n{ex.Message}", 
                          "Analysis Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnStopAnalysis_Click(object sender, EventArgs e)
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            MessageBox.Show("SignalR connection is not active.", 
                          "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtAnalysisLog.AppendText("❌ Cannot stop analysis - SignalR not connected\r\n");
            return;
        }

        if (!_isAnalyzing)
        {
            MessageBox.Show("No analysis is currently running", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtAnalysisLog.AppendText("⚠️ No analysis session to stop\r\n");
            return;
        }

        if (string.IsNullOrEmpty(_currentSessionId))
        {
            // Try to stop using a generic approach or reset state
            txtAnalysisLog.AppendText("⚠️ No session ID found - resetting state\r\n");
            _isAnalyzing = false;
            btnStartAnalysis.Enabled = true;
            btnStopAnalysis.Enabled = false;
            lblSessionId.Text = "Session: None";
            return;
        }

        txtAnalysisLog.AppendText("🛑 Stopping analysis...\r\n");
        lblStatus.Text = "Stopping analysis...";
        lblStatus.ForeColor = Color.Orange;

        try
        {
            txtAnalysisLog.AppendText($"📡 Sending StopRealtimeAnalysis request for session: {_currentSessionId}\r\n");
            await _connection.InvokeAsync("StopRealtimeAnalysis", _currentSessionId);
            
            txtAnalysisLog.AppendText("✅ Stop request sent successfully\r\n");
        }
        catch (Exception ex)
        {
            txtAnalysisLog.AppendText($"❌ Failed to stop analysis: {ex.Message}\r\n");
            lblStatus.Text = "Stop analysis failed";
            lblStatus.ForeColor = Color.Red;
            
            // Reset state manually if the request fails
            _currentSessionId = null;
            _isAnalyzing = false;
            btnStartAnalysis.Enabled = true;
            btnStopAnalysis.Enabled = false;
            lblSessionId.Text = "Session: None";
            
            MessageBox.Show($"Failed to stop analysis:\n{ex.Message}", 
                          "Analysis Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnCameraPreview_Click(object sender, EventArgs e)
    {
        if (_capture?.IsOpened() != true)
        {
            MessageBox.Show("Camera not available", "Camera Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (btnCameraPreview.Text == "Start Preview")
        {
            _frameCount = 0;
            _frameTimer.Start();
            btnCameraPreview.Text = "Stop Preview";
            txtAnalysisLog.AppendText("📹 Camera preview started\r\n");
        }
        else
        {
            _frameTimer.Stop();
            btnCameraPreview.Text = "Start Preview";
            pictureBoxCamera.Image?.Dispose();
            pictureBoxCamera.Image = null;
            txtAnalysisLog.AppendText("📹 Camera preview stopped\r\n");
            _frameCount = 0;
        }
    }

    private void FrameTimer_Tick(object? sender, EventArgs e)
    {
        if (_capture?.IsOpened() != true) 
        {
            txtAnalysisLog.AppendText("⚠️ Camera not available - stopping frame timer\r\n");
            _frameTimer.Stop();
            btnCameraPreview.Text = "Start Preview";
            _frameCount = 0;
            return;
        }

        try
        {
            using var frame = new Mat();
            bool frameRead = _capture.Read(frame);
            
            if (!frameRead || frame.Empty()) 
            {
                txtAnalysisLog.AppendText("⚠️ No frame data from camera\r\n");
                return;
            }

            // Check frame dimensions
            if (frame.Width <= 0 || frame.Height <= 0)
            {
                txtAnalysisLog.AppendText("⚠️ Invalid frame dimensions\r\n");
                return;
            }

            // Convert OpenCV Mat to Bitmap for display
            var bitmap = BitmapConverter.ToBitmap(frame);
            
            // Dispose previous image
            pictureBoxCamera.Image?.Dispose();
            pictureBoxCamera.Image = bitmap;
            
            // Increment frame counter
            _frameCount++;
            
            // Update status every 30 frames (~1 second at 30 FPS)
            if (_frameCount % 30 == 0)
            {
                txtAnalysisLog.AppendText($"📹 Camera preview active - Frame {_frameCount} (Running for {_frameCount / 30} seconds)\r\n");
            }
        }
        catch (Exception ex)
        {
            txtAnalysisLog.AppendText($"❌ Camera frame error: {ex.Message}\r\n");
            
            // Stop the timer on repeated errors
            _frameTimer.Stop();
            btnCameraPreview.Text = "Start Preview";
            pictureBoxCamera.Image?.Dispose();
            pictureBoxCamera.Image = null;
            _frameCount = 0;
        }
    }

    private void DisplayAnalysisUpdate(RealtimeAnalysisResponseDto update)
    {
        var logEntry = new StringBuilder();
        logEntry.AppendLine("🔍 === Analysis Update ===");
        
        // Debug: Show raw update data
        var updateJson = JsonSerializer.Serialize(update, new JsonSerializerOptions { WriteIndented = true });
        txtAnalysisLog.AppendText($"📥 Raw update data: {updateJson}\r\n");
        
        logEntry.AppendLine($"Session ID: {update.SessionId ?? "NULL"}");
        logEntry.AppendLine($"Timestamp: {update.Timestamp:HH:mm:ss.fff}");
        
        if (update.Update != null)
        {
            logEntry.AppendLine($"Update Type: {update.Update.UpdateType}");

            // Update the current analysis display
            lblCurrentAnalysis.Text = $"Latest: {update.Update.UpdateType} at {update.Timestamp:HH:mm:ss}";

            switch (update.Update.UpdateType)
            {
                case UpdateType.EmotionUpdate when update.Update.EmotionData != null:
                    var emotion = update.Update.EmotionData;
                    logEntry.AppendLine($"😊 Emotion: {emotion.DetectedMood ?? "Unknown"} ({emotion.Confidence:P1} confidence)");
                    lblEmotionResult.Text = $"Emotion: {emotion.DetectedMood ?? "Unknown"} ({emotion.Confidence:P1})";
                    lblEmotionResult.ForeColor = emotion.Confidence > 0.7 ? Color.Green : Color.Orange;
                    
                    if (emotion.AllEmotions?.Any() == true)
                    {
                        logEntry.AppendLine("   Emotion Scores:");
                        foreach (var score in emotion.AllEmotions.Take(3))
                        {
                            logEntry.AppendLine($"     {score.Key}: {score.Value:P1}");
                        }
                    }
                    break;

                case UpdateType.CryUpdate when update.Update.CryData != null:
                    var cry = update.Update.CryData;
                    logEntry.AppendLine($"👶 Crying: {(cry.CryDetected ? "Yes" : "No")}");
                    lblCryResult.Text = cry.CryDetected ? $"Crying: {cry.CryReason ?? "Unknown"} ({cry.Confidence:P1})" : "Crying: No";
                    lblCryResult.ForeColor = cry.CryDetected ? Color.Red : Color.Green;
                    
                    if (cry.CryDetected)
                    {
                        logEntry.AppendLine($"   Reason: {cry.CryReason ?? "Unknown"}");
                        logEntry.AppendLine($"   Confidence: {cry.Confidence:P1}");
                    }
                    break;

                case UpdateType.FusionUpdate when update.Update.FusionData != null:
                    var fusion = update.Update.FusionData;
                    logEntry.AppendLine($"🔄 Fusion: {fusion.OverallState ?? "Unknown"}");
                    logEntry.AppendLine($"   Confidence: {fusion.Confidence:P1}");
                    lblFusionResult.Text = $"Overall: {fusion.OverallState ?? "Unknown"} ({fusion.Confidence:P1})";
                    
                    if (fusion.AlertLevel != AlertLevel.Normal)
                    {
                        logEntry.AppendLine($"   Alert Level: {fusion.AlertLevel}");
                        lblAlertLevel.Text = $"Alert: {fusion.AlertLevel}";
                        lblAlertLevel.ForeColor = fusion.AlertLevel == AlertLevel.High ? Color.Red : Color.Orange;
                    }
                    else
                    {
                        lblAlertLevel.Text = "Alert: Normal";
                        lblAlertLevel.ForeColor = Color.Green;
                    }
                    
                    if (fusion.Recommendations?.Any() == true)
                    {
                        logEntry.AppendLine("💡 Recommendations:");
                        foreach (var rec in fusion.Recommendations)
                        {
                            logEntry.AppendLine($"   • {rec}");
                        }
                    }
                    break;
                    
                default:
                    logEntry.AppendLine($"⚠️ Unknown or empty update type: {update.Update.UpdateType}");
                    break;
            }
        }
        else
        {
            logEntry.AppendLine("⚠️ Update object is null");
        }

        logEntry.AppendLine("========================");
        txtAnalysisLog.AppendText(logEntry.ToString());
        
        // Auto-scroll to bottom
        txtAnalysisLog.SelectionStart = txtAnalysisLog.Text.Length;
        txtAnalysisLog.ScrollToCaret();
    }

    private void btnClearLog_Click(object sender, EventArgs e)
    {
        txtAnalysisLog.Clear();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _frameTimer?.Stop();
        _frameTimer?.Dispose();
        
        if (_isAnalyzing && !string.IsNullOrEmpty(_currentSessionId))
        {
            try
            {
                _connection?.InvokeAsync("StopRealtimeAnalysis", _currentSessionId).Wait(1000);
            }
            catch { }
        }
        
        _connection?.StopAsync().Wait(1000);
        _connection?.DisposeAsync().AsTask().Wait(1000);
        _capture?.Dispose();
        pictureBoxCamera.Image?.Dispose();
        
        base.OnFormClosing(e);
    }
}
