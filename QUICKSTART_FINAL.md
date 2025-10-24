# Duotify Membership API - Quick Start Guide

## 5-Minute Setup

### Prerequisites
- .NET 8.0 SDK
- Git

### Installation

```bash
# Clone and navigate
git clone <repo>
cd duotify-membership-v1

# Restore and build
dotnet restore
dotnet build

# Run tests
dotnet test

# Start API
dotnet run --project src/Duotify.Membership.Api
```

API will be available at `http://localhost:5000`

## Basic Usage

### 1. Register a User

```bash
curl -X POST http://localhost:5000/v1/members/register \
  -H "Content-Type: application/json" \
  -d '{
    "nationalId": "A123456789",
    "name": "John Doe",
    "email": "john@example.com",
    "password": "SecurePassword123!"
  }'
```

Response:
```json
{
  "success": true,
  "data": {
    "memberId": "550e8400-e29b-41d4-a716-446655440000",
    "email": "john@example.com",
    "message": "註冊成功！驗證碼已發送至您的電子郵件。"
  }
}
```

### 2. Verify Email

```bash
curl -X POST http://localhost:5000/v1/members/550e8400-e29b-41d4-a716-446655440000/verify \
  -H "Content-Type: application/json" \
  -d '{"code": "123456"}'
```

### 3. Login

```bash
curl -X POST http://localhost:5000/v1/members/login \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrNationalId": "john@example.com",
    "password": "SecurePassword123!"
  }'
```

### 4. Get Profile

```bash
curl http://localhost:5000/v1/members/550e8400-e29b-41d4-a716-446655440000/profile
```

## Features

✅ User Registration with National ID validation
✅ Email Verification with OTP
✅ Secure Password (BCrypt)
✅ Member Profiles
✅ Authorization (email verification required for protected resources)
✅ Rate Limiting
✅ Comprehensive Error Handling
✅ Structured Logging

## Testing

```bash
# Run all tests
dotnet test

# With verbose output
dotnet test -v normal

# Specific test class
dotnet test --filter "ClassName=MemberServiceTests"
```

## Documentation

- **README.md** - Complete API documentation
- **DEVELOPMENT.md** - Development guide and architecture
- **PROGRESS.md** - Implementation progress tracking

## Troubleshooting

**Port Already in Use**
```bash
# Change port in launchSettings.json or use environment variable
set ASPNETCORE_URLS=http://localhost:5002
```

**Build Fails**
```bash
dotnet clean
dotnet restore
dotnet build
```

**Tests Fail**
- Verify .NET 8.0 is installed: `dotnet --version`
- Check connection string in `appsettings.json`

## Next Steps

1. Review API documentation in README.md
2. Explore source code structure
3. Run tests to verify setup
4. Start building integrations

## Support

For issues, check:
1. README.md for API details
2. DEVELOPMENT.md for technical guidance
3. Source code comments and tests for examples

---

**Last Updated**: 2025-10-24
