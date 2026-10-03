using System.Globalization;
using PersonalFinanceManager1.Helpers;
using PersonalFinanceManager1.Models;
using PersonalFinanceManager1.ViewModels;

namespace PersonalFinanceManager1.Services;

public class PlanCalculationService : IPlanCalculationService
{
    private const int MonthsWindow = 12;

    private static readonly string[] ArabicMonthNames =
    [
        "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
    ];

    private static string Fmt(decimal n) =>
        Math.Round(n, MidpointRounding.AwayFromZero).ToString("N0", CultureInfo.InvariantCulture);

    private static string ArDate(DateOnly d) => $"{d.Day} {ArabicMonthNames[d.Month - 1]} {d.Year}";

    private static string Soft(string hex, string alpha) => hex + alpha;

    public decimal GetAchieved(FinancialPlan plan) =>
        plan.PlanLogs.Where(l => l.Purpose != PlanLogPurpose.Reserved).Sum(l => l.Amount);

    public decimal GetReserved(FinancialPlan plan) =>
        plan.PlanLogs.Where(l => l.Purpose == PlanLogPurpose.Reserved).Sum(l => l.Amount);

    public int GetPercentComplete(decimal target, decimal achieved)
    {
        if (target <= 0) return 0;
        var percent = Math.Round(achieved / target * 100, MidpointRounding.AwayFromZero);
        return (int)Math.Min(100, Math.Max(0, percent));
    }

    public decimal GetRemaining(decimal target, decimal achieved) => Math.Max(0, target - achieved);

    public decimal GetAveragePayment(FinancialPlan plan)
    {
        var counted = plan.PlanLogs.Where(l => l.Purpose != PlanLogPurpose.Reserved).ToList();
        return counted.Count == 0 ? 0 : counted.Sum(l => l.Amount) / counted.Count;
    }

    public PlanCardViewModel ToCardViewModel(FinancialPlan plan)
    {
        var achieved = GetAchieved(plan);
        var target = plan.Target;
        var sub = $"{plan.Type.ToArabicLabel()} · الهدف {Fmt(target)} ريال";
        if (plan.Slots > 0) sub += $" · {plan.Slots} دفعة";

        return new PlanCardViewModel
        {
            Id = plan.Id,
            Name = plan.Name,
            Type = plan.Type,
            ColorHex = plan.ColorHex,
            IconGlyph = plan.Type.ToIconGlyph(),
            Sub = sub,
            DoneLabel = plan.Type == PlanType.Debt ? "المسدد" : "المجمّع",
            Target = target,
            Achieved = achieved,
            Remaining = GetRemaining(target, achieved),
            Reserved = GetReserved(plan),
            PercentComplete = GetPercentComplete(target, achieved),
            IsPaused = plan.IsPaused
        };
    }

    public PlanDetailsViewModel ToDetailsViewModel(FinancialPlan plan)
    {
        var achieved = GetAchieved(plan);
        var target = plan.Target;
        var percent = GetPercentComplete(target, achieved);
        var remaining = GetRemaining(target, achieved);
        var paidLogsCount = plan.PlanLogs.Count(l => l.Purpose != PlanLogPurpose.Reserved);
        var filledSlots = plan.Slots > 0 ? Math.Min(plan.Slots, paidLogsCount) : 0;
        var isDebt = plan.Type == PlanType.Debt;

        string targetNote;
        if (plan.Slots > 0)
        {
            var perSlot = plan.InstallmentAmount ?? Math.Round(target / plan.Slots, MidpointRounding.AwayFromZero);
            targetNote = $"{Fmt(perSlot)} ريال × {plan.Slots} مرة";
        }
        else if (plan.EndDate.HasValue)
        {
            targetNote = $"الاستحقاق {ArDate(plan.EndDate.Value)}";
        }
        else
        {
            targetNote = "بدون جدول دفعات محدد";
        }

        var countValue = plan.Slots > 0 ? $"{filledSlots} / {plan.Slots}" : plan.PlanLogs.Count.ToString();
        var countWidth = plan.Slots > 0
            ? Math.Min(100, (int)Math.Round(filledSlots * 100.0 / plan.Slots))
            : percent;

        return new PlanDetailsViewModel
        {
            Id = plan.Id,
            Name = plan.Name,
            Type = plan.Type,
            ColorHex = plan.ColorHex,
            Target = target,
            Achieved = achieved,
            Remaining = remaining,
            Reserved = GetReserved(plan),
            PercentComplete = percent,
            IsPaused = plan.IsPaused,
            AveragePayment = GetAveragePayment(plan),
            PaymentsCount = plan.PlanLogs.Count,
            Slots = plan.Slots,
            FilledSlots = filledSlots,

            TargetNote = targetNote,
            DoneLabel = isDebt ? "إجمالي المسدد" : "المبلغ المجمّع",
            CountLabel = plan.Slots > 0 ? "الدفعات المكتملة" : "عدد العمليات",
            CountValue = countValue,
            CountWidthPercent = $"{countWidth}%",

            GaugeGradientCss = $"conic-gradient({plan.ColorHex} 0% {percent}%, var(--chip) {percent}% 100%)",
            GaugeSub = isDebt ? "الانخفاض نحو صفر دين" : "التقدّم نحو الهدف",
            GaugeCaption = isDebt ? "مسدد من الدين" : "منجز من الهدف",

            FormTitle = isDebt ? "تسجيل دفعة سداد" : "تسجيل إيداع جديد",
            SaveButtonLabel = isDebt ? "تسجيل الدفعة" : "تسجيل الإيداع",
            NotePlaceholder = isDebt ? "قسط شهري / دفعة إضافية" : "ادخار راتب / مكافأة",
            TableTitle = isDebt ? "سجل دفعات السداد" : "سجل الدفعات بالتواريخ",

            Payments = plan.PlanLogs
                .OrderByDescending(l => l.PaymentDate)
                .Select(l => new PlanLogItemViewModel
                {
                    Id = l.Id,
                    PaymentDate = l.PaymentDate,
                    DateLabel = ArDate(l.PaymentDate),
                    Amount = l.Amount,
                    Note = l.Note,
                    Purpose = l.Purpose,
                    PurposeLabel = l.Purpose.ToArabicLabel()
                })
                .ToList(),

            EditPlan = new CreatePlanViewModel
            {
                Name = plan.Name,
                Type = plan.Type,
                Target = plan.Target,
                Slots = plan.Slots,
                InstallmentAmount = plan.InstallmentAmount,
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                ColorHex = plan.ColorHex
            }
        };
    }

