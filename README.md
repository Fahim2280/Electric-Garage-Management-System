# Electric Garage Management System

A mini ERP system built with ASP.NET Core MVC 8 for managing electric garage operations, customer data, and billing processes.

## 🚗 Overview

The Electric Garage Management System is a web-based application designed to streamline garage operations by providing comprehensive customer and billing management capabilities. The system enables administrators to efficiently manage customer records, generate bills, and track service history.

## ✨ Features

- **Admin Authentication**: Secure login system using JWT authentication
- **Customer Management**: Add, edit, and manage customer information
- **Billing System**: Create and manage customer bills
- **Bill Tracking**: View and monitor all customer bills
- **User-friendly Interface**: Responsive MVC-based web interface

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core MVC 8
- **Database**: Microsoft SQL Server (MSSQL)
- **ORM**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Architecture**: Code First approach

## 📋 Prerequisites

Before running this application, ensure you have the following installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [SQL Server Management Studio (SSMS)](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) (optional)

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git https://github.com/Fahim2280/Electric-Garage-Management-System.git
cd electric-garage-management-system
```

### 2. Database Configuration

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ElectricGarageDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "Key": "your-secret-key-here",
    "Issuer": "ElectricGarageSystem",
    "Audience": "ElectricGarageUsers",
    "ExpireMinutes": 60
  }
}
```

### 3. Database Migration

Run the following commands to create and update the database:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`.

## 📁 Project Structure

```
ElectricGarageManagementSystem/
├── Controllers   
├── Models
├── Views
├── DTOs
├── Services
├── wwwroot
├── appsettings.json
└── Program.cs
```

## 🔧 Key Components

### Models

- **Admin**: Manages administrator accounts and authentication
- **Customer**: Stores customer information and contact details
- **Bill**: Handles billing information and service records

### Controllers

- **AdminController**: Handles admin authentication and dashboard
- **CustomerController**: Manages customer CRUD operations
- **BillController**: Handles bill creation and management

### Services

- **AuthService**: Manages JWT token generation and validation

## 🔐 Authentication

The system uses JWT-based authentication for admin users. Upon successful login, a JWT token is generated and used for subsequent requests to protected endpoints.

### Login Process:
1. Admin enters credentials
2. System validates credentials against database
3. JWT token is generated and returned
4. Token is stored and used for authenticated requests

## 🎯 Usage

### Admin Login
1. Navigate to `/Admin/Login`
2. Enter admin credentials
3. Access the admin dashboard

### Customer Management
1. From admin dashboard, navigate to Customer section
2. Add new customers with required information
3. Edit or view existing customer details

### Bill Management
1. Select a customer from the customer list
2. Create a new bill with service details and amount
3. View all bills for tracking and management

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For support and questions, please open an issue in the repository or contact the development team.

## 🔄 Version History

- **v1.0.0** - Initial release with basic ERP functionality
  - Admin authentication
  - Customer management
  - Basic billing system

