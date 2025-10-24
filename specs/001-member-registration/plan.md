# Implementation Plan: 會員註冊流程

**Branch**: `001-member-registration` | **Date**: 2025-10-24 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-member-registration/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

實作一個安全的會員註冊系統,讓新使用者能透過填寫身分證字號、姓名、E-Mail 與密碼完成註冊,並透過 E-Mail 收到的 6 位數驗證碼啟用帳號。系統使用 ASP.NET Core 9.0 Web API 建構,採用 SQL Server 資料庫與 EF Core Code First 工作流程。密碼使用 bcrypt 或 Argon2 進行雜湊處理,驗證碼透過第三方交易式 E-Mail 服務發送。

## Technical Context

**Language/Version**: C# / .NET 9.0 (ASP.NET Core 9.0 Web API)  
**Primary Dependencies**: 
- Entity Framework Core 9.0 (Code First)
- BCrypt.Net-Next 或 Argon2 (密碼雜湊)
- NEEDS CLARIFICATION: E-Mail 服務套件 (SendGrid / AWS SES / Mailgun)
- FluentValidation (輸入驗證)
- NEEDS CLARIFICATION: 日誌框架 (Serilog / NLog)

**Storage**: SQL Server (透過 EF Core Code First migrations)  
**Testing**: xUnit + Moq + FluentAssertions  
**Target Platform**: Linux/Windows Server (Docker 容器部署)  
**Project Type**: 單一後端 API 專案 (無前端實作)  
**Performance Goals**: 
- API 回應時間: <200ms (p50), <500ms (p95)
- 身分證字號重複檢查: <1 秒
- E-Mail 發送: 30 秒內完成

**Constraints**: 
- 不使用 AutoMapper (直接使用 POCO)
- 不使用 Redis
- 不使用 Minimal APIs (使用傳統 Controller)
- 資料庫層級唯一性約束 (身分證字號)
- 驗證碼 5 分鐘有效期限
- 驗證碼錯誤嘗試上限 3 次

**Scale/Scope**: 
- 預期並行註冊: 100 位使用者/分鐘
- 註冊峰值處理能力測試
- 單一功能模組 (會員註冊)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Code Quality Excellence (NON-NEGOTIABLE) ✅
- Clean code principles: 採用 C# coding conventions
- SOLID principles: 透過分層架構 (Controller → Service → Repository)
- Error handling: 統一異常處理機制與錯誤回應格式
- Configuration: 敏感資訊存放於 appsettings.json / 環境變數

### II. Test-First Development (NON-NEGOTIABLE) ✅
- TDD workflow: 測試先行開發 (Red-Green-Refactor)
- Unit tests: 所有業務邏輯單元測試,目標涵蓋率 >80%
- Integration tests: 所有 API 端點整合測試
- Mock dependencies: 使用 Moq 模擬外部相依性

### III. User Experience Consistency ✅
- 一致的 REST API 回應格式
- 標準化錯誤訊息 (中文)
- 統一的驗證錯誤回應結構

### IV. Performance Standards (NON-NEGOTIABLE) ✅
- API response times: <200ms (p50), <500ms (p95) ✅ 符合需求
- Database indexing: 身分證字號欄位建立唯一索引
- N+1 queries: EF Core Include/ThenInclude 最佳化
- Performance tests: CI/CD 管線中的效能回歸測試

### V. Observability & Monitoring ✅
- Structured logging: 使用 Serilog/NLog 結構化日誌
- Correlation IDs: 在 HTTP headers 中追蹤請求
- Health checks: ASP.NET Core Health Check endpoints
- Error tracking: 記錄堆疊追蹤與上下文資訊

### VI. Security By Design ✅
- Password hashing: BCrypt/Argon2 (含 salt) ✅ 符合需求
- Input validation: FluentValidation 輸入驗證
- SQL injection: EF Core 參數化查詢防護
- HTTPS/TLS: 強制使用 HTTPS
- Secrets management: Azure Key Vault / 環境變數

### VII. Simplicity & Maintainability ✅
- YAGNI principle: 不使用 AutoMapper,直接使用 POCO ✅
- Standard libraries: 使用 .NET 內建功能優先
- Clear separation: Controller / Service / Repository 分層
- ADRs: 記錄關鍵技術決策

### VIII. Language & Localization (NON-NEGOTIABLE) ✅
- Specifications in zh-TW: spec.md, plan.md 使用繁體中文 ✅
- Error messages in zh-TW: 所有使用者面向錯誤訊息使用繁體中文 ✅
- Code comments: 可使用英文以保持技術清晰度

**STATUS**: ✅ PASS - 所有憲法原則符合,可進入 Phase 0 研究

### Phase 1 後重新評估 (2025-10-24)

