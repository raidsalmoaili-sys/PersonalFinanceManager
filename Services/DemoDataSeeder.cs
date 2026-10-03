using PersonalFinanceManager1.Data;
using PersonalFinanceManager1.Models;

namespace PersonalFinanceManager1.Services;

// يضيف بيانات تجريبية بسيطة لكل مستخدم جديد عند إنشاء الحساب فقط،
// حتى لا تظهر له لوحة فارغة. يمكن حذف هذه الخطط بسهولة من صفحة التفاصيل.
public class DemoDataSeeder : IDemoDataSeeder
{
    private readonly ApplicationDbContext _context;

    public DemoDataSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedForNewUserAsync(string userId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var savePlan = new FinancialPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "صندوق الطوارئ",
            Type = PlanType.Save,
            Target = 10000,
            Slots = 10,
            ColorHex = "#10B981",
            StartDate = today.AddMonths(-3)
        };

        var debtPlan = new FinancialPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "سداد بطاقة الائتمان",
            Type = PlanType.Debt,
            Target = 5000,
            Slots = 5,
            ColorHex = "#6366F1",
            StartDate = today.AddMonths(-2)
        };

        _context.FinancialPlans.AddRange(savePlan, debtPlan);

        _context.PlanLogs.AddRange(
            new PlanLog { Id = Guid.NewGuid(), PlanId = savePlan.Id, Amount = 1500, PaymentDate = today.AddMonths(-2), Note = "دفعة أولى" },
            new PlanLog { Id = Guid.NewGuid(), PlanId = savePlan.Id, Amount = 1200, PaymentDate = today.AddMonths(-1) },
            new PlanLog { Id = Guid.NewGuid(), PlanId = savePlan.Id, Amount = 900, PaymentDate = today },
            new PlanLog { Id = Guid.NewGuid(), PlanId = debtPlan.Id, Amount = 1000, PaymentDate = today.AddMonths(-1), Note = "دفعة شهرية" },
            new PlanLog { Id = Guid.NewGuid(), PlanId = debtPlan.Id, Amount = 1000, PaymentDate = today }
        );

        await _context.SaveChangesAsync();
    }
}
