# 📚 E-Commerce Website for Books

An ASP.NET Core MVC-based e-commerce platform made using the [Repository Design Pattern](https://medium.com/@vijaymalviya320/repository-design-pattern-c24c709dd409) designed for selling books online. It features role-based access, admin management tools, user-friendly book listings, and a responsive modern UI.

## 🚀 Features

### 🛒 Customer Side
- Browse books by category or author
- Search functionality
- View detailed book information
- Add books to cart
- Cart persistence with history tracking after checkout
- Separate active cart vs. past orders
- Place orders
- Real Time notifications using SignalR

### 🔐 Authentication
- User registration and login
- Role-based access: `Admin` and `User`
- Access control to prevent unauthorized page access
- Custom `AccessDenied` view for blocked routes

### 🛠️ Admin Panel
- Dashboard with sidebar navigation
- Add/Edit/Delete books
- Manage categories and authors
- View user orders
- Filter/search functionality in listings
- Uses custom Limitless theme layout

### 🧱 Tech Stack
- **Backend:** ASP.NET Core MVC
- **Frontend:** Razor Views, Bootstrap, Material theme (Limitless)
- **Database:** Entity Framework Core with SQL Server
- **Authentication:** Custom cookie-based authentication using ASP.NET Core's built-in 
Cookie Authentication middleware with role-based access policies (`Admin`, `User`).



## 🧑‍💻 How to Run

1. **Clone the repository:**
   ```bash
   git clone https://github.com/AhmadAbd22/E-Commerce-Website.git
2. Update the connection string in appesetting.json to point your local SQL server instance.
3. Install the following dependencies from the 'NuGet Package Manager' by navigatin to ``` Tools > NuGet Package Manager > Manage NuGet Packages for Solution ```
   - Microsoft.EntityFrameworkCore
   - Microsoft.EntityFrameworkCore.SqlServer
   - Microsoft.EntityFrameworkCore.Tools
5. Navigate to ``` Tools > NuGet Package Managet > Package Manager Console ```
6. Run the command ``` Add-Migration ``` in Package Manager Console.
7. Run the command ``` Update-Database ``` in Package Manager Console. 
8. Run the project (Ctrl + F5)
