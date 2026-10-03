using PersonalFinanceManager1.Models;

namespace PersonalFinanceManager1.Helpers;

public static class PlanTypeExtensions
{
    public static string ToArabicLabel(this PlanType type) => type switch
    {
        PlanType.Save => "ادخار",
        PlanType.Debt => "سداد دين",
        PlanType.Invest => "استثمار",
        _ => type.ToString()
    };

    public static string ToIconGlyph(this PlanType type) => type switch
    {
        PlanType.Save => "◈",
        PlanType.Debt => "◉",
        PlanType.Invest => "▲",
        _ => "?"
    };

    public static string ToDefaultColor(this PlanType type) => type switch
    {
        PlanType.Save => "#10B981",
        PlanType.Debt => "#6366F1",
        PlanType.Invest => "#0EA5E9",
        _ => "#10B981"
    };
}
