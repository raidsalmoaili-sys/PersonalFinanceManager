using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinanceManager1.Models;

public class PlanLog
{
    public Guid Id { get; set; }

    [Required]
    public Guid PlanId { get; set; }

    [ForeignKey(nameof(PlanId))]
    public FinancialPlan? Plan { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateOnly PaymentDate { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public PlanLogPurpose Purpose { get; set; } = PlanLogPurpose.BillPayment;
}
