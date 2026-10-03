using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager1.Data;
using PersonalFinanceManager1.Models;
using PersonalFinanceManager1.Services;

var builder = WebApplication.CreateBuilder(args);

// إعدادات محلية لا تُرفع على GitHub (نص الاتصال بقاعدة البيانات وكلمة المرور).
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // مطابقة تماماً لما يتحقق منه RegisterViewModel (6 أحرف على الأقل، بدون شروط أحرف معقدة)
        // حتى لا يقبل النموذج كلمة مرور بالعميل ثم يرفضها الخادم بسبب سياسة أخرى.
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IdentityErrorDescriber, ArabicIdentityErrorDescriber>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddScoped<IPlanCalculationService, PlanCalculationService>();
builder.Services.AddScoped<IDemoDataSeeder, DemoDataSeeder>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

var app = builder.Build();

// ── قاعدة البيانات + الحساب التجريبي ─────────────────────────────────────
// `dotnet run -- seed` يعيد ضبط بيانات الحساب التجريبي ثم يخرج.
// التشغيل العادي: يطبّق الـ Migrations وينشئ الحساب التجريبي مرة واحدة فقط إن لم يكن موجوداً.
var seedOnly = args.Contains("seed");
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        services.GetRequiredService<ApplicationDbContext>().Database.Migrate();
        await DemoAccountSeeder.SeedAsync(services, reset: seedOnly);
        if (seedOnly)
        {
            Console.WriteLine($"[seed] Demo account ready: {DemoAccountSeeder.DemoEmail} / {DemoAccountSeeder.DemoPassword}");
            return;
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "تعذّر تجهيز قاعدة البيانات أو الحساب التجريبي");
        if (seedOnly) throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
