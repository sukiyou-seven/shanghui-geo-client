# 座天使·文渊 · Windows 管理端

geoEngine 的 Windows WinUI3 管理客户端。本仓库**只包含 Windows 端代码**；
macOS 客户端源码不在此仓库中。

## 功能

- 启动时使用**登录密码 + TOTP 动态验证码**登录，由服务端校验
- 概览：接入 API、媒体资源、24h 转发、任务池与 MySQL/Redis 状态
- API 接入：**只读查看**。Windows 版不提供新增、编辑、删除、启停、发布和接入测试
- 媒体内容：同步第三方媒体、筛选、发布/下架
- 任务池：执行间隔设置、统计、排队任务查看
- 本程序 API：按分组查看，导出 Markdown 文件
- 转发日志、服务端设置、缓存维护

修改 API 接入配置请使用 macOS 管理端。

## 构建

要求 Windows 10 1809+、.NET SDK 8、Windows App SDK（随项目还原）。

```powershell
dotnet restore windows/GeoAdminWinUI/GeoAdminWinUI.csproj
dotnet build windows/GeoAdminWinUI/GeoAdminWinUI.csproj -c Release -p:Platform=x64
```

发布自包含版本：

```powershell
dotnet publish windows/GeoAdminWinUI/GeoAdminWinUI.csproj -c Release -r win-x64 `
  --self-contained true -p:Platform=x64 -p:WindowsPackageType=None -o publish/win-x64
```

## GitHub Actions / Release

`.github/workflows/windows-build.yml` 只构建 Windows：

- 在 `windows-latest` 上还原、构建、发布自包含 win-x64
- 上传 Actions artifact `GeoAdminWinUI-win-x64`
- 构建成功后自动创建 GitHub Release，tag 形如 `win-v1.0.<run_number>`
- 将 `GeoAdminWinUI-win-x64.zip` 作为 Release asset 上传

Release 页面可直接下载可运行包，无需登录 Actions artifact 页面。
macOS 版本不使用 GitHub Actions，也不在本仓库构建。

## 首次使用

1. 从 Release 下载 `GeoAdminWinUI-win-x64.zip` 并解压
2. 运行 `GeoAdminWinUI.exe`
3. 填写服务端地址（例如 `http://127.0.0.1:5230`）
4. 输入服务端配置的登录密码和身份验证器当前的 6 位 TOTP 验证码

Windows 端只在内存中保存会话令牌；不保存密码、TOTP 密钥或管理令牌。
服务端重启后会话失效，需要重新登录。

登录凭据由服务端本地 `appsettings.Local.json` 的
`Security:LoginPasswordHash` / `Security:TotpSecret` 提供，不会进入任何仓库。
本仓库不包含数据库密码、Redis 密码、管理令牌或 TOTP 密钥。
