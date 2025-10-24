# 資料模型設計：會員註冊流程

**日期**: 2025-10-24  
**專案**: Duotify Membership - 會員註冊流程  
**分支**: `001-member-registration`

本文件定義系統的核心實體、資料庫架構、欄位規範與關係設計。

---

## 實體關係圖 (ERD)

```
┌─────────────────────────────────────┐
│            Member                    │
├─────────────────────────────────────┤
│ Id (PK)                 : Guid       │
│ NationalId (UK)         : string(10) │
│ Name                    : string(50) │
│ Email                   : string(254)│
│ PasswordHash            : string(60) │
│ IsEmailVerified         : bool       │
│ CreatedAt               : DateTime   │
│ UpdatedAt               : DateTime?  │
└─────────────────────────────────────┘
                │
                │ 1
                │
                │ HasMany
                ▼
                │ *
┌─────────────────────────────────────┐
│       VerificationCode               │
├─────────────────────────────────────┤
│ Id (PK)                 : Guid       │
│ MemberId (FK)           : Guid       │
│ Code                    : string(6)  │
│ CreatedAt               : DateTime   │
│ ExpiresAt               : DateTime   │
│ IsUsed                  : bool       │
│ FailedAttempts          : int        │
└─────────────────────────────────────┘
```

---

## 實體詳細定義

### 1. Member (會員)

代表系統中已註冊的使用者帳號。

#### 欄位規範

| 欄位名稱 | 資料型別 | 必填 | 唯一 | 預設值 | 說明 |
|---------|---------|------|------|-------|------|
| `Id` | `Guid` | ✅ | ✅ | `Guid.NewGuid()` | 主鍵,系統內部識別碼 |
| `NationalId` | `string(10)` | ✅ | ✅ | - | 台灣身分證字號,業務主鍵 |
| `Name` | `string(50)` | ✅ | ❌ | - | 會員姓名 |
| `Email` | `string(254)` | ✅ | ❌ | - | 電子郵件地址 (RFC 5321 最大長度) |
| `PasswordHash` | `string(60)` | ✅ | ❌ | - | BCrypt 雜湊後的密碼 (固定 60 字元) |
| `IsEmailVerified` | `bool` | ✅ | ❌ | `false` | E-Mail 驗證狀態 |
| `CreatedAt` | `DateTime` | ✅ | ❌ | `DateTime.UtcNow` | 帳號建立時間 (UTC) |
| `UpdatedAt` | `DateTime?` | ❌ | ❌ | `null` | 最後更新時間 (UTC) |

#### 驗證規則

1. **NationalId** (身分證字號):
   - 格式: 1 碼英文 (A-Z) + 9 碼數字
   - 必須通過台灣身分證檢查碼驗證
   - 大小寫不敏感 (儲存時統一轉為大寫)
   - 資料庫層級唯一性約束 (UNIQUE INDEX)

2. **Name** (姓名):
   - 長度: 1-50 字元
   - 允許中文、英文、空格

3. **Email** (電子郵件):
   - 格式: 符合 RFC 5322 Email 格式
   - 長度: 最大 254 字元
   - 範例: `user@example.com`

4. **PasswordHash** (密碼雜湊):
   - 原始密碼驗證規則:
     - 長度: 8-20 字元
     - 必須包含至少 1 個英文大寫 (A-Z)
     - 必須包含至少 1 個英文小寫 (a-z)
     - 必須包含至少 1 個數字 (0-9)
   - 儲存格式: BCrypt hash (Work Factor: 12)
   - 長度: 固定 60 字元

5. **IsEmailVerified** (驗證狀態):
   - `false`: 未驗證 (預設值)
   - `true`: 已透過驗證碼完成驗證

#### 索引設計

```sql
-- 主鍵索引 (自動建立)
CREATE CLUSTERED INDEX PK_Members ON Members(Id);

-- 唯一性約束索引 (防止重複註冊)
CREATE UNIQUE NONCLUSTERED INDEX IX_Members_NationalId ON Members(NationalId);

-- 查詢最佳化索引
CREATE NONCLUSTERED INDEX IX_Members_Email ON Members(Email);
CREATE NONCLUSTERED INDEX IX_Members_CreatedAt ON Members(CreatedAt);
```

