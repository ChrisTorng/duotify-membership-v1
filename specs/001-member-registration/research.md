# 技術研究報告：會員註冊流程

**日期**: 2025-10-24  
**專案**: Duotify Membership - 會員註冊流程  
**分支**: `001-member-registration`

本文件記錄所有技術決策的研究過程、決策理由與替代方案評估。

---

## 1. E-Mail 服務選擇

### 決策: **SendGrid**

### 理由:
- **易於整合**: 官方提供 .NET SDK (`SendGrid` NuGet 套件),API 簡單易用
- **免費額度**: 每日 100 封免費 E-Mail,足夠開發與初期測試
- **可靠性**: 99.95% SLA 保證,業界領先的送達率 (>95%)
- **功能完整**: 支援交易式郵件、範本管理、事件追蹤、退信處理
- **文件完整**: 繁體中文文件支援,社群資源豐富
- **企業級**: 被 Uber、Airbnb、Spotify 等大型公司使用

### 評估的替代方案:

#### AWS SES (Amazon Simple Email Service)
- **優點**: 成本最低 ($0.10/1000 封),與 AWS 生態系整合良好
- **缺點**: 
  - 需要 AWS 帳號與信用卡驗證
  - 初期在沙盒模式,需要申請正式環境
  - .NET SDK 較為複雜 (AWSSDK.SimpleEmail)
  - 台灣區域支援較弱
- **為何不選**: 設定複雜度高,不適合快速開發與 MVP

#### Mailgun
- **優點**: 彈性定價,開發者友善 API
- **缺點**: 
  - 免費額度較少 (每月 5000 封,3 個月試用)
  - .NET SDK 非官方維護 (RestSharp.Mailgun)
  - 台灣區域送達率較 SendGrid 低
- **為何不選**: 長期成本較高,社群支援不如 SendGrid

### 實作細節:
```csharp
// NuGet Package: SendGrid
// Configuration: appsettings.json
{
  "SendGrid": {
    "ApiKey": "環境變數: SENDGRID_API_KEY",
    "FromEmail": "noreply@duotify.com",
    "FromName": "Duotify 會員系統"
  }
}
```

---

## 2. 日誌框架選擇

### 決策: **Serilog**

### 理由:
- **結構化日誌**: 原生支援結構化日誌 (JSON 格式),便於查詢與分析
- **ASP.NET Core 整合**: 官方推薦,與 Microsoft.Extensions.Logging 無縫整合
- **Sink 生態系**: 豐富的輸出目標 (Console, File, Seq, Elasticsearch, Application Insights)
- **效能優異**: 非同步寫入,對效能影響最小
- **社群活躍**: .NET 社群最受歡迎的日誌函式庫,持續維護

### 評估的替代方案:

#### NLog
- **優點**: 老牌穩定,XML 設定彈性高
- **缺點**: 
  - 結構化日誌支援較弱,需要額外設定
  - 設定複雜度高 (XML vs. Serilog 的 Fluent API)
  - 效能略遜 Serilog
- **為何不選**: Serilog 在結構化日誌與開發體驗上更優

#### Microsoft.Extensions.Logging (內建)
- **優點**: .NET 內建,無需額外套件
- **缺點**: 
  - 功能陽春,缺乏進階過濾與格式化
  - 結構化日誌支援有限
  - 缺少豐富的 Sink 生態系
- **為何不選**: 對於企業級應用功能不足

### 實作細節:
```csharp
// NuGet Packages:
// - Serilog.AspNetCore
// - Serilog.Sinks.Console
// - Serilog.Sinks.File
// - Serilog.Enrichers.Environment
// - Serilog.Enrichers.Thread

// Configuration: Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.File("logs/duotify-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

---

## 3. 密碼雜湊函式選擇

### 決策: **BCrypt (BCrypt.Net-Next)**

### 理由:
- **業界標準**: 20+ 年歷史,經過充分驗證,被廣泛採用
- **.NET 生態系**: `BCrypt.Net-Next` 是 .NET 社群最受歡迎的實作,持續維護
- **自動 Salt 管理**: 自動產生並儲存 salt,開發者無需手動處理
- **可調整工作因子**: Work Factor 可隨硬體進步調整 (建議: 12-14)
- **簡單易用**: API 簡潔,不易誤用
- **足夠安全**: 對於一般應用場景的安全性已足夠

### 評估的替代方案:

#### Argon2 (Konscious.Security.Cryptography.Argon2)
- **優點**: 
  - 2015 年 Password Hashing Competition 冠軍
  - 抗 ASIC/GPU 破解能力更強
  - 可調整記憶體用量 (Memory-hard function)
- **缺點**: 
  - .NET 實作較不成熟 (Konscious.Security.Cryptography 非官方)
  - 參數調校複雜 (memory cost, time cost, parallelism)
  - 社群採用率較低,資源較少
  - 對伺服器記憶體需求較高
- **為何不選**: BCrypt 對於本專案的安全需求已足夠,Argon2 增加的複雜度不值得

#### PBKDF2 (內建)
- **優點**: .NET 內建 (Rfc2898DeriveBytes),無需外部套件
- **缺點**: 
  - 較易受 GPU 破解 (相較 BCrypt/Argon2)
  - 需要手動管理 salt 與 iteration count
  - 不如 BCrypt/Argon2 現代化
- **為何不選**: BCrypt 安全性更優,且易用性更好

### 實作細節:
```csharp
// NuGet Package: BCrypt.Net-Next

