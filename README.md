# BeeMart - Binus Point of Sale (POS) Web App (.NET 8 C#)

BeeMart is an integrated Point of Sale (POS) system built using **C# .NET 8.0** with strict **Separation of Concerns (SoC)** architecture. The system is decoupled into two independent projects: **WebClient (Frontend/UI)** and **Backend API**.

---

## 🏛️ Architecture & Tech Stack

- **Backend API**: ASP.NET Core Web API (.NET 8)
  - **ORM**: Entity Framework Core 8 with SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`)
  - **Security**: SHA-256 password hashing, Role-based Authorization (Admin & Cashier)
  - **API Documentation**: Swagger / OpenAPI v1
  - **Design**: RESTful with endpoint versioning (`/api/v1/...`)
- **WebClient (Frontend)**:
  - ASP.NET Core static web host (.NET 8)
  - **Zero Database Access**: 100% dependent on Backend RESTful API via `fetch`
  - **Pure Vanilla CSS**: Strictly NO Bootstrap used, matching the exact UI mockup specifications
  - **Dynamic State**: Real-time local clock, automatic invoice generation, cart state, dynamic payment methods, and paginated transaction history
- **Database**:
  - Microsoft SQL Server (`BeeMartDb`)
  - Pre-seeded with master payment methods, 10 categories of catalog products, users, and mockup transactions

---

## 📂 Project Structure

```
.
├── BeeMart.sln                           # Visual Studio Solution
├── QUERY CREATE TABLE N INSERT DATA.sql  # Database DDL & Seed Data
├── Backend/                              # ASP.NET Core Backend API
│   ├── Controllers/                      # Auth, Products, PaymentMethods, Transactions
│   ├── Data/                             # BeeMartDbContext
│   ├── DTOs/                             # Request & Response Data Transfer Objects
│   ├── Helpers/                          # PasswordHelper, InvoiceNumberHelper
│   ├── Models/                           # Entity Models (MsUser, MsProduct, etc.)
│   ├── appsettings.json                  # Database connection string
│   └── Program.cs                        # Web host, EF Core, CORS, Swagger
└── Frontend/                             # ASP.NET Core Frontend WebClient
    ├── wwwroot/
    │   ├── index.html                    # Main Dashboard (New Trx & History Tabs)
    │   ├── login.html                    # Login Page
    │   ├── register.html                 # Registration Page
    │   ├── css/style.css                 # Pure Vanilla CSS (No Bootstrap)
    │   ├── js/                           # api.js, transaction.js, history.js
    │   └── images/                       # Asset icons
    ├── appsettings.json                  # Backend API URL config
    └── Program.cs                        # Static file server
```

---

## 🚀 Getting Started

### 1. Database Setup
Ensure SQL Server (e.g. `SQLEXPRESS`) is running, then run the SQL script:
```powershell
sqlcmd -S ".\SQLEXPRESS" -E -C -i "QUERY CREATE TABLE N INSERT DATA.sql"
```

### 2. Run Backend API
```powershell
dotnet run --project Backend/BeeMart.Api.csproj --launch-profile http
```
- API Base URL: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`

### 3. Run Frontend WebClient
```powershell
dotnet run --project Frontend/BeeMart.WebClient.csproj --launch-profile http
```
- WebClient URL: `http://localhost:5200/login.html`

---

## 🔑 Seed User Credentials

| Role | Username | Password | User ID | Description |
| :--- | :--- | :--- | :--- | :--- |
| **Admin** | `admin` | `admin123` | `10001` | Full access, views all cashier transactions |
| **Kasir** | `steven` | `steven123` | `76985` | Primary test cashier from document mockup |
| **Kasir** | `gabri` | `gabri123` | `66859` | Seed cashier with 03/09/2026 transactions |
| **Kasir** | `mona` | `mona123` | `55794` | Seed cashier with 03/09/2026 transactions |
| **Kasir** | `felix` | `felix123` | `78886` | Seed cashier with 03/09/2026 transactions |
| **Kasir** | `gobuta` | `gobuta123` | `55356` | Seed cashier with 03/09/2026 transactions |
