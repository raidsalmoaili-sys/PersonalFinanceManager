using PersonalFinanceManager1.Models;
using PersonalFinanceManager1.ViewModels;

namespace PersonalFinanceManager1.Services;

public interface IPlanCalculationService
{
    decimal GetAchieved(FinancialPlan plan);
    decimal GetReserved(FinancialPlan plan);
    int GetPercentComplete(decimal target, decimal achieved);
    decimal GetRemaining(decimal target, decimal achieved);
    decimal GetAveragePayment(FinancialPlan plan);
    PlanCardViewModel ToCardViewModel(FinancialPlan plan);
    PlanDetailsViewModel ToDetailsViewModel(FinancialPlan plan);
    MonthlyChartViewModel BuildMonthlyChart(List<FinancialPlan> plans);
    (string GradientCss, List<DonutLegendItemViewModel> Legend) BuildDonut(List<FinancialPlan> plans);
}
