# 💰 ExpenseTracker API

A full-featured Expense Tracker REST API built with **ASP.NET Core 8** using **Onion Architecture** and **SQL Server**.

---

## 🏗️ Architecture (Onion / Clean Architecture)

```
ExpenseTracker/
├── src/
│   ├── ExpenseTracker.Domain/              ← Core (No dependencies)
│   │   ├── Entities/                       User, Expense, Category
│   │   ├── Enums/                          UserRole, ExpenseType, PaymentMethod, ...
│   │   └── Common/                         BaseEntity (Id, CreatedAt, IsDeleted, ...)
│   │
│   ├── ExpenseTracker.Application/         ← Business Logic (Depends on Domain only)
│   │   ├── DTOs/                           Auth, Expense, Category, User, Admin
│   │   ├── Interfaces/
│   │   │   ├── Repositories/               IGenericRepository<T>, IUserRepository, ...
│   │   │   └── Services/                   IAuthService, IExpenseService, ...
│   │   ├── Services/                       AuthService, ExpenseService, CategoryService, ...
│   │   ├── Mappings/                       AutoMapper MappingProfile
│   │   └── Common/                         Result<T>, PagedResult<T>, PaginationParams
│   │
│   ├── ExpenseTracker.Infrastructure/      ← Data Access (Depends on Application)
│   │   ├── Data/                           AppDbContext, Configurations, DbSeeder
│   │   ├── Repositories/                   GenericRepository<T>, UserRepository, ...
│   │   ├── UnitOfWork/                     UnitOfWork
│   │   └── Services/                       TokenService, EmailService, EmailTemplates
│   │
│   └── ExpenseTracker.API/                 ← Presentation (Depends on Application + Infrastructure)
│       ├── Controllers/                    Auth, User, Expense, Category, Admin
│       ├── Middleware/                     ExceptionMiddleware, RequestLoggingMiddleware
│       └── Extensions/                    JWT, Swagger, CORS service extensions
└── ExpenseTracker.sln
```

---

## 📦 NuGet Packages

### ExpenseTracker.Application
| Package | Purpose |
|---------|---------|
| `AutoMapper` | Object-to-object mapping |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | DI abstractions |

### ExpenseTracker.Infrastructure
| Package | Purpose |
|---------|---------|
| `Microsoft.EntityFrameworkCore` | ORM core |
| `Microsoft.EntityFrameworkCore.SqlServer` | SQL Server provider |
| `Microsoft.EntityFrameworkCore.Tools` | Migrations CLI |
| `BCrypt.Net-Next` | Password hashing |
| `MailKit` | SMTP email sending |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT middleware |
| `System.IdentityModel.Tokens.Jwt` | JWT token generation |
| `AutoMapper` | Object mapping |

### ExpenseTracker.API
| Package | Purpose |
|---------|---------|
| `Swashbuckle.AspNetCore` | Swagger/OpenAPI docs |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT auth |
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | AutoMapper DI |

---

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full)
- SMTP credentials (Gmail, Outlook, etc.)

### 1. Clone & Configure

```bash
cd src/ExpenseTracker.API
```

Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ExpenseTrackerDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "SecretKey": "YourSuperSecretKey_MustBe32CharsMinimum!",
    "Issuer": "ExpenseTrackerAPI",
    "Audience": "ExpenseTrackerClient",
    "ExpiryMinutes": "60"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SenderName": "ExpenseTracker",
    "SenderEmail": "noreply@expensetracker.com",
    "Username": "your@gmail.com",
    "Password": "your-app-password"
  }
}
```

### 2. Apply Migrations

```bash
# From solution root
dotnet ef migrations add InitialCreate --project src/ExpenseTracker.Infrastructure --startup-project src/ExpenseTracker.API

dotnet ef database update --project src/ExpenseTracker.Infrastructure --startup-project src/ExpenseTracker.API
```

### 3. Run

```bash
dotnet run --project src/ExpenseTracker.API
```

Open Swagger: `https://localhost:7001`

