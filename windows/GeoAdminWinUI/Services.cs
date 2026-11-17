using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GeoAdminWinUI;

public sealed class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
}

public sealed class ApiClient
{
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string baseUrl;
    private readonly string token;

    public ApiClient(string baseUrl, string token)
    {
        this.baseUrl = (baseUrl ?? "").Trim().TrimEnd('/');
        this.token = token ?? "";
    }

    public Task<T> GetAsync<T>(string path, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Get, path, null, ct);

    public async Task<T> SendAsync<T>(
        HttpMethod method,
        string path,
        object? body = null,
        CancellationToken ct = default)
    {
        using var response = await ExecuteAsync(method, path, body, ct);
        var text = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(ExtractError(text, response));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return default!;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(text, Options)
                ?? throw new ApiException("服务端返回了空数据");
        }
        catch (JsonException ex)
        {
            throw new ApiException("解析服务端响应失败：" + ex.Message);
        }
    }

    public async Task<SimpleResult> SendSimpleAsync(
        HttpMethod method,
        string path,
        object? body = null,
        CancellationToken ct = default)
    {
        return await SendAsync<SimpleResult>(method, path, body, ct);
    }

    private async Task<HttpResponseMessage> ExecuteAsync(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ApiException("尚未配置服务端地址，请先到「设置」填写");
        }

        var normalized = path.StartsWith('/') ? path : "/" + path;
        var uri = new Uri(baseUrl + normalized);

        using var request = new HttpRequestMessage(method, uri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.TryAddWithoutValidation("X-Admin-Token", token);
        }

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, Options);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    private static string ExtractError(string text, HttpResponseMessage response)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            try
            {
                var error = JsonSerializer.Deserialize<ApiErrorBody>(text, Options);
                if (!string.IsNullOrWhiteSpace(error?.Description))
                {
                    return error.Description;
                }

                if (!string.IsNullOrWhiteSpace(error?.Error))
                {
                    return error.Error;
                }
            }
            catch (JsonException)
            {
                // 不返回 HTML 等非 JSON 错误页的全文
            }
        }

        return $"请求失败（HTTP {(int)response.StatusCode}）";
    }

    private sealed class ApiErrorBody
    {
        public string? Description { get; set; }
        public string? Error { get; set; }
    }
}

public static class AppState
{
    public static string ServerUrl { get; set; } = "http://127.0.0.1:5230";
    /// <summary>密码 + TOTP 登录后由服务端颁发的短期会话令牌，仅保存在内存中。</summary>
    public static string SessionToken { get; set; } = "";

    public static ApiClient Client => new(ServerUrl, SessionToken);
    public static ApiClient LoginClient => new(ServerUrl, "");

    private static string SettingsDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ZtsWenyuan");

    private static string SettingsFile => Path.Combine(SettingsDirectory, "settings.json");

    public static void Load()
    {
        try
        {
            if (!File.Exists(SettingsFile))
            {
                return;
            }

            var json = File.ReadAllText(SettingsFile);
            var data = JsonSerializer.Deserialize<SettingsData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (data is not null)
            {
                ServerUrl = string.IsNullOrWhiteSpace(data.ServerUrl) ? ServerUrl : data.ServerUrl;
            }
        }
        catch
        {
            // 配置损坏时回退到默认值
        }
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            var json = JsonSerializer.Serialize(new SettingsData
            {
                ServerUrl = ServerUrl
            }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json);
        }
        catch
        {
            // 本地配置写入失败不阻断应用
        }
    }

    private sealed class SettingsData
    {
        public string ServerUrl { get; set; } = "http://127.0.0.1:5230";
    }
}
