using Microsoft.AspNetCore.Identity;

namespace PersonalFinanceManager1.Services;

// يترجم رسائل أخطاء ASP.NET Core Identity الافتراضية (الإنجليزية) إلى العربية
// حتى تبقى الواجهة كاملة بالعربي حتى عند فشل عمليات الحساب.
public class ArabicIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => new() { Code = nameof(DefaultError), Description = "حدث خطأ غير متوقع." };

    public override IdentityError DuplicateEmail(string email) => new() { Code = nameof(DuplicateEmail), Description = "هذا البريد الإلكتروني مستخدم بالفعل." };

    public override IdentityError DuplicateUserName(string userName) => new() { Code = nameof(DuplicateUserName), Description = "هذا البريد الإلكتروني مستخدم بالفعل." };

    public override IdentityError InvalidEmail(string? email) => new() { Code = nameof(InvalidEmail), Description = "صيغة البريد الإلكتروني غير صحيحة." };

    public override IdentityError InvalidUserName(string? userName) => new() { Code = nameof(InvalidUserName), Description = "صيغة البريد الإلكتروني غير صحيحة." };

    public override IdentityError PasswordTooShort(int length) => new() { Code = nameof(PasswordTooShort), Description = $"كلمة المرور يجب أن تكون {length} أحرف على الأقل." };

    public override IdentityError PasswordRequiresDigit() => new() { Code = nameof(PasswordRequiresDigit), Description = "كلمة المرور يجب أن تحتوي على رقم واحد على الأقل." };

    public override IdentityError PasswordRequiresLower() => new() { Code = nameof(PasswordRequiresLower), Description = "كلمة المرور يجب أن تحتوي على حرف صغير على الأقل." };

    public override IdentityError PasswordRequiresUpper() => new() { Code = nameof(PasswordRequiresUpper), Description = "كلمة المرور يجب أن تحتوي على حرف كبير على الأقل." };

    public override IdentityError PasswordRequiresNonAlphanumeric() => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "كلمة المرور يجب أن تحتوي على رمز خاص واحد على الأقل." };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"كلمة المرور يجب أن تحتوي على {uniqueChars} أحرف مختلفة على الأقل." };

    public override IdentityError PasswordMismatch() => new() { Code = nameof(PasswordMismatch), Description = "كلمة المرور غير صحيحة." };

    public override IdentityError UserAlreadyHasPassword() => new() { Code = nameof(UserAlreadyHasPassword), Description = "المستخدم لديه كلمة مرور بالفعل." };

    public override IdentityError UserLockoutNotEnabled() => new() { Code = nameof(UserLockoutNotEnabled), Description = "قفل الحساب غير مفعّل لهذا المستخدم." };

    public override IdentityError UserNotInRole(string role) => new() { Code = nameof(UserNotInRole), Description = "المستخدم ليس ضمن هذا الدور." };

    public override IdentityError UserAlreadyInRole(string role) => new() { Code = nameof(UserAlreadyInRole), Description = "المستخدم موجود بالفعل ضمن هذا الدور." };

    public override IdentityError ConcurrencyFailure() => new() { Code = nameof(ConcurrencyFailure), Description = "تعارض في البيانات، حاول مرة أخرى." };
}
