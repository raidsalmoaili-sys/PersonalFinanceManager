using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager1.Data;
using PersonalFinanceManager1.Helpers;
using PersonalFinanceManager1.Models;
using PersonalFinanceManager1.Services;
using PersonalFinanceManager1.ViewModels;

namespace PersonalFinanceManager1.Controllers;

[Authorize]
public class PlansController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPlanCalculationService _calc;

    public PlansController(ApplicationDbContext context, IPlanCalculationService calc)
    {
        _context = context;
        _calc = calc;
    }

    private string CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

    public async Task<IActionResult> Index()
    {
        var vm = await BuildOverviewViewModelAsync();
        return View(vm);
    }

    private async Task<OverviewViewModel> BuildOverviewViewModelAsync()
    {
        var plans = await _context.FinancialPlans
            .Include(p => p.PlanLogs)
            .Where(p => p.UserId == CurrentUserId)
            .OrderBy(p => p.StartDate)
            .ToListAsync();

        var activePlans = plans.Where(p => !p.IsPaused).ToList();

        var totalTarget = activePlans.Sum(p => p.Target);
        var totalAchieved = activePlans.Sum(p => _calc.GetAchieved(p));
        var totalReserved = activePlans.Sum(p => _calc.GetReserved(p));
        var (donutCss, donutLegend) = _calc.BuildDonut(activePlans);

        return new OverviewViewModel
        {
            TotalTarget = totalTarget,
            TotalAchieved = totalAchieved,
            TotalRemaining = _calc.GetRemaining(totalTarget, totalAchieved),
            TotalReserved = totalReserved,
            OverallPercent = _calc.GetPercentComplete(totalTarget, totalAchieved),
            TxCount = activePlans.Sum(p => p.PlanLogs.Count),
            Plans = plans.Select(_calc.ToCardViewModel).ToList(),
            MonthlyChart = _calc.BuildMonthlyChart(activePlans),
            DonutGradientCss = donutCss,
            DonutLegend = donutLegend
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePlanViewModel form)
    {
        if (!ModelState.IsValid)
        {
            var vm = await BuildOverviewViewModelAsync();
            vm.NewPlan = form;
            ViewData["OpenCreateModal"] = true;
            return View(nameof(Index), vm);
        }

        var plan = new FinancialPlan
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId,
            Name = form.Name,
            Type = form.Type,
            Target = form.Target,
            Slots = form.Slots,
            InstallmentAmount = form.InstallmentAmount,
            ColorHex = form.ColorHex,
            StartDate = form.StartDate,
            EndDate = form.EndDate
        };

        _context.FinancialPlans.Add(plan);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = plan.Id });
    }

    private async Task<List<PlanCardViewModel>> GetSidebarPlansAsync()
    {
        return await _context.FinancialPlans
            .Where(p => p.UserId == CurrentUserId)
            .OrderBy(p => p.StartDate)
            .Select(p => new PlanCardViewModel { Id = p.Id, Name = p.Name, ColorHex = p.ColorHex, IsPaused = p.IsPaused })
            .ToListAsync();
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var plan = await _context.FinancialPlans
            .Include(p => p.PlanLogs)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);

        if (plan is null) return NotFound();

        var vm = _calc.ToDetailsViewModel(plan);
        ViewData["SidebarPlans"] = await GetSidebarPlansAsync();
        ViewData["CurrentPlanId"] = id;
        ViewData["CurrentPlanPaused"] = plan.IsPaused;
        ViewData["Crumb"] = $"الإدارة المالية · {plan.Type.ToArabicLabel()}";
        ViewData["Title"] = plan.Name;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePause(Guid id)
    {
        var plan = await _context.FinancialPlans
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);

        if (plan is null) return NotFound();

        plan.IsPaused = !plan.IsPaused;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPlan(Guid id, [Bind(Prefix = "EditPlan")] CreatePlanViewModel form)
    {
        var plan = await _context.FinancialPlans
            .Include(p => p.PlanLogs)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);

        if (plan is null) return NotFound();

        if (!ModelState.IsValid)
        {
            var vm = _calc.ToDetailsViewModel(plan);
            vm.EditPlan = form;
            ViewData["SidebarPlans"] = await GetSidebarPlansAsync();
            ViewData["CurrentPlanId"] = id;
            ViewData["CurrentPlanPaused"] = plan.IsPaused;
            ViewData["Crumb"] = $"الإدارة المالية · {plan.Type.ToArabicLabel()}";
            ViewData["Title"] = plan.Name;
            ViewData["OpenEditPlanModal"] = true;
            return View("Details", vm);
        }

        plan.Name = form.Name;
        plan.Type = form.Type;
        plan.Target = form.Target;
        plan.Slots = form.Slots;
        plan.InstallmentAmount = form.InstallmentAmount;
        plan.ColorHex = form.ColorHex;
        plan.StartDate = form.StartDate;
        plan.EndDate = form.EndDate;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPayment(Guid id, [Bind(Prefix = "NewPayment")] AddPaymentViewModel form)
    {
        var plan = await _context.FinancialPlans
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);

        if (plan is null) return NotFound();

        if (!ModelState.IsValid)
        {
            var reloaded = await _context.FinancialPlans
                .Include(p => p.PlanLogs)
                .FirstAsync(p => p.Id == id);
            var vm = _calc.ToDetailsViewModel(reloaded);
            vm.NewPayment = form;
            ViewData["SidebarPlans"] = await GetSidebarPlansAsync();
            ViewData["CurrentPlanId"] = id;
            ViewData["CurrentPlanPaused"] = reloaded.IsPaused;
            ViewData["Crumb"] = $"الإدارة المالية · {reloaded.Type.ToArabicLabel()}";
            ViewData["Title"] = reloaded.Name;
            return View("Details", vm);
        }

        _context.PlanLogs.Add(new PlanLog
        {
            Id = Guid.NewGuid(),
            PlanId = plan.Id,
            Amount = form.Amount,
            PaymentDate = form.PaymentDate,
            Note = form.Note,
            Purpose = form.Purpose
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPayment(Guid id, Guid logId, AddPaymentViewModel form)
    {
        var log = await _context.PlanLogs
            .Include(l => l.Plan)
            .FirstOrDefaultAsync(l => l.Id == logId && l.PlanId == id && l.Plan!.UserId == CurrentUserId);

        if (log is null) return NotFound();

        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Details), new { id });

        log.Amount = form.Amount;
        log.PaymentDate = form.PaymentDate;
        log.Note = form.Note;
        log.Purpose = form.Purpose;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TransferPayment(Guid id, Guid logId, PlanLogPurpose target)
    {
        if (target == PlanLogPurpose.Reserved) return BadRequest();

        var log = await _context.PlanLogs
            .Include(l => l.Plan)
            .FirstOrDefaultAsync(l => l.Id == logId && l.PlanId == id && l.Plan!.UserId == CurrentUserId);

        if (log is null) return NotFound();
        if (log.Purpose != PlanLogPurpose.Reserved) return BadRequest();

        log.Purpose = target;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePayment(Guid id, Guid logId)
    {
        var log = await _context.PlanLogs
            .Include(l => l.Plan)
            .FirstOrDefaultAsync(l => l.Id == logId && l.PlanId == id && l.Plan!.UserId == CurrentUserId);

        if (log is null) return NotFound();

        _context.PlanLogs.Remove(log);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var plan = await _context.FinancialPlans
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);

        if (plan is null) return NotFound();

        _context.FinancialPlans.Remove(plan);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
