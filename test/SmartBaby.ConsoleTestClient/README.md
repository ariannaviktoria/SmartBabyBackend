# SmartBaby Console Test Client

This console application tests the SignalR real-time analysis functionality using your webcam.

## Features

- **SignalR Connection Testing**: Connects to the SmartBaby API SignalR hub
- **Live Camera Integration**: Uses OpenCV to capture webcam frames
- **Real-Time Analysis Simulation**: Tests the analysis workflow
- **Interactive Menu**: User-friendly console interface
- **Comprehensive Logging**: Detailed logging for debugging

## Prerequisites

1. **SmartBaby API Running**: Make sure your SmartBaby API is running on `https://localhost:55362`
2. **Webcam Available**: Ensure you have a working webcam connected
3. **OpenCV Libraries**: Will be installed automatically via NuGet

## How to Run

1. **Navigate to the test project directory**:
   ```bash
   cd test/SmartBaby.ConsoleTestClient
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Run the application**:
   ```bash
   dotnet run
   ```

## Usage Instructions

### Menu Options:

1. **Start Real-Time Analysis** - Begins a SignalR session and starts camera capture
2. **Stop Analysis** - Stops the current analysis session
3. **Get Active Sessions** - Retrieves list of active analysis sessions
4. **Show Camera Preview** - Opens a window showing live camera feed (press 'q' to close)
5. **Test Single Frame Analysis** - Captures and processes a single frame
6. **Connection Status** - Shows current connection and session status
0. **Exit** - Closes the application

### Testing Workflow:

1. Start the application
2. Verify SignalR connection is established
3. Test camera preview to ensure webcam is working
4. Start real-time analysis
5. Monitor the analysis updates in the console
6. Stop analysis when done

## What Gets Tested

### SignalR Functionality:
- ✅ Connection establishment with JWT authentication
- ✅ Session creation and management
- ✅ Real-time message receiving
- ✅ Error handling and reconnection
- ✅ Group-based messaging

### Camera Integration:
- ✅ Webcam initialization and configuration
- ✅ Frame capture and processing
- ✅ Image format conversion (Mat to Base64)
- ✅ Real-time frame streaming simulation

### Analysis Workflow:
- ✅ Session lifecycle management
- ✅ Real-time update processing
- ✅ Multiple analysis types (emotion, crying, fusion)
- ✅ Recommendation display
- ✅ Alert level handling

## Expected Output

When working correctly, you should see:

```
=== SmartBaby SignalR Real-Time Analysis Test Client ===

Initializing SignalR connection...
✓ SignalR connection established!
Initializing camera...
✓ Camera initialized successfully!

=== Menu ===
1. Start Real-Time Analysis
...

🚀 Starting real-time analysis...
🎯 Session started: abc123-def456-ghi789
📹 Camera capture loop started
📸 Processing frame (15234 bytes) - Session: abc123-def456-ghi789

🔍 === Analysis Update ===
Session ID: abc123-def456-ghi789
Timestamp: 14:30:25.123
Update Type: EmotionUpdate
😊 Emotion: Happy (85.2% confidence)
   Emotion Scores:
     Happy: 85.2%
     Calm: 12.1%
     Neutral: 2.7%
💡 Recommendations:
   • Baby appears content and happy
   • Continue current activity
========================
```

## Troubleshooting

### Common Issues:

1. **SignalR Connection Failed**:
   - Ensure SmartBaby API is running
   - Check the API URL (https://localhost:55362)
   - Verify SSL certificate acceptance

2. **Camera Not Found**:
   - Check if webcam is connected and not used by other applications
   - Try changing camera index in `VideoCapture(0)` to `VideoCapture(1)` etc.

3. **Authentication Errors**:
   - The app uses a mock JWT token for testing
   - In production, implement proper authentication

4. **OpenCV Issues**:
   - Ensure Visual C++ Redistributable is installed
   - Check if Windows Camera privacy settings allow access

### Debug Information:

The application provides detailed logging including:
- SignalR connection state changes
- Camera initialization status
- Frame processing statistics
- Analysis update details
- Error messages with stack traces

## Integration with Backend

This test client simulates the frontend behavior by:

1. **Establishing SignalR Connection**: Connects with authentication
2. **Starting Analysis Sessions**: Calls `StartRealtimeAnalysis` hub method
3. **Receiving Real-Time Updates**: Handles `AnalysisUpdate` events
4. **Managing Session Lifecycle**: Proper session cleanup

The backend responds with:
- Session management confirmations
- Real-time analysis updates
- Error notifications
- Connection state changes

## Next Steps

After successful testing:

1. **Verify All Features Work**: Test all menu options
2. **Check Analysis Quality**: Ensure analysis results make sense
3. **Test Error Scenarios**: Disconnect network, stop API, etc.
4. **Performance Testing**: Monitor memory usage and frame rates
5. **Frontend Integration**: Apply lessons learned to React Native app

This console test client provides a comprehensive way to validate your SignalR real-time analysis implementation before moving to mobile development.
