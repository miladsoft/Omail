# Omail

![Omail Logo](Omail/wwwroot/images/logo.png)

Omail is a comprehensive email management solution built with Blazor and ASP.NET Core.

## Features

- 📱 **Fully responsive design** for all screen sizes
- 🌙 **Beautiful dark mode** with smooth transitions
- 🔒 **Authentication and authorization system** with role-based permissions
- 📬 **Complete email workflow** (inbox, compose, sent, drafts, trash)
- 🎨 **Customizable UI components** with TailwindCSS
- 🌐 **Progressive Web App** capabilities for offline access
- 👨‍💼 **Comprehensive Admin Dashboard** for system management
- 👥 **User management** with roles and permissions
- 🏢 **Organization and department management** with hierarchical structure
- 📊 **Advanced analytics and statistics** for email usage
- 📱 **Responsive layout** with desktop, tablet, and mobile support
- ✅ **Email approval workflows** for organizational control
- 📧 **Customizable email templates** for common communications
- 🔄 **Real-time updates** for new messages and notifications

## Project Structure

```
Omail/
├── Authentication/      # Authentication providers and handlers
├── Components/          # Reusable Blazor components
├── Data/                # Data models and database context
├── Layout/              # Layout components (MainLayout, AdminLayout)
├── Pages/               # Blazor pages
│   ├── Admin/           # Admin dashboard pages
│   └── ...              # Regular user pages
├── Services/            # Business logic and services
└── wwwroot/             # Static web assets
    ├── css/             # Stylesheets
    ├── js/              # JavaScript files
    └── images/          # Image assets
```

## Technology Stack

- **Backend**: ASP.NET Core 6.0, Entity Framework Core
- **Frontend**: Blazor WebAssembly, TailwindCSS
- **Authentication**: Custom authentication with role-based authorization
- **Database**: SQL Server / SQLite
- **UI Components**: Custom UI components with TailwindCSS
- **Icons**: [Lord Icons](https://lordicon.com/)

## Getting Started

### Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- [Node.js](https://nodejs.org/) (for TailwindCSS)
- Database: SQL Server, SQLite, or compatible EF Core provider

### Installation

1. Clone the repository:

```bash
git clone https://github.com/miladsoft/Omail.git
```

2. Navigate to the project directory:

```bash
cd Omail
```

3. Install npm dependencies:

```bash
cd Omail
npm install
```

4. Build TailwindCSS:

```bash
npm run build:css
```

5. Run the application:

```bash
dotnet run
```

6. Access the application at `https://localhost:5001` or `http://localhost:5000`

### Configuration

The application can be configured through the `appsettings.json` file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Omail.db"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.example.com",
    "SmtpPort": 587,
    "EnableSsl": true
  }
}
```

## Development

To run the application in development mode:

```bash
dotnet watch run
```

## Deployment

For deploying to production, use the following commands:

```bash
dotnet publish -c Release
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Credits

- [TailwindCSS](https://tailwindcss.com/)
- [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- [Lord Icons](https://lordicon.com/)