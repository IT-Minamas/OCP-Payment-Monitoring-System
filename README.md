# OCP Payment System

OCP Payment System is a web-based application developed to manage the approval workflow and payment process for Outside Crop Producer (OCP) suppliers within Minamas Plantation (SD Guthrie Group).
The application provides an integrated workflow from Memo creation, approval process, supporting document management, supplier verification, until payment monitoring.

---
## Technology Stack
- ASP.NET Core MVC (.NET 8)
- ASP.NET Core Web API
- Microsoft SQL Server
- ADO.NET
- Bootstrap 5
- jQuery
- Git / GitHub

---
## Solution Structure
```
OCPPaymentSystem
│
├── OCPPaymentSystem.sln
│
├── OCPPaymentSystem.Web
│   Web Front-End
│
├── OCPPaymentSystemAPI
│   REST API
│
├── Database
│   Database Scripts
│
└── Documents
    Functional Documents
```

---
## Main Features
- User Login
- Memo Management
- Approval Workflow
- Multi-Level Approval
- Company Access Control
- Supplier Management
- Supporting Document Upload
- Attachment Download
- Email Notification
- Audit Trail
- Dashboard & Reporting

---
## External Integration
The system integrates with:
- Minamas Widget Authentication API
- Central Authentication Database
- SAP Replicate Database
- SMTP Mail Server

---
## Database
Microsoft SQL Server
Main Objects:
- Tables
- Views
- Stored Procedures
- Functions

---
## Requirements
- Visual Studio 2022
- .NET 8 SDK
- Microsoft SQL Server
- IIS Express / IIS
- Git

---
## Configuration
Update the following configuration before running the application.

### OCPPaymentSystem.Web
```
appsettings.json
```

### OCPPaymentSystemAPI
```
appsettings.json
```

Configure:
- Database Connection String
- External API URL
- API Key
- SMTP Configuration

---
## Getting Started
Clone repository
```bash
git clone https://github.com/<username>/OCPPaymentSystem.git
```

Open
```
OCPPaymentSystem.sln
```

Restore NuGet Packages.
Update **appsettings.json**.
Build and Run.

---
## Branch Strategy
- main
- development
- feature/*

---
## Version
Current Version
```
Version 1.0
```

---
## Author
Minamas IT Services Department
Developed by:
Rizky Yulianto Pratomo
Senior Manager IT Services
Minamas Plantation (SD Guthrie Group)

---
## License
Internal Use Only

This source code is intended for internal development and operational use within Minamas Plantation / SD Guthrie Group.
