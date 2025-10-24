# Duotify Membership API

A secure, scalable ASP.NET Core 8.0 Web API for managing user registration, email verification, and member profiles in the Duotify platform.

## Features

- **User Registration**: Secure member registration with Taiwanese National ID validation
- **Email Verification**: Two-step verification process with time-based expiration
- **Password Security**: BCrypt hashing with configurable work factor
- **Authorization**: Role-based access control with email verification checks
- **Rate Limiting**: Protection against abuse with configurable rate limits
- **Structured Logging**: Comprehensive logging with Serilog
- **Validation**: FluentValidation with custom validators
- **Error Handling**: Unified error handling with meaningful error codes

## Quick Start

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server (for production) or in-memory database (for testing)

### Installation

```bash
# Clone the repository
git clone <repository-url>
cd duotify-membership-v1

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project src/Duotify.Membership.Api
```

The API will start at `http://localhost:5000` (HTTP) and `https://localhost:5001` (HTTPS).

## API Endpoints

### Registration

#### POST /v1/members/register
Register a new member.

**Request:**
```json
{
  "nationalId": "A123456789",
  "name": "John Doe",
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```

**Response (201 Created):**
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

### Verification

#### POST /v1/members/{memberId}/verify
Verify member email with verification code.

**Request:**
```json
{
  "code": "123456"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "message": "E-Mail 驗證成功！您的帳號已完全啟用。",
    "isEmailVerified": true
  }
}
```

#### POST /v1/members/{memberId}/verification-code/resend
Resend verification code.

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "message": "驗證碼已重新發送至您的電子郵件。",
    "email": "john@example.com"
  }
}
```

### Login

#### POST /v1/members/login
Authenticate member.

**Request:**
```json
{
  "emailOrNationalId": "john@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "memberId": "550e8400-e29b-41d4-a716-446655440000",
    "email": "john@example.com",
    "name": "John Doe",
    "isEmailVerified": true
  }
}
```

### Profile Management

#### GET /v1/members/{memberId}/profile
Get member profile (requires email verification).

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "memberId": "550e8400-e29b-41d4-a716-446655440000",
    "name": "John Doe",
    "email": "john@example.com",
    "nationalId": "A123456789",
    "isEmailVerified": true,
    "createdAt": "2025-10-24T02:51:34Z"
  }
}
```

#### PUT /v1/members/{memberId}/profile
Update member profile (requires email verification).

**Request:**
```json
{
  "name": "Jane Doe"
}
```

#### GET /v1/members/{memberId}/verification-status
Get verification status.

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "memberId": "550e8400-e29b-41d4-a716-446655440000",
    "isEmailVerified": true,
    "canAccessProtectedResources": true
  }
}
```

## Error Handling

The API returns standardized error responses:

```json
{
  "success": false,
  "error": {
    "code": "CODE_EXPIRED",
    "message": "驗證碼已過期，請重新發送"
  }
}
```

### Common Error Codes

- `NATIONAL_ID_ALREADY_EXISTS` (409): Duplicate National ID
- `INVALID_CREDENTIALS` (401): Invalid email/password
- `CODE_EXPIRED` (400): Verification code expired
- `INVALID_CODE` (400): Incorrect verification code
- `TOO_MANY_ATTEMPTS` (400): Too many failed verification attempts
- `MEMBER_NOT_FOUND` (404): Member does not exist
- `ALREADY_VERIFIED` (400): Member already verified

## Validation Rules

### National ID
- Format: 1 letter + 9 digits (e.g., A123456789)
- Includes checksum validation
- Case-insensitive

### Password
- Minimum 12 characters
- Must contain uppercase, lowercase, digit, and special character

### Email
- Valid RFC 5322 email format
- Must be unique

## Database

### Configuration

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=DuotifyMembership;Trusted_Connection=true;"
  }
}
```

