# 💊 PharmaSphere

PharmaSphere is a pharmacy management system built with ASP.NET Core MVC and C#.

The system is designed to help pharmacies manage medicines, inventory, suppliers, purchases, sales, prescriptions, returns, and reports through a centralized platform.

## 🚀 Features

* 💊 Medicine Management
* 📦 Medicine Batch Management
* 🔄 FEFO Inventory Management
* 🏷️ Categories Management
* 🏢 Supplier Management
* 🛒 Point of Sale (POS)
* 🧾 Orders Management
* 📋 Prescription Management
* 🔄 Sales Returns
* 💰 Payment Transactions
* 📊 Profit Reports
* 🛍️ Online Store
* 🔍 Medicine Substitutes
* ⚠️ Drug Interaction Management
* 🔒 Stock Reservations
* 📡 Real-time System Monitoring with SignalR
* 👤 User and Role Management

## 🛠️ Technologies

* C#
* ASP.NET Core MVC
* Entity Framework Core
* SQL Server
* ASP.NET Core Identity
* SignalR
* LINQ
* Repository Pattern
* Unit of Work Pattern
* Dependency Injection
* Bootstrap
* JavaScript
* Razor Views

## 🏗️ Architecture

The project follows a layered architecture:

```text
PharmaSphere
│
├── PharmaSphere.Domain
│   └── Entities and domain models
│
├── PharmaSphere.Application
│   └── Application logic and interfaces
│
├── PharmaSphere.Infrastructure
│   └── EF Core, Identity and data access
│
└── PharmaSphere.Web
    └── MVC Controllers, Views and UI
```

## ⚙️ Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/PharmaSphere.git
```

### 2. Open the solution

Open:

```text
PharmaSphere.sln
```

using Visual Studio.

### 3. Configure the database

Update the connection string in your local configuration.

Do not commit real passwords, API keys, or production connection strings.

### 4. Apply migrations

Run:

```bash
dotnet ef database update
```

or run the application if the project is configured to apply migrations automatically.

### 5. Run the application

Press:

```text
F5
```

or run:

```bash
dotnet run
```

## 📌 Project Status

🚧 The project is under active development.

## 👨‍💻 Author

Karim Gamal Draz

.NET Full-Stack Developer
