# 快速設定指南 - 已實作版本

**狀態**: Phase 1 & 2 完成 ✅  
**下一步**: Phase 3 開始（User Story 1 單元測試）

## 一鍵啟動

### 1. 安裝 .NET 9.0 SDK

**Windows (使用 Chocolatey)**:
```bash
choco install dotnet-sdk-9.0
```

**macOS (使用 Homebrew)**:
```bash
brew install dotnet
```

**Linux (Ubuntu/Debian)**:
```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-9.0
```

**驗證安裝**:
```bash
dotnet --version  # 應該顯示 9.0.x
```

### 2. 建構解決方案

```bash
cd /home/christorng/duotify-membership-v1
dotnet restore
dotnet build
```

### 3. 設定資料庫 (選項 A: 使用 In-Memory - 推薦快速測試)

已預配置，無需額外設定！In-Memory 資料庫自動啟用。

### 4. 設定資料庫 (選項 B: 使用 SQL Server)

編輯 `src/Duotify.Membership.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DuotifyMembership;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True;"
  }
}
```

建立遷移:
```bash
dotnet ef migrations add InitialCreate `
  --project src/Duotify.Membership.Api `
  --startup-project src/Duotify.Membership.Api
```

應用遷移:
```bash
dotnet ef database update `
  --project src/Duotify.Membership.Api `
  --startup-project src/Duotify.Membership.Api
```

### 5. 執行應用程式

```bash
cd src/Duotify.Membership.Api
dotnet run
```

**預期輸出**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

### 6. 測試 API

**瀏覽器**:
```
https://localhost:5001/swagger
```

**使用 curl 測試註冊**:
```bash
curl -X POST https://localhost:5001/v1/members/register \
  -H "Content-Type: application/json" \
  -d '{
    "nationalId": "A123456789",
    "name": "測試使用者",
    "email": "test@example.com",
    "password": "TestPass123"
  }' \
  -k  # 忽略 HTTPS 憑證警告 (開發環境)
```

**預期回應 (201 Created)**:
```json
{
  "success": true,
  "data": {
    "memberId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "email": "test@example.com",
    "message": "註冊成功！驗證碼已發送至您的電子郵件。"
  }
}
```

### 7. 執行測試

```bash
# 執行所有測試
dotnet test

# 執行特定測試
dotnet test tests/Duotify.Membership.Api.Tests

# 帶涵蓋率報告
dotnet test /p:CollectCoverage=true
```

---

## 📂 核心實作檔案位置

| 功能 | 檔案位置 |
|------|--------|
| 會員實體 | `src/Duotify.Membership.Api/Models/Member.cs` |
| 驗證碼實體 | `src/Duotify.Membership.Api/Models/VerificationCode.cs` |
| 資料庫上下文 | `src/Duotify.Membership.Api/Data/MembershipDbContext.cs` |
| 會員業務邏輯 | `src/Duotify.Membership.Api/Services/MemberService.cs` |
| 驗證碼業務邏輯 | `src/Duotify.Membership.Api/Services/VerificationCodeService.cs` |
| 控制器端點 | `src/Duotify.Membership.Api/Controllers/MembersController.cs` |
| 驗證規則 | `src/Duotify.Membership.Api/Validators/` |
| 資料庫存取 | `src/Duotify.Membership.Api/Repositories/` |

---

## 🧪 Phase 3 準備 - 撰寫單元測試

位置: `tests/Duotify.Membership.Api.Tests/`

### 測試結構（已預備）
```
Validators/
  └── TaiwaneseNationalIdValidatorTests.cs     (已建立)
Services/
  ├── MemberServiceTests.cs                    (待建立)
  └── VerificationCodeServiceTests.cs          (待建立)
Models/
  ├── MemberTests.cs                           (待建立)
  └── VerificationCodeTests.cs                 (待建立)
Integration/
  ├── MembersControllerTests.cs                (待建立)
  └── RegisterFlowTests.cs                     (待建立)
```

### 範例：撰寫單元測試

```csharp
using Xunit;
using Moq;
using FluentAssertions;
using Duotify.Membership.Api.Services;
using Duotify.Membership.Api.Interfaces;
using Duotify.Membership.Api.Dtos;

namespace Duotify.Membership.Api.Tests.Services;

public class MemberServiceTests
{
    private readonly Mock<IMemberRepository> _memberRepositoryMock;
    private readonly Mock<IVerificationCodeService> _verificationCodeServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly MemberService _memberService;

    public MemberServiceTests()
    {
        _memberRepositoryMock = new Mock<IMemberRepository>();
        _verificationCodeServiceMock = new Mock<IVerificationCodeService>();
        _emailServiceMock = new Mock<IEmailService>();
        // 需要注入 ILogger - 使用 NullLogger
        var loggerMock = new Mock<ILogger<MemberService>>();
        
        _memberService = new MemberService(
            _memberRepositoryMock.Object,
            _verificationCodeServiceMock.Object,
            _emailServiceMock.Object,
            loggerMock.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_CreatesMemberSuccessfully()
    {
        // Arrange
        var request = new RegisterRequest
        {
            NationalId = "A123456789",
            Name = "王小明",
            Email = "wang@example.com",
            Password = "Password123"
        };

        _memberRepositoryMock
            .Setup(x => x.NationalIdExistsAsync(request.NationalId))
            .ReturnsAsync(false);

        // Act
        var result = await _memberService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email);
        _memberRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Member>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateNationalId_ThrowsException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            NationalId = "A123456789",
            Name = "王小明",
            Email = "wang@example.com",
            Password = "Password123"
        };

        _memberRepositoryMock
            .Setup(x => x.NationalIdExistsAsync(request.NationalId))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _memberService.RegisterAsync(request)
        );
    }
}
```

---

## 🔑 重要檔案須知

### Program.cs
- 配置 Serilog 日誌
- 設定 DbContext（自動選擇 In-Memory 或 SQL Server）
- 註冊依賴注入
- 設定中介軟體管道

### MembershipDbContext.cs
- 自動配置表結構
- 設定索引和約束
- 支援 SQL Server 和 In-Memory 資料庫

### ErrorHandlingMiddleware.cs
- 捕捉所有異常
- 轉換為適當的 HTTP 狀態碼
- 返回統一格式的錯誤訊息（繁體中文）

---

## ⚠️ 已知限制 / 待完成

- ❌ 電子郵件服務為測試實作（mock），需要配置 SendGrid
- ❌ 授權檢查尚未實作（User Story 2）
- ❌ 速率限制尚未實作（User Story 3）
- ❌ 完整單元測試套件待撰寫
- ❌ 整合測試套件待撰寫

---

## 📞 疑難排解

### 問題：`dotnet command not found`
**解決**: 重新安裝 .NET SDK 或檢查 PATH 環境變數

### 問題：資料庫連線失敗
**解決**: 
- 確保使用 In-Memory 資料庫或
- 檢查 SQL Server 連線字串

### 問題：測試失敗
**解決**: 
- 清除快取: `dotnet clean`
- 重新建構: `dotnet build`
- 檢查測試日誌

---

## 🎯 下一步工作流

1. **安裝 .NET SDK 9.0** ← 您在這裡
2. **執行應用程式** (步驟 2-5)
3. **驗證 API 運作** (步驟 6-7)
4. **開始 Phase 3** - 撰寫 User Story 1 單元測試
5. **實作服務層** - 確保所有測試通過
6. **整合測試** - 端到端流程測試
7. **Phase 4-5** - User Story 2 & 3
8. **Phase 6** - 波蘭、文件、部署準備

---

**資訊**: 詳見 `IMPLEMENTATION.md` 以獲取完整實作報告

