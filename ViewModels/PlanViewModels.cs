using System.ComponentModel.DataAnnotations;
using PersonalFinanceManager1.Models;

namespace PersonalFinanceManager1.ViewModels;

public class PlanCardViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PlanType Type { get; set; }
    public string ColorHex { get; set; } = "#10B981";
    public string IconGlyph { get; set; } = "◈";
    public string Sub { get; set; } = string.Empty;
    public string DoneLabel { get; set; } = string.Empty;
    public decimal Target { get; set; }
    public decimal Achieved { get; set; }
    public decimal Remaining { get; set; }
    public decimal Reserved { get; set; }
    public int PercentComplete { get; set; }
    public bool IsPaused { get; set; }
}

public class CreatePlanViewModel
{
    [Required(ErrorMessage = "اسم الخطة مطلوب")]
    [MinLength(3, ErrorMessage = "يرجى إدخال اسم واضح للخطة")]
    [MaxLength(200)]
    [Display(Name = "اسم الخطة")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "نوع الخطة")]
    public PlanType Type { get; set; } = PlanType.Save;

    [Required(ErrorMessage = "المبلغ المستهدف مطلوب")]
    [Range(0.01, double.MaxValue, ErrorMessage = "يرجى إدخال مبلغ مستهدف أكبر من صفر")]
    [Display(Name = "المبلغ المستهدف (ريال)")]
    public decimal Target { get; set; }

    [Range(0, 1000, ErrorMessage = "عدد الدفعات غير صالح")]
    [Display(Name = "عدد الدفعات (اختياري)")]
    public int Slots { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "يرجى إدخال مبلغ دفعة أكبر من صفر")]
    [Display(Name = "مبلغ الدفعة الشهرية (اختياري)")]
    public decimal? InstallmentAmount { get; set; }

    [Required(ErrorMessage = "تاريخ البداية مطلوب")]
    [Display(Name = "تاريخ البداية")]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "تاريخ الاستحقاق")]
    public DateOnly? EndDate { get; set; }

    [Required]
    [Display(Name = "لون الخطة")]
    public string ColorHex { get; set; } = "#10B981";
}

public class DonutLegendItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public string ColorCss { get; set; } = string.Empty;
    public string ValueF { get; set; } = string.Empty;
    public int Percent { get; set; }
}

public class MonthBarViewModel
{
    public string ColorHex { get; set; } = string.Empty;
    public string HeightPercent { get; set; } = "0%";
    public string Tooltip { get; set; } = string.Empty;
}

public class MonthPointViewModel
{
    public string Label { get; set; } = string.Empty;
    public List<MonthBarViewModel> Bars { get; set; } = new();
}

public class ChartSeriesViewModel
{
    public Guid PlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public string SoftColorCss { get; set; } = string.Empty;
    public string AreaFillPath { get; set; } = string.Empty;
    public string AreaLinePath { get; set; } = string.Empty;
}

public class MonthlyChartViewModel
{
    public List<MonthPointViewModel> Months { get; set; } = new();
    public List<ChartSeriesViewModel> Series { get; set; } = new();
    public bool HasData { get; set; }
}

public class OverviewViewModel
{
    public decimal TotalTarget { get; set; }
    public decimal TotalAchieved { get; set; }
    public decimal TotalRemaining { get; set; }
    public int OverallPercent { get; set; }
    public int TxCount { get; set; }
    public decimal TotalReserved { get; set; }
    public List<PlanCardViewModel> Plans { get; set; } = new();
    public MonthlyChartViewModel MonthlyChart { get; set; } = new();
    public string DonutGradientCss { get; set; } = string.Empty;
    public List<DonutLegendItemViewModel> DonutLegend { get; set; } = new();
    public CreatePlanViewModel NewPlan { get; set; } = new();
}

public class PlanLogItemViewModel
{
    public Guid Id { get; set; }
    public DateOnly PaymentDate { get; set; }
    public string DateLabel { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public PlanLogPurpose Purpose { get; set; }
    public string PurposeLabel { get; set; } = string.Empty;
}

public class AddPaymentViewModel
{
    [Required(ErrorMessage = "يرجى إدخال مبلغ صحيح أكبر من صفر")]
    [Range(0.01, double.MaxValue, ErrorMessage = "يرجى إدخال مبلغ صحيح أكبر من صفر")]
    [Display(Name = "المبلغ (ريال)")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "يرجى تحديد التاريخ")]
    [Display(Name = "التاريخ")]
    public DateOnly PaymentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [MaxLength(500)]
    [Display(Name = "ملاحظة")]
    public string? Note { get; set; }

    [Display(Name = "نوع الدفعة")]
    public PlanLogPurpose Purpose { get; set; } = PlanLogPurpose.BillPayment;
}

public class PlanDetailsViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PlanType Type { get; set; }
    public string ColorHex { get; set; } = string.Empty;
    public decimal Target { get; set; }
    public decimal Achieved { get; set; }
    public decimal Remaining { get; set; }
    public decimal Reserved { get; set; }
    public int PercentComplete { get; set; }
    public bool IsPaused { get; set; }
    public decimal AveragePayment { get; set; }
    public int PaymentsCount { get; set; }
    public int Slots { get; set; }
    public int FilledSlots { get; set; }

    public string TargetNote { get; set; } = string.Empty;
    public string DoneLabel { get; set; } = string.Empty;
    public string CountLabel { get; set; } = string.Empty;
    public string CountValue { get; set; } = string.Empty;
    public string CountWidthPercent { get; set; } = "0%";

    public string GaugeGradientCss { get; set; } = string.Empty;
    public string GaugeSub { get; set; } = string.Empty;
    public string GaugeCaption { get; set; } = string.Empty;

    public string FormTitle { get; set; } = string.Empty;
    public string SaveButtonLabel { get; set; } = string.Empty;
    public string NotePlaceholder { get; set; } = string.Empty;
    public string TableTitle { get; set; } = string.Empty;

    public List<PlanLogItemViewModel> Payments { get; set; } = new();
    public AddPaymentViewModel NewPayment { get; set; } = new();
    public CreatePlanViewModel EditPlan { get; set; } = new();
}
