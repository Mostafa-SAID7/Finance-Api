# Architecture Overview

## System Design

FinanceControl follows a layered architecture pattern with clear separation of concerns:

```
┌─────────────────────────────────────┐
│      Controllers (API Layer)         │
│  BalanceController                  │
│  ExpensesController                 │
│  IncomesController                  │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Application Layer                 │
│  Business Logic & Use Cases         │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Domain Layer                      │
│  Entities & Business Rules          │
│  Expenses, Incomes                  │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Infrastructure Layer              │
│  Database (PostgreSQL)              │
│  Message Broker (Kafka)             │
│  Entity Framework Core              │
└─────────────────────────────────────┘
```

## Key Components

### Controllers
- **BalanceController**: Manages balance queries
- **ExpensesController**: Handles expense operations
- **IncomesController**: Handles income operations

### Domain Models
- **Expense**: Represents a financial expense
- **Income**: Represents a financial income
- **Balance**: Calculated from incomes and expenses

### Infrastructure
- **FinanceControlDbContext**: Entity Framework Core DbContext
- **BrokerService**: Kafka event publishing
- **MigrateDatabaseHostedService**: Automatic database migration on startup

### Data Access
- **Entity Framework Core**: ORM for database operations
- **PostgreSQL**: Primary data store
- **Migrations**: Database schema versioning

### Event Publishing
- **Kafka**: Event streaming for transaction events
- **BrokerService**: Handles event publishing to Kafka topics

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 9 |
| Web Framework | ASP.NET Core |
| ORM | Entity Framework Core 9.0 |
| Database | PostgreSQL |
| Message Broker | Kafka |
| Testing | xUnit, Testcontainers |
| API Documentation | Swagger/OpenAPI |

## Data Flow

1. **Request** → Controller receives HTTP request
2. **Processing** → Application layer processes business logic
3. **Persistence** → Domain entities saved to PostgreSQL via EF Core
4. **Events** → Transaction events published to Kafka
5. **Response** → Controller returns HTTP response

## Database Schema

The application uses Entity Framework Core migrations to manage the database schema. Migrations are automatically applied on application startup.

Key tables:
- `Expenses`: Stores expense records
- `Incomes`: Stores income records

## Event-Driven Architecture

The application publishes events to Kafka for:
- Expense creation
- Income creation
- Balance updates

This enables:
- Audit logging
- Real-time notifications
- Integration with other systems
- Event sourcing capabilities

## Scalability Considerations

- **Horizontal Scaling**: Stateless API design allows multiple instances
- **Database**: PostgreSQL connection pooling configured
- **Message Queue**: Kafka enables asynchronous processing
- **Caching**: Can be added for frequently accessed data
