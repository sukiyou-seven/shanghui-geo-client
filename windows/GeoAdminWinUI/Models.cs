namespace GeoAdminWinUI;

public sealed class ApiDefinitionList
{
    public List<ApiDefinition> Items { get; set; } = new();
    public int Total { get; set; }
}

public sealed class ApiDefinition
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Description { get; set; }
    public string Method { get; set; } = "GET";
    public string UpstreamBaseUrl { get; set; } = "";
    public string UpstreamPath { get; set; } = "";
    public string LocalPath { get; set; } = "";
    public string? Token { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsPublished { get; set; }
    public int CacheTtlSeconds { get; set; }
    public int TimeoutSeconds { get; set; }
    public List<ApiParameter> Parameters { get; set; } = new();

    public bool TokenConfigured => !string.IsNullOrWhiteSpace(Token);
    public string Upstream => $"{UpstreamBaseUrl.TrimEnd('/')}{UpstreamPath}";
    public string StatusText => IsEnabled && IsPublished ? "已启用" : "未启用/未发布";
    public string TokenText => TokenConfigured ? "上游 token 已配置" : "上游 token 未配置";
    public string MetaText => $"{(string.IsNullOrWhiteSpace(Category) ? "其他" : Category)} · 缓存 {CacheTtlSeconds}s · 超时 {TimeoutSeconds}s";
}

public sealed class ApiParameter
{
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
}

public sealed class MediaListResult
{
    public List<MediaItem> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public sealed class MediaItem
{
    public int Id { get; set; }
    public string SourceCode { get; set; } = "";
    public string MediaType { get; set; } = "";
    public long ExternalId { get; set; }
    public string Title { get; set; } = "";
    public string? Category { get; set; }
    public double? Price { get; set; }
    public double? ExtraPrice { get; set; }
    public double? ThirdPrice { get; set; }
    public string? Channel { get; set; }
    public string? Industry { get; set; }
    public string? Area { get; set; }
    public long? Fans { get; set; }
    public long? Reading { get; set; }
    public int? RankScore { get; set; }
    public string? LogoUrl { get; set; }
    public string? CaseUrl { get; set; }
    public string PublishStatus { get; set; } = "draft";
    public DateTime? ExternalUpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string StatusText => PublishStatus switch
    {
        "published" => "已发布",
        "offline" => "已下架",
        _ => "草稿"
    };

    public string ActionText => PublishStatus == "published" ? "下架" : "发布";
    public string PriceText => Price.HasValue ? $"¥{Price.Value:N2}" : "—";
    public string FansText => Fans.HasValue ? Fans.Value.ToString("N0") : "—";
    public string MetaText => $"{(string.IsNullOrWhiteSpace(Category) ? "未分类" : Category)} · {(string.IsNullOrWhiteSpace(Area) ? "—" : Area)} · 粉丝 {FansText}";
    public string UpdatedText => UpdatedAt.HasValue ? UpdatedAt.Value.ToString("MM-dd HH:mm") : "—";
}

public sealed class Dashboard
{
    public DashboardStats Stats { get; set; } = new();
    public List<MediaTypeCount> MediaByType { get; set; } = new();
    public DatabaseStatus Database { get; set; } = new();
    public RedisStatus Redis { get; set; } = new();
    public GatewayStatus Gateway { get; set; } = new();
    public SyncStatus Sync { get; set; } = new();
    public TaskPoolStats? TaskPool { get; set; }
}

public sealed class DashboardStats
{
    public int ApiCount { get; set; }
    public int EnabledApiCount { get; set; }
    public int PublishedApiCount { get; set; }
    public int TokenConfiguredCount { get; set; }
    public int MediaCount { get; set; }
    public int PublishedMediaCount { get; set; }
    public int RequestCount24h { get; set; }
    public int SuccessCount24h { get; set; }
    public double AvgDurationMs { get; set; }

