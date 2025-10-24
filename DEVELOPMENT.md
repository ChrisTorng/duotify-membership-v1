# Development Guide

## Setup

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 / VS Code
- SQL Server (optional, InMemory DB for development)
- Git

### Local Development Setup

```bash
# Clone repository
git clone <repository-url>
cd duotify-membership-v1

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Start development server
dotnet run --project src/Duotify.Membership.Api
```

## Code Organization

### Services Layer

Business logic is separated into service classes:

- **MemberService**: Registration, login, member operations
- **VerificationCodeService**: Code generation, validation, expiration
- **AuthorizationService**: Permission checks, resource access
- **EmailService**: Email sending (extensible)

### Repository Pattern

Data access through repository interfaces:

- **IMemberRepository**: Member CRUD operations
- **IVerificationCodeRepository**: Code CRUD operations

### Middleware

Cross-cutting concerns:

- **ErrorHandlingMiddleware**: Exception handling and error responses
- **RateLimitingMiddleware**: Request rate limiting

## Key Design Patterns

### 1. Dependency Injection

All services and repositories are registered in `Program.cs`:

```csharp
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
```

### 2. Exception Handling

Exceptions propagate from services to the ErrorHandlingMiddleware:

```csharp
throw new InvalidOperationException("CODE_EXPIRED");
```

The middleware catches and translates to HTTP responses.

### 3. Validation

FluentValidation validators run automatically via middleware:

```csharp
builder.Services.AddFluentValidationAutoValidation();
```

### 4. Logging

Structured logging with Serilog context:

```csharp
_logger.LogInformation("Member registered: {MemberId}", memberId);
```

## Testing

### Unit Tests

Mock dependencies and test service logic:

```csharp
var mockRepository = new Mock<IMemberRepository>();
var service = new MemberService(mockRepository.Object, ...);

var result = await service.RegisterAsync(request);
Assert.NotNull(result);
```

### Integration Tests

Use `WebApplicationFactory` with in-memory database:

```csharp
var factory = new ApiWebApplicationFactory();
var client = factory.CreateClient();

var response = await client.PostAsync("/v1/members/register", content);
Assert.Equal(HttpStatusCode.Created, response.StatusCode);
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverageReportFormats=opencover

# Run specific test
dotnet test --filter "FullyQualifiedName~MemberServiceTests"

# Watch mode (requires xunit.runner.visualstudio)
dotnet watch test
```

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

### Environment-Specific Settings

- `appsettings.Development.json`
- `appsettings.Production.json`

## Database Migrations

### Create Migration

```bash
dotnet ef migrations add InitialCreate --project src/Duotify.Membership.Api
```

### Apply Migration

```bash
dotnet ef database update --project src/Duotify.Membership.Api
```

### Remove Latest Migration

```bash
dotnet ef migrations remove --project src/Duotify.Membership.Api
```

## Code Style

### Naming Conventions

- **Classes**: PascalCase (e.g., `MemberService`)
- **Methods**: PascalCase (e.g., `RegisterAsync`)
- **Properties**: PascalCase (e.g., `IsEmailVerified`)
- **Fields**: _camelCase (e.g., `_memberRepository`)
- **Local variables**: camelCase (e.g., `memberId`)

### Code Format

- 4-space indentation
- Use `using` statements at top
- `async/await` for all I/O operations
- Guard clauses over nested conditionals

### Documentation

Add XML comments to public APIs:

```csharp
/// <summary>
/// Registers a new member with email verification.
/// </summary>
/// <param name="request">Registration request details</param>
/// <returns>Registration response with member ID</returns>
public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
{
    // ...
}
```

## Common Tasks

### Add New API Endpoint

1. Create DTO in `Dtos/`
2. Add validator if needed in `Validators/`
3. Add method in appropriate service
4. Add controller method
5. Write tests
6. Update documentation

### Add New Validation Rule

1. Create validator class extending `AbstractValidator<T>`
2. Define rules in constructor
3. Register in `Program.cs`
4. Test with unit tests

### Add New Service

1. Create interface in `Interfaces/`
2. Implement class in `Services/`
3. Register in `Program.cs`
4. Write tests
5. Use in controllers/services

## Troubleshooting

### Build Fails

```bash
# Clean and rebuild
dotnet clean
dotnet build
```

### Tests Fail

```bash
# Ensure database is in-memory
# Check connection string in appsettings.json
# Run with verbose output
dotnet test -v normal
```

### Async Issues

- All database operations must be `async`
- Use `await` not `.Result`
- Methods calling async methods must be `async`

### Null Reference Exceptions

- Check null-forgiving operator usage
- Use `?.` operator for safe navigation
- Add null checks before operations

## Performance Considerations

### Database Queries

- Use `async/await` for I/O
- Minimize database round-trips
- Use projections to select needed fields

### Caching

- Consider caching validation rules
- Cache rate limit entries (already done)
- Consider user profile caching for verification checks

### Logging

- Log important events (registration, verification)
- Avoid logging sensitive data
- Use appropriate log levels

## Security Checklist

- [ ] No hardcoded credentials
- [ ] No plaintext passwords logged
- [ ] SQL injection prevention (EF Core)
- [ ] CORS properly configured
- [ ] HTTPS enforced
- [ ] Rate limiting enabled
- [ ] Input validation on all endpoints
- [ ] Authorization checks on protected resources

## Deployment

### Staging

```bash
# Build for deployment
dotnet publish -c Release

# Run migration
dotnet ef database update
```

### Production

- Set production connection string
- Configure email service credentials
- Set HTTPS certificates
- Enable comprehensive logging
- Set up monitoring/alerting

## Useful Commands

```bash
# View solution structure
dotnet sln list

# Format code
dotnet format

# Analyze code quality
dotnet analyze

# View package versions
dotnet package-info

# Update packages
dotnet package update
```

## Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [FluentValidation](https://fluentvalidation.net)
- [Serilog](https://serilog.net)
- [BCrypt.Net-Next](https://github.com/BcryptNet/bcrypt.net)

## Getting Help

1. Check existing issues in repository
2. Review code comments and documentation
3. Look at similar implementations
4. Ask team members
5. Create issue with detailed information

---

**Last Updated**: 2025-10-24