// 雜湊密碼
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);

// 驗證密碼
bool isValid = BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
```

**Work Factor 建議**: 12 (2^12 = 4096 rounds)
- Work Factor 10: ~100ms
- Work Factor 12: ~300ms (建議)
- Work Factor 14: ~1200ms (過高會影響使用者體驗)

---

## 4. ASP.NET Core 9.0 最佳實踐

### 架構模式: **傳統分層架構 (Controller-Service-Repository)**

#### 分層職責:
1. **Controller Layer**: 
   - 處理 HTTP 請求/回應
   - 輸入驗證 (透過 FluentValidation)
   - 呼叫 Service Layer
   - 不包含業務邏輯

2. **Service Layer**: 
   - 業務邏輯實作
   - 編排多個 Repository 操作
   - 交易管理
   - 業務規則驗證

3. **Repository Layer**: 
   - 資料存取抽象
   - EF Core DbContext 封裝
   - CRUD 操作
   - 查詢最佳化

### 相依性注入 (Dependency Injection) 設定:
```csharp
// Program.cs
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IEmailService, EmailService>();
```

### 錯誤處理最佳實踐:
```csharp
// 全域異常處理中介軟體
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 統一 API 回應格式
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorCode { get; set; }
}
```

### FluentValidation 整合:
```csharp
// Program.cs
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Validator 範例
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("身分證字號為必填")
            .Must(BeValidTaiwanId).WithMessage("身分證字號格式錯誤");
            
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼為必填")
            .Length(8, 20).WithMessage("密碼長度必須為 8-20 碼")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")
            .WithMessage("密碼必須包含至少一個英文大寫、小寫與數字");
    }
}
```

### Health Checks 設定:
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<MembershipDbContext>()
    .AddUrlGroup(new Uri("https://api.sendgrid.com/v3/health"), "SendGrid");

app.MapHealthChecks("/health");
```

---

## 5. EF Core Code First 最佳實踐

### Migration 工作流程:
```bash
# 建立 Migration
dotnet ef migrations add InitialCreate --project src/Duotify.Membership.Api

# 預覽 SQL
dotnet ef migrations script --project src/Duotify.Membership.Api

# 套用 Migration
dotnet ef database update --project src/Duotify.Membership.Api
```

### Entity Configuration 最佳實踐:
```csharp
// 使用 Fluent API 在 OnModelCreating 中設定
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Member Entity Configuration
    modelBuilder.Entity<Member>(entity =>
    {
        entity.ToTable("Members");
        
        entity.HasKey(e => e.Id);
        
        entity.Property(e => e.NationalId)
            .IsRequired()
            .HasMaxLength(10)
            .IsUnicode(false);
            
        entity.HasIndex(e => e.NationalId)
            .IsUnique();
            
        entity.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(60); // BCrypt hash length
            
        entity.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(254);
            
        entity.HasMany(m => m.VerificationCodes)
            .WithOne(v => v.Member)
            .HasForeignKey(v => v.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    });
}
```

### 效能最佳化:
1. **索引策略**: 
   - 唯一索引: NationalId (並行衝突防護)
   - 一般索引: Email (查詢最佳化)
   - 複合索引: (MemberId, CreatedAt) for VerificationCode

2. **查詢最佳化**:
   - 使用 `AsNoTracking()` 於唯讀查詢
   - 避免 N+1 問題: 使用 `Include()` 預載
   - 分頁: `Skip().Take()` 大量資料

3. **交易管理**:
   ```csharp
   using var transaction = await _context.Database.BeginTransactionAsync();
   try
   {
       // 多個資料庫操作
       await _context.SaveChangesAsync();
       await transaction.CommitAsync();
   }
   catch
   {
       await transaction.RollbackAsync();
       throw;
   }
   ```

---

## 6. 台灣身分證字號驗證演算法

### 驗證規則:
1. 格式: 1 碼英文字母 + 9 碼數字 (總長度 10)
2. 第 1 碼: 英文字母 (A-Z),代表發證地區
3. 第 2 碼: 1 (男性) 或 2 (女性)
4. 第 3-9 碼: 流水號
5. 第 10 碼: 檢查碼 (Checksum)

### 檢查碼計算:
```
英文字母對應數值 (A=10, B=11, ..., Z=35)
將對應數值拆成兩位數字,第一位數字乘以1,第二位數字乘以9

公式:
(字母十位數×1 + 字母個位數×9 + 第2碼×8 + 第3碼×7 + ... + 第9碼×1) % 10
檢查碼 = (10 - 餘數) % 10
```

