# 📚 E-Commerce Website for Books

An ASP.NET Core MVC-based e-commerce platform made using the [Repository Design Pattern](https://medium.com/@vijaymalviya320/repository-design-pattern-c24c709dd409) designed for selling books online. It features role-based access, admin management tools, user-friendly book listings, and a responsive modern UI.

## 🚀 Features

### 🛒 Customer Side
- Browse books by category or author
- Search functionality
- View detailed book information
- Add books to cart
- Place orders

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
- **Frontend:** Razor Views, Bootstrap, Material theme
- **Database:** Entity Framework Core with SQL Server
- **Authentication:** ASP.NET Identity



## 🧑‍💻 How to Run

1. **Clone the repository:**
   ```bash
   git clone https://github.com/AhmadAbd22/E-Commerce-Website.git
2. Update the connection string in appesetting.json to point your local SQL server instance.
3. Run the command "Update-Database" in Package Manager Console.
4. Run the project (Ctrl + F5)
