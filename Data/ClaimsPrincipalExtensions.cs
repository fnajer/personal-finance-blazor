using System.Security.Claims;

namespace FinanceTracker.Data;

public static class ClaimsPrincipalExtensions
{
    public static string GetRequiredUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Не удалось определить текущего пользователя.");
}
