# 快速開始指南：會員註冊流程

**專案**: Duotify Membership API  
**功能**: 會員註冊與 E-Mail 驗證  
**分支**: `001-member-registration`  
**日期**: 2025-10-24

本指南將協助您在本地環境中建立、設定、執行與測試會員註冊功能。

---

## 📋 前置需求

### 必要軟體

| 軟體 | 最低版本 | 安裝指令/連結 |
|-----|---------|------------|
| **.NET SDK** | 9.0+ | https://dotnet.microsoft.com/download |
| **SQL Server** | 2019+ | https://www.microsoft.com/sql-server/sql-server-downloads |
| **Git** | 2.30+ | https://git-scm.com/ |

### 選用軟體 (建議)

| 軟體 | 用途 | 連結 |
|-----|------|-----|
| **Visual Studio 2022** | IDE (Community 版免費) | https://visualstudio.microsoft.com/ |
| **Visual Studio Code** | 輕量級編輯器 | https://code.visualstudio.com/ |
| **SQL Server Management Studio (SSMS)** | 資料庫管理工具 | https://aka.ms/ssmsfullsetup |
| **Azure Data Studio** | 跨平台資料庫工具 | https://aka.ms/azuredatastudio |
| **Postman** | API 測試工具 | https://www.postman.com/downloads/ |

### 驗證安裝

```bash
# 確認 .NET SDK 版本
dotnet --version
# 輸出應為: 9.0.x

# 確認 SQL Server 連線
sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT @@VERSION"
```

---

## 🚀 專案建立

### 1. 複製儲存庫

```bash
# 複製專案
git clone https://github.com/your-org/duotify-membership-v1.git
cd duotify-membership-v1

# 切換到功能分支
git checkout 001-member-registration
```

### 2. 建立解決方案與專案結構

```bash
# 建立解決方案
dotnet new sln -n Duotify.Membership

# 建立 API 專案
mkdir -p src
cd src
dotnet new webapi -n Duotify.Membership.Api --framework net9.0 --no-https false
cd ..

# 建立測試專案
mkdir -p tests
cd tests
dotnet new xunit -n Duotify.Membership.Api.UnitTests
dotnet new xunit -n Duotify.Membership.Api.IntegrationTests
cd ..

# 將專案加入解決方案
dotnet sln add src/Duotify.Membership.Api/Duotify.Membership.Api.csproj
dotnet sln add tests/Duotify.Membership.Api.UnitTests/Duotify.Membership.Api.UnitTests.csproj
dotnet sln add tests/Duotify.Membership.Api.IntegrationTests/Duotify.Membership.Api.IntegrationTests.csproj

# 測試專案加入 API 專案參考
dotnet add tests/Duotify.Membership.Api.UnitTests reference src/Duotify.Membership.Api
dotnet add tests/Duotify.Membership.Api.IntegrationTests reference src/Duotify.Membership.Api
```

### 3. 安裝 NuGet 套件

```bash
# 進入 API 專案目錄
cd src/Duotify.Membership.Api

# Entity Framework Core
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.0

# 密碼雜湊
dotnet add package BCrypt.Net-Next --version 4.0.3

# E-Mail 服務
dotnet add package SendGrid --version 9.29.3

# 日誌
dotnet add package Serilog.AspNetCore --version 8.0.1
dotnet add package Serilog.Sinks.Console --version 5.0.1
dotnet add package Serilog.Sinks.File --version 5.0.0
dotnet add package Serilog.Enrichers.Environment --version 3.0.0
dotnet add package Serilog.Enrichers.Thread --version 3.1.0

# 驗證
dotnet add package FluentValidation.AspNetCore --version 11.3.0

# OpenAPI/Swagger
dotnet add package Swashbuckle.AspNetCore --version 6.5.0

# 返回專案根目錄
cd ../..

# 進入單元測試專案
cd tests/Duotify.Membership.Api.UnitTests
dotnet add package Moq --version 4.20.70
dotnet add package FluentAssertions --version 6.12.0
cd ../..

# 進入整合測試專案
cd tests/Duotify.Membership.Api.IntegrationTests
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 9.0.0
dotnet add package Moq --version 4.20.70
dotnet add package FluentAssertions --version 6.12.0
cd ../..
```

---

## ⚙️ 環境設定

### 1. 設定資料庫連線字串

建立或編輯 `src/Duotify.Membership.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DuotifyMembership;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True;"
  },
  "SendGrid": {
    "ApiKey": "請設定環境變數: SENDGRID_API_KEY",
    "FromEmail": "noreply@duotify.com",
    "FromName": "Duotify 會員系統"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    }
  },
  "AllowedHosts": "*"
}
```

