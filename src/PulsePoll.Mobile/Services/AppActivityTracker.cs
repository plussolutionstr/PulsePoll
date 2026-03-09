namespace PulsePoll.Mobile.Services;

/// <summary>
/// Uygulama açılış, heartbeat ve kapanış olaylarını API'ye bildirir.
/// Platform (iOS/Android), uygulama versiyonu ve cihaz bilgisini gönderir.
/// </summary>
public sealed class AppActivityTracker : IDisposable
{
    private readonly IPulsePollApiClient _apiClient;
    private readonly string _platform;
    private readonly string _appVersion;
    private readonly string _deviceId;
    private PeriodicTimer? _heartbeatTimer;
    private CancellationTokenSource? _cts;

    // AppActivityType enum values (backend: Open=1, Heartbeat=2, Close=3)
    private const int ActivityOpen = 1;
    private const int ActivityHeartbeat = 2;
    private const int ActivityClose = 3;

    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromMinutes(5);

    public AppActivityTracker(IPulsePollApiClient apiClient)
    {
        _apiClient = apiClient;
        _platform = DeviceInfo.Platform == DevicePlatform.iOS ? "iOS" : "Android";
        _appVersion = AppInfo.Current.VersionString;
        _deviceId = GetDeviceId();
    }

    /// <summary>App açıldığında çağrılır. Open event gönderir ve heartbeat timer'ı başlatır.</summary>
    public void Start()
    {
        _ = TrackSafeAsync(ActivityOpen);
        StartHeartbeat();
    }

    /// <summary>App ön plana geldiğinde çağrılır.</summary>
    public void OnResumed()
    {
        _ = TrackSafeAsync(ActivityOpen);
        StartHeartbeat();
    }

    /// <summary>App arka plana gittiğinde çağrılır.</summary>
    public void OnStopped()
    {
        StopHeartbeat();
        _ = TrackSafeAsync(ActivityClose);
    }

    private void StartHeartbeat()
    {
        StopHeartbeat();
        _cts = new CancellationTokenSource();
        _heartbeatTimer = new PeriodicTimer(HeartbeatInterval);
        _ = HeartbeatLoopAsync(_cts.Token);
    }

    private void StopHeartbeat()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;
    }

    private async Task HeartbeatLoopAsync(CancellationToken ct)
    {
        try
        {
            while (await _heartbeatTimer!.WaitForNextTickAsync(ct))
            {
                await TrackSafeAsync(ActivityHeartbeat);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on stop
        }
    }

    private async Task TrackSafeAsync(int activityType)
    {
        try
        {
            await _apiClient.TrackActivityAsync(activityType, _platform, _appVersion, _deviceId);
        }
        catch
        {
            // Telemetri hataları uygulamayı kesmemeli
        }
    }

    private static string GetDeviceId()
    {
        const string key = "device_unique_id";
        var stored = Preferences.Default.Get<string?>(key, null);
        if (!string.IsNullOrEmpty(stored))
            return stored;

        var newId = Guid.NewGuid().ToString("N");
        Preferences.Default.Set(key, newId);
        return newId;
    }

    public void Dispose()
    {
        StopHeartbeat();
    }
}
