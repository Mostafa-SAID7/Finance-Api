# API Documentation

## Overview

FinanceControl API provides endpoints for managing financial transactions including incomes, expenses, and balance tracking.

## Base URL

```
https://localhost:5001/api
```

## Authentication

Currently, the API does not require authentication. This should be implemented for production use.

## Endpoints

### Balance

#### Get Balance
- **Endpoint**: `GET /api/balance`
- **Description**: Retrieve the current financial balance
- **Response**: 
  ```json
  {
    "balance": 1000.00
  }
  ```

### Expenses

#### Create Expense
- **Endpoint**: `POST /api/expenses`
- **Description**: Record a new expense
- **Request Body**:
  ```json
  {
    "description": "Grocery shopping",
    "amount": 50.00,
    "date": "2026-04-21"
  }
  ```
- **Response**: `201 Created`

### Incomes

#### Create Income
- **Endpoint**: `POST /api/incomes`
- **Description**: Record a new income
- **Request Body**:
  ```json
  {
    "description": "Salary",
    "amount": 3000.00,
    "date": "2026-04-21"
  }
  ```
- **Response**: `201 Created`

## Error Handling

The API returns standard HTTP status codes:
- `200 OK` - Successful request
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid request data
- `500 Internal Server Error` - Server error

## Testing

Use Swagger UI to test endpoints interactively:
```
https://localhost:5001/swagger
```

## Rate Limiting

Currently not implemented. Consider adding for production.

## Versioning

API versioning is not currently implemented. Consider implementing for future compatibility.
