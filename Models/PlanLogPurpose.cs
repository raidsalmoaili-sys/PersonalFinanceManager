namespace PersonalFinanceManager1.Models;

public enum PlanLogPurpose
{
    BillPayment = 0,   // سداد الفاتورة — يخصم من إجمالي الدين فورًا
    AdvancePayment = 1, // سداد السلفة — يخصم من إجمالي الدين فورًا
    Reserved = 2        // إدخال / حجز مستقبلي — لا يخصم من الدين حتى يُحوَّل لسداد فعلي
}
