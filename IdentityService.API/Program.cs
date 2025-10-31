using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.IdentityService.Data;
using OnlineAuctionSystem.IdentityService.Models;
using OnlineAuctionSystem.IdentityService.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Database and Identity Setup (Code First)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 2. JWT Helper Registration
builder.Services.AddScoped<JwtTokenGenerator>();

// 3. CORS Policy for React Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactAppPolicy",
        policy =>
        {
            // IMPORTANT: Replace with your actual React app's URL
            policy.WithOrigins("http://localhost:5175")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
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

// This is correct. You have commented it out.
//app.UseHttpsRedirection();

// Use CORS before Authorization/Authentication
app.UseCors("ReactAppPolicy");

app.UseAuthentication(); // Not strictly needed here, but good practice
app.UseAuthorization();

app.MapControllers();

app.Run();
