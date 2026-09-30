using FinanceTracker.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Data;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/account/actions");

        group.MapPost("/login", LoginAsync);
        group.MapPost("/register", RegisterAsync);
        group.MapPost("/logout", LogoutAsync).RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        HttpContext context,
        IAntiforgery antiforgery,
        SignInManager<ApplicationUser> signInManager)
    {
        await antiforgery.ValidateRequestAsync(context);
        var form = await context.Request.ReadFormAsync();
        var email = form["email"].ToString().Trim();
        var password = form["password"].ToString();
        var rememberMe = form["rememberMe"] == "true";
        var returnUrl = SafeReturnUrl(context, form["returnUrl"]);

        var result = await signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            return Results.LocalRedirect(returnUrl);
        }

        var error = result.IsLockedOut
            ? "Вход временно заблокирован. Попробуйте позже."
            : "Неверный email или пароль.";
        return Results.LocalRedirect(LoginUrl(email, returnUrl, error));
    }

    private static async Task<IResult> RegisterAsync(
        HttpContext context,
        IAntiforgery antiforgery,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IDbContextFactory<FinanceDbContext> dbFactory)
    {
        await antiforgery.ValidateRequestAsync(context);
        var form = await context.Request.ReadFormAsync();
        var email = form["email"].ToString().Trim();
        var password = form["password"].ToString();
        var confirmation = form["passwordConfirmation"].ToString();

        if (password != confirmation)
        {
            return Results.LocalRedirect(RegisterUrl(email, "Пароли не совпадают."));
        }

        var user = new ApplicationUser { UserName = email, Email = email };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var error = string.Join(" ", result.Errors.Select(x => TranslateIdentityError(x.Code)));
            return Results.LocalRedirect(RegisterUrl(email, error));
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        db.Categories.AddRange(UserDataSeeder.CreateDefaultCategories(user.Id));
        await db.SaveChangesAsync();

        await signInManager.SignInAsync(user, isPersistent: false);
        return Results.LocalRedirect("/");
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext context,
        IAntiforgery antiforgery,
        SignInManager<ApplicationUser> signInManager)
    {
        await antiforgery.ValidateRequestAsync(context);
        await signInManager.SignOutAsync();
        return Results.LocalRedirect("/account/login");
    }

    private static string SafeReturnUrl(HttpContext context, string? value) =>
        !string.IsNullOrWhiteSpace(value) && Uri.IsWellFormedUriString(value, UriKind.Relative)
            && value.StartsWith('/') && !value.StartsWith("//")
                ? value
                : "/";

    private static string LoginUrl(string email, string returnUrl, string error) =>
        $"/account/login?email={Uri.EscapeDataString(email)}&returnUrl={Uri.EscapeDataString(returnUrl)}&error={Uri.EscapeDataString(error)}";

    private static string RegisterUrl(string email, string error) =>
        $"/account/register?email={Uri.EscapeDataString(email)}&error={Uri.EscapeDataString(error)}";

    private static string TranslateIdentityError(string code) => code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => "Аккаунт с таким email уже существует.",
        "InvalidEmail" or "InvalidUserName" => "Введите корректный email.",
        "PasswordTooShort" => "Пароль должен содержать не менее 8 символов.",
        "PasswordRequiresDigit" => "Добавьте в пароль хотя бы одну цифру.",
        "PasswordRequiresLower" => "Добавьте в пароль строчную букву.",
        "PasswordRequiresUpper" => "Добавьте в пароль заглавную букву.",
        _ => "Не удалось создать аккаунт. Проверьте введённые данные."
    };
}
