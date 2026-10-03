using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager1.Models;

namespace PersonalFinanceManager1.Data;

/// <summary>
/// حساب تجريبي جاهز ببيانات وهمية واقعية (6 خطط + سجل دفعات لعدة أشهر) حتى يقدر أي شخص
/// يفتح المشروع يشوف كل المزايا مباشرة بدون ما يسجّل ويدخل بيانات بنفسه.
///
/// - عند تشغيل التطبيق: يُنشأ الحساب مرة واحدة فقط إذا لم يكن موجوداً.
/// - `dotnet run -- seed`: يعيد ضبط بيانات الحساب التجريبي من جديد (لا يلمس حسابات المستخدمين الآخرين).
///
/// كل البيانات هنا وهمية، والتواريخ نسبية لتاريخ اليوم حتى تظهر الرسوم البيانية دائماً "حيّة".
/// </summary>
public static class DemoAccountSeeder
{
    public const string DemoEmail = "demo@finance.local";
    public const string DemoPassword = "Demo1234";

    public static async Task SeedAsync(IServiceProvider services, bool reset)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await users.FindByEmailAsync(DemoEmail);
        if (user != null && !reset) return;

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = DemoEmail,
                Email = DemoEmail,
                EmailConfirmed = true,
                FullName = "حساب تجريبي"
            };
            var result = await users.CreateAsync(user, DemoPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException("تعذّر إنشاء الحساب التجريبي: " +
                    string.Join("، ", result.Errors.Select(e => e.Description)));
        }
        else
        {
            // إعادة ضبط: حذف خطط الحساب التجريبي فقط (الدفعات تنحذف معها تلقائياً Cascade).
            var old = await db.FinancialPlans.Where(p => p.UserId == user.Id).ToListAsync();
            db.FinancialPlans.RemoveRange(old);
            await db.SaveChangesAsync();
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var month0 = new DateOnly(today.Year, today.Month, 1);

        // ── 1) صندوق الطوارئ — ادخار شهري ثابت + مكافأة ──
        var emergency = Plan(user.Id, "صندوق الطوارئ", PlanType.Save, 30000, 12, 2500, "#10B981", month0.AddMonths(-8));
        for (var i = 8; i >= 1; i--)
            Log(emergency, 2500, month0.AddMonths(-i).AddDays(26), "ادخار من الراتب");
        Log(emergency, 3000, month0.AddMonths(-3).AddDays(10), "مكافأة سنوية");

        // ── 2) قرض السيارة — أقساط شهرية + قسط محجوز للشهر القادم ──
        var car = Plan(user.Id, "سداد قرض السيارة", PlanType.Debt, 48000, 24, 2000, "#6366F1", month0.AddMonths(-10));
        for (var i = 10; i >= 1; i--)
            Log(car, 2000, month0.AddMonths(-i).AddDays(27), "قسط شهري");
        Log(car, 2000, month0.AddMonths(1).AddDays(26), "قسط الشهر القادم", PlanLogPurpose.Reserved);

        // ── 3) بطاقة الائتمان — سداد فواتير + سداد سلفة ──
        var card = Plan(user.Id, "سداد بطاقة الائتمان", PlanType.Debt, 7500, 5, 1500, "#F43F5E", month0.AddMonths(-4));
        Log(card, 1500, month0.AddMonths(-3).AddDays(4), "سداد الفاتورة");
        Log(card, 1500, month0.AddMonths(-2).AddDays(4), "سداد الفاتورة");
        Log(card, 1000, month0.AddMonths(-1).AddDays(15), "سداد سلفة نقدية", PlanLogPurpose.AdvancePayment);
        Log(card, 1500, month0.AddDays(3), "سداد الفاتورة");

        // ── 4) محفظة استثمارية — إيداعات غير منتظمة بدون جدول دفعات ──
        var invest = Plan(user.Id, "محفظة استثمارية طويلة المدى", PlanType.Invest, 50000, 0, null, "#0EA5E9", month0.AddMonths(-7));
        invest.EndDate = month0.AddYears(3);
        decimal[] deposits = { 5000, 3000, 4500, 6000, 3500, 4000, 5500 };
        for (var i = 0; i < deposits.Length; i++)
            Log(invest, deposits[i], month0.AddMonths(-7 + i).AddDays(12), i == 0 ? "إيداع افتتاحي" : "إيداع شهري");

        // ── 5) رحلة الصيف — خطة جديدة قصيرة ──
        var trip = Plan(user.Id, "رحلة الصيف العائلية", PlanType.Save, 12000, 6, 2000, "#F59E0B", month0.AddMonths(-2));
        trip.EndDate = month0.AddMonths(4);
        Log(trip, 2000, month0.AddMonths(-2).AddDays(25), "ادخار من الراتب");
        Log(trip, 2000, month0.AddMonths(-1).AddDays(25), "ادخار من الراتب");
        Log(trip, 1500, month0.AddMonths(1).AddDays(25), "حجز مبلغ الشهر القادم", PlanLogPurpose.Reserved);

        // ── 6) دفعة أولى لشقة — خطة موقوفة مؤقتاً (تظهر كيف تُستبعد من الإجماليات) ──
        var flat = Plan(user.Id, "الدفعة الأولى لشقة", PlanType.Save, 120000, 0, null, "#8B5CF6", month0.AddMonths(-12));
        flat.IsPaused = true;
        Log(flat, 10000, month0.AddMonths(-12).AddDays(5), "إيداع افتتاحي");
        Log(flat, 7500, month0.AddMonths(-9).AddDays(5), "إيداع");
        Log(flat, 6000, month0.AddMonths(-6).AddDays(5), "إيداع");

        db.FinancialPlans.AddRange(emergency, car, card, invest, trip, flat);
        await db.SaveChangesAsync();
    }

    private static FinancialPlan Plan(string userId, string name, PlanType type, decimal target, int slots,
        decimal? installment, string color, DateOnly start) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Name = name,
        Type = type,
        Target = target,
        Slots = slots,
        InstallmentAmount = installment,
        ColorHex = color,
        StartDate = start
    };

    private static void Log(FinancialPlan plan, decimal amount, DateOnly date, string? note,
        PlanLogPurpose purpose = PlanLogPurpose.BillPayment) =>
        plan.PlanLogs.Add(new PlanLog
        {
            Id = Guid.NewGuid(),
            PlanId = plan.Id,
            Amount = amount,
            PaymentDate = date,
            Note = note,
            Purpose = purpose
        });
}
