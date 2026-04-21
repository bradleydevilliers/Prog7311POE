using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Models.Entities;

namespace TechMoveGLMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options) 
        { 
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships using Fluent API
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Client)
                .WithMany(c => c.Contracts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(s => s.Contract)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(s => s.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}