#### 業務規則

1. **註冊時**:
   - 檢查 `NationalId` 是否已存在 → 若存在則拒絕註冊
   - 密碼必須使用 BCrypt Work Factor 12 進行雜湊
   - `IsEmailVerified` 預設為 `false`
   - `CreatedAt` 自動設定為當前 UTC 時間

2. **登入時**:
   - 未驗證使用者 (`IsEmailVerified = false`) 可登入,但功能受限
   - 使用 BCrypt.Verify() 驗證密碼

3. **驗證完成時**:
   - 設定 `IsEmailVerified = true`
   - 更新 `UpdatedAt` 為當前 UTC 時間

#### EF Core Entity 定義

```csharp
public class Member
{
    public Guid Id { get; set; }
    public string NationalId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation Property
    public ICollection<VerificationCode> VerificationCodes { get; set; } = new List<VerificationCode>();
}
```

---

### 2. VerificationCode (驗證碼)

代表用於 E-Mail 驗證的臨時憑證。

#### 欄位規範

| 欄位名稱 | 資料型別 | 必填 | 唯一 | 預設值 | 說明 |
|---------|---------|------|------|-------|------|
| `Id` | `Guid` | ✅ | ✅ | `Guid.NewGuid()` | 主鍵 |
| `MemberId` | `Guid` | ✅ | ❌ | - | 外鍵,關聯至 Member.Id |
| `Code` | `string(6)` | ✅ | ❌ | - | 6 位數驗證碼 (000000-999999) |
| `CreatedAt` | `DateTime` | ✅ | ❌ | `DateTime.UtcNow` | 驗證碼產生時間 (UTC) |
| `ExpiresAt` | `DateTime` | ✅ | ❌ | `CreatedAt + 5 mins` | 驗證碼過期時間 (UTC) |
| `IsUsed` | `bool` | ✅ | ❌ | `false` | 驗證碼是否已被使用 |
| `FailedAttempts` | `int` | ✅ | ❌ | `0` | 驗證失敗次數 |

#### 驗證規則

1. **Code** (驗證碼):
   - 格式: 6 位數數字 (000000-999999)
   - 產生方式: 使用 `RandomNumberGenerator` (CSPRNG)
   - 範例: `123456`, `000789`, `999000`

2. **ExpiresAt** (過期時間):
   - 計算: `CreatedAt + TimeSpan.FromMinutes(5)`
   - 驗證時需檢查 `DateTime.UtcNow <= ExpiresAt`

3. **IsUsed** (使用狀態):
   - `false`: 未使用 (預設值)
   - `true`: 已被成功使用 (驗證成功後設定)

4. **FailedAttempts** (失敗次數):
   - 預設: `0`
   - 每次驗證失敗時 +1
   - 達到 3 次後,該驗證碼失效

#### 狀態機 (State Machine)

```
[建立] → IsUsed=false, FailedAttempts=0
    │
    ├─→ [輸入正確] → IsUsed=true → [驗證成功]
    │
    ├─→ [輸入錯誤 #1] → FailedAttempts=1
    │
    ├─→ [輸入錯誤 #2] → FailedAttempts=2
    │
    ├─→ [輸入錯誤 #3] → FailedAttempts=3 → [驗證碼失效]
    │
    └─→ [超過 5 分鐘] → [驗證碼過期]
```

#### 索引設計

```sql
-- 主鍵索引 (自動建立)
CREATE CLUSTERED INDEX PK_VerificationCodes ON VerificationCodes(Id);

-- 外鍵索引 (查詢最佳化)
CREATE NONCLUSTERED INDEX IX_VerificationCodes_MemberId ON VerificationCodes(MemberId);

-- 複合索引 (查詢最新驗證碼)
CREATE NONCLUSTERED INDEX IX_VerificationCodes_MemberId_CreatedAt 
ON VerificationCodes(MemberId, CreatedAt DESC);
```

#### 業務規則

