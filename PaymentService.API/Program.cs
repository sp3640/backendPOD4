using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineAuctionSystem.PaymentService.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Database Setup (Code First)
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

// 2. Configure HttpClient for communication with Auction Service
builder.Services.AddHttpClient();

// 3. Configure JWT Bearer Authentication (Must match other services)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                ?? throw new ArgumentNullException("JWT Key missing in configuration.")))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BuyerOnly", policy => policy.RequireRole("Buyer"));
});


// 4. CORS Policy for React Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactAppPolicy",
        policy =>
        {
            // IMPORTANT: Replace with your actual React app's URL
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

app.UseCors("ReactAppPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();