---
description: "Task list for 會員註冊流程 (Member Registration Flow) implementation"
---

# Tasks: 會員註冊流程 (Member Registration Flow)

**Input**: Design documents from `/specs/001-member-registration/`  
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/openapi.yaml  
**Framework**: C# / .NET 9.0 (ASP.NET Core 9.0 Web API)  
**Tests**: Included as part of development (TDD approach with xUnit + Moq)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure for ASP.NET Core 9.0 Web API

- [ ] T001 Create project structure: `src/`, `tests/`, `src/Duotify.Membership.Api/`, `tests/Duotify.Membership.Api.Tests/`
- [ ] T002 Initialize ASP.NET Core 9.0 Web API project with required NuGet packages: Entity Framework Core 9.0, BCrypt.Net-Next, FluentValidation, xUnit, Moq, FluentAssertions
- [ ] T003 [P] Create .gitignore and solution structure
- [ ] T004 [P] Configure launch settings in `src/Duotify.Membership.Api/Properties/launchSettings.json` for local development (HTTP localhost:5000, HTTPS localhost:5001)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T005 [P] Create DbContext class `src/Duotify.Membership.Api/Data/MembershipDbContext.cs` with EF Core configuration
- [ ] T006 [P] Create Member entity class `src/Duotify.Membership.Api/Models/Member.cs` with all properties (Id, NationalId, Name, Email, PasswordHash, IsEmailVerified, CreatedAt, UpdatedAt)
- [ ] T007 [P] Create VerificationCode entity class `src/Duotify.Membership.Api/Models/VerificationCode.cs` with all properties (Id, MemberId, Code, CreatedAt, ExpiresAt, IsUsed, FailedAttempts)
- [ ] T008 Configure EF Core Fluent API in DbContext for Member entity (indexes, constraints, navigation properties) in `src/Duotify.Membership.Api/Data/MembershipDbContext.cs`
- [ ] T009 Configure EF Core Fluent API in DbContext for VerificationCode entity (indexes, constraints, navigation properties) in `src/Duotify.Membership.Api/Data/MembershipDbContext.cs`
- [ ] T010 [P] Create API response DTOs: `src/Duotify.Membership.Api/Dtos/ApiResponse.cs`, `ApiResponseData.cs`, `ErrorResponse.cs`
- [ ] T011 [P] Create unified error handling middleware in `src/Duotify.Membership.Api/Middleware/ErrorHandlingMiddleware.cs` to return consistent error format (繁體中文 error messages)
- [ ] T012 [P] Create validation validators using FluentValidation:
  - `src/Duotify.Membership.Api/Validators/RegisterRequestValidator.cs` (NationalId format, password strength, email format)
  - `src/Duotify.Membership.Api/Validators/VerifyCodeRequestValidator.cs` (6-digit code format)
- [ ] T013 [P] Create custom validators for business rules:
  - `src/Duotify.Membership.Api/Validators/TaiwaneseNationalIdValidator.cs` (checksum validation)
  - Register in dependency injection
- [ ] T014 [P] Create service interfaces:
  - `src/Duotify.Membership.Api/Interfaces/IMemberService.cs`
  - `src/Duotify.Membership.Api/Interfaces/IVerificationCodeService.cs`
  - `src/Duotify.Membership.Api/Interfaces/IEmailService.cs` (for E-Mail sending)
- [ ] T015 [P] Create repository interfaces:
  - `src/Duotify.Membership.Api/Interfaces/IMemberRepository.cs`
  - `src/Duotify.Membership.Api/Interfaces/IVerificationCodeRepository.cs`
- [ ] T016 [P] Configure dependency injection in `src/Duotify.Membership.Api/Program.cs` (DbContext, services, repositories, validators)
- [ ] T017 [P] Configure ASP.NET Core middleware pipeline in `Program.cs` (error handling, logging, routing)
- [ ] T018 Create EF Core initial migration in `src/Duotify.Membership.Api/Migrations/` using `dotnet ef migrations add InitialCreate`
- [ ] T019 [P] Create abstract repository base class `src/Duotify.Membership.Api/Repositories/BaseRepository.cs` for common CRUD operations
- [ ] T020 [P] Create structured logging configuration using Serilog in `Program.cs` with console sink for development

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - 完整註冊與驗證流程 (Priority: P1) 🎯 MVP

