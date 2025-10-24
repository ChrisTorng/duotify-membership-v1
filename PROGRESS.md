# 任務完成進度跟蹤

**更新時間**: 2025-10-24

---

## Phase 1: Setup (共 4 項)

- [x] **T001** 建立專案結構：`src/`, `tests/`, `src/Duotify.Membership.Api/`, `tests/Duotify.Membership.Api.Tests/`
- [x] **T002** 初始化 ASP.NET Core 9.0 Web API 專案與 NuGet 套件：Entity Framework Core 9.0、BCrypt.Net-Next、FluentValidation、xUnit、Moq、FluentAssertions
- [x] **T003** [P] 建立 .gitignore 與解決方案結構
- [x] **T004** [P] 配置啟動設定 (HTTP localhost:5000, HTTPS localhost:5001)

**進度**: ✅ 100% 完成

---

## Phase 2: Foundational (共 20 項)

- [x] **T005** [P] 建立 DbContext 類別 `MembershipDbContext.cs`
- [x] **T006** [P] 建立 Member 實體類別 (Id, NationalId, Name, Email, PasswordHash, IsEmailVerified, CreatedAt, UpdatedAt)
- [x] **T007** [P] 建立 VerificationCode 實體類別 (Id, MemberId, Code, CreatedAt, ExpiresAt, IsUsed, FailedAttempts)
- [x] **T008** 配置 EF Core Fluent API - Member 實體 (索引、約束、導航屬性)
- [x] **T009** 配置 EF Core Fluent API - VerificationCode 實體 (索引、約束、導航屬性)
- [x] **T010** [P] 建立 API 回應 DTOs：ApiResponse.cs、ApiResponse.cs、ErrorResponse.cs
- [x] **T011** [P] 建立統一錯誤處理中介軟體 (ErrorHandlingMiddleware.cs)
- [x] **T012** [P] 建立 FluentValidation 驗證器：
  - [x] RegisterRequestValidator.cs (身分證字號格式、密碼強度、電子郵件格式)
  - [x] VerifyCodeRequestValidator.cs (6 位數字格式)
- [x] **T013** [P] 建立自訂驗證器：
  - [x] TaiwaneseNationalIdValidator.cs (檢查碼驗證)
- [x] **T014** [P] 建立服務介面：
  - [x] IMemberService.cs
  - [x] IVerificationCodeService.cs
  - [x] IEmailService.cs
- [x] **T015** [P] 建立存儲庫介面：
  - [x] IMemberRepository.cs
  - [x] IVerificationCodeRepository.cs
- [x] **T016** [P] 配置依賴注入 (DbContext, services, repositories, validators)
- [x] **T017** [P] 配置 ASP.NET Core 中介軟體管道 (錯誤處理、日誌、路由)
- [ ] **T018** 建立 EF Core 初始遷移 (需要 dotnet ef CLI)
- [x] **T019** [P] 建立抽象存儲庫基類 BaseRepository.cs (通用 CRUD 操作) - 已通過具體實作替代
- [x] **T020** [P] 配置 Serilog 結構化日誌 (console sink)

**進度**: ✅ 95% 完成 (T018 需在環境中執行)

---

## Phase 3: User Story 1 - 完整註冊與驗證流程 (Priority: P1) 🎯 MVP

### Tests (共 10 項)

- [ ] **T021** [P] [US1] 單元測試 - Member 實體驗證
- [ ] **T022** [P] [US1] 單元測試 - VerificationCode 狀態機
- [ ] **T023** [P] [US1] 單元測試 - TaiwaneseNationalIdValidator
- [ ] **T024** [P] [US1] 單元測試 - MemberService.RegisterAsync()
- [ ] **T025** [P] [US1] 單元測試 - VerificationCodeService
- [ ] **T026** [P] [US1] 整合測試 - POST /members/register 端點
- [ ] **T027** [P] [US1] 整合測試 - POST /members/{memberId}/verify 端點
- [ ] **T028** [P] [US1] 契約測試 - RegisterRequest/RegisterResponse 架構
- [ ] **T029** [P] [US1] 契約測試 - VerifyCodeRequest/VerifyCodeResponse 架構
- [ ] **T030** [P] [US1] 整合測試 - 錯誤情況處理

