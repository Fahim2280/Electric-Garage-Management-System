# Electric Garage Management System

A comprehensive mini ERP system designed specifically for garage management operations, built with modern web technologies.

## 🚗 Overview

The Electric Garage Management System is a full-featured web application that streamlines garage operations including vehicle management, service tracking, inventory control, customer management, and billing. This system is designed to help garage owners and mechanics efficiently manage their daily operations with an intuitive and responsive interface.

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core MVC 8
- **Database**: Microsoft SQL Server (MSSQL)
- **ORM**: Entity Framework Core
- **Frontend**: HTML5, CSS3, Bootstrap 5, JavaScript
- **Authentication**: ASP.NET Core Identity
- **Architecture**: MVC (Model-View-Controller)

## ✨ Key Features

### 🔧 Service Management
- Service order creation and tracking
- Work progress monitoring
- Service history maintenance
- Technician assignment and scheduling

### 👥 Customer Management
- Customer registration and profiles
- Contact information management
- Service history tracking
- Customer communication logs

### 🚙 Vehicle Management
- Vehicle registration and details
- Maintenance history tracking
- Vehicle inspection records
- Multi-vehicle support per customer

### 📦 Inventory Management
- Parts and supplies tracking
- Stock level monitoring
- Supplier management
- Purchase order generation

### 💰 Financial Management
- Invoice generation and management
- Payment tracking
- Revenue reporting
- Expense management

### 📊 Reporting & Analytics
- Service performance metrics
- Revenue analysis
- Inventory reports
- Customer analytics

### 🔐 User Management
- Role-based access control
- User authentication and authorization
- Staff management
- Activity logging

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- Microsoft SQL Server (LocalDB, Express, or Full version)
- Visual Studio 2022 or Visual Studio Code
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Fahim2280/Electric-Garage-Management-System.git
   cd electric-garage-management-system
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection string**
   
   Edit `appsettings.json` and update the connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ElectricGarageDB;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the application**
   
   Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`

## 📁 Project Structure

```
ElectricGarageManagementSystem/
├── Controllers/          # MVC Controllers
├── DTOs/                 # Data models and ViewModels
├── Views/                # Razor views
├── Models/                 # DbContext and configurations
├── Services/             # Business logic services
├── wwwroot/              # Static files (CSS, JS, images)
├── Migrations/           # EF Core migrations
├── Areas/                # Area-specific features
└── appsettings.json      # Configuration settings
```

## 🗄️ Database Schema

### Core Entities

- **Customers**: Customer information and contact details
- **Vehicles**: Vehicle registration and specifications
- **Services**: Service orders and work details
- **ServiceItems**: Individual service line items
- **Inventory**: Parts and supplies management
- **Invoices**: Billing and payment information
- **Users**: System users and authentication
- **Roles**: User roles and permissions

## 🔧 Configuration

### Database Configuration

The application uses Entity Framework Core with SQL Server. Configure your database connection in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your SQL Server Connection String"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Identity Configuration

ASP.NET Core Identity is pre-configured for user authentication and authorization with role-based access control.

## 🚀 Deployment

### Development Environment

1. Ensure SQL Server is running
2. Update connection strings for your environment
3. Run database migrations
4. Start the application using `dotnet run`

### Production Deployment

1. Configure production connection strings
2. Set up SQL Server database
3. Apply migrations in production
4. Configure IIS or hosting environment
5. Set environment variables for production


## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For support and questions:

- Create an issue in the GitHub repository
- Email: kfahim2280@gmail.com


## 🔄 Version History

- **v1.0.0** - Initial release with core functionality
- **v1.1.0** - Added reporting features
- **v1.2.0** - Enhanced inventory management

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Entity Framework Core for robust ORM capabilities
- Bootstrap team for responsive UI components
- Community contributors and testers