Phase 1 設計完成後,重新檢視所有憲法原則:

#### I. Code Quality Excellence ✅
- 設計採用 Controller-Service-Repository 分層架構
- SOLID principles 體現在介面設計 (IMemberRepository, IMemberService)
- 錯誤處理透過統一的 ApiResponse<T> 與 ErrorResponse
- 敏感資訊 (SendGrid API Key) 使用 User Secrets 管理

#### II. Test-First Development ✅
- 已建立單元測試與整合測試專案結構
- 已定義測試策略 (xUnit + Moq + FluentAssertions)
- quickstart.md 中包含測試執行指令
- 目標測試涵蓋率 >80%

#### III. User Experience Consistency ✅
- API 回應格式統一 (ApiResponse<T>)
- 錯誤訊息全部使用繁體中文
- OpenAPI 規格定義完整,確保 API 一致性

#### IV. Performance Standards ✅
- 資料庫索引策略已定義 (data-model.md)
- EF Core 查詢最佳化策略已規劃 (AsNoTracking, Include)
- 效能目標已明確 (API <200ms p50, <500ms p95)

#### V. Observability & Monitoring ✅
- Serilog 結構化日誌已規劃
- Health Check 端點已規劃 (/health, /health/ready)
- 日誌策略包含 Correlation IDs

#### VI. Security By Design ✅
- BCrypt 密碼雜湊 (Work Factor: 12)
- EF Core 參數化查詢防止 SQL Injection
- HTTPS 強制執行
- Secrets 管理透過 User Secrets / 環境變數

#### VII. Simplicity & Maintainability ✅
- 不使用 AutoMapper,直接使用 POCO (遵循 YAGNI)
- 不使用 Redis (避免過度工程)
- 不使用 Minimal APIs (使用熟悉的 Controller 模式)
- 架構決策記錄在 research.md

#### VIII. Language & Localization ✅
- 所有文件使用繁體中文 (spec.md, plan.md, research.md, data-model.md, quickstart.md)
- OpenAPI 規格描述使用繁體中文
- 錯誤訊息使用繁體中文
- 程式碼註解可使用英文

**Phase 1 後狀態**: ✅ PASS - 設計完全符合憲法原則,可進入 Phase 2 (任務分解)

## Project Structure

### Documentation (this feature)

```
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
Duotify.Membership/                  # 解決方案根目錄
├── src/
│   └── Duotify.Membership.Api/      # ASP.NET Core Web API 專案
│       ├── Controllers/              # API Controllers
│       │   └── MembersController.cs
│       ├── Models/                   # 實體模型 (EF Core entities)
│       │   ├── Member.cs
│       │   └── VerificationCode.cs
│       ├── DTOs/                     # Data Transfer Objects (POCO)
│       │   ├── RegisterRequest.cs
│       │   ├── VerifyCodeRequest.cs
│       │   └── ApiResponse.cs
│       ├── Services/                 # 業務邏輯層
│       │   ├── IMemberService.cs
│       │   ├── MemberService.cs
│       │   ├── IEmailService.cs
│       │   └── EmailService.cs
│       ├── Repositories/             # 資料存取層
│       │   ├── IMemberRepository.cs
│       │   ├── MemberRepository.cs
│       │   ├── IVerificationCodeRepository.cs
│       │   └── VerificationCodeRepository.cs
│       ├── Data/                     # EF Core DbContext
│       │   └── MembershipDbContext.cs
│       ├── Validators/               # FluentValidation validators
│       │   ├── RegisterRequestValidator.cs
│       │   └── VerifyCodeRequestValidator.cs
│       ├── Middlewares/              # 自訂中介軟體
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   └── RequestLoggingMiddleware.cs
│       ├── Migrations/               # EF Core migrations
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs
│
└── tests/
    ├── Duotify.Membership.Api.UnitTests/       # 單元測試
    │   ├── Services/
    │   │   └── MemberServiceTests.cs
    │   ├── Validators/
    │   │   └── RegisterRequestValidatorTests.cs
    │   └── Utils/
    │       └── TaiwanIdValidatorTests.cs
    │
    └── Duotify.Membership.Api.IntegrationTests/ # 整合測試
        ├── Controllers/
        │   └── MembersControllerTests.cs
        ├── Setup/
        │   └── TestWebApplicationFactory.cs
        └── TestData/
            └── MemberTestData.cs
```

**Structure Decision**: 採用單一 ASP.NET Core Web API 專案結構,遵循傳統 MVC 分層架構 (非 Minimal APIs)。使用 Controller → Service → Repository 分層模式,確保關注點分離與可測試性。測試專案分為單元測試與整合測試兩個獨立專案。

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

無需追蹤 - 所有憲法檢查項目均已通過。