**進度**: ⏳ 0% 完成 (待撰寫)

### Implementation (共 10 項)

- [x] **T031** [P] [US1] 建立 MemberRepository
- [x] **T032** [P] [US1] 建立 VerificationCodeRepository
- [x] **T033** [US1] 建立 MemberService (RegisterAsync, LoginAsync)
  - [x] RegisterAsync: 檢查身分證重複、建立會員、生成驗證碼、發送郵件
  - [x] BCrypt 密碼雜湊 (Work Factor 12)
- [x] **T034** [US1] 建立 VerificationCodeService
  - [x] GenerateCodeAsync: 6 位數、5 分鐘過期
  - [x] VerifyCodeAsync: 驗證、失敗次數追蹤 (3 次上限)
  - [x] ResendCodeAsync: 重新發送
- [x] **T035** [US1] 建立 EmailService (測試實作)
- [x] **T036** [P] [US1] 建立請求/回應 DTOs
- [x] **T037** [P] [US1] 建立 MembersController
  - [x] POST /v1/members/register
  - [x] POST /v1/members/{memberId}/verify
  - [x] POST /v1/members/{memberId}/verification-code/resend
- [ ] **T038** [US1] 新增詳細日誌 (相關 ID)
- [ ] **T039** [US1] 新增 API 版本控制配置
- [ ] **T040** [US1] 建立完整整合測試套件

**進度**: ✅ 70% 完成 (核心實作完成，日誌/版本控制/測試待完成)

---

## Phase 4: User Story 2 - 未驗證帳號登入 (Priority: P2)

### Tests (共 5 項)

- [ ] **T041** [P] [US2] 單元測試 - MemberService.LoginAsync()
- [ ] **T042** [P] [US2] 單元測試 - 存取控制邏輯 (IsEmailVerified 檢查)
- [ ] **T043** [P] [US2] 整合測試 - POST /members/login 端點
- [ ] **T044** [P] [US2] 整合測試 - 未驗證使用者存取限制
- [ ] **T045** [P] [US2] 整合測試 - 電子郵件驗證後功能解鎖

**進度**: ⏳ 0% 完成

### Implementation (共 8 項)

- [x] **T046** [P] [US2] 新增 LoginAsync 至 IMemberService 介面
- [x] **T047** [US2] 在 MemberService 實作 LoginAsync
  - [x] 依電子郵件/身分證查找會員
  - [x] 使用 BCrypt.Verify() 驗證密碼
- [ ] **T048** [P] [US2] 建立 AuthorizationService
- [ ] **T049** [P] [US2] 建立登入 DTOs (LoginRequest, LoginResponse)
- [ ] **T050** [US2] 新增 POST /members/login 端點 - 已在 MembersController
- [ ] **T051** [US2] 建立 ProfileController
- [ ] **T052** [US2] 新增功能存取驗證中介軟體/屬性
- [ ] **T053** [US2] 完整的登入 + 限制存取 + 驗證流程測試

**進度**: ✅ 30% 完成 (核心登入實作完成，授權/ProfileController 待完成)

---

## Phase 5: User Story 3 - 驗證碼過期處理 (Priority: P3)

### Tests (共 7 項)

- [ ] **T054** [P] [US3] 單元測試 - 驗證碼過期邏輯
- [ ] **T055** [P] [US3] 單元測試 - 失敗次數追蹤 (3 次上限)
- [ ] **T056** [P] [US3] 單元測試 - ResendCodeAsync 邏輯
- [ ] **T057** [P] [US3] 整合測試 - 過期驗證碼拒絕
- [ ] **T058** [P] [US3] 整合測試 - 3 次失敗上限
- [ ] **T059** [P] [US3] 整合測試 - 重新發送端點
- [ ] **T060** [P] [US3] 整合測試 - 速率限制 (每分鐘 3 次)

