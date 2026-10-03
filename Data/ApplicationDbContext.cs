using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager1.Models;

namespace PersonalFinanceManager1.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<FinancialPlan> FinancialPlans => Set<FinancialPlan>();
    public DbSet<PlanLog> PlanLogs => Set<PlanLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<FinancialPlan>(entity =>
        {
            entity.HasOne(p => p.User)
                .WithMany(u => u.FinancialPlans)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => p.UserId);
        });

        builder.Entity<PlanLog>(entity =>
        {
            entity.HasOne(l => l.Plan)
                .WithMany(p => p.PlanLogs)
                .HasForeignKey(l => l.PlanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(l => l.PlanId);
        });
    }
}