### 2. 設定環境變數 (SendGrid API Key)

**Windows (PowerShell):**
```powershell
$env:SENDGRID_API_KEY="SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
```

**Linux/macOS:**
```bash
export SENDGRID_API_KEY="SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
```

**永久設定 (建議使用 .NET User Secrets):**
```bash
cd src/Duotify.Membership.Api
dotnet user-secrets init
dotnet user-secrets set "SendGrid:ApiKey" "SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
```

### 3. 建立資料庫

```bash
# 確認 SQL Server 執行中
# 建立資料庫 (可透過 SSMS 或指令)
sqlcmd -S localhost -U sa -P YourStrong@Password -Q "CREATE DATABASE DuotifyMembership"

# 或使用 EF Core 自動建立 (執行 migration 時)
```

---

## 📦 資料庫遷移 (EF Core Migrations)

### 1. 建立初始 Migration

```bash
# 確保在專案根目錄
cd /path/to/duotify-membership-v1

# 建立 Migration
dotnet ef migrations add InitialCreate \
  --project src/Duotify.Membership.Api \
  --startup-project src/Duotify.Membership.Api \
  --context MembershipDbContext

# 預覽 SQL (選用)
dotnet ef migrations script \
  --project src/Duotify.Membership.Api \
  --output migrations.sql
```

### 2. 套用 Migration 至資料庫

```bash
# 更新資料庫
dotnet ef database update \
  --project src/Duotify.Membership.Api \
  --startup-project src/Duotify.Membership.Api

# 驗證資料表建立成功
sqlcmd -S localhost -U sa -P YourStrong@Password -d DuotifyMembership \
  -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'"
```

**預期輸出:**
```
TABLE_NAME
---------------------------
Members
VerificationCodes
__EFMigrationsHistory
```

---

## ▶️ 執行應用程式

### 1. 啟動 API 伺服器

```bash
# 方法 1: 使用 dotnet run
cd src/Duotify.Membership.Api
dotnet run

# 方法 2: 使用 dotnet watch (支援熱重載)
dotnet watch run

# 方法 3: 使用 Visual Studio
# 開啟 Duotify.Membership.sln,按 F5 啟動除錯
```

### 2. 驗證 API 執行中

```bash
# 檢查 Health Check 端點
curl https://localhost:5001/health

# 預期回應: Healthy
```

### 3. 存取 Swagger UI

開啟瀏覽器,前往:
```
https://localhost:5001/swagger
```

您應該會看到 API 文件頁面,包含以下端點:
- `POST /v1/members/register` - 會員註冊
- `POST /v1/members/{memberId}/verify` - 驗證 E-Mail
- `POST /v1/members/{memberId}/verification-code/resend` - 重新發送驗證碼

---

## 🧪 測試

### 1. 執行單元測試

```bash
# 執行所有單元測試
dotnet test tests/Duotify.Membership.Api.UnitTests

# 執行特定測試類別
dotnet test tests/Duotify.Membership.Api.UnitTests \
  --filter "FullyQualifiedName~MemberServiceTests"

# 產生測試涵蓋率報告 (需安裝 coverlet)
dotnet test tests/Duotify.Membership.Api.UnitTests \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=lcov \
  /p:CoverletOutput=./coverage/
```

### 2. 執行整合測試

```bash
# 執行所有整合測試
dotnet test tests/Duotify.Membership.Api.IntegrationTests

# 執行特定測試
dotnet test tests/Duotify.Membership.Api.IntegrationTests \
  --filter "FullyQualifiedName~MembersControllerTests"
```

### 3. 手動 API 測試 (使用 curl)

#### 測試 1: 會員註冊

```bash
curl -X POST https://localhost:5001/v1/members/register \
  -H "Content-Type: application/json" \
  -d '{
    "nationalId": "A123456789",
    "name": "王小明",
    "email": "wang@example.com",
    "password": "MyPass123"
  }'
```

**預期回應 (201 Created):**
```json
{
  "success": true,
  "data": {
    "memberId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "wang@example.com",
    "message": "註冊成功!驗證碼已發送至您的電子郵件。"
  }
}
```

#### 測試 2: 驗證 E-Mail (使用上一步取得的 memberId)

```bash
# 先檢查 E-Mail 收到的驗證碼 (例如: 123456)
curl -X POST https://localhost:5001/v1/members/3fa85f64-5717-4562-b3fc-2c963f66afa6/verify \
  -H "Content-Type: application/json" \
  -d '{
    "code": "123456"
  }'
```

