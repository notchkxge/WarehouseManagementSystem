# Warehouse Management System - Backend API

A backend system for warehouse management built with C# .NET and MySQL, featuring role-based access control and comprehensive inventory management.

## Overview

This backend API implements a complete warehouse management system with:
- RESTful API with proper HTTP conventions
- Role-based user permissions (Storekeeper, Director)
- Inventory tracking and management
- Goods receipt and issuance workflows
- Storage location management with capacity constraints
- Comprehensive reporting capabilities

## My Stack
- **Backend**: C# .NET (ASP.NET Core Web API)
- **Database**: MySQL with Entity Framework Core
- **Architecture**: MVC (Model-View-Controller) for API structure
- **ORM**: Entity Framework Core for data access
- **Containerization**: Docker for database
- **Postman** : Api calls test

## What I Learned
During this internship project, I gained experience in:
- Complex relationships between classes and entities
- API request handling and response management
- Business logic implementation and validation (**Learned how to join tables**)
- Building RESTful APIs with proper endpoints and HTTP methods
- MVC architecture for backend systems
- Database design and Entity Framework Core

## API Endpoints

### Authentication & Setup
- `POST /api/employees` - Create an employee (Director/Storekeeper)
- `POST /api/roles/login` - User authentication
- `GET /api/employees` - Get all employees

### Products & Storage Management
- `POST /api/products` - Create a product
- `POST /api/warehouses` - Create a warehouse
- `POST /api/storagelocations` - Create a storage location

### Goods Receipt Workflow
- `POST /api/goodsreceipt` - Create a goods receipt document
- `POST /api/goodsreceipt/1/lines` - Add product lines to receipt
- `PUT /api/goodsreceipt/3/assign-locations` - Change status from New to Layout
- `PUT /api/goodsreceipt/3/close` - Change status from Layout to Closed

### Goods Issue Workflow
- `POST /api/goodsissue` - Create a goods issue document
- `POST /api/goodsissue/4/lines` - Add item lines
- `PUT /api/goodsissue/4/issue` - Change status from New to Issued
- `PUT /api/goodsissue/4/close` - Change status from Issued to Closed

### Reporting & Inventory
- `GET /api/inventory/report/csv` - Product balances report
- `GET /api/storageLocations/report/capacity/csv` - Warehouse load report
- `GET /api/inventory/report/status/csv` - Stock status report
- `GET /api/goodsreceipt/report/csv` - Acceptance history report
- `GET /api/goodsissue/report/csv` - Issuance history report
- `GET /api/inventorydocument/report/csv` - Inventory document report

### Personnal Notes:
- Try to never mix llm code with human code. Because I couldn;t understand a lot, So wasted time on debugging alot !!
- Had fun doing it !

### Screenshots on how it works
[MyRootApi.docx](https://github.com/user-attachments/files/23196256/MyRootApi.docx)

### Report Generated with API
1. [Отчет_по_приемкам_20251021_005528.xlsx](https://github.com/user-attachments/files/23315033/_._._20251021_005528.xlsx)

2. [Отчет_по_остаткам_20251020_221526.xlsx](https://github.com/user-attachments/files/23315035/_._._20251020_221526.xlsx)

3. [Статус_запасов_20251021_005448.xlsx](https://github.com/user-attachments/files/23315036/_._20251021_005448.xlsx)

4. [Отчет_по_выдаче_20251021_005649.xlsx](https://github.com/user-attachments/files/23315034/_._._20251021_005649.xlsx)

5. [Загрузка_склада_20251020_221614.xlsx](https://github.com/user-attachments/files/23315037/_._20251020_221614.xlsx)

6. [Инвентаризация_20251021_005733.xlsx](https://github.com/user-attachments/files/23315038/_20251021_005733.xlsx)

