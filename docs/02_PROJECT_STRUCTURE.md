# Project Structure

See [00_PROJECT_OVERVIEW.md](./00_PROJECT_OVERVIEW.md) for the high-level solution layout.

## Per-Project Structure

### Domain
```
Vargshala.Domain/
├── Common/        # BaseEntity, shared abstractions
├── Entities/      # Organization, User, Student, etc.
├── Enums/         # Role, etc.
├── ValueObjects/  # (future)
├── Exceptions/    # DomainException
└── Constants/     # (future)
```

### Application
```
Vargshala.Application/
├── Abstractions/     # ITokenService, IPasswordHasher, IVargshalaDbContext, ICurrentUser
├── Features/         # Grouped by feature: Authentication, Users, etc.
│   └── FeatureName/
│       ├── Commands/
│       ├── Queries/
│       ├── Validators/
│       └── Mappings/
├── Behaviors/        # MediatR pipeline behaviors
├── DependencyInjection/
└── Common/
```

### Infrastructure
```
Vargshala.Infrastructure/
├── Persistence/         # DbContext, Configurations, Migrations, Repositories
├── Authentication/      # JWT, BCrypt, CurrentUser
├── DependencyInjection/
└── (future: Files, Notifications, BackgroundJobs)
```

### API
```
Vargshala.API/
├── Controllers/
├── Middleware/
├── Extensions/
├── Configuration/
└── Program.cs
```

### Contracts
```
Vargshala.Contracts/
├── Authentication/   # LoginRequest, LoginResponse, etc.
├── Organizations/    # OrganizationDto
├── Users/           # UserDto, CreateUserRequest, etc.
├── Common/          # ApiResponse, PagedResponse
└── Pagination/
```