**Goal**: New users can register with ID/name/email/password, receive a 6-digit verification code via email, and complete account activation by entering the correct code.

**Independent Test**: Register new user → receive verification email → input verification code → verify account is fully activated with `IsEmailVerified=true`

### Tests for User Story 1 (Unit & Integration Tests - TDD: Write FIRST, ensure FAIL before implementation)

- [ ] T021 [P] [US1] Unit test for Member entity validation (Taiwan ID checksum, password strength rules) in `tests/Duotify.Membership.Api.Tests/Models/MemberTests.cs`
- [ ] T022 [P] [US1] Unit test for VerificationCode entity status machine in `tests/Duotify.Membership.Api.Tests/Models/VerificationCodeTests.cs`
- [ ] T023 [P] [US1] Unit test for TaiwaneseNationalIdValidator in `tests/Duotify.Membership.Api.Tests/Validators/TaiwaneseNationalIdValidatorTests.cs`
- [ ] T024 [P] [US1] Unit test for MemberService.RegisterAsync() in `tests/Duotify.Membership.Api.Tests/Services/MemberServiceTests.cs` (mock IMemberRepository, IVerificationCodeService, IEmailService)
- [ ] T025 [P] [US1] Unit test for VerificationCodeService in `tests/Duotify.Membership.Api.Tests/Services/VerificationCodeServiceTests.cs` (code generation, expiration, failed attempts tracking)
- [ ] T026 [P] [US1] Integration test for POST /members/register endpoint in `tests/Duotify.Membership.Api.Tests/Integration/MembersRegisterEndpointTests.cs`
- [ ] T027 [P] [US1] Integration test for POST /members/{memberId}/verify endpoint in `tests/Duotify.Membership.Api.Tests/Integration/MembersVerifyEndpointTests.cs`
- [ ] T028 [P] [US1] Contract test for RegisterRequest/RegisterResponse schemas in `tests/Duotify.Membership.Api.Tests/Contracts/RegisterContractTests.cs`
- [ ] T029 [P] [US1] Contract test for VerifyCodeRequest/VerifyCodeResponse schemas in `tests/Duotify.Membership.Api.Tests/Contracts/VerifyContractTests.cs`
- [ ] T030 [P] [US1] Integration test for error scenarios (duplicate NationalId=409, invalid email=400, weak password=400, email send failure=500) in `tests/Duotify.Membership.Api.Tests/Integration/ErrorHandlingTests.cs`

### Implementation for User Story 1

- [ ] T031 [P] [US1] Create MemberRepository in `src/Duotify.Membership.Api/Repositories/MemberRepository.cs` implementing IMemberRepository (CRUD + CheckNationalIdExists query)
- [ ] T032 [P] [US1] Create VerificationCodeRepository in `src/Duotify.Membership.Api/Repositories/VerificationCodeRepository.cs` implementing IVerificationCodeRepository (create, get active code, update attempts)
- [ ] T033 [US1] Create MemberService in `src/Duotify.Membership.Api/Services/MemberService.cs` implementing IMemberService:
  - RegisterAsync(RegisterRequest) → creates Member with bcrypt password hash, generates VerificationCode, sends email
  - Depends on: IMemberRepository, IVerificationCodeService, IEmailService
- [ ] T034 [US1] Create VerificationCodeService in `src/Duotify.Membership.Api/Services/VerificationCodeService.cs` implementing IVerificationCodeService:
  - GenerateCodeAsync(memberId) → creates 6-digit code with 5-min expiry
  - VerifyCodeAsync(memberId, code) → validates code, tracks failed attempts, marks used
  - ResendCodeAsync(memberId) → generates new code, sends email
  - Depends on: IVerificationCodeRepository, IEmailService