For in-memory database (testing):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "InMemory"
  }
}
```

### Schema

**Members Table**
- Id (GUID, PK)
- NationalId (string, unique)
- Name (string)
- Email (string, unique)
- PasswordHash (string)
- IsEmailVerified (boolean)
- CreatedAt (datetime)
- UpdatedAt (datetime)

**VerificationCodes Table**
- Id (GUID, PK)
- MemberId (GUID, FK)
- Code (string)
- CreatedAt (datetime)
- ExpiresAt (datetime)
- IsUsed (boolean)
- FailedAttempts (int)

## Testing

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test -v normal

# Run specific test class
dotnet test --filter "ClassName=MemberServiceTests"

# Generate coverage report
dotnet test /p:CollectCoverageReportFormats=opencover
```

## Rate Limiting

- **General requests**: 60 per minute per IP
- **Verification resend**: 3 per minute per member

## Security

- **Password Hashing**: BCrypt with work factor 12
- **Email Verification**: Required for protected resources
- **SQL Injection**: Prevented through EF Core parameterized queries
- **CORS**: Configurable policy for cross-origin requests
- **HTTPS**: Enforced in production

## Logging

Logs are written to:
- Console (development and production)
- Rolling file (daily rotation)

Log level: Information (configurable in Serilog settings)

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "InMemory"
  },
  "Serilog": {
    "MinimumLevel": "Information"
  }
}
```

## Architecture

- **Controllers**: HTTP endpoint handlers
- **Services**: Business logic
- **Repositories**: Data access layer
- **Models**: Domain entities
- **DTOs**: Data transfer objects
- **Validators**: FluentValidation rules
- **Middleware**: Cross-cutting concerns

## Project Structure

```
src/Duotify.Membership.Api/
├── Controllers/
│   ├── MembersController.cs
│   └── ProfileController.cs
├── Services/
│   ├── MemberService.cs
│   ├── VerificationCodeService.cs
│   ├── EmailService.cs
│   └── AuthorizationService.cs
├── Repositories/
│   ├── MemberRepository.cs
│   └── VerificationCodeRepository.cs
├── Models/
│   ├── Member.cs
│   └── VerificationCode.cs
├── Dtos/
│   ├── RegisterRequest.cs
│   ├── LoginRequest.cs
│   └── ... (other DTOs)
├── Validators/
│   ├── RegisterRequestValidator.cs
│   ├── TaiwaneseNationalIdValidator.cs
│   └── VerifyCodeRequestValidator.cs
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs
│   └── RateLimitingMiddleware.cs
└── Program.cs

tests/Duotify.Membership.Api.Tests/
├── Models/
├── Services/
├── Validators/
└── Integration/
```

## Development Workflow

1. Create feature branch: `git checkout -b feature/my-feature`
2. Make changes following code style guidelines
3. Write/update tests
4. Run tests locally: `dotnet test`
5. Build: `dotnet build`
6. Commit and push
7. Create pull request

## Troubleshooting

### Build Issues

- **"The current .NET SDK does not support targeting .NET 9.0"**
  - Install .NET 8.0 SDK or update to appropriate version

- **"Cannot resolve package"**
  - Run `dotnet restore`
  - Clear NuGet cache if necessary

### Runtime Issues

- **"Connection timeout"**
  - Verify database connection string
  - Check database server is running

- **"Port already in use"**
  - Change port in `launchSettings.json`
  - Or kill process on port 5000/5001

## Contributing

1. Fork the repository
2. Create feature branch
3. Make changes
4. Ensure tests pass
5. Create pull request

## License

[Add License Information]

## Support

For issues, questions, or suggestions, please create an issue in the repository.

## Changelog

### v1.0.0 (2025-10-24)

- Initial release
- User registration with National ID validation
- Email verification with OTP
- Member profile management
- Authorization with email verification checks
- Rate limiting
- Comprehensive error handling
- Structured logging

---

**Last Updated**: 2025-10-24