1. **產生驗證碼時**:
   - 產生 6 位數隨機驗證碼
   - 設定 `ExpiresAt = CreatedAt + 5 minutes`
   - 不檢查驗證碼是否重複 (6 位數碰撞機率極低: 1/1,000,000)
   - 允許同一會員有多個未使用的驗證碼 (支援「重新發送」功能)

2. **驗證時**:
   - 檢查驗證碼是否屬於該會員
   - 檢查 `IsUsed == false` (未被使用)
   - 檢查 `FailedAttempts < 3` (未超過錯誤上限)
   - 檢查 `DateTime.UtcNow <= ExpiresAt` (未過期)
   - 比對 `Code` 是否正確

3. **驗證失敗時**:
   - `FailedAttempts += 1`
   - 若 `FailedAttempts >= 3`,該驗證碼失效

4. **驗證成功時**:
   - 設定 `IsUsed = true`
   - 更新 `Member.IsEmailVerified = true`
   - 更新 `Member.UpdatedAt = DateTime.UtcNow`

5. **重新發送驗證碼時**:
   - 產生新的 `VerificationCode` 記錄
   - 舊的驗證碼仍保留 (不刪除),但會自動過期

#### EF Core Entity 定義

```csharp
public class VerificationCode
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string Code { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public int FailedAttempts { get; set; }

    // Navigation Property
    public Member Member { get; set; } = null!;
}
```

---

## 資料庫架構設計

### Fluent API Configuration

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Member Configuration
    modelBuilder.Entity<Member>(entity =>
    {
        entity.ToTable("Members");
        
        entity.HasKey(e => e.Id);
        
        entity.Property(e => e.NationalId)
            .IsRequired()
            .HasMaxLength(10)
            .IsUnicode(false);
            
        entity.HasIndex(e => e.NationalId)
            .IsUnique()
            .HasDatabaseName("IX_Members_NationalId");
            
        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50);
            
        entity.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(254);
            
        entity.HasIndex(e => e.Email)
            .HasDatabaseName("IX_Members_Email");
            
        entity.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(60)
            .IsUnicode(false);
            
        entity.Property(e => e.IsEmailVerified)
            .IsRequired()
            .HasDefaultValue(false);
            
        entity.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
            
        entity.Property(e => e.UpdatedAt)
            .IsRequired(false);
            
        entity.HasMany(e => e.VerificationCodes)
            .WithOne(v => v.Member)
            .HasForeignKey(v => v.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // VerificationCode Configuration
    modelBuilder.Entity<VerificationCode>(entity =>
    {
        entity.ToTable("VerificationCodes");
        
        entity.HasKey(e => e.Id);
        
        entity.Property(e => e.MemberId)
            .IsRequired();
            
        entity.HasIndex(e => e.MemberId)
            .HasDatabaseName("IX_VerificationCodes_MemberId");
            
        entity.HasIndex(e => new { e.MemberId, e.CreatedAt })
            .HasDatabaseName("IX_VerificationCodes_MemberId_CreatedAt");
            
        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(6)
            .IsUnicode(false);
            
        entity.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
            
        entity.Property(e => e.ExpiresAt)
            .IsRequired();
            
        entity.Property(e => e.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);
            
        entity.Property(e => e.FailedAttempts)
            .IsRequired()
            .HasDefaultValue(0);
    });
}
```

---

## 資料遷移 (Migration) 策略

### 初始 Migration

```bash
# 建立初始 Migration
dotnet ef migrations add InitialCreate \
  --project src/Duotify.Membership.Api \
  --context MembershipDbContext

# 檢視產生的 SQL
dotnet ef migrations script \
  --project src/Duotify.Membership.Api

# 套用至資料庫
dotnet ef database update \
  --project src/Duotify.Membership.Api
```

### 預期產生的 SQL (參考)

```sql
-- Create Members Table
CREATE TABLE [Members] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [NationalId] varchar(10) NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Email] nvarchar(254) NOT NULL,
    [PasswordHash] varchar(60) NOT NULL,
    [IsEmailVerified] bit NOT NULL DEFAULT 0,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Members] PRIMARY KEY ([Id])
);

