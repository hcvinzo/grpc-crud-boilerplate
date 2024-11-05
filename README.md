# gRPC CRUD Boilerplate

A modern .NET 8.0 boilerplate for building production-ready gRPC services with REST API support through HTTP/JSON transcoding.

## Features

- 🔐 **Authentication & Authorization**
  - JWT-based authentication
  - Role-based access control
  - Permission-based authorization
  - Refresh token support
  - Argon2 password hashing

- 🛠 **API Features**
  - gRPC services with HTTP/JSON transcoding
  - Swagger/OpenAPI documentation
  - Both unary and streaming operations
  - Correlation ID tracking
  - Exception handling middleware
  - Input validation

- 📊 **Data Management**
  - Entity Framework Core with in-memory database
  - AutoMapper for object mapping
  - CSV import/export functionality
  - Streaming support for large datasets

- 📝 **Logging & Monitoring**
  - Structured logging with Serilog
  - Console and file logging
  - Request/response logging
  - Performance monitoring

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022, VS Code, or Rider

## Getting Started

1. Clone the repository:
```bash
git clone https://github.com/yourusername/grpc-crud-boilerplate.git
cd grpc-crud-boilerplate
```

2. Run the application:
```bash
dotnet run
```

3. Access the Swagger UI:
- HTTP: http://localhost:5000/swagger
- HTTPS: https://localhost:5001/swagger

## Project Structure

```
├── Contracts/              # DTOs and contracts
├── DataContext/           
│   ├── Entities/          # Domain entities
│   └── SeedInitialData.cs # Initial seed data
├── GrpcServices/          
│   └── v1/               # gRPC service implementations
├── Infrastructure/        
│   ├── Authorization/    # Authorization components
│   └── Exception/        # Exception handling
├── Mappings/             # AutoMapper profiles
├── Protos/               # Protocol buffer definitions
└── Services/             # Business logic services
```

## API Documentation

The API supports both gRPC and REST endpoints through HTTP/JSON transcoding. Available services:

### Auth Service
- POST `/v1/auth/authenticate` - User authentication
- POST `/v1/auth/refresh` - Refresh access token

### Order Service
- POST `/v1/orders` - Create order
- GET `/v1/orders/{id}` - Get order by ID

### Order Import Service
- POST `/v1/orders/import` - Import orders from CSV
- POST `/v1/orders/import-streaming` - Stream import orders from CSV
- GET `/v1/orders/export` - Export orders to CSV
- GET `/v1/orders/export-streaming` - Stream export orders to CSV

## Configuration

Key configuration files:

```csharp:appsettings.Development.json
startLine: 1
endLine: 14
```

## Security

The project implements several security best practices:

1. **Authentication**: JWT tokens with refresh token support
2. **Password Security**: Argon2 password hashing
3. **Authorization**: Fine-grained permission-based access control
4. **API Security**: 
   - HTTPS enforcement
   - Input validation
   - Exception handling
   - Rate limiting (configurable)

## Logging

Structured logging is implemented using Serilog with multiple sinks:

```csharp:Program.cs
startLine: 22
endLine: 28
```

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- [gRPC](https://grpc.io/)
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core)
- [Serilog](https://serilog.net/)
- [Swagger](https://swagger.io/)
