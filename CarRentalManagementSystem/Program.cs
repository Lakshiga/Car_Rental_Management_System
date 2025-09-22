using CarRentalManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using CarRentalManagementSystem.Interfaces;
using CarRentalManagementSystem.Repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Register Services
builder.Services.AddScoped<CarRentalManagementSystem.Services.Interfaces.IUserService, CarRentalManagementSystem.Services.UserService>();
builder.Services.AddScoped<CarRentalManagementSystem.Services.Interfaces.ICarService, CarRentalManagementSystem.Services.CarService>();
builder.Services.AddScoped<CarRentalManagementSystem.Services.Interfaces.IBookingService, CarRentalManagementSystem.Services.BookingService>();
builder.Services.AddScoped<CarRentalManagementSystem.Services.Interfaces.IEmailService, CarRentalManagementSystem.Services.EmailService>();
builder.Services.AddScoped<CarRentalManagementSystem.Services.Interfaces.IPaymentService, CarRentalManagementSystem.Services.PaymentService>();
builder.Services.AddScoped<CarRentalManagementSystem.Services.Interfaces.IRentService, CarRentalManagementSystem.Services.RentService>();

// Register Repositories
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register AI Assistant Services
builder.Services.AddHttpClient<CarRentalManagementSystem.Services.Interfaces.IAIAssistantService, CarRentalManagementSystem.Services.AIAssistantService>();
builder.Services.AddScoped<CarRentalManagementSystem.Services.Interfaces.IAIAssistantService, CarRentalManagementSystem.Services.AIAssistantService>();
builder.Services.AddScoped<CarRentalManagementSystem.Services.CarRentalDataFeedService>();
builder.Services.AddScoped<CarRentalManagementSystem.Services.CarRentalFAQService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Disabled for HTTP-only access
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

app.Run();