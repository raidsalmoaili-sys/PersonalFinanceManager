using PersonalFinanceManager1.Models;

namespace PersonalFinanceManager1.Helpers;

public static class PlanLogPurposeExtensions
{
    public static string ToArabicLabel(this PlanLogPurpose purpose) => purpose switch
    {
        PlanLogPurpose.BillPayment => "سداد الفاتورة",
        PlanLogPurpose.AdvancePayment => "سداد السلفة",
        PlanLogPurpose.Reserved => "محجوز / مستقبلي",
        _ => purpose.ToString()
    };
}
