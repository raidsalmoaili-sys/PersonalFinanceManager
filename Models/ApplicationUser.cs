using Microsoft.AspNetCore.Identity;

namespace PersonalFinanceManager1.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public ICollection<FinancialPlan> FinancialPlans { get; set; } = new List<FinancialPlan>();
}