**預期回應 (200 OK):**
```json
{
  "success": true,
  "data": {
    "message": "E-Mail 驗證成功!您的帳號已完全啟用。",
    "isEmailVerified": true
  }
}
```

#### 測試 3: 重新發送驗證碼

```bash
curl -X POST https://localhost:5001/v1/members/3fa85f64-5717-4562-b3fc-2c963f66afa6/verification-code/resend
```

**預期回應 (200 OK):**
```json
{
  "success": true,
  "data": {
    "message": "驗證碼已重新發送至您的電子郵件。",
    "email": "wang@example.com"
  }
}
```

---

## 🐛 除錯與疑難排解

### 常見問題

#### 1. 資料庫連線失敗

**錯誤訊息:**
```
A network-related or instance-specific error occurred while establishing a connection to SQL Server.
```

**解決方法:**
- 確認 SQL Server 服務執行中
- 檢查連線字串是否正確 (Server, User Id, Password)
- 確認防火牆允許 1433 port
- 使用 `sqlcmd` 測試連線

#### 2. Migration 失敗

**錯誤訊息:**
```
Build failed. Use dotnet build to see the errors.
```

**解決方法:**
```bash
# 先建構專案
dotnet build src/Duotify.Membership.Api

# 再執行 migration
dotnet ef migrations add InitialCreate --project src/Duotify.Membership.Api
```

#### 3. SendGrid E-Mail 發送失敗

**錯誤訊息:**
```
UNAUTHORIZED: The provided authorization grant is invalid, expired, or revoked
```

**解決方法:**
- 檢查 SendGrid API Key 是否正確設定
- 確認 API Key 有發送 E-Mail 權限
- 前往 SendGrid Dashboard 驗證 API Key 狀態
- 確認環境變數已正確設定:
  ```bash
  echo $SENDGRID_API_KEY  # Linux/macOS
  echo %SENDGRID_API_KEY%  # Windows CMD
  echo $env:SENDGRID_API_KEY  # Windows PowerShell
  ```

#### 4. HTTPS 憑證錯誤

**錯誤訊息:**
```
Unable to configure HTTPS endpoint. No server certificate was specified.
```

**解決方法:**
```bash
# 信任開發憑證
dotnet dev-certs https --trust

# 重新產生開發憑證
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

---

## 📊 日誌與監控

### 查看日誌

**Console 輸出:**
- 執行 `dotnet run` 時,日誌會即時輸出到終端

**檔案日誌:**
- 位置: `src/Duotify.Membership.Api/logs/duotify-YYYYMMDD.log`
- 滾動策略: 每日建立新檔案

**檢視日誌:**
```bash
# 即時監控日誌 (Linux/macOS)
tail -f src/Duotify.Membership.Api/logs/duotify-$(date +%Y%m%d).log

# 搜尋錯誤
grep "Error" src/Duotify.Membership.Api/logs/*.log
```

### Health Check 端點

```bash
# 檢查 API 健康狀態
curl https://localhost:5001/health

# 詳細健康檢查 (包含資料庫連線)
curl https://localhost:5001/health/ready
```

---

## 🔧 開發工具建議

### Visual Studio Code 擴充功能

```bash
# 安裝建議的擴充功能
code --install-extension ms-dotnettools.csharp
code --install-extension ms-mssql.mssql
code --install-extension humao.rest-client
code --install-extension 42crunch.vscode-openapi
```

### Postman Collection

匯入專案提供的 Postman Collection:
```
specs/001-member-registration/contracts/Duotify-Membership-API.postman_collection.json
```

---

## 📚 相關文件

- **功能規格**: [spec.md](./spec.md)
- **技術研究**: [research.md](./research.md)
- **資料模型**: [data-model.md](./data-model.md)
- **API 契約**: [contracts/openapi.yaml](./contracts/openapi.yaml)
- **實作計畫**: [plan.md](./plan.md)

---

## 🚦 下一步

1. ✅ 環境設定完成
2. ✅ 資料庫建立與遷移完成
3. ✅ API 執行中
4. 📝 開始實作功能 (參考 `tasks.md` - 由 `/speckit.tasks` 指令產生)
5. 🧪 撰寫測試
6. 🚀 部署至測試環境

---

## 🆘 需要協助?

- **技術文件**: 參考 `research.md` 與 `data-model.md`
- **API 規格**: 參考 `contracts/openapi.yaml`
- **問題回報**: 建立 GitHub Issue
- **團隊溝通**: Slack #duotify-dev 頻道

---

**版本**: 1.0.0  
**最後更新**: 2025-10-24
