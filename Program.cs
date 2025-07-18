using BankSysAPI.Data;
using BankSysAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BankSysAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// JWT servisini ekle
builder.Services.AddScoped<JwtService>();

// JWT Authentication ayarları
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var config = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key")))
        };
    });

// DB bağlantısı
builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS politikası
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddScoped<EmailService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// CORS burada aktif edilmeli!
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ✅ LoanStatus kayıtlarını seed et
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();

    // LoanStatus seed işlemi
    if (!db.LoanStatuses.Any())
    {
        var statuses = new List<LoanStatus>
        {
            new LoanStatus { Id = 1, Name = "Pending" },
            new LoanStatus { Id = 2, Name = "Approved" },
            new LoanStatus { Id = 3, Name = "Rejected" }
        };

        db.LoanStatuses.AddRange(statuses);
        db.SaveChanges();
    }

    // Eski dummy account temizleme işlemi
    var existing = db.Accounts.FirstOrDefault(a => a.UserId == 15);
    if (existing != null)
    {
        db.Accounts.Remove(existing);
        db.SaveChanges();
    }
}

app.Run();