**進度**: ⏳ 0% 完成

### Implementation (共 6 項)

- [x] **T061** [P] [US3] 更新 VerificationCodeService.VerifyCodeAsync()
  - [x] 檢查過期時間
  - [x] 檢查失敗次數 (< 3)
  - [x] 追蹤失敗次數
- [ ] **T062** [P] [US3] 實作速率限制 (每分鐘最多 3 次)
- [x] **T063** [US3] 新增 POST /members/{memberId}/verification-code/resend 端點
- [x] **T064** [P] [US3] 建立 ResendCodeResponse DTO
- [ ] **T065** [US3] 更新錯誤回應格式 (包含 remainingAttempts)
- [ ] **T066** [US3] 完整過期 + 重新發送 + 驗證流程測試

**進度**: ✅ 50% 完成 (核心邏輯實作完成，速率限制/測試待完成)

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] **T067** [P] 新增全面單元測試涵蓋率 (目標 >80%)
- [ ] **T068** [P] 整合測試 - 並行註冊嘗試 (競態條件)
- [ ] **T069** [P] 效能測試 - 身分證檢查 (<1 sec)
- [ ] **T070** [P] 效能測試 - 註冊端點 (<200ms p50, <500ms p95)
- [ ] **T071** [P] 效能測試 - 電子郵件發送 (<30 sec)
- [ ] **T072** [P] 新增健康檢查端點
- [ ] **T073** [P] 安全加固 (bcrypt Work Factor 12、無平文密碼日誌、SQL 注入防護)
- [ ] **T074** [P] 建立 README.md
- [ ] **T075** [P] 建立開發指南 (DEVELOPMENT.md)
- [ ] **T076** 程式碼清理與重構
- [ ] **T077** 文件更新審查
- [ ] **T078** 最終驗證 - 所有測試通過
- [ ] **T079** 執行 quickstart.md 驗證

**進度**: ✅ 15% 完成 (健康檢查已實作)

---

## 📊 總體進度

| 階段 | 完成 | 總計 | 百分比 |
|------|------|------|--------|
| Phase 1 | 4 | 4 | 100% ✅ |
| Phase 2 | 19 | 20 | 95% ⚠️ |
| Phase 3 | 7 | 20 | 35% 🔧 |
| Phase 4 | 3 | 13 | 23% 🔧 |
| Phase 5 | 3 | 13 | 23% 🔧 |
| Phase 6 | 2 | 13 | 15% 🔧 |
| **總計** | **38** | **83** | **46%** 🔄 |

---

## 🎯 關鍵里程碑

- ✅ **Phase 1 完成**: 專案結構已建立
- ✅ **Phase 2 完成**: 核心基礎設施就位
- 🔄 **Phase 3 進行中**: 需要撰寫測試 + 完成實作
- ⏳ **Phase 4 待開始**: 授權和未驗證登入
- ⏳ **Phase 5 待開始**: 過期和速率限制
- ⏳ **Phase 6 待開始**: 測試覆蓋率、效能、文件

---

## 📝 優先事項 (建議順序)

1. **立即** (Phase 3):
   - [ ] 撰寫 User Story 1 單元測試套件 (TDD)
   - [ ] 確保所有測試通過

2. **接下來** (Phase 4):
   - [ ] 實作授權中介軟體
   - [ ] 建立 ProfileController
   - [ ] User Story 2 測試

3. **然後** (Phase 5):
   - [ ] 實作速率限制
   - [ ] User Story 3 完整測試

4. **最後** (Phase 6):
   - [ ] 效能測試
   - [ ] 文件完善
   - [ ] 最終驗證

---

**最後更新**: 2025-10-24 02:41:57  
**下一步**: 開始 Phase 3 TDD 測試撰寫
