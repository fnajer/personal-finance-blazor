using System.Globalization;
using System.Text;
using FinanceTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Data;

public static class TransactionExportEndpoints
{
    public static IEndpointRouteBuilder MapTransactionExportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/transactions/export", ExportAsync)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> ExportAsync(
        HttpContext context,
        IDbContextFactory<FinanceDbContext> dbFactory,
        string? search,
        string? type,
        DateOnly? from,
        DateOnly? to)
    {
        var userId = context.User.GetRequiredUserId();
        await using var db = await dbFactory.CreateDbContextAsync();

        var query = db.Transactions
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.UserId == userId);

        if (from.HasValue)
        {
            query = query.Where(x => x.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.Date <= to.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Description.Contains(term));
        }

        if (Enum.TryParse<TransactionType>(type, out var transactionType))
        {
            query = query.Where(x => x.Category!.Type == transactionType);
        }

        var transactions = await query
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .ToListAsync();

        var csv = BuildCsv(transactions);
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var content = encoding.GetPreamble().Concat(encoding.GetBytes(csv)).ToArray();
        var fileName = $"transactions-{DateTime.Today:yyyy-MM-dd}.csv";

        context.Response.Headers.CacheControl = "no-store";
        return Results.File(content, "text/csv; charset=utf-8", fileName);
    }

    internal static string BuildCsv(IEnumerable<FinanceTransaction> transactions)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Дата;Тип;Описание;Категория;Сумма;Заметка");

        foreach (var transaction in transactions)
        {
            var type = transaction.Category?.Type == TransactionType.Income ? "Доход" : "Расход";
            csv.Append(CsvField(transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))
                .Append(';').Append(CsvField(type))
                .Append(';').Append(CsvField(transaction.Description, protectFromFormula: true))
                .Append(';').Append(CsvField(transaction.Category?.Name ?? string.Empty, protectFromFormula: true))
                .Append(';').Append(CsvField(transaction.Amount.ToString("0.00", CultureInfo.InvariantCulture)))
                .Append(';').Append(CsvField(transaction.Note ?? string.Empty, protectFromFormula: true))
                .AppendLine();
        }

        return csv.ToString();
    }

    private static string CsvField(string value, bool protectFromFormula = false)
    {
        if (protectFromFormula && value.Length > 0 && value[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
        {
            value = "'" + value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
