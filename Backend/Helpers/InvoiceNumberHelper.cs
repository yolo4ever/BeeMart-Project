using BeeMart.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Helpers;

public static class InvoiceNumberHelper
{
    public static async Task<string> GenerateNextInvoiceNumberAsync(BeeMartDbContext context, string userId, DateTime? targetDate = null)
    {
        DateTime date = targetDate ?? DateTime.Now;
        string datePart = date.ToString("ddMMyy"); // ddmmyy format

        // Pad userId to 5 digits if needed
        string paddedUserId = userId.PadLeft(5, '0');
        if (paddedUserId.Length > 5)
        {
            paddedUserId = paddedUserId.Substring(paddedUserId.Length - 5);
        }

        // Transactions on the same date (using TransactionDate day/month/year or InvoiceNo pattern)
        DateTime startOfDay = date.Date;
        DateTime endOfDay = startOfDay.AddDays(1);

        var existingInvoices = await context.TrTransactions
            .Where(t => t.TransactionDate >= startOfDay && t.TransactionDate < endOfDay)
            .Select(t => t.InvoiceNo)
            .ToListAsync();

        int maxRunningNumber = 0;
        foreach (var inv in existingInvoices)
        {
            // Format: [5 digit userID][ddmmyy][3 digit running number] = 14 chars
            if (inv != null && inv.Length >= 14)
            {
                string runNumStr = inv.Substring(inv.Length - 3);
                if (int.TryParse(runNumStr, out int runNum))
                {
                    if (runNum > maxRunningNumber)
                    {
                        maxRunningNumber = runNum;
                    }
                }
            }
        }

        int nextNumber = maxRunningNumber + 1;
        string runningNumberPart = nextNumber.ToString("D3");

        return $"{paddedUserId}{datePart}{runningNumberPart}";
    }
}
