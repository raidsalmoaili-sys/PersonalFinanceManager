namespace PersonalFinanceManager1.Services;

public interface IDemoDataSeeder
{
    Task SeedForNewUserAsync(string userId);
}
