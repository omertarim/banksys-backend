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
builder.Services.AddScoped<LoanScoreService>();
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

    // RiskLevels seed işlemi
    if (!db.RiskLevels.Any())
    {
        var riskLevels = new List<RiskLevel>
        {
            new RiskLevel { Id = 1, Name = "Low", Description = "Düşük risk seviyesi" },
            new RiskLevel { Id = 2, Name = "Medium", Description = "Orta risk seviyesi" },
            new RiskLevel { Id = 3, Name = "High", Description = "Yüksek risk seviyesi" }
        };

        db.RiskLevels.AddRange(riskLevels);
        db.SaveChanges();
    }

    // RecommendedApprovals seed işlemi
    if (!db.RecommendedApprovals.Any())
    {
        var recommendedApprovals = new List<RecommendedApproval>
        {
            new RecommendedApproval { Id = 1, Name = "Approved", Description = "Kredi onayı önerilir" },
            new RecommendedApproval { Id = 2, Name = "Rejected", Description = "Kredi reddi önerilir" }
        };

        db.RecommendedApprovals.AddRange(recommendedApprovals);
        db.SaveChanges();
    }

    // RecommendationStatuses seed işlemi
    if (!db.RecommendationStatuses.Any())
    {
        var recommendationStatuses = new List<RecommendationStatus>
        {
            new RecommendationStatus { Id = 1, Name = "Good financial standing", Description = "İyi finansal durum" },
            new RecommendationStatus { Id = 2, Name = "Low balance or income", Description = "Düşük bakiye veya gelir" },
            new RecommendationStatus { Id = 3, Name = "High debt-to-income ratio", Description = "Yüksek borç-gelir oranı" },
            new RecommendationStatus { Id = 4, Name = "Too many previous loan applications", Description = "Çok fazla önceki kredi başvurusu" },
            new RecommendationStatus { Id = 5, Name = "Score calculation failed", Description = "Skor hesaplama başarısız" }
        };

        db.RecommendationStatuses.AddRange(recommendationStatuses);
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