    public (string GradientCss, List<DonutLegendItemViewModel> Legend) BuildDonut(List<FinancialPlan> plans)
    {
        var segs = new List<(string Label, decimal Value, string Color)>();
        foreach (var plan in plans)
        {
            var achieved = GetAchieved(plan);
            var done = Math.Min(achieved, plan.Target);
            var left = Math.Max(0, plan.Target - achieved);
            segs.Add(($"منجز — {plan.Name}", done, plan.ColorHex));
            segs.Add(($"متبقي — {plan.Name}", left, Soft(plan.ColorHex, "40")));
        }

        if (segs.Count == 0) return ("var(--chip)", new List<DonutLegendItemViewModel>());

        var segTotal = segs.Sum(s => s.Value);
        if (segTotal <= 0) segTotal = 1;

        decimal cumulative = 0;
        var stops = new List<string>();
        var legend = new List<DonutLegendItemViewModel>();

        foreach (var seg in segs)
        {
            var from = cumulative / segTotal * 100;
            cumulative += seg.Value;
            var to = cumulative / segTotal * 100;
            stops.Add($"{seg.Color} {from.ToString("F2", CultureInfo.InvariantCulture)}% {to.ToString("F2", CultureInfo.InvariantCulture)}%");

            legend.Add(new DonutLegendItemViewModel
            {
                Label = seg.Label,
                ColorCss = seg.Color,
                ValueF = Fmt(seg.Value),
                Percent = (int)Math.Round(seg.Value / segTotal * 100, MidpointRounding.AwayFromZero)
            });
        }

        return ($"conic-gradient({string.Join(",", stops)})", legend);
    }

    public MonthlyChartViewModel BuildMonthlyChart(List<FinancialPlan> plans)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthStarts = Enumerable.Range(0, MonthsWindow)
            .Select(i => new DateOnly(today.Year, i + 1, 1))
            .ToList();

        // monthlyValues[planIndex][monthIndex]
        var monthlyValues = plans.Select(plan =>
        {
            var values = new decimal[MonthsWindow];
            foreach (var log in plan.PlanLogs.Where(l => l.Purpose != PlanLogPurpose.Reserved))
            {
                var idx = monthStarts.FindIndex(m => m.Year == log.PaymentDate.Year && m.Month == log.PaymentDate.Month);
                if (idx >= 0) values[idx] += log.Amount;
            }
            return values;
        }).ToList();

        var max = 1m;
        foreach (var values in monthlyValues)
            foreach (var v in values)
                if (v > max) max = v;

        var hasData = monthlyValues.Any(values => values.Any(v => v > 0));

        var months = new List<MonthPointViewModel>();
        for (var m = 0; m < MonthsWindow; m++)
        {
            var bars = new List<MonthBarViewModel>();
            for (var p = 0; p < plans.Count; p++)
            {
                var v = monthlyValues[p][m];
                bars.Add(new MonthBarViewModel
                {
                    ColorHex = plans[p].ColorHex,
                    HeightPercent = $"{(v / max * 100).ToString("F1", CultureInfo.InvariantCulture)}%",
                    Tooltip = $"{plans[p].Name}: {Fmt(v)} ريال"
                });
            }
            months.Add(new MonthPointViewModel { Label = $"{monthStarts[m].Month} {ArabicMonthNames[monthStarts[m].Month - 1]}", Bars = bars });
        }

        var series = new List<ChartSeriesViewModel>();
        for (var p = 0; p < plans.Count; p++)
        {
            series.Add(new ChartSeriesViewModel
            {
                PlanId = plans[p].Id,
                Name = plans[p].Name,
                ColorHex = plans[p].ColorHex,
                SoftColorCss = Soft(plans[p].ColorHex, "33"),
                AreaFillPath = BuildAreaPath(monthlyValues[p], max, close: true),
                AreaLinePath = BuildAreaPath(monthlyValues[p], max, close: false)
            });
        }

        return new MonthlyChartViewModel { Months = months, Series = series, HasData = hasData };
    }

    private static string BuildAreaPath(decimal[] vals, decimal max, bool close)
    {
        var n = vals.Length;
        if (n == 0) return string.Empty;

        var pts = new List<string>();
        for (var i = 0; i < n; i++)
        {
            var x = n == 1 ? 50m : (decimal)i / (n - 1) * 100;
            var y = 42 - (max > 0 ? vals[i] / max * 38 : 0);
            pts.Add($"{x.ToString("F2", CultureInfo.InvariantCulture)},{y.ToString("F2", CultureInfo.InvariantCulture)}");
        }

        var line = "M" + string.Join(" L", pts);
        return close ? line + " L100,44 L0,44 Z" : line;
    }
}
