using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ECommerceWebsite.Models.Context;
using ECommerceWebsite.Repository;
using ECommerceWebsite.Models.Repository;
using ECommerceWebsite.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ECommerceWebsiteDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

//authorizaion
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login"; 
        options.LogoutPath = "/Login/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.AccessDeniedPath = "/Login/Login";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});


builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<ICartHistoryRepository, CartHistoryRepository>();
builder.Services.AddScoped<IOrderServiceRepository, OrderServiceRepository>();


var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SignUp}/{action=SignUp}/{id?}");
app.Run();
