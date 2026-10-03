using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinanceManager1.Models;

public class FinancialPlan
{
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public PlanType Type { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Target { get; set; }

    // عدد الدفعات المخطط لها، 0 يعني بدون خريطة دفعات
    public int Slots { get; set; }

    // مبلغ الدفعة الشهرية إذا حدده المستخدم يدويًا، وإلا يُحسب تلقائيًا من Target / Slots
    [Column(TypeName = "decimal(18,2)")]
    public decimal? InstallmentAmount { get; set; }

    [Required, MaxLength(7)]
    public string ColorHex { get; set; } = "#10B981";

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    // إيقاف الخطة مؤقتًا: تختفي من إجماليات لوحة التحكم والرسوم لحد ما يعاد تفعيلها
    public bool IsPaused { get; set; }

    public ICollection<PlanLog> PlanLogs { get; set; } = new List<PlanLog>();
}