---

## 🔐 Default Admin Credentials

After seeding, use these credentials to login as Admin:

| Field | Value |
|-------|-------|
| Email | `admin@expensetracker.com` |
| Password | `Admin@123456` |

---

## 📡 API Endpoints

### Auth
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register new user | ❌ |
| POST | `/api/auth/login` | Login | ❌ |
| POST | `/api/auth/refresh-token` | Refresh JWT | ❌ |
| POST | `/api/auth/forgot-password` | Send reset email | ❌ |
| POST | `/api/auth/reset-password` | Reset password | ❌ |
| POST | `/api/auth/change-password` | Change password | ✅ |
| POST | `/api/auth/verify-email` | Verify email | ❌ |
| GET  | `/api/auth/verify-email` | Verify email (link) | ❌ |
| POST | `/api/auth/resend-verification` | Resend verify email | ❌ |
| POST | `/api/auth/logout` | Logout | ✅ |

### User
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/user/profile` | Get profile | ✅ |
| PUT | `/api/user/profile` | Update profile | ✅ |
| POST | `/api/user/profile/picture` | Upload avatar | ✅ |
| DELETE | `/api/user/account` | Delete account | ✅ |

### Expense
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/expense` | Get expenses (paginated + filtered) | ✅ |
| GET | `/api/expense/{id}` | Get single expense | ✅ |
| POST | `/api/expense` | Create expense | ✅ |
| PUT | `/api/expense/{id}` | Update expense | ✅ |
| DELETE | `/api/expense/{id}` | Delete expense | ✅ |
| GET | `/api/expense/summary` | Stats & charts data | ✅ |
| POST | `/api/expense/{id}/receipt` | Upload receipt | ✅ |

### Category
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/category` | Get all categories | ✅ |
| GET | `/api/category/{id}` | Get single category | ✅ |
| POST | `/api/category` | Create category | ✅ |
| PUT | `/api/category/{id}` | Update category | ✅ |
| DELETE | `/api/category/{id}` | Delete category | ✅ |

### Admin (Role: Admin required)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/admin/dashboard` | Dashboard stats |
| GET | `/api/admin/users` | All users |
| GET | `/api/admin/users/{id}` | Single user |
| PATCH | `/api/admin/users/{id}/role` | Change role |
| PATCH | `/api/admin/users/{id}/toggle-active` | Activate/deactivate |
| DELETE | `/api/admin/users/{id}` | Delete user |
| GET | `/api/admin/expenses` | All expenses |

---

## 🧩 Key Design Patterns

- **Onion Architecture**: Domain → Application → Infrastructure → API
- **Generic Repository Pattern**: `IGenericRepository<T>` for all CRUD ops
- **Unit of Work Pattern**: Single transaction scope with `IUnitOfWork`
- **Result Pattern**: `Result<T>` / `Result` wrapper — no raw exceptions to controllers
- **Soft Delete**: All entities use `IsDeleted` + EF global query filters
- **AutoMapper**: All DTO mappings centralized in `MappingProfile`
- **JWT + Refresh Tokens**: Secure stateless auth with refresh token rotation
- **SMTP via MailKit**: Welcome, verify, reset, password-changed emails

---

## 📧 Gmail SMTP Setup

1. Enable 2-Step Verification on your Google account
2. Go to: Google Account → Security → App Passwords
3. Create a new App Password (select "Mail")
4. Use that 16-character password in `appsettings.json`

---

## 🗃️ Database Schema

```
Users           → Id, FirstName, LastName, Email, PasswordHash, Role, IsEmailVerified, ...
Categories      → Id, Name, Icon, Color, IsDefault, UserId (FK)
Expenses        → Id, Title, Amount, Date, Type, PaymentMethod, CategoryId (FK), UserId (FK), ...
```

All tables include: `CreatedAt`, `UpdatedAt`, `IsDeleted`, `DeletedAt`
