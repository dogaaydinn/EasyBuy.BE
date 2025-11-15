# EasyBuy.BE - Enterprise E-Commerce Platform

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-Latest-DC382D)](https://redis.io/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Status](https://img.shields.io/badge/Status-In_Development-yellow)](https://github.com/dogaaydinn/EasyBuy.BE)

EasyBuy.BE is an enterprise-grade e-commerce platform built with .NET 8.0, implementing Clean Architecture, CQRS, and Domain-Driven Design principles.

> **⚠️ Current Status**: **42% Complete** - Active Development (NOT Production Ready)
>
> See [PRODUCTION_READINESS.md](PRODUCTION_READINESS.md) for detailed status.

---

## 🏗️ Architecture

This project implements **Clean Architecture** (Onion Architecture) with the following layers:

```
┌─────────────────────────────────────┐
│    Presentation (WebAPI)            │  ← Controllers, Middleware
├─────────────────────────────────────┤
│    Application Layer                │  ← CQRS, DTOs, Validators
├─────────────────────────────────────┤
│    Domain Layer                     │  ← Entities, Events, Business Logic
├─────────────────────────────────────┤
│    Infrastructure + Persistence     │  ← EF Core, External Services
└─────────────────────────────────────┘
```

**Key Patterns**:
- **CQRS** with MediatR for command/query separation
- **Domain-Driven Design** with aggregates and domain events
- **Repository Pattern** with generic base repositories
- **Event-Driven Architecture** with domain and integration events
- **Multi-level Caching** (L1 in-memory + L2 Redis)

---

## ✨ Features

### ✅ Implemented
- **Clean Architecture** with 5 projects
- **CQRS Pattern** with MediatR
- **Products API** (Complete CRUD)
- **Authentication API** (Partial - JWT configured)
- **Multi-level Caching** (L1 Memory + L2 Redis)
- **Domain Events** with 6 event handlers
- **Enterprise Middleware** (Exception handling, Security headers, Correlation IDs)
- **Rate Limiting** (IP-based)
- **API Versioning** (URL + Header-based)
- **Swagger/OpenAPI** Documentation
- **Health Checks** (Database + Redis)
- **Structured Logging** (Serilog with Console, File, Seq)
- **Background Jobs** (Hangfire - configured)
- **Message Bus** (MassTransit + RabbitMQ - configured)
- **Secrets Management** (Azure Key Vault + User Secrets)
- **Response Compression** (Brotli, Gzip)

### 🚧 In Progress
- Orders Management (CRUD pending)
- Shopping Basket (CRUD pending)
- Payment Processing (Service ready, endpoints pending)
- Reviews & Ratings (Entities ready, endpoints pending)

### 📋 Planned
- GraphQL API (HotChocolate)
- SignalR for Real-time features
- Elasticsearch for Advanced search
- Comprehensive Testing (Unit, Integration, E2E)

---

## 🛠️ Technology Stack

| Category | Technology |
|----------|------------|
| **Framework** | .NET 8.0, C# 12 |
| **Database** | PostgreSQL 16 + EF Core 9.0 |
| **Caching** | Redis (StackExchange.Redis) + In-Memory |
| **CQRS** | MediatR 12.4 |
| **Validation** | FluentValidation 11.11 |
| **Mapping** | AutoMapper 13.0 |
| **Logging** | Serilog (Console, File, Seq) |
| **Auth** | ASP.NET Core Identity + JWT |
| **API Docs** | Swagger/OpenAPI (Swashbuckle) |
| **Background Jobs** | Hangfire 1.8 + PostgreSQL |
| **Messaging** | MassTransit 8.2 + RabbitMQ |
| **Email** | SendGrid |
| **SMS** | Twilio |
| **Payments** | Stripe.net |
| **Storage** | Azure Blob Storage + Local |
| **Rate Limiting** | AspNetCoreRateLimit |
| **Telemetry** | OpenTelemetry 1.9 |
| **Testing** | xUnit, FluentAssertions, Moq, Testcontainers |

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 16](https://www.postgresql.org/download/)
- [Redis](https://redis.io/download) (optional - will fallback to in-memory)
- [Docker](https://www.docker.com/) (optional - for containerized setup)

### Option 1: Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/dogaaydinn/EasyBuy.BE.git
cd EasyBuy.BE

# Start all services (PostgreSQL, Redis, RabbitMQ, Seq, API)
docker-compose up -d

# View logs
docker-compose logs -f api

# Access the application
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
# Hangfire: http://localhost:5000/hangfire
# Seq Logs: http://localhost:5341
```

### Option 2: Local Development

```bash
# 1. Configure database connection
# Update appsettings.json or use User Secrets (recommended)
# See docs/SECRETS_SETUP.md for detailed instructions

# 2. Restore dependencies
dotnet restore

# 3. Apply database migrations
dotnet ef database update --project Infrastructure/EasyBuy.Persistence --startup-project Presentation/EasyBuy.WebAPI

# 4. Run the application
dotnet run --project Presentation/EasyBuy.WebAPI

# Access: https://localhost:5001 or http://localhost:5000
```

### Database Migrations

```bash
# Create a new migration
dotnet ef migrations add <MigrationName> \
  --project Infrastructure/EasyBuy.Persistence \
  --startup-project Presentation/EasyBuy.WebAPI

# Update database
dotnet ef database update \
  --project Infrastructure/EasyBuy.Persistence \
  --startup-project Presentation/EasyBuy.WebAPI
```

See [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md) for more details.

---

## 📚 Documentation

- **[PRODUCTION_READINESS.md](PRODUCTION_READINESS.md)** - Detailed production readiness assessment ⭐
- **[MIGRATION_GUIDE.md](MIGRATION_GUIDE.md)** - Database migration instructions
- **[docs/SECRETS_SETUP.md](docs/SECRETS_SETUP.md)** - Configuration and secrets management
- **[Swagger UI](http://localhost:5000/swagger)** - Interactive API documentation (when running)

---

## 🔧 Configuration

### Environment Variables

The application uses the following configuration sources (in order of precedence):

1. **Azure Key Vault** (Production)
2. **User Secrets** (Development)
3. **Environment Variables**
4. **appsettings.json**

### Key Settings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=EasyBuyDB;Username=postgres;Password=...",
    "RedisConnection": "localhost:6379",
    "HangfireConnection": "Host=localhost;Database=EasyBuyHangfireDB;..."
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "EasyBuy.API",
    "Audience": "EasyBuy.Client",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "FeatureFlags": {
    "EnableSwagger": true,
    "EnableHangfire": true,
    "EnableDistributedCache": true,
    "EnableRateLimiting": true,
    "EnableRabbitMQ": false
  }
}
```

**⚠️ NEVER commit secrets to source control!** Use User Secrets or Azure Key Vault.

See [docs/SECRETS_SETUP.md](docs/SECRETS_SETUP.md) for detailed setup.

---

## 📊 API Endpoints

### Products (✅ Complete)
- `GET /api/v1/products` - List products (with pagination, filtering, sorting)
- `GET /api/v1/products/{id}` - Get product by ID
- `POST /api/v1/products` - Create product
- `PUT /api/v1/products/{id}` - Update product
- `DELETE /api/v1/products/{id}` - Delete product

### Authentication (⚠️ Partial)
- `POST /api/v1/auth/register` - Register new user
- `POST /api/v1/auth/login` - Login
- `POST /api/v1/auth/refresh-token` - Refresh access token

### Orders (❌ Coming Soon)
- Create order
- Update order status
- Cancel order
- List user orders

### Baskets (❌ Coming Soon)
- Add item to basket
- Update item quantity
- Remove item
- Clear basket

See [Swagger UI](http://localhost:5000/swagger) for complete API documentation.

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReporter=html

# Run specific test project
dotnet test Tests/EasyBuy.Application.UnitTests
```

**⚠️ Note**: Test implementation is pending (0% coverage currently).

---

## 🔒 Security

### Implemented
- ✅ Secrets management (Azure Key Vault + User Secrets)
- ✅ Security headers (X-Frame-Options, CSP, HSTS, etc.)
- ✅ CORS configuration
- ✅ Rate limiting
- ✅ Input validation framework
- ✅ SQL injection prevention (EF Core parameterization)

### Pending
- ⚠️ JWT authentication (configured but not enforced)
- ⚠️ Authorization policies
- ❌ Two-factor authentication
- ❌ OWASP Top 10 audit
- ❌ Penetration testing

---

## 📈 Performance

### Current Optimizations
- ✅ Multi-level caching (L1 + L2)
- ✅ Response compression
- ✅ Async/await throughout
- ✅ Connection pooling
- ✅ Distributed caching

### Pending
- ❌ Database indexes
- ❌ Query optimization
- ❌ Load testing
- ❌ Performance baselines

---

## 🏃 Development Workflow

### Adding a New Feature

1. **Domain Layer**: Create/update entities, events, value objects
2. **Application Layer**: Create CQRS commands/queries, handlers, DTOs, validators
3. **Infrastructure**: Implement external service integrations if needed
4. **Presentation**: Create controller endpoints
5. **Tests**: Write unit and integration tests
6. **Documentation**: Update Swagger comments

### Code Style

This project uses `.editorconfig` for consistent code formatting. Ensure your IDE supports EditorConfig.

### Branch Strategy

- `main` - Production-ready code
- `develop` - Integration branch
- `feature/*` - New features
- `bugfix/*` - Bug fixes
- `hotfix/*` - Production hotfixes

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Write tests for your changes
5. Ensure all tests pass (`dotnet test`)
6. Commit with conventional commits (`git commit -m 'feat: add amazing feature'`)
7. Push to your branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

### Commit Convention

Use [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation only
- `refactor:` - Code refactoring
- `test:` - Adding tests
- `chore:` - Maintenance tasks

---

## 📦 Project Structure

```
EasyBuy.BE/
├── Core/
│   ├── EasyBuy.Domain/              # Domain entities, events, value objects
│   └── EasyBuy.Application/         # CQRS, DTOs, validators, interfaces
├── Infrastructure/
│   ├── EasyBuy.Infrastructure/      # External services (Email, SMS, Payment, etc.)
│   └── EasyBuy.Persistence/         # EF Core, repositories, database config
├── Presentation/
│   └── EasyBuy.WebAPI/             # Controllers, middleware, configuration
├── Tests/
│   ├── EasyBuy.Domain.UnitTests/
│   ├── EasyBuy.Application.UnitTests/
│   ├── EasyBuy.IntegrationTests/
│   └── EasyBuy.ArchitectureTests/
└── docs/                            # Additional documentation
```

---

## 🔗 Useful Links

- **API Documentation**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health
- **Hangfire Dashboard**: http://localhost:5000/hangfire
- **Seq Logs**: http://localhost:5341
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Clean Architecture by Robert C. Martin
- Domain-Driven Design by Eric Evans
- CQRS pattern inspiration from Greg Young
- .NET community for excellent libraries

---

## 📞 Support

For issues and questions:
- **GitHub Issues**: [Create an issue](https://github.com/dogaaydinn/EasyBuy.BE/issues)
- **Email**: support@easybuy.com (placeholder)

---

**Last Updated**: 2025-11-15
**Status**: Active Development (42% Complete)
**Next Milestone**: Orders & Baskets CRUD Implementation

---

**Made with ❤️ using .NET 8.0**