-- Create Unique Index on NationalId
CREATE UNIQUE NONCLUSTERED INDEX [IX_Members_NationalId] 
ON [Members]([NationalId]);

-- Create Index on Email
CREATE NONCLUSTERED INDEX [IX_Members_Email] 
ON [Members]([Email]);

-- Create VerificationCodes Table
CREATE TABLE [VerificationCodes] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [MemberId] uniqueidentifier NOT NULL,
    [Code] varchar(6) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [ExpiresAt] datetime2 NOT NULL,
    [IsUsed] bit NOT NULL DEFAULT 0,
    [FailedAttempts] int NOT NULL DEFAULT 0,
    CONSTRAINT [PK_VerificationCodes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VerificationCodes_Members_MemberId] 
        FOREIGN KEY ([MemberId]) REFERENCES [Members]([Id]) ON DELETE CASCADE
);

-- Create Index on MemberId
CREATE NONCLUSTERED INDEX [IX_VerificationCodes_MemberId] 
ON [VerificationCodes]([MemberId]);

-- Create Composite Index
CREATE NONCLUSTERED INDEX [IX_VerificationCodes_MemberId_CreatedAt] 
ON [VerificationCodes]([MemberId], [CreatedAt] DESC);
```

---

## 查詢範例

### 1. 檢查身分證字號是否已註冊

```csharp
var exists = await _context.Members
    .AsNoTracking()
    .AnyAsync(m => m.NationalId == nationalId);
```

### 2. 取得會員最新的未使用驗證碼

```csharp
var latestCode = await _context.VerificationCodes
    .AsNoTracking()
    .Where(v => v.MemberId == memberId 
             && !v.IsUsed 
             && v.FailedAttempts < 3
             && v.ExpiresAt > DateTime.UtcNow)
    .OrderByDescending(v => v.CreatedAt)
    .FirstOrDefaultAsync();
```

### 3. 驗證碼驗證 (含樂觀鎖)

```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    var code = await _context.VerificationCodes
        .Include(v => v.Member)
        .FirstOrDefaultAsync(v => v.Id == codeId);
        
    if (code == null || code.IsUsed || code.FailedAttempts >= 3)
        throw new InvalidOperationException("驗證碼無效");
        
    if (code.ExpiresAt < DateTime.UtcNow)
        throw new InvalidOperationException("驗證碼已過期");
        
    if (code.Code != inputCode)
    {
        code.FailedAttempts++;
        await _context.SaveChangesAsync();
        throw new InvalidOperationException("驗證碼錯誤");
    }
    
    code.IsUsed = true;
    code.Member.IsEmailVerified = true;
    code.Member.UpdatedAt = DateTime.UtcNow;
    
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

## 資料保留政策

### 驗證碼清理策略

建議定期清理過期的驗證碼以維護資料庫效能:

```csharp
// 每日清理 30 天前的已使用/過期驗證碼 (背景工作)
var cutoffDate = DateTime.UtcNow.AddDays(-30);

await _context.VerificationCodes
    .Where(v => v.CreatedAt < cutoffDate)
    .Where(v => v.IsUsed || v.ExpiresAt < DateTime.UtcNow)
    .ExecuteDeleteAsync();
```

### 會員資料保留

- Member 記錄不自動刪除
- 提供「帳號註銷」功能時,應實作軟刪除 (Soft Delete):
  - 新增 `IsDeleted` 欄位
  - 新增 `DeletedAt` 欄位
  - 查詢時過濾 `IsDeleted == false`

---

## 資料模型版本歷史

| 版本 | 日期 | 變更說明 |
|-----|------|---------|
| 1.0 | 2025-10-24 | 初始版本：Member、VerificationCode 實體定義 |

---

## 下一步

資料模型設計完成後,接下來應:
1. ✅ 實作 EF Core Entities (Member, VerificationCode)
2. ✅ 實作 DbContext 與 Fluent API Configuration
3. ✅ 建立初始 Migration (`dotnet ef migrations add InitialCreate`)
4. ✅ 撰寫 Repository 介面與實作
5. ✅ 撰寫單元測試 (驗證業務規則)
