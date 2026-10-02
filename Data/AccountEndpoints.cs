using FinanceTracker.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FinanceTracker.Data;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/account/actions");

        group.MapPost("/login", LoginAsync);
        group.MapPost("/register", RegisterAsync);
        group.MapGet("/confirm-email", ConfirmEmailAsync);
        group.MapPost("/resend-confirmation", ResendConfirmationAsync);
        group.MapPost("/forgot-password", ForgotPasswordAsync);
        group.MapPost("/reset-password", ResetPasswordAsync);
        group.MapPost("/logout", LogoutAsync).RequireAuthorization();
        return endpoints;
    }

    private static async Task<IResult> LoginAsync(HttpContext context, IAntiforgery antiforgery,
        SignInManager<ApplicationUser> signInManager)
    {
        await antiforgery.ValidateRequestAsync(context);
        var form = await context.Request.ReadFormAsync();
        var email = form["email"].ToString().Trim();
        var password = form["password"].ToString();
        var rememberMe = form["rememberMe"] == "true";
        var returnUrl = SafeReturnUrl(form["returnUrl"]);
        var result = await signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return Results.LocalRedirect(returnUrl);
        }

        var error = result.IsLockedOut
            ? "Вход временно заблокирован. Попробуйте позже."
            : result.IsNotAllowed
                ? "Подтвердите email перед входом."
                : "Неверный email или пароль.";
        return Results.LocalRedirect(LoginUrl(email, returnUrl, error));
    }

    private static async Task<IResult> RegisterAsync(HttpContext context, IAntiforgery antiforgery,
        UserManager<ApplicationUser> userManager, IDbContextFactory<FinanceDbContext> dbFactory,
        IAccountEmailSender emailSender)
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
        await SendConfirmationEmailAsync(context, userManager, emailSender, user);

        return Results.LocalRedirect($"/account/register-confirmation?email={Uri.EscapeDataString(email)}");
    }

    private static async Task<IResult> ConfirmEmailAsync(HttpContext context,
        UserManager<ApplicationUser> userManager)
    {
        var userId = context.Request.Query["userId"].ToString();
        var code = DecodeToken(context.Request.Query["code"]);
        if (string.IsNullOrWhiteSpace(userId) || code is null)
        {
            return Results.LocalRedirect("/account/email-confirmed?success=false");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Results.LocalRedirect("/account/email-confirmed?success=false");
        }

        if (await userManager.IsEmailConfirmedAsync(user))
        {
            return Results.LocalRedirect("/account/email-confirmed?success=true");
        }

        var result = await userManager.ConfirmEmailAsync(user, code);
        return Results.LocalRedirect($"/account/email-confirmed?success={result.Succeeded.ToString().ToLowerInvariant()}");
    }

    private static async Task<IResult> ResendConfirmationAsync(HttpContext context,
        IAntiforgery antiforgery, UserManager<ApplicationUser> userManager,
        IAccountEmailSender emailSender)
    {
        await antiforgery.ValidateRequestAsync(context);
        var form = await context.Request.ReadFormAsync();
        var email = form["email"].ToString().Trim();
        var user = await userManager.FindByEmailAsync(email);

        if (user is not null && !await userManager.IsEmailConfirmedAsync(user))
        {
            await SendConfirmationEmailAsync(context, userManager, emailSender, user);
        }

        return Results.LocalRedirect($"/account/register-confirmation?email={Uri.EscapeDataString(email)}");
    }

    private static async Task<IResult> ForgotPasswordAsync(HttpContext context,
        IAntiforgery antiforgery, UserManager<ApplicationUser> userManager,
        IAccountEmailSender emailSender)
    {
        await antiforgery.ValidateRequestAsync(context);
        var form = await context.Request.ReadFormAsync();
        var email = form["email"].ToString().Trim();
        var user = await userManager.FindByEmailAsync(email);

        if (user is not null && await userManager.IsEmailConfirmedAsync(user))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var link = AbsoluteUrl(context, "/account/reset-password", new Dictionary<string, string?>
            {
                ["email"] = email,
                ["code"] = EncodeToken(token)
            });
            await emailSender.SendPasswordResetLinkAsync(email, link, context.RequestAborted);
        }

        return Results.LocalRedirect("/account/forgot-password-confirmation");
    }

    private static async Task<IResult> ResetPasswordAsync(HttpContext context,
        IAntiforgery antiforgery, UserManager<ApplicationUser> userManager)
    {
        await antiforgery.ValidateRequestAsync(context);
        var form = await context.Request.ReadFormAsync();
        var email = form["email"].ToString().Trim();
        var code = form["code"].ToString();
        var password = form["password"].ToString();
        var confirmation = form["passwordConfirmation"].ToString();

        if (password != confirmation)
        {
            return Results.LocalRedirect(ResetPasswordUrl(email, code, "Пароли не совпадают."));
        }

        var token = DecodeToken(code);
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || token is null)
        {
            return Results.LocalRedirect(ResetPasswordUrl(email, code, "Ссылка недействительна или устарела."));
        }

        var result = await userManager.ResetPasswordAsync(user, token, password);
        if (!result.Succeeded)
        {
            var error = result.Errors.Any(x => x.Code.Contains("Token", StringComparison.OrdinalIgnoreCase))
                ? "Ссылка недействительна или устарела."
                : string.Join(" ", result.Errors.Select(x => TranslateIdentityError(x.Code)));
            return Results.LocalRedirect(ResetPasswordUrl(email, code, error));
        }

        return Results.LocalRedirect("/account/reset-password-confirmation");
    }

    private static async Task<IResult> LogoutAsync(HttpContext context, IAntiforgery antiforgery,
        SignInManager<ApplicationUser> signInManager)
    {
        await antiforgery.ValidateRequestAsync(context);
        await signInManager.SignOutAsync();
        return Results.LocalRedirect("/account/login");
    }

    private static async Task SendConfirmationEmailAsync(HttpContext context,
        UserManager<ApplicationUser> userManager, IAccountEmailSender emailSender,
        ApplicationUser user)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var link = AbsoluteUrl(context, "/account/actions/confirm-email", new Dictionary<string, string?>
        {
            ["userId"] = user.Id,
            ["code"] = EncodeToken(token)
        });
        await emailSender.SendConfirmationLinkAsync(user.Email!, link, context.RequestAborted);
    }

    private static string AbsoluteUrl(HttpContext context, string path, Dictionary<string, string?> query)
    {
        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}";
        return QueryHelpers.AddQueryString($"{baseUrl}{path}", query);
    }

    private static string EncodeToken(string token) =>
        WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

    private static string? DecodeToken(string? encodedToken)
    {
        if (string.IsNullOrWhiteSpace(encodedToken)) return null;
        try
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static string SafeReturnUrl(string? value) =>
        !string.IsNullOrWhiteSpace(value) && Uri.IsWellFormedUriString(value, UriKind.Relative)
            && value.StartsWith('/') && !value.StartsWith("//") ? value : "/";

    private static string LoginUrl(string email, string returnUrl, string error) =>
        $"/account/login?email={Uri.EscapeDataString(email)}&returnUrl={Uri.EscapeDataString(returnUrl)}&error={Uri.EscapeDataString(error)}";

    private static string RegisterUrl(string email, string error) =>
        $"/account/register?email={Uri.EscapeDataString(email)}&error={Uri.EscapeDataString(error)}";

    private static string ResetPasswordUrl(string email, string code, string error) =>
        $"/account/reset-password?email={Uri.EscapeDataString(email)}&code={Uri.EscapeDataString(code)}&error={Uri.EscapeDataString(error)}";

    private static string TranslateIdentityError(string code) => code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => "Аккаунт с таким email уже существует.",
        "InvalidEmail" or "InvalidUserName" => "Введите корректный email.",
        "PasswordTooShort" => "Пароль должен содержать не менее 8 символов.",
        "PasswordRequiresDigit" => "Добавьте в пароль хотя бы одну цифру.",
        "PasswordRequiresLower" => "Добавьте в пароль строчную букву.",
        "PasswordRequiresUpper" => "Добавьте в пароль заглавную букву.",
        _ => "Не удалось выполнить операцию. Проверьте введённые данные."
    };
}