- [ ] T035 [US1] Create EmailService in `src/Duotify.Membership.Api/Services/EmailService.cs` implementing IEmailService:
  - SendVerificationCodeAsync(email, code, memberId) → sends email with code (NEEDS CLARIFICATION: decide on SendGrid/AWS SES/Mailgun)
  - Note: Currently mock implementation, actual integration deferred per plan.md clarification
- [ ] T036 [P] [US1] Create request/response DTOs:
  - `src/Duotify.Membership.Api/Dtos/RegisterRequest.cs`
  - `src/Duotify.Membership.Api/Dtos/RegisterResponse.cs`
  - `src/Duotify.Membership.Api/Dtos/VerifyCodeRequest.cs`
  - `src/Duotify.Membership.Api/Dtos/VerifyCodeResponse.cs`
- [ ] T037 [P] [US1] Create MembersController in `src/Duotify.Membership.Api/Controllers/MembersController.cs`:
  - POST /members/register → calls MemberService.RegisterAsync(), returns 201 with RegisterResponse
  - POST /members/{memberId}/verify → calls VerificationCodeService.VerifyCodeAsync(), returns 200 with VerifyCodeResponse on success
  - Handle 400 (validation), 409 (duplicate NationalId), 500 (email failure) per OpenAPI spec
- [ ] T038 [US1] Add detailed logging to MemberService and VerificationCodeService (registration attempts, verification attempts, email send status) with correlation IDs
- [ ] T039 [US1] Add API versioning configuration in `Program.cs` (routing to /v1/members/*)
- [ ] T040 [US1] Create comprehensive integration test suite covering all happy path and error scenarios in `tests/Duotify.Membership.Api.Tests/Integration/FullRegistrationFlowTests.cs`

**Checkpoint**: User Story 1 fully functional - new users can register and verify email independently

---

## Phase 4: User Story 2 - 未驗證帳號登入 (Priority: P2)

**Goal**: Unverified users can login with username/password but have restricted access to profile page only. Once verified, all platform features unlock without re-login.

**Independent Test**: Register user without verifying → login with credentials → confirm access restricted to profile page only → verify email → confirm full access restored without re-login

### Tests for User Story 2 (Unit & Integration Tests - TDD)

- [ ] T041 [P] [US2] Unit test for MemberService.LoginAsync() in `tests/Duotify.Membership.Api.Tests/Services/MemberServiceTests.cs`
- [ ] T042 [P] [US2] Unit test for access control logic (IsEmailVerified check) in `tests/Duotify.Membership.Api.Tests/Services/AuthorizationServiceTests.cs`
- [ ] T043 [P] [US2] Integration test for POST /members/login endpoint in `tests/Duotify.Membership.Api.Tests/Integration/MembersLoginEndpointTests.cs`
- [ ] T044 [P] [US2] Integration test for unverified user profile access restrictions in `tests/Duotify.Membership.Api.Tests/Integration/AccessControlTests.cs`
- [ ] T045 [P] [US2] Integration test for feature unlock after email verification in `tests/Duotify.Membership.Api.Tests/Integration/FeatureUnlockTests.cs`

### Implementation for User Story 2

- [ ] T046 [P] [US2] Add LoginAsync method to IMemberService interface
- [ ] T047 [US2] Implement LoginAsync in MemberService:
  - Find member by email/nationalId
  - Verify password using bcrypt.Verify()
  - Return member info without auto-login (per spec: "no auto-login after registration")
- [ ] T048 [P] [US2] Create AuthorizationService in `src/Duotify.Membership.Api/Services/AuthorizationService.cs`:
  - CheckEmailVerificationRequired() → checks IsEmailVerified flag
  - CanAccessFeature(memberId) → returns false if not verified
- [ ] T049 [P] [US2] Create login request/response DTOs:
  - `src/Duotify.Membership.Api/Dtos/LoginRequest.cs`
  - `src/Duotify.Membership.Api/Dtos/LoginResponse.cs`
- [ ] T050 [US2] Add POST /members/login endpoint to MembersController
- [ ] T051 [US2] Create ProfileController in `src/Duotify.Membership.Api/Controllers/ProfileController.cs`:
  - GET /members/{memberId}/profile → allowed for all users (verified + unverified)
  - Authorization: Check membership exists, no feature access restrictions on profile
- [ ] T052 [US2] Add feature access validation middleware or attributes (mark endpoints that require verification)
- [ ] T053 [US2] Create comprehensive test for login + restricted access + verify flow in `tests/Duotify.Membership.Api.Tests/Integration/UnverifiedUserFlowTests.cs`

**Checkpoint**: Users can login unverified, restricted access works, verification unlocks features without re-login

---

## Phase 5: User Story 3 - 驗證碼過期處理 (Priority: P3)

**Goal**: Expired verification codes (>5 mins) are rejected with clear error message. Users can request new codes via resend endpoint. Implement 3-attempt limit per code, with new codes required after limit exceeded.

**Independent Test**: Register user → wait >5 mins → try old code (rejected, CODE_EXPIRED) → resend code → receive new code → verify success within 5 mins

### Tests for User Story 3 (Unit & Integration Tests - TDD)

- [ ] T054 [P] [US3] Unit test for code expiration logic in VerificationCodeService in `tests/Duotify.Membership.Api.Tests/Services/VerificationCodeServiceTests.cs`
- [ ] T055 [P] [US3] Unit test for failed attempts tracking (3-attempt limit) in `tests/Duotify.Membership.Api.Tests/Services/VerificationCodeServiceTests.cs`
- [ ] T056 [P] [US3] Unit test for ResendCodeAsync logic in VerificationCodeService
- [ ] T057 [P] [US3] Integration test for expired code rejection in `tests/Duotify.Membership.Api.Tests/Integration/CodeExpirationTests.cs`
- [ ] T058 [P] [US3] Integration test for 3-attempt limit in `tests/Duotify.Membership.Api.Tests/Integration/AttemptLimitTests.cs`
- [ ] T059 [P] [US3] Integration test for resend endpoint (POST /members/{memberId}/verification-code/resend) in `tests/Duotify.Membership.Api.Tests/Integration/ResendCodeEndpointTests.cs`
- [ ] T060 [P] [US3] Integration test for rate limiting (3 resends per minute per spec) in `tests/Duotify.Membership.Api.Tests/Integration/RateLimitTests.cs`

### Implementation for User Story 3

- [ ] T061 [P] [US3] Update VerificationCodeService.VerifyCodeAsync() to:
  - Check ExpiresAt > DateTime.UtcNow → throw CODE_EXPIRED if expired
  - Check FailedAttempts < 3 before incrementing → throw TOO_MANY_ATTEMPTS after 3 failures
  - Track FailedAttempts on each verification failure
- [ ] T062 [P] [US3] Implement rate limiting for ResendCodeAsync (max 3 per minute) using in-memory cache or database tracking
- [ ] T063 [US3] Add POST /members/{memberId}/verification-code/resend endpoint to MembersController:
  - Generate new VerificationCode (old code remains but expires automatically)
  - Send new code via email
  - Return ALREADY_VERIFIED error if Member.IsEmailVerified == true
  - Return RATE_LIMIT_EXCEEDED if user exceeded 3 resends/min
  - Return 500 if email send fails (per spec: FR-021)
- [ ] T064 [P] [US3] Create ResendCodeResponse DTO: `src/Duotify.Membership.Api/Dtos/ResendCodeResponse.cs`
- [ ] T065 [US3] Update error response format to include remainingAttempts field when CODE error occurs
- [ ] T066 [US3] Add comprehensive integration test for full expiration + resend + verify flow in `tests/Duotify.Membership.Api.Tests/Integration/FullCodeExpirationFlowTests.cs`

**Checkpoint**: All user stories independently functional - expiration, attempt limits, resend all working

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements affecting multiple user stories, documentation, and final validation

- [ ] T067 [P] Add comprehensive unit test coverage (aim for >80%) across all services, validators, repositories
- [ ] T068 [P] Add integration test for concurrent registration attempts (race condition testing for duplicate NationalId with database UNIQUE constraint)
- [ ] T069 [P] Performance testing: Verify NationalId duplicate check completes <1 sec per spec SC-005
- [ ] T070 [P] Performance testing: Verify register endpoint completes <200ms (p50), <500ms (p95) per spec SC-001
- [ ] T071 [P] Performance testing: Verify email send completes within 30 sec per spec SC-002
- [ ] T072 [P] Add health check endpoint in `src/Duotify.Membership.Api/Controllers/HealthController.cs` (GET /health, GET /health/ready)
- [ ] T073 [P] Security hardening:
  - Ensure all passwords hashed with bcrypt Work Factor 12 (not less)
  - Verify no plain text passwords in logs
  - Verify SQL injection prevention (EF Core parameterized queries)
  - Verify HTTPS enforcement in launchSettings.json
- [ ] T074 [P] Create comprehensive README in `src/Duotify.Membership.Api/README.md` with:
  - Project overview
  - Setup instructions (database, migrations, running locally)
  - API endpoints summary (link to OpenAPI spec)
  - Testing instructions
  - Architecture diagram
- [ ] T075 [P] Create development guide in `docs/DEVELOPMENT.md`:
  - Code style conventions (C# naming, formatting)
  - Branching strategy (feature branches)
  - Commit message conventions
  - Testing requirements (unit + integration + contract)
  - Database migration process
- [ ] T076 Code cleanup and refactoring (remove TODO comments, ensure consistent naming, DRY principles)
- [ ] T077 Documentation updates: Review spec.md, plan.md, data-model.md for completeness
- [ ] T078 Final validation: Run all tests, verify no warnings, confirm all user stories independently functional
- [ ] T079 Run quickstart.md validation from `/specs/001-member-registration/quickstart.md` (if exists)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - **BLOCKS all user stories**
- **User Stories (Phase 3-5)**: All depend on Foundational completion
  - User stories can run in parallel (if staffed)
  - Or sequentially in priority order: US1 (P1) → US2 (P2) → US3 (P3)
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies on other stories - can start after Foundational (Phase 2)
  - Core registration + verification flow
  - Independently testable
  - MVP scope
- **User Story 2 (P2)**: Can start after Foundational - integrates with US1 but independently testable
  - Requires: Member entity, login logic, authorization service
  - Builds on: US1 registration/verification
- **User Story 3 (P3)**: Can start after Foundational - independent from US1/US2 core flow
  - Requires: VerificationCode expiration logic, attempt tracking
  - Builds on: US1 code generation

### Within Each User Story

- Tests (TDD) MUST be written and FAIL before implementation
- Tests validate all requirements from spec.md before moving to next task
- Models/entities before services
- Services before endpoints/controllers
- Core implementation before integration
- Story complete and independently testable before moving to next priority

### Parallel Opportunities

- **Phase 1 Setup**: All [P] tasks can run in parallel (project structure, dependencies, config)
- **Phase 2 Foundational**: All [P] tasks can run in parallel (entities, DTOs, validators, middleware, repositories)
- **Phase 3 US1 Tests**: All [P] test tasks (T021-T030) can run in parallel before implementation
- **Phase 3 US1 Repositories**: T031-T032 can run in parallel (different entities)
- **Phase 3 US1 Services**: T033-T035 can run in parallel after repositories ready
- **Phase 3 US1 DTOs**: T036 can run in parallel with services
- **Once Foundational complete**: All user story implementations (US1, US2, US3) can proceed in parallel by different team members

---

## Parallel Example: User Story 1 Implementation

```
Foundational phase COMPLETE → User Story 1 can begin:

PARALLEL TEST WRITING (TDD - all fail initially):
- T021: Member entity validation tests
- T022: VerificationCode status machine tests
- T023: Taiwan ID validator tests
- T024-T025: Service unit tests
- T026-T030: Integration & contract tests

Once tests exist and fail:

PARALLEL IMPLEMENTATION:
- T031-T032: MemberRepository + VerificationCodeRepository
- T036: DTOs (RegisterRequest, RegisterResponse, etc.)
- After repositories ready:
  - T033-T035: MemberService, VerificationCodeService, EmailService (parallel)
- After services ready:
  - T037-T039: MembersController, logging, versioning (parallel)
- T040: Final integration test suite

All tests should now PASS.
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. ✅ Complete Phase 1: Setup
2. ✅ Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. ✅ Complete Phase 3: User Story 1 (TDD: write tests first, implement, verify all pass)
4. **STOP and VALIDATE**: 
   - All US1 tests pass
   - New user can register, receive verification email, verify code, activate account
   - Test independently without US2/US3 features
5. Deploy/demo MVP if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Complete US1 → All US1 tests pass → Deploy/Demo (MVP!)
3. Complete US2 → All US2 tests pass → Verify no regression in US1 → Deploy/Demo
4. Complete US3 → All US3 tests pass → Verify no regression in US1/US2 → Deploy/Demo
5. Complete Polish → Final validation → Deploy/Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy (3+ developers)

With multiple developers:

1. Team completes Setup + Foundational together (1-2 days)
2. Foundational complete → team splits:
   - **Developer A**: User Story 1 (registration + verification)
   - **Developer B**: User Story 2 (unverified login + access control)
   - **Developer C**: User Story 3 (expiration + resend + attempt limits)
3. Each developer:
   - Writes tests first (TDD)
   - Implements features
   - Verifies tests pass
   - Integration tests for their story
4. All stories complete independently
5. Together: Polish phase + final validation

---

## Success Criteria (from spec.md)

Each user story is considered complete when:

- [ ] **US1**: New user successfully registers → receives verification email within 30s (SC-002) → inputs code within 5 mins → account verified → all platform features accessible
- [ ] **US2**: Unverified user logs in → can access only profile page (SC-006) → completes verification → all features unlock without re-login (SC-007)
- [ ] **US3**: Expired code (>5 mins) rejected (SC-001: <3 min registration + verification) → resend works → new code received and verified
- [ ] **Performance**: Registration <200ms (p50), <500ms (p95) (SC-001)
- [ ] **Performance**: NationalId check <1 sec (SC-005)
- [ ] **Concurrency**: 100 users/min registration peak handled without errors (SC-004)
- [ ] **Error handling**: Email failure shows error <2 sec with retry button (SC-008)

---

## Testing Approach

### Test Pyramid

1. **Unit Tests** (Fast, many): Services, validators, repositories with mocked dependencies
2. **Integration Tests** (Medium, some): Full endpoint flows with real DbContext (in-memory test DB)
3. **Contract Tests** (Fast, few): OpenAPI schema validation

### Test Database

- Use in-memory SQLite for integration tests
- Each test gets fresh isolated database
- Setup/teardown in test fixtures

### Running Tests

```bash
# Unit tests only
dotnet test --filter "Category=Unit"

# Integration tests only
dotnet test --filter "Category=Integration"

# All tests
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true
```

---

## Notes

- [P] tasks = different files, independent, can run in parallel
- [Story] label maps task to specific user story for traceability
- Each user story independently completable and testable
- TDD approach: Write tests first (RED), implement (GREEN), refactor
- Verify tests fail before implementing
- Commit after each logical task or group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- Per plan.md: "NEEDS CLARIFICATION: E-Mail service (SendGrid/AWS SES/Mailgun)" → Currently stub implementation sufficient for MVP

---

## Estimated Effort (Team of 1-3)

- **Phase 1 Setup**: 1-2 hours
- **Phase 2 Foundational**: 4-6 hours
- **Phase 3 US1**: 8-12 hours (TDD + comprehensive testing)
- **Phase 4 US2**: 4-6 hours (depends on US1)
- **Phase 5 US3**: 4-6 hours (depends on US1)
- **Phase 6 Polish**: 2-4 hours
- **Total**: ~23-36 hours for complete implementation

With parallel team of 3:
- Phases 1-2 together: ~6 hours
- Phases 3-5 in parallel: ~12 hours (longest path)
- Phase 6: ~4 hours
- **Total timeline**: ~20-22 hours (vs ~23-36 sequential)

