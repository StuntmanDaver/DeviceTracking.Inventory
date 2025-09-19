# Device Tracking Inventory System

[![.NET Version](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/)
[![Build Status](https://img.shields.io/badge/build-passing-green)](https://github.com/StuntmanDaver/DeviceTracking.Inventory/actions)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

## 📋 Overview

The Device Tracking Inventory System is an enterprise-grade barcode-based inventory management solution designed as a sidecar application to the existing Device Tracking platform. It provides seamless QuickBooks Desktop integration and comprehensive inventory tracking capabilities.

## 🚀 Key Features

### Core Functionality
- **Barcode Scanning**: Real-time barcode scanning with OpenCvSharp4 and ZXing.Net
- **Multi-Location Support**: Hierarchical location management with capacity tracking
- **Stock Transactions**: Receipt, Issue, Transfer, and Adjustment operations
- **Low Stock Alerts**: Automated notifications for inventory replenishment
- **Supplier Management**: Comprehensive vendor relationship management

### Advanced Features
- **ETag Concurrency Control**: Optimistic concurrency for data integrity
- **RESTful API**: Complete REST API with OpenAPI/Swagger documentation
- **Role-Based Security**: Viewer, Clerk, Manager, and Admin permission levels
- **Audit Trail**: Complete transaction history and change tracking
- **QuickBooks Integration**: Bidirectional sync with QuickBooks Desktop SDK v2.0

### Technical Excellence
- **Clean Architecture**: Layered architecture with clear separation of concerns
- **MVP Pattern**: Model-View-Presenter pattern for WinForms UI
- **Repository Pattern**: Data access abstraction with Unit of Work
- **Comprehensive Testing**: Unit tests (80%+ coverage) and integration tests
- **Performance Optimized**: Efficient database queries and caching strategies

## 🏗️ Architecture

```
DeviceTracking.Inventory/
├── 📁 Api/                          # ASP.NET Core Web API Layer
│   ├── Controllers/                 # REST API Controllers
│   ├── Middleware/                  # Custom middleware (Auth, Logging, Errors)
│   └── Program.cs                   # API configuration and startup
├── 📁 Application/                  # Business Logic Layer
│   ├── Services/                    # Business services and logic
│   ├── Common/                      # Shared utilities and exceptions
│   ├── BusinessRules/               # Domain business rules
│   ├── Validators/                  # FluentValidation rules
│   └── Mapping/                     # AutoMapper configurations
├── 📁 Infrastructure/               # Data Access Layer
│   ├── Repositories/                # Repository implementations
│   ├── Data/                        # Entity Framework context
│   ├── Migrations/                  # Database migrations
│   └── Database/                    # Database setup scripts
├── 📁 Shared/                       # Shared Models and DTOs
│   ├── Entities/                    # Domain entities
│   └── DTOs/                        # Data Transfer Objects
├── 📁 WinForms/                     # Desktop UI Layer (MVP Pattern)
│   ├── Presenters/                  # MVP Presenters
│   ├── Forms/                       # WinForms views
│   └── CameraDeviceWrapper.cs       # Camera integration
└── 📁 Tests/                        # Testing Layer
    ├── UnitTests/                   # xUnit unit tests
    └── IntegrationTests/            # API integration tests
```

## 🔧 Prerequisites

### System Requirements
- **Operating System**: Windows 10/11 (WinForms), Linux/macOS (API/Web)
- **Database**: SQL Server 2019+ or SQL Server Express
- **.NET Runtime**: .NET 9.0 SDK
- **Camera**: Webcam or barcode scanner (optional for UI features)

### Development Tools
- **IDE**: Visual Studio 2022 or VS Code
- **Database Tools**: SQL Server Management Studio (SSMS)
- **API Testing**: Postman, Swagger UI, or curl
- **Version Control**: Git

## 🚀 Quick Start

### 1. Clone and Setup
```bash
git clone https://github.com/StuntmanDaver/DeviceTracking.Inventory.git
cd DeviceTracking.Inventory
```

### 2. Database Setup
```sql
-- Run database setup scripts
sqlcmd -S localhost -i Database/Setup/01_CreateDatabase.sql
sqlcmd -S localhost -i Database/Setup/02_CreateSchema.sql
```

### 3. Build and Run
```bash
# Build the solution
dotnet build DeviceTracking.Inventory.sln

# Run the API
dotnet run --project DeviceTracking.Inventory.Api

# Run the WinForms application
dotnet run --project DeviceTracking.Inventory.WinForms

# Run tests
dotnet test DeviceTracking.Inventory.sln
```

### 4. Access the Application
- **API**: https://localhost:5001/swagger (Swagger UI)
- **WinForms**: Launches as desktop application
- **Health Check**: https://localhost:5001/health

## 📚 API Documentation

### Authentication
The API uses JWT Bearer token authentication. Include the token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

### Key Endpoints

#### Inventory Items
```http
GET    /api/v1/inventory/items              # List with pagination/filtering
GET    /api/v1/inventory/items/{id}         # Get specific item
GET    /api/v1/inventory/items/by-barcode/{barcode}  # Get by barcode
POST   /api/v1/inventory/items              # Create new item
PUT    /api/v1/inventory/items/{id}         # Update item
DELETE /api/v1/inventory/items/{id}         # Soft delete item
POST   /api/v1/inventory/items/{id}/scan    # Record barcode scan
GET    /api/v1/inventory/items/low-stock    # Get low stock alerts
PATCH  /api/v1/inventory/items/{id}/stock   # Update stock levels
```

#### Locations
```http
GET    /api/v1/locations                    # List locations
GET    /api/v1/locations/{id}               # Get specific location
GET    /api/v1/locations/by-code/{code}     # Get by code
POST   /api/v1/locations                    # Create location
PUT    /api/v1/locations/{id}               # Update location
DELETE /api/v1/locations/{id}               # Delete location
GET    /api/v1/locations/hierarchy          # Get location hierarchy
POST   /api/v1/locations/transfer           # Transfer items
```

#### Transactions
```http
GET    /api/v1/transactions                 # List transactions
GET    /api/v1/transactions/{id}            # Get specific transaction
POST   /api/v1/transactions/receipt         # Record receipt
POST   /api/v1/transactions/issue           # Record issue
POST   /api/v1/transactions/transfer        # Record transfer
POST   /api/v1/transactions/adjustment      # Record adjustment
POST   /api/v1/transactions/{id}/approve    # Approve transaction
POST   /api/v1/transactions/{id}/process    # Process transaction
GET    /api/v1/transactions/summary         # Get transaction summary
```

### Request/Response Examples

#### Create Inventory Item
```json
POST /api/v1/inventory/items
{
  "partNumber": "CPU-I7-10700K",
  "description": "Intel Core i7-10700K Processor",
  "barcode": "123456789012",
  "category": "Electronics",
  "subCategory": "Processors",
  "unitOfMeasure": "Each",
  "currentStock": 25,
  "minimumStock": 10,
  "maximumStock": 100,
  "standardCost": 350.00,
  "sellingPrice": 450.00,
  "locationId": "11111111-1111-1111-1111-111111111111",
  "supplierId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "isActive": true,
  "notes": "High-performance processor"
}
```

#### Get Inventory Items with Filtering
```http
GET /api/v1/inventory/items?page=1&pageSize=10&category=Electronics&searchTerm=CPU
```

#### Update with ETag Concurrency Control
```http
PUT /api/v1/inventory/items/aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa
If-Match: "abc123def456"
{
  "description": "Updated description",
  "minimumStock": 15
}
```

## 🧪 Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter Category=UnitTest

# Run integration tests only
dotnet test --filter Category=IntegrationTest

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

### Test Structure
```
Tests/
├── UnitTests/                    # xUnit unit tests
│   ├── Services/                # Service layer tests
│   ├── Validators/              # Validation rule tests
│   └── BusinessRules/           # Business rule tests
└── IntegrationTests/            # API integration tests
    ├── Controllers/             # Controller endpoint tests
    └── TestData/                # Test data seeding
```

### Test Coverage Goals
- **Unit Tests**: 80%+ code coverage
- **Integration Tests**: All critical API endpoints
- **Business Rules**: All validation scenarios
- **Error Handling**: All exception paths

## 🔒 Security

### Authentication & Authorization
- **JWT Tokens**: Secure token-based authentication
- **Role-Based Access**: Granular permissions system
- **Session Management**: Secure cookie handling
- **Password Policies**: PBKDF2-based password hashing

### Data Protection
- **Encryption**: AES-256 for sensitive data
- **SQL Injection Prevention**: Parameterized queries
- **XSS Protection**: Input validation and sanitization
- **CSRF Protection**: Anti-forgery tokens

### Audit & Compliance
- **Complete Audit Trail**: All data changes tracked
- **User Attribution**: All actions linked to users
- **GDPR Compliance**: Data minimization and consent
- **Retention Policies**: Configurable data retention

## 📊 Performance

### API Performance Targets
- **Response Time**: <200ms for 95% of requests
- **Throughput**: 1000+ requests per second
- **Error Rate**: <1% under normal load
- **Availability**: >99.9% uptime

### Database Optimization
- **Indexing Strategy**: Composite indexes on frequently queried columns
- **Query Optimization**: Dapper for high-performance queries
- **Connection Pooling**: Efficient SQL Server resource management
- **Batch Operations**: Bulk inserts/updates for performance

### Caching Strategy
- **Response Caching**: HTTP caching headers
- **ETag Support**: Conditional requests optimization
- **Database Caching**: Query result caching
- **Static Asset Caching**: CDN-ready asset optimization

## 🔧 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "InventoryDb": "Server=localhost;Database=DeviceTracking_Inventory;Trusted_Connection=True;",
    "DeviceTrackingDb": "Server=localhost;Database=DeviceTracking;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyHere",
    "Issuer": "DeviceTracking.Inventory",
    "Audience": "DeviceTracking.Inventory.Api",
    "ExpiryInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "BarcodeScanner": {
    "SupportedFormats": ["CODE_128", "QR_CODE", "CODE_39", "EAN_13"],
    "ScanTimeout": 30,
    "ConfidenceThreshold": 0.8
  }
}
```

### Environment Variables
```bash
# Database
CONNECTIONSTRINGS__INVENTORYDB=Server=localhost;Database=DeviceTracking_Inventory;Trusted_Connection=True;

# JWT
JWT__KEY=YourSuperSecretKeyHere
JWT__ISSUER=DeviceTracking.Inventory

# Logging
LOGGING__LOGLEVEL__DEFAULT=Information
LOGGING__LOGLEVEL__MICROSOFTASPNETCORE=Warning
```

## 🚀 Deployment

### Docker Deployment
```bash
# Build and run with Docker Compose
docker-compose up -d

# Build individual services
docker build -f docker/api.Dockerfile -t inventory-api .
docker build -f docker/web.Dockerfile -t inventory-web .
```

### IIS Deployment
```powershell
# Publish for IIS
dotnet publish DeviceTracking.Inventory.Api -c Release -o ./publish

# Create IIS website
New-Website -Name "InventoryAPI" -Port 80 -PhysicalPath "C:\inetpub\wwwroot\inventory\publish" -ApplicationPool "DefaultAppPool"
```

### Database Migration
```bash
# Apply migrations
dotnet ef database update --project DeviceTracking.Inventory.Infrastructure
```

## 📈 Monitoring & Observability

### Application Insights
- **Request Tracking**: All API requests monitored
- **Dependency Tracking**: Database and external service calls
- **Performance Metrics**: Response times and throughput
- **Error Tracking**: Exception logging and alerting

### Health Checks
```http
GET /health          # Overall health status
GET /health/ready    # Readiness probe
GET /health/live     # Liveness probe
```

### Logging
```json
{
  "RequestId": "0HMKH7QDQ2J1C:00000001",
  "Method": "GET",
  "Path": "/api/v1/inventory/items",
  "StatusCode": 200,
  "ElapsedMs": 45,
  "UserId": "user123",
  "Timestamp": "2024-01-15T10:30:00Z"
}
```

## 🤝 Contributing

### Development Workflow
1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### Code Standards
- **C# Coding Standards**: Follow Microsoft's coding guidelines
- **Naming Conventions**: PascalCase for classes, camelCase for variables
- **Documentation**: XML documentation for all public APIs
- **Testing**: 80%+ code coverage requirement
- **Commit Messages**: Clear, descriptive commit messages

### Pull Request Process
1. **Update** the README.md with details of changes
2. **Update** the version numbers in any examples files
3. **Run** the test suite and ensure all tests pass
4. **Update** the CHANGELOG.md with notable changes
5. **The PR** will be merged once you have the sign-off of maintainers

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Support

### Documentation
- **API Documentation**: https://localhost:5001/swagger
- **Architecture Docs**: See `/docs` directory
- **Troubleshooting**: Check `/docs/troubleshooting.md`

### Community
- **Issues**: [GitHub Issues](https://github.com/StuntmanDaver/DeviceTracking.Inventory/issues)
- **Discussions**: [GitHub Discussions](https://github.com/StuntmanDaver/DeviceTracking.Inventory/discussions)
- **Wiki**: [Project Wiki](https://github.com/StuntmanDaver/DeviceTracking.Inventory/wiki)

### Contact
- **Project Lead**: David Kerr
- **Email**: david.kerr@example.com
- **LinkedIn**: [David Kerr](https://linkedin.com/in/davidkerr)

## 🙏 Acknowledgments

- **Microsoft**: For .NET 9.0 and ASP.NET Core
- **Entity Framework Team**: For excellent ORM capabilities
- **OpenCV Community**: For computer vision libraries
- **ZXing Team**: For barcode processing libraries
- **Intuit**: For QuickBooks Desktop SDK
- **Open Source Community**: For countless libraries and tools

---

**Built with ❤️ using .NET 9.0, Clean Architecture, and enterprise-grade best practices.**

*Last updated: January 15, 2024*