    public double SuccessRate => RequestCount24h == 0 ? 1 : (double)SuccessCount24h / RequestCount24h;
}

public sealed class MediaTypeCount
{
    public string MediaType { get; set; } = "";
    public int Count { get; set; }
}

public sealed class DatabaseStatus
{
    public bool Connected { get; set; }
    public string Display { get; set; } = "";
    public string? SeedVersion { get; set; }
}

public sealed class RedisStatus
{
    public bool Enabled { get; set; }
    public bool Available { get; set; }
    public int Database { get; set; }
    public int LatencyMs { get; set; }
    public string Message { get; set; } = "";
}

public sealed class GatewayStatus
{
    public bool GatewayKeyConfigured { get; set; }
    public bool AdminTokenConfigured { get; set; }
    public string Note { get; set; } = "";
}

public sealed class SyncStatus
{
    public bool Running { get; set; }
    public DateTime? LastStartedAt { get; set; }
    public DateTime? LastFinishedAt { get; set; }
    public string Message { get; set; } = "";
    public List<SyncSourceResult> Sources { get; set; } = new();
}

public sealed class SyncSourceResult
{
    public string SourceCode { get; set; } = "";
    public string MediaType { get; set; } = "";
    public bool Success { get; set; }
    public int Pages { get; set; }
    public int Fetched { get; set; }
    public int Saved { get; set; }
    public string Message { get; set; } = "";
}

public sealed class QueueTaskList
{
    public List<QueueTask> Items { get; set; } = new();
    public int Total { get; set; }
}

public sealed class QueueTask
{
    public int Id { get; set; }
    public string ApiCode { get; set; } = "";
    public string Method { get; set; } = "";
    public string LocalPath { get; set; } = "";
    public string? QueryString { get; set; }
    public string Status { get; set; } = "";
    public int? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public bool CacheHit { get; set; }
    public int RetryCount { get; set; }
    public int WaitMs { get; set; }
    public int DurationMs { get; set; }
    public string Source { get; set; } = "client";
    public string? ClientIp { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public string StatusText => Status switch
    {
        "pending" => "待执行",
        "running" => "执行中",
        "success" => "成功",
        "failed" => "失败",
        _ => Status
    };

    public string SourceText => Source switch
    {
        "client" => "A 端",
        "media-sync" => "媒体同步",
        "admin-test" => "接入测试",
        _ => Source
    };

    public string TitleText => $"#{Id}  {ApiCode}";
    public string WaitText => $"等待 {WaitMs} ms · 执行 {DurationMs} ms";
    public string TimeText => CreatedAt.HasValue ? CreatedAt.Value.ToString("MM-dd HH:mm:ss") : "—";
    public string ErrorText => string.IsNullOrWhiteSpace(ErrorMessage) ? "" : ErrorMessage!;
}

public sealed class TaskPoolStats
{
    public int Pending { get; set; }
    public int Running { get; set; }
    public int Success { get; set; }
    public int Failed { get; set; }
    public DateTime? OldestPendingAt { get; set; }
    public int IntervalSeconds { get; set; }
    public int MaxWaitSeconds { get; set; }
    public int MaxPending { get; set; }
}

public sealed class TaskSettingsResponse
{
    public int IntervalSeconds { get; set; }
    public int MaxWaitSeconds { get; set; }
    public int MaxPending { get; set; }
    public TaskPoolStats Stats { get; set; } = new();
}

public sealed class LogListResult
{
    public List<RequestLog> Items { get; set; } = new();
    public int Total { get; set; }
}

public sealed class RequestLog
{
    public int Id { get; set; }
    public string ApiCode { get; set; } = "";
    public string Method { get; set; } = "";
    public string LocalPath { get; set; } = "";
    public string? QueryString { get; set; }
    public int StatusCode { get; set; }
    public bool Success { get; set; }
    public int DurationMs { get; set; }
    public string? ClientIp { get; set; }
    public string? ErrorMessage { get; set; }
    public int? TaskId { get; set; }
    public int? QueueWaitMs { get; set; }
    public bool? CacheHit { get; set; }
    public string? Source { get; set; }
    public DateTime? CreatedAt { get; set; }

    public string TimeText => CreatedAt.HasValue ? CreatedAt.Value.ToString("MM-dd HH:mm:ss") : "—";
    public string SourceText => Source switch
    {
        "media-sync" => "同步",
        "admin-test" => "测试",
        _ => "A 端"
    };
    public string TaskText => TaskId.HasValue ? $"#{TaskId}" : "";
    public string QueueText => QueueWaitMs is > 0 ? $"排队 {QueueWaitMs} ms" : "";
    public string ErrorText => string.IsNullOrWhiteSpace(ErrorMessage) ? "" : ErrorMessage!;
    public string StatusCodeText => StatusCode.ToString();
    public string DurationText => $"{DurationMs} ms";
}

public sealed class LocalApiCatalog
{
    public string ServerName { get; set; } = "geoEngine";
    public string Version { get; set; } = "1.0.0";
    public string BaseUrl { get; set; } = "";
    public DateTime? GeneratedAt { get; set; }
    public int Total { get; set; }
    public List<LocalApiGroup> Groups { get; set; } = new();
}

public sealed class LocalApiGroup
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<LocalApiItem> Items { get; set; } = new();
}

public sealed class LocalApiItem
{
    public string Id { get; set; } = "";
    public string Group { get; set; } = "";
    public string Name { get; set; } = "";
    public string Method { get; set; } = "GET";
    public string Path { get; set; } = "";
    public string Auth { get; set; } = "";
    public bool RequiresAdminToken { get; set; }
    public bool RequiresGatewayKey { get; set; }
    public string Source { get; set; } = "builtin";
    public string? ApiCode { get; set; }
    public bool Enabled { get; set; } = true;
    public string Description { get; set; } = "";
    public string? Category { get; set; }
    public string? RequestExample { get; set; }
    public string? ResponseExample { get; set; }
    public bool? UpstreamTokenConfigured { get; set; }

    public string StatusText => Enabled ? "启用" : "未启用/未发布";
    public string TokenText => UpstreamTokenConfigured switch
    {
        true => "上游 token 已配置",
        false => "上游 token 未配置",
        null => ""
    };
    public string CategoryText => string.IsNullOrWhiteSpace(Category) ? "" : Category!;
}

public sealed class SimpleResult
{
    public bool? Success { get; set; }
    public string? Description { get; set; }
}

public sealed class CleanupResult
{
    public bool? Success { get; set; }
    public int Deleted { get; set; }
}

public sealed class PublishBody
{
    public List<int> Ids { get; set; } = new();
}

public sealed class SyncRequestBody
{
    public List<string> Sources { get; set; } = new();
    public int MaxPages { get; set; } = 5;
    public bool Full { get; set; } = true;
}

public sealed class TaskIntervalBody
{
    public int IntervalSeconds { get; set; } = 5;
}

public static class UiText
{
    public static string Time(DateTime? value) => value?.ToString("MM-dd HH:mm") ?? "—";

    public static string Number(long? value) => value?.ToString("N0") ?? "—";

    public static string Percent(double value) => $"{value * 100:F0}%";
}


public sealed class LoginRequest
{
    public string Password { get; set; } = "";
    public string Totp { get; set; } = "";
}

public sealed class LoginResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = "";
    public DateTime? ExpiresAt { get; set; }
    public string? Description { get; set; }
}
