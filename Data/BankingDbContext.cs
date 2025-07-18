using Microsoft.EntityFrameworkCore;
using BankSysAPI.Models;

namespace BankSysAPI.Data
{
    public class BankingDbContext : DbContext
    {
        public BankingDbContext(DbContextOptions<BankingDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; } 
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<CustomerEmail> CustomerEmails { get; set; }

        public DbSet<LoanApplicationType> LoanApplicationTypes { get; set; }
        public DbSet<LoanStatus> LoanStatuses { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer ayarları
            modelBuilder.Entity<Customer>().HasKey(c => c.CustomerId);
            modelBuilder.Entity<Customer>().HasIndex(c => c.Email).IsUnique();

            // Role enum'u int olarak mapleniyor
            modelBuilder.Entity<User>()
                .Property(u => u.RoleId)
                .HasConversion<int>();

            // CustomerEmail ayarları
            modelBuilder.Entity<CustomerEmail>()
                .HasKey(e => e.Id); // <-- BURASI önemli

            modelBuilder.Entity<CustomerEmail>()
                .HasOne(e => e.Customer)
                .WithMany(c => c.Emails)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LoanApplication>()
                .HasOne(l => l.LoanStatus)
                .WithMany()
                .HasForeignKey(l => l.LoanStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanStatus>().HasData(
                new LoanStatus { Id = -1, Name = "Pending", Description = "Waiting for approval", IsActive = true },
                new LoanStatus { Id = -2, Name = "Approved", Description = "Loan has been approved", IsActive = true },
                new LoanStatus { Id = -3, Name = "Rejected", Description = "Loan has been rejected", IsActive = true }
            );


        }
    }
}