### 實作範例:
```csharp
public static class TaiwanIdValidator
{
    private static readonly Dictionary<char, int> LetterValues = new()
    {
        {'A', 10}, {'B', 11}, {'C', 12}, {'D', 13}, {'E', 14},
        {'F', 15}, {'G', 16}, {'H', 17}, {'I', 34}, {'J', 18},
        {'K', 19}, {'L', 20}, {'M', 21}, {'N', 22}, {'O', 35},
        {'P', 23}, {'Q', 24}, {'R', 25}, {'S', 26}, {'T', 27},
        {'U', 28}, {'V', 29}, {'W', 32}, {'X', 30}, {'Y', 31}, {'Z', 33}
    };

    public static bool IsValid(string nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length != 10)
            return false;

        nationalId = nationalId.ToUpper();
        
        if (!LetterValues.ContainsKey(nationalId[0]))
            return false;
            
        if (!char.IsDigit(nationalId[1]) || (nationalId[1] != '1' && nationalId[1] != '2'))
            return false;
            
        for (int i = 2; i < 10; i++)
        {
            if (!char.IsDigit(nationalId[i]))
                return false;
        }

        int letterValue = LetterValues[nationalId[0]];
        int sum = (letterValue / 10) * 1 + (letterValue % 10) * 9;
        
        int[] weights = { 8, 7, 6, 5, 4, 3, 2, 1, 1 };
        for (int i = 1; i < 10; i++)
        {
            sum += (nationalId[i] - '0') * weights[i - 1];
        }

        return sum % 10 == 0;
    }
}
```

---

## 7. 驗證碼產生最佳實踐

### 需求:
- 6 位數數字驗證碼
- 密碼學安全的隨機數產生器 (CSPRNG)
- 避免易混淆字元 (0/O, 1/I/l)

### 實作:
```csharp
public static class VerificationCodeGenerator
{
    public static string Generate()
    {
        // 使用 .NET 的密碼學安全隨機數產生器
        using var rng = RandomNumberGenerator.Create();
        byte[] randomBytes = new byte[4];
        rng.GetBytes(randomBytes);
        
        // 轉換為 6 位數數字 (000000-999999)
        int randomNumber = BitConverter.ToInt32(randomBytes, 0) & int.MaxValue;
        return (randomNumber % 1000000).ToString("D6");
    }
}
```

### 安全性考量:
- **不使用** `Random` 類別 (非密碼學安全)
- **使用** `RandomNumberGenerator` (密碼學安全)
- 儲存時需記錄產生時間與過期時間

---

## 8. E-Mail 範本最佳實踐

### 驗證碼 E-Mail 範本:
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <style>
        body { font-family: 'Microsoft JhengHei', Arial, sans-serif; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .code { font-size: 32px; font-weight: bold; color: #4CAF50; 
                letter-spacing: 5px; text-align: center; padding: 20px; }
        .footer { color: #666; font-size: 12px; text-align: center; }
    </style>
</head>
<body>
    <div class="container">
        <h2>Duotify 會員驗證</h2>
        <p>您好,</p>
        <p>感謝您註冊 Duotify 會員。請使用以下驗證碼完成帳號驗證:</p>
        <div class="code">{{VERIFICATION_CODE}}</div>
        <p>此驗證碼將於 <strong>5 分鐘</strong>後失效。</p>
        <p>如果您沒有註冊 Duotify 帳號,請忽略此郵件。</p>
        <div class="footer">
            <p>此為系統自動發送的郵件,請勿直接回覆。</p>
            <p>&copy; 2025 Duotify. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
```

---

## 研究結論

### 核心技術堆疊確認:
- **語言/框架**: C# / ASP.NET Core 9.0 Web API
- **資料庫**: SQL Server + EF Core 9.0 (Code First)
- **密碼雜湊**: BCrypt (BCrypt.Net-Next, Work Factor: 12)
- **E-Mail 服務**: SendGrid (.NET SDK)
- **日誌**: Serilog (結構化日誌)
- **驗證**: FluentValidation
- **測試**: xUnit + Moq + FluentAssertions

### 關鍵技術決策:
1. ✅ 使用 SendGrid 作為 E-Mail 服務 (易用性 + 可靠性)
2. ✅ 使用 Serilog 作為日誌框架 (結構化日誌 + ASP.NET Core 整合)
3. ✅ 使用 BCrypt 作為密碼雜湊 (安全性足夠 + 易用性高)
4. ✅ 採用 Controller-Service-Repository 分層架構 (關注點分離)
5. ✅ 使用 FluentValidation 進行輸入驗證 (表達力強 + 可測試性)

### Phase 0 完成狀態:
所有 "NEEDS CLARIFICATION" 項目已解決,可進入 Phase 1 設計階段。
