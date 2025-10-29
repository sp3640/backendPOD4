using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // Added back
using OnlineAuctionSystem.AuctionService.Data;
using System.Text; // Added back

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Database Setup
builder.Services.AddDbContext<AuctionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

// --- START of Re-enabled JWT Configuration ---

// 2. Read JWT Settings
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new ArgumentNullException("jwtKey", "JWT Key missing in configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new ArgumentNullException("jwtIssuer", "JWT Issuer missing in configuration.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new ArgumentNullException("jwtAudience", "JWT Audience missing in configuration.");

// 3. Configure JWT Bearer Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// 4. Authorization Policy Setup
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SellerOnly", policy => policy.RequireRole("Seller"));
});

// --- END of Re-enabled JWT Configuration ---


// 5. CORS Policy for React Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactAppPolicy",
        policy =>
        {
            // ? FIX: Point to your Vite React app's URL
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// Use CORS
app.UseCors("ReactAppPolicy");

// ? FIX: Add static files to serve uploaded images from wwwroot
app.UseStaticFiles();

// ? FIX: Re-enable Authentication and Authorization middleware
// These MUST be after UseCors and before MapControllers.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();