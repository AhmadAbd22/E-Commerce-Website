using ECommerceWebsite.Hubs;
using ECommerceWebsite.Middleware;
using ECommerceWebsite.Models.Context;
using ECommerceWebsite.Models.Helping_Classes;
using ECommerceWebsite.Models.Repository;
using ECommerceWebsite.Repository;
using ECommerceWebsite.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ECommerceWebsiteDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

//Real Time Notifications
builder.Services.AddSignalR();

//authorizaion
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login"; 
        options.LogoutPath = "/Login/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.AccessDeniedPath = "/Login/Login";
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("UserOnly", policy => policy.RequireRole("User"));



builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<ICartHistoryRepository, CartHistoryRepository>();
builder.Services.AddScoped<IOrderServiceRepository, OrderServiceRepository>();
builder.Services.AddScoped<Authorization>();

var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    //custom exception middleware in development for better debugging
    app.UseMiddleware<GlobalExceptionMiddleware>();
}

// Security headers middleware
app.UseMiddleware<SecurityHeadersMiddleware>();

// Rate limiting middleware (before authentication)
app.UseMiddleware<RateLimitingMiddleware>();

// Request logging middleware
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// User activity tracking (after authentication)
app.UseMiddleware<UserActivityMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SignUp}/{action=SignUp}/{id?}");
app.MapHub<NotificationHub>("/NotificationHub");
app.Run();
